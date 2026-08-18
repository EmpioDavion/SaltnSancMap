#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 Offset;

Texture2D DiffuseTex;

sampler2D DiffuseTexSampler = sampler_state
{
    Texture = <DiffuseTex>;
};

Texture2D DownResTex : register(t1);

sampler2D DownResTexSampler : register(s1)
{
    Texture = <DownResTex>;
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
    float4 colour = tex2D(DiffuseTexSampler, i.uv + Offset);
    colour += tex2D(DiffuseTexSampler, i.uv - Offset);
	
    colour += tex2D(DownResTexSampler, i.uv - Offset.yx);
    colour += tex2D(DownResTexSampler, i.uv + Offset.yx);
	
	return colour * 0.25;
}

technique Drunk
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};