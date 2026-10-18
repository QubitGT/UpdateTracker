using System;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[Serializable]
public class EmbeddedSplineData
{
	[SerializeField]
	private SplineContainer m_Container;

	[SerializeField]
	private int m_SplineIndex;

	[SerializeField]
	private EmbeddedSplineDataType m_Type;

	[SerializeField]
	private string m_Key;

	public SplineContainer Container
	{
		get
		{
			return m_Container;
		}
		set
		{
			m_Container = value;
		}
	}

	public int SplineIndex
	{
		get
		{
			return m_SplineIndex;
		}
		set
		{
			m_SplineIndex = value;
		}
	}

	public EmbeddedSplineDataType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public string Key
	{
		get
		{
			return m_Key;
		}
		set
		{
			m_Key = value;
		}
	}

	public EmbeddedSplineData()
		: this(null, EmbeddedSplineDataType.Float)
	{
	}

	public EmbeddedSplineData(string key, EmbeddedSplineDataType type, SplineContainer container = null, int splineIndex = 0)
	{
		m_Container = container;
		m_SplineIndex = splineIndex;
		m_Key = key;
		m_Type = type;
	}

	public bool TryGetSpline(out Spline spline)
	{
		if (Container == null || SplineIndex < 0 || SplineIndex >= Container.Splines.Count)
		{
			spline = null;
		}
		else
		{
			spline = Container.Splines[SplineIndex];
		}
		return spline != null;
	}

	public bool TryGetFloatData(out SplineData<float> data)
	{
		if (Type != EmbeddedSplineDataType.Float)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(float)}");
		}
		return Container.Splines[SplineIndex].TryGetFloatData(Key, out data);
	}

	public bool TryGetFloat4Data(out SplineData<float4> data)
	{
		if (Type != EmbeddedSplineDataType.Float4)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(float4)}");
		}
		return Container.Splines[SplineIndex].TryGetFloat4Data(Key, out data);
	}

	public bool TryGetIntData(out SplineData<int> data)
	{
		if (Type != 0)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(int)}");
		}
		return Container.Splines[SplineIndex].TryGetIntData(Key, out data);
	}

	public bool TryGetObjectData(out SplineData<Object> data)
	{
		if (Type != EmbeddedSplineDataType.Object)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(Object)}");
		}
		return Container.Splines[SplineIndex].TryGetObjectData(Key, out data);
	}

	public SplineData<float> GetOrCreateFloatData()
	{
		if (Type != EmbeddedSplineDataType.Float)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(float)}");
		}
		return Container.Splines[SplineIndex].GetOrCreateFloatData(Key);
	}

	public SplineData<float4> GetOrCreateFloat4Data()
	{
		if (Type != EmbeddedSplineDataType.Float4)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(float4)}");
		}
		return Container.Splines[SplineIndex].GetOrCreateFloat4Data(Key);
	}

	public SplineData<int> GetOrCreateIntData()
	{
		if (Type != 0)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(int)}");
		}
		return Container.Splines[SplineIndex].GetOrCreateIntData(Key);
	}

	public SplineData<Object> GetOrCreateObjectData()
	{
		if (Type != EmbeddedSplineDataType.Object)
		{
			throw new InvalidCastException($"EmbeddedSplineDataType {Type} does not match requested SplineData collection: {typeof(Object)}");
		}
		return Container.Splines[SplineIndex].GetOrCreateObjectData(Key);
	}
}
