#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D texture;
uniform float time;
uniform vec2 resolution;

vec4 GetPixel(vec2 Pos) {
	return texture2D(texture, Pos / resolution.xy);
}

vec4 Scanlines(vec3 FULL, vec3 EMPTY, float AlphaMult) {
	float SINE = sin(gl_FragCoord.y * /*3.1456*/ 3.13 + time);
	int IsTrue = 0;
	if (SINE > 0.5) { IsTrue = 1; }
	if (mod(gl_FragCoord.y, 2.0) < 1) { if (IsTrue == 1) { return vec4(FULL, AlphaMult); } else { return vec4(EMPTY, AlphaMult); } }
}

vec4 MIX(vec4 A, vec4 B) {
	return mix(A, B, B.a);
}

vec4 Blur(float AlphaMult) {
	return MIX(GetPixel(gl_FragCoord.xy), vec4(GetPixel(gl_FragCoord.xy + vec2(1.0, 1.0)).xyz, AlphaMult));
}

void main() {
	vec4 Default = vec4(GetPixel(gl_FragCoord.xy).xyz, 1);
	Default = MIX(Default, Scanlines(vec3(1.0), vec3(0.9), .4));
	Default = MIX(Default, Blur(.25));
	gl_FragColor = Default;
}