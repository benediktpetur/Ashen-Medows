"""Import selected static image-to-3D drafts; originals and existing actors are preserved."""
from pathlib import Path
import json, hashlib, shutil, re, struct
REPO=Path(__file__).resolve().parents[1]
ROOT=REPO.parents[1]
BATCH=ROOT/'Ashen Meadows - 3D Batch'
OUT=REPO/'game/Assets/Hunyuan/Static'
OUT.mkdir(parents=True,exist_ok=True)
audit=json.loads((BATCH/'Model Audit.json').read_text(encoding='utf-8'))
coverage=json.loads((ROOT/'Ashen Meadows - Local Styled Collection/Catalogue Coverage.json').read_text(encoding='utf-8'))
manifest={}; deferred=[]
rejections=json.loads((REPO/'scripts/batch-model-rejections.json').read_text(encoding='utf-8'))
def add(key,name,source,usage):
    if key in rejections:
        deferred.append(dict(key=key,reason=rejections[key]));return False
    source=Path(source)
    data=source.read_bytes(); length=struct.unpack_from('<I',data,12)[0]
    doc=json.loads(data[20:20+length])
    if not doc.get('meshes') or not doc.get('images'):return False
    # Refuse external dependencies; every game copy must be self-contained.
    if any('uri' in x and not x['uri'].startswith('data:') for x in doc.get('buffers',[])+doc.get('images',[])):return False
    target=OUT/(re.sub('[^a-zA-Z0-9]+','-',key).strip('-')+'.glb')
    if target.exists() and hashlib.sha256(target.read_bytes()).digest()!=hashlib.sha256(data).digest():
        raise RuntimeError('Existing game copy differs; review before replacing '+str(target))
    if not target.exists():shutil.copy2(source,target)
    manifest[key]=dict(name=name,source=str(source),sha256=hashlib.sha256(data).hexdigest(),resource='res://Assets/Hunyuan/Static/'+target.name,usage=usage)
    return True
for row in audit['catalogue']:
    # Existing articulated actors stay intact. Fused houses cannot replace usable interiors.
    if row['category'] not in ['Weapons','Equipment','Items','Buildings','Furniture','Props','Nature'] or row['category']=='Houses':continue
    if not row['key'].startswith('catalog-'):continue
    candidates=list(row['new_textured'])
    covered=next((c for c in coverage if c['key']==row['key'] and c['status']=='Covered concept'),None)
    if covered:
        candidates += [j['model'] for j in audit['queue'] if j['collection']=='Styled Collection' and j['relative'] in covered['paths'] and j['status']=='complete']
    if candidates:
        add(row['key'],row['name'],sorted(candidates)[0],'inventory/crafting preview')
    else:deferred.append(dict(key=row['key'],reason='No new matching textured conversion'))
for key,relative in [('wreck-car','Props/Vehicles/Abandoned Cars/abandoned-car-01.jpg'),('burned-car','props/burned-car-burned-car-option-01.jpg')]:
    job=next((j for j in audit['queue'] if j['relative']==relative and j['status']=='complete'),None)
    if job:add(key,key,job['model'],'world wreck visual; existing collision retained')
world_selections=REPO/'scripts/world-model-selections.json'
if world_selections.exists():
    for key,relative in json.loads(world_selections.read_text(encoding='utf-8')).items():
        job=next((j for j in audit['queue'] if j['relative']==relative and j['status']=='complete'),None)
        if not job:raise RuntimeError('Selected world model is missing: '+relative)
        add(key,Path(relative).stem,job['model'],'world environment; authored gameplay collision retained')
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
(OUT/'deferred.json').write_text(json.dumps(deferred,indent=2),encoding='utf-8')
print(f'Prepared {len(manifest)} self-contained static models; {len(deferred)} static catalogue entries deferred (missing or rejected).')
