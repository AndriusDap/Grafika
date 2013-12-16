float4x4 MVP;
float CurrentTime;

struct VertexShaderInput
{
	float3 Position : POSITION0;
    float3 StartingPoint : TEXCOORD0;
	float3 TargetPoint : TEXCOORD1;
	float StartingTime : TEXCOORD2;

};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4 dist = float4(normalize(input.TargetPoint - input.StartingPoint), 1);
	
	output.Position = float4(input.StartingPoint, 1);// + mul(float4(input.Position, 1), MVP);// + dist * (CurrentTime - input.StartingTime) / 10000;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return float4(1, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
