Texture Pass0;
Texture Pass1;

sampler SceneSampler0 = sampler_state {  texture = <Pass0> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};
sampler SceneSampler1 = sampler_state {  texture = <Pass1> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};


float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
   {
       return  tex2D(SceneSampler0, texCoord); // + tex2D(SceneSampler0, texCoord);
   }

technique T
{
    pass P
    {
        PixelShader = compile ps_2_0 main();
    }
}