shader_type spatial;
render_mode cull_disabled;
uniform sampler2D water_map : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D ripple_map : filter_linear_mipmap, repeat_enable;
uniform sampler2D detail_map : filter_linear_mipmap, repeat_enable;
uniform sampler2D foam_map : filter_linear_mipmap, repeat_enable;
uniform sampler2D silt_map : source_color, filter_linear_mipmap, repeat_enable;
varying vec3 world_point;
void vertex(){world_point=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;}
void fragment(){
 vec2 p=world_point.xz;
 vec2 drift=TIME*vec2(.012,.008);
 vec3 a=texture(ripple_map,p/7.0+drift).rgb*2.0-1.0;
 vec3 b=texture(detail_map,p/2.7-drift*.63).rgb*2.0-1.0;
 vec2 ripple=a.xy*1.2+b.xy*.8+vec2(sin(p.x*.7+TIME*.8)*.035,cos(p.y*.6+TIME*.6)*.03);
 NORMAL=normalize(mat3(VIEW_MATRIX)*vec3(ripple.x,1.0,ripple.y));
 vec3 water=texture(water_map,p/24.0+ripple*.04).rgb;
 float shallow=COLOR.g;
 vec3 bed=texture(silt_map,p/4.0).rgb;
 water=mix(water,bed*vec3(.38,.60,.57),shallow*.42);
 float foam=texture(foam_map,p/3.5+drift*.2+ripple*.025).r;
 foam=smoothstep(.42,.8,foam)*COLOR.r*.40;
 ALBEDO=mix(water,vec3(.56,.65,.60),foam);
 ROUGHNESS=mix(.26,.75,foam);METALLIC=0.0;SPECULAR=.45;
}
