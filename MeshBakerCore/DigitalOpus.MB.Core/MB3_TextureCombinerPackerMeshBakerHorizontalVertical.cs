using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

internal class MB3_TextureCombinerPackerMeshBakerHorizontalVertical : MB3_TextureCombinerPackerMeshBaker
{
	private interface IPipeline
	{
		MB2_PackingAlgorithmEnum GetPackingAlg();

		void SortTexSetIntoBins(MB_TexSet texSet, List<MB_TexSet> horizontalVert, List<MB_TexSet> regular, int maxAtlasWidth, int maxAtlasHeight);

		MB_TextureTilingTreatment GetEdge2EdgeTreatment();

		void InitializeAtlasPadding(ref AtlasPadding padding, int paddingValue);

		void MergeAtlasPackingResultStackBonAInternal(AtlasPackingResult a, AtlasPackingResult b, out Rect AatlasToFinal, out Rect BatlasToFinal, bool stretchBToAtlasWidth, int maxWidthDim, int maxHeightDim, out int atlasX, out int atlasY);

		void GetExtraRoomForRegularAtlas(int usedHorizontalVertWidth, int usedHorizontalVertHeight, int maxAtlasWidth, int maxAtlasHeight, out int atlasRegularMaxWidth, out int atlasRegularMaxHeight);
	}

	private class VerticalPipeline : IPipeline
	{
		public MB2_PackingAlgorithmEnum GetPackingAlg()
		{
			return MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical;
		}

		public void SortTexSetIntoBins(MB_TexSet texSet, List<MB_TexSet> horizontalVert, List<MB_TexSet> regular, int maxAtlasWidth, int maxAtlasHeight)
		{
			if (texSet.idealHeight_pix >= maxAtlasHeight && texSet.ts[0].GetEncapsulatingSamplingRect().height >= 1.0)
			{
				horizontalVert.Add(texSet);
			}
			else
			{
				regular.Add(texSet);
			}
		}

		public MB_TextureTilingTreatment GetEdge2EdgeTreatment()
		{
			return MB_TextureTilingTreatment.edgeToEdgeY;
		}

		public void InitializeAtlasPadding(ref AtlasPadding padding, int paddingValue)
		{
			padding.topBottom = 0;
			padding.leftRight = paddingValue;
		}

		public void MergeAtlasPackingResultStackBonAInternal(AtlasPackingResult a, AtlasPackingResult b, out Rect AatlasToFinal, out Rect BatlasToFinal, bool stretchBToAtlasWidth, int maxWidthDim, int maxHeightDim, out int atlasX, out int atlasY)
		{
			float num = a.usedW + b.usedW;
			if (num > (float)maxWidthDim)
			{
				float num2 = (float)maxWidthDim / num;
				float num3 = (float)Mathf.FloorToInt((float)a.usedW * num2) / (float)maxWidthDim;
				num2 = num3;
				float width = 1f - num3;
				AatlasToFinal = new Rect(0f, 0f, num2, 1f);
				BatlasToFinal = new Rect(num3, 0f, width, 1f);
			}
			else
			{
				float num4 = (float)a.usedW / num;
				AatlasToFinal = new Rect(0f, 0f, num4, 1f);
				BatlasToFinal = new Rect(num4, 0f, (float)b.usedW / num, 1f);
			}
			if (a.atlasX > b.atlasX)
			{
				if (!stretchBToAtlasWidth)
				{
					BatlasToFinal.width = (float)b.atlasX / (float)a.atlasX;
				}
			}
			else if (b.atlasX > a.atlasX)
			{
				AatlasToFinal.width = (float)a.atlasX / (float)b.atlasX;
			}
			atlasX = a.usedW + b.usedW;
			atlasY = Mathf.Max(a.usedH, b.usedH);
		}

		public void GetExtraRoomForRegularAtlas(int usedHorizontalVertWidth, int usedHorizontalVertHeight, int maxAtlasWidth, int maxAtlasHeight, out int atlasRegularMaxWidth, out int atlasRegularMaxHeight)
		{
			atlasRegularMaxWidth = maxAtlasWidth - usedHorizontalVertWidth;
			atlasRegularMaxHeight = maxAtlasHeight;
		}
	}

	private class HorizontalPipeline : IPipeline
	{
		public MB2_PackingAlgorithmEnum GetPackingAlg()
		{
			return MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal;
		}

