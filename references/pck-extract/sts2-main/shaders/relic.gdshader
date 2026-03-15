shader_type canvas_item;

uniform float pulse;
uniform float is_used;
uniform float is_wax;

float when_eq(float x, float y) {
  return 1.0 - abs(sign(x - y));
}

float when_neq(float x, float y) {
  return abs(sign(x - y));
}

void fragment() {        
	COLOR.rgb += abs(sin(TIME * 3.0)) * 0.2 * when_eq(pulse, 1);
    
    float avg = (COLOR.r + COLOR.g + COLOR.b) / 3.0;
    COLOR.rgb = vec3(avg) * when_eq(is_used, 1) + COLOR.rgb * when_neq(is_used, 1);
    vec3 wax_color = vec3(1, 0.8, 0.8);
    COLOR.rgb = vec3(avg) * wax_color * when_eq(is_wax, 1) + COLOR.rgb * when_neq(is_wax, 1);
}

