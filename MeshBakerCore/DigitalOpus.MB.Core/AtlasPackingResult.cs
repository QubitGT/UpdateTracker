using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[Serializable]
public class AtlasPackingResult
{
	public int atlasX;

	public int atlasY;

	public int usedW;

	public int usedH;

	public Rect[] rects;

	public AtlasPadding[] padding;

	public int[] srcImgIdxs;

	public object data;

	public AtlasPackingResult(AtlasPadding[] pds)
	{
		padding = pds;
	}

	public void CalcUsedWidthAndHeight()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < rects.Length; i++)
		{
			num3 += (float)padding[i].leftRight * 2f;
			num4 += (float)padding[i].topBottom * 2f;
			num = Mathf.Max(num, rects[i].x + rects[i].width);
			num2 = Mathf.Max(num2, rects[i].y + rects[i].height);
		}
		usedW = Mathf.CeilToInt(num * (float)atlasX + num3);
		usedH = Mathf.CeilToInt(num2 * (float)atlasY + num4);
		if (usedW > atlasX)
		{
			usedW = atlasX;
		}
		if (usedH > atlasY)
		{
			usedH = atlasY;
		}
	}

	public override string ToString()
	{
		return $"numRects: {rects.Length}, atlasX: {atlasX} atlasY: {atlasY} usedW: {usedW} usedH: {usedH}";
	}
}
