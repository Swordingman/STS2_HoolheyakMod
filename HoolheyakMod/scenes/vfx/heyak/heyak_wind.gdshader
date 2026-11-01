shader_type canvas_item;
render_mode unshaded, blend_mix;

// Per-instance time is animated by AnimationPlayer, so pausing the scene pauses the effect.
uniform float vfx_time = 0.0;
uniform float intensity : hint_range(0.0, 2.0) = 1.0;
uniform float phase = 0.0;
uniform vec4 wind_color : source_color = vec4(0.18, 0.95, 0.9, 1.0);
varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;
}

void fragment() {
	float height = 1.0 - UV.y;
	float width = mix(0.055, 0.48, pow(height, 0.78));
	float centered_x = UV.x - 0.5;
	float edge = 1.0 - smoothstep(width * 0.82, width, abs(centered_x));
	float side = clamp(centered_x / max(width, 0.001), -1.0, 1.0);
	float angle = asin(side);
	float turn = angle / TAU + height * 0.75 - vfx_time * 1.65 + phase;
	float ribbon = 0.5 + 0.5 * sin(height * 35.0 + angle * 2.7 - vfx_time * 16.0 + phase * TAU);
	float fine_ribbon = pow(ribbon, 7.0);
	// The first three rows of the original atlas contain feather-shaped wind strips.
	vec2 atlas_uv = vec2(fract(turn), fract(height * 2.2 - vfx_time * 0.3) * 0.25);
	vec4 wind = texture(TEXTURE, atlas_uv);
	float end_fade = smoothstep(0.0, 0.07, height) * (1.0 - smoothstep(0.88, 1.0, height));
	float body = 0.17 + 0.2 * (1.0 - side * side);
	float alpha = edge * end_fade * (body + fine_ribbon * 0.55 + wind.a * 0.3) * intensity;
	vec3 color = mix(wind_color.rgb * 0.5, wind_color.rgb, fine_ribbon);
	color = mix(color, wind.rgb, wind.a * 0.6);
	COLOR = vec4(color, alpha * wind_color.a) * vertex_color;
}
