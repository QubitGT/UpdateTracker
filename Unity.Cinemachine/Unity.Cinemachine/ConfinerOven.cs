using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

internal class ConfinerOven
{
	private class FloatToIntScaler
	{
		private readonly long m_FloatToInt;

		private readonly float m_IntToFloat;

		public float ClipperEpsilon => 0.01f * (float)m_FloatToInt;

		public float FloatToInt(float f)
		{
			return f * (float)m_FloatToInt;
		}

		public float IntToFloat(long i)
		{
			return (float)i * m_IntToFloat;
		}

		public FloatToIntScaler(Rect polygonBounds)
		{
			float num = Mathf.Max(polygonBounds.width, polygonBounds.height);
			float t = Mathf.Max(0f, num - 100f) / 9900f;
			m_FloatToInt = (long)Mathf.Lerp(100000f, 100f, t);
			m_IntToFloat = 1f / (float)m_FloatToInt;
		}
	}

	public class BakedSolution
	{
		private float m_FrustumSizeIntSpace;

		private readonly AspectStretcher m_AspectStretcher;

		private readonly bool m_HasBones;

		private readonly double m_SqrPolygonDiagonal;

		private List<List<Point64>> m_OriginalPolygon;

		internal List<List<Point64>> m_Solution;

		private FloatToIntScaler m_FloatToInt;

		public BakedSolution(float aspectRatio, float frustumHeight, bool hasBones, Rect polygonBounds, List<List<Point64>> originalPolygon, List<List<Point64>> solution)
		{
			m_FloatToInt = new FloatToIntScaler(polygonBounds);
			m_AspectStretcher = new AspectStretcher(aspectRatio, polygonBounds.center.x);
			m_FrustumSizeIntSpace = m_FloatToInt.FloatToInt(frustumHeight);
			m_HasBones = hasBones;
			m_OriginalPolygon = originalPolygon;
			m_Solution = solution;
			float num = m_FloatToInt.FloatToInt(polygonBounds.width / aspectRatio);
			float num2 = m_FloatToInt.FloatToInt(polygonBounds.height);
			m_SqrPolygonDiagonal = num * num + num2 * num2;
		}

		public bool IsValid()
		{
			return m_Solution != null;
		}

		public Vector2 ConfinePoint(in Vector2 pointToConfine)
		{
			if (m_Solution.Count <= 0)
			{
				return pointToConfine;
			}
			Vector2 vector = m_AspectStretcher.Stretch(pointToConfine);
			Point64 point2 = new Point64(m_FloatToInt.FloatToInt(vector.x), m_FloatToInt.FloatToInt(vector.y));
			for (int i = 0; i < m_Solution.Count; i++)
			{
				if (Clipper.PointInPolygon(point2, m_Solution[i]) != PointInPolygonResult.IsOutside)
				{
					return pointToConfine;
				}
			}
			bool flag = m_HasBones && IsInsideOriginal(point2);
			Point64 point3 = point2;
			double num = double.MaxValue;
			for (int j = 0; j < m_Solution.Count; j++)
			{
				int count = m_Solution[j].Count;
				for (int k = 0; k < count; k++)
				{
					Point64 point4 = m_Solution[j][k];
					Point64 point5 = m_Solution[j][(k + 1) % count];
					Point64 point6 = IntPointLerp(point4, point5, ClosestPointOnSegment(point2, point4, point5));
					double num2 = Mathf.Abs(point2.X - point6.X);
					double num3 = Mathf.Abs(point2.Y - point6.Y);
					double num4 = num2 * num2 + num3 * num3;
					if (num2 > (double)m_FrustumSizeIntSpace || num3 > (double)m_FrustumSizeIntSpace)
					{
						num4 += m_SqrPolygonDiagonal;
					}
					if (num4 < num && (!flag || !DoesIntersectOriginal(point2, point6)))
					{
						num = num4;
						point3 = point6;
					}
				}
			}
			Vector2 p = new Vector2(m_FloatToInt.IntToFloat(point3.X), m_FloatToInt.IntToFloat(point3.Y));
			return m_AspectStretcher.Unstretch(p);
			float ClosestPointOnSegment(Point64 point, Point64 s0, Point64 s1)
			{
				double num5 = s1.X - s0.X;
				double num6 = s1.Y - s0.Y;
				double num7 = num5 * num5 + num6 * num6;
				if (num7 < (double)m_FloatToInt.ClipperEpsilon)
				{
					return 0f;
				}
				double num8 = point.X - s0.X;
				double num9 = point.Y - s0.Y;
				return Mathf.Clamp01((float)((num8 * num5 + num9 * num6) / num7));
			}
			bool DoesIntersectOriginal(Point64 l1, Point64 l2)
			{
				double epsilon = m_FloatToInt.ClipperEpsilon;
				for (int m = 0; m < m_OriginalPolygon.Count; m++)
				{
					List<Point64> list = m_OriginalPolygon[m];
					int count2 = list.Count;
					for (int n = 0; n < count2; n++)
					{
						Point64 p2 = list[n];
						Point64 p3 = list[(n + 1) % count2];
						if (FindIntersection(in l1, in l2, in p2, in p3, epsilon) == 2)
						{
							return true;
						}
					}
				}
				return false;
			}
			static Point64 IntPointLerp(Point64 a, Point64 b, float lerp)
			{
				Point64 result = default(Point64);
				result.X = Mathf.RoundToInt((float)a.X + (float)(b.X - a.X) * lerp);
				result.Y = Mathf.RoundToInt((float)a.Y + (float)(b.Y - a.Y) * lerp);
				return result;
			}
			bool IsInsideOriginal(Point64 point)
			{
				for (int num10 = 0; num10 < m_OriginalPolygon.Count; num10++)
				{
					if (Clipper.PointInPolygon(point, m_OriginalPolygon[num10]) != PointInPolygonResult.IsOutside)
					{
						return true;
					}
				}
				return false;
			}
		}

