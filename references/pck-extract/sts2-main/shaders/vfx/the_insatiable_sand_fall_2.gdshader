shader_type canvas_item;
//makes noise texture
uniform sampler2D noise : repeat_enable;
uniform sampler2D mask;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float speed : hint_range(0.002,1) = 0.1;
uniform float strength : hint_range(0.002,0.5) = 0.1;
uniform float angle_degrees : hint_range(0, 360) = 45.0;

void fragment() {
	float angle_rad = radians(angle_degrees);
	vec2 direction = vec2(cos(angle_rad), sin(angle_rad));
	float noise_value = texture(noise,UV+-TIME*speed * direction).r;
	COLOR = texture(SCREEN_TEXTURE,SCREEN_UV - (strength/2.0) + vec2(noise_value) * strength);
	
	COLOR.a *= texture(mask,UV).a;;
	
}
