using System;
using Unity.Mathematics;

namespace UnityEngine.Splines.ExtrusionShapes;

[Serializable]
public class SplineShape : IExtrudeShape
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	[SerializeField]
	private SplineContainer m_Template;

	[SerializeField]
	[SplineIndex("m_Template")]
	private int m_SplineIndex;

	[SerializeField]
	[Min(2f)]
	private int m_SideCount = 12;

	[SerializeField]
	[Tooltip("The axis of the template spline to be used when winding the vertices along the extruded mesh.")]
	public Axis m_Axis = Axis.Y;

	public int SideCount
	{
		get
		{
			return m_SideCount;
		}
		set
		{
			m_SideCount = value;
		}
	}

	public SplineContainer SplineContainer
	{
		get
		{
			return m_Template;
		}
		set
		{
			m_Template = value;
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
			m_SplineIndex = math.max(0, value);
		}
	}

	public Spline Spline
	{
		get
		{
			if (!(m_Template != null))
			{
				return null;
			}
			return m_Template[m_SplineIndex % m_Template.Splines.Count];
		}
	}

	public float2 GetPosition(float t, int index)
	{
		if (Spline == null)
		{
			return 0f;
		}
		if (t == 1f)
		{
			t = 0.9999f;
		}
		else if (t == 0f)
		{
			t = 0.0001f;
		}
		return m_Axis switch
		{
			Axis.X => Spline.EvaluatePosition(1f - t).zy, 
			Axis.Y => Spline.EvaluatePosition(1f - t).xz, 
			Axis.Z => Spline.EvaluatePosition(1f - t).xy, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
