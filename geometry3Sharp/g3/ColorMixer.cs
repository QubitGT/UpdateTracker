namespace g3;

public static class ColorMixer
{
	public static Colorf Lighten(Colorf baseColor, float fValueMult = 1.25f)
	{
		ColorHSV colorHSV = new ColorHSV(baseColor);
		colorHSV.v = MathUtil.Clamp(colorHSV.v * fValueMult, 0f, 1f);
		return colorHSV.ConvertToRGB();
	}

	public static Colorf Darken(Colorf baseColor, float fValueMult = 0.75f)
	{
		ColorHSV colorHSV = new ColorHSV(baseColor);
		colorHSV.v *= fValueMult;
		return colorHSV.ConvertToRGB();
	}

	public static Colorf CopyHue(Colorf BaseColor, Colorf TakeHue, float fBlendAlpha)
	{
		ColorHSV colorHSV = new ColorHSV(BaseColor);
		ColorHSV colorHSV2 = new ColorHSV(TakeHue);
		colorHSV.h = colorHSV2.h;
		colorHSV.s = MathUtil.Lerp(colorHSV.s, colorHSV2.s, fBlendAlpha);
		colorHSV.v = MathUtil.Lerp(colorHSV.v, colorHSV2.v, fBlendAlpha);
		return colorHSV.ConvertToRGB();
	}
}
