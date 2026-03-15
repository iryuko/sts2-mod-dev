shader_type canvas_item;
render_mode blend_mix;

uniform vec2 pivot_offset = vec2(0.0, 0.0);

uniform sampler2D lut;
uniform sampler2D vertical_alpha;

varying vec4 vertex_color;
varying float flipbook_offset;

void vertex() {
	vertex_color = COLOR;
	flipbook_offset = INSTANCE_CUSTOM.z;

	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
	VERTEX.x *= flipbook_offset;
}

void fragment() {
	vec4 grayscale = texture(TEXTURE, UV);

	vec4 lut_tex = texture(lut, vec2(grayscale.r));
	vec4 color_sub = lut_tex * vertex_color;

	vec4 vertical_alpha_tex = texture(vertical_alpha, UV.yx);

	COLOR = vec4(color_sub.rgb, vertex_color.a * grayscale.a * vertical_alpha_tex.r);
}