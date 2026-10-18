using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB2_TexturePackerRegular : MB2_TexturePacker
{
	private class ProbeResult
	{
		public int w;

		public int h;

		public int outW;

		public int outH;

		public Node root;

		public bool largerOrEqualToMaxDim;

		public float efficiency;

		public float squareness;

		public float totalAtlasArea;

		public int numAtlases;

		public void Set(int ww, int hh, int outw, int outh, Node r, bool fits, float e, float sq)
		{
			w = ww;
			h = hh;
			outW = outw;
			outH = outh;
			root = r;
			largerOrEqualToMaxDim = fits;
			efficiency = e;
			squareness = sq;
		}

		public float GetScore(bool doPowerOfTwoScore)
		{
			float num = (largerOrEqualToMaxDim ? 1f : 0f);
			if (doPowerOfTwoScore)
			{
				return num * 2f + efficiency;
			}
			return squareness + 2f * efficiency + num;
		}

		public void PrintTree()
		{
			printTree(root, "  ");
		}
	}

	internal class Node
	{
		internal NodeType isFullAtlas;

		internal Node[] child = new Node[2];

		internal PixRect r;

		internal Image img;

		private ProbeResult bestRoot;

		internal Node(NodeType rootType)
		{
			isFullAtlas = rootType;
		}

		private bool isLeaf()
		{
			if (child[0] == null || child[1] == null)
			{
				return true;
			}
			return false;
		}

		internal Node Insert(Image im, bool handed)
		{
			int num;
			int num2;
			if (handed)
			{
				num = 0;
				num2 = 1;
			}
			else
			{
				num = 1;
				num2 = 0;
			}
			if (!isLeaf())
			{
				Node node = child[num].Insert(im, handed);
				if (node != null)
				{
					return node;
				}
				return child[num2].Insert(im, handed);
			}
			if (img != null)
			{
				return null;
			}
			if (r.w < im.w || r.h < im.h)
			{
				return null;
			}
			if (r.w == im.w && r.h == im.h)
			{
				img = im;
				return this;
			}
			child[num] = new Node(NodeType.regular);
			child[num2] = new Node(NodeType.regular);
			int num3 = r.w - im.w;
			int num4 = r.h - im.h;
			if (num3 > num4)
			{
				child[num].r = new PixRect(r.x, r.y, im.w, r.h);
				child[num2].r = new PixRect(r.x + im.w, r.y, r.w - im.w, r.h);
			}
			else
			{
				child[num].r = new PixRect(r.x, r.y, r.w, im.h);
				child[num2].r = new PixRect(r.x, r.y + im.h, r.w, r.h - im.h);
			}
			return child[num].Insert(im, handed);
		}
	}

	private ProbeResult bestRoot;

	public int atlasY;

	private static void printTree(Node r, string spc)
	{
		Debug.Log(spc + "Nd img=" + (r.img != null) + " r=" + r.r);
		if (r.child[0] != null)
		{
			printTree(r.child[0], spc + "      ");
		}
		if (r.child[1] != null)
		{
			printTree(r.child[1], spc + "      ");
		}
	}

	private static void flattenTree(Node r, List<Image> putHere)
	{
		if (r.img != null)
		{
			r.img.x = r.r.x;
			r.img.y = r.r.y;
			putHere.Add(r.img);
		}
		if (r.child[0] != null)
		{
			flattenTree(r.child[0], putHere);
		}
		if (r.child[1] != null)
		{
			flattenTree(r.child[1], putHere);
		}
	}

	private static void drawGizmosNode(Node r)
	{
		Vector3 size = new Vector3(r.r.w, r.r.h, 0f);
		Vector3 center = new Vector3((float)r.r.x + size.x / 2f, (float)(-r.r.y) - size.y / 2f, 0f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(center, size);
		if (r.img != null)
		{
			Gizmos.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
			size = new Vector3(r.img.w, r.img.h, 0f);
			Gizmos.DrawCube(new Vector3((float)r.r.x + size.x / 2f, (float)(-r.r.y) - size.y / 2f, 0f), size);
		}
		if (r.child[0] != null)
		{
			Gizmos.color = Color.red;
			drawGizmosNode(r.child[0]);
		}
		if (r.child[1] != null)
		{
			Gizmos.color = Color.green;
			drawGizmosNode(r.child[1]);
		}
	}

	public void DrawGizmos()
	{
		if (bestRoot != null)
		{
			drawGizmosNode(bestRoot.root);
			Gizmos.color = Color.yellow;
			Vector3 size = new Vector3(bestRoot.outW, -bestRoot.outH, 0f);
			Gizmos.DrawWireCube(new Vector3(size.x / 2f, size.y / 2f, 0f), size);
		}
	}

	private bool ProbeSingleAtlas(Image[] imgsToAdd, int idealAtlasW, int idealAtlasH, float imgArea, int maxAtlasDimX, int maxAtlasDimY, ProbeResult pr)
	{
		Node node = new Node(NodeType.maxDim);
		node.r = new PixRect(0, 0, idealAtlasW, idealAtlasH);
		for (int i = 0; i < imgsToAdd.Length; i++)
		{
			if (node.Insert(imgsToAdd[i], handed: false) == null)
			{
				return false;
			}
			if (i != imgsToAdd.Length - 1)
			{
				continue;
			}
			int x = 0;
			int y = 0;
			GetExtent(node, ref x, ref y);
			int num = x;
			int num2 = y;
			bool fits;
			float e;
			float sq;
			if (atlasMustBePowerOfTwo)
			{
				num = Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(x), maxAtlasDimX);
				num2 = Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(y), maxAtlasDimY);
				if (num2 < num / 2)
				{
					num2 = num / 2;
				}
				if (num < num2 / 2)
				{
					num = num2 / 2;
				}
				fits = x <= maxAtlasDimX && y <= maxAtlasDimY;
				float num3 = Mathf.Max(1f, (float)x / (float)maxAtlasDimX);
				float num4 = Mathf.Max(1f, (float)y / (float)maxAtlasDimY);
				float num5 = (float)num * num3 * (float)num2 * num4;
				e = 1f - (num5 - imgArea) / num5;
				sq = 1f;
			}
			else
			{
				e = 1f - ((float)(x * y) - imgArea) / (float)(x * y);
				sq = ((x >= y) ? ((float)y / (float)x) : ((float)x / (float)y));
				fits = x <= maxAtlasDimX && y <= maxAtlasDimY;
			}
			pr.Set(x, y, num, num2, node, fits, e, sq);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Probe success efficiency w=" + x + " h=" + y + " e=" + e + " sq=" + sq + " fits=" + fits);
			}
			return true;
		}
		Debug.LogError("Should never get here.");
		return false;
	}

	private bool ProbeMultiAtlas(Image[] imgsToAdd, int idealAtlasW, int idealAtlasH, float imgArea, int maxAtlasDimX, int maxAtlasDimY, ProbeResult pr)
	{
		int num = 0;
		Node node = new Node(NodeType.maxDim);
		node.r = new PixRect(0, 0, idealAtlasW, idealAtlasH);
		for (int i = 0; i < imgsToAdd.Length; i++)
		{
			if (node.Insert(imgsToAdd[i], handed: false) == null)
			{
				if (imgsToAdd[i].x > idealAtlasW && imgsToAdd[i].y > idealAtlasH)
				{
					return false;
				}
				Node obj = new Node(NodeType.Container)
				{
					r = new PixRect(0, 0, node.r.w + idealAtlasW, idealAtlasH)
				};
				Node node2 = new Node(NodeType.maxDim)
				{
					r = new PixRect(node.r.w, 0, idealAtlasW, idealAtlasH)
				};
				obj.child[1] = node2;
				obj.child[0] = node;
				node = obj;
				node.Insert(imgsToAdd[i], handed: false);
				num++;
			}
		}
		pr.numAtlases = num;
		pr.root = node;
		pr.totalAtlasArea = num * maxAtlasDimX * maxAtlasDimY;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.LogDebug("Probe success efficiency numAtlases=" + num + " totalArea=" + pr.totalAtlasArea);
		}
		return true;
	}

	internal void GetExtent(Node r, ref int x, ref int y)
	{
		if (r.img != null)
		{
			if (r.r.x + r.img.w > x)
			{
				x = r.r.x + r.img.w;
			}
			if (r.r.y + r.img.h > y)
			{
				y = r.r.y + r.img.h;
			}
		}
		if (r.child[0] != null)
		{
			GetExtent(r.child[0], ref x, ref y);
		}
		if (r.child[1] != null)
		{
			GetExtent(r.child[1], ref x, ref y);
		}
	}

	private int StepWidthHeight(int oldVal, int step, int maxDim)
	{
		if (atlasMustBePowerOfTwo && oldVal < maxDim)
		{
			return oldVal * 2;
		}
		int num = oldVal + step;
		if (num > maxDim && oldVal < maxDim)
		{
			num = maxDim;
		}
		return num;
	}

	public override AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, int maxDimensionX, int maxDimensionY, int atPadding)
	{
		List<AtlasPadding> list = new List<AtlasPadding>();
		for (int i = 0; i < imgWidthHeights.Count; i++)
		{
			AtlasPadding item = default(AtlasPadding);
			item.leftRight = (item.topBottom = atPadding);
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
			return _GetRectsMultiAtlas(imgWidthHeights, paddings, maxDimensionX, maxDimensionY, 2 + num * 2, 2 + num2 * 2, 2 + num * 2, 2 + num2 * 2);
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
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log($"_GetRects numImages={imgWidthHeights.Count}, maxDimension={maxDimensionX}, minImageSizeX={minImageSizeX}, minImageSizeY={minImageSizeY}, masterImageSizeX={masterImageSizeX}, masterImageSizeY={masterImageSizeY}, recursionDepth={recursionDepth}");
		}
		if (recursionDepth > 10 && LOG_LEVEL >= MB2_LogLevel.error)
		{
			Debug.LogError("Maximum recursion depth reached. The baked atlas is likely not very good.  This happens when the packed atlases exceeds the maximum atlas size in one or both dimensions so that the atlas needs to be downscaled AND there are some very thin or very small images (only-a-few-pixels). these very thin images can 'vanish' completely when the atlas is downscaled.\n\n Try one or more of the following: using multiple atlases, increase the maximum atlas size, don't use 'force-power-of-two', remove the source materials that are are using very small/thin textures.");
		}
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		Image[] array = new Image[imgWidthHeights.Count];
		for (int i = 0; i < array.Length; i++)
		{
			int tw = (int)imgWidthHeights[i].x;
			int th = (int)imgWidthHeights[i].y;
			Image image = (array[i] = new Image(i, tw, th, paddings[i], minImageSizeX, minImageSizeY));
			num += (float)(image.w * image.h);
			num2 = Mathf.Max(num2, image.w);
			num3 = Mathf.Max(num3, image.h);
		}
		if ((float)num3 / (float)num2 > 2f)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Using height Comparer");
			}
			Array.Sort(array, new ImageHeightComparer());
		}
		else if ((double)((float)num3 / (float)num2) < 0.5)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Using width Comparer");
			}
			Array.Sort(array, new ImageWidthComparer());
		}
		else
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Using area Comparer");
			}
			Array.Sort(array, new ImageAreaComparer());
		}
		int num4 = (int)Mathf.Sqrt(num);
		int num6;
		int num5;
		if (atlasMustBePowerOfTwo)
		{
			num6 = (num5 = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo(num4));
			if (num2 > num6)
			{
				num6 = MB2_TexturePacker.CeilToNearestPowerOfTwo(num6);
			}
			if (num3 > num5)
			{
				num5 = MB2_TexturePacker.CeilToNearestPowerOfTwo(num5);
			}
		}
		else
		{
			num6 = num4;
			num5 = num4;
			if (num2 > num4)
			{
				num6 = num2;
				num5 = Mathf.Max(Mathf.CeilToInt(num / (float)num2), num3);
			}
			if (num3 > num4)
			{
				num6 = Mathf.Max(Mathf.CeilToInt(num / (float)num3), num2);
				num5 = num3;
			}
		}
		if (num6 == 0)
		{
			num6 = 4;
		}
		if (num5 == 0)
		{
			num5 = 4;
		}
		int num7 = (int)((float)num6 * 0.15f);
		int num8 = (int)((float)num5 * 0.15f);
		if (num7 == 0)
		{
			num7 = 1;
		}
		if (num8 == 0)
		{
			num8 = 1;
		}
		int num9 = 2;
		int num10 = num6;
		int num11 = num5;
		while (num9 >= 1 && num11 < num4 * 1000)
		{
			bool flag = false;
			num9 = 0;
			num10 = num6;
			while (!flag && num10 < num4 * 1000)
			{
				ProbeResult probeResult = new ProbeResult();
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					Debug.Log("Probing h=" + num11 + " w=" + num10);
				}
				if (ProbeSingleAtlas(array, num10, num11, num, maxDimensionX, maxDimensionY, probeResult))
				{
					flag = true;
					if (bestRoot == null)
					{
						bestRoot = probeResult;
					}
					else if (probeResult.GetScore(atlasMustBePowerOfTwo) > bestRoot.GetScore(atlasMustBePowerOfTwo))
					{
						bestRoot = probeResult;
					}
				}
				else
				{
					num9++;
					num10 = StepWidthHeight(num10, num7, maxDimensionX);
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						MB2_Log.LogDebug("increasing Width h=" + num11 + " w=" + num10);
					}
				}
			}
			num11 = StepWidthHeight(num11, num8, maxDimensionY);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("increasing Height h=" + num11 + " w=" + num10);
			}
		}
		if (bestRoot == null)
		{
			return null;
		}
		int num12 = 0;
		int num13 = 0;
		if (atlasMustBePowerOfTwo)
		{
			num12 = Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(bestRoot.w), maxDimensionX);
			num13 = Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(bestRoot.h), maxDimensionY);
			if (num13 < num12 / 2)
			{
				num13 = num12 / 2;
			}
			if (num12 < num13 / 2)
			{
				num12 = num13 / 2;
			}
		}
		else
		{
			num12 = Mathf.Min(bestRoot.w, maxDimensionX);
			num13 = Mathf.Min(bestRoot.h, maxDimensionY);
		}
		bestRoot.outW = num12;
		bestRoot.outH = num13;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Best fit found: atlasW=" + num12 + " atlasH" + num13 + " w=" + bestRoot.w + " h=" + bestRoot.h + " efficiency=" + bestRoot.efficiency + " squareness=" + bestRoot.squareness + " fits in max dimension=" + bestRoot.largerOrEqualToMaxDim);
		}
		List<Image> list = new List<Image>();
		flattenTree(bestRoot.root, list);
		list.Sort(new ImgIDComparer());
		Vector2 rootWH = new Vector2(bestRoot.w, bestRoot.h);
		if (!ScaleAtlasToFitMaxDim(rootWH, list, maxDimensionX, maxDimensionY, paddings[0], minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY, ref num12, ref num13, out var padX, out var padY, out var newMinSizeX, out var newMinSizeY) || recursionDepth > 10)
		{
			AtlasPackingResult atlasPackingResult = new AtlasPackingResult(paddings.ToArray());
			atlasPackingResult.rects = new Rect[list.Count];
			atlasPackingResult.srcImgIdxs = new int[list.Count];
			atlasPackingResult.atlasX = num12;
			atlasPackingResult.atlasY = num13;
			atlasPackingResult.usedW = -1;
			atlasPackingResult.usedH = -1;
			for (int j = 0; j < list.Count; j++)
			{
				Image image2 = list[j];
				Rect rect = (atlasPackingResult.rects[j] = new Rect((float)image2.x / (float)num12 + padX, (float)image2.y / (float)num13 + padY, (float)image2.w / (float)num12 - padX * 2f, (float)image2.h / (float)num13 - padY * 2f));
				atlasPackingResult.srcImgIdxs[j] = image2.imgId;
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Image: " + j + " imgID=" + image2.imgId + " x=" + rect.x * (float)num12 + " y=" + rect.y * (float)num13 + " w=" + rect.width * (float)num12 + " h=" + rect.height * (float)num13 + " padding=" + paddings[j].leftRight * 2 + "x" + paddings[j].topBottom * 2);
				}
			}
			atlasPackingResult.CalcUsedWidthAndHeight();
			return atlasPackingResult;
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("==================== REDOING PACKING ================");
		}
		return _GetRectsSingleAtlas(imgWidthHeights, paddings, maxDimensionX, maxDimensionY, newMinSizeX, newMinSizeY, masterImageSizeX, masterImageSizeY, recursionDepth + 1);
	}

	private AtlasPackingResult[] _GetRectsMultiAtlas(List<Vector2> imgWidthHeights, List<AtlasPadding> paddings, int maxDimensionPassedX, int maxDimensionPassedY, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log($"_GetRects numImages={imgWidthHeights.Count}, maxDimensionX={maxDimensionPassedX}, maxDimensionY={maxDimensionPassedY} minImageSizeX={minImageSizeX}, minImageSizeY={minImageSizeY}, masterImageSizeX={masterImageSizeX}, masterImageSizeY={masterImageSizeY}");
		}
		float num = 0f;
		int a = 0;
		int a2 = 0;
		Image[] array = new Image[imgWidthHeights.Count];
		int num2 = maxDimensionPassedX;
		int num3 = maxDimensionPassedY;
		if (atlasMustBePowerOfTwo)
		{
			num2 = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo(num2);
			num3 = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo(num3);
		}
		for (int i = 0; i < array.Length; i++)
		{
			int a3 = (int)imgWidthHeights[i].x;
			int a4 = (int)imgWidthHeights[i].y;
			a3 = Mathf.Min(a3, num2 - paddings[i].leftRight * 2);
			a4 = Mathf.Min(a4, num3 - paddings[i].topBottom * 2);
			Image image = (array[i] = new Image(i, a3, a4, paddings[i], minImageSizeX, minImageSizeY));
			num += (float)(image.w * image.h);
			a = Mathf.Max(a, image.w);
			a2 = Mathf.Max(a2, image.h);
		}
		int num4;
		int num5;
		if (atlasMustBePowerOfTwo)
		{
			num4 = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo(num3);
			num5 = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo(num2);
		}
		else
		{
			num4 = num3;
			num5 = num2;
		}
		if (num5 == 0)
		{
			num5 = 4;
		}
		if (num4 == 0)
		{
			num4 = 4;
		}
		ProbeResult probeResult = new ProbeResult();
		Array.Sort(array, new ImageHeightComparer());
		if (ProbeMultiAtlas(array, num5, num4, num, num2, num3, probeResult))
		{
			bestRoot = probeResult;
		}
		Array.Sort(array, new ImageWidthComparer());
		if (ProbeMultiAtlas(array, num5, num4, num, num2, num3, probeResult) && probeResult.totalAtlasArea < bestRoot.totalAtlasArea)
		{
			bestRoot = probeResult;
		}
		Array.Sort(array, new ImageAreaComparer());
		if (ProbeMultiAtlas(array, num5, num4, num, num2, num3, probeResult) && probeResult.totalAtlasArea < bestRoot.totalAtlasArea)
		{
			bestRoot = probeResult;
		}
		if (bestRoot == null)
		{
			return null;
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("Best fit found: w=" + bestRoot.w + " h=" + bestRoot.h + " efficiency=" + bestRoot.efficiency + " squareness=" + bestRoot.squareness + " fits in max dimension=" + bestRoot.largerOrEqualToMaxDim);
		}
		List<AtlasPackingResult> list = new List<AtlasPackingResult>();
		List<Node> list2 = new List<Node>();
		Stack<Node> stack = new Stack<Node>();
		for (Node node = bestRoot.root; node != null; node = node.child[0])
		{
			stack.Push(node);
		}
		while (stack.Count > 0)
		{
			Node node = stack.Pop();
			if (node.isFullAtlas == NodeType.maxDim)
			{
				list2.Add(node);
			}
			if (node.child[1] != null)
			{
				for (node = node.child[1]; node != null; node = node.child[0])
				{
					stack.Push(node);
				}
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			List<Image> list3 = new List<Image>();
			flattenTree(list2[j], list3);
			Rect[] array2 = new Rect[list3.Count];
			int[] array3 = new int[list3.Count];
			for (int k = 0; k < list3.Count; k++)
			{
				array2[k] = new Rect(list3[k].x - list2[j].r.x, list3[k].y, list3[k].w, list3[k].h);
				array3[k] = list3[k].imgId;
			}
			AtlasPackingResult atlasPackingResult = new AtlasPackingResult(paddings.ToArray());
			GetExtent(list2[j], ref atlasPackingResult.usedW, ref atlasPackingResult.usedH);
			atlasPackingResult.usedW -= list2[j].r.x;
			int w = list2[j].r.w;
			int h = list2[j].r.h;
			if (atlasMustBePowerOfTwo)
			{
				w = Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(atlasPackingResult.usedW), list2[j].r.w);
				h = Mathf.Min(MB2_TexturePacker.CeilToNearestPowerOfTwo(atlasPackingResult.usedH), list2[j].r.h);
				if (h < w / 2)
				{
					h = w / 2;
				}
				if (w < h / 2)
				{
					w = h / 2;
				}
			}
			else
			{
				w = atlasPackingResult.usedW;
				h = atlasPackingResult.usedH;
			}
			atlasPackingResult.atlasY = h;
			atlasPackingResult.atlasX = w;
			atlasPackingResult.rects = array2;
			atlasPackingResult.srcImgIdxs = array3;
			atlasPackingResult.CalcUsedWidthAndHeight();
			list.Add(atlasPackingResult);
			ConvertToRectsWithoutPaddingAndNormalize01(atlasPackingResult, paddings[j]);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug($"Done GetRects ");
			}
		}
		return list.ToArray();
	}
}
