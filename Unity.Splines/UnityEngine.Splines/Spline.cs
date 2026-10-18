using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[Serializable]
public class Spline : ISpline, IReadOnlyList<BezierKnot>, IEnumerable<BezierKnot>, IEnumerable, IReadOnlyCollection<BezierKnot>, IList<BezierKnot>, ICollection<BezierKnot>
{
	[Serializable]
	private sealed class MetaData
	{
		public TangentMode Mode;

		public float Tension;

		private DistanceToInterpolation[] m_DistanceToInterpolation = new DistanceToInterpolation[30];

		private float3[] m_UpVectors = new float3[30];

		public DistanceToInterpolation[] DistanceToInterpolation
		{
			get
			{
				if (m_DistanceToInterpolation == null || m_DistanceToInterpolation.Length != 30)
				{
					m_DistanceToInterpolation = new DistanceToInterpolation[30];
					InvalidateCache();
				}
				return m_DistanceToInterpolation;
			}
		}

		public float3[] UpVectors
		{
			get
			{
				if (m_UpVectors == null || m_UpVectors.Length != 30)
				{
					m_UpVectors = new float3[30];
					InvalidateCache();
				}
				return m_UpVectors;
			}
		}

		public MetaData()
		{
			Mode = TangentMode.Broken;
			Tension = 0.5f;
			InvalidateCache();
		}

		public MetaData(MetaData toCopy)
		{
			Mode = toCopy.Mode;
			Tension = toCopy.Tension;
			Array.Copy(toCopy.DistanceToInterpolation, DistanceToInterpolation, DistanceToInterpolation.Length);
			Array.Copy(toCopy.UpVectors, UpVectors, UpVectors.Length);
		}

		public void InvalidateCache()
		{
			DistanceToInterpolation[0] = UnityEngine.Splines.DistanceToInterpolation.Invalid;
			UpVectors[0] = Vector3.zero;
		}
	}

	private const TangentMode k_DefaultTangentMode = TangentMode.Broken;

	private const BezierTangent k_DefaultMainTangent = BezierTangent.Out;

	private const int k_BatchModification = -1;

	private const int k_CurveDistanceLutResolution = 30;

	[SerializeField]
	[Obsolete]
	[HideInInspector]
	private SplineType m_EditModeType = SplineType.Bezier;

	[SerializeField]
	private List<BezierKnot> m_Knots = new List<BezierKnot>();

	private float m_Length = -1f;

	[SerializeField]
	[HideInInspector]
	private List<MetaData> m_MetaData = new List<MetaData>();

	[SerializeField]
	private bool m_Closed;

	[SerializeField]
	private SplineDataDictionary<int> m_IntData = new SplineDataDictionary<int>();

	[SerializeField]
	private SplineDataDictionary<float> m_FloatData = new SplineDataDictionary<float>();

	[SerializeField]
	private SplineDataDictionary<float4> m_Float4Data = new SplineDataDictionary<float4>();

	[SerializeField]
	private SplineDataDictionary<Object> m_ObjectData = new SplineDataDictionary<Object>();

	private (float curve0, float curve1) m_LastKnotChangeCurveLengths;

	private IEnumerable<ISplineModificationHandler> embeddedSplineData
	{
		get
		{
			foreach (SplineDataKeyValuePair<int> intDatum in m_IntData)
			{
				yield return intDatum.Value;
			}
			foreach (SplineDataKeyValuePair<float> floatDatum in m_FloatData)
			{
				yield return floatDatum.Value;
			}
			foreach (SplineDataKeyValuePair<float4> float4Datum in m_Float4Data)
			{
				yield return float4Datum.Value;
			}
			foreach (SplineDataKeyValuePair<Object> objectDatum in m_ObjectData)
			{
				yield return objectDatum.Value;
			}
		}
	}

	public int Count => m_Knots.Count;

	public bool IsReadOnly => false;

