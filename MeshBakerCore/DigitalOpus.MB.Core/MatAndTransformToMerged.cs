using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MatAndTransformToMerged
{
	public Material mat;

	public string objName;

	public DRect obUVRectIfTilingSame { get; private set; }

	public DRect samplingRectMatAndUVTiling { get; private set; }

	public DRect materialTiling { get; private set; }

	public MatAndTransformToMerged(DRect obUVrect, bool fixOutOfBoundsUVs)
	{
		_init(obUVrect, fixOutOfBoundsUVs, null);
	}

	public MatAndTransformToMerged(DRect obUVrect, bool fixOutOfBoundsUVs, Material m)
	{
		_init(obUVrect, fixOutOfBoundsUVs, m);
	}

	private void _init(DRect obUVrect, bool fixOutOfBoundsUVs, Material m)
	{
		if (fixOutOfBoundsUVs)
		{
			obUVRectIfTilingSame = obUVrect;
		}
		else
		{
			obUVRectIfTilingSame = new DRect(0f, 0f, 1f, 1f);
		}
		mat = m;
	}

	public override bool Equals(object obj)
	{
		if (obj is MatAndTransformToMerged)
		{
			MatAndTransformToMerged matAndTransformToMerged = (MatAndTransformToMerged)obj;
			if (matAndTransformToMerged.mat == mat && matAndTransformToMerged.obUVRectIfTilingSame == obUVRectIfTilingSame)
			{
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return mat.GetHashCode() ^ obUVRectIfTilingSame.GetHashCode() ^ samplingRectMatAndUVTiling.GetHashCode();
	}

	public string GetMaterialName()
	{
		if (mat != null)
		{
			return mat.name;
		}
		if (objName != null)
		{
			return $"[matFor: {objName}]";
		}
		return "Unknown";
	}

	public void AssignInitialValuesForMaterialTilingAndSamplingRectMatAndUVTiling(bool allTexturesUseSameMatTiling, DRect matTiling)
	{
		if (allTexturesUseSameMatTiling)
		{
			materialTiling = matTiling;
		}
		else
		{
			materialTiling = new DRect(0f, 0f, 1f, 1f);
		}
		DRect r = materialTiling;
		DRect r2 = obUVRectIfTilingSame;
		samplingRectMatAndUVTiling = MB3_UVTransformUtility.CombineTransforms(ref r2, ref r);
	}
}
