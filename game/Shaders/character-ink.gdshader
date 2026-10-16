shader_type spatial;
render_mode specular_disabled;
varying vec3 local_position;
void vertex(){local_position=VERTEX;}
void fragment(){
 vec3 pigment=COLOR.rgb;
 float weave=step(.89,fract(local_position.y*165.0+local_position.x*115.0));
 float cross=step(.95,fract(local_position.x*103.0-local_position.y*121.0));
 ALBEDO=pigment*(1.0-weave*.08*COLOR.a-cross*.04*COLOR.a);ROUGHNESS=1.0;
}
void light(){float facing=dot(NORMAL,LIGHT);float band=facing>.5?1.0:(facing>.02?.67:.30);DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
