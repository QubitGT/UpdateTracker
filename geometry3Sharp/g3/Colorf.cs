using System;
using UnityEngine;

namespace g3;

public struct Colorf : IComparable<Colorf>, IEquatable<Colorf>
{
	public float r;

	public float g;

	public float b;

	public float a;

	public static readonly Colorf TransparentWhite = new Colorf(255, 255, 255, 0);

	public static readonly Colorf TransparentBlack = new Colorf(0, 0, 0, 0);

	public static readonly Colorf White = new Colorf(255, 255, 255);

	public static readonly Colorf Black = new Colorf(0, 0, 0);

	public static readonly Colorf Blue = new Colorf(0, 0, 255);

	public static readonly Colorf Green = new Colorf(0, 255, 0);

	public static readonly Colorf Red = new Colorf(255, 0, 0);

	public static readonly Colorf Yellow = new Colorf(255, 255, 0);

	public static readonly Colorf Cyan = new Colorf(0, 255, 255);

	public static readonly Colorf Magenta = new Colorf(255, 0, 255);

	public static readonly Colorf VideoWhite = new Colorf(235, 235, 235);

	public static readonly Colorf VideoBlack = new Colorf(16, 16, 16);

	public static readonly Colorf VideoBlue = new Colorf(16, 16, 235);

	public static readonly Colorf VideoGreen = new Colorf(16, 235, 16);

	public static readonly Colorf VideoRed = new Colorf(235, 16, 16);

	public static readonly Colorf VideoYellow = new Colorf(235, 235, 16);

	public static readonly Colorf VideoCyan = new Colorf(16, 235, 235);

	public static readonly Colorf VideoMagenta = new Colorf(235, 16, 235);

	public static readonly Colorf Purple = new Colorf(161, 16, 193);

	public static readonly Colorf DarkRed = new Colorf(128, 16, 16);

	public static readonly Colorf FireBrick = new Colorf(178, 34, 34);

	public static readonly Colorf HotPink = new Colorf(255, 105, 180);

	public static readonly Colorf LightPink = new Colorf(255, 182, 193);

	public static readonly Colorf DarkBlue = new Colorf(16, 16, 139);

	public static readonly Colorf BlueMetal = new Colorf(176, 197, 235);

	public static readonly Colorf Navy = new Colorf(16, 16, 128);

	public static readonly Colorf CornflowerBlue = new Colorf(100, 149, 237);

	public static readonly Colorf LightSteelBlue = new Colorf(176, 196, 222);

	public static readonly Colorf DarkSlateBlue = new Colorf(72, 61, 139);

	public static readonly Colorf Teal = new Colorf(16, 128, 128);

	public static readonly Colorf ForestGreen = new Colorf(16, 139, 16);

	public static readonly Colorf LightGreen = new Colorf(144, 238, 144);

	public static readonly Colorf Orange = new Colorf(230, 73, 16);

	public static readonly Colorf Gold = new Colorf(235, 115, 63);

	public static readonly Colorf DarkYellow = new Colorf(235, 200, 95);

	public static readonly Colorf SiennaBrown = new Colorf(160, 82, 45);

	public static readonly Colorf SaddleBrown = new Colorf(139, 69, 19);

	public static readonly Colorf Goldenrod = new Colorf(218, 165, 32);

	public static readonly Colorf Wheat = new Colorf(245, 222, 179);

	public static readonly Colorf LightGrey = new Colorf(211, 211, 211);

	public static readonly Colorf Silver = new Colorf(192, 192, 192);

	public static readonly Colorf LightSlateGrey = new Colorf(119, 136, 153);

	public static readonly Colorf Grey = new Colorf(128, 128, 128);

	public static readonly Colorf DarkGrey = new Colorf(169, 169, 169);

	public static readonly Colorf SlateGrey = new Colorf(112, 128, 144);

	public static readonly Colorf DimGrey = new Colorf(105, 105, 105);

	public static readonly Colorf DarkSlateGrey = new Colorf(47, 79, 79);

	public static readonly Colorf StandardBeige = new Colorf(0.75f, 0.75f, 0.5f);

	public static readonly Colorf SelectionGold = new Colorf(1f, 0.6f, 0.05f);

	public static readonly Colorf PivotYellow = new Colorf(1f, 1f, 0.05f);

	public float this[int key]
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

	public Colorf(float greylevel, float a = 1f)
	{
		r = (g = (b = greylevel));
		this.a = a;
	}

	public Colorf(float r, float g, float b, float a = 1f)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public Colorf(int r, int g, int b, int a = 255)
	{
		this.r = MathUtil.Clamp(r, 0f, 255f) / 255f;
		this.g = MathUtil.Clamp(g, 0f, 255f) / 255f;
		this.b = MathUtil.Clamp(b, 0f, 255f) / 255f;
		this.a = MathUtil.Clamp(a, 0f, 255f) / 255f;
	}

	public Colorf(float[] v2)
	{
		r = v2[0];
		g = v2[1];
		b = v2[2];
		a = v2[3];
	}

	public Colorf(Colorf copy)
	{
		r = copy.r;
		g = copy.g;
		b = copy.b;
		a = copy.a;
	}