	[Obsolete("Use GetTangentMode and SetTangentMode.")]
	public SplineType EditType
	{
		get
		{
			return m_EditModeType;
		}
		set
		{
			if (m_EditModeType != value)
			{
				m_EditModeType = value;
				TangentMode tangentMode = value.GetTangentMode();
				for (int i = 0; i < Count; i++)
				{
					SetTangentModeNoNotify(i, tangentMode);
				}
				SetDirty(SplineModification.Default);
			}
		}
	}

	public IEnumerable<BezierKnot> Knots
	{
		get
		{
			return m_Knots;
		}
		set
		{
			m_Knots = new List<BezierKnot>(value);
			m_MetaData = new List<MetaData>(m_Knots.Count);
			SetDirty(SplineModification.Default);
		}
	}

	public bool Closed
	{
		get
		{
			return m_Closed;
		}
		set
		{
			if (m_Closed != value)
			{
				m_Closed = value;
				CheckAutoSmoothExtremityKnots();
				SetDirty(SplineModification.ClosedModified);
			}
		}
	}

	public BezierKnot this[int index]
	{
		get
		{
			return m_Knots[index];
		}
		set
		{
			SetKnot(index, value);
		}
	}

	[Obsolete("Deprecated, use Changed instead.")]
	public event Action changed;

	public static event Action<Spline, int, SplineModification> Changed;

	public bool TryGetFloatData(string key, out SplineData<float> data)
	{
		return m_FloatData.TryGetValue(key, out data);
	}

	public bool TryGetFloat4Data(string key, out SplineData<float4> data)
	{
		return m_Float4Data.TryGetValue(key, out data);
	}

	public bool TryGetIntData(string key, out SplineData<int> data)
	{
		return m_IntData.TryGetValue(key, out data);
	}

	public bool TryGetObjectData(string key, out SplineData<Object> data)
	{
		return m_ObjectData.TryGetValue(key, out data);
	}

	public SplineData<float> GetOrCreateFloatData(string key)
	{
		return m_FloatData.GetOrCreate(key);
	}

	public SplineData<float4> GetOrCreateFloat4Data(string key)
	{
		return m_Float4Data.GetOrCreate(key);
	}

	public SplineData<int> GetOrCreateIntData(string key)
	{
		return m_IntData.GetOrCreate(key);
	}

	public SplineData<Object> GetOrCreateObjectData(string key)
	{
		return m_ObjectData.GetOrCreate(key);
	}

	public bool RemoveFloatData(string key)
	{
		return m_FloatData.Remove(key);
	}

	public bool RemoveFloat4Data(string key)
	{
		return m_Float4Data.Remove(key);
	}

	public bool RemoveIntData(string key)
	{
		return m_IntData.Remove(key);
	}

	public bool RemoveObjectData(string key)
	{
		return m_ObjectData.Remove(key);
	}

	public IEnumerable<string> GetFloatDataKeys()
	{
		return m_FloatData.Keys;
	}

	public IEnumerable<string> GetFloat4DataKeys()
	{
		return m_Float4Data.Keys;
	}

	public IEnumerable<string> GetIntDataKeys()
	{
		return m_IntData.Keys;
	}

	public IEnumerable<string> GetObjectDataKeys()
	{
		return m_ObjectData.Keys;
	}

	public IEnumerable<string> GetSplineDataKeys(EmbeddedSplineDataType type)
	{
		return type switch
		{
			EmbeddedSplineDataType.Float => m_FloatData.Keys, 
			EmbeddedSplineDataType.Float4 => m_Float4Data.Keys, 
			EmbeddedSplineDataType.Int => m_IntData.Keys, 
			EmbeddedSplineDataType.Object => m_ObjectData.Keys, 
			_ => throw new InvalidEnumArgumentException(), 
		};
	}

	public IEnumerable<SplineData<float>> GetFloatDataValues()
	{
		return m_FloatData.Values;
	}

	public IEnumerable<SplineData<float4>> GetFloat4DataValues()
	{
		return m_Float4Data.Values;
	}

	public IEnumerable<SplineData<int>> GetIntDataValues()
	{
		return m_IntData.Values;
	}