		private static int FindIntersection(in Point64 p1, in Point64 p2, in Point64 p3, in Point64 p4, double epsilon)
		{
			double num = p2.X - p1.X;
			double num2 = p2.Y - p1.Y;
			double num3 = p4.X - p3.X;
			double num4 = p4.Y - p3.Y;
			double num5 = num2 * num3 - num * num4;
			double num6 = ((double)(p1.X - p3.X) * num4 + (double)(p3.Y - p1.Y) * num3) / num5;
			if (double.IsInfinity(num6) || double.IsNaN(num6))
			{
				if (IntPointDiffSqrMagnitude(p1, p3) < epsilon || IntPointDiffSqrMagnitude(p1, p4) < epsilon || IntPointDiffSqrMagnitude(p2, p3) < epsilon || IntPointDiffSqrMagnitude(p2, p4) < epsilon)
				{
					return 2;
				}
				return 0;
			}
			double num7 = ((double)(p3.X - p1.X) * num2 + (double)(p1.Y - p3.Y) * num) / (0.0 - num5);
			if (!(num6 >= 0.0) || !(num6 <= 1.0) || !(num7 >= 0.0) || !(num7 < 1.0))
			{
				return 1;
			}
			return 2;
			static double IntPointDiffSqrMagnitude(Point64 point1, Point64 point2)
			{
				double num8 = point1.X - point2.X;
				double num9 = point1.Y - point2.Y;
				return num8 * num8 + num9 * num9;
			}
		}
	}

	private readonly struct AspectStretcher
	{
		private readonly float m_InverseAspect;

		private readonly float m_CenterX;

		public float Aspect { get; }

		public AspectStretcher(float aspect, float centerX)
		{
			Aspect = aspect;
			m_InverseAspect = 1f / Aspect;
			m_CenterX = centerX;
		}

		public Vector2 Stretch(Vector2 p)
		{
			return new Vector2((p.x - m_CenterX) * m_InverseAspect + m_CenterX, p.y);
		}

		public Vector2 Unstretch(Vector2 p)
		{
			return new Vector2((p.x - m_CenterX) * Aspect + m_CenterX, p.y);
		}
	}

	private struct PolygonSolution
	{
		public List<List<Point64>> polygons;

		public float frustumHeight;

		public bool IsNull => polygons == null;

		public bool StateChanged(in List<List<Point64>> paths)
		{
			if (paths.Count != polygons.Count)
			{
				return true;
			}
			for (int i = 0; i < paths.Count; i++)
			{
				if (paths[i].Count != polygons[i].Count)
				{
					return true;
				}
			}
			return false;
		}
	}

