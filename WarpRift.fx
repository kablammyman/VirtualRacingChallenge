sampler2D TexMap0;
sampler2D TexMap1;

const float2 ScreenCenter = {0.25, 0.5};
const float2 LensCenterLeft = {0.2863248, 0.5};
const float2 LensCenterRight = {0.7136753, 0.5};
const float2 ScaleIn = {4.0, 2.5};
const float2 Scale = {0.1469278, 0.2350845};
const float4 HmdWarpParam = {1.0, 0.22, 0.24, 0.0};

// Scales input texture coordinates for distortion.
float2 HmdWarp(float2 Tex : TEXCOORD0, float2 LensCenter)
{
	float2 theta = (Tex - LensCenter) * ScaleIn; // Scales to [-1, 1]
	float rSq = theta.x * theta.x + theta.y * theta.y;
	float2 rvector= theta * (HmdWarpParam.x + HmdWarpParam.y * rSq +
	                         HmdWarpParam.z * rSq * rSq +
	                         HmdWarpParam.w * rSq * rSq  * rSq);
	return LensCenter + Scale * rvector;
}

float4 SBSRift(float2 Tex : TEXCOORD0) : COLOR
{
	float4 tColor;
	float2 newPos = Tex;
	float2 warpPos;

	if(newPos.x < 0.5)
	{
		warpPos = HmdWarp(newPos, LensCenterLeft);
	}
	else
	{
		warpPos = HmdWarp(newPos, LensCenterRight);
	}
	tColor = tex2D(TexMap0, warpPos);

	if(warpPos.x < 0.0f || warpPos.x > 1.0f || warpPos.y < 0.0f || warpPos.y > 1.0f) {
		tColor[0] = 0.0f;
		tColor[1] = 0.0f;
		tColor[2] = 0.0f;
	}

	return tColor;
}

//float4 SBSRift(float2 Tex : TEXCOORD0) : COLOR
//{
//	float4 tColor;
//	float2 newPos = Tex;
//	float2 warpPos;
//
//	if(newPos.x < 0.5)
//	{
//		newPos.x = newPos.x * 2.0f;
//		warpPos = HmdWarp(newPos);
//		warpPos.x = warpPos.x * 0.5f;
//		tColor = tex2D(TexMap0, warpPos);
//	}
//	else 
//	{
//		newPos.x = (newPos.x - 0.5f) * 2.0f;
//		warpPos = HmdWarp(newPos);
//		warpPos.x = warpPos.x * 0.5f + 0.5f;
//		tColor = tex2D(TexMap0, warpPos);
//	}
//
//	if(warpPos.x < 0.0f || warpPos.x > 1.0f || warpPos.y < 0.0f || warpPos.y > 1.0f) {
//		tColor[0] = 0.0f;
//		tColor[1] = 0.0f;
//		tColor[2] = 0.0f;
//	}
//
//	return tColor;
//}

technique ViewShader
{
	pass P0
    {
		VertexShader = null;
        PixelShader  = compile ps_2_0 SBSRift();
    }
}
