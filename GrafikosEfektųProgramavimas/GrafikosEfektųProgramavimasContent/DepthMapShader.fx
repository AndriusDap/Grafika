float4x4 Model;
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

struct VertexShaderInput
{
	float4 Position : POSITION;
};

struct VertexShaderOutput
{
	float4 Position		:POSITION;
	float4 Position2D	:TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 WorldViewProj = mul(Model, View);
	WorldViewProj = mul(WorldViewProj, Projection);

	output.Position = mul(input.Position, WorldViewProj);
	output.Position2D = input.Position;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = float4(0, 0, 0, 1);
	output.r = input.Position2D.z / input.Position2D.w;
	return output;
}

technique DepthMapShader
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
