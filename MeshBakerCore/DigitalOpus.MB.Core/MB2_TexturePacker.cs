using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public abstract class MB2_TexturePacker
{
	internal enum NodeType
	{
		Container,
		maxDim,
		regular
	}

	internal class PixRect
	{
		public int x;

		public int y;

		public int w;

		public int h;

		public PixRect()
		{
		}

		public PixRect(int xx, int yy, int ww, int hh)
		{
			x = xx;
			y = yy;
			w = ww;
			h = hh;
		}

		public override string ToString()
		{
			return $"x={x},y={y},w={w},h={h}";
		}
	}

	internal class Image
	{
		public int imgId;

		public int w;

		public int h;

		public int x;

		public int y;

		public Image(int id, int tw, int th, AtlasPadding padding, int minImageSizeX, int minImageSizeY)
		{
			imgId = id;
			w = Mathf.Max(tw + padding.leftRight * 2, minImageSizeX);
			h = Mathf.Max(th + padding.topBottom * 2, minImageSizeY);
		}

		public Image(Image im)
		{
			imgId = im.imgId;
			w = im.w;
			h = im.h;
			x = im.x;
			y = im.y;
		}
	}

	internal class ImgIDComparer : IComparer<Image>
	{
		public int Compare(Image x, Image y)
		{
			if (x.imgId > y.imgId)
			{
				return 1;
			}
			if (x.imgId == y.imgId)
			{
				return 0;
			}
			return -1;
		}
	}

	internal class ImageHeightComparer : IComparer<Image>
	{
		public int Compare(Image x, Image y)
		{
			if (x.h > y.h)
			{
				return -1;
			}
			if (x.h == y.h)
			{
				return 0;
			}
			return 1;
		}
	}

	internal class ImageWidthComparer : IComparer<Image>
	{
		public int Compare(Image x, Image y)
		{
			if (x.w > y.w)
			{
				return -1;
			}
			if (x.w == y.w)
			{
				return 0;
			}
			return 1;
		}
	}

	internal class ImageAreaComparer : IComparer<Image>
	{
		public int Compare(Image x, Image y)
		{
			int num = x.w * x.h;
			int num2 = y.w * y.h;
			if (num > num2)
			{
				return -1;
			}
			if (num == num2)
			{
				return 0;
			}
			return 1;
		}
	}

	public const int MAX_ATLAS_SIZE = 8192;

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	internal const int MAX_RECURSION_DEPTH = 10;

	public bool atlasMustBePowerOfTwo = true;

	public static int RoundToNearestPositivePowerOfTwo(int x)
	{
		int num = (int)Mathf.Pow(2f, Mathf.RoundToInt(Mathf.Log(x) / Mathf.Log(2f)));
		if (num == 0 || num == 1)
		{
			num = 2;
		}
		return num;
	}

	public static int CeilToNearestPowerOfTwo(int x)
	{
		int num = (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
		if (num == 0 || num == 1)
		{
			num = 2;
		}
		return num;
	}

	public abstract AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, int maxDimensionX, int maxDimensionY, int padding);

	public abstract AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, List<AtlasPadding> paddings, int maxDimensionX, int maxDimensionY, bool doMultiAtlas);

	internal bool ScaleAtlasToFitMaxDim(Vector2 rootWH, List<Image> images, int maxDimensionX, int maxDimensionY, AtlasPadding padding, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY, ref int outW, ref int outH, out float padX, out float padY, out int newMinSizeX, out int newMinSizeY)
	{
		newMinSizeX = minImageSizeX;
		newMinSizeY = minImageSizeY;
		bool result = false;
		padX = (float)padding.leftRight / (float)outW;
		if (rootWH.x > (float)maxDimensionX)
		{
			padX = (float)padding.leftRight / (float)maxDimensionX;
			float num = (float)maxDimensionX / rootWH.x;
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Packing exceeded atlas width shrinking to " + num);
			}
			for (int i = 0; i < images.Count; i++)
			{
				Image image = images[i];
				if ((float)image.w * num < (float)masterImageSizeX)
				{
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						Debug.Log("Small images are being scaled to zero. Will need to redo packing with larger minTexSizeX.");
					}
					result = true;
					newMinSizeX = Mathf.CeilToInt((float)minImageSizeX / num);
				}
				int num2 = (int)((float)(image.x + image.w) * num);
				image.x = (int)(num * (float)image.x);
				image.w = num2 - image.x;
			}
			outW = maxDimensionX;
		}
		padY = (float)padding.topBottom / (float)outH;
		if (rootWH.y > (float)maxDimensionY)
		{
			padY = (float)padding.topBottom / (float)maxDimensionY;
			float num3 = (float)maxDimensionY / rootWH.y;
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				Debug.LogWarning("Packing exceeded atlas height shrinking to " + num3);
			}
			for (int j = 0; j < images.Count; j++)
			{
				Image image2 = images[j];
				if ((float)image2.h * num3 < (float)masterImageSizeY)
				{
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						Debug.Log("Small images are being scaled to zero. Will need to redo packing with larger minTexSizeY.");
					}
					result = true;
					newMinSizeY = Mathf.CeilToInt((float)minImageSizeY / num3);
				}
				int num4 = (int)((float)(image2.y + image2.h) * num3);
				image2.y = (int)(num3 * (float)image2.y);
				image2.h = num4 - image2.y;
			}
			outH = maxDimensionY;
		}
		return result;
	}

	public void ConvertToRectsWithoutPaddingAndNormalize01(AtlasPackingResult rr, AtlasPadding padding)
	{
		for (int i = 0; i < rr.rects.Length; i++)
		{
			rr.rects[i].x = (rr.rects[i].x + (float)padding.leftRight) / (float)rr.atlasX;
			rr.rects[i].y = (rr.rects[i].y + (float)padding.topBottom) / (float)rr.atlasY;
			rr.rects[i].width = (rr.rects[i].width - (float)(padding.leftRight * 2)) / (float)rr.atlasX;
			rr.rects[i].height = (rr.rects[i].height - (float)(padding.topBottom * 2)) / (float)rr.atlasY;
		}
	}
}
