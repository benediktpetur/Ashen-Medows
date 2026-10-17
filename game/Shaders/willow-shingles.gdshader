shader_type spatial;
render_mode specular_disabled;
varying vec3 wp;
void vertex(){wp=(MODEL_MATRIX*vec4(VERTEX,1)).xyz;}
void fragment(){vec2 uv=wp.xz*vec2(3.0,4.0);uv.x+=mod(floor(uv.y),2.0)*.5;vec2 cell=floor(uv),f=fract(uv);float r=fract(sin(dot(cell,vec2(41.3,71.7)))*43758.54);float seam=smoothstep(.02,.07,min(f.x,f.y));float moss=smoothstep(.55,.85,fract(sin(dot(floor(wp.xz*1.5),vec2(13.1,37.7)))*43758.54));ALBEDO=mix(vec3(.23,.24,.22),vec3(.18,.25,.15),moss)*(.75+r*.3)*mix(.4,1.0,seam);ROUGHNESS=1.0;}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.55?1.0:(n>.05?.72:.36);DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
