"""Prepare reviewed existing GLBs without changing the generation originals.
Remove degenerate triangles and tiny disconnected scraps; preserve UVs and textures.
The bow's fused arrow/string are removed so the held rig can animate replacements.
"""
from pathlib import Path
import json, struct, math, hashlib
REPO=Path(__file__).resolve().parents[1]
selection=json.loads((REPO/'scripts/held-weapon-selections.json').read_text())
TYPES={5121:('B',1),5123:('H',2),5125:('I',4),5126:('f',4)}
COUNTS={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4}
def clean(source,target,kind):
    raw=source.read_bytes(); n=struct.unpack_from('<I',raw,12)[0]; doc=json.loads(raw[20:20+n]); off=20+n
    length=struct.unpack_from('<I',raw,off)[0]; binary=bytearray(raw[off+8:off+8+length]); removed=0; before=0
    def read(ai):
        a=doc['accessors'][ai];v=doc['bufferViews'][a['bufferView']];fmt,size=TYPES[a['componentType']];count=COUNTS[a['type']];stride=v.get('byteStride',size*count);start=v.get('byteOffset',0)+a.get('byteOffset',0)
        return [struct.unpack_from('<'+fmt*count,binary,start+i*stride) for i in range(a['count'])]
    def bow_curve(y):
        knots=[(0,.31),(.2,.20),(.4,.05),(.6,-.15),(.7,-.30),(.8,-.46),(.9,-.58),(1.05,-.56)]
        y=abs(y)
        for (a,x),(b,z) in zip(knots,knots[1:]):
            if y<=b:return x+(z-x)*(y-a)/(b-a)
        return -.56
    def add(values,component,type):
        while len(binary)%4:binary.append(0)
        start=len(binary);fmt,_=TYPES[component]
        for row in values:binary.extend(struct.pack('<'+fmt*len(row),*row))
        doc['bufferViews'].append({'buffer':0,'byteOffset':start,'byteLength':len(binary)-start})
        a={'bufferView':len(doc['bufferViews'])-1,'componentType':component,'count':len(values),'type':type}
        if type=='VEC3':a.update(min=[min(x[k] for x in values) for k in range(3)],max=[max(x[k] for x in values) for k in range(3)])
        doc['accessors'].append(a);return len(doc['accessors'])-1
    for mesh in doc.get('meshes',[]):
      for primitive in mesh['primitives']:
        pos=read(primitive['attributes']['POSITION']);idx=[x[0] for x in read(primitive['indices'])] if 'indices' in primitive else list(range(len(pos)));before+=len(idx)//3
        parent=list(range(len(pos)))
        def find(a):
            while parent[a]!=a:parent[a]=parent[parent[a]];a=parent[a]
            return a
        def union(a,b):parent[find(a)]=find(b)
        weld={}
        for i,p in enumerate(pos):
            key=tuple(round(x,5) for x in p)
            if key in weld:union(i,weld[key])
            else:weld[key]=i
        tris=[]
        for j in range(0,len(idx),3):
            t=idx[j:j+3];a,b,c=[pos[i] for i in t];u=[b[k]-a[k] for k in range(3)];v=[c[k]-a[k] for k in range(3)];area=sum(x*x for x in [u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0]])**.5/2
            if not math.isfinite(area) or area<1e-11:removed+=1;continue
            center=[sum(pos[i][k] for i in t)/3 for k in range(3)]
            if kind=='shaft' and center[0]>.40:
                removed+=1;continue
            if kind=='bow' and (abs(center[1])<.035 or abs(center[0]-bow_curve(center[1]))>.16):
                removed+=1;continue
            union(t[0],t[1]);union(t[1],t[2]);tris.append((t,area))
        stats={};total=sum(a for _,a in tris)
        for t,a in tris:
            root=find(t[0]);cnt,ar=stats.get(root,(0,0));stats[root]=(cnt+1,ar+a)
        kept=[]
        for t,a in tris:
            cnt,ar=stats[find(t[0])]
            if cnt<16 and ar<total*.0005:removed+=1
            else:kept.extend(t)
        if not kept:raise RuntimeError('Cleanup removed the complete mesh: '+str(source))
        used=sorted(set(kept));remap={old:new for new,old in enumerate(used)}
        for key,ai in list(primitive['attributes'].items()):
            a=doc['accessors'][ai];values=read(ai)
            if kind=='sword' and key=='POSITION':values=[((.25+(v[0]-.25)*.45) if v[0]>.25 else v[0],v[1],v[2]) for v in values]
            if kind=='shaft' and key=='POSITION':values=[(v[0],v[1]*.45,v[2]*.45) for v in values]
            if kind=='shaft' and key=='NORMAL':
                values=[tuple(x/(sum(y*y for y in (v[0],v[1]/.45,v[2]/.45))**.5 or 1) for x in (v[0],v[1]/.45,v[2]/.45)) for v in values]
            if kind=='sword' and key=='NORMAL':
                adjusted=[]
                for i,v in enumerate(values):
                    if pos[i][0]>.25:
                        v=(v[0]/.45,v[1],v[2]);length=sum(x*x for x in v)**.5
                        if length>1e-8:v=tuple(x/length for x in v)
                    adjusted.append(v)
                values=adjusted
            primitive['attributes'][key]=add([values[i] for i in used],a['componentType'],a['type'])
        primitive['indices']=add([(remap[i],) for i in kept],5125,'SCALAR')
    for material in doc.get('materials',[]):
        pbr=material.setdefault('pbrMetallicRoughness',{});pbr['metallicFactor']=.05;pbr['roughnessFactor']=.8
        if 'normalTexture' in material:material['normalTexture']['scale']=.35
    # Discard unreferenced original geometry blocks after reindexing.
    used_accessors=sorted({a for mesh in doc['meshes'] for p in mesh['primitives'] for a in list(p['attributes'].values())+[p['indices']]})
    accessor_map={old:new for new,old in enumerate(used_accessors)}
    for mesh in doc['meshes']:
        for primitive in mesh['primitives']:
            primitive['attributes']={k:accessor_map[v] for k,v in primitive['attributes'].items()};primitive['indices']=accessor_map[primitive['indices']]
    doc['accessors']=[doc['accessors'][i] for i in used_accessors]
    used_views=sorted({a['bufferView'] for a in doc['accessors']}|{image['bufferView'] for image in doc.get('images',[]) if 'bufferView' in image})
    view_map={old:new for new,old in enumerate(used_views)};packed=bytearray();views=[]
    for old in used_views:
        view=dict(doc['bufferViews'][old]);offset=view.get('byteOffset',0);payload=binary[offset:offset+view['byteLength']]
        while len(packed)%4:packed.append(0)
        view['byteOffset']=len(packed);packed.extend(payload);views.append(view)
    for a in doc['accessors']:a['bufferView']=view_map[a['bufferView']]
    for image in doc.get('images',[]):
        if 'bufferView' in image:image['bufferView']=view_map[image['bufferView']]
    doc['bufferViews']=views;binary=packed
    doc['buffers'][0]['byteLength']=len(binary)
    js=json.dumps(doc,separators=(',',':')).encode();js+=b' '*((-len(js))%4);binary+=b'\0'*((-len(binary))%4)
    output=struct.pack('<III',0x46546c67,2,12+8+len(js)+8+len(binary))+struct.pack('<II',len(js),0x4e4f534a)+js+struct.pack('<II',len(binary),0x004e4942)+binary
    target.parent.mkdir(parents=True,exist_ok=True)
    if not target.exists() or target.read_bytes()!=output:target.write_bytes(output)
    return {'triangles_before':before,'triangles_removed':removed,'sha256':hashlib.sha256(output).hexdigest()}
report={}
for key,asset in selection['assets'].items():
    source=Path(asset['source']);target=REPO/asset['resource'].replace('res://','game/')
    report[key]=clean(source,target,asset['cleanup']);print(key,report[key])
(REPO/'game/Assets/Hunyuan/Weapons/manifest.json').write_text(json.dumps(selection,indent=2))
(REPO/'game/Assets/Hunyuan/Weapons/cleanup-report.json').write_text(json.dumps(report,indent=2))
