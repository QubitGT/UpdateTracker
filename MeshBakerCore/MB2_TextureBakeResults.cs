using System.Collections;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB2_TextureBakeResults : ScriptableObject
{
	public class CoroutineResult
	{
		public bool isComplete;
	}

	public enum ResultType
	{
		atlas,
		textureArray
	}

	public int version;

	public ResultType resultType;

	[NonReorderable]
	public MB_MaterialAndUVRect[] materialsAndUVRects;

	[NonReorderable]
	public MB_MultiMaterial[] resultMaterials;

	[NonReorderable]
	public MB_MultiMaterialTexArray[] resultMaterialsTexArray;

	public bool doMultiMaterial;

	public static int VERSION => 3252;

	public MB2_TextureBakeResults()
	{
		version = VERSION;
	}

	private void OnEnable()
	{
		if (version < 3251)
		{
			for (int i = 0; i < materialsAndUVRects.Length; i++)
			{
				materialsAndUVRects[i].allPropsUseSameTiling = true;
			}
		}
		version = VERSION;
	}

	public int NumResultMaterials()
	{
		if (resultType == ResultType.atlas)
		{
			return resultMaterials.Length;
		}
		return resultMaterialsTexArray.Length;
	}

	public Material GetCombinedMaterialForSubmesh(int idx)
	{
		if (resultType == ResultType.atlas)
		{
			return resultMaterials[idx].combinedMaterial;
		}
		return resultMaterialsTexArray[idx].combinedMaterial;
	}

	public IEnumerator FindRuntimeMaterialsFromAddresses(CoroutineResult isComplete)
	{
		yield return MBVersion.FindRuntimeMaterialsFromAddresses(this, isComplete);
		isComplete.isComplete = true;
	}

	public bool GetConsiderMeshUVs(int idxInSrcMats, Material srcMaterial)
	{
		if (resultType == ResultType.atlas)
		{
			return resultMaterials[idxInSrcMats].considerMeshUVs;
		}
		List<MB_TexArraySlice> slices = resultMaterialsTexArray[idxInSrcMats].slices;
		for (int i = 0; i < slices.Count; i++)
		{
			if (slices[i].ContainsMaterial(srcMaterial))
			{
				return slices[i].considerMeshUVs;
			}
		}
		Debug.LogError("There were no source materials for any slice in this result material.");
		return false;
	}

	public List<Material> GetSourceMaterialsUsedByResultMaterial(int resultMatIdx)
	{
		if (resultType == ResultType.atlas)
		{
			return resultMaterials[resultMatIdx].sourceMaterials;
		}
		HashSet<Material> hashSet = new HashSet<Material>();
		List<MB_TexArraySlice> slices = resultMaterialsTexArray[resultMatIdx].slices;
		for (int i = 0; i < slices.Count; i++)
		{
			List<Material> list = new List<Material>();
			slices[i].GetAllUsedMaterials(list);
			for (int j = 0; j < list.Count; j++)
			{
				hashSet.Add(list[j]);
			}
		}
		return new List<Material>(hashSet);
	}

	public static MB2_TextureBakeResults CreateForMaterialsOnRenderer(GameObject[] gos, List<Material> matsOnTargetRenderer)
	{
		HashSet<Material> hashSet = new HashSet<Material>(matsOnTargetRenderer);
		for (int i = 0; i < gos.Length; i++)
		{
			if (gos[i] == null)
			{
				Debug.LogError($"Game object {i} in list of objects to add was null");
				return null;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(gos[i]);
			if (gOMaterials.Length == 0)
			{
				Debug.LogError($"Game object {i} in list of objects to add no renderer");
				return null;
			}
			for (int j = 0; j < gOMaterials.Length; j++)
			{
				if (!hashSet.Contains(gOMaterials[j]))
				{
					hashSet.Add(gOMaterials[j]);
				}
			}
		}
		Material[] array = new Material[hashSet.Count];
		hashSet.CopyTo(array);
		MB2_TextureBakeResults mB2_TextureBakeResults = (MB2_TextureBakeResults)ScriptableObject.CreateInstance(typeof(MB2_TextureBakeResults));
		List<MB_MaterialAndUVRect> list = new List<MB_MaterialAndUVRect>();
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] != null)
			{
				MB_MaterialAndUVRect item = new MB_MaterialAndUVRect(array[k], new Rect(0f, 0f, 1f, 1f), allPropsUseSameTiling: true, new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, 0f, 0f), MB_TextureTilingTreatment.none, "");
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		mB2_TextureBakeResults.resultMaterials = new MB_MultiMaterial[list.Count];
		for (int l = 0; l < list.Count; l++)
		{
			mB2_TextureBakeResults.resultMaterials[l] = new MB_MultiMaterial();
			List<Material> list2 = new List<Material>();
			list2.Add(list[l].material);
			mB2_TextureBakeResults.resultMaterials[l].sourceMaterials = list2;
			mB2_TextureBakeResults.resultMaterials[l].combinedMaterial = list[l].material;
			mB2_TextureBakeResults.resultMaterials[l].considerMeshUVs = false;
		}
		if (array.Length == 1)
		{
			mB2_TextureBakeResults.doMultiMaterial = false;
		}
		else
		{
			mB2_TextureBakeResults.doMultiMaterial = true;
		}
		mB2_TextureBakeResults.materialsAndUVRects = list.ToArray();
		return mB2_TextureBakeResults;
	}

	public bool DoAnyResultMatsUseConsiderMeshUVs()
	{
		if (resultType == ResultType.atlas)
		{
			if (resultMaterials == null)
			{
				return false;
			}
			for (int i = 0; i < resultMaterials.Length; i++)
			{
				if (resultMaterials[i].considerMeshUVs)
				{
					return true;
				}
			}
			return false;
		}
		if (resultMaterialsTexArray == null)
		{
			return false;
		}
		for (int j = 0; j < resultMaterialsTexArray.Length; j++)
		{
			MB_MultiMaterialTexArray mB_MultiMaterialTexArray = resultMaterialsTexArray[j];
			for (int k = 0; k < mB_MultiMaterialTexArray.slices.Count; k++)
			{
				if (mB_MultiMaterialTexArray.slices[k].considerMeshUVs)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ContainsMaterial(Material m)
	{
		for (int i = 0; i < materialsAndUVRects.Length; i++)
		{
			if (materialsAndUVRects[i].material == m)
			{
				return true;
			}
		}
		return false;
	}

	public string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Shaders:\n");
		HashSet<Shader> hashSet = new HashSet<Shader>();
		if (materialsAndUVRects != null)
		{
			for (int i = 0; i < materialsAndUVRects.Length; i++)
			{
				if (materialsAndUVRects[i].material != null)
				{
					hashSet.Add(materialsAndUVRects[i].material.shader);
				}
			}
		}
		foreach (Shader item in hashSet)
		{
			stringBuilder.Append("  ").Append(item.name).AppendLine();
		}
		stringBuilder.Append("Materials:\n");
		if (materialsAndUVRects != null)
		{
			for (int j = 0; j < materialsAndUVRects.Length; j++)
			{
				if (materialsAndUVRects[j].material != null)
				{
					stringBuilder.Append("  ").Append(materialsAndUVRects[j].material.name).AppendLine();
				}
			}
		}
		return stringBuilder.ToString();
	}

	public void UpgradeToCurrentVersion(MB2_TextureBakeResults tbr)
	{
		if (tbr.version < 3252)
		{
			for (int i = 0; i < tbr.materialsAndUVRects.Length; i++)
			{
				tbr.materialsAndUVRects[i].allPropsUseSameTiling = true;
			}
		}
	}

	public static bool IsMeshAndMaterialRectEnclosedByAtlasRect(MB_TextureTilingTreatment tilingTreatment, Rect uvR, Rect sourceMaterialTiling, Rect samplingEncapsulatinRect, MB2_LogLevel logLevel)
	{
		Rect rect = default(Rect);
		rect = MB3_UVTransformUtility.CombineTransforms(ref uvR, ref sourceMaterialTiling);
		if (logLevel >= MB2_LogLevel.trace && logLevel >= MB2_LogLevel.trace)
		{
			Debug.Log("IsMeshAndMaterialRectEnclosedByAtlasRect Rect in atlas uvR=" + uvR.ToString("f5") + " sourceMaterialTiling=" + sourceMaterialTiling.ToString("f5") + "Potential Rect (must fit in encapsulating) " + rect.ToString("f5") + " encapsulating=" + samplingEncapsulatinRect.ToString("f5") + " tilingTreatment=" + tilingTreatment);
		}
		switch (tilingTreatment)
		{
		case MB_TextureTilingTreatment.edgeToEdgeX:
			if (MB3_UVTransformUtility.LineSegmentContainsShifted(samplingEncapsulatinRect.y, samplingEncapsulatinRect.height, rect.y, rect.height))
			{
				return true;
			}
			break;
		case MB_TextureTilingTreatment.edgeToEdgeY:
			if (MB3_UVTransformUtility.LineSegmentContainsShifted(samplingEncapsulatinRect.x, samplingEncapsulatinRect.width, rect.x, rect.width))
			{
				return true;
			}
			break;
		case MB_TextureTilingTreatment.edgeToEdgeXY:
			return true;
		default:
			if (MB3_UVTransformUtility.RectContainsShifted(ref samplingEncapsulatinRect, ref rect))
			{
				return true;
			}
			break;
		}
		return false;
	}
}
