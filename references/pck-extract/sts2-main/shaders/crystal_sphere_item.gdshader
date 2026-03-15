shader_type canvas_item;

uniform float val : hint_range(0,1) = 1;

void fragment() {
    float angle = UV.x - UV.y;

    float val_offset = mix(-1.0f, 1.5f, val);
    vec4 shine_1 = vec4(step(abs(angle - val_offset), 0.17f));
    vec4 shine_2 = vec4(step(abs(angle - val_offset + 0.3f), 0.03f));

    COLOR.rgb += shine_1.rgb * 0.75f;
    COLOR.rgb += shine_2.rgb * 0.5f;
}
