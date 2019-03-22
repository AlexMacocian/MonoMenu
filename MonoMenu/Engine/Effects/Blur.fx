//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

float kernel_size = 81;
float weights[81];
float2 offsets[81];

//-----------------------------------------------------------------------------
// Textures.
//-----------------------------------------------------------------------------

texture colorMapTexture;

sampler2D colorMap = sampler_state
{
	Texture = <colorMapTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_GaussianBlur(float2 texCoord : TEXCOORD) : COLOR0
{
	float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);

	[unroll(81)]for (int i = 0; i < kernel_size; ++i)
		color += tex2D(colorMap, texCoord + offsets[i]) * weights[i];

	return color;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique GaussianBlur
{
	pass
	{
		PixelShader = compile ps_3_0 PS_GaussianBlur();
	}
}
