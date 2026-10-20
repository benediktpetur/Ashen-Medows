"""Prepare game copies of Hunyuan actors; never edits workshop originals.

Requires numpy. Removes distant disconnected reconstruction debris while retaining
large separate parts and small parts close to the main body. UVs/materials remain.
"""
import argparse
import collections
import hashlib
import itertools
import json
from pathlib import Path
import struct
import numpy as np

REPO = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source', type=Path, default=REPO.parents[1] / 'Ashen Meadows - Asset Copy/Hunyuan Workshop/Models from text')
args = parser.parse_args()
output = REPO / 'game/Assets/Hunyuan/Actors'
output.mkdir(parents=True, exist_ok=True)
manifest = []

def prepare(source, target):
    raw = source.read_bytes()
    length = struct.unpack_from('<I', raw, 12)[0]
    doc = json.loads(raw[20:20 + length])
    binary = bytearray(raw[28 + length:])
    def read(index):
        a = doc['accessors'][index]
        view = doc['bufferViews'][a['bufferView']]
        dtype = {5126: '<f4', 5125: '<u4', 5123: '<u2', 5121: 'u1', 5122: '<i2', 5120: 'i1'}[a['componentType']]
        width = {'SCALAR': 1, 'VEC2': 2, 'VEC3': 3, 'VEC4': 4}[a['type']]
        item = np.dtype(dtype).itemsize
        return np.ndarray((a['count'], width), dtype, buffer=binary,
                          offset=view.get('byteOffset', 0) + a.get('byteOffset', 0),
                          strides=(view.get('byteStride', width * item), item)).copy()
    def append(array, old):
        while len(binary) % 4:
            binary.append(0)
        view = len(doc['bufferViews'])
        data = array.tobytes()
        doc['bufferViews'].append({'buffer': 0, 'byteOffset': len(binary), 'byteLength': len(data)})
        binary.extend(data)
        accessor = dict(doc['accessors'][old])
        accessor.update(bufferView=view, byteOffset=0, count=len(array))
        accessor.pop('min', None)
        accessor.pop('max', None)
        if accessor['type'] == 'VEC3':
            accessor['min'] = array.min(axis=0).tolist()
            accessor['max'] = array.max(axis=0).tolist()
        doc['accessors'].append(accessor)
        return len(doc['accessors']) - 1
    before = after = 0
    for mesh in doc['meshes']:
        for primitive in mesh['primitives']:
            vertices = read(primitive['attributes']['POSITION'])
            faces = read(primitive['indices']).reshape(-1, 3)
            original_count = len(faces)
            # Long needle-like reconstruction faces turn into ribbons when skinned.
            triangles = vertices[faces]
            e0 = triangles[:, 1] - triangles[:, 0]
            e1 = triangles[:, 2] - triangles[:, 0]
            e2 = triangles[:, 2] - triangles[:, 1]
            longest = np.maximum.reduce((np.linalg.norm(e0, axis=1), np.linalg.norm(e1, axis=1), np.linalg.norm(e2, axis=1)))
            area2 = np.linalg.norm(np.cross(e0, e1), axis=1)
            extent = float(np.ptp(vertices, axis=0).max())
            needles = (longest > extent * .06) & (area2 < longest * longest * .025)
            faces = faces[~needles & (area2 > extent * extent * 1e-10)]
            _, weld = np.unique(np.round(vertices, 5), axis=0, return_inverse=True)
            parent = list(range(int(weld.max()) + 1))
            def find(i):
                while parent[i] != i:
                    parent[i] = parent[parent[i]]
                    i = parent[i]
                return i
            for face in weld[faces]:
                a = find(int(face[0]))
                for i in face[1:]:
                    parent[find(int(i))] = a
            groups = collections.defaultdict(list)
            for i, face in enumerate(faces):
                groups[find(int(weld[face[0]]))].append(i)
            main = max(groups.values(), key=len)
            points = vertices[np.unique(faces[main])]
            cell = max(float(np.ptp(points, axis=0).max()) * .012, .00001)
            occupied = {tuple(p) for p in np.floor(points / cell).astype(int)}
            neighbours = list(itertools.product((-1, 0, 1), repeat=3))
            keep = list(main)
            for group in groups.values():
                if group is main:
                    continue
                significant = len(group) >= len(main) * .15
                cells = np.unique(np.floor(vertices[np.unique(faces[group])] / cell).astype(int), axis=0)
                touching = len(group) >= 32 and any((int(p[0])+x, int(p[1])+y, int(p[2])+z) in occupied for p in cells for x, y, z in neighbours)
                if (significant or touching) and source.parent.parent.name != 'zombie-4-city-security':
                    keep.extend(group)
            selected = faces[sorted(keep)]
            used, inverse = np.unique(selected, return_inverse=True)
            # Copy all attributes before appending, which resizes the GLB buffer.
            attributes = {name: (index, read(index)[used]) for name, index in primitive['attributes'].items()}
            for name, (index, array) in attributes.items():
                primitive['attributes'][name] = append(array, index)
            indices = inverse.astype(read(primitive['indices']).dtype).reshape(-1, 1)
            primitive['indices'] = append(indices, primitive['indices'])
            before += original_count
            after += len(selected)
    while len(binary) % 4:
        binary.append(0)
    doc['buffers'][0]['byteLength'] = len(binary)
    text = json.dumps(doc, separators=(',', ':')).encode()
    text += b' ' * (-len(text) % 4)
    result = struct.pack('<III', 0x46546c67, 2, 12+8+len(text)+8+len(binary))
    result += struct.pack('<II', len(text), 0x4e4f534a) + text
    result += struct.pack('<II', len(binary), 0x004e4942) + binary
    target.write_bytes(result)
    return before, after

for category in ('Characters', 'Enemies'):
    for folder in sorted((args.source / category).glob('*')):
        if folder.name in ('zombie-4-city-security', 'special-worm-worm'):
            print('Skipping rejected output:', folder.name)
            continue
        source = folder / 'textured/model-1.glb'
        if not source.is_file():
            continue
        target = output / (folder.name + '.glb')
        before, after = prepare(source, target)
        manifest.append(dict(key=folder.name, category=category, source=str(source.resolve()),
                             sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),
                             gameSha256=hashlib.sha256(target.read_bytes()).hexdigest(),
                             sourceTriangles=before, gameTriangles=after,
                             resource='res://Assets/Hunyuan/Actors/' + target.name))
        print(f'{folder.name}: {before} -> {after} triangles')
(output / 'source-manifest.json').write_text(json.dumps(manifest, indent=2))
print(f'Prepared {len(manifest)} actors; workshop originals unchanged.')
