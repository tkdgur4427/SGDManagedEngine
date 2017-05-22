struct H1VertexFactoryInput
{
	float3 Position : ATTRIBUTE;
	float3 Normal	: ATTRIBUTE1;
#if H1_NUM_TEXCOORD2D > 0
	float2 Texcoord2D0 : ATTRIBUTE2;
#endif // currently maximum number of 2D texcoordinates is 1
#if H1_NUM_TEXCOORD3D > 0
	float3 Texcoord3D0 : ATTRIBUTE3;
#endif // currently maximum number of 3D texcoordinates is 1
#if H1_COLOR 
	float4 Color : ATTRIBUTE4;
#endif // when enabling color instancing
};