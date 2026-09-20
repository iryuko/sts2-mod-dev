// Be careful with this shader! Most of the time you can use vfx_water_reflection instead directly
// on the texture you want to distort. Only use this if the scene you are attempting to distort
// is complex (like a spine shader).

shader_type canvas_item;
//makes noise texture
uniform sampler2D noise : repeat_enable;
uniform sampler2D mask;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float speed : hint_range(0.002,0.5) = 0.1;
uniform float strength : hint_range(0.002,0.5) = 0.1;

void fragment() {
	//sample noise texture
	float noise_value = texture(noise,UV+-TIME*speed).r;
	COLOR = texture(SCREEN_TEXTURE,SCREEN_UV - (strength/2.0) + vec2(noise_value) * strength);

	COLOR.a *= texture(mask,UV).a;;

}
