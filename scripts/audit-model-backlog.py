"""Reconcile conversion audit, registered models and actual runtime preview lookups.
Run a regional game check first to refresh user://tests/runtime-model-coverage.json.
This is read-only with respect to source images, conversions and game assets.
"""
from pathlib import Path
from collections import Counter,defaultdict
import json,argparse

repo=Path(__file__).resolve().parents[1];workspace=repo.parents[1]
parser=argparse.ArgumentParser();parser.add_argument('--date',default='2026-10-29');args=parser.parse_args()
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
audit=read(workspace/'Ashen Meadows - 3D Batch/Model Audit.json')
manifest=read(repo/'game/Assets/Hunyuan/Static/manifest.json')
deferred=read(repo/'game/Assets/Hunyuan/Static/deferred.json')
rejected=read(repo/'scripts/batch-model-rejections.json')
coverage=read(workspace/'Ashen Meadows - Local Styled Collection/Catalogue Coverage.json')
catalog={r['key']:r for r in audit['catalogue']};concepts={r['key']:r for r in coverage}
runtime_path=workspace/'AshenMeadow-Godot/.tools/appdata/AshenMeadowExpedition/tests/runtime-model-coverage.json'
runtime=read(runtime_path)
assert all('lookup_key' in r for r in runtime),'Refresh regional runtime model coverage with the current game build'
aliases={'broadleaf-tree':'world-oak-c','pine-tree':'world-pine','forest-tree':'world-oak-c','rock':'world-rock','wreck-car':'wreck-car'}
aliases.update({f'furniture-{i}':f'world-furniture-{i}' for i in range(4)})
aliases.update({r['key']:r['lookup_key'] for r in runtime if r['registered_model'] and r['key']!=r['lookup_key']})
no_exact=[r for r in audit['catalogue'] if not any(Path(p).exists() for p in r['textured'])]
covered=[dict(key=r['key'],name=r['name'],replacement=r['key'] if r['key'] in manifest else aliases[r['key']]) for r in no_exact if r['key'] in manifest or r['key'] in aliases and aliases[r['key']] in manifest]
unconverted=[r for r in no_exact if r['key'] not in {a['key'] for a in covered}]
actor_stems={p.stem for p in (repo/'game/Assets/Hunyuan/Actors').glob('*.glb')}
def actor_registered(key):
 prefix=key.replace('/','-');return any(s==prefix or s.startswith(prefix+'-') for s in actor_stems)
available=[r for r in audit['catalogue'] if r['textured'] and r['key'] not in manifest and r['key'] not in aliases and r['key'] not in rejected and not actor_registered(r['key'])]
missing=defaultdict(list)
for r in runtime:
 if not r['registered_model']:missing[r['lookup_key']].append(r)
def status(key):
 r=catalog.get(key,{})
 if key in rejected:return 'Replace rejected conversion',rejected[key]
 if r.get('new_textured'):return 'Review existing new conversion','A textured draft exists but this default runtime lookup has no registered model'
 if r.get('older_textured'):return 'Review existing older conversion','Older textured draft exists; do not regenerate before reviewing it'
 return 'Create or reuse model','No matching textured draft found in the audited catalogue'
rows=[]
for key,items in sorted(missing.items()):
 action,reason=status(key)
 if key=='catalog-material/1':action='Review reuse candidate';reason='The integrated world-rock model is available for a stone inventory/pickup adaptation'
 rows.append(dict(key=key,name=items[0]['name'],aliases=sorted({i['name'] for i in items}),kind=items[0]['kind'],tier=items[0]['tier'],action=action,reason=reason,concepts=concepts.get(key,{}).get('paths',[])))
