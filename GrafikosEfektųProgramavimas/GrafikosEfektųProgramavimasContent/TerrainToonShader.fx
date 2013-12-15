// Transformacijos matricos
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 Model;

// ?viesos kryptis
float3 Light0Direction;
float3 Light1Direction;
float3 Light2Direction;

// Difuzin?s ?viesos spalva
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

float IsThisToon = 1;
float FogEnabled = 1;
Texture DiffuseTexture;
sampler DiffuseTextureSampler = sampler_state {  texture = <DiffuseTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture NormalMap;
sampler NormalMapSampler = sampler_state { texture = <NormalMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture CellMap;
sampler2D ColorMapSampler = sampler_state {texture = <CellMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};
struct VertexShaderInput
{
        float4 Position : POSITION0;
};

struct VertexShaderOutput
{
        float4 Position : POSITION0;
        float3 Normal : TEXCOORD0;
        float2 TexCoords : TEXCOORD1;
		float Fog : TEXCOORD2;
};


float ComputeFog(float3 position, float3 camera)
{
	float d = distance(position, camera);
	return clamp((d - 100) / (2000 - 100), 0, 1) * FogEnabled;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL, float2 TexCoords : TEXCOORD0)
{
        VertexShaderOutput output;

		float4x4 WorldViewProj = mul(Model, View);
		WorldViewProj = mul(WorldViewProj, Projection);

		output.Position = mul(input.Position, WorldViewProj);
        output.Normal = normalize(mul(Normal, World));
        output.TexCoords = TexCoords;
		output.Fog = ComputeFog(output.Position, CameraPosition);
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
        cellSum = diffuseSum + float4(Light0DiffuseColor, 1) * intensity * DiffuseIntensity * IsThisToon;
        cellSum[3] = 1;
        return lerp(ambientSum + cellSum , float4(0.3, 0.3, 0.3, 1), input.Fog);
}

technique TerrainToonShader
{
        pass Pass1
        {
                VertexShader = compile vs_2_0 VertexShaderFunction();
                PixelShader = compile ps_2_0 PixelShaderFunction();
        }
}