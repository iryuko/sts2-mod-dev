shader_type canvas_item;

uniform float pulse;

float when_eq(float x, float y) {
  return 1.0 - abs(sign(x - y));
}


void fragment() {        
	COLOR.rgb += abs(sin(TIME * 3.0)) * 0.2 * when_eq(pulse, 1);
}

