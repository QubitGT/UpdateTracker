using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[Serializable]
public class SplineData<T> : IEnumerable<DataPoint<T>>, IEnumerable, ISplineModificationHandler
{
	private static readonly DataPointComparer<DataPoint<T>> k_DataPointComparer = new DataPointComparer<DataPoint<T>>();

	[SerializeField]
	private PathIndexUnit m_IndexUnit = PathIndexUnit.Knot;

	[SerializeField]
	private T m_DefaultValue;

	[SerializeField]
	private List<DataPoint<T>> m_DataPoints = new List<DataPoint<T>>();

	[NonSerialized]
	private bool m_NeedsSort;

	public DataPoint<T> this[int index]
	{
		get
		{
			return m_DataPoints[index];
		}
		set
		{
			SetDataPoint(index, value);
		}
	}

	public PathIndexUnit PathIndexUnit
	{
		get
		{
			return m_IndexUnit;
		}
		set
		{
			m_IndexUnit = value;
		}
	}

	public T DefaultValue
	{
		get
		{
			return m_DefaultValue;
		}
		set
		{
			m_DefaultValue = value;
		}
	}

	public int Count => m_DataPoints.Count;

	public IEnumerable<float> Indexes => m_DataPoints.Select((DataPoint<T> dp) => dp.Index);

	[Obsolete("Use Changed instead.", false)]
	public event Action changed;

	public event Action Changed;

	public SplineData()
	{
	}

	public SplineData(T init)
	{
		Add(0f, init);
		SetDirty();
	}

	public SplineData(IEnumerable<DataPoint<T>> dataPoints)
	{
		foreach (DataPoint<T> dataPoint in dataPoints)
		{
			Add(dataPoint);
		}
		SetDirty();
	}

	private void SetDirty()
	{
		this.changed?.Invoke();
		this.Changed?.Invoke();
	}

	public void Add(float t, T data)
	{
		Add(new DataPoint<T>(t, data));
	}

	public int Add(DataPoint<T> dataPoint)
	{
		int num = m_DataPoints.BinarySearch(0, Count, dataPoint, k_DataPointComparer);
		num = ((num < 0) ? (~num) : num);
		m_DataPoints.Insert(num, dataPoint);
		SetDirty();
		return num;
	}

	public int AddDataPointWithDefaultValue(float t, bool useDefaultValue = false)
	{
		DataPoint<T> dataPoint = new DataPoint<T>(t, m_DefaultValue);
		if (Count == 0 || useDefaultValue)
		{
			return Add(dataPoint);
		}
		if (Count == 1)
		{
			dataPoint.Value = m_DataPoints[0].Value;
			return Add(dataPoint);
		}
		int num = m_DataPoints.BinarySearch(0, Count, dataPoint, k_DataPointComparer);
		num = ((num < 0) ? (~num) : num);
		dataPoint.Value = ((num == 0) ? m_DataPoints[0].Value : m_DataPoints[num - 1].Value);
		m_DataPoints.Insert(num, dataPoint);
		SetDirty();
		return num;
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_DataPoints.RemoveAt(index);
		SetDirty();
	}

	public bool RemoveDataPoint(float t)
	{
		bool num = m_DataPoints.Remove(m_DataPoints.FirstOrDefault((DataPoint<T> point) => Mathf.Approximately(point.Index, t)));
		if (num)
		{
			SetDirty();
		}
		return num;
	}

