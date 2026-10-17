shader_type spatial;
render_mode specular_disabled;
varying vec3 wp;
varying vec3 wn;
float grain(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);float a=fract(sin(dot(i,vec2(127.1,311.7)))*43758.5453);float b=fract(sin(dot(i+vec2(1,0),vec2(127.1,311.7)))*43758.5453);float c=fract(sin(dot(i+vec2(0,1),vec2(127.1,311.7)))*43758.5453);float d=fract(sin(dot(i+vec2(1,1),vec2(127.1,311.7)))*43758.5453);return mix(mix(a,b,f.x),mix(c,d,f.x),f.y);}
void vertex(){wp=(MODEL_MATRIX*vec4(VERTEX,1)).xyz;wn=MODEL_NORMAL_MATRIX*NORMAL;}
void fragment(){vec2 uv=abs(wn.z)>abs(wn.x)?wp.xy:wp.zy;float row=floor(uv.y*6.0);float seam=smoothstep(.035,.08,fract(uv.y*6.0));float wear=smoothstep(.55,.80,grain(uv*vec2(1.2,2.2)));float fine=grain(uv*vec2(4.0,170.0));vec3 paint=vec3(.48,.47,.43)*( .88+fine*.15+grain(vec2(row,1.0))*.14);vec3 exposed=vec3(.27,.24,.19)*( .8+fine*.3);ALBEDO=mix(paint,exposed,wear)*mix(.42,1.0,seam);ROUGHNESS=1.0;}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.55?1.0:(n>.05?.72:.36);DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