		public void SortTexSetIntoBins(MB_TexSet texSet, List<MB_TexSet> horizontalVert, List<MB_TexSet> regular, int maxAtlasWidth, int maxAtlasHeight)
		{
			if (texSet.idealWidth_pix >= maxAtlasWidth && texSet.ts[0].GetEncapsulatingSamplingRect().width >= 1.0)
			{
				horizontalVert.Add(texSet);
			}
			else
			{
				regular.Add(texSet);
			}
		}

		public MB_TextureTilingTreatment GetEdge2EdgeTreatment()
		{
			return MB_TextureTilingTreatment.edgeToEdgeX;
		}

		public void InitializeAtlasPadding(ref AtlasPadding padding, int paddingValue)
		{
			padding.topBottom = paddingValue;
			padding.leftRight = 0;
		}

		public void MergeAtlasPackingResultStackBonAInternal(AtlasPackingResult a, AtlasPackingResult b, out Rect AatlasToFinal, out Rect BatlasToFinal, bool stretchBToAtlasWidth, int maxWidthDim, int maxHeightDim, out int atlasX, out int atlasY)
		{
			float num = a.usedH + b.usedH;
			if (num > (float)maxHeightDim)
			{
				float num2 = (float)maxHeightDim / num;
				float num3 = (float)Mathf.FloorToInt((float)a.usedH * num2) / (float)maxHeightDim;
				num2 = num3;
				float height = 1f - num3;
				AatlasToFinal = new Rect(0f, 0f, 1f, num2);
				BatlasToFinal = new Rect(0f, num3, 1f, height);
			}
			else
			{
				float num4 = (float)a.usedH / num;
				AatlasToFinal = new Rect(0f, 0f, 1f, num4);
				BatlasToFinal = new Rect(0f, num4, 1f, (float)b.usedH / num);
			}
			if (a.atlasX > b.atlasX)
			{
				if (!stretchBToAtlasWidth)
				{
					BatlasToFinal.width = (float)b.atlasX / (float)a.atlasX;
				}
			}
			else if (b.atlasX > a.atlasX)
			{
				AatlasToFinal.width = (float)a.atlasX / (float)b.atlasX;
			}
			atlasX = Mathf.Max(a.usedW, b.usedW);
			atlasY = a.usedH + b.usedH;
		}

		public void GetExtraRoomForRegularAtlas(int usedHorizontalVertWidth, int usedHorizontalVertHeight, int maxAtlasWidth, int maxAtlasHeight, out int atlasRegularMaxWidth, out int atlasRegularMaxHeight)
		{
			atlasRegularMaxWidth = maxAtlasWidth;
			atlasRegularMaxHeight = maxAtlasHeight - usedHorizontalVertHeight;
		}
	}

	public enum AtlasDirection
	{
		horizontal,
		vertical
	}

	private AtlasDirection _atlasDirection;

	public MB3_TextureCombinerPackerMeshBakerHorizontalVertical(AtlasDirection ad)
	{
		_atlasDirection = ad;
	}

