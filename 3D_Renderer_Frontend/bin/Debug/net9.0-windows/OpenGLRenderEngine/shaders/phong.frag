#version 450

out vec4 color;

in vec4 position;
in vec3 fragPos;
in vec2 uv;
//set by GO
uniform vec4 lightColor;
uniform vec3 lightPosition;
uniform vec3 lightDirection;
uniform float ambientStrength;
uniform float diffuseStrength;
uniform vec3 cameraPosition;

//Set by material
uniform sampler2D albedoMap;
uniform sampler2D normalMap;
uniform sampler2D roughnessMap;
uniform sampler2D metallicMap;


vec3 normal;
float shininess;
float specularStrength;


float CalculateAmbient(){
    return ambientStrength;
}
float CalculateDiffuse(){
    float diffFactor= max(dot(normalize(normal), normalize(lightDirection)), 0);
    return diffFactor;
}
float CalculateSpecular(){
    vec3 fragmentToEye= normalize(cameraPosition-fragPos);
    vec3 reflectedVector= normalize(reflect(normalize(lightDirection), normalize(normal)));
    float specBaseFactor= dot(fragmentToEye, reflectedVector);
    float specularFactor= pow(specBaseFactor, shininess);
    return specularFactor;
}
vec4 CalculateLight(){
    float amb= CalculateAmbient();
    float diff= CalculateDiffuse();
    float spec= CalculateSpecular();
    vec4 specularColor= vec4(0.0f);
    if(diff>0){
        specularColor= spec*specularStrength*lightColor;
    }
    vec4 diffColor= diff*diffuseStrength*lightColor;
    vec4 ambientColor= ambientStrength*lightColor;

    return diffColor+ambientColor+specularColor;
}
void getShininess(){

    shininess= 1- texture(roughnessMap, uv).r;
}
void getSpecularity(){
    specularStrength= texture(metallicMap, uv).r;
}
void getNormal(){
    normal = normalize(texture(normalMap, uv).rgb * 2.0 - 1.0);
}
void main(){
    getNormal();
    getShininess();
    getSpecularity();
    vec4 lightValue= CalculateLight();
    vec4 baseColor= texture(albedoMap, uv);
    color= baseColor*lightValue;
}