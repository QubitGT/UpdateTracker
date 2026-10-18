using Unity.Mathematics;
using UnityEngine.Splines.ExtrusionShapes;

namespace UnityEngine.Splines;

public struct ExtrudeSettings<T> where T : IExtrudeShape
{
	private const int k_SegmentsMin = 2;

	private const int k_SegmentsMax = 4096;

	private const float k_RadiusMin = 1E-05f;

	private const float k_RadiusMax = 10000f;

	[SerializeField]
	private T m_Shape;

	[SerializeField]
	private bool m_CapEnds;

	[SerializeField]
	private bool m_FlipNormals;

	[SerializeField]
	private int m_SegmentCount;

	[SerializeField]
	private float m_Radius;

	[SerializeField]
	private Vector2 m_Range;

	public int SegmentCount
	{
		get
		{
			return m_SegmentCount;
		}
		set
		{
			m_SegmentCount = math.clamp(value, 2, 4096);
		}
	}

	public bool CapEnds
	{
		get
		{
			return m_CapEnds;
		}
		set
		{
			m_CapEnds = value;
		}
	}

	public bool FlipNormals
	{
		get
		{
			return m_FlipNormals;
		}
		set
		{
			m_FlipNormals = value;
		}
	}

	public float2 Range
	{
		get
		{
			return m_Range;
		}
		set
		{
			m_Range = math.clamp(new float2(math.min(value.x, value.y), math.max(value.x, value.y)), 0f, 1f);
		}
	}

	public float Radius
	{
		get
		{
			return m_Radius;
		}
		set
		{
			m_Radius = math.clamp(value, 1E-05f, 10000f);
		}
	}

	public T Shape
	{
		get
		{
			return m_Shape;
		}
		set
		{
			m_Shape = value;
		}
	}

	internal int sides
	{
		get
		{
			if (Shape is SplineShape)
			{
				if (!wrapped)
				{
					return Shape.SideCount;
				}
				return Shape.SideCount + 1;
			}
			if (!wrapped)
			{
				return Shape.SideCount + 1;
			}
			return Shape.SideCount;
		}
	}

	internal bool wrapped
	{
		get
		{
			if (Shape is SplineShape { Spline: not null } splineShape)
			{
				return splineShape.Spline.Closed;
			}
			if (Shape is Road)
			{
				return false;
			}
			return true;
		}
	}

	internal bool DoCapEnds<K>(K spline) where K : ISpline
	{
		if (m_CapEnds)
		{
			return !spline.Closed;
		}
		return false;
	}

	internal bool DoCloseSpline<K>(K spline) where K : ISpline
	{
		if (math.abs(1f - (Range.y - Range.x)) < float.Epsilon)
		{
			return spline.Closed;
		}
		return false;
	}

	public ExtrudeSettings(T shape)
		: this(16, capped: false, new float2(0f, 1f), 0.5f, shape)
	{
	}

	public ExtrudeSettings(int segments, bool capped, float2 range, float radius, T shape)
	{
		m_SegmentCount = math.clamp(segments, 2, 4096);
		m_FlipNormals = false;
		m_Range = math.clamp(new float2(math.min(range.x, range.y), math.max(range.x, range.y)), 0f, 1f);
		m_CapEnds = capped;
		m_Radius = math.clamp(radius, 1E-05f, 10000f);
		m_Shape = shape;
	}
}
