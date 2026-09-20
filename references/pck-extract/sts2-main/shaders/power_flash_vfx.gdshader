shader_type canvas_item;
render_mode blend_add;

uniform float strength = 3.0;

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    vec4 ModularCol = COLOR / col;

	vec4 pixelCol = texture(TEXTURE, UV);
	pixelCol.rgb *= strength;
	COLOR *= pixelCol;
}