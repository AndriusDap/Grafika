// Transformacijos matricos
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4x4 Model;

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

float4x4 LightView;
float4x4 LightProjection;

Texture ShadowTexture;
sampler ShadowMapSampler = sampler_state {texture = <ShadowTexture>; magFilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror;};

Texture DiffuseTexture;
sampler DiffuseTextureSampler = sampler_state {  texture = <DiffuseTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture NormalMap;
sampler NormalMapSampler = sampler_state { texture = <NormalMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 TexCoords : TEXCOORD2;
	float4 PositionInLight : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL, float2 TexCoords : TEXCOORD0)
{
	VertexShaderOutput output;

	float4x4 WorldViewProj = mul(Model, View);
	WorldViewProj = mul(WorldViewProj, Projection);

	output.Position = mul(input.Position, WorldViewProj);
	output.Normal = normalize(mul(Normal, World));
	output.TexCoords = TexCoords;

	float4x4 LightWorldViewProj = mul(Model, LightView);
	LightWorldViewProj = mul(LightWorldViewProj, LightProjection);

	output.PositionInLight = mul(input.Position, LightWorldViewProj);
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 ambientSum = float4(AmbientLightColor * AmbientIntensity, 0.0);
	float4 diffuseSum = (0.0, 0.0, 0.0, 0.0);
	
	float3 DiffuseColor = tex2D(DiffuseTextureSampler, input.TexCoords);	
	ambientSum = ambientSum * float4(DiffuseColor, 1);

	float4 normal = float4(input.Normal, 1);

	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = input.PositionInLight.x/input.PositionInLight.w/2.0f + 0.5f;
    ProjectedTexCoords[1] = -input.PositionInLight.y/input.PositionInLight.w/2.0f + 0.5f;
	float Brightness = DiffuseIntensity;
    float ShadowDepth = tex2D(ShadowMapSampler, ProjectedTexCoords).r;
	float RealDepth = input.PositionInLight.x/input.PositionInLight.w/2.0f; 
	if((RealDepth - 0.01f) <= ShadowDepth)
	{
		Brightness = 0.0f;
	}

	float4 diffuse = saturate(dot(-Light0Direction, normal));
	diffuseSum = diffuseSum + (float4(Light0DiffuseColor, 1) * diffuse) * Brightness;
			
	diffuse = saturate(dot(-Light1Direction, normal));
	diffuseSum = diffuseSum + (float4(Light1DiffuseColor, 1) * diffuse) * Brightness;
		
	diffuse = saturate(dot(-Light2Direction, normal));
	diffuseSum = diffuseSum + (float4(Light2DiffuseColor, 1) * diffuse) * Brightness;

	return ambientSum + diffuseSum;;
}

technique ShadowShader
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