	public enum BakingState
	{
		BAKING,
		BAKED,
		TIMEOUT
	}

	private struct BakingStateCache
	{
		public ClipperOffset offsetter;

		public List<PolygonSolution> solutions;

		public PolygonSolution rightCandidate;

		public PolygonSolution leftCandidate;

		public List<List<Point64>> userSetMaxCandidate;

		public List<List<Point64>> theoreticalMaxCandidate;

		public float stepSize;

		public float maxFrustumHeight;

		public float userSetMaxFrustumHeight;

		public float theoreticalMaxFrustumHeight;

		public float currentFrustumHeight;

		public float bakeTime;
	}

	private float m_MinFrustumHeightWithBones;

	private float m_SkeletonPadding;

	private List<List<Point64>> m_OriginalPolygon;

	private Point64 m_MidPoint;

	internal List<List<Point64>> m_Skeleton = new List<List<Point64>>();

	private FloatToIntScaler m_FloatToInt;

	private const int k_MiterLimit = 2;

	private const float k_MaxComputationTimeForFullSkeletonBakeInSeconds = 5f;

	private Rect m_PolygonRect;

	private AspectStretcher m_AspectStretcher = new AspectStretcher(1f, 0f);

	public float bakeProgress;

	private BakingStateCache m_Cache;

	public BakingState State { get; private set; }

	public ConfinerOven(in List<List<Vector2>> inputPath, in float aspectRatio, float maxFrustumHeight, float skeletonPadding)
	{
		Initialize(in inputPath, in aspectRatio, maxFrustumHeight, Mathf.Max(0f, skeletonPadding) + 1f);
	}

	public BakedSolution GetBakedSolution(float frustumHeight)
	{
		if (m_Cache.userSetMaxFrustumHeight > 0f)
		{
			frustumHeight = Mathf.Min(m_Cache.userSetMaxFrustumHeight, frustumHeight);
		}
		if (State == BakingState.BAKED && frustumHeight >= m_Cache.theoreticalMaxFrustumHeight)
		{
			return new BakedSolution(m_AspectStretcher.Aspect, frustumHeight, hasBones: false, m_PolygonRect, m_OriginalPolygon, m_Cache.theoreticalMaxCandidate);
		}
		ClipperOffset clipperOffset = new ClipperOffset();
		clipperOffset.AddPaths(m_OriginalPolygon, JoinType.Miter, EndType.Polygon);
		List<List<Point64>> list = clipperOffset.Execute(-1f * m_FloatToInt.FloatToInt(frustumHeight));
		if (list.Count == 0)
		{
			list = m_Cache.theoreticalMaxCandidate;
		}
		List<List<Point64>> list2 = new List<List<Point64>>();
		if (State == BakingState.BAKING || m_Skeleton.Count == 0)
		{
			list2 = list;
		}
		else
		{
			Clipper64 clipper = new Clipper64();
			clipper.AddSubject(list);
			clipper.AddClip(m_Skeleton);
			clipper.Execute(ClipType.Union, FillRule.EvenOdd, list2);
		}
		return new BakedSolution(m_AspectStretcher.Aspect, frustumHeight, m_MinFrustumHeightWithBones < frustumHeight, m_PolygonRect, m_OriginalPolygon, list2);
	}

