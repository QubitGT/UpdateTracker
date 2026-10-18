using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_UVTransformUtility
{
	public static void Test()
	{
		DRect t = new DRect(0.5, 0.5, 2.0, 2.0);
		DRect t2 = new DRect(0.25, 0.25, 3.0, 3.0);
		DRect r = InverseTransform(ref t);
		DRect r2 = InverseTransform(ref t2);
		DRect r3 = CombineTransforms(ref t, ref r2);
		Debug.Log(r);
		Debug.Log(r3);
		Debug.Log("one mat trans " + TransformPoint(ref t, new Vector2(1f, 1f)).ToString());
		Debug.Log("one inv mat trans " + TransformPoint(ref r, new Vector2(1f, 1f)).ToString("f4"));
		Debug.Log("zero " + TransformPoint(ref r3, new Vector2(0f, 0f)).ToString("f4"));
		Debug.Log("one " + TransformPoint(ref r3, new Vector2(1f, 1f)).ToString("f4"));
	}

	public static float TransformX(DRect r, double x)
	{
		return (float)(r.width * x + r.x);
	}

	public static DRect CombineTransforms(ref DRect r1, ref DRect r2)
	{
		return new DRect(r1.x * r2.width + r2.x, r1.y * r2.height + r2.y, r1.width * r2.width, r1.height * r2.height);
	}

	public static Rect CombineTransforms(ref Rect r1, ref Rect r2)
	{
		return new Rect(r1.x * r2.width + r2.x, r1.y * r2.height + r2.y, r1.width * r2.width, r1.height * r2.height);
	}

	public static DRect InverseTransform(ref DRect t)
	{
		DRect result = default(DRect);
		result.x = (0.0 - t.x) / t.width;
		result.y = (0.0 - t.y) / t.height;
		result.width = 1.0 / t.width;
		result.height = 1.0 / t.height;
		return result;
	}

	public static DRect GetShiftTransformToFitBinA(ref DRect A, ref DRect B)
	{
		DVector2 center = A.center;
		DVector2 center2 = B.center;
		DVector2 dVector = DVector2.Subtract(center, center2);
		double xx = Convert.ToInt32(dVector.x);
		double yy = Convert.ToInt32(dVector.y);
		return new DRect(xx, yy, 1.0, 1.0);
	}

	public static DRect GetEncapsulatingRectShifted(ref DRect uvRect1, ref DRect willBeIn)
	{
		DVector2 center = uvRect1.center;
		DVector2 center2 = willBeIn.center;
		DVector2 dVector = DVector2.Subtract(center, center2);
		double num = Convert.ToInt32(dVector.x);
		double num2 = Convert.ToInt32(dVector.y);
		DRect dRect = new DRect(willBeIn);
		dRect.x += num;
		dRect.y += num2;
		double x = uvRect1.x;
		double y = uvRect1.y;
		double num3 = uvRect1.x + uvRect1.width;
		double num4 = uvRect1.y + uvRect1.height;
		double x2 = dRect.x;
		double y2 = dRect.y;
		double num5 = dRect.x + dRect.width;
		double num6 = dRect.y + dRect.height;
		double num7;
		double num8 = (num7 = x);
		double num9;
		double num10 = (num9 = y);
		if (x2 < num8)
		{
			num8 = x2;
		}
		if (x < num8)
		{
			num8 = x;
		}
		if (y2 < num10)
		{
			num10 = y2;
		}
		if (y < num10)
		{
			num10 = y;
		}
		if (num5 > num7)
		{
			num7 = num5;
		}
		if (num3 > num7)
		{
			num7 = num3;
		}
		if (num6 > num9)
		{
			num9 = num6;
		}
		if (num4 > num9)
		{
			num9 = num4;
		}
		return new DRect(num8, num10, num7 - num8, num9 - num10);
	}

	public static DRect GetEncapsulatingRect(ref DRect uvRect1, ref DRect uvRect2)
	{
		double x = uvRect1.x;
		double y = uvRect1.y;
		double num = uvRect1.x + uvRect1.width;
		double num2 = uvRect1.y + uvRect1.height;
		double x2 = uvRect2.x;
		double y2 = uvRect2.y;
		double num3 = uvRect2.x + uvRect2.width;
		double num4 = uvRect2.y + uvRect2.height;
		double num5;
		double num6 = (num5 = x);
		double num7;
		double num8 = (num7 = y);
		if (x2 < num6)
		{
			num6 = x2;
		}
		if (x < num6)
		{
			num6 = x;
		}
		if (y2 < num8)
		{
			num8 = y2;
		}
		if (y < num8)
		{
			num8 = y;
		}
		if (num3 > num5)
		{
			num5 = num3;
		}
		if (num > num5)
		{
			num5 = num;
		}
		if (num4 > num7)
		{
			num7 = num4;
		}
		if (num2 > num7)
		{
			num7 = num2;
		}
		return new DRect(num6, num8, num5 - num6, num7 - num8);
	}

	public static bool RectContainsShifted(ref DRect bucket, ref DRect tryFit)
	{
		DVector2 center = bucket.center;
		DVector2 center2 = tryFit.center;
		DVector2 dVector = DVector2.Subtract(center, center2);
		double num = Convert.ToInt32(dVector.x);
		double num2 = Convert.ToInt32(dVector.y);
		DRect smallToTestIfFits = new DRect(tryFit);
		smallToTestIfFits.x += num;
		smallToTestIfFits.y += num2;
		return bucket.Encloses(smallToTestIfFits);
	}

	public static bool RectContainsShifted(ref Rect bucket, ref Rect tryFit)
	{
		Vector2 center = bucket.center;
		Vector2 center2 = tryFit.center;
		Vector2 vector = center - center2;
		float num = Convert.ToInt32(vector.x);
		float num2 = Convert.ToInt32(vector.y);
		Rect smallToTestIfFits = new Rect(tryFit);
		smallToTestIfFits.x += num;
		smallToTestIfFits.y += num2;
		return RectContains(ref bucket, ref smallToTestIfFits);
	}

	public static bool LineSegmentContainsShifted(float bucketOffset, float bucketLength, float tryFitOffset, float tryFitLength)
	{
		float num = bucketOffset + bucketLength / 2f;
		float num2 = tryFitOffset + tryFitLength / 2f;
		float num3 = Convert.ToInt32(num - num2);
		tryFitOffset += num3;
		float num4 = tryFitOffset;
		float num5 = tryFitOffset + tryFitLength;
		float num6 = bucketOffset - 0.01f;
		float num7 = bucketOffset + bucketLength + 0.01f;
		if (num6 <= num4 && num4 <= num7 && num6 <= num5)
		{
			return num5 <= num7;
		}
		return false;
	}

	public static bool RectContains(ref DRect bigRect, ref DRect smallToTestIfFits)
	{
		double x = smallToTestIfFits.x;
		double y = smallToTestIfFits.y;
		double num = smallToTestIfFits.x + smallToTestIfFits.width;
		double num2 = smallToTestIfFits.y + smallToTestIfFits.height;
		double num3 = bigRect.x - 0.009999999776482582;
		double num4 = bigRect.y - 0.009999999776482582;
		double num5 = bigRect.x + bigRect.width + 0.009999999776482582;
		double num6 = bigRect.y + bigRect.height + 0.009999999776482582;
		if (num3 <= x && x <= num5 && num3 <= num && num <= num5 && num4 <= y && y <= num6 && num4 <= num2)
		{
			return num2 <= num6;
		}
		return false;
	}

	public static bool RectContains(ref Rect bigRect, ref Rect smallToTestIfFits)
	{
		float x = smallToTestIfFits.x;
		float y = smallToTestIfFits.y;
		float num = smallToTestIfFits.x + smallToTestIfFits.width;
		float num2 = smallToTestIfFits.y + smallToTestIfFits.height;
		float num3 = bigRect.x - 0.01f;
		float num4 = bigRect.y - 0.01f;
		float num5 = bigRect.x + bigRect.width + 0.01f;
		float num6 = bigRect.y + bigRect.height + 0.01f;
		if (num3 <= x && x <= num5 && num3 <= num && num <= num5 && num4 <= y && y <= num6 && num4 <= num2)
		{
			return num2 <= num6;
		}
		return false;
	}

	public static Vector2 TransformPoint(ref DRect r, Vector2 p)
	{
		return new Vector2((float)(r.width * (double)p.x + r.x), (float)(r.height * (double)p.y + r.y));
	}

	public static DVector2 TransformPoint(ref DRect r, DVector2 p)
	{
		return new DVector2(r.width * p.x + r.x, r.height * p.y + r.y);
	}
}
