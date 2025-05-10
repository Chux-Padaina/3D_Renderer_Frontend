#version 450
layout (location=0) in vec3 pos;
layout (location=1) in vec2 UV;
layout (location=2) in vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 position;
out vec2 uv;
out vec3 fragPos;
vec3 normal;
void main(){
    position= projection*view*model* vec4(pos, 1.0f);
    uv=UV;
    normal= -mat3(transpose(inverse(model)))*Normal;
    fragPos= (model*vec4(pos, 1.0f)).xyz;
    gl_Position= position;
}