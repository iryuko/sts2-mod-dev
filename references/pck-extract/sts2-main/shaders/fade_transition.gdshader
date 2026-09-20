shader_type canvas_item;

uniform float threshold : hint_range(0,1);

void fragment() {
    COLOR.a = threshold;
}
