"""Derive a deterministic playable atlas from the user's November world map.
Keep the original image and extracted semantic preview for review. No AI service.
"""
from pathlib import Path
import json, shutil, struct
import cv2
import numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'game/WorldMap';OUT.mkdir(exist_ok=True)
SOURCE=Path('C:/Users/bened/Desktop/GAME MAP!!! LETS GOOOO!!!!.png')
shutil.copy2(SOURCE,OUT/'reference.png')
N=2048;SPAN=9000;MINX=-2400;MINZ=-5000
rgb=np.array(Image.open(SOURCE).convert('RGB').resize((N,N),Image.Resampling.LANCZOS))
r,g,b=[rgb[:,:,i].astype(float) for i in range(3)]
# Blue waterways are separate from green terrain and grey buildings.
water=((b-r>24)&(g-r>14)&(b>g-35)&(b>100)).astype('uint8')
water[:4,:]=0;water[-4:,:]=0;water[:,:4]=0;water[:,-4:]=0
count,labels,stats,_=cv2.connectedComponentsWithStats(water,8)
water=np.isin(labels,[i for i in range(1,count) if stats[i,4]>7]).astype('uint8')
# Roads are white/cream or ochre, connected across the map. Detached roofs and
# parcel outlines are excluded by connected-component size, then combined with
# explicitly traced arterials so bridges and interchanges remain continuous.
white=(np.maximum.reduce([r,g,b])-np.minimum.reduce([r,g,b])<27)&(r>192)&(g>190)&(b>169)
yellow=(r>174)&(g>137)&(b<g-23)&(r>g+12)
strict=((np.maximum.reduce([r,g,b])-np.minimum.reduce([r,g,b])<23)&(r>205)&(g>202)&(b>185)).astype('uint8')
large=cv2.morphologyEx(strict,cv2.MORPH_OPEN,np.ones((9,9),np.uint8))
yy_grid,xx_grid=np.indices((N,N));urban=(xx_grid<N*.68)&(yy_grid<N*.70)
white=np.where(urban,strict*(1-large),white).astype('uint8')
candidate=(white|yellow).astype('uint8');candidate[:4,:]=0;candidate[-4:,:]=0;candidate[:,:4]=0;candidate[:,-4:]=0
count,labels,stats,_=cv2.connectedComponentsWithStats(candidate,8)
roads=np.isin(labels,[i for i in range(1,count) if stats[i,4]>640]).astype('uint8')
def line(points,width,value=1):
    cv2.polylines(roads,[np.array([(int(x*N),int(y*N)) for x,y in points],np.int32)],False,value,width,cv2.LINE_8)
highway=[(0,.74),(.13,.69),(.26,.64),(.40,.61),(.49,.56),(.56,.48),(.60,.40),(.63,.31),(.66,.23),(.71,.16),(.78,.12),(.80,.07),(.83,0)]
line(highway,8)
line([(.70,.16),(.80,.12),(.90,.11),(1,.10)],8)
# Larger interchanges at the four junctions in the supplied image.
for x,y in [(.14,.68),(.42,.58),(.59,.39),(.79,.12)]:
    cv2.ellipse(roads,(int(x*N),int(y*N)),(20,30),-25,0,360,1,3)
