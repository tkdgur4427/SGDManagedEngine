#include "SGDBasePassCommon.hlsl"

#define MAX_BONES 100

cbuffer Transformation : register(b0)
{
	float4x4 ViewProjectionMatrix;
	float4x4 BoneMatrices[MAX_BONES];
};

H1BasePassVSToPS Main(H1VertexFactoryInput input)
{
	H1BasePassVSToPS output;
#if H1_GPUVERTEXFACTORY
	// do vertex skinning
	float4x4 boneTransform = BoneMatrices[input.BlendIndices[0]] * input.BlendWeights[0];
	boneTransform += BoneMatrices[input.BlendIndices[1]] * input.BlendWeights[1];
	boneTransform += BoneMatrices[input.BlendIndices[2]] * input.BlendWeights[2];
	boneTransform += BoneMatrices[input.BlendIndices[3]] * input.BlendWeights[3];

	float4 skinnedPosition = mul(boneTransform, input.Position);
	output.Position = mul(ViewProjectionMatrix, float4(skinnedPosition.xyz, 1.0f));
	//output.Position = mul(ViewProjectionMatrix, input.Position);
#else
	output.Position = mul(ViewProjectionMatrix, float4(input.Position, 1.0f));
#endif

#if H1_GPUVERTEXFACTORY
	output.Texcoord2D0 = float2(input.Texcoord2D0.x, input.Texcoord2D0.y);
	// @TODO - normal initialization
	output.Normal = float4(0.0f, 0.0f, 0.0f, 0.0f);
#else

#endif

#if H1_COLOR
	// @TODO - separate color component to color and texcoord
	float2 texcoord = float2(input.Texcoord2D0.x, input.Texcoord2D0.y);
	output.Color = float4(texcoord, 0.0f, 1.0f);
	//output.Color = input.Color;
#endif

	return output;
}