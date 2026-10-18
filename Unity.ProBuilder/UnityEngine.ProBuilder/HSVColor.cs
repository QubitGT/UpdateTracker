namespace UnityEngine.ProBuilder;

internal sealed class HSVColor
{
	public float h;

	public float s;

	public float v;

	public HSVColor(float h, float s, float v)
	{
		this.h = h;
		this.s = s;
		this.v = v;
	}

	public HSVColor(float h, float s, float v, float sv_modifier)
	{
		this.h = h;
		this.s = s * sv_modifier;
		this.v = v * sv_modifier;
	}

	public static HSVColor FromRGB(Color col)
	{
		return ColorUtility.RGBtoHSV(col);
	}

	public override string ToString()
	{
		return $"( {h}, {s}, {v} )";
	}

	public float SqrDistance(HSVColor InColor)
	{
		return InColor.h / 360f - h / 360f + (InColor.s - s) + (InColor.v - v);
	}
}
