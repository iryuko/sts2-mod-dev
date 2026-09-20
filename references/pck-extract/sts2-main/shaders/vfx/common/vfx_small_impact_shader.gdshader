shader_type canvas_item;
render_mode blend_mix;

uniform float frame_count = 7;
uniform vec2 flipbook_size = vec2(3,3);

uniform sampler2D lut;

varying float rotation_rad;
varying vec4 vertex_color;

vec2 flipbook(vec2 uv, vec2 size, float progress) {
	progress = floor( mod(progress, (size.x * size.y)) );
	vec2 frame_size = vec2(1.0, 1.0) / vec2(size.x, size.y);
	vec2 frame = fract(uv / size) + frame_size;

	frame.x += ( (progress / size.x) - frame_size.x * floor(progress/size.x) * size.x ) - frame_size.x;
	frame.y += (frame_size.y * floor(progress/size.x) ) - frame_size.y ;

	return frame;
}

void vertex() {
	vertex_color = COLOR;
	rotation_rad = INSTANCE_CUSTOM.x;
}

void fragment() {
	vec2 flipbook_uv = flipbook(UV, flipbook_size, rotation_rad);
	vec4 texture_color =  texture(TEXTURE, flipbook_uv);
	
	vec4 lut_tex = texture(lut, vec2(texture_color.r));
	vec4 color_sub = lut_tex * vertex_color;
	
	COLOR = vec4(color_sub.rgb, vertex_color.a * texture_color.r);
}