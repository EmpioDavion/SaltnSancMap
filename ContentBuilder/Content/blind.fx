#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 Offset;
float2 Tiling;

Texture2D DiffuseTex;

sampler2D DiffuseTexSampler
{
    Texture = <DiffuseTex>;
};

Texture2D CloudsTex : register(t1);

sampler2D CloudsTexSampler : register(s1)
{
    Texture = <CloudsTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderOutput
{
    float4 position : SV_POSITION;
    float4 colour : COLOR0;
	float2 uv : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput i) : COLOR
{
    float4 colour = tex2D(DiffuseTexSampler, i.uv);
    float4 clouds = tex2D(CloudsTexSampler, (i.uv + Offset) * Tiling);
    float cloud = dot(clouds, 0.25);
	
    float diff = length(i.uv - 0.5) * 2.0; // distance to edge
    float brightness = saturate(1.0 - diff);
    float brightness2 = brightness * brightness;
	
    return lerp(colour * cloud, colour, brightness2) * brightness2;
}

technique Blind
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};