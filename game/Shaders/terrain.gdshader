shader_type spatial;
render_mode cull_disabled, specular_disabled;
uniform sampler2D ink_atlas : source_color, filter_linear_mipmap;
varying vec3 world_pos;
float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash(i),hash(i+vec2(1,0)),f.x),mix(hash(i+vec2(0,1)),hash(i+vec2(1)),f.x),f.y);}
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;}
void fragment(){vec2 p=world_pos.xz;float grain=noise(p*7.0);float broad=noise(p*.13);float hatch=smoothstep(.83,.95,fract((p.x+p.y)*8.5));vec3 base=COLOR.rgb;float road=step(base.g,base.r+0.015);float cracks=step(.83,noise(p*1.6))*step(.65,noise(p*4.0));base*=.82+broad*.24+grain*.13-hatch*.025-cracks*.18*road;float dirt=step(COLOR.g*1.15,COLOR.r);vec2 cell=road>0.5&&dirt<.5?vec2(0.0,1.0):vec2(1.0,1.0);vec3 ink=texture(ink_atlas,cell*.5+vec2(.001)+fract(p*.35)*.498).rgb;base=mix(base,road>0.5&&dirt<.5?ink*.7:ink*.45+base*.5,road>0.5?.68:.4);if(COLOR.b>COLOR.g*.95&&COLOR.b>COLOR.r*1.1){base=COLOR.rgb;}ALBEDO=base;ROUGHNESS=1.0;}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.5?1.0:n>.08?.78:.48;DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
