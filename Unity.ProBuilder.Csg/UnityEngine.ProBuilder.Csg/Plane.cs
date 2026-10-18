using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.Csg;

internal sealed class Plane
{
	[Flags]
	private enum EPolygonType
	{
		Coplanar = 0,
		Front = 1,
		Back = 2,
		Spanning = 3
	}

	public Vector3 normal;

	public float w;

	public Plane()
	{
		normal = Vector3.zero;
		w = 0f;
	}

	public Plane(Vector3 a, Vector3 b, Vector3 c)
	{
		normal = Vector3.Cross(b - a, c - a);
		w = Vector3.Dot(normal, a);
	}

	public override string ToString()
	{
		return $"{normal} {w}";
	}

	public bool Valid()
	{
		return normal.magnitude > 0f;
	}

	public void Flip()
	{
		normal *= -1f;
		w *= -1f;
	}

	public void SplitPolygon(Polygon polygon, List<Polygon> coplanarFront, List<Polygon> coplanarBack, List<Polygon> front, List<Polygon> back)
	{
		EPolygonType ePolygonType = EPolygonType.Coplanar;
		List<EPolygonType> list = new List<EPolygonType>();
		for (int i = 0; i < polygon.vertices.Count; i++)
		{
			float num = Vector3.Dot(normal, polygon.vertices[i].position) - w;
			EPolygonType ePolygonType2 = ((num < 0f - CSG.epsilon) ? EPolygonType.Back : ((num > CSG.epsilon) ? EPolygonType.Front : EPolygonType.Coplanar));
			ePolygonType |= ePolygonType2;
			list.Add(ePolygonType2);
		}
		switch (ePolygonType)
		{
		case EPolygonType.Coplanar:
			if (Vector3.Dot(normal, polygon.plane.normal) > 0f)
			{
				coplanarFront.Add(polygon);
			}
			else
			{
				coplanarBack.Add(polygon);
			}
			break;
		case EPolygonType.Front:
			front.Add(polygon);
			break;
		case EPolygonType.Back:
			back.Add(polygon);
			break;
		case EPolygonType.Spanning:
		{
			List<Vertex> list2 = new List<Vertex>();
			List<Vertex> list3 = new List<Vertex>();
			for (int j = 0; j < polygon.vertices.Count; j++)
			{
				int index = (j + 1) % polygon.vertices.Count;
				EPolygonType ePolygonType3 = list[j];
				EPolygonType ePolygonType4 = list[index];
				Vertex vertex = polygon.vertices[j];
				Vertex y = polygon.vertices[index];
				if (ePolygonType3 != EPolygonType.Back)
				{
					list2.Add(vertex);
				}
				if (ePolygonType3 != EPolygonType.Front)
				{
					list3.Add(vertex);
				}
				if ((ePolygonType3 | ePolygonType4) == EPolygonType.Spanning)
				{
					float weight = (w - Vector3.Dot(normal, vertex.position)) / Vector3.Dot(normal, y.position - vertex.position);
					Vertex item = vertex.Mix(y, weight);
					list2.Add(item);
					list3.Add(item);
				}
			}
			if (list2.Count >= 3)
			{
				if (list2.SequenceEqual(polygon.vertices))
				{
					front.Add(polygon);
				}
				else
				{
					Polygon polygon2 = new Polygon(list2, polygon.material);
					if (polygon2.plane.Valid())
					{
						front.Add(polygon2);
					}
				}
			}
			if (list3.Count < 3)
			{
				break;
			}
			if (list3.SequenceEqual(polygon.vertices))
			{
				back.Add(polygon);
				break;
			}
			Polygon polygon3 = new Polygon(list3, polygon.material);
			if (polygon3.plane.Valid())
			{
				back.Add(polygon3);
			}
			break;
		}
		}
	}
}
