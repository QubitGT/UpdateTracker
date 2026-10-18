using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB2_TexturePackerHorizontalVert : MB2_TexturePacker
{
	public enum TexturePackingOrientation
	{
		horizontal,
		vertical
	}

	public TexturePackingOrientation packingOrientation;

	public bool stretchImagesToEdges = true;

	public override AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, int maxDimensionX, int maxDimensionY, int padding)
	{
		List<AtlasPadding> list = new List<AtlasPadding>();
		for (int i = 0; i < imgWidthHeights.Count; i++)
		{
			AtlasPadding item = default(AtlasPadding);
			if (packingOrientation == TexturePackingOrientation.horizontal)
			{
				item.leftRight = 0;
				item.topBottom = 8;
			}
			else
			{
				item.leftRight = 8;
				item.topBottom = 0;
			}
			list.Add(item);
		}
		return GetRects(imgWidthHeights, list, maxDimensionX, maxDimensionY, doMultiAtlas: false);
	}

	public override AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, List<AtlasPadding> paddings, int maxDimensionX, int maxDimensionY, bool doMultiAtlas)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < paddings.Count; i++)
		{
			num = Mathf.Max(num, paddings[i].leftRight);
			num2 = Mathf.Max(num2, paddings[i].topBottom);
		}
		if (doMultiAtlas)
		{
			if (packingOrientation == TexturePackingOrientation.vertical)
			{
				return _GetRectsMultiAtlasVertical(imgWidthHeights, paddings, maxDimensionX, maxDimensionY, 2 + num * 2, 2 + num2 * 2, 2 + num * 2, 2 + num2 * 2);
			}
			return _GetRectsMultiAtlasHorizontal(imgWidthHeights, paddings, maxDimensionX, maxDimensionY, 2 + num * 2, 2 + num2 * 2, 2 + num * 2, 2 + num2 * 2);
		}
		AtlasPackingResult atlasPackingResult = _GetRectsSingleAtlas(imgWidthHeights, paddings, maxDimensionX, maxDimensionY, 2 + num * 2, 2 + num2 * 2, 2 + num * 2, 2 + num2 * 2, 0);
		if (atlasPackingResult == null)
		{
			return null;
		}
		return new AtlasPackingResult[1] { atlasPackingResult };
	}

	private AtlasPackingResult _GetRectsSingleAtlas(List<Vector2> imgWidthHeights, List<AtlasPadding> paddings, int maxDimensionX, int maxDimensionY, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY, int recursionDepth)
	{
		AtlasPackingResult atlasPackingResult = new AtlasPackingResult(paddings.ToArray());
		List<Rect> list = new List<Rect>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<Image> list2 = new List<Image>();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Packing rects for: " + imgWidthHeights.Count);
		}
		for (int i = 0; i < imgWidthHeights.Count; i++)
		{
			Image image = new Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, paddings[i], minImageSizeX, minImageSizeY);
			if (packingOrientation == TexturePackingOrientation.vertical)
			{
				image.h -= paddings[i].topBottom * 2;
				image.x = num;
				image.y = 0;
				list.Add(new Rect(image.w, image.h, num, 0f));
				num += image.w;
				num2 = Mathf.Max(num2, image.h);
			}
			else
			{
				image.w -= paddings[i].leftRight * 2;
				image.y = num;
				image.x = 0;
				list.Add(new Rect(image.w, image.h, 0f, num));
				num += image.h;
				num3 = Mathf.Max(num3, image.w);
			}
			list2.Add(image);
		}
		Vector2 rootWH = ((packingOrientation != TexturePackingOrientation.vertical) ? new Vector2(num3, num) : new Vector2(num, num2));
		int outW = (int)rootWH.x;
		int outH = (int)rootWH.y;
		if (packingOrientation != TexturePackingOrientation.vertical)
		{
			outH = ((!atlasMustBePowerOfTwo) ? Mathf.Min(outH, maxDimensionY) : Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(outH), maxDimensionY));
		}
		else
		{
			outW = ((!atlasMustBePowerOfTwo) ? Mathf.Min(outW, maxDimensionX) : Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(outW), maxDimensionX));
		}
		if (!ScaleAtlasToFitMaxDim(rootWH, list2, maxDimensionX, maxDimensionY, paddings[0], minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY, ref outW, ref outH, out var padX, out var padY, out var _, out var _))
		{
			atlasPackingResult = new AtlasPackingResult(paddings.ToArray());
			atlasPackingResult.rects = new Rect[list2.Count];
			atlasPackingResult.srcImgIdxs = new int[list2.Count];
			atlasPackingResult.atlasX = outW;
			atlasPackingResult.atlasY = outH;
			for (int j = 0; j < list2.Count; j++)
			{
				Image image2 = list2[j];
				Rect rect = ((packingOrientation != TexturePackingOrientation.vertical) ? (atlasPackingResult.rects[j] = new Rect((float)image2.x / (float)outW, (float)image2.y / (float)outH + padY, stretchImagesToEdges ? 1f : ((float)image2.w / (float)outW), (float)image2.h / (float)outH - padY * 2f)) : (atlasPackingResult.rects[j] = new Rect((float)image2.x / (float)outW + padX, (float)image2.y / (float)outH, (float)image2.w / (float)outW - padX * 2f, stretchImagesToEdges ? 1f : ((float)image2.h / (float)outH))));
				atlasPackingResult.srcImgIdxs[j] = image2.imgId;
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Image: " + j + " imgID=" + image2.imgId + " x=" + rect.x * (float)outW + " y=" + rect.y * (float)outH + " w=" + rect.width * (float)outW + " h=" + rect.height * (float)outH + " padding=" + paddings[j].ToString() + " outW=" + outW + " outH=" + outH);
				}
			}
			atlasPackingResult.CalcUsedWidthAndHeight();
			return atlasPackingResult;
		}
		Debug.Log("Packing failed returning null atlas result");
		return null;
	}

	private AtlasPackingResult[] _GetRectsMultiAtlasVertical(List<Vector2> imgWidthHeights, List<AtlasPadding> paddings, int maxDimensionPassedX, int maxDimensionPassedY, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY)
	{
		List<AtlasPackingResult> list = new List<AtlasPackingResult>();
		int num = 0;
		int num2 = 0;
		int atlasX = 0;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Packing rects for: " + imgWidthHeights.Count);
		}
		List<Image> list2 = new List<Image>();
		for (int i = 0; i < imgWidthHeights.Count; i++)
		{
			Image image = new Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, paddings[i], minImageSizeX, minImageSizeY);
			image.h -= paddings[i].topBottom * 2;
			list2.Add(image);
		}
		list2.Sort(new ImageWidthComparer());
		List<Image> list3 = new List<Image>();
		List<Rect> list4 = new List<Rect>();
		int spaceRemaining = maxDimensionPassedX;
		while (list2.Count > 0 || list3.Count > 0)
		{
			Image image2 = PopLargestThatFits(list2, spaceRemaining, maxDimensionPassedX, list3.Count == 0);
			if (image2 == null)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Atlas filled creating a new atlas ");
				}
				AtlasPackingResult atlasPackingResult = new AtlasPackingResult(paddings.ToArray());
				atlasPackingResult.atlasX = atlasX;
				atlasPackingResult.atlasY = num2;
				Rect[] array = new Rect[list3.Count];
				int[] array2 = new int[list3.Count];
				for (int j = 0; j < list3.Count; j++)
				{
					Rect rect = new Rect(list3[j].x, list3[j].y, list3[j].w, stretchImagesToEdges ? num2 : list3[j].h);
					array[j] = rect;
					array2[j] = list3[j].imgId;
				}
				atlasPackingResult.rects = array;
				atlasPackingResult.srcImgIdxs = array2;
				atlasPackingResult.CalcUsedWidthAndHeight();
				list3.Clear();
				list4.Clear();
				num = 0;
				num2 = 0;
				list.Add(atlasPackingResult);
				spaceRemaining = maxDimensionPassedX;
			}
			else
			{
				image2.x = num;
				image2.y = 0;
				list3.Add(image2);
				list4.Add(new Rect(num, 0f, image2.w, image2.h));
				num += image2.w;
				num2 = Mathf.Max(num2, image2.h);
				atlasX = num;
				spaceRemaining = maxDimensionPassedX - num;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			int atlasX2 = list[k].atlasX;
			int outH = Mathf.Min(list[k].atlasY, maxDimensionPassedY);
			atlasX2 = ((!atlasMustBePowerOfTwo) ? Mathf.Min(atlasX2, maxDimensionPassedX) : Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(atlasX2), maxDimensionPassedX));
			list[k].atlasX = atlasX2;
			ScaleAtlasToFitMaxDim(new Vector2(list[k].atlasX, list[k].atlasY), list3, maxDimensionPassedX, maxDimensionPassedY, paddings[0], minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY, ref atlasX2, ref outH, out var _, out var _, out var _, out var _);
		}
		for (int l = 0; l < list.Count; l++)
		{
			ConvertToRectsWithoutPaddingAndNormalize01(list[l], paddings[l]);
			list[l].CalcUsedWidthAndHeight();
		}
		return list.ToArray();
	}

	private AtlasPackingResult[] _GetRectsMultiAtlasHorizontal(List<Vector2> imgWidthHeights, List<AtlasPadding> paddings, int maxDimensionPassedX, int maxDimensionPassedY, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY)
	{
		List<AtlasPackingResult> list = new List<AtlasPackingResult>();
		int num = 0;
		int atlasY = 0;
		int num2 = 0;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Packing rects for: " + imgWidthHeights.Count);
		}
		List<Image> list2 = new List<Image>();
		for (int i = 0; i < imgWidthHeights.Count; i++)
		{
			Image image = new Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, paddings[i], minImageSizeX, minImageSizeY);
			image.w -= paddings[i].leftRight * 2;
			list2.Add(image);
		}
		list2.Sort(new ImageHeightComparer());
		List<Image> list3 = new List<Image>();
		List<Rect> list4 = new List<Rect>();
		int spaceRemaining = maxDimensionPassedY;
		while (list2.Count > 0 || list3.Count > 0)
		{
			Image image2 = PopLargestThatFits(list2, spaceRemaining, maxDimensionPassedY, list3.Count == 0);
			if (image2 == null)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Atlas filled creating a new atlas ");
				}
				AtlasPackingResult atlasPackingResult = new AtlasPackingResult(paddings.ToArray());
				atlasPackingResult.atlasX = num2;
				atlasPackingResult.atlasY = atlasY;
				Rect[] array = new Rect[list3.Count];
				int[] array2 = new int[list3.Count];
				for (int j = 0; j < list3.Count; j++)
				{
					Rect rect = new Rect(list3[j].x, list3[j].y, stretchImagesToEdges ? num2 : list3[j].w, list3[j].h);
					array[j] = rect;
					array2[j] = list3[j].imgId;
				}
				atlasPackingResult.rects = array;
				atlasPackingResult.srcImgIdxs = array2;
				list3.Clear();
				list4.Clear();
				num = 0;
				atlasY = 0;
				list.Add(atlasPackingResult);
				spaceRemaining = maxDimensionPassedY;
			}
			else
			{
				image2.x = 0;
				image2.y = num;
				list3.Add(image2);
				list4.Add(new Rect(0f, num, image2.w, image2.h));
				num += image2.h;
				num2 = Mathf.Max(num2, image2.w);
				atlasY = num;
				spaceRemaining = maxDimensionPassedY - num;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			int atlasY2 = list[k].atlasY;
			int outW = Mathf.Min(list[k].atlasX, maxDimensionPassedX);
			atlasY2 = ((!atlasMustBePowerOfTwo) ? Mathf.Min(atlasY2, maxDimensionPassedY) : Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(atlasY2), maxDimensionPassedY));
			list[k].atlasY = atlasY2;
			ScaleAtlasToFitMaxDim(new Vector2(list[k].atlasX, list[k].atlasY), list3, maxDimensionPassedX, maxDimensionPassedY, paddings[0], minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY, ref outW, ref atlasY2, out var _, out var _, out var _, out var _);
		}
		for (int l = 0; l < list.Count; l++)
		{
			ConvertToRectsWithoutPaddingAndNormalize01(list[l], paddings[l]);
			list[l].CalcUsedWidthAndHeight();
		}
		return list.ToArray();
	}

	private Image PopLargestThatFits(List<Image> images, int spaceRemaining, int maxDim, bool emptyAtlas)
	{
		if (images.Count == 0)
		{
			return null;
		}
		int num = ((packingOrientation != TexturePackingOrientation.vertical) ? images[0].h : images[0].w);
		if (images.Count > 0 && num >= maxDim)
		{
			if (emptyAtlas)
			{
				Image result = images[0];
				images.RemoveAt(0);
				return result;
			}
			return null;
		}
		int i;
		for (i = 0; i < images.Count; i++)
		{
			if (num < spaceRemaining)
			{
				break;
			}
		}
		if (i < images.Count)
		{
			Image result2 = images[i];
			images.RemoveAt(i);
			return result2;
		}
		return null;
	}
}
