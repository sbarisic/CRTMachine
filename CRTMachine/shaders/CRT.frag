#ifdef GL_ES
precision mediump float;
#endif

uniform float time;
uniform float R;
uniform float G;
uniform float B;

void main(void) {
	float mult = .99 + sin(gl_FragCoord.y * 3.1456 + time * 0.2);
	vec3 col = vec3(.1 * mult, .1 * mult, .1 * mult);
	col += vec3(R, G, B);
	gl_FragColor = vec4(col, .5);

}