using System;
using UnityEngine;

namespace Unity.Cinemachine;

public static class Cinemachine3OrbitRig
{
	[Serializable]
	public struct Orbit
	{
		[Tooltip("Horizontal radius of the orbit")]
		public float Radius;

		[Tooltip("Height of the horizontal orbit circle, relative to the target position")]
		public float Height;
	}

	[Serializable]
	public struct Settings
	{
		[Tooltip("Value to take at the top of the axis range")]
		public Orbit Top;

		[Tooltip("Value to take at the center of the axis range")]
		public Orbit Center;

		[Tooltip("Value to take at the bottom of the axis range")]
		public Orbit Bottom;

		[Tooltip("Controls how taut is the line that connects the rigs' orbits, which determines final placement on the Y axis")]
		[Range(0f, 1f)]
		public float SplineCurvature;

		public static Settings Default
		{
			get
			{
				Settings result = default(Settings);
				result.SplineCurvature = 0.5f;
				result.Top = new Orbit
				{
					Height = 5f,
					Radius = 2f
				};
				result.Center = new Orbit
				{
					Height = 2.25f,
					Radius = 4f
				};
				result.Bottom = new Orbit
				{
					Height = 0.1f,
					Radius = 2.5f
				};
				return result;
			}
		}
	}

	internal struct OrbitSplineCache
	{
		private Settings OrbitSettings;

		private Vector4[] CachedKnots;

		private Vector4[] CachedCtrl1;

		private Vector4[] CachedCtrl2;

		public bool SettingsChanged(in Settings other)
		{
			if (OrbitSettings.SplineCurvature == other.SplineCurvature && OrbitSettings.Top.Height == other.Top.Height && OrbitSettings.Top.Radius == other.Top.Radius && OrbitSettings.Center.Height == other.Center.Height && OrbitSettings.Center.Radius == other.Center.Radius && OrbitSettings.Bottom.Height == other.Bottom.Height)
			{
				return OrbitSettings.Bottom.Radius != other.Bottom.Radius;
			}
			return true;
		}

		public void UpdateOrbitCache(in Settings orbits)
		{
			OrbitSettings = orbits;
			float splineCurvature = orbits.SplineCurvature;
			CachedKnots = new Vector4[5];
			CachedCtrl1 = new Vector4[5];
			CachedCtrl2 = new Vector4[5];
			CachedKnots[1] = new Vector4(0f, orbits.Bottom.Height, 0f - orbits.Bottom.Radius, -1f);
			CachedKnots[2] = new Vector4(0f, orbits.Center.Height, 0f - orbits.Center.Radius, 0f);
			CachedKnots[3] = new Vector4(0f, orbits.Top.Height, 0f - orbits.Top.Radius, 1f);
			CachedKnots[0] = Vector4.Lerp(CachedKnots[1] + (CachedKnots[1] - CachedKnots[2]) * 0.5f, Vector4.zero, splineCurvature);
			CachedKnots[4] = Vector4.Lerp(CachedKnots[3] + (CachedKnots[3] - CachedKnots[2]) * 0.5f, Vector4.zero, splineCurvature);
			SplineHelpers.ComputeSmoothControlPoints(ref CachedKnots, ref CachedCtrl1, ref CachedCtrl2);
		}

		public Vector4 SplineValue(float t)
		{
			if (CachedKnots == null)
			{
				return Vector4.zero;
			}
			int num = 1;
			if (t > 0.5f)
			{
				t -= 0.5f;
				num = 2;
			}
			Vector4 result = SplineHelpers.Bezier3(t * 2f, CachedKnots[num], CachedCtrl1[num], CachedCtrl2[num], CachedKnots[num + 1]);
			result.w = SplineHelpers.Bezier1(t * 2f, CachedKnots[num].w, CachedCtrl1[num].w, CachedCtrl2[num].w, CachedKnots[num + 1].w);
			return result;
		}
	}
}
