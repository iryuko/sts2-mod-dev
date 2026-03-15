shader_type canvas_item;
render_mode blend_mix;

uniform float frame_count = 4;
uniform vec2 flipbook_size = vec2(2,2);

vec2 flipbook(vec2 uv, vec2 size, float progress) {
	progress = floor( mod(progress, (size.x * size.y)) );
	vec2 frame_size = vec2(1.0, 1.0) / vec2(size.x, size.y);
	vec2 frame = fract(uv / size) + frame_size;

	frame.x += ( (progress / size.x) - frame_size.x * floor(progress/size.x) * size.x ) - frame_size.x;
	frame.y += (frame_size.y * floor(progress/size.x) ) - frame_size.y ;

	return frame;
}

void vertex() {
	vec2 flipbook_uv = flipbook(UV, flipbook_size, frame_count * INSTANCE_CUSTOM.z * 0.99);
	UV = flipbook_uv;
}

void fragment() {
	vec4 texture_color =  texture(TEXTURE, UV);
	COLOR = texture_color.rrrr;
}