	public IEnumerable<SplineData<Object>> GetObjectDataValues()
	{
		return m_ObjectData.Values;
	}

	public void SetFloatData(string key, SplineData<float> value)
	{
		m_FloatData[key] = value;
	}

	public void SetFloat4Data(string key, SplineData<float4> value)
	{
		m_Float4Data[key] = value;
	}

	public void SetIntData(string key, SplineData<int> value)
	{
		m_IntData[key] = value;
	}

	public void SetObjectData(string key, SplineData<Object> value)
	{
		m_ObjectData[key] = value;
	}

	internal void SetDirtyNoNotify()
	{
		EnsureMetaDataValid();
		m_Length = -1f;
		int i = 0;
		for (int count = m_MetaData.Count; i < count; i++)
		{
			m_MetaData[i].InvalidateCache();
		}
	}

	internal void SetDirty(SplineModification modificationEvent, int knotIndex = -1)
	{
		SetDirtyNoNotify();
		this.changed?.Invoke();
		OnSplineChanged();
		foreach (ISplineModificationHandler embeddedSplineDatum in embeddedSplineData)
		{
			embeddedSplineDatum.OnSplineModified(new SplineModificationData(this, modificationEvent, knotIndex, m_LastKnotChangeCurveLengths.curve0, m_LastKnotChangeCurveLengths.curve1));
		}
		Spline.Changed?.Invoke(this, knotIndex, modificationEvent);
	}

	protected virtual void OnSplineChanged()
	{
	}

	private void EnsureMetaDataValid()
	{
		while (m_MetaData.Count < m_Knots.Count)
		{
			m_MetaData.Add(new MetaData());
		}
	}

	public void EnforceTangentModeNoNotify(int index)
	{
		EnforceTangentModeNoNotify(new SplineRange(index, 1));
	}

	public void EnforceTangentModeNoNotify(SplineRange range)
	{
		for (int i = range.Start; i <= range.End; i++)
		{
			ApplyTangentModeNoNotify(i);
		}
	}

	public TangentMode GetTangentMode(int index)
	{
		EnsureMetaDataValid();
		if (m_MetaData.Count <= 0)
		{
			return TangentMode.Broken;
		}
		return m_MetaData[index].Mode;
	}

	public void SetTangentMode(TangentMode mode)
	{
		SetTangentMode(new SplineRange(0, Count), mode);
	}

	public void SetTangentMode(int index, TangentMode mode, BezierTangent main = BezierTangent.Out)
	{
		if (GetTangentMode(index) != mode)
		{
			if (index == Count - 1 && !Closed)
			{
				main = BezierTangent.In;
			}
			SetTangentMode(new SplineRange(index, 1), mode, main);
		}
	}

	public void SetTangentMode(SplineRange range, TangentMode mode, BezierTangent main = BezierTangent.Out)
	{
		foreach (int item in range)
		{
			CacheKnotOperationCurves(item);
			SetTangentModeNoNotify(item, mode, main);
			SetDirty(SplineModification.KnotModified, item);
		}
	}

	public void SetTangentModeNoNotify(int index, TangentMode mode, BezierTangent main = BezierTangent.Out)
	{
		EnsureMetaDataValid();
		BezierKnot bezierKnot = m_Knots[index];
		if (m_MetaData[index].Mode == TangentMode.Linear && mode >= TangentMode.Mirrored)
		{
			bezierKnot.TangentIn = SplineUtility.GetExplicitLinearTangent(bezierKnot, this.Previous(index));
			bezierKnot.TangentOut = SplineUtility.GetExplicitLinearTangent(bezierKnot, this.Next(index));
		}
		m_MetaData[index].Mode = mode;
		m_Knots[index] = bezierKnot;
		ApplyTangentModeNoNotify(index, main);
	}

