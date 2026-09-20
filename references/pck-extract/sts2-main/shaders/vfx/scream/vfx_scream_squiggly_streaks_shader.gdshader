shader_type canvas_item;
render_mode blend_mix;

uniform float distortion_strength = 0.05;
uniform vec2 distortion_tiling;
uniform vec2 distortion_offset_speed;

varying float rotation_rad;
varying vec4 vertex_color;

void vertex() {
	rotation_rad = INSTANCE_CUSTOM.x;
	vertex_color = COLOR;
}

void fragment()
{
	float distortion_mask = texture(TEXTURE, (UV * distortion_tiling) + (TIME * distortion_offset_speed) + vec2(rotation_rad)).b;
	distortion_mask -= 0.5;
	distortion_mask *= 2.0;
	
	vec4 main_tex = texture(TEXTURE, UV + vec2(distortion_mask * distortion_strength, 0));
	
	vec4 sub_color = vertex_color;
	float alpha = main_tex.a * vertex_color.a;
	
	COLOR = vec4(sub_color.rgb, alpha); 
}