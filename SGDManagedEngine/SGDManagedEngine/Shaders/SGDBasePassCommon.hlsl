struct H1BasePassVSToPS
{
	float4 Position : SV_Position;
#if H1_GPUVERTEXFACTORY
	float4 Normal : TEXCOORD0;
#if H1_NUM_TEXCOORD2D > 0
	float2 Texcoord2D0 : TEXCOORD1;
#endif 
#else // NON-H1_GPUVERTEXFACTORY
#endif // H1_GPUVERTEXFACTORY
#if H1_COLOR
	float4 Color : COLOR;
#endif
};