failed=[r for r in audit['queue'] if r['status']=='failed']
priorities=[
 ('P1','Grass / tall dry grass / fern / low shrub LOD set','Procedural ground cover is now present; dedicated optimized meshes would improve silhouettes and draw-distance transitions.'),
 ('P1','Enterable house modular kit','Open door/window wall sections, interior faces, roof, foundation, porch and corners. Fused house conversions cannot replace usable rooms.'),
 ('P1','Garden fence and gate kit','Straight, corner, sloped, damaged sections and working gate apertures. Review existing build drafts before creating more.'),
 ('P1','Roadside and pavement kit','Curbs, dropped curbs, drains, manholes, road signs and guardrail endings. Current roads are terrain materials, not missing terrain models.'),
 ('P1','Industrial yard set','Pallets, cable reels, crates, bins, drums and scrap piles. Existing catalogue drafts need selection/scale review before any new batch.'),
 ('P2','Fallen logs / stumps / branches / leaf-litter set','Review completed Nature input conversions; optimize and add LODs instead of generating duplicate oak trees.'),
 ('P2','City frontage kit','Shopfronts, doors, broken/boarded windows, awnings, vents, pipes and rooftop units. Current facade trim is procedural.'),
 ('P2','Suburban garden and household dressing','Planters, mailbox, bins, bicycle, clothesline and porch light. Use approved current furniture; a full duplicate furniture batch is unnecessary.'),
 ('P2','Street furniture and debris','Benches, bus stop, utility cabinets, litter, broken masonry and damaged traffic barriers. Review existing props first.'),
 ('P2','Wildlife and dog animation-ready assets','Textured drafts exist; anatomy checks, rigging, LODs and animation are the remaining work. Static GLBs are not animation-ready replacements.'),
 ('P3','Variant survivor / story NPC assets','Use current survivor/actor rigs as the baseline; different clothing and identity variants need rig-compatible preparation.'),
 ('P3','Dungeon and landmark modular sets','Doors with real openings, stairs, pipe junctions, sewer/medical/industrial dressing. Preserve existing colliders and authored story rooms.')]
out=repo/'MODEL-BACKLOG.md'
lines=[f'# Model work still needed — {args.date}','',
 'This list separates genuinely absent conversions, rejected drafts, reusable existing models and procedural art that could benefit from dedicated assets. A model being present does not mean it is approved, rigged, optimized or placed in the world.','',
 f'- Conversion inventory: {audit["summary"]["queue_status"]["complete"]} completed textured outputs from {audit["summary"]["queue_total"]} inputs; {len(failed)} failed inputs, including variants.',
 f'- Current static/environment registry: {len(manifest)} keys. Many are inventory/crafting previews; registration does not imply world placement.',
 f'- Catalogue entries without an exact textured draft: {len(no_exact)}. Of those, {len(covered)} already have explicit working game replacements.',
 f'- Default runtime preview lookups without a registered imported model: {len(rows)} unique lookup slots across {len(runtime)} item definitions. Procedural visuals remain functional.',
 f'- Importer deferrals: {len(deferred)}; {sum(r["key"] in rejected for r in deferred)} rejected conversions, the rest missing a matching new conversion. Deferrals overlap the runtime list; do not add these counts together.','',
 'Runtime coverage is exported by the game and uses the same first matching kind/index/tier lookup as ItemModel, so aliases are not counted as separate missing meshes. Authored relic appearances, first-person views, procedural world structures and animation rigs are separate paths.','',
 '## Best next art tasks','', '| Priority | Asset group | What remains |','|---|---|---|']
for p,n,note in priorities:lines.append(f'| {p} | {n} | {note} |')
lines+=['','## No exact converted catalogue model','',
 'These entries have no exact matching textured GLB in the audited folders. Review the reuse note before generating anything.','',
 '| Asset | Key | Next action |','|---|---|---|']
for r in unconverted:
 note='Review world-rock reuse for stone; no new generation required if that adaptation is suitable.' if r['key']=='catalog-material/1' else 'Create a matching model or explicitly map a suitable existing draft.'
 lines.append(f'| {r["name"]} | `{r["key"]}` | {note} |')
lines+=['','### Previously labelled missing, already covered','']
for r in covered:lines.append(f'- {r["name"]}: `{r["replacement"]}` is registered through the corresponding model lookup; the source may be a selected-collection alternative outside the catalogue audit match.')
lines+=['','## Every unfilled default runtime model slot','',
 'These are imported-model gaps, not invisible/missing gameplay objects. Rejected models need replacement or cleanup; existing unregistered drafts need review, not an automatic generation job.','',
 '| Asset | Key | Next action | Detail |','|---|---|---|']
for r in rows:lines.append(f'| {r["name"]} | `{r["key"]}` | {r["action"]} | {r["reason"].replace("|","/")} |')
lines+=['','## All rejected static conversions','',
 'The files exist but were rejected after rendering. Existing procedural art remains active. A new conversion is only one option; cleanup or a reviewed alternative may be sufficient.','',
 '| Asset | Key | Rejection |','|---|---|---|']
