shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D lut;

uniform vec2 distortion_panning_speed;
uniform float distortion_strength;

uniform sampler2D erosion_curve;
uniform float erosion_offset = 0.25;

varying float lifetime;
varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
}

void fragment() {
	vec4 distortion_tex = texture(TEXTURE, UV + (TIME * distortion_panning_speed));
	vec4 texture_color =  texture(TEXTURE, UV + (distortion_tex.b * distortion_strength * vec2(0, 1)));
	
	vec4 lut_color = texture(lut, texture_color.rr);

	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = smoothstep(erosion_curve_val, erosion_curve_val + erosion_offset, texture_color.g);

	COLOR = vec4(lut_color.rgb, erosion_val * texture_color.a) * vertex_color;
}