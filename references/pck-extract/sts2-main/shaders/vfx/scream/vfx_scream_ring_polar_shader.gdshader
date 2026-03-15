shader_type canvas_item;
render_mode blend_mix;

uniform vec2 tiling;
uniform vec2 x_offsets;

uniform sampler2D offset_curve : repeat_disable;

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
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
}

void fragment() {
	float interpolation = texture(offset_curve, vec2(lifetime)).r;

	vec2 polar_uv = get_polar_uv(UV);
	polar_uv *= tiling;
	polar_uv += vec2(0.0, mix(x_offsets.x, x_offsets.y, interpolation));

	vec4 main_tex = texture(TEXTURE, UV);
	vec4 main_tex_polar = texture(TEXTURE, polar_uv);

	vec3 sub_color = vertex_color.rgb;
	float alpha = vertex_color.a * main_tex_polar.r * main_tex.a;

	COLOR = vec4(sub_color, alpha);
}