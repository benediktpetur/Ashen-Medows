shader_type spatial;
render_mode cull_disabled, specular_disabled;
uniform int world_seed=431;
uniform float spawn_x=1100.0;
uniform float spawn_z=160.0;
varying vec3 world_pos;
float route_legacy(float z){float phase=float(world_seed%997)*.006;return 105.0*sin(z*.003)+55.0*(sin(z*.007+phase)-sin(phase));}
float route_x(float z){float t=clamp((spawn_z-z)/(spawn_z+550.0),0.0,1.0);t=t*t*t*(t*(t*6.0-15.0)+10.0);return z<=-550.0?route_legacy(z):mix(spawn_x,route_legacy(-550.0),t);}
float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash(i),hash(i+vec2(1,0)),f.x),mix(hash(i+vec2(0,1)),hash(i+vec2(1)),f.x),f.y);}
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;}
void fragment(){
 vec2 p=world_pos.xz;int kind=int(round(COLOR.a*8.0));
 // Compatibility for existing authored overlays and distant terrain.
 if(kind==8){kind=COLOR.b>COLOR.r*1.1&&COLOR.b>COLOR.g*.95?6:(COLOR.g>COLOR.r*1.15?0:(COLOR.r>COLOR.g*1.15?1:(COLOR.r>.3?3:2)));}
 float footprint=max(length(dFdx(p)),length(dFdy(p)));
 float detail=1.0-smoothstep(.025,.15,footprint);
 float broad=noise(p*.065),patch=noise(p*.37),fine=noise(p*5.0);
 vec3 base=COLOR.rgb;
 float rx=route_x(p.y),dx=abs(p.x-rx);
 float edge=35.0*sin(rx*.006+float(world_seed)*.01)+20.0*sin(rx*.014);
 int tier=p.y> -320.0+edge?1:(p.y> -1210.0+edge?2:(p.y> -1740.0+edge?3:4));
 float half_width=tier==3?7.0:5.0;
 float spacing=tier==3?64.0:32.0;
 float junction=min(mod(p.y+4096.0,spacing),spacing-mod(p.y+4096.0,spacing));
 float crossroad=min(mod(p.y+4096.0,128.0),128.0-mod(p.y+4096.0,128.0));
 bool crossing=junction<4.0||crossroad<8.0;
 float river=-2510.0+55.0*sin(p.x*.003+float(world_seed)*.003)+25.0*sin(p.x*.008);
 bool route_here=p.y>river+72.0&&kind!=7;
 // Paint the exact route directly on the terrain triangles. Separate road
 // sheets caused depth interference and could diverge from hill collision.
 if(route_here&&dx<half_width){kind=2;base=vec3(.19,.20,.20);}
 else if(route_here&&dx<half_width+(tier>=3?2.6:1.1)){kind=tier>=3?(crossing?2:3):5;base=tier>=3?(crossing?vec3(.19,.20,.20):vec3(.47,.45,.39)):vec3(.38,.35,.28);}
 if(kind==0){
  base=mix(vec3(.21,.26,.13),vec3(.39,.38,.23),smoothstep(.36,.78,broad));
  base=mix(base,vec3(.32,.27,.18),smoothstep(.68,.85,patch)*.3);
 }else if(kind==4){
  base=mix(vec3(.22,.20,.14),vec3(.36,.29,.19),broad);
  float leaves=smoothstep(.68,.81,noise(p*3.2));base=mix(base,vec3(.41,.35,.20),leaves*.28*detail);
 }else if(kind==1||kind==5){
  base*=.89+patch*.20;float stones=smoothstep(.72,.82,fine)*detail;
  base=mix(base,kind==5?vec3(.48,.45,.37):vec3(.39,.33,.23),stones*.35);
 }else if(kind==2){
  base*=.89+patch*.16;
  float crack=(1.0-smoothstep(.008,.025,abs(noise(p*.8)-.48)))*smoothstep(.55,.72,broad);
  base*=1.0-crack*.25*detail;
 }else if(kind==3){
  vec2 joint=min(fract(p/2.4),1.0-fract(p/2.4));
  float seam=1.0-smoothstep(.005,.015,min(joint.x,joint.y));
  base*=.94+patch*.10-seam*.15*detail;
 }
 if(kind!=6&&kind!=7)base*=.96+fine*.07*detail;
 if(route_here&&tier>=3&&dx<half_width){
  float aa=max(.015,fwidth(dx));float edge_line=1.0-smoothstep(.055,.055+aa,abs(dx-(half_width-.4)));
  float dash=1.0-smoothstep(5.7,6.0,mod(p.y+4096.0,16.0));
  float center=(1.0-smoothstep(.065,.065+aa,dx))*dash;
  float lanes=tier==3?(1.0-smoothstep(.055,.055+aa,abs(dx-3.3)))*dash:0.0;
  if(junction>5.0&&crossroad>9.0){base=mix(base,vec3(.63,.55,.34),center);base=mix(base,vec3(.68,.66,.57),max(edge_line,lanes));}
 }
 ALBEDO=base;ROUGHNESS=1.0;
}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.5?1.0:n>.08?.78:.48;DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
