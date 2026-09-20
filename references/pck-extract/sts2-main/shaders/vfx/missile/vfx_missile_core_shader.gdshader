shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D lut;

uniform float flipbook_speed = 1;
uniform float frame_count = 9;
uniform vec2 flipbook_size = vec2(3,3);

varying float lifetime;
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
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
}

void fragment() {
	vec2 flipbook_uv = flipbook(UV, flipbook_size, mod(TIME * flipbook_speed, frame_count));
	vec4 texture_color =  texture(TEXTURE, flipbook_uv);

	vec4 lut_color = texture(lut, texture_color.rr);

	vec4 final_color = lut_color * vertex_color;
	float alpha = texture_color.a * vertex_color.a;

	COLOR = vec4(final_color.rgb, alpha);
}