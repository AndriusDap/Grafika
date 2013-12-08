// Transformacijos matricos
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 Model;
float3 CameraPosition;

// Ðviesos kryptis
float3 Light0Direction;
float3 Light1Direction;
float3 Light2Direction;

// Difuzinës ðviesos spalva
float3 Light0DiffuseColor;
float3 Light1DiffuseColor;
float3 Light2DiffuseColor;
float DiffuseIntensity;

float3 Light0SpecularColor;
float3 Light1SpecularColor;
float3 Light2SpecularColor;

float3 AmbientLightColor;
float AmbientIntensity;

Texture DiffuseTexture;
sampler DiffuseTextureSampler = sampler_state {  texture = <DiffuseTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture NormalMap;
sampler NormalMapSampler = sampler_state { texture = <NormalMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;	
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 TexCoords : TEXCOORD2;
	float3x3 WorldToTangentSpace : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL, float2 TexCoords : TEXCOORD0)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Normal = normalize(mul(Normal, World));
	output.TexCoords = TexCoords;

	output.WorldToTangentSpace[0] = mul(normalize(input.Tangent), World);
	output.WorldToTangentSpace[1] = mul(normalize(input.Binormal), World);
	output.WorldToTangentSpace[2] = mul(normalize(Normal), World);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 ambientSum = float4(AmbientLightColor * AmbientIntensity, 0.0);
	float4 diffuseSum = (0.0, 0.0, 0.0, 0.0);
	float4 specularSum = (0.0, 0.0, 0.0, 0.0);
	
	float3 DiffuseColor = tex2D(DiffuseTextureSampler, input.TexCoords);
	
	ambientSum = ambientSum * float4(DiffuseColor, 1);
	float3 normalMap = 2.0*(tex2D(NormalMapSampler, input.TexCoords)) - 1.0; 
	normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));	
	float4 normal = float4(normalMap, 1);
	
	float4 diffuse = saturate(dot(-Light0Direction, normal));
	float4 reflect = normalize(2 * diffuse * normal - float4(Light0Direction, 1));
	float4 specular = pow(saturate(dot(reflect, CameraPosition)), 32);
	diffuseSum = diffuseSum + float4(Light0DiffuseColor, 1) * diffuse * DiffuseIntensity;
	specularSum = specularSum +  (float4(Light0SpecularColor, 1) * specular);
		
	diffuse = saturate(dot(-Light1Direction, normal));
	reflect = normalize(2 * diffuse * normal - float4(Light1Direction, 1));
	specular = pow(saturate(dot(reflect, CameraPosition)), 32);
	diffuseSum = diffuseSum + float4(Light1DiffuseColor, 1) * diffuse * DiffuseIntensity;
	specularSum = specularSum +  float4(Light1SpecularColor, 1) * specular;
		
	diffuse = saturate(dot(-Light2Direction, normal));
	reflect = normalize(2 * diffuse * normal - float4(Light2Direction, 1));
	specular = pow(saturate(dot(reflect, CameraPosition)), 32);
	diffuseSum = diffuseSum + float4(Light2DiffuseColor, 1) * diffuse * DiffuseIntensity;
	specularSum = specularSum +  float4(Light2SpecularColor, 1) * specular;
	
	return ambientSum + specularSum + diffuseSum;
}

technique BasicLight
{
	pass Pass1
	{

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