	private void Initialize(in List<List<Vector2>> inputPath, in float aspectRatio, float maxFrustumHeight, float skeletonPadding)
	{
		m_Skeleton.Clear();
		m_Cache.userSetMaxFrustumHeight = maxFrustumHeight;
		m_MinFrustumHeightWithBones = float.MaxValue;
		m_SkeletonPadding = skeletonPadding;
		m_PolygonRect = GetPolygonBoundingBox(in inputPath);
		m_AspectStretcher = new AspectStretcher(aspectRatio, m_PolygonRect.center.x);
		m_FloatToInt = new FloatToIntScaler(m_PolygonRect);
		m_Cache.theoreticalMaxFrustumHeight = Mathf.Max(m_PolygonRect.width / aspectRatio, m_PolygonRect.height) / 2f;
		m_OriginalPolygon = new List<List<Point64>>(inputPath.Count);
		for (int i = 0; i < inputPath.Count; i++)
		{
			List<Vector2> list = inputPath[i];
			int count = list.Count;
			List<Point64> list2 = new List<Point64>(count);
			for (int j = 0; j < count; j++)
			{
				Vector2 vector = m_AspectStretcher.Stretch(list[j]);
				list2.Add(new Point64(m_FloatToInt.FloatToInt(vector.x), m_FloatToInt.FloatToInt(vector.y)));
			}
			m_OriginalPolygon.Add(list2);
		}
		m_MidPoint = MidPointOfIntRect(Clipper.GetBounds(m_OriginalPolygon));
		m_Cache.theoreticalMaxCandidate = new List<List<Point64>>
		{
			new List<Point64> { m_MidPoint }
		};
		if (m_Cache.userSetMaxFrustumHeight < 0f)
		{
			State = BakingState.BAKED;
			return;
		}
		m_Cache.offsetter = new ClipperOffset();
		m_Cache.offsetter.AddPaths(m_OriginalPolygon, JoinType.Miter, EndType.Polygon);
		m_Cache.maxFrustumHeight = m_Cache.userSetMaxFrustumHeight;
		if (m_Cache.maxFrustumHeight == 0f || m_Cache.maxFrustumHeight > m_Cache.theoreticalMaxFrustumHeight)
		{
			m_Cache.maxFrustumHeight = m_Cache.theoreticalMaxFrustumHeight;
			m_Cache.userSetMaxCandidate = m_Cache.theoreticalMaxCandidate;
		}
		else
		{
			m_Cache.userSetMaxCandidate = new List<List<Point64>>(m_Cache.offsetter.Execute(-1f * m_FloatToInt.FloatToInt(m_Cache.userSetMaxFrustumHeight)));
			if (m_Cache.userSetMaxCandidate.Count == 0)
			{
				m_Cache.userSetMaxCandidate = m_Cache.theoreticalMaxCandidate;
			}
		}
		m_Cache.stepSize = m_Cache.maxFrustumHeight;
		List<List<Point64>> polygons2 = new List<List<Point64>>(m_Cache.offsetter.Execute(0.0));
		m_Cache.solutions = new List<PolygonSolution>();
		m_Cache.solutions.Add(new PolygonSolution
		{
			polygons = polygons2,
			frustumHeight = 0f
		});
		m_Cache.rightCandidate = default(PolygonSolution);
		m_Cache.leftCandidate = new PolygonSolution
		{
			polygons = polygons2,
			frustumHeight = 0f
		};
		m_Cache.currentFrustumHeight = 0f;
		m_Cache.bakeTime = 0f;
		State = BakingState.BAKING;
		bakeProgress = 0f;
		static Rect GetPolygonBoundingBox(in List<List<Vector2>> polygons)
		{
			float num = float.PositiveInfinity;
			float num2 = float.NegativeInfinity;
			float num3 = float.PositiveInfinity;
			float num4 = float.NegativeInfinity;
			for (int k = 0; k < polygons.Count; k++)
			{
				for (int l = 0; l < polygons[k].Count; l++)
				{
					Vector2 vector2 = polygons[k][l];
					num = Mathf.Min(num, vector2.x);
					num2 = Mathf.Max(num2, vector2.x);
					num3 = Mathf.Min(num3, vector2.y);
					num4 = Mathf.Max(num4, vector2.y);
				}
			}
			return new Rect(num, num3, Mathf.Max(0f, num2 - num), Mathf.Max(0f, num4 - num3));
		}
		static Point64 MidPointOfIntRect(Rect64 bounds)
		{
			return new Point64((bounds.left + bounds.right) / 2, (bounds.top + bounds.bottom) / 2);
		}
	}

