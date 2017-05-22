#include "SGDBasePassCommon.hlsl"

Texture2D DiffuseMap : register(t0);
SamplerState LinearSampler : register(s0);

float4 Main(H1BasePassVSToPS input) : SV_TARGET
{
	float4 output = float4(1, 0, 0, 1);
#if H1_COLOR
	//output = input.Color;
#endif

#if H1_GPUVERTEXFACTORY
	output = DiffuseMap.Sample(LinearSampler, input.Texcoord2D0.xy);
#else
	//output = DiffuseMap.Sample(LinearSampler, input.Color.xy);
#endif

	return output;
}