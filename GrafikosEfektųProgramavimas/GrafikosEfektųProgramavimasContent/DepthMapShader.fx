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
	float4 Position	:POSITION;
	float Depth	:TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4x4 WorldViewProj = mul(Model, World);
	WorldViewProj = mul(WorldViewProj, View);
	WorldViewProj = mul(WorldViewProj, Projection);

	output.Position = mul(input.Position, WorldViewProj);
	output.Depth.x = (output.Position.z/output.Position.w); 

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return float4(1 - input.Depth, 0, 0, 1);
}

technique DepthMapShader
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
