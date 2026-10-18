using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

internal sealed class Bounds2D
{
	public Vector2 center = Vector2.zero;

	[SerializeField]
	private Vector2 m_Size = Vector2.zero;

	[SerializeField]
	private Vector2 m_Extents = Vector2.zero;

	public Vector2 size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size = value;
			m_Extents.x = m_Size.x * 0.5f;
			m_Extents.y = m_Size.y * 0.5f;
		}
	}

	public Vector2 extents => m_Extents;

	public Vector2[] corners => new Vector2[4]
	{
		new Vector2(center.x - extents.x, center.y + extents.y),
		new Vector2(center.x + extents.x, center.y + extents.y),
		new Vector2(center.x - extents.x, center.y - extents.y),
		new Vector2(center.x + extents.x, center.y - extents.y)
	};

	public Bounds2D()
	{
	}

	public Bounds2D(Vector2 center, Vector2 size)
	{
		this.center = center;
		this.size = size;
	}

	public Bounds2D(IList<Vector2> points)
	{
		SetWithPoints(points);
	}

	public Bounds2D(IList<Vector2> points, IList<int> indexes)
	{
		SetWithPoints(points, indexes);
	}

	internal Bounds2D(Vector3[] points, Edge[] edges)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (points.Length != 0 && edges.Length != 0)
		{
			num = points[edges[0].a].x;
			num3 = points[edges[0].a].y;
			num2 = num;
			num4 = num3;
			for (int i = 0; i < edges.Length; i++)
			{
				num = Mathf.Min(num, points[edges[i].a].x);
				num = Mathf.Min(num, points[edges[i].b].x);
				num3 = Mathf.Min(num3, points[edges[i].a].y);
				num3 = Mathf.Min(num3, points[edges[i].b].y);
				num2 = Mathf.Max(num2, points[edges[i].a].x);
				num2 = Mathf.Max(num2, points[edges[i].b].x);
				num4 = Mathf.Max(num4, points[edges[i].a].y);
				num4 = Mathf.Max(num4, points[edges[i].b].y);
			}
		}
		center = new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
		size = new Vector3(num2 - num, num4 - num3);
	}

	public Bounds2D(Vector2[] points, int length)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (points.Length != 0)
		{
			num = points[0].x;
			num3 = points[0].y;
			num2 = num;
			num4 = num3;
			for (int i = 1; i < length; i++)
			{
				num = Mathf.Min(num, points[i].x);
				num3 = Mathf.Min(num3, points[i].y);
				num2 = Mathf.Max(num2, points[i].x);
				num4 = Mathf.Max(num4, points[i].y);
			}
		}
		center = new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
		size = new Vector3(num2 - num, num4 - num3);
	}

	public bool ContainsPoint(Vector2 point)
	{
		if (!(point.x > center.x + extents.x) && !(point.x < center.x - extents.x) && !(point.y > center.y + extents.y))
		{
			return !(point.y < center.y - extents.y);
		}
		return false;
	}

	public bool IntersectsLineSegment(Vector2 lineStart, Vector2 lineEnd)
	{
		if (ContainsPoint(lineStart) || ContainsPoint(lineEnd))
		{
			return true;
		}
		Vector2[] array = corners;
		if (!Math.GetLineSegmentIntersect(array[0], array[1], lineStart, lineEnd) && !Math.GetLineSegmentIntersect(array[1], array[3], lineStart, lineEnd) && !Math.GetLineSegmentIntersect(array[3], array[2], lineStart, lineEnd))
		{
			return Math.GetLineSegmentIntersect(array[2], array[0], lineStart, lineEnd);
		}
		return true;
	}

	public bool Intersects(Bounds2D bounds)
	{
		Vector2 vector = center - bounds.center;
		Vector2 vector2 = size + bounds.size;
		if (Mathf.Abs(vector.x) * 2f < vector2.x)
		{
			return Mathf.Abs(vector.y) * 2f < vector2.y;
		}
		return false;
	}

	public bool Intersects(Rect rect)
	{
		Vector2 vector = center - rect.center;
		Vector2 vector2 = size + rect.size;
		if (Mathf.Abs(vector.x) * 2f < vector2.x)
		{
			return Mathf.Abs(vector.y) * 2f < vector2.y;
		}
		return false;
	}

	public void SetWithPoints(IList<Vector2> points)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = points.Count;
		if (count > 0)
		{
			num = points[0].x;
			num3 = points[0].y;
			num2 = num;
			num4 = num3;
			for (int i = 1; i < count; i++)
			{
				float x = points[i].x;
				float y = points[i].y;
				if (x < num)
				{
					num = x;
				}
				if (x > num2)
				{
					num2 = x;
				}
				if (y < num3)
				{
					num3 = y;
				}
				if (y > num4)
				{
					num4 = y;
				}
			}
		}
		center.x = (num + num2) / 2f;
		center.y = (num3 + num4) / 2f;
		m_Size.x = num2 - num;
		m_Size.y = num4 - num3;
		m_Extents.x = m_Size.x * 0.5f;
		m_Extents.y = m_Size.y * 0.5f;
	}

	public void SetWithPoints(IList<Vector2> points, IList<int> indexes)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (points.Count > 0 && indexes.Count > 0)
		{
			num = points[indexes[0]].x;
			num3 = points[indexes[0]].y;
			num2 = num;
			num4 = num3;
			for (int i = 1; i < indexes.Count; i++)
			{
				float x = points[indexes[i]].x;
				float y = points[indexes[i]].y;
				if (x < num)
				{
					num = x;
				}
				if (x > num2)
				{
					num2 = x;
				}
				if (y < num3)
				{
					num3 = y;
				}
				if (y > num4)
				{
					num4 = y;
				}
			}
		}
		center.x = (num + num2) / 2f;
		center.y = (num3 + num4) / 2f;
		m_Size.x = num2 - num;
		m_Size.y = num4 - num3;
		m_Extents.x = m_Size.x * 0.5f;
		m_Extents.y = m_Size.y * 0.5f;
	}

	public static Vector2 Center(IList<Vector2> points)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = points.Count;
		num = points[0].x;
		num3 = points[0].y;
		num2 = num;
		num4 = num3;
		for (int i = 1; i < count; i++)
		{
			float x = points[i].x;
			float y = points[i].y;
			if (x < num)
			{
				num = x;
			}
			if (x > num2)
			{
				num2 = x;
			}
			if (y < num3)
			{
				num3 = y;
			}
			if (y > num4)
			{
				num4 = y;
			}
		}
		return new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
	}

	public static Vector2 Center(IList<Vector2> points, IList<int> indexes)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = indexes.Count;
		num = points[indexes[0]].x;
		num3 = points[indexes[0]].y;
		num2 = num;
		num4 = num3;
		for (int i = 1; i < count; i++)
		{
			float x = points[indexes[i]].x;
			float y = points[indexes[i]].y;
			if (x < num)
			{
				num = x;
			}
			if (x > num2)
			{
				num2 = x;
			}
			if (y < num3)
			{
				num3 = y;
			}
			if (y > num4)
			{
				num4 = y;
			}
		}
		return new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
	}

	public static Vector2 Size(IList<Vector2> points, IList<int> indexes)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int count = indexes.Count;
		num = points[indexes[0]].x;
		num3 = points[indexes[0]].y;
		num2 = num;
		num4 = num3;
		for (int i = 1; i < count; i++)
		{
			float x = points[indexes[i]].x;
			float y = points[indexes[i]].y;
			if (x < num)
			{
				num = x;
			}
			if (x > num2)
			{
				num2 = x;
			}
			if (y < num3)
			{
				num3 = y;
			}
			if (y > num4)
			{
				num4 = y;
			}
		}
		return new Vector2(num2 - num, num4 - num3);
	}

	internal static Vector2 Center(IList<Vector4> points, IEnumerable<int> indexes)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (indexes.Any())
		{
			int index = indexes.First();
			num = points[index].x;
			num3 = points[index].y;
			num2 = num;
			num4 = num3;
			foreach (int index2 in indexes)
			{
				float x = points[index2].x;
				float y = points[index2].y;
				if (x < num)
				{
					num = x;
				}
				if (x > num2)
				{
					num2 = x;
				}
				if (y < num3)
				{
					num3 = y;
				}
				if (y > num4)
				{
					num4 = y;
				}
			}
		}
		return new Vector2((num + num2) / 2f, (num3 + num4) / 2f);
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "[cen: ", null, null, null, null };
		Vector2 vector = center;
		obj[1] = vector.ToString();
		obj[2] = " size: ";
		obj[3] = size.ToString();
		obj[4] = "]";
		return string.Concat(obj);
	}
}
