shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx;

varying vec3 hand_position;

void vertex() {
    hand_position = VERTEX;
}

void fragment() {
    vec3 p = hand_position;
    float leather = 1.0 - smoothstep(0.0, 0.08, COLOR.r - COLOR.g);
    vec3 grain_cell = floor(p * 1450.0);
    float grain = fract(sin(dot(grain_cell, vec3(12.9898, 78.233, 37.719))) * 43758.5453);
    float crease = sin(p.x * 390.0 + sin(p.y * 75.0) * 1.6);
    float seam = 1.0 - smoothstep(0.0007, 0.0016, abs(abs(p.y) - 0.028));
    seam *= step(0.023, p.z) * smoothstep(-0.02, 0.018, abs(p.x));
    float stitch = step(0.35, fract(abs(p.x) * 290.0));
    ALBEDO = COLOR.rgb * (0.91 + grain * 0.16 + crease * 0.035);
    ALBEDO = mix(ALBEDO, ALBEDO * (0.65 + 0.6 * stitch), leather * seam);
    ROUGHNESS = mix(0.72, 0.91, leather);
    SPECULAR = 0.22;
}
