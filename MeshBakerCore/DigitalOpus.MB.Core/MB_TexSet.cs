using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB_TexSet
{
	private interface PipelineVariation
	{
		void GetRectsForTextureBakeResults(out Rect allPropsUseSameTiling_encapsulatingSamplingRect, out Rect propsUseDifferntTiling_obUVRect);

		void SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(MB_TextureTilingTreatment newTilingTreatment);

		Rect GetMaterialTilingRectForTextureBakerResults(int materialIndex);

		void AdjustResultMaterialNonTextureProperties(Material resultMaterial, List<ShaderTextureProperty> props);
	}

	private class PipelineVariationAllTexturesUseSameMatTiling : PipelineVariation
	{
		private MB_TexSet texSet;

		public PipelineVariationAllTexturesUseSameMatTiling(MB_TexSet ts)
		{
			texSet = ts;
		}

		public void GetRectsForTextureBakeResults(out Rect allPropsUseSameTiling_encapsulatingSamplingRect, out Rect propsUseDifferntTiling_obUVRect)
		{
			propsUseDifferntTiling_obUVRect = new Rect(0f, 0f, 0f, 0f);
			allPropsUseSameTiling_encapsulatingSamplingRect = texSet.GetEncapsulatingSamplingRectIfTilingSame();
			if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeX)
			{
				allPropsUseSameTiling_encapsulatingSamplingRect.x = 0f;
				allPropsUseSameTiling_encapsulatingSamplingRect.width = 1f;
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeY)
			{
				allPropsUseSameTiling_encapsulatingSamplingRect.y = 0f;
				allPropsUseSameTiling_encapsulatingSamplingRect.height = 1f;
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeXY)
			{
				allPropsUseSameTiling_encapsulatingSamplingRect = new Rect(0f, 0f, 1f, 1f);
			}
		}

		public void SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(MB_TextureTilingTreatment newTilingTreatment)
		{
			if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeX)
			{
				MeshBakerMaterialTexture[] ts = texSet.ts;
				foreach (MeshBakerMaterialTexture obj in ts)
				{
					DRect encapsulatingSamplingRect = obj.GetEncapsulatingSamplingRect();
					encapsulatingSamplingRect.width = 1.0;
					obj.SetEncapsulatingSamplingRect(texSet, encapsulatingSamplingRect);
				}
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeY)
			{
				MeshBakerMaterialTexture[] ts = texSet.ts;
				foreach (MeshBakerMaterialTexture obj2 in ts)
				{
					DRect encapsulatingSamplingRect2 = obj2.GetEncapsulatingSamplingRect();
					encapsulatingSamplingRect2.height = 1.0;
					obj2.SetEncapsulatingSamplingRect(texSet, encapsulatingSamplingRect2);
				}
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeXY)
			{
				MeshBakerMaterialTexture[] ts = texSet.ts;
				foreach (MeshBakerMaterialTexture obj3 in ts)
				{
					DRect encapsulatingSamplingRect3 = obj3.GetEncapsulatingSamplingRect();
					encapsulatingSamplingRect3.height = 1.0;
					encapsulatingSamplingRect3.width = 1.0;
					obj3.SetEncapsulatingSamplingRect(texSet, encapsulatingSamplingRect3);
				}
			}
		}

		public Rect GetMaterialTilingRectForTextureBakerResults(int materialIndex)
		{
			return texSet.matsAndGOs.mats[materialIndex].materialTiling.GetRect();
		}

		public void AdjustResultMaterialNonTextureProperties(Material resultMaterial, List<ShaderTextureProperty> props)
		{
		}
	}

	private class PipelineVariationSomeTexturesUseDifferentMatTiling : PipelineVariation
	{
		private MB_TexSet texSet;

		public PipelineVariationSomeTexturesUseDifferentMatTiling(MB_TexSet ts)
		{
			texSet = ts;
		}

		public void GetRectsForTextureBakeResults(out Rect allPropsUseSameTiling_encapsulatingSamplingRect, out Rect propsUseDifferntTiling_obUVRect)
		{
			allPropsUseSameTiling_encapsulatingSamplingRect = new Rect(0f, 0f, 0f, 0f);
			propsUseDifferntTiling_obUVRect = texSet.obUVrect.GetRect();
			if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeX)
			{
				propsUseDifferntTiling_obUVRect.x = 0f;
				propsUseDifferntTiling_obUVRect.width = 1f;
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeY)
			{
				propsUseDifferntTiling_obUVRect.y = 0f;
				propsUseDifferntTiling_obUVRect.height = 1f;
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeXY)
			{
				propsUseDifferntTiling_obUVRect = new Rect(0f, 0f, 1f, 1f);
			}
		}

		public void SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(MB_TextureTilingTreatment newTilingTreatment)
		{
			if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeX)
			{
				MeshBakerMaterialTexture[] ts = texSet.ts;
				foreach (MeshBakerMaterialTexture obj in ts)
				{
					DRect encapsulatingSamplingRect = obj.GetEncapsulatingSamplingRect();
					encapsulatingSamplingRect.width = 1.0;
					obj.SetEncapsulatingSamplingRect(texSet, encapsulatingSamplingRect);
				}
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeY)
			{
				MeshBakerMaterialTexture[] ts = texSet.ts;
				foreach (MeshBakerMaterialTexture obj2 in ts)
				{
					DRect encapsulatingSamplingRect2 = obj2.GetEncapsulatingSamplingRect();
					encapsulatingSamplingRect2.height = 1.0;
					obj2.SetEncapsulatingSamplingRect(texSet, encapsulatingSamplingRect2);
				}
			}
			else if (texSet.tilingTreatment == MB_TextureTilingTreatment.edgeToEdgeXY)
			{
				MeshBakerMaterialTexture[] ts = texSet.ts;
				foreach (MeshBakerMaterialTexture obj3 in ts)
				{
					DRect encapsulatingSamplingRect3 = obj3.GetEncapsulatingSamplingRect();
					encapsulatingSamplingRect3.height = 1.0;
					encapsulatingSamplingRect3.width = 1.0;
					obj3.SetEncapsulatingSamplingRect(texSet, encapsulatingSamplingRect3);
				}
			}
		}

		public Rect GetMaterialTilingRectForTextureBakerResults(int materialIndex)
		{
			return new Rect(0f, 0f, 0f, 0f);
		}

		public void AdjustResultMaterialNonTextureProperties(Material resultMaterial, List<ShaderTextureProperty> props)
		{
			if (!texSet.thisIsOnlyTexSetInAtlas)
			{
				return;
			}
			for (int i = 0; i < props.Count; i++)
			{
				if (resultMaterial.HasProperty(props[i].name))
				{
					resultMaterial.SetTextureOffset(props[i].name, texSet.ts[i].matTilingRect.min);
					resultMaterial.SetTextureScale(props[i].name, texSet.ts[i].matTilingRect.size);
				}
			}
		}
	}

	public MeshBakerMaterialTexture[] ts;

	public MatsAndGOs matsAndGOs;

	public int idealWidth_pix;

	public int idealHeight_pix;

	private PipelineVariation pipelineVariation;

	public bool allTexturesUseSameMatTiling { get; private set; }

	public bool thisIsOnlyTexSetInAtlas { get; private set; }

	public MB_TextureTilingTreatment tilingTreatment { get; private set; }

	public Vector2 obUVoffset { get; private set; }

	public Vector2 obUVscale { get; private set; }

	internal DRect obUVrect => new DRect(obUVoffset, obUVscale);

	public MB_TexSet(MeshBakerMaterialTexture[] tss, Vector2 uvOffset, Vector2 uvScale, MB_TextureTilingTreatment treatment)
	{
		ts = tss;
		tilingTreatment = treatment;
		obUVoffset = uvOffset;
		obUVscale = uvScale;
		allTexturesUseSameMatTiling = false;
		thisIsOnlyTexSetInAtlas = false;
		matsAndGOs = new MatsAndGOs();
		matsAndGOs.mats = new List<MatAndTransformToMerged>();
		matsAndGOs.gos = new List<GameObject>();
		pipelineVariation = new PipelineVariationSomeTexturesUseDifferentMatTiling(this);
	}

	internal bool IsEqual(object obj, bool fixOutOfBoundsUVs, MB3_TextureCombinerNonTextureProperties resultMaterialTextureBlender)
	{
		if (!(obj is MB_TexSet))
		{
			return false;
		}
		MB_TexSet mB_TexSet = (MB_TexSet)obj;
		if (mB_TexSet.ts.Length != ts.Length)
		{
			return false;
		}
		for (int i = 0; i < ts.Length; i++)
		{
			if (ts[i].matTilingRect != mB_TexSet.ts[i].matTilingRect)
			{
				return false;
			}
			if (!ts[i].AreTexturesEqual(mB_TexSet.ts[i]))
			{
				return false;
			}
			if (!resultMaterialTextureBlender.NonTexturePropertiesAreEqual(matsAndGOs.mats[0].mat, mB_TexSet.matsAndGOs.mats[0].mat))
			{
				return false;
			}
		}
		if (fixOutOfBoundsUVs && (obUVoffset.x != mB_TexSet.obUVoffset.x || obUVoffset.y != mB_TexSet.obUVoffset.y))
		{
			return false;
		}
		if (fixOutOfBoundsUVs && (obUVscale.x != mB_TexSet.obUVscale.x || obUVscale.y != mB_TexSet.obUVscale.y))
		{
			return false;
		}
		return true;
	}

	public Vector2 GetMaxRawTextureHeightWidth()
	{
		Vector2 result = new Vector2(0f, 0f);
		for (int i = 0; i < ts.Length; i++)
		{
			MeshBakerMaterialTexture meshBakerMaterialTexture = ts[i];
			if (!meshBakerMaterialTexture.isNull)
			{
				result.x = Mathf.Max(result.x, meshBakerMaterialTexture.width);
				result.y = Mathf.Max(result.y, meshBakerMaterialTexture.height);
			}
		}
		return result;
	}

	private Rect GetEncapsulatingSamplingRectIfTilingSame()
	{
		if (ts.Length != 0)
		{
			return ts[0].GetEncapsulatingSamplingRect().GetRect();
		}
		return new Rect(0f, 0f, 1f, 1f);
	}

	public void SetEncapsulatingSamplingRectWhenMergingTexSets(DRect newEncapsulatingSamplingRect)
	{
		for (int i = 0; i < ts.Length; i++)
		{
			ts[i].SetEncapsulatingSamplingRect(this, newEncapsulatingSamplingRect);
		}
	}

	public void SetEncapsulatingSamplingRectForTesting(int propIdx, DRect newEncapsulatingSamplingRect)
	{
		ts[propIdx].SetEncapsulatingSamplingRect(this, newEncapsulatingSamplingRect);
	}

	public void SetEncapsulatingRect(int propIdx, bool considerMeshUVs)
	{
		if (considerMeshUVs)
		{
			ts[propIdx].SetEncapsulatingSamplingRect(this, obUVrect);
		}
		else
		{
			ts[propIdx].SetEncapsulatingSamplingRect(this, new DRect(0f, 0f, 1f, 1f));
		}
	}

	public void CreateColoredTexToReplaceNull(string propName, int propIdx, bool considerMeshUVs, MB3_TextureCombiner combiner, Color col, bool isLinear)
	{
		MeshBakerMaterialTexture obj = ts[propIdx];
		Texture2D t = combiner._createTemporaryTexture(propName, 16, 16, TextureFormat.ARGB32, mipMaps: true, isLinear);
		obj.t = t;
		MB_Utility.setSolidColor(obj.GetTexture2D(), col);
	}

	public void SetThisIsOnlyTexSetInAtlasTrue()
	{
		thisIsOnlyTexSetInAtlas = true;
	}

	public void SetAllTexturesUseSameMatTilingTrue()
	{
		allTexturesUseSameMatTiling = true;
		pipelineVariation = new PipelineVariationAllTexturesUseSameMatTiling(this);
	}

	public void AdjustResultMaterialNonTextureProperties(Material resultMaterial, List<ShaderTextureProperty> props)
	{
		pipelineVariation.AdjustResultMaterialNonTextureProperties(resultMaterial, props);
	}

	public void SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(MB_TextureTilingTreatment newTilingTreatment)
	{
		tilingTreatment = newTilingTreatment;
		pipelineVariation.SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(newTilingTreatment);
	}

	internal void GetRectsForTextureBakeResults(out Rect allPropsUseSameTiling_encapsulatingSamplingRect, out Rect propsUseDifferntTiling_obUVRect)
	{
		pipelineVariation.GetRectsForTextureBakeResults(out allPropsUseSameTiling_encapsulatingSamplingRect, out propsUseDifferntTiling_obUVRect);
	}

	internal Rect GetMaterialTilingRectForTextureBakerResults(int materialIndex)
	{
		return pipelineVariation.GetMaterialTilingRectForTextureBakerResults(materialIndex);
	}

	internal void CalcInitialFullSamplingRects(bool fixOutOfBoundsUVs)
	{
		DRect r = new DRect(0f, 0f, 1f, 1f);
		if (fixOutOfBoundsUVs)
		{
			r = obUVrect;
		}
		for (int i = 0; i < ts.Length; i++)
		{
			if (!ts[i].isNull)
			{
				DRect r2 = ts[i].matTilingRect;
				DRect r3 = ((!fixOutOfBoundsUVs) ? new DRect(0.0, 0.0, 1.0, 1.0) : obUVrect);
				ts[i].SetEncapsulatingSamplingRect(this, MB3_UVTransformUtility.CombineTransforms(ref r3, ref r2));
				r = ts[i].GetEncapsulatingSamplingRect();
			}
		}
		for (int j = 0; j < ts.Length; j++)
		{
			if (ts[j].isNull)
			{
				ts[j].SetEncapsulatingSamplingRect(this, r);
			}
		}
	}

	internal void CalcMatAndUVSamplingRects()
	{
		DRect matTiling = new DRect(0f, 0f, 1f, 1f);
		if (allTexturesUseSameMatTiling)
		{
			for (int i = 0; i < ts.Length; i++)
			{
				if (!ts[i].isNull)
				{
					matTiling = ts[i].matTilingRect;
					break;
				}
			}
		}
		for (int j = 0; j < matsAndGOs.mats.Count; j++)
		{
			matsAndGOs.mats[j].AssignInitialValuesForMaterialTilingAndSamplingRectMatAndUVTiling(allTexturesUseSameMatTiling, matTiling);
		}
	}

	public bool AllTexturesAreSameForMerge(MB_TexSet other, bool considerNonTextureProperties, MB3_TextureCombinerNonTextureProperties resultMaterialTextureBlender)
	{
		if (other.ts.Length != ts.Length)
		{
			return false;
		}
		if (!other.allTexturesUseSameMatTiling || !allTexturesUseSameMatTiling)
		{
			return false;
		}
		int num = -1;
		for (int i = 0; i < ts.Length; i++)
		{
			if (!ts[i].AreTexturesEqual(other.ts[i]))
			{
				return false;
			}
			if (num == -1 && !ts[i].isNull)
			{
				num = i;
			}
			if (considerNonTextureProperties && !resultMaterialTextureBlender.NonTexturePropertiesAreEqual(matsAndGOs.mats[0].mat, other.matsAndGOs.mats[0].mat))
			{
				return false;
			}
		}
		if (num != -1)
		{
			for (int j = 0; j < ts.Length; j++)
			{
				if (!ts[j].AreTexturesEqual(other.ts[j]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void DrawRectsToMergeGizmos(Color encC, Color innerC)
	{
		DRect A = ts[0].GetEncapsulatingSamplingRect();
		A.Expand(0.05f);
		Gizmos.color = encC;
		Gizmos.DrawWireCube(A.center.GetVector2(), A.size);
		for (int i = 0; i < matsAndGOs.mats.Count; i++)
		{
			DRect B = matsAndGOs.mats[i].samplingRectMatAndUVTiling;
			DRect r = MB3_UVTransformUtility.GetShiftTransformToFitBinA(ref A, ref B);
			Vector2 vector = MB3_UVTransformUtility.TransformPoint(ref r, B.min);
			B.x = vector.x;
			B.y = vector.y;
			Gizmos.color = innerC;
			Gizmos.DrawWireCube(B.center.GetVector2(), B.size);
		}
	}

	internal string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("[GAME_OBJS=");
		for (int i = 0; i < matsAndGOs.gos.Count; i++)
		{
			stringBuilder.AppendFormat("{0},", matsAndGOs.gos[i].name);
		}
		stringBuilder.AppendFormat("MATS=");
		for (int j = 0; j < matsAndGOs.mats.Count; j++)
		{
			stringBuilder.AppendFormat("{0},", matsAndGOs.mats[j].GetMaterialName());
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	internal string GetMatSubrectDescriptions()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < matsAndGOs.mats.Count; i++)
		{
			stringBuilder.AppendFormat("\n    {0}={1},", matsAndGOs.mats[i].GetMaterialName(), matsAndGOs.mats[i].samplingRectMatAndUVTiling);
		}
		return stringBuilder.ToString();
	}
}
