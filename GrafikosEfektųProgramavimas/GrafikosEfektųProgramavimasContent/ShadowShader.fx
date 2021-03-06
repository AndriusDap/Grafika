// Transformacijos matricos
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4x4 Model;

// �viesos kryptis
float3 Light0Direction;
float3 Light1Direction;
float3 Light2Direction;

// Difuzin�s �viesos spalva
float3 Light0DiffuseColor;
float3 Light1DiffuseColor;
float3 Light2DiffuseColor;
float DiffuseIntensity;

float3 Light0SpecularColor;
float3 Light1SpecularColor;
float3 Light2SpecularColor;

float3 AmbientLightColor;
float AmbientIntensity;

float FogEnabled = 1;
float4x4 LightView;
float4x4 LightProjection;

Texture ShadowTexture;
sampler ShadowMapSampler = sampler_state {texture = <ShadowTexture>; filter = Point; AddressU = mirror; AddressV = mirror;};

Texture DiffuseTexture;
sampler DiffuseTextureSampler = sampler_state {  texture = <DiffuseTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

Texture NormalMap;
sampler NormalMapSampler = sampler_state { texture = <NormalMap>; magFilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};



float4 GetPositionFromLight(float4 position)
{
    float4x4 WorldViewProjection =
     mul(mul(Model, LightView), LightProjection);
    return mul(position, WorldViewProjection);  
}


struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 TexCoords : TEXCOORD1;
	float4 OriginalPosition : TEXCOORD2;
	float Fog : TEXCOORD3;
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
	output.OriginalPosition = mul(mul(input.Position, Model), World);

	output.Position = mul(input.Position, WorldViewProj);
	output.Normal = normalize(mul(Normal, World));
	output.TexCoords = TexCoords;

	float4x4 LightWorldViewProj = mul(mul(Model, World), LightView);
	LightWorldViewProj = mul(LightWorldViewProj, LightProjection);

	output.OriginalPosition = mul(input.Position, LightWorldViewProj);
	output.Fog = ComputeFog(output.Position, CameraPosition);
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 ambientSum = float4(AmbientLightColor * AmbientIntensity, 0.0);
	float4 diffuseSum = (0.0, 0.0, 0.0, 0.0);
	
	float3 DiffuseColor = tex2D(DiffuseTextureSampler, input.TexCoords);	
	ambientSum = ambientSum * float4(DiffuseColor, 1);

	float4 normal = float4(input.Normal, 1);

	float2 ShadowTexC = 0.5 * input.OriginalPosition.xy / input.OriginalPosition.w - float2(-0.5, 0.5);

	float Brightness = DiffuseIntensity;
	float shadowdepth = 1 - tex2D(ShadowMapSampler, ShadowTexC).r;    

	// Check our value against the depth value
	float ourdepth = abs(input.OriginalPosition.z / input.OriginalPosition.w);

	// Check the shadowdepth against the depth of this pixel
	// a fudge factor is added to account for floating-point error
	if (shadowdepth <= (ourdepth-0.003))
	{
		// we're in shadow, cut the light
		Brightness *= 0.5;
	};
	
	float4 diffuse = saturate(dot(-Light0Direction, normal));
	diffuseSum = diffuseSum + (float4(Light0DiffuseColor, 1) * diffuse) * Brightness;
			
	diffuse = saturate(dot(-Light1Direction, normal));
	diffuseSum = diffuseSum + (float4(Light1DiffuseColor, 1) * diffuse) * Brightness;
		
	diffuse = saturate(dot(-Light2Direction, normal));
	diffuseSum = diffuseSum + (float4(Light2DiffuseColor, 1) * diffuse) * Brightness;

	return lerp(ambientSum + diffuseSum, float4(0.3, 0.3, 0.3, 1), input.Fog);
}

technique ShadowShader
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
