   sampler SceneSampler : register(s0);


   #if 0
		//Sobel kernel
	   float3x3 kernelx =  { 1, 0, -1, 2, 0, -2, 1, 0, -1};
	   float3x3 kernely = { 1, 2, 1, 0, 0, 0, -1, -2, -1};
	   float offset = 0.2f;
   #else
		// Scharr kernel
	   float3x3 kernelx = {3, 10, 3, 0, 0, 0, -3, -10, -3};
	   float3x3 kernely = {3, 0, -3, 10, 0, -10, 3, 0, -3};
	   float offset = -0.2f;
   #endif

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
			ColorX += kernely[x][y] * pixels[x][y];
			ColorY += kernelx[x][y] * pixels[x][y];
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
      int clipped = round(sobelSum(nearbyPixels) + offset);
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