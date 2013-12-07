// Transformacijos matricos
float4x4 World;
float4x4 View;
float4x4 Projection;

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

float3 CameraPosition;

float SpecularToggle;
float NormalToggle;
float LightMapToggle;

Texture DiffuseTexture;
sampler DiffuseTextureSampler = sampler_state {  texture = <DiffuseTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture NormalMap;
sampler NormalMapSampler = sampler_state { texture = <NormalMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture CellMap;
sampler2D ColorMapSampler = sampler_state {texture = <CellMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; };
struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 View : TEXCOORD1;
	float2 TexCoords : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL, float2 TexCoords : TEXCOORD0)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.Normal = normalize(mul(Normal, World));
	output.View = normalize(float4(CameraPosition, 1.0) - worldPosition);
	output.TexCoords = TexCoords;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 ambientSum = float4(AmbientLightColor * AmbientIntensity, 0.0);
	float4 diffuseSum = (0.0, 0.0, 0.0, 0.0);
	float4 cellSum = (0.0, 0.0, 0.0, 0.0);
	float3 DiffuseColor = tex2D(DiffuseTextureSampler, input.TexCoords);	
	ambientSum = ambientSum * float4(DiffuseColor, 1);

	float4 normal = float4(input.Normal, 1);
	
	float diffuse = saturate(dot(-Light0Direction, normal));
	float2 intensityCoordinate = float2(diffuse, 0);
	float4 intensity = tex2D(ColorMapSampler, intensityCoordinate);
	diffuseSum = diffuseSum + float4(Light0DiffuseColor, 1) * diffuse * DiffuseIntensity;
	cellSum = diffuseSum + float4(Light0DiffuseColor, 1) * intensity * DiffuseIntensity;
	cellSum[3] = 1;
	return ambientSum + cellSum;
}

technique TerrainToonShader
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
