shader_type spatial;
render_mode cull_disabled,specular_disabled;
uniform vec3 leaf_color : source_color = vec3(.19,.29,.13);
void fragment(){if(!FRONT_FACING)NORMAL=-NORMAL;ALBEDO=leaf_color*COLOR.rgb;ROUGHNESS=1.0;BACKLIGHT=vec3(.12,.17,.07);}
