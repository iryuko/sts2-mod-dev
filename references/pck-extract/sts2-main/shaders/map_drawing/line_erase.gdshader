// This shader is used on eraser lines drawn by the player. They are placed in a subviewport along
// with the lines drawn by the player.

shader_type canvas_item;

// Use subtractive blending so that this removes lines that are already present in the viewport.
render_mode blend_sub;

void fragment()
{
	// In this shader, we must premultiply alpha into the color. Otherwise, black artifacts will be
	// displayed along the edges of the erased line.
	// Why is this? We have a texture set in the Line2D with alpha falling off at the edges. However,
	// the RGB value along the line is still uniform all along the line, even where alpha is zero.
	// Without this premultiply, when we subtract color along the alpha falloff, the color always
	// goes to black even where we want it to still be colored, just with a lower alpha.
	// With this premultiply, the subtraction takes it to somewhere between black and color, and
	// rendering with premultiplied alpha in the final composite takes care of mixing the color
	// correctly.
	// If we were able to control the blend mode more precisely, inverting the alpha blending of the
	// mix blend mode would likely achieve a similar result.
	COLOR.rgb *= texture(TEXTURE, UV).a;
}