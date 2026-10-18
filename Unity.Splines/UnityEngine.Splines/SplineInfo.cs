using System;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[Serializable]
public struct SplineInfo : IEquatable<SplineInfo>, ISerializationCallbackReceiver
{
	[SerializeField]
	private Object m_Object;

	[SerializeReference]
	private ISplineContainer m_Container;

	[SerializeField]
	private int m_SplineIndex;

	public Object Object => m_Object;

	public ISplineContainer Container
	{
		get
		{
			return m_Container ?? (m_Object as ISplineContainer);
		}
		set
		{
			m_Container = value;
		}
	}

	public Transform Transform
	{
		get
		{
			if (!(Object is Component component))
			{
				return null;
			}
			return component.transform;
		}
	}

	public Spline Spline
	{
		get
		{
			if (Container == null || Index <= -1 || Index >= Container.Splines.Count)
			{
				return null;
			}
			return Container.Splines[Index];
		}
	}

	public int Index
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

	public float4x4 LocalToWorld
	{
		get
		{
			if (!(Transform != null))
			{
				return float4x4.identity;
			}
			return Transform.localToWorldMatrix;
		}
	}

	public SplineInfo(ISplineContainer container, int index)
	{
		m_Container = container;
		m_Object = container as Object;
		m_SplineIndex = index;
	}

	public bool Equals(SplineInfo other)
	{
		if (object.Equals(Container, other.Container))
		{
			return Index == other.Index;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is SplineInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((Container != null) ? Container.GetHashCode() : 0) * 397) ^ Index;
	}

	public void OnBeforeSerialize()
	{
		if (m_Container is Object @object)
		{
			m_Object = @object;
			m_Container = null;
		}
	}

	public void OnAfterDeserialize()
	{
		if (m_Container == null)
		{
			m_Container = m_Object as ISplineContainer;
		}
	}
}