for key,reason in sorted(rejected.items()):lines.append(f'| {catalog.get(key,{}).get("name",key)} | `{key}` | {reason.replace("|","/")} |')
lines+=['','## Existing textured drafts to review before generating more','',
 'These catalogue entries have a textured draft but no matching registered static key or recognized actor key. They may need cleanup, rigging, a modular remake or integration. This is a review queue, not a claim that every file is usable or every world object is absent. Rejected conversions are listed separately above.','',
 '| Category | Asset | Key | Draft files |','|---|---|---|---|']
for r in sorted(available,key=lambda r:(r['category'],r['name'])):lines.append(f'| {r["category"]} | {r["name"]} | `{r["key"]}` | {len(r["textured"])} |')
lines+=['','## Failed input variants','',
 'These are failed individual input jobs, not necessarily missing asset types; another successful variant may already cover the asset.','']
for r in failed:lines.append(f'- `{r["collection"]}/{r["relative"]}`')
lines+=['','## Named story art and assembly work outside the default item list','',
 'These are source-backed art tasks without a one-to-one conversion catalogue slot. They are not added to the 96 default item-lookup gaps or treated as proven absent files. Existing procedural assemblies work; dedicated replacements need a separate shape/rig/interaction review.','',
 '- Mara Holt, Ivar Reed and Nora Vale: distinct faces, clothing and accessories sharing the working survivor rig (`ExpeditionCommunityRules.cs`).',
 '- Foreman reinforced apron, Wrecker road helmet, Bellkeeper armored gloves and Brood-sealed cuirass: rig-compatible wearable variants, preserving their authored relic appearances (`ExpeditionMillBoss.cs`, `ExpeditionCampaignBosses.cs`).',
 '- Birthday room: whole/cut cake, candles, music box with lid/crank, party hat, presents, bunting/banner, tablecloth, plates/cups/cutlery and balloon debris (`ExpeditionBirthdayArt.cs`). Reuse current tables/chairs; lettering can remain a texture.',
 '- Willow/story house: reusable stairs, floor/upper-floor/roof sections, open wall/window sections, doors and porch trim (`ExpeditionWillowArchitecture.cs`, `ExpeditionWillowUpperFloor.cs`, `ExpeditionWillowAccess.cs`). Preserve the functional layout; a fused house GLB is insufficient.',
 '- Dungeon/campaign rooms: gates, supports, room-specific pipework and encounter mechanisms (`ExpeditionContentDungeons.cs`, `ExpeditionCampaignWorld.cs`). A room-by-room modular art pass remains; no exhaustive unique-mesh count is claimed.',
 '- First-person weapons/tools and animated wildlife: pivots, grips, separated moving components, rigging, retopology and animation are additional tasks even when a static inventory GLB exists.','']
lines+=['','## Reproduce','',
 'Refresh the source conversion audit with the workspace `Ashen Meadows - 3D Batch/audit_models.py`; run the game with `--region-check` to export runtime coverage; then run `scripts/audit-model-backlog.py --date YYYY-MM-DD`. The script writes this report and `model-backlog.json`; it does not launch generation or modify source assets.','']
out.write_text('\n'.join(lines),encoding='utf-8')
(repo/'model-backlog.json').write_text(json.dumps(dict(date=args.date,summary=dict(registered=len(manifest),runtime_item_definitions=len(runtime),unfilled_runtime_lookups=len(rows),exact_catalogue_conversion_gaps=len(no_exact),already_covered=len(covered),rejected=len(rejected),failed_input_variants=len(failed)),no_exact_conversion=[dict(key=r['key'],name=r['name'],category=r['category']) for r in unconverted],covered_aliases=covered,runtime_model_gaps=rows,existing_drafts_for_review=[dict(key=r['key'],name=r['name'],category=r['category'],sources=r['textured']) for r in available],rejected=rejected,failed_inputs=[dict(collection=r['collection'],relative=r['relative']) for r in failed],priorities=[dict(priority=p,group=n,note=t) for p,n,t in priorities]),indent=2),encoding='utf-8')
print(json.dumps(dict(report=str(out),runtime_definitions=len(runtime),runtime_gaps=len(rows),actions=dict(Counter(r['action'] for r in rows)),exact_conversion_gaps=len(unconverted),covered=len(covered)),indent=2))
