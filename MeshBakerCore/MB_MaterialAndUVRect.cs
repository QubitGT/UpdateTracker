using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MB_MaterialAndUVRect
{
	public Material material;

	public Rect atlasRect;

	public string srcObjName;

	public int textureArraySliceIdx = -1;

	public bool allPropsUseSameTiling = true;

	[FormerlySerializedAs("sourceMaterialTiling")]
	public Rect allPropsUseSameTiling_sourceMaterialTiling;

	[FormerlySerializedAs("samplingEncapsulatinRect")]
	public Rect allPropsUseSameTiling_samplingEncapsulatinRect;

	public Rect propsUseDifferntTiling_srcUVsamplingRect;

	[NonSerialized]
	public List<GameObject> objectsThatUse;

	public MB_TextureTilingTreatment tilingTreatment = MB_TextureTilingTreatment.unknown;

	public MB_MaterialAndUVRect(Material mat, Rect destRect, bool allPropsUseSameTiling, Rect sourceMaterialTiling, Rect samplingEncapsulatingRect, Rect srcUVsamplingRect, MB_TextureTilingTreatment treatment, string objName)
	{
		material = mat;
		atlasRect = destRect;
		tilingTreatment = treatment;
		this.allPropsUseSameTiling = allPropsUseSameTiling;
		allPropsUseSameTiling_sourceMaterialTiling = sourceMaterialTiling;
		allPropsUseSameTiling_samplingEncapsulatinRect = samplingEncapsulatingRect;
		propsUseDifferntTiling_srcUVsamplingRect = srcUVsamplingRect;
		srcObjName = objName;
	}

	public override int GetHashCode()
	{
		return material.GetInstanceID() ^ allPropsUseSameTiling_samplingEncapsulatinRect.GetHashCode() ^ propsUseDifferntTiling_srcUVsamplingRect.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MB_MaterialAndUVRect))
		{
			return false;
		}
		MB_MaterialAndUVRect mB_MaterialAndUVRect = (MB_MaterialAndUVRect)obj;
		if (material == mB_MaterialAndUVRect.material && allPropsUseSameTiling_samplingEncapsulatinRect == mB_MaterialAndUVRect.allPropsUseSameTiling_samplingEncapsulatinRect && allPropsUseSameTiling_sourceMaterialTiling == mB_MaterialAndUVRect.allPropsUseSameTiling_sourceMaterialTiling && allPropsUseSameTiling == mB_MaterialAndUVRect.allPropsUseSameTiling)
		{
			return propsUseDifferntTiling_srcUVsamplingRect == mB_MaterialAndUVRect.propsUseDifferntTiling_srcUVsamplingRect;
		}
		return false;
	}

	public Rect GetEncapsulatingRect()
	{
		if (allPropsUseSameTiling)
		{
			return allPropsUseSameTiling_samplingEncapsulatinRect;
		}
		return propsUseDifferntTiling_srcUVsamplingRect;
	}

	public Rect GetMaterialTilingRect()
	{
		if (allPropsUseSameTiling)
		{
			return allPropsUseSameTiling_sourceMaterialTiling;
		}
		return new Rect(0f, 0f, 1f, 1f);
	}
}
