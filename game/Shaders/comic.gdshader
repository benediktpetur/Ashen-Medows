shader_type spatial;
render_mode specular_disabled;
uniform vec4 base_color : source_color = vec4(0.4,0.5,0.4,1.0);
uniform float surface_grain = 0.08;
uniform sampler2D ink_atlas : source_color, filter_linear_mipmap;
uniform int texture_cell = -1;
uniform float texture_scale = 1.0;
varying vec3 world_pos;
varying vec3 world_normal;
float noise(vec3 p){vec3 i=floor(p);vec3 f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(fract(sin(dot(i,vec3(17.1,71.7,33.4)))*43758.5),fract(sin(dot(i+vec3(1,0,0),vec3(17.1,71.7,33.4)))*43758.5),f.x),mix(fract(sin(dot(i+vec3(0,0,1),vec3(17.1,71.7,33.4)))*43758.5),fract(sin(dot(i+vec3(1,0,1),vec3(17.1,71.7,33.4)))*43758.5),f.x),f.z);}
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;world_normal=MODEL_NORMAL_MATRIX*NORMAL;}
void fragment(){float fleck=noise(world_pos*14.0);float hatch=smoothstep(0.84,0.96,fract((world_pos.x+world_pos.z)*31.0+world_pos.y*25.0));vec3 base=base_color.rgb;if(texture_cell>=0){vec3 n=abs(world_normal);vec2 uv=n.z>n.x&&n.z>n.y?world_pos.xy:(n.x>n.y?world_pos.zy:world_pos.xz);vec2 cell=vec2(float(texture_cell%2),float(texture_cell/2));vec2 coord=cell*.5+vec2(.001)+fract(uv*texture_scale)*.498;vec3 ink=texture(ink_atlas,coord).rgb;base=mix(base,ink*(base*.9+vec3(.35)),.85);}ALBEDO=base*(0.94+fleck*surface_grain-hatch*0.055);ROUGHNESS=1.0;}
void light(){float n=dot(NORMAL,LIGHT);float band=n>0.55?1.0:(n>0.05?0.72:0.36);DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
