shader_type canvas_item;
render_mode blend_mix;

uniform float skew = 1.0;

varying vec2 normalized_pos;

void vertex() {
	normalized_pos = VERTEX * TEXTURE_PIXEL_SIZE;
	normalized_pos += 1.0;
	normalized_pos *= 0.5;
	VERTEX.x = VERTEX.x * mix(1.0 / skew, 1.0 * skew, normalized_pos.y);
}

void fragment() {
	//COLOR = vec4(normalized_pos.y,0.0,0.0, 1.0);
}