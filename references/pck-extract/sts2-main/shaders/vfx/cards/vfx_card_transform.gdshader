shader_type canvas_item;

uniform float brightness: hint_range(0,1) = 0;
uniform vec2 boing;
uniform vec2 edgeClampPoint;

void fragment() {
    vec4 falloff = texture(TEXTURE, UV);
    vec2 centerOffset = UV - vec2(0.5,0.5);
    vec2 distToCenter = abs(centerOffset);
    vec2 invertedDistToCenter = vec2(distToCenter.y, distToCenter.x);

    vec2 maxOffset = (edgeClampPoint - vec2(pow(invertedDistToCenter.x,2), pow(invertedDistToCenter.y,2))) * boing * sign(centerOffset);
    vec2 displaced_uv = UV + (distToCenter * maxOffset);
    COLOR = texture(TEXTURE, displaced_uv);


	COLOR.rgb += brightness;
}