	private void ApplyTangentModeNoNotify(int index, BezierTangent main = BezierTangent.Out)
	{
		BezierKnot value = m_Knots[index];
		switch (GetTangentMode(index))
		{
		case TangentMode.Continuous:
			value = value.BakeTangentDirectionToRotation(mirrored: false, main);
			break;
		case TangentMode.Mirrored:
			value = value.BakeTangentDirectionToRotation(mirrored: true, main);
			break;
		case TangentMode.Linear:
			value.TangentIn = float3.zero;
			value.TangentOut = float3.zero;
			break;
		case TangentMode.AutoSmooth:
			value = SplineUtility.GetAutoSmoothKnot(value.Position, this.Previous(index).Position, this.Next(index).Position, math.mul(value.Rotation, math.up()), m_MetaData[index].Tension);
			break;
		}
		m_Knots[index] = value;
		SetDirtyNoNotify();
	}

	public float GetAutoSmoothTension(int index)
	{
		return m_MetaData[index].Tension;
	}

	public void SetAutoSmoothTension(int index, float tension)
	{
		SetAutoSmoothTension(new SplineRange(index, 1), tension);
	}

	public void SetAutoSmoothTension(SplineRange range, float tension)
	{
		SetAutoSmoothTensionInternal(range, tension, setDirty: true);
	}

	public void SetAutoSmoothTensionNoNotify(int index, float tension)
	{
		SetAutoSmoothTensionInternal(new SplineRange(index, 1), tension, setDirty: false);
	}

	public void SetAutoSmoothTensionNoNotify(SplineRange range, float tension)
	{
		SetAutoSmoothTensionInternal(range, tension, setDirty: false);
	}

	private void SetAutoSmoothTensionInternal(SplineRange range, float tension, bool setDirty)
	{
		int i = 0;
		for (int count = range.Count; i < count; i++)
		{
			int num = range[i];
			CacheKnotOperationCurves(num);
			m_MetaData[num].Tension = tension;
			if (m_MetaData[num].Mode == TangentMode.AutoSmooth)
			{
				ApplyTangentModeNoNotify(num);
			}
			if (setDirty)
			{
				SetDirty(SplineModification.KnotModified, num);
			}
		}
	}

	internal void CheckAutoSmoothExtremityKnots()
	{
		if (GetTangentMode(0) == TangentMode.AutoSmooth)
		{
			ApplyTangentModeNoNotify(0);
		}
		if (Count > 2 && GetTangentMode(Count - 1) == TangentMode.AutoSmooth)
		{
			ApplyTangentModeNoNotify(Count - 1);
		}
	}

	public int IndexOf(BezierKnot item)
	{
		return m_Knots.IndexOf(item);
	}

	public void Insert(int index, BezierKnot knot)
	{
		Insert(index, knot, TangentMode.Broken, 0.5f);
	}

	public void Insert(int index, BezierKnot knot, TangentMode mode)
	{
		Insert(index, knot, mode, 0.5f);
	}

	public void Insert(int index, BezierKnot knot, TangentMode mode, float tension)
	{
		CacheKnotOperationCurves(index);
		InsertNoNotify(index, knot, mode, tension);
		SetDirty(SplineModification.KnotInserted, index);
	}

	private void InsertNoNotify(int index, BezierKnot knot, TangentMode mode, float tension)
	{
		EnsureMetaDataValid();
		m_Knots.Insert(index, knot);
		m_MetaData.Insert(index, new MetaData
		{
			Mode = mode,
			Tension = tension
		});
		int num = this.PreviousIndex(index);
		if (num != index)
		{
			ApplyTangentModeNoNotify(num);
		}
		ApplyTangentModeNoNotify(index);
		int num2 = this.NextIndex(index);
		if (num2 != index)
		{
			ApplyTangentModeNoNotify(num2);
		}
	}

