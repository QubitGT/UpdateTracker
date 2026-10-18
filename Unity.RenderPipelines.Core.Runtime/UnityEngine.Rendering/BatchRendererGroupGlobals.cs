using System;

namespace UnityEngine.Rendering;

[Serializable]
[Obsolete("BatchRendererGroupGlobals and associated cbuffer are now set automatically by Unity. Setting it manually is no longer necessary or supported.")]
public struct BatchRendererGroupGlobals : IEquatable<BatchRendererGroupGlobals>
{
	public const string kGlobalsPropertyName = "unity_DOTSInstanceGlobalValues";

	public static readonly int kGlobalsPropertyId = Shader.PropertyToID("unity_DOTSInstanceGlobalValues");

	public Vector4 ProbesOcclusion;

	public Vector4 SpecCube0_HDR;

	public Vector4 SpecCube1_HDR;

	public SHCoefficients SHCoefficients;

	public static BatchRendererGroupGlobals Default
	{
		get
		{
			BatchRendererGroupGlobals result = default(BatchRendererGroupGlobals);
			result.ProbesOcclusion = Vector4.one;
			result.SpecCube0_HDR = ReflectionProbe.defaultTextureHDRDecodeValues;
			result.SpecCube1_HDR = result.SpecCube0_HDR;
			result.SHCoefficients = new SHCoefficients(RenderSettings.ambientProbe);
			return result;
		}
	}

	public bool Equals(BatchRendererGroupGlobals other)
	{
		if (ProbesOcclusion.Equals(other.ProbesOcclusion) && SpecCube0_HDR.Equals(other.SpecCube0_HDR) && SpecCube1_HDR.Equals(other.SpecCube1_HDR))
		{
			return SHCoefficients.Equals(other.SHCoefficients);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BatchRendererGroupGlobals other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ProbesOcclusion, SpecCube0_HDR, SpecCube1_HDR, SHCoefficients);
	}

	public static bool operator ==(BatchRendererGroupGlobals left, BatchRendererGroupGlobals right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BatchRendererGroupGlobals left, BatchRendererGroupGlobals right)
	{
		return !left.Equals(right);
	}
}
