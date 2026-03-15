shader_type canvas_item;

uniform float speed;
uniform float strength;
uniform sampler2D flame_tex : repeat_enable;
uniform sampler2D noise_tex : repeat_enable;
uniform vec4 color : source_color;

void fragment() {
    float tex_col = texture(flame_tex, UV).b;
	vec2 noise = texture(noise_tex, UV + vec2(0, TIME * speed)).rg * tex_col * strength;

    vec2 distortion_uv = mix(UV, vec2(0.5, 1), noise);
    vec4 final_col = texture(flame_tex, distortion_uv);
    COLOR.rgb = color.rgb;
    COLOR.a = 1.0 - step(final_col.x, 0.3
    );
}
