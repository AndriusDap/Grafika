   sampler SceneSampler : register(s0);

   float3x3 sobelKernelX = { 1, 0, -1, 2, 0, -2, 1, 0, -1};
   float3x3 sobelKernelY = { 1, 2, 1, 0, 0, 0, -1, -2, -1};
   float sobelSum(float3x3 pixels)
   {
	 float ColorX = 0;
	 float ColorY = 0;
	 // Convolution operation. Matrix elements with same coords are multiplied and added to result
	 [unroll]
	 for(int x = 0; x < 3; x++)
	 {
		[unroll]
		for(int y = 0; y < 3; y++)
		{
			ColorX += sobelKernelY[x][y] * pixels[x][y];
			ColorY += sobelKernelX[x][y] * pixels[x][y];
		}
	 }
	 return sqrt(ColorX * ColorX + ColorY * ColorY);
   }

   float3 pixelOffsetX;
   float3 pixelOffsetY;

   float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
   {
	  float3x3 nearbyPixels;
	  [unroll]
	  for(int x = 0; x < 3; x++)
	  {
	 	 [unroll]
		 for(int y = 0; y < 3; y++)
		 {
			 nearbyPixels[x][y] = tex2D(SceneSampler, texCoord + float2(pixelOffsetX[x], pixelOffsetY[y]));
		 }
	  }
      int clipped = round(sobelSum(nearbyPixels) + 0.10f);
	  float result = !clipped;
      return tex2D(SceneSampler, texCoord) * float4(result.xxx, 1);
   }

   technique T
   {
      pass P
      {
         PixelShader = compile ps_2_0 main();
      }
   }