shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D overlay_texture : repeat_enable;
uniform float overlay_influence;

uniform vec2 distortion_panning_speed;
uniform float distortion_strength;

uniform vec2 overlay_panning_speed;

uniform vec4 tint : source_color = vec4(1.0);
uniform sampler2D lut;

uniform float h: hint_range(0,1) = 1;
uniform float s: hint_range(0,5) = 1;
uniform float v = 1;

vec4 get_hue_shifted_color(vec4 col, vec4 base_color) {
    mat3 RGB_to_YIQ = mat3(
        vec3(0.2989,  0.5959,  0.2115),
        vec3(0.5870, -0.2774, -0.5229),
        vec3(0.1140, -0.3216,  0.3114));

    vec4 modular_col = base_color / col;

    col.rgb = RGB_to_YIQ * col.rgb;
    float hue = 1.0 - h;
    hue = mix(0, 6.283185, hue);

    float sin_hue = sin(hue);
    float cos_hue = cos(hue);
    mat3 hue_shift = mat3(
        vec3(1.0, 0, 0),
        vec3(0, cos_hue, -sin_hue),
        vec3(0, sin_hue, cos_hue));
    col.rgb *= hue_shift;

    mat3 sat_shift = mat3(
        vec3(1.0, 0, 0),
        vec3(0, s, 0),
        vec3(0, 0, s));
    col.rgb = sat_shift * col.rgb;

    col.rgb = mix(vec3(0,0,0), col.rgb, v);

    col.rgb = inverse(RGB_to_YIQ) * col.rgb;
    return col * modular_col;
}

void fragment() {
	vec4 distortion_tex = texture(overlay_texture, (SCREEN_UV * 10.0) + (TIME * distortion_panning_speed));
	vec4 overlay_color =  texture(overlay_texture, (SCREEN_UV * 10.0) + (TIME * overlay_panning_speed) + (distortion_tex.b * distortion_strength));

	vec4 lut_tex = texture(lut, vec2(overlay_color.r)) * tint;
	vec4 base_tex = texture(TEXTURE, UV);
	vec4 hue_shifted_color = get_hue_shifted_color(base_tex, COLOR);

	float nontransparent_base_color = step(0.5, hue_shifted_color.a);
	COLOR = vec4(mix(hue_shifted_color, lut_tex, overlay_color.a * nontransparent_base_color * overlay_influence).rgb, hue_shifted_color.a);
}