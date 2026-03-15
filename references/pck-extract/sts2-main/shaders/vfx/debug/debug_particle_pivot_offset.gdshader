shader_type canvas_item;
render_mode blend_mix;

uniform vec2 pivot_offset;

void vertex() {
	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
}

void fragment() {

}