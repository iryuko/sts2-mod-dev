shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float s: hint_range(0,5) = 1;
uniform float v = 1;
uniform vec4 gradient_color : source_color;


void fragment(){
    mat3 RGB_to_YIQ = mat3(
        vec3(0.2989,  0.5959,  0.2115),
        vec3(0.5870, -0.2774, -0.5229),
        vec3(0.1140, -0.3216,  0.3114));
        
	vec4 color = texture(SCREEN_TEXTURE, SCREEN_UV);
    color.rgb = RGB_to_YIQ * color.rgb;

    mat3 sat_shift = mat3(
        vec3(1.0, 0, 0),
        vec3(0, mix(1, s, COLOR.a), 0),
        vec3(0, 0, mix(1, s, COLOR.a)));
    color.rgb = sat_shift * color.rgb;
    color.rgb = mix(vec3(0,0,0), color.rgb, mix(1, v, COLOR.a));

    color.rgb = inverse(RGB_to_YIQ) * color.rgb;
    float y = pow(SCREEN_UV.g, 6.0f) * 0.7f * COLOR.a * smoothstep(-10.0f, 1.0f, sin(TIME * 20.0f) + sin(TIME * 10.0f));
    
    COLOR = color + y * gradient_color;
}