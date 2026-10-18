using System;

namespace UnityEngine.ProBuilder;

internal static class Clipping
{
	[Flags]
	private enum OutCode
	{
		Inside = 0,
		Left = 1,
		Right = 2,
		Bottom = 4,
		Top = 8
	}

	private static OutCode ComputeOutCode(Rect rect, float x, float y)
	{
		OutCode outCode = OutCode.Inside;
		if (x < rect.xMin)
		{
			outCode |= OutCode.Left;
		}
		else if (x > rect.xMax)
		{
			outCode |= OutCode.Right;
		}
		if (y < rect.yMin)
		{
			outCode |= OutCode.Bottom;
		}
		else if (y > rect.yMax)
		{
			outCode |= OutCode.Top;
		}
		return outCode;
	}

	internal static bool RectContainsLineSegment(Rect rect, float x0, float y0, float x1, float y1)
	{
		OutCode outCode = ComputeOutCode(rect, x0, y0);
		OutCode outCode2 = ComputeOutCode(rect, x1, y1);
		bool result = false;
		while (true)
		{
			if ((outCode | outCode2) == OutCode.Inside)
			{
				result = true;
				break;
			}
			if ((outCode & outCode2) != 0)
			{
				break;
			}
			float num = 0f;
			float num2 = 0f;
			OutCode outCode3 = ((outCode != 0) ? outCode : outCode2);
			if ((outCode3 & OutCode.Top) == OutCode.Top)
			{
				num = x0 + (x1 - x0) * (rect.yMax - y0) / (y1 - y0);
				num2 = rect.yMax;
			}
			else if ((outCode3 & OutCode.Bottom) == OutCode.Bottom)
			{
				num = x0 + (x1 - x0) * (rect.yMin - y0) / (y1 - y0);
				num2 = rect.yMin;
			}
			else if ((outCode3 & OutCode.Right) == OutCode.Right)
			{
				num2 = y0 + (y1 - y0) * (rect.xMax - x0) / (x1 - x0);
				num = rect.xMax;
			}
			else if ((outCode3 & OutCode.Left) == OutCode.Left)
			{
				num2 = y0 + (y1 - y0) * (rect.xMin - x0) / (x1 - x0);
				num = rect.xMin;
			}
			if (outCode3 == outCode)
			{
				x0 = num;
				y0 = num2;
				outCode = ComputeOutCode(rect, x0, y0);
			}
			else
			{
				x1 = num;
				y1 = num2;
				outCode2 = ComputeOutCode(rect, x1, y1);
			}
		}
		return result;
	}
}
