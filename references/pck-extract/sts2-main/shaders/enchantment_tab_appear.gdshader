shader_type canvas_item;

uniform float progress;

vec2 rotate(vec2 v, float a)
{
	float s = sin(a);
	float c = cos(a);
	mat2 m = mat2(vec2(c, s), vec2(-s, c));
	return m * v;
}

void fragment()
{
	// White fade
	const float whiteWidth = 0.2;
	const float fadeWidth = 0.05;
	vec2 uv = UV;
	uv.y = mix(uv.y, (uv.y - 0.5) * 0.75 + 0.5, smoothstep(progress, progress + whiteWidth, uv.x));
	vec4 tex = texture(TEXTURE, uv);
	vec3 addCol = vec3(1.0, 0.75, 0.9);
	vec4 col = mix(tex, vec4(tex.rgb + addCol, 1.0), smoothstep(progress, progress + whiteWidth, uv.x));
	col = mix(col, vec4(addCol, 0.0), smoothstep(progress+whiteWidth, progress + whiteWidth+fadeWidth, uv.x));
	
	// Shine
	/*
	const float shineWidth = 0.05;
	const float shineFade = 0.03;
	float shineProgress = (progress - 0.7) / 0.3;
	float shineAngle = 60.0 * PI / 360.0;
	vec2 rotatedUv = rotate(UV, shineAngle);
	float shine = smoothstep(shineProgress - shineWidth * 0.5 - shineFade, shineProgress - shineWidth * 0.5, rotatedUv.x);
	shine = shine * smoothstep(shineProgress + shineWidth * 0.5 + shineFade, shineProgress + shineWidth * 0.5, rotatedUv.x);
	col.rgb += shine * vec3(1.0, 0.9, 1.0) * 0.5;
	*/
	
	COLOR = vec4(col.rgb, col.a * tex.a);
}