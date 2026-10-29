shader_type spatial;
render_mode cull_disabled, specular_disabled;
varying float height;
void vertex(){height=VERTEX.y;vec3 w=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;VERTEX.x+=sin(TIME*.85+w.x*.3+w.z*.2)*.025*VERTEX.y;}
void fragment(){if(!FRONT_FACING)NORMAL=-NORMAL;ALBEDO=COLOR.rgb*mix(.67,1.0,clamp(height*2.3,0.0,1.0));ROUGHNESS=1.0;}
void light(){float n=dot(NORMAL,LIGHT);float band=n>.5?1.0:(n>.08?.78:.48);DIFFUSE_LIGHT+=LIGHT_COLOR*band*ATTENUATION/3.14159265;}
