sampler SceneSampler : register(s0);

float pixelWidth;
float pixelHeight;

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 result = float4(0, 0, 0, 0);
	for(int x = -2; x < 2; x++)
	{
		for(int y = -2; y < 2; y++)
		{
			result = result + tex2D(SceneSampler, texCoord + float2(x * pixelWidth, y * pixelHeight));
		}
	}
	return result/16;
}

technique T
{
    pass P
    {
        PixelShader = compile ps_2_0 main();
    }
}