	internal void InsertOnCurve(int index, float curveT)
	{
		int index2 = SplineUtility.PreviousIndex(index, Count, Closed);
		BezierKnot bezierKnot = m_Knots[index2];
		BezierKnot value = m_Knots[index];
		BezierCurve curve = new BezierCurve(bezierKnot, m_Knots[index]);
		CurveUtility.Split(curve, curveT, out var left, out var right);
		if (GetTangentMode(index2) == TangentMode.Mirrored)
		{
			SetTangentMode(index2, TangentMode.Continuous);
		}
		if (GetTangentMode(index) == TangentMode.Mirrored)
		{
			SetTangentMode(index, TangentMode.Continuous);
		}
		if (SplineUtility.AreTangentsModifiable(GetTangentMode(index2)))
		{
			bezierKnot.TangentOut = math.mul(math.inverse(bezierKnot.Rotation), left.Tangent0);
		}
		if (SplineUtility.AreTangentsModifiable(GetTangentMode(index)))
		{
			value.TangentIn = math.mul(math.inverse(value.Rotation), right.Tangent1);
		}
		float3 up = CurveUtility.EvaluateUpVector(curve, curveT, math.rotate(bezierKnot.Rotation, math.up()), math.rotate(value.Rotation, math.up()));
		quaternion quaternion = quaternion.LookRotationSafe(math.normalizesafe(right.Tangent0), up);
		quaternion q = math.inverse(quaternion);
		SetKnotNoNotify(index2, bezierKnot);
		SetKnotNoNotify(index, value);
		BezierKnot knot = new BezierKnot(left.P3, math.mul(q, left.Tangent1), math.mul(q, right.Tangent0), quaternion);
		Insert(index, knot);
	}

	public void RemoveAt(int index)
	{
		EnsureMetaDataValid();
		CacheKnotOperationCurves(index);
		m_Knots.RemoveAt(index);
		m_MetaData.RemoveAt(index);
		int index2 = Mathf.Clamp(index, 0, Count - 1);
		if (Count > 0)
		{
			ApplyTangentModeNoNotify(this.PreviousIndex(index2));
			ApplyTangentModeNoNotify(index2);
		}
		SetDirty(SplineModification.KnotRemoved, index);
	}

	public void SetKnot(int index, BezierKnot value, BezierTangent main = BezierTangent.Out)
	{
		CacheKnotOperationCurves(index);
		SetKnotNoNotify(index, value, main);
		SetDirty(SplineModification.KnotModified, index);
	}

	public void SetKnotNoNotify(int index, BezierKnot value, BezierTangent main = BezierTangent.Out)
	{
		m_Knots[index] = value;
		ApplyTangentModeNoNotify(index, main);
		int index2 = this.PreviousIndex(index);
		int index3 = this.NextIndex(index);
		if (m_MetaData[index2].Mode == TangentMode.AutoSmooth)
		{
			ApplyTangentModeNoNotify(index2, main);
		}
		if (m_MetaData[index3].Mode == TangentMode.AutoSmooth)
		{
			ApplyTangentModeNoNotify(index3, main);
		}
	}

	public Spline()
	{
	}

	public Spline(int knotCapacity, bool closed = false)
	{
		m_Knots = new List<BezierKnot>(knotCapacity);
		m_Closed = closed;
	}

	public Spline(IEnumerable<BezierKnot> knots, bool closed = false)
	{
		m_Knots = knots.ToList();
		m_Closed = closed;
	}

	public Spline(IEnumerable<float3> knotPositions, TangentMode tangentMode = TangentMode.AutoSmooth, bool closed = false)
	{
		InsertRangeNoNotify(Count, knotPositions, tangentMode);
		m_Closed = closed;
	}

	public Spline(Spline spline)
	{
		m_Knots = spline.Knots.ToList();
		m_Closed = spline.Closed;
		foreach (SplineDataKeyValuePair<int> intDatum in spline.m_IntData)
		{
			m_IntData[intDatum.Key] = intDatum.Value;
		}
		foreach (SplineDataKeyValuePair<float> floatDatum in spline.m_FloatData)
		{
			m_FloatData[floatDatum.Key] = floatDatum.Value;
		}
		foreach (SplineDataKeyValuePair<float4> float4Datum in spline.m_Float4Data)
		{
			m_Float4Data[float4Datum.Key] = float4Datum.Value;
		}
		foreach (SplineDataKeyValuePair<Object> objectDatum in spline.m_ObjectData)
		{
			m_ObjectData[objectDatum.Key] = objectDatum.Value;
		}
	}

