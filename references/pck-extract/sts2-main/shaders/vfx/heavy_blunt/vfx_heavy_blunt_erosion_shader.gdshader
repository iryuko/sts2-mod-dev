shader_type canvas_item;
render_mode blend_mix;

uniform vec2 pivot_offset;

uniform sampler2D erosion_curve;
uniform float erosion_offset = 0.1;

uniform sampler2D lut;

varying float lifetime;
varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
	lifetime = INSTANCE_CUSTOM.y/INSTANCE_CUSTOM.w;
	
	VERTEX += pivot_offset * (1.0/TEXTURE_PIXEL_SIZE);
}

void fragment() {
	vec4 texture_color =  texture(TEXTURE, UV);
	
	float erosion_curve_val = texture(erosion_curve, vec2(lifetime)).r;
	float erosion_val = smoothstep(erosion_curve_val, erosion_curve_val + erosion_offset, texture_color.r);
	
	vec4 lut_tex = texture(lut, texture_color.rr);
	vec4 color_sub = lut_tex * vertex_color;
	
	COLOR = vec4(color_sub.rgb, vertex_color.a * erosion_val);
}