shader_type canvas_item;

uniform sampler2D transitionTex;
uniform float threshold : hint_range(0,1);
uniform float additionalFade : hint_range(0,1);
uniform float alpha : hint_range(0,1);

void fragment() {
    float falloff = 1.0 - texture(transitionTex, UV).r;

    // helps with falloff artifacts issues towards the transition extremes
    float remap  = mix(-0.1, 1.1, threshold);
    falloff = step(falloff, remap);
    COLOR.a = falloff;

    COLOR.a += threshold * additionalFade;

    // Apply overall transparency
    COLOR.a *= alpha;
}
