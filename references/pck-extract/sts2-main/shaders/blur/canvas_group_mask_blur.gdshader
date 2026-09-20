// This is the shader used on NCard PortraitCanvasGroup when it is blurred.
// It is intended to be used as the second pass (X or Y) of a Gaussian blur.
// It supports masking the CanvasGroup with the rounded ancient card border.

shader_type canvas_item;

// This should be set to the max size of the contents of the canvas group.
const vec2 size = vec2(598, 842);

// Radius that the shader was designed for.
const float DEFAULT_RADIUS = 32.000000000;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform sampler2D mask_texture : repeat_disable;
uniform vec4 mask_region;
uniform vec2 mask_offset;

// Unit vector: (1, 0) or (0, 1)
uniform vec2 step;
// Desired blur radius.
uniform float radius = 32.000000000;

uniform bool mask;
uniform bool blur;

varying float rotation;

// Samples the backbuffer at the correct position for the canvas group. Taken from the Godot docs:
// https://docs.godotengine.org/en/stable/classes/class_canvasgroup.html
vec4 sample(vec2 uv)
{
	vec4 col = textureLod(screen_texture, uv, 0.0);

	if (col.a > 0.0001) {
		col.rgb /= col.a;
	}
	
	return col;
}
// When the CanvasGroup is rotated, the renderer enlarges for some reason (seems like no reason...)
// https://forum.godotengine.org/t/canvasgroup-shader-uv-is-too-large-with-rotated-node/124571/12
// This method returns the corrected UVs that tightly enclose the rotated group.
vec2 shrink_rotated_canvas_group_uv(vec2 uv) {
	float pixel_total_width = size.x + sin(2.0 * abs(rotation)) * size.y;
	float pixel_total_height = size.y + sin(2.0 * abs(rotation)) * size.x;
	
	// Turn pixel values to shader values
	float shader_total_width = pixel_total_width / size.x;
	float shader_total_height = pixel_total_height / size.y;
	
	float shader_offset_x = (shader_total_width - 1.0) / 2.0;
	float shader_offset_y = (shader_total_height - 1.0) / 2.0;
	
	return uv * vec2(shader_total_width, shader_total_height) - vec2(shader_offset_x, shader_offset_y);
}

void vertex() {
	vec4 rot_checker = MODEL_MATRIX * vec4(1, 0, 0, 0);
	rotation = atan(rot_checker.y, rot_checker.x);
}

void fragment() {
	if (blur)
	{
		vec2 s = radius / DEFAULT_RADIUS * step / vec2(textureSize(screen_texture, 0));
		COLOR.rgb =
			0.003523096 * sample(SCREEN_UV - 32.000000000 * s).rgb +
			0.008472943 * sample(SCREEN_UV - 30.470250026 * s).rgb +
			0.010666648 * sample(SCREEN_UV - 28.472196679 * s).rgb +
			0.013220332 * sample(SCREEN_UV - 26.474144178 * s).rgb +
			0.016131604 * sample(SCREEN_UV - 24.476092464 * s).rgb +
			0.019379095 * sample(SCREEN_UV - 22.478041477 * s).rgb +
			0.022919767 * sample(SCREEN_UV - 20.479991160 * s).rgb +
			0.026687486 * sample(SCREEN_UV - 18.481941452 * s).rgb +
			0.030593271 * sample(SCREEN_UV - 16.483892295 * s).rgb +
			0.034527483 * sample(SCREEN_UV - 14.485843628 * s).rgb +
			0.038364071 * sample(SCREEN_UV - 12.487795393 * s).rgb +
			0.041966740 * sample(SCREEN_UV - 10.489747531 * s).rgb +
			0.045196682 * sample(SCREEN_UV - 8.491699981 * s).rgb +
			0.047921307 * sample(SCREEN_UV - 6.493652685 * s).rgb +
			0.050023206 * sample(SCREEN_UV - 4.495605582 * s).rgb +
			0.051408527 * sample(SCREEN_UV - 2.497558613 * s).rgb +
			0.052013919 * sample(SCREEN_UV - 0.499511719 * s).rgb +
			0.051811334 * sample(SCREEN_UV + 1.498535160 * s).rgb +
			0.050810181 * sample(SCREEN_UV + 3.496582084 * s).rgb +
			0.049056605 * sample(SCREEN_UV + 5.494629113 * s).rgb +
			0.046629956 * sample(SCREEN_UV + 7.492676305 * s).rgb +
			0.043636841 * sample(SCREEN_UV + 9.490723721 * s).rgb +
			0.040203362 * sample(SCREEN_UV + 11.488771419 * s).rgb +
			0.036466342 * sample(SCREEN_UV + 13.486819461 * s).rgb +
			0.032564380 * sample(SCREEN_UV + 15.484867904 * s).rgb +
			0.028629528 * sample(SCREEN_UV + 17.482916808 * s).rgb +
			0.024780288 * sample(SCREEN_UV + 19.480966233 * s).rgb +
			0.021116370 * sample(SCREEN_UV + 21.479016239 * s).rgb +
			0.017715480 * sample(SCREEN_UV + 23.477066883 * s).rgb +
			0.014632124 * sample(SCREEN_UV + 25.475118226 * s).rgb +
			0.011898236 * sample(SCREEN_UV + 27.473170326 * s).rgb +
			0.009525297 * sample(SCREEN_UV + 29.471223243 * s).rgb +
			0.007507498 * sample(SCREEN_UV + 31.469277035 * s).rgb;
	}
	else
	{
		COLOR.rgb = sample(SCREEN_UV).rgb;
	}
	
	if (mask)
	{
		vec2 maskSize = vec2(textureSize(mask_texture, 0));
		vec2 texSize = vec2(textureSize(TEXTURE, 0));
		vec4 screenSample = sample(SCREEN_UV);
		
		vec2 adjustedUv = shrink_rotated_canvas_group_uv(UV);
		adjustedUv = clamp(adjustedUv, 0, 1);
		
		vec2 maskOffset = mask_offset / maskSize;
		vec2 maskRegionSizeUv = mask_region.zw / maskSize + maskOffset;
		vec2 maskRegionUv = mask_region.xy / maskSize;
		vec2 maskUv = adjustedUv * maskRegionSizeUv + maskRegionUv - maskOffset;
		maskUv = clamp(maskUv, maskRegionUv, maskRegionUv + maskRegionSizeUv);
		vec4 maskSample = texture(mask_texture, maskUv);
		COLOR.a *= screenSample.a * maskSample.a;
	}
}
