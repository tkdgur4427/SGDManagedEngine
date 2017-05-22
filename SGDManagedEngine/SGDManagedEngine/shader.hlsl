struct PSInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

struct Transformation
{
	float4x4 viewProjectionMatrix;
};

ConstantBuffer<Transformation> cBuffer0 : register(b0);

PSInput VSMain(float4 position : POSITION1, float4 color : COLOR1)
{
	PSInput result;	

	float4 resultPosition = mul(cBuffer0.viewProjectionMatrix, float4(position.xyz, 1.0f));
	result.position = resultPosition;
	result.color = color;

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}