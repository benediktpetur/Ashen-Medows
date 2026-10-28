shader_type spatial;
render_mode specular_disabled;

uniform vec4 paint : source_color = vec4(.45,.43,.38,1.0);
uniform int surface_kind = 0;
uniform float variation = 0.0;
varying vec3 world_point;
varying vec3 world_face;

float hash21(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float grain(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash21(i),hash21(i+vec2(1,0)),f.x),mix(hash21(i+vec2(0,1)),hash21(i+vec2(1,1)),f.x),f.y);}
void vertex(){world_point=(MODEL_MATRIX*vec4(VERTEX,1)).xyz;world_face=MODEL_NORMAL_MATRIX*NORMAL;}
void fragment(){
 vec3 normal_axis=abs(world_face);
 vec2 uv=normal_axis.y>.65?world_point.xz:(normal_axis.z>normal_axis.x?world_point.xy:world_point.zy);
 uv+=vec2(variation*13.7,variation*2.31);
 float footprint=max(length(dFdx(uv)),length(dFdy(uv)));
 float detail=1.0-smoothstep(.035,.18,footprint);
 vec3 color=paint.rgb;
 float ink=1.0;
 if(surface_kind==0){
  // Narrow overlapping timber boards with local paint loss and fine grain.
  float row=floor(uv.y/.19);float seam=smoothstep(.008,.025,mod(uv.y,.19));
  float board=hash21(vec2(row,variation));
  float wood=grain(uv*vec2(1.2,90.0));
  float chips=smoothstep(.72,.88,grain(uv*vec2(1.5,7.0)));
  color=mix(color*(.87+board*.22+wood*.10),vec3(.28,.22,.16)*(.8+wood*.3),chips*.65);
  ink=mix(.42,1.0,seam);
 }else if(surface_kind==1){
  // Staggered roof shingles, with charcoal seams and restrained tile variation.
  vec2 tile=world_point.xz*vec2(2.2,3.0);tile.x+=mod(floor(tile.y),2.0)*.5;
  vec2 f=fract(tile);float tile_tone=hash21(floor(tile)+variation);
  ink=mix(.35,1.0,smoothstep(.018,.06,min(f.x,f.y)));
  color*=.82+tile_tone*.25;
 }else if(surface_kind==2){
  vec2 brick=uv/vec2(.42,.18);brick.x+=mod(floor(brick.y),2.0)*.5;
  vec2 f=fract(brick);float edge=smoothstep(.025,.065,min(f.x,f.y));
  color=mix(vec3(.24,.23,.21),color*(.78+hash21(floor(brick))*.28),edge);
 }else if(surface_kind==3){
  float rib=fract(uv.x*5.0);ink=.76+.24*smoothstep(.08,.25,rib)*(1.0-smoothstep(.78,.96,rib));
  float rust=smoothstep(.67,.88,grain(uv*vec2(2.0,.7)));
  color=mix(color,vec3(.38,.21,.12),rust*.55);
 }else{
  vec2 panel=fract(uv/vec2(3.0,3.7));
  ink=mix(.65,1.0,smoothstep(.004,.014,min(panel.x,panel.y)));
  color*=.90+grain(uv*2.0)*.13;
 }
 float hatch=smoothstep(.94,.995,fract((uv.x+uv.y*.8)*29.0))*grain(floor(uv*3.0));
 // Fade sub-pixel seams and hatching instead of letting distant roofs shimmer.
 color=mix(paint.rgb*.94,color,detail);ink=mix(1.0,ink,detail);
 ALBEDO=color*ink*(1.0-hatch*.07*detail);ROUGHNESS=1.0;
}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.55?1.0:(n>.05?.72:.36);DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
