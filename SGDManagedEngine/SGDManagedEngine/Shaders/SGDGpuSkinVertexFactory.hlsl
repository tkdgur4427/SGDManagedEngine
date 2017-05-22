struct H1VertexFactoryInput
{
	float4 Position		: ATTRIBUTE;
	half4 TangentZ		: ATTRIBUTE1;
	half4 TangentX		: ATTRIBUTE2;	
	int4 BlendIndices	: ATTRIBUTE3;
	float4 BlendWeights	: ATTRIBUTE4;
#if H1_NUM_TEXCOORD2D > 0
	float2 Texcoord2D0	: ATTRIBUTE5;
#endif // currently maximum number of 2D texcoordinates is 1
#if H1_COLOR
	float4 Color : ATTRIBUTE6;
#endif
};