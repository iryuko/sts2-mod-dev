shader_type canvas_item;
 
uniform float frequency : hint_range(0.0, 10.0) = 2.0;              // Rate of glow change
uniform float dimLevel : hint_range(0.0, 1.0) = 0.0;                // Alpha level of area outside the mask
uniform float brightnessRange : hint_range(0.0, 1.0) = 0.05;        // Range of brightness change
uniform float brightnessOffset : hint_range(-0.1, 0.1) = -0.05;     // Offset of brightness from center
uniform vec3 tintColor : source_color;                              // Base tint color
uniform float tintBrightness : hint_range(0.0, 1.0) = 0.2;          // Brightness of the tint color
uniform sampler2D maskImage;                                        // Mask Image
 
void fragment() {
    vec4 maskImageColor = texture(maskImage, UV);
     
    // Caluclate the brightness of the pixel in the mask image
    float maskImageBrightness = (maskImageColor.r + maskImageColor.g + maskImageColor.b) / 3.0;
     
    // Show only the white area on the target texture and dim the rest
    COLOR.a = min(dimLevel + maskImageBrightness, 1.0);
     
    // Combined shader of inner glow and color tint
    COLOR.rgb += maskImageBrightness * (brightnessRange * sin(frequency * TIME) + brightnessOffset) * tintColor * tintBrightness;
}