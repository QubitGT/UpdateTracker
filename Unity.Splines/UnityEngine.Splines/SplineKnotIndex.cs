using System;

namespace UnityEngine.Splines;

[Serializable]
public struct SplineKnotIndex : IEquatable<SplineKnotIndex>
{
	public static SplineKnotIndex Invalid = new SplineKnotIndex(-1, -1);

	public int Spline;

	public int Knot;

	public SplineKnotIndex(int spline, int knot)
	{
		Spline = spline;
		Knot = knot;
	}

	public static bool operator ==(SplineKnotIndex indexA, SplineKnotIndex indexB)
	{
		return indexA.Equals(indexB);
	}

	public static bool operator !=(SplineKnotIndex indexA, SplineKnotIndex indexB)
	{
		return !indexA.Equals(indexB);
	}

	public bool Equals(SplineKnotIndex otherIndex)
	{
		if (Spline == otherIndex.Spline)
		{
			return Knot == otherIndex.Knot;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is SplineKnotIndex otherIndex)
		{
			return Equals(otherIndex);
		}
		return false;
	}

	public bool IsValid()
	{
		if (Spline >= 0)
		{
			return Knot >= 0;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Spline * 397) ^ Knot;
	}

	public override string ToString()
	{
		return $"{{{Spline}, {Knot}}}";
	}
}
