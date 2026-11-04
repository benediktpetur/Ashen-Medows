"""Stage completed house sources without changing the generation batch."""
from pathlib import Path
import hashlib, json, shutil

repo = Path(__file__).resolve().parents[1]
source = repo.parents[1] / 'Ashen Meadows - AI Houses - 2026-11-01/Models/Full 9B Catalogue/environment'
out = repo / 'game/Assets/GeneratedHouses'
(out / 'Source').mkdir(parents=True, exist_ok=True)
manifest = []
for folder in sorted(source.glob('house-*')):
    model = folder / 'textured.glb'
    if folder.name.startswith('house-kit-') or not model.exists():
        continue
    shutil.copy2(model, out / 'Source' / (folder.name + '.glb'))
    manifest.append(dict(key=folder.name, source=str(model), sha256=hashlib.sha256(model.read_bytes()).hexdigest()))
(out / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
print(f'Staged {len(manifest)} houses; modular components and unfinished jobs remain in the source batch.')
