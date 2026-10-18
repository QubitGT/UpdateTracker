using System;
using UnityEngine.Serialization;

namespace UnityEngine.Splines;

[Serializable]
public struct DataPoint<TDataType> : IComparable<DataPoint<TDataType>>, IComparable<float>, IDataPoint
{
	[FormerlySerializedAs("m_Time")]
	[SerializeField]
	private float m_Index;

	[SerializeField]
	private TDataType m_Value;

	public float Index
	{
		get
		{
			return m_Index;
		}
		set
		{
			m_Index = value;
		}
	}

	public TDataType Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public DataPoint(float index, TDataType value)
	{
		m_Index = index;
		m_Value = value;
	}

	public int CompareTo(DataPoint<TDataType> other)
	{
		return Index.CompareTo(other.Index);
	}

	public int CompareTo(float other)
	{
		return Index.CompareTo(other);
	}

	public override string ToString()
	{
		return $"{Index} {Value}";
	}
}