	public void BakeConfiner(float maxComputationTimePerFrameInSeconds)
	{
		if (State != 0)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = m_FloatToInt.IntToFloat(50L);
		while (m_Cache.solutions.Count < 1000)
		{
			m_Cache.stepSize = Mathf.Min(m_Cache.stepSize, m_Cache.maxFrustumHeight - m_Cache.leftCandidate.frustumHeight);
			m_Cache.currentFrustumHeight = m_Cache.leftCandidate.frustumHeight + m_Cache.stepSize;
			List<List<Point64>> paths = ((Math.Abs(m_Cache.currentFrustumHeight - m_Cache.maxFrustumHeight) < 0.0001f) ? m_Cache.userSetMaxCandidate : m_Cache.offsetter.Execute(-1f * m_FloatToInt.FloatToInt(m_Cache.currentFrustumHeight)));
			if (paths.Count == 0)
			{
				paths = m_Cache.userSetMaxCandidate;
			}
			if (m_Cache.leftCandidate.StateChanged(in paths))
			{
				m_Cache.rightCandidate = new PolygonSolution
				{
					polygons = new List<List<Point64>>(paths),
					frustumHeight = m_Cache.currentFrustumHeight
				};
				m_Cache.stepSize = Mathf.Max(m_Cache.stepSize / 2f, num);
			}
			else
			{
				m_Cache.leftCandidate = new PolygonSolution
				{
					polygons = new List<List<Point64>>(paths),
					frustumHeight = m_Cache.currentFrustumHeight
				};
				if (!m_Cache.rightCandidate.IsNull)
				{
					m_Cache.stepSize = Mathf.Max(m_Cache.stepSize / 2f, num);
				}
			}
			if (!m_Cache.rightCandidate.IsNull && m_Cache.stepSize <= num)
			{
				m_Cache.solutions.Add(m_Cache.leftCandidate);
				m_Cache.solutions.Add(m_Cache.rightCandidate);
				m_Cache.leftCandidate = m_Cache.rightCandidate;
				m_Cache.rightCandidate = default(PolygonSolution);
				m_Cache.stepSize = m_Cache.maxFrustumHeight;
			}
			else if (m_Cache.rightCandidate.IsNull || m_Cache.leftCandidate.frustumHeight >= m_Cache.maxFrustumHeight)
			{
				m_Cache.solutions.Add(m_Cache.leftCandidate);
				break;
			}
			float num2 = Time.realtimeSinceStartup - realtimeSinceStartup;
			if (num2 > maxComputationTimePerFrameInSeconds)
			{
				m_Cache.bakeTime += num2;
				if (m_Cache.bakeTime > 5f)
				{
					State = BakingState.TIMEOUT;
				}
				bakeProgress = m_Cache.leftCandidate.frustumHeight / m_Cache.maxFrustumHeight;
				return;
			}
		}
		ComputeSkeleton(in m_Cache.solutions);
		for (int num3 = m_Cache.solutions.Count - 1; num3 >= 0; num3--)
		{
			if (m_Cache.solutions[num3].polygons.Count == 0)
			{
				m_Cache.solutions.RemoveAt(num3);
			}
		}
		bakeProgress = 1f;
		State = BakingState.BAKED;
		void ComputeSkeleton(in List<PolygonSolution> solutions)
		{
			Clipper64 clipper = new Clipper64();
			ClipperOffset clipperOffset = new ClipperOffset();
			for (int i = 1; i < solutions.Count - 1; i += 2)
			{
				PolygonSolution polygonSolution = solutions[i];
				PolygonSolution polygonSolution2 = solutions[i + 1];
				double num4 = m_FloatToInt.FloatToInt(m_SkeletonPadding) * (polygonSolution2.frustumHeight - polygonSolution.frustumHeight);
				clipperOffset.Clear();
				clipperOffset.AddPaths(polygonSolution.polygons, JoinType.Miter, EndType.Polygon);
				List<List<Point64>> paths2 = new List<List<Point64>>(clipperOffset.Execute(num4));
				clipperOffset.Clear();
				clipperOffset.AddPaths(polygonSolution2.polygons, JoinType.Miter, EndType.Polygon);
				List<List<Point64>> paths3 = new List<List<Point64>>(clipperOffset.Execute(num4 * 2.0));
				List<List<Point64>> list = new List<List<Point64>>();
				clipper.Clear();
				clipper.AddSubject(paths2);
				clipper.AddClip(paths3);
				clipper.Execute(ClipType.Difference, FillRule.EvenOdd, list);
				if (list.Count > 0 && list[0].Count > 0)
				{
					m_Skeleton.AddRange(list);
					if (m_MinFrustumHeightWithBones == float.MaxValue)
					{
						m_MinFrustumHeightWithBones = polygonSolution2.frustumHeight;
					}
				}
			}
		}
	}
}