	public BezierCurve GetCurve(int index)
	{
		int index2 = (m_Closed ? ((index + 1) % m_Knots.Count) : math.min(index + 1, m_Knots.Count - 1));
		return new BezierCurve(m_Knots[index], m_Knots[index2]);
	}

	public float GetCurveLength(int index)
	{
		EnsureMetaDataValid();
		DistanceToInterpolation[] distanceToInterpolation = m_MetaData[index].DistanceToInterpolation;
		if (distanceToInterpolation[0].Distance < 0f)
		{
			CurveUtility.CalculateCurveLengths(GetCurve(index), distanceToInterpolation);
		}
		if (distanceToInterpolation.Length == 0)
		{
			return 0f;
		}
		return distanceToInterpolation[^1].Distance;
	}

	public float GetLength()
	{
		if (m_Length < 0f)
		{
			m_Length = 0f;
			int i = 0;
			for (int num = (Closed ? Count : (Count - 1)); i < num; i++)
			{
				m_Length += GetCurveLength(i);
			}
		}
		return m_Length;
	}

	private DistanceToInterpolation[] GetCurveDistanceLut(int index)
	{
		if (m_MetaData[index].DistanceToInterpolation[0].Distance < 0f)
		{
			CurveUtility.CalculateCurveLengths(GetCurve(index), m_MetaData[index].DistanceToInterpolation);
		}
		return m_MetaData[index].DistanceToInterpolation;
	}

	public float GetCurveInterpolation(int curveIndex, float curveDistance)
	{
		return CurveUtility.GetDistanceToInterpolation(GetCurveDistanceLut(curveIndex), curveDistance);
	}

	private void WarmUpCurveUps()
	{
		EnsureMetaDataValid();
		int i = 0;
		for (int num = (Closed ? Count : (Count - 1)); i < num; i++)
		{
			this.EvaluateUpVectorsForCurve(i, m_MetaData[i].UpVectors);
		}
	}

	public float3 GetCurveUpVector(int index, float t)
	{
		EnsureMetaDataValid();
		float3[] upVectors = m_MetaData[index].UpVectors;
		if (math.all(upVectors[0] == float3.zero))
		{
			this.EvaluateUpVectorsForCurve(index, upVectors);
		}
		float num = 1f / (float)(upVectors.Length - 1);
		float num2 = 0f;
		for (int i = 0; i < upVectors.Length; i++)
		{
			if (t <= num2 + num)
			{
				return Vector3.Lerp(upVectors[i], upVectors[i + 1], (t - num2) / num);
			}
			num2 += num;
		}
		return upVectors[^1];
	}

	public void Warmup()
	{
		GetLength();
		WarmUpCurveUps();
	}

	public void Resize(int newSize)
	{
		int count = Count;
		newSize = math.max(0, newSize);
		if (newSize == count)
		{
			return;
		}
		if (newSize > count)
		{
			while (m_Knots.Count < newSize)
			{
				Add(default(BezierKnot));
			}
		}
		else if (newSize < count)
		{
			while (newSize < Count)
			{
				RemoveAt(Count - 1);
			}
			int num = newSize - 1;
			if (num > -1 && num < m_Knots.Count)
			{
				ApplyTangentModeNoNotify(num);
			}
		}
	}

	public BezierKnot[] ToArray()
	{
		return m_Knots.ToArray();
	}

	public void Copy(Spline copyFrom)
	{
		if (copyFrom != this)
		{
			m_Closed = copyFrom.Closed;
			m_Knots.Clear();
			m_Knots.AddRange(copyFrom.m_Knots);
			m_MetaData.Clear();
			for (int i = 0; i < copyFrom.m_MetaData.Count; i++)
			{
				m_MetaData.Add(new MetaData(copyFrom.m_MetaData[i]));
			}
			SetDirty(SplineModification.Default);
		}
	}

