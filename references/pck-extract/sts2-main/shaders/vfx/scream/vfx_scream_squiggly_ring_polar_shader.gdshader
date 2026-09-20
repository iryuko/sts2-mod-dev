shader_type canvas_item;
render_mode blend_mix;

uniform vec2 tiling;
uniform vec2 x_offsets;

uniform sampler2D offset_curve : repeat_disable;

uniform float erosion_offset;
uniform sampler2D erosion_curve;

uniform float distortion_strength = 0.05;
uniform vec2 distortion_tiling;
uniform vec2 distortion_offset_speed;

varying float rotation_rad;
varying vec4 vertex_color;
varying float lifetime;

vec2 get_polar_uv(vec2 base_uv) {
	base_uv -= 0.5;
	base_uv *= 2.0;

	float angle = atan(base_uv.y, base_uv.x) / (2.0 * PI);
	float dist = length(vec2(
			base_uv.x,
			base_uv.y
			));

	vec2 polar_uv = vec2(
			angle,
			dist
			);

	return polar_uv;
}

void vertex() {
	rotation_rad = INSTANCE_CUSTOM.x;
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
}

void fragment() {
	float interpolation = texture(offset_curve, vec2(lifetime)).r;

	vec2 distortion_polar_uv = get_polar_uv(UV);
	distortion_polar_uv *= distortion_tiling;
	distortion_polar_uv += (TIME * distortion_offset_speed) + vec2(rotation_rad);

	vec2 direction = normalize(vec2(0.5, 0.5) - UV);
	vec4 distortion_mask = texture(TEXTURE, distortion_polar_uv);

	vec2 polar_uv = get_polar_uv(UV + (direction * distortion_mask.g * distortion_mask.b * distortion_strength));
	polar_uv *= tiling;
	polar_uv += vec2(0.0, mix(x_offsets.x, x_offsets.y, interpolation));

	vec4 main_tex = texture(TEXTURE, UV);
	vec4 main_tex_polar = texture(TEXTURE, polar_uv);

	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = smoothstep(erosion_curve_val, erosion_curve_val + erosion_offset, main_tex_polar.r);

	vec3 sub_color = vertex_color.rgb;
	float alpha = vertex_color.a * main_tex.a * erosion_val;

	//COLOR = vec4(distortion_mask);
	COLOR = vec4(sub_color, alpha);
}