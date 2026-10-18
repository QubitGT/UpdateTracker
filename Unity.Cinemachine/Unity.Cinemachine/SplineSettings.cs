using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[Serializable]
public struct SplineSettings
{
	[Tooltip("The Spline container to which the position will apply.")]
	public SplineContainer Spline;

	[NoSaveDuringPlay]
	[Tooltip("The position along the spline.  The actual value corresponding to a given point on the spline will depend on the unity type.")]
	public float Position;

	[Tooltip("How to interpret the Spline Position:\n- <b>Distance</b>: Values range from 0 (start of Spline) to Length of the Spline (end of Spline).\n- <b>Normalized</b>: Values range from 0 (start of Spline) to 1 (end of Spline).\n- <b>Knot</b>: Values are defined by knot indices and a fractional value representing the normalized interpolation between the specific knot index and the next knot.\n")]
	public PathIndexUnit Units;

	private CachedScaledSpline m_CachedSpline;

	private int m_CachedFrame;

	public void ChangeUnitPreservePosition(PathIndexUnit newUnits)
	{
		if (Spline.IsValid() && newUnits != Units)
		{
			Position = GetCachedSpline().ConvertIndexUnit(Position, Units, newUnits);
		}
		Units = newUnits;
	}

	internal CachedScaledSpline GetCachedSpline()
	{
		if (!Spline.IsValid())
		{
			InvalidateCache();
		}
		else
		{
			if (m_CachedSpline == null || (Time.frameCount != m_CachedFrame && !m_CachedSpline.IsCrudelyValid(Spline.Spline, Spline.transform)))
			{
				InvalidateCache();
				m_CachedSpline = new CachedScaledSpline(Spline.Spline, Spline.transform);
			}
			m_CachedFrame = Time.frameCount;
		}
		return m_CachedSpline;
	}

	public void InvalidateCache()
	{
		m_CachedSpline?.Dispose();
		m_CachedSpline = null;
	}
}
