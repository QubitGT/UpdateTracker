using UnityEngine;

namespace g3;

public struct Colorb
{
	public byte r;

	public byte g;

	public byte b;

	public byte a;

	public byte this[int key]
	{
		get
		{
			return key switch
			{
				0 => r, 
				1 => g, 
				2 => b, 
				_ => a, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				r = value;
				break;
			case 1:
				g = value;
				break;
			case 2:
				b = value;
				break;
			default:
				a = value;
				break;
			}
		}
	}

	public Colorb(byte greylevel, byte a = 1)
	{
		r = (g = (b = greylevel));
		this.a = a;
	}

	public Colorb(byte r, byte g, byte b, byte a = 1)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public Colorb(float r, float g, float b, float a = 1f)
	{
		this.r = (byte)MathUtil.Clamp((int)(r * 255f), 0, 255);
		this.g = (byte)MathUtil.Clamp((int)(g * 255f), 0, 255);
		this.b = (byte)MathUtil.Clamp((int)(b * 255f), 0, 255);
		this.a = (byte)MathUtil.Clamp((int)(a * 255f), 0, 255);
	}

	public Colorb(byte[] v2)
	{
		r = v2[0];
		g = v2[1];
		b = v2[2];
		a = v2[3];
	}

	public Colorb(Colorb copy)
	{
		r = copy.r;
		g = copy.g;
		b = copy.b;
		a = copy.a;
	}

	public Colorb(Colorb copy, byte newAlpha)
	{
		r = copy.r;
		g = copy.g;
		b = copy.b;
		a = newAlpha;
	}

	public static implicit operator Colorb(Color32 c)
	{
		return new Colorb(c.r, c.g, c.b, c.a);
	}

	public static implicit operator Color32(Colorb c)
	{
		return new Color32(c.r, c.g, c.b, c.a);
	}
}
