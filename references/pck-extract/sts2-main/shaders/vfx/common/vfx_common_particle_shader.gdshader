shader_type canvas_item;
render_mode blend_mix;

uniform float frame_count = 4;
uniform vec2 flipbook_size = vec2(2,2);

uniform sampler2D lut;

uniform vec2 pivot_offset = vec2(0.0, 0.0);

uniform sampler2D erosion_curve;
uniform float erosion_offset = 0.25;

varying float lifetime;
varying float flipbook_offset;
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
	flipbook_offset = INSTANCE_CUSTOM.z;
	
	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
}

void fragment() {
	vec2 flipbook_uv = flipbook(UV, flipbook_size, frame_count * flipbook_offset * 0.99);
	
	vec4 texture_color =  texture(TEXTURE, flipbook_uv);

	vec4 lut_color = texture(lut, texture_color.rr);

	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = smoothstep(erosion_curve_val, erosion_curve_val + erosion_offset, texture_color.a);

	COLOR = vec4(lut_color.rgb, erosion_val) * vertex_color;
}