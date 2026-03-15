shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D lut;

varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
}

void fragment() {
	vec4 texture_color = texture(TEXTURE, UV);
	vec4 lut_color = texture(lut, texture_color.rr);

	COLOR = vec4(lut_color.rgb, lut_color.a * texture_color.a) * vertex_color;
}