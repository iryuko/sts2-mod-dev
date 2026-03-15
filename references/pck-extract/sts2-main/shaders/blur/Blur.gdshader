// This is the shader used on NCard portrait & ancient portrait when it is blurred.
// It is intended to be used as the first pass (X or Y) of a Gaussian blur, where the first pass
// is done by a canvas group.

shader_type canvas_item;

// Radius that the shader was designed for.
const float DEFAULT_RADIUS = 32.000000000;

// Unit vector: (1, 0) or (0, 1)
uniform vec2 step;
// Desired blur radius.
uniform float radius = 32.000000000;

void fragment() {
	vec2 s = radius / DEFAULT_RADIUS * step / vec2(textureSize(TEXTURE, 0));
	COLOR.rgb =
		0.003523096 * texture(TEXTURE, UV - 32.000000000 * s).rgb +
		0.008472943 * texture(TEXTURE, UV - 30.470250026 * s).rgb +
		0.010666648 * texture(TEXTURE, UV - 28.472196679 * s).rgb +
		0.013220332 * texture(TEXTURE, UV - 26.474144178 * s).rgb +
		0.016131604 * texture(TEXTURE, UV - 24.476092464 * s).rgb +
		0.019379095 * texture(TEXTURE, UV - 22.478041477 * s).rgb +
		0.022919767 * texture(TEXTURE, UV - 20.479991160 * s).rgb +
		0.026687486 * texture(TEXTURE, UV - 18.481941452 * s).rgb +
		0.030593271 * texture(TEXTURE, UV - 16.483892295 * s).rgb +
		0.034527483 * texture(TEXTURE, UV - 14.485843628 * s).rgb +
		0.038364071 * texture(TEXTURE, UV - 12.487795393 * s).rgb +
		0.041966740 * texture(TEXTURE, UV - 10.489747531 * s).rgb +
		0.045196682 * texture(TEXTURE, UV - 8.491699981 * s).rgb +
		0.047921307 * texture(TEXTURE, UV - 6.493652685 * s).rgb +
		0.050023206 * texture(TEXTURE, UV - 4.495605582 * s).rgb +
		0.051408527 * texture(TEXTURE, UV - 2.497558613 * s).rgb +
		0.052013919 * texture(TEXTURE, UV - 0.499511719 * s).rgb +
		0.051811334 * texture(TEXTURE, UV + 1.498535160 * s).rgb +
		0.050810181 * texture(TEXTURE, UV + 3.496582084 * s).rgb +
		0.049056605 * texture(TEXTURE, UV + 5.494629113 * s).rgb +
		0.046629956 * texture(TEXTURE, UV + 7.492676305 * s).rgb +
		0.043636841 * texture(TEXTURE, UV + 9.490723721 * s).rgb +
		0.040203362 * texture(TEXTURE, UV + 11.488771419 * s).rgb +
		0.036466342 * texture(TEXTURE, UV + 13.486819461 * s).rgb +
		0.032564380 * texture(TEXTURE, UV + 15.484867904 * s).rgb +
		0.028629528 * texture(TEXTURE, UV + 17.482916808 * s).rgb +
		0.024780288 * texture(TEXTURE, UV + 19.480966233 * s).rgb +
		0.021116370 * texture(TEXTURE, UV + 21.479016239 * s).rgb +
		0.017715480 * texture(TEXTURE, UV + 23.477066883 * s).rgb +
		0.014632124 * texture(TEXTURE, UV + 25.475118226 * s).rgb +
		0.011898236 * texture(TEXTURE, UV + 27.473170326 * s).rgb +
		0.009525297 * texture(TEXTURE, UV + 29.471223243 * s).rgb +
		0.007507498 * texture(TEXTURE, UV + 31.469277035 * s).rgb;
	COLOR.a = 1.0;
}
