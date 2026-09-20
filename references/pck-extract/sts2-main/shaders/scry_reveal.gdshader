// Shader for the crystal ball, for the Scrying event.
// Does two things:
//  - Displays procedural crystal ball noise
//  - Fades out grid points when they are selected by the player

shader_type canvas_item;

const float borderwidth = 0.05;
const int gridSize = 11;
const float gridSizef = 11.0;

// Texture that is blended together to display the noise
uniform sampler2D noiseTex1 : source_color, filter_linear, repeat_enable;
uniform sampler2D noiseTex2 : source_color, filter_linear, repeat_enable;

// Color of the border between tiles
uniform vec3 borderColor : source_color;

// Colors used to generate the crystal ball noise. Should be one more than circleData, because each
// circle blends from i to i+1.
uniform vec3 colors[8] : source_color;

// Data for the "circles" that are displayed rotating within the crystal ball.
// X is offset from center, Y is size, Z is "power" (how much noise is displayed), W is step factor (how far the circle goes)
uniform vec4 circleData[8];

// The number of circles to use. Only this many will be sampled from circleData.
uniform int circles = 8;

// Which circle is the outer circle.
// If this is >= 0, we'll assume this is the outside of the ball and that this circle should clip
// tightly around the outside of the grid.
// If this is < 0, we'll assume that this is the inside of the ball and that the entire mask should
// clip tightly on the inside of the grid.
uniform int outerCircle = -1;

// Fade parameters for each of the cells inside the grid.
// X is start alpha, Y is end alpha, Z is start time.
// When you want to begin fading a grid cell, set the start/end alphas, and set the start time to
// the current value of the time uniform. The shader takes care of the rest.
uniform vec3 gridFadeParams[gridSize * gridSize];

// The current shader time. Not using TIME here for precision reasons. Start this at zero when the
// shader loads in and add delta time every frame.
uniform float time;

// The margin to leave on the sides of the grid, since the grid is inset into the image.
uniform float uvMargin;

// Returns grid position given a UV coordinate from 0-1
vec2 getGridPos(vec2 uv)
{
	return (((uv * gridSizef - gridSizef * 0.5) * (1.0 + uvMargin * 2.0) - uvMargin) + gridSizef * 0.5);
}

// Function like step but with antialiasing (from shadertoy)
float aaStep(float edge, float gradient)
{
    float halfPix = fwidth(gradient) / 2.0;
    float low = edge - halfPix;
    float hi  = edge + halfPix;
    return clamp((gradient - low) / (hi - low), 0.0, 1.0);
}

void fragment() {
    vec4 original_col = texture(TEXTURE, UV);
    vec4 ModularCol = COLOR / original_col;

	// ---- GRID STUFF -----
	
	// Calculate the alpha of the current grid position.
	vec2 gridposf = getGridPos(UV);
	vec2 gridpos = floor(gridposf);
	bool validIndex = gridpos.x >= 0.0 && gridpos.y >= 0.0 && gridpos.x < gridSizef && gridpos.y < gridSizef;
	int index = int(floor(gridpos.y * gridSizef + gridpos.x));
	float lastTimeChange = validIndex ? gridFadeParams[index].z : 0.0;
	float fromAlpha = validIndex ? gridFadeParams[index].x : 0.0;
	float toAlpha = validIndex ? gridFadeParams[index].y : 0.0;
	float timediff = time - lastTimeChange;
	float alpha = lastTimeChange == 0.0 ? 1.0 : mix(fromAlpha, toAlpha, clamp(timediff / 0.5, 0, 1));
	
	// Calculate the border value based on the neighboring grid alphas.
	float border = 0.0;
	
	for (int x = -1; x <= 1; x++)
	{
		for (int y = -1; y <= 1; y++)
		{
			if (x == 0 && y == 0) continue;
			vec2 gridpos_n = gridpos + vec2(float(x), float(y));
			int index_n = int(floor(gridpos_n.y) * gridSizef + gridpos_n.x);
			bool validIndex_n = gridpos_n.x >= 0.0 && gridpos_n.y >= 0.0 && gridpos_n.x < gridSizef && gridpos_n.y < gridSizef;
			if (!validIndex_n) continue;
			float lastTimeChange_n = gridFadeParams[index_n].z;
			float timediff_n = time - lastTimeChange_n;
			float alpha_n = lastTimeChange_n == 0.0 ? 1.0 : mix(gridFadeParams[index_n].x, gridFadeParams[index_n].y, clamp(timediff_n / 0.5, 0, 1));
			vec2 toBorder = max(max(gridpos_n - gridposf, vec2(0.0)), gridposf - (gridpos_n + vec2(1.0)));
			float borderdist = length(toBorder);
			border = max(border, smoothstep(borderwidth, borderwidth - 0.03, borderdist) * abs(alpha - alpha_n));
		}
	}
	
	// ------- NOISE STUFF --------
	
	vec2 fromCenter = UV - 0.5;
	float dist = length(fromCenter);
	
	// Sample the noises using polar coordinates
	vec2 polar1 = vec2(dist * 2.0, (atan(fromCenter.y, fromCenter.x) + PI) / (PI * 2.0));
	float noiseSample1 = texture(noiseTex1, (polar1 + vec2(0, TIME * 0.0065))).r;
	noiseSample1 = pow(noiseSample1, 0.8);
	
	vec2 polar2 = vec2(dist * 0.5, (atan(fromCenter.y, fromCenter.x) + PI) / (PI * 2.0));
	float noiseSample2 = texture(noiseTex2, (polar2 + vec2(0, TIME * 0.0011))).r;
	noiseSample2 = pow(noiseSample2 + 0.4, 1.0);
	
	// Blend the two samples together to get a little variance
	float noiseSample = noiseSample1 * noiseSample2 * 2.0;
	
	// Begin rendering circles
	vec3 col = colors[0];
	float lastmask;
	
	for (int i = 0; i < circles; i++)
	{
		vec4 curData = circleData[i];
		float circle = smoothstep(curData.x * 0.5, (curData.x + curData.y) * 0.5, dist);
		float mask = mix(1.0, noiseSample, pow(circle, curData.z)) * (1.0 - circle);
		float aaFactor = fwidth(mask);
		mask = aaStep(curData.w, mask);
		
		// If this is the outer circle, we want this circle to clip tightly around the outside border
		// of the grid. For us, the "outside border" is every cell with manhattan distance 3 from a
		// corner of the grid.
		if (i == outerCircle)
		{
			float halfgrid = (gridSizef - 1.0) * 0.5;
			vec2 normcoord = halfgrid - abs(halfgrid - gridpos);
			mask = (normcoord.x + normcoord.y) < 3.0 || !validIndex ? 0.0 : mask;
		}
		
		lastmask = mask;
		col = mix(colors[i+1], col, mask);
	}
	
	// If we are on the inside of the ball, then instead we want to clip tightly into the border.
	if (outerCircle < 0)
	{
		float halfgrid = (gridSizef - 1.0) * 0.5;
		vec2 normcoord = halfgrid - abs(halfgrid - gridpos);
		lastmask = (normcoord.x + normcoord.y) < 3.0 || !validIndex ? 0.0 : 1.0;
	}

	// Combine everything
	COLOR = vec4(col + borderColor * border, max(alpha, border) * lastmask) * ModularCol;
	//COLOR = vec4(vec3(noiseSample2), 1.0);
}