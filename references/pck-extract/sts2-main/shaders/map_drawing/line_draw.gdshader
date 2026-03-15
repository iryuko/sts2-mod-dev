// This shader is used on player-drawn lines that are placed into a subviewport, then drawn on top
// of the map using the premultiplied alpha blend mode.

shader_type canvas_item;

// Even though the viewport texture is drawn using premultiplied alpha, the lines must mix RGB values
// with the lines that are already drawn into the viewport.
render_mode blend_mix;

void fragment()
{
	// Use alpha from texture. Texture is grayscale so no need to modify RGB
	COLOR.a *= texture(TEXTURE, UV).a;
}