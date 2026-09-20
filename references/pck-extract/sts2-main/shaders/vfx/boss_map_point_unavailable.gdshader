shader_type canvas_item;

uniform vec3 additive_tint : source_color;

void fragment() {
	COLOR.rgb += additive_tint;
}

