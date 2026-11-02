shader_type spatial;
render_mode cull_disabled, specular_disabled;
uniform int world_seed=431;
uniform float spawn_x=1100.0;
uniform float spawn_z=160.0;
uniform sampler2D grass_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D soil_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D asphalt_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D concrete_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D gravel_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D wet_shore_mud_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D shore_pebbles_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D shallow_silt_map : source_color, filter_linear_mipmap, repeat_enable;
varying vec3 world_pos;
float hash(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
float noise(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.0-2.0*f);return mix(mix(hash(i),hash(i+vec2(1,0)),f.x),mix(hash(i+vec2(0,1)),hash(i+vec2(1)),f.x),f.y);}
void vertex(){world_pos=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;}
void fragment(){
 vec2 p=world_pos.xz;int kind=int(round(COLOR.a*8.0));
 // Surface codes follow the shared atlas; never overpaint the old road route.
 if(kind==8){kind=COLOR.b>COLOR.r*1.1&&COLOR.b>COLOR.g*.95?6:(COLOR.g>COLOR.r*1.15?0:(COLOR.r>COLOR.g*1.15?1:(COLOR.r>.3?3:2)));}
 float broad=noise(p*.055+float(world_seed%97));
 vec3 base;
 if(kind==0){base=texture(grass_map,p/4.0).rgb;}
 else if(kind==1){base=mix(texture(soil_map,p/3.0).rgb,texture(gravel_map,p/3.5).rgb,.38);}
 else if(kind==2){base=texture(asphalt_map,p/6.0).rgb;}
 else if(kind==3){base=texture(concrete_map,p/3.0).rgb;}
 else if(kind==4){base=mix(texture(soil_map,p/3.0).rgb,texture(grass_map,p/4.0).rgb,.17);}
 else if(kind==5){base=texture(gravel_map,p/3.5).rgb;}
 else if(kind==6){base=texture(shallow_silt_map,p/4.0).rgb*vec3(.48,.63,.59);}
 else{float dry=smoothstep(3.3,8.0,world_pos.y);base=mix(texture(wet_shore_mud_map,p/3.0).rgb,texture(shore_pebbles_map,p/3.0).rgb,dry*.75);}
 ALBEDO=base*(.92+broad*.16);ROUGHNESS=kind==7?.62:1.0;
}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.5?1.0:n>.08?.78:.48;DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
