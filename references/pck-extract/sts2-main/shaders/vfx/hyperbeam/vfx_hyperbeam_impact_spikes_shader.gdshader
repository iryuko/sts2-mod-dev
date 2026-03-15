shader_type canvas_item;
render_mode blend_mix;

uniform vec2 pivot_offset;

uniform vec2 texture_tiling = vec2(1.0, 1.0);
uniform float initial_offset = 0.0;
uniform sampler2D offset_curve;

uniform sampler2D mask_texture;

varying float lifetime;
varying vec4 vertex_color;
varying float flipbook_offset;

void vertex() {
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
	flipbook_offset = INSTANCE_CUSTOM.z;
	
	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
}

void fragment() {
	float offset = texture(offset_curve, vec2(lifetime)).r;
	vec4 tex = texture(TEXTURE, UV * texture_tiling + vec2(flipbook_offset, initial_offset + offset));
	float mask = texture(mask_texture, UV).r;
	
	COLOR = vec4(vertex_color.rgb, smoothstep(0.35, 0.65, vertex_color.a * tex.r * mask));
}