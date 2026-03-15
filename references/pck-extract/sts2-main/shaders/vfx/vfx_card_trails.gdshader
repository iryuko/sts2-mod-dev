shader_type canvas_item;

uniform sampler2D noise_tex : repeat_enable;

void fragment() {
	COLOR.rgb += 0.2 * (1.0 - UV.x);
    COLOR.rgb += 0.15 * (1.0 - abs(0.5 - UV.y));
    float noise = texture(noise_tex, SCREEN_UV).r;
    noise += 0.8 * (1.0 - UV.x);
    //noise += 1.0 * (1.0 - abs(0.5 - UV.y));
    noise = smoothstep(0.7, 0.8, noise);
    COLOR.a *= noise;
}
