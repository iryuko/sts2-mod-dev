shader_type canvas_item;
render_mode blend_premul_alpha;

uniform sampler2D dissolveGradient1 : repeat_enable;
uniform sampler2D dissolveGradient2 : repeat_enable;

uniform float threshold : hint_range(0.0, 1.0);

float get_dissolve_strength(vec2 uv, float offset, vec2 texSize)
{
	const float refSize = 1.0 / 400.0;
	vec2 scaling = refSize / texSize;

	uv.x += offset;
	float yOffset = pow(uv.x, 3);

	float dissolveStrength1 = (texture(dissolveGradient1, uv * scaling + vec2(-TIME * 0.25, yOffset)).r + uv.x) * 0.5;
	float dissolveStrength2 = (texture(dissolveGradient2, uv * scaling + vec2(-TIME * 0.25, yOffset)).r + uv.x) * 0.5;

	float combinedDissolveStrength = mix(dissolveStrength1, dissolveStrength2, 0.4);
	return step(combinedDissolveStrength, threshold);
}

void fragment() {
	const float upWarpAmount = 1024.0;
	const float leftWarpAmount = 200.0;
	vec2 warpAmount = vec2(-leftWarpAmount, upWarpAmount) * TEXTURE_PIXEL_SIZE;
	vec2 offset = max(UV.x - (threshold * 1.9 - 0.55), 0.0) * warpAmount;
	vec4 texCol = texture(TEXTURE, UV + offset * offset);

	float desatMask = get_dissolve_strength(UV, 0.05, TEXTURE_PIXEL_SIZE);
    texCol.rgb = mix(vec3(0,0,0), texCol.rgb, desatMask);
	texCol.rgb = mix(vec3(dot(texCol.rgb, vec3(0.299, 0.587, 0.114))), texCol.rgb, threshold);	// texCol *= get_dissolve_strength(UV, 0);
	//COLOR = texCol;
	COLOR = texCol * get_dissolve_strength(UV, 0, TEXTURE_PIXEL_SIZE);
	//COLOR = vec4(dissolveStrength, dissolveStrength, dissolveStrength, dissolveStrength);
}
