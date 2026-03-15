shader_type canvas_item;

uniform vec3 map_color : source_color;
uniform vec3 black_layer_color : source_color;

void fragment() {
    vec3 red_mask = COLOR.r * map_color;
    vec3 blue_mask = COLOR.b * black_layer_color;
    vec3 green_mask = COLOR.g * vec3(1,1,1);
	COLOR.rgb = red_mask + blue_mask + green_mask;
}
