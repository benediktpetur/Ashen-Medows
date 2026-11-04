"""Stage completed November 3 skyscrapers without modifying source jobs."""
from pathlib import Path
import hashlib, json, shutil, struct

repo = Path(__file__).resolve().parents[1]
batch = repo.parents[1] / 'Ashen Meadows - 9B Images to 3D'
out = repo / 'game/Assets/GeneratedSkyscrapers'
out.mkdir(parents=True, exist_ok=True)
manifest = []
review = json.loads((repo / 'scripts/skyscraper-selections.json').read_text(encoding='utf-8'))
deferred = []
for job in json.loads((batch / 'queue.json').read_text(encoding='utf-8')):
    if 'sky scraper' not in job['relative'].lower():
        continue
    folder = batch / job['output']
    source = folder / 'textured.glb'
    if not source.exists() or not (folder / 'completed.json').exists():
        continue
    data = source.read_bytes()
    length = struct.unpack_from('<I', data, 12)[0]
    document = json.loads(data[20:20 + length])
    if not document.get('meshes') or not document.get('images'):
        raise ValueError(f'Missing textured geometry: {source}')
    if any('uri' in item and not item['uri'].startswith('data:')
           for item in document.get('buffers', []) + document.get('images', [])):
        raise ValueError(f'External GLB dependency: {source}')
    key = 'tower-' + folder.name.rsplit('-', 1)[1]
    if key not in review['approved']:
        deferred.append(dict(key=key, source=str(source), reason=review['deferred'].get(key, 'Awaiting rendered review')))
        # Remove only our earlier staging copies, never generation outputs.
        for staged in (out / (key + '.glb'), out / (key + '.glb.import')):
            if staged.parent.resolve() != out.resolve():
                raise RuntimeError('Staging path escaped output directory')
            staged.unlink(missing_ok=True)
        continue
    shutil.copy2(source, out / (key + '.glb'))
    manifest.append(dict(key=key, resource='res://Assets/GeneratedSkyscrapers/' + key + '.glb',
                         source=str(source), reference=job['source'], sha256=hashlib.sha256(data).hexdigest()))
if not manifest:
    raise RuntimeError('No completed skyscrapers found')
(out / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
(out / 'deferred.json').write_text(json.dumps(deferred, indent=2) + '\n', encoding='utf-8')
print(f'Staged {len(manifest)} reviewed skyscrapers; {len(deferred)} defective reconstructions deferred with original files preserved.')
