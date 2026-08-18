#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Offset;

Texture2D DiffuseTex;

sampler2D DiffuseTexSampler
{
    Texture = <DiffuseTex>;
};

Texture2D AcidTex : register(t1);

sampler2D AcidTexSampler : register(s1)
{
    Texture = <AcidTex>;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderOutput
{
    float4 position : SV_POSITION;
    float4 colour : COLOR0;
	float2 uv : TEXCOORD0;
};

float3 RGBtoHSV(float3 In)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
    float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
    float D = Q.x - min(Q.w, Q.y);
    float E = 1e-10;
    
    return float3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
}

// Helper function: Converts HSV back to RGB
float3 HSVtoRGB(float3 In)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 P = abs(frac(In.xxx + K.xyz) * 6.0 - K.www);
    
    return In.z * lerp(K.xxx, saturate(P - K.xxx), In.y);
}

// Main function to rotate color hue
// Rotation amount is normalized (0.0 to 1.0 represents 0 to 360 degrees)
float3 RotateColorHue(float3 originalColor, float rotationAmount)
{
    float3 hsv = RGBtoHSV(originalColor);
    hsv.x = frac(hsv.x + rotationAmount);
    
    return HSVtoRGB(hsv);
}

float4 MainPS(VertexShaderOutput i) : COLOR
{
    float4 colour = tex2D(DiffuseTexSampler, i.uv);
    float4 acid = tex2D(AcidTexSampler, i.uv);
    
    colour.rgb = RotateColorHue(colour.rgb, Offset + acid.r);
    
    return colour;
}

technique Acid
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};