	public override AtlasPackingResult[] CalculateAtlasRectangles(MB3_TextureCombinerPipeline.TexturePipelineData data, bool doMultiAtlas, MB2_LogLevel LOG_LEVEL)
	{
		IPipeline pipeline = ((_atlasDirection != 0) ? ((IPipeline)new VerticalPipeline()) : ((IPipeline)new HorizontalPipeline()));
		if (_atlasDirection == AtlasDirection.horizontal)
		{
			if (!data._useMaxAtlasWidthOverride)
			{
				int num = 2;
				for (int i = 0; i < data.distinctMaterialTextures.Count; i++)
				{
					MB_TexSet mB_TexSet = data.distinctMaterialTextures[i];
					int num2 = ((!data._fixOutOfBoundsUVs) ? mB_TexSet.idealWidth_pix : ((int)mB_TexSet.GetMaxRawTextureHeightWidth().x));
					if (mB_TexSet.idealWidth_pix > num)
					{
						num = num2;
					}
				}
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					Debug.Log("Calculated max atlas width: " + num);
				}
				data._maxAtlasWidth = num;
			}
		}
		else if (!data._useMaxAtlasHeightOverride)
		{
			int num3 = 2;
			for (int j = 0; j < data.distinctMaterialTextures.Count; j++)
			{
				MB_TexSet mB_TexSet2 = data.distinctMaterialTextures[j];
				int num4 = ((!data._fixOutOfBoundsUVs) ? mB_TexSet2.idealHeight_pix : ((int)mB_TexSet2.GetMaxRawTextureHeightWidth().y));
				if (mB_TexSet2.idealHeight_pix > num3)
				{
					num3 = num4;
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Calculated max atlas height: " + num3);
			}
			data._maxAtlasHeight = num3;
		}
		List<MB_TexSet> list = new List<MB_TexSet>();
		List<MB_TexSet> list2 = new List<MB_TexSet>();
		for (int k = 0; k < data.distinctMaterialTextures.Count; k++)
		{
			pipeline.SortTexSetIntoBins(data.distinctMaterialTextures[k], list, list2, data._maxAtlasWidth, data._maxAtlasHeight);
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log($"Splitting list of distinctMaterialTextures numHorizontalVertical={list.Count} numRegular={list2.Count} maxAtlasWidth={data._maxAtlasWidth} maxAtlasHeight={data._maxAtlasHeight}");
		}
		AtlasPackingResult[] array;
		if (list.Count > 0)
		{
			MB2_PackingAlgorithmEnum packingAlg = pipeline.GetPackingAlg();
			List<Vector2> list3 = new List<Vector2>();
			for (int l = 0; l < list.Count; l++)
			{
				list[l].SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(pipeline.GetEdge2EdgeTreatment());
				list3.Add(new Vector2(list[l].idealWidth_pix, list[l].idealHeight_pix));
			}
			MB2_TexturePacker mB2_TexturePacker = MB3_TextureCombinerPipeline.CreateTexturePacker(packingAlg);
			mB2_TexturePacker.atlasMustBePowerOfTwo = false;
			List<AtlasPadding> list4 = new List<AtlasPadding>();
			for (int m = 0; m < list3.Count; m++)
			{
				AtlasPadding padding = default(AtlasPadding);
				pipeline.InitializeAtlasPadding(ref padding, data._atlasPadding_pix);
				list4.Add(padding);
			}
			mB2_TexturePacker.LOG_LEVEL = MB2_LogLevel.trace;
			array = mB2_TexturePacker.GetRects(list3, list4, data._maxAtlasWidth, data._maxAtlasHeight, doMultiAtlas: false);
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				Debug.Log($"Packed {list.Count} textures with edgeToEdge tiling into an atlas of size {array[0].atlasX} by {array[0].atlasY} usedW {array[0].usedW} usedH {array[0].usedH}");
			}
		}
		else
		{
			array = new AtlasPackingResult[0];
		}
		AtlasPackingResult[] array2;
		if (list2.Count > 0)
		{
			MB2_PackingAlgorithmEnum packingAlg = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;
			List<Vector2> list5 = new List<Vector2>();
			for (int n = 0; n < list2.Count; n++)
			{
				list5.Add(new Vector2(list2[n].idealWidth_pix, list2[n].idealHeight_pix));
			}
			MB2_TexturePacker mB2_TexturePacker = MB3_TextureCombinerPipeline.CreateTexturePacker(MB2_PackingAlgorithmEnum.MeshBakerTexturePacker);
			mB2_TexturePacker.atlasMustBePowerOfTwo = false;
			List<AtlasPadding> list6 = new List<AtlasPadding>();
			for (int num5 = 0; num5 < list5.Count; num5++)
			{
				AtlasPadding item = default(AtlasPadding);
				item.topBottom = data._atlasPadding_pix;
				item.leftRight = data._atlasPadding_pix;
				list6.Add(item);
			}
			int usedHorizontalVertWidth = 0;
			int usedHorizontalVertHeight = 0;
			if (array.Length != 0)
			{
				usedHorizontalVertHeight = array[0].atlasY;
				usedHorizontalVertWidth = array[0].atlasX;
			}
			pipeline.GetExtraRoomForRegularAtlas(usedHorizontalVertWidth, usedHorizontalVertHeight, data._maxAtlasWidth, data._maxAtlasHeight, out var atlasRegularMaxWidth, out var atlasRegularMaxHeight);
			array2 = mB2_TexturePacker.GetRects(list5, list6, atlasRegularMaxWidth, atlasRegularMaxHeight, doMultiAtlas: false);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log($"Packed {list2.Count} textures without edgeToEdge tiling into an atlas of size {array2[0].atlasX} by {array2[0].atlasY} usedW {array2[0].usedW} usedH {array2[0].usedH}");
			}
		}
		else
		{
			array2 = new AtlasPackingResult[0];
		}
		AtlasPackingResult atlasPackingResult = null;
		if (array.Length == 0 && array2.Length == 0)
		{
			return null;
		}
		if (array.Length != 0 && array2.Length != 0)
		{
			atlasPackingResult = MergeAtlasPackingResultStackBonA(array[0], array2[0], data._maxAtlasWidth, data._maxAtlasHeight, stretchBToAtlasWidth: true, pipeline);
		}
		else if (array.Length != 0)
		{
			atlasPackingResult = array[0];
		}
		else if (array2.Length != 0)
		{
			atlasPackingResult = array2[0];
		}
		list.AddRange(list2);
		data.distinctMaterialTextures = list;
		if (atlasPackingResult != null)
		{
			return new AtlasPackingResult[1] { atlasPackingResult };
		}
		return new AtlasPackingResult[0];
	}

	public static AtlasPackingResult TestStackRectanglesHorizontal(AtlasPackingResult a, AtlasPackingResult b, int maxHeightDim, int maxWidthDim, bool stretchBToAtlasWidth)
	{
		return MergeAtlasPackingResultStackBonA(a, b, maxWidthDim, maxHeightDim, stretchBToAtlasWidth, new HorizontalPipeline());
	}

	public static AtlasPackingResult TestStackRectanglesVertical(AtlasPackingResult a, AtlasPackingResult b, int maxHeightDim, int maxWidthDim, bool stretchBToAtlasWidth)
	{
		return MergeAtlasPackingResultStackBonA(a, b, maxWidthDim, maxHeightDim, stretchBToAtlasWidth, new VerticalPipeline());
	}

	private static AtlasPackingResult MergeAtlasPackingResultStackBonA(AtlasPackingResult a, AtlasPackingResult b, int maxWidthDim, int maxHeightDim, bool stretchBToAtlasWidth, IPipeline pipeline)
	{
		pipeline.MergeAtlasPackingResultStackBonAInternal(a, b, out var AatlasToFinal, out var BatlasToFinal, stretchBToAtlasWidth, maxWidthDim, maxHeightDim, out var atlasX, out var atlasY);
		Rect[] array = new Rect[a.rects.Length + b.rects.Length];
		AtlasPadding[] array2 = new AtlasPadding[a.rects.Length + b.rects.Length];
		int[] array3 = new int[a.rects.Length + b.rects.Length];
		Array.Copy(a.padding, array2, a.padding.Length);
		Array.Copy(b.padding, 0, array2, a.padding.Length, b.padding.Length);
		Array.Copy(a.srcImgIdxs, array3, a.srcImgIdxs.Length);
		Array.Copy(b.srcImgIdxs, 0, array3, a.srcImgIdxs.Length, b.srcImgIdxs.Length);
		Array.Copy(a.rects, array, a.rects.Length);
		for (int i = 0; i < a.rects.Length; i++)
		{
			Rect rect = a.rects[i];
			rect.x = AatlasToFinal.x + rect.x * AatlasToFinal.width;
			rect.y = AatlasToFinal.y + rect.y * AatlasToFinal.height;
			rect.width *= AatlasToFinal.width;
			rect.height *= AatlasToFinal.height;
			array[i] = rect;
			array3[i] = a.srcImgIdxs[i];
		}
		for (int j = 0; j < b.rects.Length; j++)
		{
			Rect rect2 = b.rects[j];
			rect2.x = BatlasToFinal.x + rect2.x * BatlasToFinal.width;
			rect2.y = BatlasToFinal.y + rect2.y * BatlasToFinal.height;
			rect2.width *= BatlasToFinal.width;
			rect2.height *= BatlasToFinal.height;
			array[a.rects.Length + j] = rect2;
			array3[a.rects.Length + j] = b.srcImgIdxs[j];
		}
		AtlasPackingResult atlasPackingResult = new AtlasPackingResult(array2);
		atlasPackingResult.atlasX = atlasX;
		atlasPackingResult.atlasY = atlasY;
		atlasPackingResult.padding = array2;
		atlasPackingResult.rects = array;
		atlasPackingResult.srcImgIdxs = array3;
		atlasPackingResult.CalcUsedWidthAndHeight();
		return atlasPackingResult;
	}
}
