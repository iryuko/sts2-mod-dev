shader_type canvas_item;
render_mode blend_mix;

uniform float vpo_speed;
uniform float vpo_intensity;

uniform sampler2D main_texture : repeat_enable;
uniform float texture_panning_speed;

uniform sampler2D overlay : repeat_disable;

uniform sampler2D lut;

varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
	
	vec4 vpo_tex = textureLod(main_texture, UV + vec2(TIME * vpo_speed, 0), 0);
	VERTEX.y += (vpo_tex.b - 0.5) * 2.0 * vpo_intensity;
}

void fragment() {
	vec4 tex_panning = texture(main_texture, UV * vec2(1.0, 1.0) + vec2(TIME * texture_panning_speed, 0.0));
	vec4 overlay_tex = texture(overlay, UV);
	
	vec4 lut_color = texture(lut, tex_panning.rr + overlay_tex.rr);

	vec4 final_color = lut_color * vertex_color;
	float alpha = tex_panning.a * vertex_color.a;

	COLOR = vec4(final_color.rgb, alpha);
}