	public int MoveDataPoint(int index, float newIndex)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		DataPoint<T> dataPoint = m_DataPoints[index];
		if (Mathf.Approximately(newIndex, dataPoint.Index))
		{
			return index;
		}
		RemoveAt(index);
		dataPoint.Index = newIndex;
		return Add(dataPoint);
	}

	public void Clear()
	{
		m_DataPoints.Clear();
		SetDirty();
	}

	private static int Wrap(int value, int lowerBound, int upperBound)
	{
		int num = upperBound - lowerBound + 1;
		if (value < lowerBound)
		{
			value += num * ((lowerBound - value) / num + 1);
		}
		return lowerBound + (value - lowerBound) % num;
	}

	private int ResolveBinaryIndex(int index, bool wrap)
	{
		index = ((index < 0) ? (~index) : index) - 1;
		if (wrap)
		{
			index = Wrap(index, 0, Count - 1);
		}
		return math.clamp(index, 0, Count - 1);
	}

	private (int, int, float) GetIndex(float t, float splineLength, int knotCount, bool closed)
	{
		if (Count < 1)
		{
			return default((int, int, float));
		}
		SortIfNecessary();
		float num = splineLength;
		if (m_IndexUnit == PathIndexUnit.Normalized)
		{
			num = 1f;
		}
		else if (m_IndexUnit == PathIndexUnit.Knot)
		{
			num = (closed ? knotCount : (knotCount - 1));
		}
		float x = math.ceil(m_DataPoints[m_DataPoints.Count - 1].Index / num) * num;
		float num2 = (closed ? math.max(x, num) : num);
		t = ((!closed) ? math.clamp(t, 0f, num2) : ((!(t < 0f)) ? (t % num2) : (num2 + t % num2)));
		int index = m_DataPoints.BinarySearch(0, Count, new DataPoint<T>(t, default(T)), k_DataPointComparer);
		int num3 = ResolveBinaryIndex(index, closed);
		int num4 = (closed ? ((num3 + 1) % Count) : math.clamp(num3 + 1, 0, Count - 1));
		float index2 = m_DataPoints[num3].Index;
		float num5 = m_DataPoints[num4].Index;
		if (num3 > num4)
		{
			num5 += num2;
		}
		if (t < index2 && closed)
		{
			t += num2;
		}
		if (index2 == num5)
		{
			return (num3, num4, index2);
		}
		return (num3, num4, math.abs(math.max(0f, t - index2) / (num5 - index2)));
	}

	public T Evaluate<TSpline, TInterpolator>(TSpline spline, float t, PathIndexUnit indexUnit, TInterpolator interpolator) where TSpline : ISpline where TInterpolator : IInterpolator<T>
	{
		if (indexUnit == m_IndexUnit)
		{
			return Evaluate(spline, t, interpolator);
		}
		return Evaluate(spline, spline.ConvertIndexUnit(t, indexUnit, m_IndexUnit), interpolator);
	}

	public T Evaluate<TSpline, TInterpolator>(TSpline spline, float t, TInterpolator interpolator) where TSpline : ISpline where TInterpolator : IInterpolator<T>
	{
		int count = spline.Count;
		if (count < 1 || m_DataPoints.Count == 0)
		{
			return default(T);
		}
		(int, int, float) index = GetIndex(t, spline.GetLength(), count, spline.Closed);
		DataPoint<T> dataPoint = m_DataPoints[index.Item1];
		DataPoint<T> dataPoint2 = m_DataPoints[index.Item2];
		return interpolator.Interpolate(dataPoint.Value, dataPoint2.Value, index.Item3);
	}

	public void SetDataPoint(int index, DataPoint<T> value)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		RemoveAt(index);
		Add(value);
		SetDirty();
	}

	public void SetDataPointNoSort(int index, DataPoint<T> value)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_NeedsSort = true;
		m_DataPoints[index] = value;
	}

	public void SortIfNecessary()
	{
		if (m_NeedsSort)
		{
			m_NeedsSort = false;
			m_DataPoints.Sort();
			SetDirty();
		}
	}

	internal void ForceSort()
	{
		m_NeedsSort = true;
		SortIfNecessary();
	}

	public void ConvertPathUnit<TSplineType>(TSplineType spline, PathIndexUnit toUnit) where TSplineType : ISpline
	{
		if (toUnit != m_IndexUnit)
		{
			for (int i = 0; i < m_DataPoints.Count; i++)
			{
				DataPoint<T> dataPoint = m_DataPoints[i];
				float index = spline.ConvertIndexUnit(dataPoint.Index, m_IndexUnit, toUnit);
				m_DataPoints[i] = new DataPoint<T>(index, dataPoint.Value);
			}
			m_IndexUnit = toUnit;
			SetDirty();
		}
	}

	public float GetNormalizedInterpolation<TSplineType>(TSplineType spline, float t) where TSplineType : ISpline
	{
		return SplineUtility.GetNormalizedInterpolation(spline, t, m_IndexUnit);
	}

	public IEnumerator<DataPoint<T>> GetEnumerator()
	{
		int i = 0;
		int c = Count;
		while (i < c)
		{
			yield return m_DataPoints[i];
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private static float WrapInt(float index, int lowerBound, int upperBound)
	{
		return (float)Wrap((int)math.floor(index), lowerBound, upperBound) + math.frac(index);
	}

	private static float ClampInt(float index, int lowerBound, int upperBound)
	{
		return (float)math.clamp((int)math.floor(index), lowerBound, upperBound) + math.frac(index);
	}

	void ISplineModificationHandler.OnSplineModified(SplineModificationData data)
	{
		if (m_IndexUnit != PathIndexUnit.Knot || data.Modification == SplineModification.KnotModified || data.Modification == SplineModification.KnotReordered || data.Modification == SplineModification.Default)
		{
			return;
		}
		int knotIndex = data.KnotIndex;
		float prevCurveLength = data.PrevCurveLength;
		float nextCurveLength = data.NextCurveLength;
		List<int> list = new List<int>();
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			DataPoint<T> value = m_DataPoints[i];
			int num = (int)math.floor(value.Index);
			float num2 = value.Index - (float)num;
			if (data.Modification == SplineModification.KnotInserted)
			{
				float curveLength = data.Spline.GetCurveLength(data.Spline.PreviousIndex(knotIndex));
				if (num == knotIndex - 1)
				{
					if (num2 < curveLength / prevCurveLength)
					{
						value.Index = (float)num + num2 * (prevCurveLength / curveLength);
					}
					else
					{
						value.Index = (float)(num + 1) + (num2 * prevCurveLength - curveLength) / (prevCurveLength - curveLength);
					}
				}
				else if (data.Spline.Closed && num == data.Spline.Count - 2 && knotIndex == 0)
				{
					if (num2 < curveLength / prevCurveLength)
					{
						value.Index = (float)(num + 1) + num2 * (prevCurveLength / curveLength);
					}
					else
					{
						value.Index = (num2 * prevCurveLength - curveLength) / (prevCurveLength - curveLength);
					}
				}
				else if (num >= knotIndex)
				{
					value.Index += 1f;
				}
			}
			else if (data.Modification == SplineModification.KnotRemoved)
			{
				if (knotIndex == -1)
				{
					list.Add(i);
					continue;
				}
				bool num3 = num2 == 0f && num == knotIndex;
				bool flag = !data.Spline.Closed && ((num <= 0 && knotIndex == 0) || (knotIndex == data.Spline.Count && math.ceil(value.Index) >= (float)knotIndex));
				if (num3 || flag || data.Spline.Count == 1)
				{
					list.Add(i);
				}
				else if (num == knotIndex - 1)
				{
					value.Index = (float)num + num2 * prevCurveLength / (prevCurveLength + nextCurveLength);
				}
				else if (num == knotIndex)
				{
					value.Index = (float)(num - 1) + (prevCurveLength + num2 * nextCurveLength) / (prevCurveLength + nextCurveLength);
				}
				else if (data.Spline.Closed && knotIndex == 0 && num == data.Spline.Count)
				{
					value.Index = (float)(num - 1) + num2 * prevCurveLength / (prevCurveLength + nextCurveLength);
				}
				else if (num >= knotIndex)
				{
					value.Index -= 1f;
				}
			}
			else if (!data.Spline.Closed && math.ceil(value.Index) >= (float)data.Spline.Count)
			{
				list.Add(i);
			}
			m_DataPoints[i] = value;
		}
		for (int num4 = list.Count - 1; num4 > -1; num4--)
		{
			m_DataPoints.RemoveAt(list[num4]);
		}
	}
}