# The starting lane is retained as one short residential street on the new map.
sx=(1100-MINX)/SPAN;sz=(160-MINZ)/SPAN
cv2.rectangle(roads,(int((sx-.0098)*N),int((sz-.0225)*N)),(int((sx+.0098)*N),int((sz+.005)*N)),0,-1)
line([(sx,sz+.005),(sx,sz-.021)],3)
line([(sx,sz-.019),(sx+.025,sz-.019)],3)
roads[int(N*.965):,int(N*.36):int(N*.66)]=0
roads[int(N*.94):,int(N*.96):]=0
water=cv2.morphologyEx(water,cv2.MORPH_CLOSE,np.ones((9,9),np.uint8))
# Functional road shoulders need a smooth signed distance, not pixel stair steps.
roads=cv2.morphologyEx(roads,cv2.MORPH_CLOSE,np.ones((2,2),np.uint8))
road_dist=(cv2.distanceTransform(1-roads,cv2.DIST_L2,5)-cv2.distanceTransform(roads,cv2.DIST_L2,5))*SPAN/N
water_dist=(cv2.distanceTransform(1-water,cv2.DIST_L2,5)-cv2.distanceTransform(water,cv2.DIST_L2,5))*SPAN/N
district=np.full((N,N),2,np.uint8)
def polygon(points,value):cv2.fillPoly(district,[np.array([(int(x*N),int(y*N)) for x,y in points],np.int32)],value)
polygon([(0,0),(.41,0),(.40,.12),(.40,.35),(.52,.43),(.51,.55),(.43,.61),(.23,.66),(.07,.70),(0,.69)],1)
polygon([(.055,.095),(.16,.02),(.39,.015),(.405,.30),(.35,.365),(.12,.365),(.035,.24)],4)
polygon([(.405,.035),(.55,.035),(.63,.15),(.64,.30),(.60,.405),(.48,.425),(.39,.34)],3)
polygon([(.52,.43),(.65,.33),(.79,.32),(.84,.43),(.77,.53),(.84,.65),(.94,.83),(.92,1),(.62,1),(.53,.87),(.34,.81),(.40,.67)],5)
# Park and rail-yard reservations from the image.
polygon([(.27,.025),(.33,.025),(.33,.10),(.27,.10)],6)
polygon([(.48,.13),(.52,.13),(.56,.31),(.52,.41),(.45,.40),(.48,.30)],7)
polygon([(.075,.75),(.16,.75),(.18,.85),(.08,.86)],8)
# Preserve the authored starting neighbourhood within its small reserve.
cv2.rectangle(district,(int((sx-.012)*N),int((sz-.025)*N)),(int((sx+.012)*N),int((sz+.008)*N)),1,-1)
district[water!=0]=0
# Nearest paved pixel coordinates allow lots to face the real curved streets.
_,nearest=cv2.distanceTransformWithLabels(1-roads,cv2.DIST_L2,5,labelType=cv2.DIST_LABEL_PIXEL)
yy,xx=np.nonzero(roads);lookup=np.zeros((len(xx)+1,2),np.float32);lookup[1:,0]=MINX+xx*SPAN/(N-1);lookup[1:,1]=MINZ+yy*SPAN/(N-1)
near=lookup[np.minimum(nearest,len(xx))]
with (OUT/'atlas.bin').open('wb') as f:
    f.write(struct.pack('<4siffff',b'AMAP',N,float(MINX),float(MINZ),float(SPAN),float(SPAN)))
    record=np.dtype([('road','<f4'),('water','<f4'),('nearx','<f4'),('nearz','<f4'),('district','u1')])
    data=np.zeros((N,N),record);data['road']=road_dist;data['water']=water_dist;data['nearx']=near[:,:,0];data['nearz']=near[:,:,1];data['district']=district;f.write(data.tobytes())
preview=np.zeros_like(rgb);palette=[(66,139,175),(144,163,109),(54,91,61),(136,130,117),(111,119,122),(176,181,126),(104,147,84),(96,90,82),(151,145,126)]
for i,color in enumerate(palette):preview[district==i]=color
preview[roads!=0]=(224,212,169);preview[(water!=0)&(roads==0)]=(66,139,175)
Image.fromarray(preview).save(OUT/'semantic-preview.png')
(OUT/'atlas.json').write_text(json.dumps(dict(version=1,size=N,world_bounds=[MINX,MINZ,MINX+SPAN,MINZ+SPAN],scale_metres=SPAN,source=SOURCE.name,water_pixels=int(water.sum()),road_pixels=int(roads.sum()),districts=['water','suburbs','woodland','industry','city','farmland','park','rail yard','quarry'],highway=highway),indent=2))
print(json.dumps(dict(size=N,water=int(water.sum()),roads=int(roads.sum()),output=str(OUT)),indent=2))
