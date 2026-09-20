shader_type canvas_item;
render_mode blend_mix;

uniform vec2 pivot_offset = vec2(0.0, 0.0);

uniform sampler2D lut : repeat_disable;

uniform float frame_count = 9;
uniform vec2 flipbook_size = vec2(3,3);
uniform sampler2D flipbook_curve : repeat_disable, hint_default_black;

uniform float use_erosion_texture = 0.0;
uniform sampler2D erosion_texture : repeat_disable;
uniform sampler2D erosion_curve : hint_default_black;
uniform float erosion_offset = 0.1;

uniform float use_hue_shift = 0.0;
uniform sampler2D hue_shift_curve : repeat_disable, hint_default_black;

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

//https://godotshaders.com/shader/hue-shift/
vec4 shift_hue(in vec4 color, in float shift) {
	// The unit gray vector in RGB color space.
	vec3 gray = vec3(0.57735);
	// Project color onto gray axis.
	vec3 projection = gray * dot(gray, color.rgb);
	// Vector from gray axis to original color.
	vec3 U = color.rgb - projection;
	// Vector perpendicular to gray axis and U.
	vec3 V = cross(gray, U);
	// Rotate U and V around the gray axis.
	vec3 shifted = U * cos(shift * 2.0 * PI) + V * sin(shift * 2.0 * PI) + projection;
	return vec4(shifted, color.a);
}

void vertex() {
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
	flipbook_offset = INSTANCE_CUSTOM.z;
	
	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
}

void fragment() {
	float flipbook_progress = texture(flipbook_curve, vec2(lifetime)).r;
	vec2 flipbook_uv = flipbook(UV, flipbook_size, ((frame_count * flipbook_offset) + (frame_count * flipbook_progress)) * 0.99);

	vec4 flipbook_texture_color =  texture(TEXTURE, flipbook_uv);

	vec4 erosion_color = texture(erosion_texture, UV);

	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = smoothstep(erosion_curve_val, erosion_curve_val + erosion_offset, erosion_color.r);

    float erosion = mix(1.0, erosion_val, use_erosion_texture);
	float alpha = flipbook_texture_color.a * vertex_color.a * erosion;

	//if(alpha < 0.001)
		//discard;
		
	vec4 lut_color = texture(lut, flipbook_texture_color.rr);
	vec4 initial_color = lut_color * vertex_color;

	float hue_shift_val = texture(hue_shift_curve, flipbook_texture_color.rr).r;
	vec4 shifted_color = shift_hue(initial_color, hue_shift_val);
	
    vec4 final_color = mix(initial_color, shifted_color, use_hue_shift);

	COLOR = vec4(final_color.rgb, alpha);
}