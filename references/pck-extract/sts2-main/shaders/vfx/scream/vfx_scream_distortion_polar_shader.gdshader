shader_type canvas_item;
render_mode blend_mix;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;

uniform float distortion_base_intensity = 0.05;
uniform vec2 tiling = vec2(1.0, 1.0);
uniform vec2 offset_speed;

varying vec4 vertex_color;

vec2 get_polar_uv(vec2 base_uv) {
	base_uv -= 0.5;
	base_uv *= 2.0;

	float angle = atan(base_uv.y, base_uv.x) / (2.0 * PI);
	float dist = length(vec2(
			base_uv.x,
			base_uv.y
			));

	vec2 polar_uv = vec2(
			angle,
			dist
			);

	return polar_uv;
}

void vertex() {
	vertex_color = COLOR;
}

void fragment() {	
	vec2 direction = normalize(vec2(0.5, 0.5) - UV);
	vec2 polar_uv = get_polar_uv(UV);
	polar_uv *= tiling;
	polar_uv += (TIME * offset_speed);
	
	float distortion_mask = texture(TEXTURE, UV).a;
	float distortion_polar_mask = texture(TEXTURE, polar_uv).r;	
	float distortion_intensity = vertex_color.a;
	
	vec4 screen_tex = textureLod(
		screen_texture, 
		SCREEN_UV + (distortion_base_intensity * distortion_intensity * distortion_mask * distortion_polar_mask * direction), 
		0.0
		);
		
	COLOR = screen_tex;
}
