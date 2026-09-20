shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D erosion_curve;
uniform float erosion_offset = 0.1;

varying float lifetime;
varying vec4 vertex_color;

vec2 polar_uv(vec2 base_uv, vec2 center, float radial_scale, float length_scale) {
	vec2 delta = base_uv - center;
	float radius = 2.0 * radial_scale * length(delta);
	float angle = atan(delta.x, delta.y) * 1.0/6.28 * length_scale;
	return vec2(radius, angle);
}

vec2 polar_uv_twist(vec2 polar_uv, float twist) {
	return vec2(
			polar_uv.x,
			(polar_uv.x * twist) + polar_uv.y
			);
}

void vertex() {
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
}

void fragment() {
	vec4 texture_color =  texture(TEXTURE, polar_uv(UV, vec2(0.5, 0.5), 1, 1));
	
	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = smoothstep(erosion_curve_val, erosion_curve_val + erosion_offset, texture_color.r);
	
	COLOR = vec4(vertex_color.rgb, vertex_color.a * erosion_val);
}