shader_type canvas_item;

uniform float speed = 3.0;

void fragment() {
	vec4 texture = texture(TEXTURE, UV + vec2(TIME * 0.005 * speed,0));
    COLOR = texture;
}