	public IEnumerator<BezierKnot> GetEnumerator()
	{
		return m_Knots.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_Knots.GetEnumerator();
	}

	public void Add(BezierKnot item)
	{
		Add(item, TangentMode.Broken);
	}

	public void Add(float3 knotPosition, TangentMode tangentMode = TangentMode.AutoSmooth)
	{
		Insert(Count, knotPosition, tangentMode);
	}

	public void AddRange(IEnumerable<float3> knotPositions, TangentMode tangentMode = TangentMode.AutoSmooth)
	{
		InsertRange(Count, knotPositions, tangentMode);
	}

	public void Insert(int index, float3 knotPosition, TangentMode tangentMode = TangentMode.AutoSmooth)
	{
		if (tangentMode == TangentMode.AutoSmooth)
		{
			Insert(index, new BezierKnot(knotPosition), tangentMode);
			return;
		}
		CacheKnotOperationCurves(index);
		InsertNoNotify(index, new BezierKnot(knotPosition), TangentMode.AutoSmooth, 1f / 3f);
		SetTangentModeNoNotify(index, tangentMode);
		SetDirty(SplineModification.KnotInserted, index);
	}

	public void InsertRange(int index, IEnumerable<float3> knotPositions, TangentMode tangentMode = TangentMode.AutoSmooth)
	{
		InsertRangeNoNotify(index, knotPositions, tangentMode, cacheCurves: true);
		SetDirty(SplineModification.KnotInserted);
	}

	private void InsertRangeNoNotify(int index, IEnumerable<float3> knotPositions, TangentMode tangentMode = TangentMode.AutoSmooth, bool cacheCurves = false)
	{
		int num = 0;
		foreach (float3 knotPosition in knotPositions)
		{
			int index2 = index + num;
			if (cacheCurves)
			{
				CacheKnotOperationCurves(index2);
			}
			InsertNoNotify(index2, new BezierKnot(knotPosition), TangentMode.AutoSmooth, 1f / 3f);
			num++;
		}
		if (tangentMode == TangentMode.AutoSmooth)
		{
			return;
		}
		num = 0;
		foreach (float3 knotPosition2 in knotPositions)
		{
			_ = knotPosition2;
			int index3 = index + num;
			SetTangentModeNoNotify(index3, tangentMode);
			num++;
		}
	}

	public void Add(BezierKnot item, TangentMode mode)
	{
		Insert(Count, item, mode);
	}

	public void Add(BezierKnot item, TangentMode mode, float tension)
	{
		Insert(Count, item, mode, tension);
	}

	public void Add(Spline spline)
	{
		for (int i = 0; i < spline.Count; i++)
		{
			Insert(Count, spline[i], spline.GetTangentMode(i), spline.GetAutoSmoothTension(i));
		}
	}

	public void Clear()
	{
		m_Knots.Clear();
		m_MetaData.Clear();
		SetDirty(SplineModification.KnotRemoved);
	}

	public bool Contains(BezierKnot item)
	{
		return m_Knots.Contains(item);
	}

	public void CopyTo(BezierKnot[] array, int arrayIndex)
	{
		m_Knots.CopyTo(array, arrayIndex);
	}

	public bool Remove(BezierKnot item)
	{
		int num = m_Knots.IndexOf(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	internal void RemoveUnusedSplineData()
	{
		m_FloatData.RemoveEmpty();
		m_Float4Data.RemoveEmpty();
		m_IntData.RemoveEmpty();
		m_ObjectData.RemoveEmpty();
	}

	internal void CacheKnotOperationCurves(int index)
	{
		if (Count > 1)
		{
			m_LastKnotChangeCurveLengths.curve0 = GetCurveLength(this.PreviousIndex(index));
			if (index < Count)
			{
				m_LastKnotChangeCurveLengths.curve1 = GetCurveLength(index);
			}
		}
	}
}
