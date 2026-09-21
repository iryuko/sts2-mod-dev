shader_type canvas_item;
render_mode unshaded;

uniform float elapsed = 0.0;
varying vec4 item_color;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash(i), hash(i + vec2(1.0, 0.0)), f.x),
		mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0)), f.x), f.y);
}

void vertex() {
	item_color = COLOR;
}

void fragment() {
	float wrap = smoothstep(0.25, 0.31, elapsed);
	float hit = sin(clamp((elapsed - 0.31) / 0.08, 0.0, 1.0) * PI);
	float dissolve = clamp((elapsed - 0.39) / 0.41, 0.0, 1.0);
	float height = 1.0 - UV.y;
	// Bend only the tips. Roots stay anchored and the image is never stretched vertically.
	vec2 uv = UV;
	uv.x = 0.5 + (UV.x - 0.5) * (1.0 + 0.10 * wrap * height * height);
	uv.x += sin(UV.y * 11.0 + elapsed * 10.0) * 0.008 * height * (1.0 - wrap);
	vec4 source = texture(TEXTURE, clamp(uv, vec2(0.001), vec2(0.999)));
	float grain = noise(uv * vec2(49.0, 65.0));
	float broad = noise(uv * vec2(14.0, 19.0));
	float delay = 0.025 * smoothstep(0.35, 0.7, UV.x) + 0.012 * broad;
	float growth = clamp((elapsed - 0.065 - delay) / (0.185 - delay), 0.0, 1.0);
	float front = height * 0.89 + broad * 0.075 + grain * 0.025;
	float reveal = smoothstep(front, front + 0.032, growth);
	float luminance = dot(source.rgb, vec3(0.2126, 0.7152, 0.0722));
	// Compress the old gold highlights into dry charcoal; red cracks remain selective.
	float texture_value = pow(luminance, 0.65);
	vec3 pigment = mix(vec3(0.029, 0.020, 0.033), vec3(0.27, 0.075, 0.105), texture_value);
	pigment *= 0.84 + grain * 0.25;
	float red_vein = smoothstep(0.04, 0.22, source.r - source.g * 1.45) * smoothstep(0.22, 0.55, grain);
	pigment += vec3(0.32, 0.024, 0.052) * red_vein * (0.55 + hit * 1.1);
	// Erode the tips first, then break the remaining roots into uneven islands.
	float erosion_order = UV.y * 0.45 + broad * 0.36 + grain * 0.19;
	float erosion = smoothstep(dissolve * 1.08 - 0.045, dissolve * 1.08 + 0.015, erosion_order);
	float dying_edge = (1.0 - smoothstep(0.015, 0.075, abs(erosion_order - dissolve * 1.08)))
		* smoothstep(0.01, 0.12, dissolve) * (1.0 - dissolve);
	pigment += vec3(0.23, 0.018, 0.038) * dying_edge;
	float growth_edge = (1.0 - smoothstep(0.0, 0.055, abs(front - growth))) * (1.0 - wrap);
	pigment += vec3(0.17, 0.019, 0.038) * growth_edge;
	COLOR = vec4(pigment, source.a * reveal * erosion) * item_color;
}
