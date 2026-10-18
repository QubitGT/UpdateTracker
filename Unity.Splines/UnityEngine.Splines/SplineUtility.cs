using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public static class SplineUtility
{
	private struct Segment
	{
		public float start;

		public float length;

		public Segment(float start, float length)
		{
			this.start = start;
			this.length = length;
		}
	}

	private const int k_SubdivisionCountMin = 6;

	private const int k_SubdivisionCountMax = 1024;

	public const float DefaultTension = 1f / 3f;

	public const float CatmullRomTension = 0.5f;

	public const int PickResolutionMin = 2;

	public const int PickResolutionDefault = 4;

	public const int PickResolutionMax = 64;

	public const int DrawResolutionDefault = 10;

	public static bool Evaluate<T>(this T spline, float t, out float3 position, out float3 tangent, out float3 upVector) where T : ISpline
	{
		if (spline.Count < 1)
		{
			position = float3.zero;
			tangent = new float3(0f, 0f, 1f);
			upVector = new float3(0f, 1f, 0f);
			return false;
		}
		float curveT;
		int index = spline.SplineToCurveT(t, out curveT);
		BezierCurve curve = spline.GetCurve(index);
		position = CurveUtility.EvaluatePosition(curve, curveT);
		tangent = CurveUtility.EvaluateTangent(curve, curveT);
		upVector = spline.GetCurveUpVector(index, curveT);
		return true;
	}

	public static bool EvaluateNurbs(float t, List<float3> controlPoints, List<double> knotVector, int order, out float3 position)
	{
		position = float3.zero;
		if (knotVector.Count < controlPoints.Count + order - 1 || controlPoints.Count < order || t < 0f || 1f < t)
		{
			return false;
		}
		knotVector = new List<double>(knotVector);
		double num = knotVector[0];
		double num2 = knotVector[knotVector.Count - 1] - knotVector[0];
		if (knotVector[0] != 0.0 || knotVector[knotVector.Count - 1] != 1.0)
		{
			for (int i = 0; i < knotVector.Count; i++)
			{
				knotVector[i] = (knotVector[i] - num) / num2;
			}
		}
		int j;
		for (j = order; j < controlPoints.Count && knotVector[j] <= (double)t; j++)
		{
		}
		j--;
		float[] nurbsBasisFunctions = GetNurbsBasisFunctions(order, t, knotVector, j);
		for (int k = 0; k < order; k++)
		{
			position += nurbsBasisFunctions[k] * controlPoints[j - order + 1 + k];
		}
		return true;
	}

	private static float[] GetNurbsBasisFunctions(int degree, float t, List<double> knotVector, int span)
	{
		float[] array = new float[degree];
		float[] array2 = new float[degree];
		float[] array3 = new float[degree];
		for (int i = 0; i < degree; i++)
		{
			array[i] = 0f;
			array2[i] = 0f;
			array3[i] = 1f;
		}
		for (int j = 1; j < degree; j++)
		{
			array[j] = (float)((double)t - knotVector[span + 1 - j]);
			array2[j] = (float)(knotVector[span + j] - (double)t);
			float num = 0f;
			for (int k = 0; k < j; k++)
			{
				float num2 = array3[k] / (array2[k + 1] + array[j - k]);
				array3[k] = num + array2[k + 1] * num2;
				num = array[j - k] * num2;
			}
			array3[j] = num;
		}
		return array3;
	}

	public static float3 EvaluatePosition<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return float.PositiveInfinity;
		}
		float curveT;
		return CurveUtility.EvaluatePosition(spline.GetCurve(spline.SplineToCurveT(t, out curveT)), curveT);
	}

	public static float3 EvaluateTangent<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return float.PositiveInfinity;
		}
		float curveT;
		return CurveUtility.EvaluateTangent(spline.GetCurve(spline.SplineToCurveT(t, out curveT)), curveT);
	}

	public static float3 EvaluateUpVector<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return float3.zero;
		}
		float curveT;
		int index = spline.SplineToCurveT(t, out curveT);
		return spline.GetCurveUpVector(index, curveT);
	}

	public static float3 CalculateUpVector<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return float3.zero;
		}
		float curveT;
		int curveIndex = spline.SplineToCurveT(t, out curveT);
		return spline.CalculateUpVector(curveIndex, curveT);
	}

	internal static float3 CalculateUpVector<T>(this T spline, int curveIndex, float curveT) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return float3.zero;
		}
		BezierCurve curve = spline.GetCurve(curveIndex);
		float3 @float = math.rotate(spline[curveIndex].Rotation, math.up());
		if (curveT == 0f)
		{
			return @float;
		}
		int index = spline.NextIndex(curveIndex);
		float3 float2 = math.rotate(spline[index].Rotation, math.up());
		if (curveT == 1f)
		{
			return float2;
		}
		return CurveUtility.EvaluateUpVector(curve, curveT, @float, float2);
	}

	internal static void EvaluateUpVectorsForCurve<T>(this T spline, int curveIndex, NativeArray<float3> upVectors) where T : ISpline
	{
		if (spline.Count >= 1 && upVectors.IsCreated)
		{
			float3 startUp = math.rotate(spline[curveIndex].Rotation, math.up());
			int index = spline.NextIndex(curveIndex);
			float3 endUp = math.rotate(spline[index].Rotation, math.up());
			CurveUtility.EvaluateUpVectors(spline.GetCurve(curveIndex), startUp, endUp, upVectors);
		}
	}

	internal static void EvaluateUpVectorsForCurve<T>(this T spline, int curveIndex, float3[] upVectors) where T : ISpline
	{
		if (upVectors != null)
		{
			NativeArray<float3> upVectors2 = new NativeArray<float3>(upVectors, Allocator.Temp);
			spline.EvaluateUpVectorsForCurve(curveIndex, upVectors2);
			upVectors2.CopyTo(upVectors);
		}
	}

	public static float3 EvaluateAcceleration<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return float3.zero;
		}
		float curveT;
		return CurveUtility.EvaluateAcceleration(spline.GetCurve(spline.SplineToCurveT(t, out curveT)), curveT);
	}

	public static float EvaluateCurvature<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return 0f;
		}
		float curveT;
		int index = spline.SplineToCurveT(t, out curveT);
		return CurveUtility.EvaluateCurvature(spline.GetCurve(index), curveT);
	}

	public static float3 EvaluateCurvatureCenter<T>(this T spline, float t) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return 0f;
		}
		float curveT;
		int index = spline.SplineToCurveT(t, out curveT);
		BezierCurve curve = spline.GetCurve(index);
		float num = CurveUtility.EvaluateCurvature(curve, curveT);
		if (num != 0f)
		{
			float num2 = 1f / num;
			float3 @float = CurveUtility.EvaluatePosition(curve, curveT);
			float3 float2 = CurveUtility.EvaluateTangent(curve, curveT);
			float3 y = math.normalize(math.cross(CurveUtility.EvaluateAcceleration(curve, curveT), float2));
			float3 float3 = math.normalize(math.cross(float2, y));
			return @float + num2 * float3;
		}
		return float3.zero;
	}

	public static int SplineToCurveT<T>(this T spline, float splineT, out float curveT) where T : ISpline
	{
		return spline.SplineToCurveT(splineT, out curveT, useLUT: true);
	}

	private static int SplineToCurveT<T>(this T spline, float splineT, out float curveT, bool useLUT) where T : ISpline
	{
		int count = spline.Count;
		if (count <= 1)
		{
			curveT = 0f;
			return 0;
		}
		splineT = math.clamp(splineT, 0f, 1f);
		float num = splineT * spline.GetLength();
		float num2 = 0f;
		bool closed = spline.Closed;
		int i = 0;
		for (int num3 = (closed ? count : (count - 1)); i < num3; i++)
		{
			int num4 = i % count;
			float curveLength = spline.GetCurveLength(num4);
			if (num <= num2 + curveLength)
			{
				curveT = (useLUT ? spline.GetCurveInterpolation(num4, num - num2) : ((num - num2) / curveLength));
				return num4;
			}
			num2 += curveLength;
		}
		curveT = 1f;
		if (!closed)
		{
			return count - 2;
		}
		return count - 1;
	}

	public static float CurveToSplineT<T>(this T spline, float curve) where T : ISpline
	{
		if (spline.Count <= 1 || curve < 0f)
		{
			return 0f;
		}
		if (curve >= (float)(spline.Closed ? spline.Count : (spline.Count - 1)))
		{
			return 1f;
		}
		int num = (int)math.floor(curve);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 += spline.GetCurveLength(i);
		}
		num2 += spline.GetCurveLength(num) * math.frac(curve);
		return num2 / spline.GetLength();
	}

	public static float CalculateLength<T>(this T spline, float4x4 transform) where T : ISpline
	{
		using NativeSpline nativeSpline = new NativeSpline(spline, transform);
		return nativeSpline.GetLength();
	}

	public static int GetCurveCount<T>(this T spline) where T : ISpline
	{
		return math.max(0, spline.Count - ((!spline.Closed) ? 1 : 0));
	}

	public static Bounds GetBounds<T>(this T spline) where T : ISpline
	{
		return spline.GetBounds(float4x4.identity);
	}

	public static Bounds GetBounds<T>(this T spline, float4x4 transform) where T : ISpline
	{
		if (spline.Count < 1)
		{
			return default(Bounds);
		}
		BezierKnot bezierKnot = spline[0];
		Bounds result = new Bounds(math.transform(transform, bezierKnot.Position), Vector3.zero);
		if (spline.Closed)
		{
			result.Encapsulate(math.transform(transform, bezierKnot.Position + math.rotate(bezierKnot.Rotation, bezierKnot.TangentIn)));
		}
		result.Encapsulate(math.transform(transform, bezierKnot.Position + math.rotate(bezierKnot.Rotation, bezierKnot.TangentOut)));
		int i = 1;
		for (int count = spline.Count; i < count; i++)
		{
			bezierKnot = spline[i];
			result.Encapsulate(math.transform(transform, bezierKnot.Position));
			result.Encapsulate(math.transform(transform, bezierKnot.Position + math.rotate(bezierKnot.Rotation, bezierKnot.TangentIn)));
			if (spline.Closed || (!spline.Closed && i < count - 1))
			{
				result.Encapsulate(math.transform(transform, bezierKnot.Position + math.rotate(bezierKnot.Rotation, bezierKnot.TangentOut)));
			}
		}
		return result;
	}

	[Obsolete("Use GetSubdivisionCount instead.", false)]
	public static int GetSegmentCount(float length, int resolution)
	{
		return GetSubdivisionCount(length, resolution);
	}

	public static int GetSubdivisionCount(float length, int resolution)
	{
		return (int)math.max(6f, math.min(1024f, math.sqrt(length) * (float)resolution));
	}

	private static Segment GetNearestPoint<T>(T spline, float3 ro, float3 rd, Segment range, out float distance, out float3 nearest, out float time, int segments) where T : ISpline
	{
		distance = float.PositiveInfinity;
		nearest = float.PositiveInfinity;
		time = float.PositiveInfinity;
		Segment result = new Segment(-1f, 0f);
		float num = range.start;
		float3 a = spline.EvaluatePosition(num);
		for (int i = 1; i < segments; i++)
		{
			float num2 = range.start + range.length * ((float)i / ((float)segments - 1f));
			float3 @float = spline.EvaluatePosition(num2);
			float rayParam;
			float lineParam;
			(float3 rayPoint, float3 linePoint) tuple = SplineMath.RayLineNearestPoint(ro, rd, a, @float, out rayParam, out lineParam);
			float3 item = tuple.rayPoint;
			float3 item2 = tuple.linePoint;
			float num3 = math.lengthsq(item2 - item);
			if (num3 < distance)
			{
				result.start = num;
				result.length = num2 - num;
				time = result.start + result.length * lineParam;
				distance = num3;
				nearest = item2;
			}
			num = num2;
			a = @float;
		}
		distance = math.sqrt(distance);
		return result;
	}

	private static Segment GetNearestPoint<T>(T spline, float3 point, Segment range, out float distance, out float3 nearest, out float time, int segments) where T : ISpline
	{
		distance = float.PositiveInfinity;
		nearest = float.PositiveInfinity;
		time = float.PositiveInfinity;
		Segment result = new Segment(-1f, 0f);
		float num = range.start;
		float3 a = spline.EvaluatePosition(num);
		for (int i = 1; i < segments; i++)
		{
			float num2 = range.start + range.length * ((float)i / ((float)segments - 1f));
			float3 @float = spline.EvaluatePosition(num2);
			float lineParam;
			float3 float2 = SplineMath.PointLineNearestPoint(point, a, @float, out lineParam);
			float num3 = math.distancesq(float2, point);
			if (num3 < distance)
			{
				result.start = num;
				result.length = num2 - num;
				time = result.start + result.length * lineParam;
				distance = num3;
				nearest = float2;
			}
			num = num2;
			a = @float;
		}
		distance = math.sqrt(distance);
		return result;
	}

	public static float GetNearestPoint<T>(T spline, Ray ray, out float3 nearest, out float t, int resolution = 4, int iterations = 2) where T : ISpline
	{
		float distance = float.PositiveInfinity;
		nearest = float.PositiveInfinity;
		float3 ro = ray.origin;
		float3 rd = ray.direction;
		Segment range = new Segment(0f, 1f);
		t = 0f;
		int resolution2 = math.min(math.max(2, resolution), 64);
		int i = 0;
		for (int num = math.min(10, iterations); i < num; i++)
		{
			int subdivisionCount = GetSubdivisionCount(spline.GetLength() * range.length, resolution2);
			range = GetNearestPoint(spline, ro, rd, range, out distance, out nearest, out t, subdivisionCount);
		}
		return distance;
	}

	public static float GetNearestPoint<T>(T spline, float3 point, out float3 nearest, out float t, int resolution = 4, int iterations = 2) where T : ISpline
	{
		float distance = float.PositiveInfinity;
		nearest = float.PositiveInfinity;
		Segment range = new Segment(0f, 1f);
		t = 0f;
		int resolution2 = math.min(math.max(2, resolution), 64);
		int i = 0;
		for (int num = math.min(10, iterations); i < num; i++)
		{
			int subdivisionCount = GetSubdivisionCount(spline.GetLength() * range.length, resolution2);
			range = GetNearestPoint(spline, point, range, out distance, out nearest, out t, subdivisionCount);
		}
		return distance;
	}

	public static float3 GetPointAtLinearDistance<T>(this T spline, float fromT, float relativeDistance, out float resultPointT) where T : ISpline
	{
		if (fromT < 0f)
		{
			resultPointT = 0f;
			return spline.EvaluatePosition(0f);
		}
		float length = spline.GetLength();
		float num = fromT * length;
		if (num + relativeDistance >= length)
		{
			resultPointT = 1f;
			return spline.EvaluatePosition(1f);
		}
		if (num + relativeDistance <= 0f)
		{
			resultPointT = 0f;
			return spline.EvaluatePosition(0f);
		}
		float3 @float = spline.EvaluatePosition(fromT);
		resultPointT = fromT;
		bool flag = relativeDistance >= 0f;
		float num2 = math.abs(relativeDistance);
		float num3 = 0f;
		float3 float2 = @float;
		while (num2 > 0.001f && (flag ? (resultPointT < 1f) : (resultPointT > 0f)))
		{
			num += (flag ? num2 : (0f - num2));
			resultPointT = num / length;
			if (resultPointT > 1f)
			{
				resultPointT = 1f;
			}
			else if (resultPointT < 0f)
			{
				resultPointT = 0f;
			}
			float2 = spline.EvaluatePosition(resultPointT);
			num3 = math.distance(@float, float2);
			num2 = math.abs(relativeDistance) - num3;
		}
		return float2;
	}

	public static float ConvertIndexUnit<T>(this T spline, float t, PathIndexUnit targetPathUnit) where T : ISpline
	{
		if (targetPathUnit == PathIndexUnit.Normalized)
		{
			return WrapInterpolation(t, spline.Closed);
		}
		return ConvertNormalizedIndexUnit(spline, t, targetPathUnit);
	}

	public static float ConvertIndexUnit<T>(this T spline, float value, PathIndexUnit fromPathUnit, PathIndexUnit targetPathUnit) where T : ISpline
	{
		if (fromPathUnit == targetPathUnit)
		{
			if (targetPathUnit == PathIndexUnit.Normalized)
			{
				value = WrapInterpolation(value, spline.Closed);
			}
			return value;
		}
		return ConvertNormalizedIndexUnit(spline, GetNormalizedInterpolation(spline, value, fromPathUnit), targetPathUnit);
	}

	private static float ConvertNormalizedIndexUnit<T>(T spline, float t, PathIndexUnit targetPathUnit) where T : ISpline
	{
		float curveT;
		return targetPathUnit switch
		{
			PathIndexUnit.Knot => (float)spline.SplineToCurveT(t, out curveT, useLUT: false) + curveT, 
			PathIndexUnit.Distance => t * spline.GetLength(), 
			_ => t, 
		};
	}

	private static float WrapInterpolation(float t, bool closed)
	{
		if (!closed)
		{
			return math.clamp(t, 0f, 1f);
		}
		if (t % 1f != 0f)
		{
			return t - math.floor(t);
		}
		return math.clamp(t, 0f, 1f);
	}

	public static float GetNormalizedInterpolation<T>(T spline, float t, PathIndexUnit originalPathUnit) where T : ISpline
	{
		switch (originalPathUnit)
		{
		case PathIndexUnit.Knot:
			return WrapInterpolation(spline.CurveToSplineT(t), spline.Closed);
		case PathIndexUnit.Distance:
		{
			float length = spline.GetLength();
			return WrapInterpolation((length > 0f) ? (t / length) : 0f, spline.Closed);
		}
		default:
			return WrapInterpolation(t, spline.Closed);
		}
	}

	public static int PreviousIndex<T>(this T spline, int index) where T : ISpline
	{
		return PreviousIndex(index, spline.Count, spline.Closed);
	}

	public static int NextIndex<T>(this T spline, int index) where T : ISpline
	{
		return NextIndex(index, spline.Count, spline.Closed);
	}

	public static BezierKnot Previous<T>(this T spline, int index) where T : ISpline
	{
		return spline[spline.PreviousIndex(index)];
	}

	public static BezierKnot Next<T>(this T spline, int index) where T : ISpline
	{
		return spline[spline.NextIndex(index)];
	}

	internal static int PreviousIndex(int index, int count, bool wrap)
	{
		if (!wrap)
		{
			return math.max(index - 1, 0);
		}
		return (index + (count - 1)) % count;
	}

	internal static int NextIndex(int index, int count, bool wrap)
	{
		if (!wrap)
		{
			return math.min(index + 1, count - 1);
		}
		return (index + 1) % count;
	}

	internal static float3 GetExplicitLinearTangent(float3 point, float3 to)
	{
		return (to - point) / 3f;
	}

	internal static float3 GetExplicitLinearTangent(BezierKnot from, BezierKnot to)
	{
		float3 @float = to.Position - from.Position;
		return math.mul(math.inverse(from.Rotation), @float * 0.33f);
	}

	public static float3 GetCatmullRomTangent(float3 previous, float3 next)
	{
		return GetAutoSmoothTangent(previous, next, 0.5f);
	}

	public static float3 GetAutoSmoothTangent(float3 previous, float3 next, float tension = 1f / 3f)
	{
		if (next.Equals(previous))
		{
			return 0f;
		}
		return (next - previous) / math.sqrt(math.length(next - previous)) * tension;
	}

	public static float3 GetAutoSmoothTangent(float3 previous, float3 current, float3 next, float tension = 1f / 3f)
	{
		float num = math.length(current - previous);
		float num2 = math.length(next - current);
		if (num == 0f)
		{
			return (next - current) * 0.1f;
		}
		if (num2 == 0f)
		{
			return (current - previous) * 0.1f;
		}
		float y = 2f * tension;
		float num3 = math.pow(num, tension);
		float num4 = math.pow(num, y);
		float num5 = math.pow(num2, tension);
		float num6 = math.pow(num2, y);
		return (num4 * next - num6 * previous + (num6 - num4) * current) / (3f * num3 * (num3 + num5));
	}

	private static float3 GetUniformAutoSmoothTangent(float3 previous, float3 next, float tension)
	{
		return (next - previous) * tension;
	}

	public static BezierKnot GetAutoSmoothKnot(float3 position, float3 previous, float3 next)
	{
		return GetAutoSmoothKnot(position, previous, next, math.up());
	}

	public static BezierKnot GetAutoSmoothKnot(float3 position, float3 previous, float3 next, float3 normal)
	{
		return GetAutoSmoothKnot(position, previous, next, normal, 0.5f);
	}

	public static BezierKnot GetAutoSmoothKnot(float3 position, float3 previous, float3 next, float3 normal, float tension = 1f / 3f)
	{
		float3 autoSmoothTangent = GetAutoSmoothTangent(next, position, previous, tension);
		float3 autoSmoothTangent2 = GetAutoSmoothTangent(previous, position, next, tension);
		float3 @float = new float3(0f, 0f, math.length(autoSmoothTangent));
		float3 tangentOut = new float3(0f, 0f, math.length(autoSmoothTangent2));
		float3 tangent = autoSmoothTangent2;
		if (@float.z == 0f)
		{
			@float.z = tangentOut.z;
		}
		if (tangentOut.z == 0f)
		{
			tangentOut.z = @float.z;
			tangent = -autoSmoothTangent;
		}
		return new BezierKnot(position, -@float, tangentOut, GetKnotRotation(tangent, normal));
	}

	internal static quaternion GetKnotRotation(float3 tangent, float3 normal)
	{
		if (math.lengthsq(tangent) == 0f)
		{
			tangent = math.rotate(Quaternion.FromToRotation(math.up(), normal), math.forward());
		}
		float3 up = (Mathf.Approximately(math.abs(math.dot(math.normalizesafe(tangent), math.normalizesafe(normal))), 1f) ? math.cross(math.normalizesafe(tangent), math.right()) : ((float3)Vector3.ProjectOnPlane(normal, tangent).normalized));
		return quaternion.LookRotationSafe(math.normalizesafe(tangent), up);
	}

	public static void SetPivot(SplineContainer container, Vector3 position)
	{
		Transform transform = container.transform;
		Vector3 vector = position - transform.position;
		transform.position = position;
		Spline spline = container.Spline;
		int i = 0;
		for (int count = spline.Count; i < count; i++)
		{
			spline[i] -= (float3)vector;
		}
	}

	public static bool FitSplineToPoints(List<float3> points, float errorThreshold, bool closed, out Spline spline)
	{
		if (points == null || points.Count < 2)
		{
			Debug.LogError("FitSplineToPoints failed: The provided points list is either null, empty or contains only one point.");
			spline = new Spline();
			return false;
		}
		List<float3> list = new List<float3>(points);
		if (closed)
		{
			if (math.length(list[0] - list[list.Count - 1]) > 0.001f)
			{
				list.Add(list[0]);
			}
			list.Add(list[1]);
			list.Insert(0, list[list.Count - 3]);
		}
		float3 leftTangent = math.normalize(list[1] - list[0]);
		float3 rightTangent = math.normalize(list[list.Count - 2] - list[list.Count - 1]);
		if (FitSplineToPointsStepInternal(list, 0, list.Count - 1, leftTangent, rightTangent, errorThreshold, closed, closed, out spline))
		{
			if (closed && spline.Count > 2)
			{
				spline.RemoveAt(0);
				spline.RemoveAt(spline.Count - 1);
				BezierKnot value = spline[0];
				Spline obj = spline;
				value.TangentIn = obj[obj.Count - 1].TangentOut;
				spline[0] = value;
				spline.RemoveAt(spline.Count - 1);
				spline.Closed = true;
			}
			float3 @float = float3.zero;
			for (int i = 0; i < spline.Count; i++)
			{
				BezierKnot value2 = spline[i];
				float3 x = value2.TangentOut;
				if (math.lengthsq(x) == 0f)
				{
					x = spline.EvaluateTangent(GetNormalizedInterpolation(spline, i, PathIndexUnit.Knot));
				}
				float3 float2 = math.normalizesafe(x);
				@float = ((i != 0) ? CurveUtility.EvaluateUpVector(spline.GetCurve(i - 1), 1f, @float, float3.zero, fixEndUpMismatch: false) : CalculatePreferredNormalForDirection(float2));
				TangentMode tangentMode = TangentMode.Broken;
				if (math.dot(math.normalizesafe(value2.TangentIn), math.normalizesafe(value2.TangentOut)) <= -0.999f)
				{
					tangentMode = ((!(math.abs(math.length(value2.TangentIn) - math.length(value2.TangentOut)) <= 0.001f)) ? TangentMode.Continuous : TangentMode.Mirrored);
				}
				value2.TangentIn = new float3(0f, 0f, 0f - math.length(value2.TangentIn));
				value2.TangentOut = new float3(0f, 0f, math.length(value2.TangentOut));
				value2.Rotation = quaternion.LookRotationSafe(float2, @float);
				spline[i] = value2;
				if (tangentMode != TangentMode.Broken)
				{
					spline.SetTangentModeNoNotify(i, tangentMode);
				}
			}
			return true;
		}
		return false;
	}

	private static bool FitSplineToPointsStepInternal(IReadOnlyList<float3> allPoints, int rangeStart, int rangeEnd, float3 leftTangent, float3 rightTangent, float errorThreshold, bool closed, bool splineClosed, out Spline spline)
	{
		spline = new Spline();
		int num = rangeEnd - rangeStart + 1;
		if (num < 2)
		{
			return false;
		}
		if (num == 2)
		{
			float num2 = math.length(allPoints[rangeStart] - allPoints[rangeEnd]) / 3f;
			float3 @float = leftTangent * num2;
			float3 float2 = rightTangent * num2;
			BezierKnot item = new BezierKnot(allPoints[rangeStart], -@float, @float, Quaternion.identity);
			BezierKnot item2 = new BezierKnot(allPoints[rangeStart + 1], float2, -float2, Quaternion.identity);
			spline = new Spline();
			spline.Add(item, TangentMode.Broken);
			spline.Add(item2, TangentMode.Broken);
			spline.Closed = closed;
			return true;
		}
		float[] array = new float[num];
		float[] array2 = new float[num];
		array2[0] = 0f;
		float[] array3 = new float[num - 1];
		float num3 = 0f;
		for (int i = 1; i < num; i++)
		{
			float3 x = allPoints[rangeStart + i] - allPoints[rangeStart + i - 1];
			num3 = (array3[i - 1] = num3 + math.length(x));
		}
		for (int j = 0; j < array3.Length; j++)
		{
			array2[j + 1] = array3[j] / num3;
		}
		array = array2;
		spline = GenerateSplineFromTValues(allPoints, rangeStart, rangeEnd, closed, array, leftTangent, rightTangent);
		float3[] array4 = new float3[num];
		for (int k = 0; k < array.Length; k++)
		{
			float t = array[k];
			float3 float3 = spline.EvaluatePosition(t);
			array4[k] = float3;
		}
		var (num4, num5) = ComputeMaxError(allPoints, rangeStart, rangeEnd, array4, errorThreshold, splineClosed);
		if (num4 < errorThreshold)
		{
			return true;
		}
		if (num4 < 4f * errorThreshold)
		{
			for (int l = 0; l < 4; l++)
			{
				BezierKnot[] array5 = new BezierKnot[spline.Count];
				int num6 = 0;
				foreach (BezierKnot knot in spline.Knots)
				{
					array5[num6] = knot;
					num6++;
				}
				float3 position = array5[0].Position;
				float3 float4 = array5[0].Position + array5[0].TangentOut;
				float3 float5 = array5[1].Position + array5[1].TangentIn;
				float3 position2 = array5[1].Position;
				float3[] array6 = new float3[4] { position, float4, float5, position2 };
				float3[] array7 = new float3[3];
				for (int m = 0; m <= 2; m++)
				{
					array7[m] = (array6[m + 1] - array6[m]) * 3f;
				}
				float3[] array8 = new float3[2];
				for (int n = 0; n <= 1; n++)
				{
					array8[n] = (array7[n + 1] - array7[n]) * 2f;
				}
				for (int num7 = rangeStart; num7 < num - 1; num7++)
				{
					float t2 = array[num7];
					float3 float6 = Bernstein(t2, array6, 3);
					float3 float7 = Bernstein(t2, array7, 2);
					float3 y = Bernstein(t2, array8, 1);
					float num8 = math.dot(float6 - array4[num7], float7);
					float num9 = math.dot(float7, float7) + math.dot(float6 - array4[num7], y);
					if (num9 != 0f)
					{
						array[num7] -= num8 / num9;
					}
				}
				spline = GenerateSplineFromTValues(allPoints, rangeStart, rangeEnd, closed, array, leftTangent, rightTangent);
				for (int num10 = 0; num10 < array4.Length; num10++)
				{
					float t3 = array[num10];
					float3 float8 = spline.EvaluatePosition(t3);
					array4[num10] = float8;
				}
				float num11;
				(num11, num5) = ComputeMaxError(allPoints, rangeStart, rangeEnd, array4, errorThreshold, splineClosed);
				if (num11 < errorThreshold)
				{
					return true;
				}
			}
		}
		if (num == 3)
		{
			num5 = rangeStart + 1;
		}
		else if (num5 == 0)
		{
			num5++;
		}
		else if (num5 == num - 1)
		{
			num5--;
		}
		float3 float9 = CalculateCenterTangent(allPoints[num5 - 1], allPoints[num5], allPoints[num5 + 1]);
		if (!FitSplineToPointsStepInternal(allPoints, rangeStart, num5, leftTangent, float9, errorThreshold, closed: false, splineClosed, out var spline2) || !FitSplineToPointsStepInternal(allPoints, num5, rangeEnd, -float9, rightTangent, errorThreshold, closed: false, splineClosed, out var spline3))
		{
			spline = new Spline();
			return false;
		}
		int index = spline2.Count - 1;
		Spline spline4 = spline2;
		float3 tangentIn = spline4[spline4.Count - 1].TangentIn;
		spline2.RemoveAt(index);
		spline2.Add(spline3);
		spline = spline2;
		BezierKnot value = spline[index];
		value.TangentIn = tangentIn;
		spline[index] = value;
		return true;
	}

	private static float3 CalculatePreferredNormalForDirection(float3 splineDirection)
	{
		float3 @float = math.normalizesafe(splineDirection);
		float3 float2 = default(float3);
		float2.x = math.abs(math.dot(@float, math.right()));
		float2.y = math.abs(math.dot(@float, math.up()));
		float2.z = math.abs(math.dot(@float, math.forward()));
		float3 y = ((float2.x < float2.y && float2.x < float2.z) ? math.normalizesafe(math.cross(math.right(), @float)) : ((!(float2.y < float2.x) || !(float2.y < float2.z)) ? math.normalizesafe(math.cross(math.forward(), @float)) : math.normalizesafe(math.cross(math.up(), @float))));
		return math.normalizesafe(math.cross(@float, y));
	}

	private static float3 CalculateCenterTangent(float3 prevPoint, float3 centerPoint, float3 nextPoint)
	{
		float3 @float = prevPoint - centerPoint;
		float3 float2 = centerPoint - nextPoint;
		return math.normalize((@float + float2) / 2f);
	}

	private static float3 Bernstein(float t, float3[] bezier, int degree)
	{
		float3[] array = new float3[bezier.Length];
		bezier.CopyTo(array, 0);
		for (int i = 1; i <= degree; i++)
		{
			for (int j = 0; j <= degree - i; j++)
			{
				array[j] = (1f - t) * array[j] + t * array[j + 1];
			}
		}
		return array[0];
	}

	private static Spline GenerateSplineFromTValues(IReadOnlyList<float3> allPoints, int rangeStart, int rangeEnd, bool closed, float[] tValues, float3 leftTangent, float3 rightTangent)
	{
		int num = rangeEnd - rangeStart + 1;
		float3[] array = new float3[num];
		float3[] array2 = new float3[num];
		for (int i = 0; i < num; i++)
		{
			float num2 = tValues[i];
			array[i] = leftTangent * 3f * num2 * (1f - num2) * (1f - num2);
			array2[i] = rightTangent * 3f * num2 * num2 * (1f - num2);
		}
		float[,] array3 = new float[2, 2];
		float num3 = 0f;
		float num4 = 0f;
		for (int j = 0; j < num; j++)
		{
			float num5 = tValues[j];
			array3[0, 0] += math.dot(array[j], array[j]);
			array3[0, 1] += math.dot(array[j], array2[j]);
			array3[1, 0] = array3[0, 1];
			array3[1, 1] += math.dot(array2[j], array2[j]);
			float3 y = allPoints[rangeStart + j] - (allPoints[rangeStart] * (1f - num5) * (1f - num5) * (1f - num5) + allPoints[rangeStart] * 3f * num5 * (1f - num5) * (1f - num5) + allPoints[rangeEnd] * 3f * num5 * num5 * (1f - num5) + allPoints[rangeEnd] * num5 * num5 * num5);
			num3 += math.dot(array[j], y);
			num4 += math.dot(array2[j], y);
		}
		float num6 = array3[0, 0] * array3[1, 1] - array3[1, 0] * array3[0, 1];
		float num7 = array3[0, 0] * num4 - array3[1, 0] * num3;
		float num8 = num3 * array3[1, 1] - num4 * array3[0, 1];
		float num9 = ((num6 == 0f) ? 0f : (num8 / num6));
		float num10 = ((num6 == 0f) ? 0f : (num7 / num6));
		float num11 = math.length(allPoints[rangeEnd] - allPoints[rangeStart]);
		float num12 = 1E-06f * num11;
		if (num9 < num12 || num10 < num12)
		{
			num9 = (num10 = num11 / 3f);
		}
		BezierKnot item = new BezierKnot(allPoints[rangeStart], leftTangent * (0f - num9), leftTangent * num9, quaternion.identity);
		BezierKnot item2 = new BezierKnot(allPoints[rangeEnd], rightTangent * num10, rightTangent * (0f - num10), quaternion.identity);
		Spline spline = new Spline();
		spline.Add(item, TangentMode.Broken);
		spline.Add(item2, TangentMode.Broken);
		spline.Closed = closed;
		return spline;
	}

	private static (float maxError, int maxErrorIndex) ComputeMaxError(IReadOnlyList<float3> allPoints, int rangeStart, int rangeEnd, float3[] positions, float errorThreshold, bool splineClosed)
	{
		int num = rangeEnd - rangeStart + 1;
		if (splineClosed && num > 2)
		{
			int num2 = 1;
			if (rangeStart < num2 && rangeEnd > num2)
			{
				return (maxError: errorThreshold, maxErrorIndex: num2);
			}
			int num3 = allPoints.Count - 2;
			if (rangeStart < num3 && rangeEnd > num3)
			{
				return (maxError: errorThreshold, maxErrorIndex: num3);
			}
		}
		float num4 = 0f;
		int item = 0;
		for (int i = 0; i < num; i++)
		{
			int num5 = rangeStart + i;
			float num6 = math.length(allPoints[num5] - positions[i]);
			if (num6 > num4)
			{
				num4 = num6;
				item = num5;
			}
		}
		return (maxError: num4, maxErrorIndex: item);
	}

	public static Spline AddSpline<T>(this T container) where T : ISplineContainer
	{
		Spline spline = new Spline();
		container.AddSpline(spline);
		return spline;
	}

	public static void AddSpline<T>(this T container, Spline spline) where T : ISplineContainer
	{
		List<Spline> list = new List<Spline>(container.Splines);
		list.Add(spline);
		container.Splines = list;
	}

	public static bool RemoveSplineAt<T>(this T container, int splineIndex) where T : ISplineContainer
	{
		if (splineIndex < 0 || splineIndex >= container.Splines.Count)
		{
			return false;
		}
		List<Spline> list = new List<Spline>(container.Splines);
		list.RemoveAt(splineIndex);
		container.KnotLinkCollection.SplineRemoved(splineIndex);
		container.Splines = list;
		return true;
	}

	public static bool RemoveSpline<T>(this T container, Spline spline) where T : ISplineContainer
	{
		List<Spline> list = new List<Spline>(container.Splines);
		int num = list.IndexOf(spline);
		if (num < 0)
		{
			return false;
		}
		list.RemoveAt(num);
		container.KnotLinkCollection.SplineRemoved(num);
		container.Splines = list;
		return true;
	}

	public static bool ReorderSpline<T>(this T container, int previousSplineIndex, int newSplineIndex) where T : ISplineContainer
	{
		if (previousSplineIndex < 0 || previousSplineIndex >= container.Splines.Count || newSplineIndex < 0 || newSplineIndex >= container.Splines.Count)
		{
			return false;
		}
		List<Spline> list = new List<Spline>(container.Splines);
		Spline item = list[previousSplineIndex];
		list.RemoveAt(previousSplineIndex);
		list.Insert(newSplineIndex, item);
		container.KnotLinkCollection.SplineIndexChanged(previousSplineIndex, newSplineIndex);
		container.Splines = list;
		return true;
	}

	internal static bool IsIndexValid<T>(T container, SplineKnotIndex index) where T : ISplineContainer
	{
		if (index.Knot >= 0 && index.Knot < container.Splines[index.Spline].Count && index.Spline < container.Splines.Count)
		{
			return index.Knot < container.Splines[index.Spline].Count;
		}
		return false;
	}

	public static void SetLinkedKnotPosition<T>(this T container, SplineKnotIndex index) where T : ISplineContainer
	{
		if (!container.KnotLinkCollection.TryGetKnotLinks(index, out var linkedKnots))
		{
			return;
		}
		IReadOnlyList<Spline> splines = container.Splines;
		float3 position = splines[index.Spline][index.Knot].Position;
		foreach (SplineKnotIndex item in linkedKnots)
		{
			if (!IsIndexValid(container, item))
			{
				break;
			}
			BezierKnot value = splines[item.Spline][item.Knot];
			float3 position2 = value.Position;
			value.Position = position;
			splines[item.Spline].SetKnotNoNotify(item.Knot, value);
			if (!Mathf.Approximately(Vector3.Distance(position, position2), 0f))
			{
				splines[item.Spline].SetDirty(SplineModification.KnotModified, item.Knot);
			}
		}
	}

	public static void LinkKnots<T>(this T container, SplineKnotIndex knotA, SplineKnotIndex knotB) where T : ISplineContainer
	{
		IReadOnlyList<SplineKnotIndex> readOnlyList = (Mathf.Approximately(math.length(container.Splines[knotA.Spline][knotA.Knot].Position - container.Splines[knotB.Spline][knotB.Knot].Position), 0f) ? null : container.KnotLinkCollection.GetKnotLinks(knotB));
		container.KnotLinkCollection.Link(knotA, knotB);
		if (readOnlyList == null)
		{
			return;
		}
		foreach (SplineKnotIndex item in readOnlyList)
		{
			container.Splines[item.Spline].SetDirty(SplineModification.KnotModified, item.Knot);
		}
	}

	public static void UnlinkKnots<T>(this T container, IReadOnlyList<SplineKnotIndex> knots) where T : ISplineContainer
	{
		foreach (SplineKnotIndex knot in knots)
		{
			container.KnotLinkCollection.Unlink(knot);
		}
	}

	public static bool AreKnotLinked(this ISplineContainer container, SplineKnotIndex knotA, SplineKnotIndex knotB)
	{
		if (!container.KnotLinkCollection.TryGetKnotLinks(knotA, out var linkedKnots))
		{
			return false;
		}
		for (int i = 0; i < linkedKnots.Count; i++)
		{
			if (linkedKnots[i] == knotB)
			{
				return true;
			}
		}
		return false;
	}

	public static void CopyKnotLinks<T>(this T container, int srcSplineIndex, int destSplineIndex) where T : ISplineContainer
	{
		if (srcSplineIndex < 0 || srcSplineIndex >= container.Splines.Count || destSplineIndex < 0 || destSplineIndex >= container.Splines.Count)
		{
			return;
		}
		Spline spline = container.Splines[srcSplineIndex];
		Spline spline2 = container.Splines[destSplineIndex];
		if (spline.Count == 0 || spline.Count != spline2.Count)
		{
			return;
		}
		int i = 0;
		for (int count = spline.Count; i < count; i++)
		{
			if (container.KnotLinkCollection.TryGetKnotLinks(new SplineKnotIndex(srcSplineIndex, i), out var _))
			{
				container.KnotLinkCollection.Link(new SplineKnotIndex(srcSplineIndex, i), new SplineKnotIndex(destSplineIndex, i));
			}
		}
	}

	public static List<float3> ReducePoints<T>(T line, float epsilon = 0.15f) where T : IList<float3>
	{
		List<float3> list = new List<float3>();
		new RamerDouglasPeucker<T>(line).Reduce(list, epsilon);
		return list;
	}

	public static void ReducePoints<T>(T line, List<float3> results, float epsilon = 0.15f) where T : IList<float3>
	{
		new RamerDouglasPeucker<T>(line).Reduce(results, epsilon);
	}

	internal static bool AreTangentsModifiable(TangentMode mode)
	{
		if (mode != TangentMode.Broken && mode != TangentMode.Continuous)
		{
			return mode == TangentMode.Mirrored;
		}
		return true;
	}

	public static void ReverseFlow(this ISplineContainer container, int splineIndex)
	{
		ReverseFlow(new SplineInfo(container, splineIndex));
	}

	public static void ReverseFlow(SplineInfo splineInfo)
	{
		Spline spline = splineInfo.Spline;
		BezierKnot[] array = splineInfo.Spline.ToArray();
		TangentMode[] array2 = new TangentMode[spline.Count];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = spline.GetTangentMode(i);
		}
		List<List<SplineKnotIndex>> list = new List<List<SplineKnotIndex>>();
		for (int j = 0; j < spline.Count; j++)
		{
			SplineKnotIndex knotIndex = new SplineKnotIndex(splineInfo.Index, j);
			List<SplineKnotIndex> item = splineInfo.Container.KnotLinkCollection.GetKnotLinks(knotIndex).ToList();
			list.Add(item);
		}
		foreach (List<SplineKnotIndex> item2 in list)
		{
			splineInfo.Container.UnlinkKnots(item2);
		}
		for (int k = 0; k < spline.Count; k++)
		{
			BezierKnot value = array[k];
			BezierKnot bezierKnot = value.Transform(splineInfo.LocalToWorld);
			float3 tangentIn = bezierKnot.TangentIn;
			float3 tangentOut = bezierKnot.TangentOut;
			quaternion q = quaternion.AxisAngle(math.mul(value.Rotation, math.up()), math.radians(180f));
			q = math.normalizesafe(q);
			value.Rotation = math.mul(q, value.Rotation);
			if (array2[k] == TangentMode.Broken)
			{
				quaternion q2 = quaternion.AxisAngle(math.up(), math.radians(180f));
				value.TangentIn = math.rotate(q2, tangentOut);
				value.TangentOut = math.rotate(q2, tangentIn);
			}
			else if (array2[k] == TangentMode.Continuous)
			{
				value.TangentIn = -tangentOut;
				value.TangentOut = -tangentIn;
			}
			int index = spline.Count - 1 - k;
			spline.SetTangentMode(index, array2[k]);
			spline[index] = value;
		}
		foreach (List<SplineKnotIndex> item3 in list)
		{
			if (item3.Count == 1)
			{
				continue;
			}
			SplineKnotIndex knotA = item3[0];
			knotA = new SplineKnotIndex(knotA.Spline, knotA.Spline.Equals(splineInfo.Index) ? (spline.Count - 1 - knotA.Knot) : knotA.Knot);
			for (int l = 1; l < item3.Count; l++)
			{
				SplineKnotIndex splineKnotIndex = item3[l];
				if (splineKnotIndex.Spline.Equals(splineInfo.Index))
				{
					item3[l] = new SplineKnotIndex(splineInfo.Index, spline.Count - 1 - splineKnotIndex.Knot);
				}
				splineInfo.Container.LinkKnots(knotA, item3[l]);
			}
		}
	}

	public static void ReverseFlow(Spline spline)
	{
		BezierKnot[] array = spline.ToArray();
		TangentMode[] array2 = new TangentMode[spline.Count];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = spline.GetTangentMode(i);
		}
		for (int j = 0; j < spline.Count; j++)
		{
			BezierKnot value = array[j];
			float3 tangentIn = value.TangentIn;
			float3 tangentOut = value.TangentOut;
			quaternion q = quaternion.AxisAngle(math.mul(value.Rotation, math.up()), math.radians(180f));
			q = math.normalizesafe(q);
			value.Rotation = math.mul(q, value.Rotation);
			if (array2[j] == TangentMode.Broken)
			{
				quaternion q2 = quaternion.AxisAngle(math.up(), math.radians(180f));
				value.TangentIn = math.rotate(q2, tangentOut);
				value.TangentOut = math.rotate(q2, tangentIn);
			}
			else if (array2[j] == TangentMode.Continuous)
			{
				value.TangentIn = -tangentOut;
				value.TangentOut = -tangentIn;
			}
			int index = spline.Count - 1 - j;
			spline.SetTangentMode(index, array2[j]);
			spline[index] = value;
		}
	}

	public static SplineKnotIndex JoinSplinesOnKnots(this ISplineContainer container, SplineKnotIndex mainKnot, SplineKnotIndex otherKnot)
	{
		if (mainKnot.Spline == otherKnot.Spline)
		{
			Debug.LogError("Trying to join Knots already belonging to the same spline.", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		if (mainKnot.Spline < 0 || mainKnot.Spline > container.Splines.Count)
		{
			Debug.LogError($"Spline index {mainKnot.Spline} does not exist for the current container.", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		if (otherKnot.Spline < 0 || otherKnot.Spline > container.Splines.Count)
		{
			Debug.LogError($"Spline index {otherKnot.Spline} does not exist for the current container.", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		if (mainKnot.Knot < 0 || mainKnot.Knot > container.Splines[mainKnot.Spline].Count)
		{
			Debug.LogError($"Knot index {mainKnot.Knot} does not exist for the current container for Spline[{mainKnot.Spline}].", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		if (otherKnot.Knot < 0 || otherKnot.Knot > container.Splines[otherKnot.Spline].Count)
		{
			Debug.LogError($"Knot index {otherKnot.Knot} does not exist for the current container for Spline[{otherKnot.Spline}].", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		if (mainKnot.Knot != 0 && mainKnot.Knot != container.Splines[mainKnot.Spline].Count - 1)
		{
			Debug.LogError($"Knot index {mainKnot.Knot} is not an extremity knot for the current container for Spline[{mainKnot.Spline}]." + "Only extremity knots can be joined.", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		if (otherKnot.Knot != 0 && otherKnot.Knot != container.Splines[otherKnot.Spline].Count - 1)
		{
			Debug.LogError($"Knot index {otherKnot.Knot} is not an extremity knot for the current container for Spline[{otherKnot.Spline}]." + "Only extremity knots can be joined.", container as SplineContainer);
			return default(SplineKnotIndex);
		}
		bool flag = mainKnot.Knot == 0;
		bool flag2 = otherKnot.Knot == 0;
		if (flag == flag2)
		{
			container.ReverseFlow(otherKnot.Spline);
		}
		List<List<SplineKnotIndex>> list = new List<List<SplineKnotIndex>>();
		int spline = mainKnot.Spline;
		Spline spline2 = container.Splines[spline];
		int count = spline2.Count;
		int spline3 = otherKnot.Spline;
		Spline spline4 = container.Splines[spline3];
		int count2 = spline4.Count;
		for (int i = 0; i < count; i++)
		{
			SplineKnotIndex knotIndex = new SplineKnotIndex(mainKnot.Spline, i);
			list.Add(container.KnotLinkCollection.GetKnotLinks(knotIndex).ToList());
		}
		for (int j = 0; j < count2; j++)
		{
			SplineKnotIndex knotIndex2 = new SplineKnotIndex(otherKnot.Spline, j);
			list.Add(container.KnotLinkCollection.GetKnotLinks(knotIndex2).ToList());
		}
		foreach (List<SplineKnotIndex> item in list)
		{
			container.UnlinkKnots(item);
		}
		if (count2 > 1)
		{
			if (flag)
			{
				for (int num = count2 - 2; num >= 0; num--)
				{
					spline2.Insert(0, spline4[num], spline4.GetTangentMode(num));
				}
			}
			else
			{
				for (int k = 1; k < count2; k++)
				{
					spline2.Add(spline4[k], spline4.GetTangentMode(k));
				}
			}
		}
		container.RemoveSplineAt(spline3);
		int spline5 = ((spline3 > spline) ? spline : (mainKnot.Spline - 1));
		foreach (List<SplineKnotIndex> item2 in list)
		{
			if (item2.Count == 1)
			{
				continue;
			}
			for (int l = 0; l < item2.Count; l++)
			{
				SplineKnotIndex splineKnotIndex = item2[l];
				if (splineKnotIndex.Spline == spline || splineKnotIndex.Spline == spline3)
				{
					int num2 = splineKnotIndex.Knot;
					if (splineKnotIndex.Spline == spline && flag)
					{
						num2 += count2 - 1;
					}
					if (splineKnotIndex.Spline == spline3 && !flag)
					{
						num2 += count - 1;
					}
					item2[l] = new SplineKnotIndex(spline5, num2);
				}
				else if (splineKnotIndex.Spline > spline3)
				{
					item2[l] = new SplineKnotIndex(splineKnotIndex.Spline - 1, splineKnotIndex.Knot);
				}
			}
			SplineKnotIndex knotA = item2[0];
			for (int m = 1; m < item2.Count; m++)
			{
				container.LinkKnots(knotA, item2[m]);
			}
		}
		return new SplineKnotIndex(spline5, flag ? (count2 - 1) : mainKnot.Knot);
	}

	internal static SplineKnotIndex DuplicateKnot(this ISplineContainer container, SplineKnotIndex originalKnot, int targetIndex)
	{
		Spline spline = container.Splines[originalKnot.Spline];
		BezierKnot knot = spline[originalKnot.Knot];
		spline.Insert(targetIndex, knot);
		spline.SetTangentMode(targetIndex, spline.GetTangentMode(originalKnot.Knot));
		return new SplineKnotIndex(originalKnot.Spline, targetIndex);
	}

	public static void DuplicateSpline(this ISplineContainer container, SplineKnotIndex fromKnot, SplineKnotIndex toKnot, out int newSplineIndex)
	{
		newSplineIndex = -1;
		if (!fromKnot.IsValid() || !toKnot.IsValid())
		{
			throw new ArgumentException("Duplicate failed: The 2 provided knots must be valid knots.");
		}
		if (fromKnot.Spline != toKnot.Spline)
		{
			throw new ArgumentException("Duplicate failed: The 2 provided knots must be on the same Spline.");
		}
		Spline spline = container.AddSpline();
		int num = Math.Min(fromKnot.Knot, toKnot.Knot);
		int num2 = Math.Max(fromKnot.Knot, toKnot.Knot);
		int spline2 = fromKnot.Spline;
		Spline spline3 = container.Splines[spline2];
		newSplineIndex = container.Splines.Count - 1;
		for (int i = num; i <= num2; i++)
		{
			spline.Add(spline3[i], spline3.GetTangentMode(i));
			if (container.KnotLinkCollection.TryGetKnotLinks(new SplineKnotIndex(spline2, i), out var _))
			{
				container.KnotLinkCollection.Link(new SplineKnotIndex(spline2, i), new SplineKnotIndex(newSplineIndex, i - num));
			}
		}
	}

	public static SplineKnotIndex SplitSplineOnKnot(this ISplineContainer container, SplineKnotIndex knotInfo)
	{
		if (knotInfo.Spline < 0 || knotInfo.Spline > container.Splines.Count)
		{
			throw new IndexOutOfRangeException($"Spline index {knotInfo.Spline} does not exist for the current container.");
		}
		if (knotInfo.Knot < 0 || knotInfo.Knot > container.Splines[knotInfo.Spline].Count)
		{
			throw new IndexOutOfRangeException($"Knot index {knotInfo.Knot} does not exist for the current container for Spline[{knotInfo.Spline}].");
		}
		Spline spline = container.Splines[knotInfo.Spline];
		if (spline.Closed)
		{
			spline.Closed = false;
			SplineKnotIndex splineKnotIndex = new SplineKnotIndex(knotInfo.Spline, 0);
			SplineKnotIndex knotB = container.DuplicateKnot(splineKnotIndex, spline.Count);
			if (knotInfo.Knot == 0)
			{
				return splineKnotIndex;
			}
			container.LinkKnots(splineKnotIndex, knotB);
		}
		else if (knotInfo.Knot == 0 || knotInfo.Knot == spline.Count - 1)
		{
			return knotInfo;
		}
		container.DuplicateSpline(knotInfo, new SplineKnotIndex(knotInfo.Spline, spline.Count - 1), out var newSplineIndex);
		spline.Resize(knotInfo.Knot + 1);
		return new SplineKnotIndex(newSplineIndex, 0);
	}
}
