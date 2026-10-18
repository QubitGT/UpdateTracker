using System;

namespace UnityEngine.Rendering;

[Serializable]
public struct SHCoefficients : IEquatable<SHCoefficients>
{
	public Vector4 SHAr;

	public Vector4 SHAg;

	public Vector4 SHAb;

	public Vector4 SHBr;

	public Vector4 SHBg;

	public Vector4 SHBb;

	public Vector4 SHC;

	public Vector4 ProbesOcclusion;

	public SHCoefficients(SphericalHarmonicsL2 sh)
	{
		SHAr = GetSHA(sh, 0);
		SHAg = GetSHA(sh, 1);
		SHAb = GetSHA(sh, 2);
		SHBr = GetSHB(sh, 0);
		SHBg = GetSHB(sh, 1);
		SHBb = GetSHB(sh, 2);
		SHC = GetSHC(sh);
		ProbesOcclusion = Vector4.one;
	}

	public SHCoefficients(SphericalHarmonicsL2 sh, Vector4 probesOcclusion)
		: this(sh)
	{
		ProbesOcclusion = probesOcclusion;
	}

	private static Vector4 GetSHA(SphericalHarmonicsL2 sh, int i)
	{
		return new Vector4(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]);
	}

	private static Vector4 GetSHB(SphericalHarmonicsL2 sh, int i)
	{
		return new Vector4(sh[i, 4], sh[i, 5], sh[i, 6] * 3f, sh[i, 7]);
	}

	private static Vector4 GetSHC(SphericalHarmonicsL2 sh)
	{
		return new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1f);
	}

	public bool Equals(SHCoefficients other)
	{
		if (SHAr.Equals(other.SHAr) && SHAg.Equals(other.SHAg) && SHAb.Equals(other.SHAb) && SHBr.Equals(other.SHBr) && SHBg.Equals(other.SHBg) && SHBb.Equals(other.SHBb) && SHC.Equals(other.SHC))
		{
			return ProbesOcclusion.Equals(other.ProbesOcclusion);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SHCoefficients other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(SHAr, SHAg, SHAb, SHBr, SHBg, SHBb, SHC, ProbesOcclusion);
	}

	public static bool operator ==(SHCoefficients left, SHCoefficients right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SHCoefficients left, SHCoefficients right)
	{
		return !left.Equals(right);
	}
}
