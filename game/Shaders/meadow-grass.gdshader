shader_type spatial;
render_mode cull_disabled,specular_disabled;
varying float blade_height;
void vertex(){blade_height=VERTEX.y;vec3 world=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;VERTEX.x+=sin(TIME*1.25+world.x*.4+world.z*.3)*.035*VERTEX.y;}
void fragment(){if(!FRONT_FACING)NORMAL=-NORMAL;ALBEDO=mix(vec3(.19,.24,.12),vec3(.40,.41,.24),clamp(blade_height*2.4,0.0,1.0))*COLOR.rgb;ROUGHNESS=1.0;BACKLIGHT=vec3(.15,.19,.08);}
