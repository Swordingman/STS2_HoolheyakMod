shader_type canvas_item;
render_mode unshaded, blend_add;

uniform vec4 tint_color : source_color = vec4(1.0);
uniform float recolor_amount : hint_range(0.0, 1.0) = 0.0;
varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
}

void fragment() {
	vec4 sample_color = texture(TEXTURE, UV);
	vec2 border = smoothstep(vec2(0.0), vec2(0.018), UV) * (vec2(1.0) - smoothstep(vec2(0.982), vec2(1.0), UV));
	float light = max(sample_color.r, max(sample_color.g, sample_color.b));
	vec3 color = mix(sample_color.rgb, vec3(light), recolor_amount);
	COLOR = vec4(color * tint_color.rgb, sample_color.a * tint_color.a * border.x * border.y) * vertex_color;
}
