shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D noise : repeat_enable;
uniform vec4 noise_st_a = vec4(1.0, 1.0, 0, 0);
uniform vec2 noise_a_static_offset = vec2(0, 0);
uniform vec4 noise_st_b = vec4(1.0, 1.0, 0, 0);
uniform vec2 noise_b_static_offset = vec2(0, 0);
uniform float noise_layer_lerp : hint_range(0.0, 1.0, 0.1);
uniform float noise_panning_multiplier = 1.0;
uniform float noise_alpha_multiplier = 1.0;

uniform sampler2D distortion : repeat_enable, hint_default_black;
uniform vec4 distortion_st = vec4(1.0, 1.0, 0, 0);
uniform float distortion_intensity = 0.1;

uniform float min_base_alpha = 0.1;
uniform float alpha_multiplier = 1.0;

uniform float erosion_base = 0.25;
uniform float erosion_offset = 0.5;

uniform float bloat_intensity = 0.0;
uniform vec2 bloat_center = vec2(0.5, 0.5);

varying vec4 vertex_color;

vec2 bloat(vec2 uv, vec2 center, float intensity) {
	vec2 delta = uv - center;
	float delta_dot = dot(delta.xy, delta.xy);
	float delta_dot_sqr = delta_dot * delta_dot;
	float delta_offset = delta_dot_sqr * intensity;
	
	return uv + delta * delta_offset;
}

vec2 rotate(vec2 uv, vec2 pivot, float angle) {
	mat2 rotation = mat2(vec2(sin(angle), -cos(angle)),
						vec2(cos(angle), sin(angle)));
	
	uv -= pivot;
	uv = uv * rotation;
	uv += pivot;
	return uv;
}

vec2 panning_uv(vec2 base_uv, vec4 st, vec2 extra_tiling, vec2 static_offset) {
	return (base_uv * (st.xy * extra_tiling)) + ((st.zw * TIME * noise_panning_multiplier) + static_offset);
}

void vertex() {
	vertex_color = COLOR;
}

void fragment() {
	vec2 bloated_uv = bloat(UV, bloat_center, bloat_intensity);
	vec2 rotated_uv = rotate(bloated_uv, vec2(0.5, 0.5), 0);
	
	vec2 distortion_uv = panning_uv(rotated_uv, distortion_st, vec2(1.0), vec2(0.0));
	float distortion_val = texture(distortion, distortion_uv).r;
	distortion_val = (distortion_val - 0.5) * 2.0;
	distortion_val *= distortion_intensity;
	
	vec2 noise_uv_a = rotated_uv;
	noise_uv_a = panning_uv(noise_uv_a, noise_st_a, vec2(1.0), noise_a_static_offset + vec2(0.0, distortion_val));
	
	vec2 noise_uv_b = rotated_uv;
	noise_uv_b = panning_uv(noise_uv_b, noise_st_b, vec2(1.0), noise_b_static_offset + vec2(0.0, distortion_val));
	
	vec4 noise_a_tex = texture(noise, noise_uv_a);
	vec4 noise_b_tex = texture(noise, noise_uv_b);
	
	float combined_noise = mix(noise_a_tex, noise_b_tex, noise_layer_lerp).r;
	combined_noise = smoothstep(erosion_base, erosion_base + erosion_offset, combined_noise);
	combined_noise *= noise_alpha_multiplier;
	
	float sub_alpha = vertex_color.a * combined_noise;
	sub_alpha = max(sub_alpha, min_base_alpha);
	
	COLOR = vec4(vertex_color.rgb, sub_alpha * alpha_multiplier);
}