shader_type canvas_item;

uniform vec3 tint_color : source_color = vec3(1.0, 1.0, 1.0);

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    // Convert to grayscale using luminance
    float gray = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    // Blend grayscale toward tint color (lighter base)
    vec3 result = mix(tint_color, vec3(gray) * tint_color, 0.3);
    COLOR.rgb = result;
    COLOR.a = tex.a * 0.75;
}
