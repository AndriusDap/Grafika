sampler SceneSampler : register(s0);

float pixelWidth;
float pixelHeight;

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 result;
	result.w = 0;
	result.z = tex2D(SceneSampler, texCoord + float2(0, -4*pixelHeight)).z;
	result.x = tex2D(SceneSampler, texCoord + float2(2*pixelWidth, 0)).x;
	result.y = tex2D(SceneSampler, texCoord + float2(-2*pixelWidth, 0)).y;
	return result;
}

technique T
{
    pass P
    {
        PixelShader = compile ps_2_0 main();
    }
}