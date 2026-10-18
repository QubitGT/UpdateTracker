using System;

namespace g3;

public class ColorHSV
{
	public float h;

	public float s;

	public float v;

	public float a;

	public Colorf RGBA
	{
		get
		{
			return ConvertToRGB();
		}
		set
		{
			ConvertFromRGB(value);
		}
	}

	public ColorHSV(float h, float s, float v, float a = 1f)
	{
		this.h = h;
		this.s = s;
		this.v = v;
		this.a = a;
	}

	public ColorHSV(Colorf rgb)
	{
		ConvertFromRGB(rgb);
	}

	public Colorf ConvertToRGB()
	{
		float num = h;
		float f = s;
		float f2 = v;
		if (num > 360f)
		{
			num -= 360f;
		}
		if (num < 0f)
		{
			num += 360f;
		}
		num = MathUtil.Clamp(num, 0f, 360f);
		f = MathUtil.Clamp(f, 0f, 1f);
		float num2 = MathUtil.Clamp(f2, 0f, 1f);
		float num3 = num2 * f;
		float num4 = num3 * (1f - Math.Abs(num / 60f % 2f - 1f));
		float num5 = num2 - num3;
		float num6;
		float num7;
		float num8;
		switch ((int)(num / 60f))
		{
		case 0:
			num6 = num3;
			num7 = num4;
			num8 = 0f;
			break;
		case 1:
			num6 = num4;
			num7 = num3;
			num8 = 0f;
			break;
		case 2:
			num6 = 0f;
			num7 = num3;
			num8 = num4;
			break;
		case 3:
			num6 = 0f;
			num7 = num4;
			num8 = num3;
			break;
		case 4:
			num6 = num4;
			num7 = 0f;
			num8 = num3;
			break;
		default:
			num6 = num3;
			num7 = 0f;
			num8 = num4;
			break;
		}
		return new Colorf(MathUtil.Clamp(num6 + num5, 0f, 1f), MathUtil.Clamp(num7 + num5, 0f, 1f), MathUtil.Clamp(num8 + num5, 0f, 1f), a);
	}

	public void ConvertFromRGB(Colorf rgb)
	{
		a = rgb.a;
		float r = rgb.r;
		float g = rgb.g;
		float b = rgb.b;
		float num = r;
		int num2 = 0;
		if (g > num)
		{
			num = g;
			num2 = 1;
		}
		if (b > num)
		{
			num = b;
			num2 = 2;
		}
		float num3 = r;
		if (g < num3)
		{
			num3 = g;
		}
		if (b < num3)
		{
			num3 = b;
		}
		float num4 = num - num3;
		if (num4 == 0f)
		{
			h = 0f;
		}
		else
		{
			switch (num2)
			{
			case 0:
				h = 60f * ((g - b) / num4 % 6f);
				break;
			case 1:
				h = 60f * ((b - r) / num4 + 2f);
				break;
			case 2:
				h = 60f * ((r - g) / num4 + 4f);
				break;
			}
			if (h < 0f)
			{
				h += 360f;
			}
		}
		v = num;
		if (num == 0f)
		{
			s = 0f;
		}
		else
		{
			s = num4 / num;
		}
	}
}
