using System;
using Unity.Mathematics;

namespace UnityEngine.Splines.ExtrusionShapes;

[Serializable]
public sealed class Circle : IExtrudeShape
{
	[SerializeField]
	[Min(2f)]
	private int m_Sides = 8;

	private float m_Rads;

	public int SideCount
	{
		get
		{
			return m_Sides;
		}
		set
		{
			m_Sides = value;
		}
	}

	public void Setup(ISpline path, int segmentCount)
	{
		m_Rads = math.radians(360f / (float)SideCount);
	}

	public float2 GetPosition(float t, int index)
	{
		return new float2(math.cos((float)index * m_Rads), math.sin((float)index * m_Rads));
	}
}
