shader_type spatial;
render_mode unshaded,cull_front;
uniform float ink_width=.002;
void vertex(){VERTEX+=NORMAL*ink_width;}
void fragment(){ALBEDO=vec3(.016,.018,.018);}
