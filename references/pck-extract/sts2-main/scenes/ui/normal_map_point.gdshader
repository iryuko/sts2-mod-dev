shader_type canvas_item;

uniform vec3 initial_color : source_color;
uniform vec3 map_color : source_color;

void fragment() {
    float diff = distance(COLOR.rgb, initial_color.rgb);
    if (diff < 0.5) {
        COLOR.rgb = map_color;
    }
}