	public Colorf(Colorf copy, float newAlpha)
	{
		r = copy.r;
		g = copy.g;
		b = copy.b;
		a = newAlpha;
	}

	public Colorf Clone(float fAlphaMultiply = 1f)
	{
		return new Colorf(r, g, b, a * fAlphaMultiply);
	}

	public float SqrDistance(Colorf v2)
	{
		float num = r - v2.r;
		float num2 = g - v2.g;
		float num3 = num2 - v2.b;
		float num4 = num - v2.a;
		return num * num + num2 * num2 + num3 * num3 + num4 * num4;
	}

	public Vector3f ToRGB()
	{
		return new Vector3f(r, g, b);
	}

	public Colorb ToBytes()
	{
		return new Colorb(r, g, b, a);
	}

	public void Set(Colorf o)
	{
		r = o.r;
		g = o.g;
		b = o.b;
		a = o.a;
	}

	public void Set(float fR, float fG, float fB, float fA)
	{
		r = fR;
		g = fG;
		b = fB;
		a = fA;
	}

	public Colorf SetAlpha(float a)
	{
		this.a = a;
		return this;
	}

	public void Add(Colorf o)
	{
		r += o.r;
		g += o.g;
		b += o.b;
		a += o.a;
	}

	public void Subtract(Colorf o)
	{
		r -= o.r;
		g -= o.g;
		b -= o.b;
		a -= o.a;
	}

	public Colorf WithAlpha(float newAlpha)
	{
		return new Colorf(r, g, b, newAlpha);
	}

	public static Colorf operator -(Colorf v)
	{
		return new Colorf(0f - v.r, 0f - v.g, 0f - v.b, 0f - v.a);
	}

	public static Colorf operator *(float f, Colorf v)
	{
		return new Colorf(f * v.r, f * v.g, f * v.b, f * v.a);
	}

	public static Colorf operator *(Colorf v, float f)
	{
		return new Colorf(f * v.r, f * v.g, f * v.b, f * v.a);
	}

	public static Colorf operator +(Colorf v0, Colorf v1)
	{
		return new Colorf(v0.r + v1.r, v0.g + v1.g, v0.b + v1.b, v0.a + v1.a);
	}

	public static Colorf operator +(Colorf v0, float f)
	{
		return new Colorf(v0.r + f, v0.g + f, v0.b + f, v0.a + f);
	}

	public static Colorf operator -(Colorf v0, Colorf v1)
	{
		return new Colorf(v0.r - v1.r, v0.g - v1.g, v0.b - v1.b, v0.a - v1.a);
	}

	public static Colorf operator -(Colorf v0, float f)
	{
		return new Colorf(v0.r - f, v0.g - f, v0.b - f, v0.a = f);
	}

	public static bool operator ==(Colorf a, Colorf b)
	{
		if (a.r == b.r && a.g == b.g && a.b == b.b)
		{
			return a.a == b.a;
		}
		return false;
	}

	public static bool operator !=(Colorf a, Colorf b)
	{
		if (a.r == b.r && a.g == b.g && a.b == b.b)
		{
			return a.a != b.a;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Colorf)obj;
	}

	public override int GetHashCode()
	{
		return (r + g + b + a).GetHashCode();
	}

	public int CompareTo(Colorf other)
	{
		if (r != other.r)
		{
			if (!(r < other.r))
			{
				return 1;
			}
			return -1;
		}
		if (g != other.g)
		{
			if (!(g < other.g))
			{
				return 1;
			}
			return -1;
		}
		if (b != other.b)
		{
			if (!(b < other.b))
			{
				return 1;
			}
			return -1;
		}
		if (a != other.a)
		{
			if (!(a < other.a))
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Colorf other)
	{
		if (r == other.r && g == other.g && b == other.b)
		{
			return a == other.a;
		}
		return false;
	}

	public static Colorf Lerp(Colorf a, Colorf b, float t)
	{
		float num = 1f - t;
		return new Colorf(num * a.r + t * b.r, num * a.g + t * b.g, num * a.b + t * b.b, num * a.a + t * b.a);
	}

	public override string ToString()
	{
		return $"{r:F8} {g:F8} {b:F8} {a:F8}";
	}

	public string ToString(string fmt)
	{
		return $"{r.ToString(fmt)} {g.ToString(fmt)} {b.ToString(fmt)} {a.ToString(fmt)}";
	}

	public static implicit operator Vector3f(Colorf c)
	{
		return new Vector3f(c.r, c.g, c.b);
	}

	public static implicit operator Colorf(Vector3f c)
	{
		return new Colorf(c.x, c.y, c.z);
	}

	public static implicit operator Colorf(Color c)
	{
		return new Colorf(c.r, c.g, c.b, c.a);
	}

	public static implicit operator Color(Colorf c)
	{
		return new Color(c.r, c.g, c.b, c.a);
	}

	public static implicit operator Color32(Colorf c)
	{
		Colorb colorb = c.ToBytes();
		return new Color32(colorb.r, colorb.g, colorb.b, colorb.a);
	}
}
