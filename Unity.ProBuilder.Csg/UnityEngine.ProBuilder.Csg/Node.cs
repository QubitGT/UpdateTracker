using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.Csg;

internal sealed class Node
{
	public List<Polygon> polygons;

	public Node front;

	public Node back;

	public Plane plane;

	public Node()
	{
		front = null;
		back = null;
	}

	public Node(List<Polygon> list)
	{
		Build(list);
	}

	public Node(List<Polygon> list, Plane plane, Node front, Node back)
	{
		polygons = list;
		this.plane = plane;
		this.front = front;
		this.back = back;
	}

	public Node Clone()
	{
		return new Node(polygons, plane, front, back);
	}

	public void ClipTo(Node other)
	{
		polygons = other.ClipPolygons(polygons);
		if (front != null)
		{
			front.ClipTo(other);
		}
		if (back != null)
		{
			back.ClipTo(other);
		}
	}

	public void Invert()
	{
		for (int i = 0; i < polygons.Count; i++)
		{
			polygons[i].Flip();
		}
		plane.Flip();
		if (front != null)
		{
			front.Invert();
		}
		if (back != null)
		{
			back.Invert();
		}
		Node node = front;
		front = back;
		back = node;
	}

	public void Build(List<Polygon> list)
	{
		if (list.Count < 1)
		{
			return;
		}
		bool flag = plane == null || !plane.Valid();
		if (flag)
		{
			plane = new Plane();
			plane.normal = list[0].plane.normal;
			plane.w = list[0].plane.w;
		}
		if (polygons == null)
		{
			polygons = new List<Polygon>();
		}
		List<Polygon> list2 = new List<Polygon>();
		List<Polygon> list3 = new List<Polygon>();
		for (int i = 0; i < list.Count; i++)
		{
			plane.SplitPolygon(list[i], polygons, polygons, list2, list3);
		}
		if (list2.Count > 0)
		{
			if (flag && list.SequenceEqual(list2))
			{
				polygons.AddRange(list2);
			}
			else
			{
				(front ?? (front = new Node())).Build(list2);
			}
		}
		if (list3.Count > 0)
		{
			if (flag && list.SequenceEqual(list3))
			{
				polygons.AddRange(list3);
			}
			else
			{
				(back ?? (back = new Node())).Build(list3);
			}
		}
	}

	public List<Polygon> ClipPolygons(List<Polygon> list)
	{
		if (!plane.Valid())
		{
			return list;
		}
		List<Polygon> list2 = new List<Polygon>();
		List<Polygon> list3 = new List<Polygon>();
		for (int i = 0; i < list.Count; i++)
		{
			plane.SplitPolygon(list[i], list2, list3, list2, list3);
		}
		if (front != null)
		{
			list2 = front.ClipPolygons(list2);
		}
		if (back != null)
		{
			list3 = back.ClipPolygons(list3);
		}
		else
		{
			list3.Clear();
		}
		list2.AddRange(list3);
		return list2;
	}

	public List<Polygon> AllPolygons()
	{
		List<Polygon> list = polygons;
		List<Polygon> collection = new List<Polygon>();
		List<Polygon> collection2 = new List<Polygon>();
		if (front != null)
		{
			collection = front.AllPolygons();
		}
		if (back != null)
		{
			collection2 = back.AllPolygons();
		}
		list.AddRange(collection);
		list.AddRange(collection2);
		return list;
	}

	public static Node Union(Node a1, Node b1)
	{
		Node node = a1.Clone();
		Node node2 = b1.Clone();
		node.ClipTo(node2);
		node2.ClipTo(node);
		node2.Invert();
		node2.ClipTo(node);
		node2.Invert();
		node.Build(node2.AllPolygons());
		return new Node(node.AllPolygons());
	}

	public static Node Subtract(Node a1, Node b1)
	{
		Node node = a1.Clone();
		Node node2 = b1.Clone();
		node.Invert();
		node.ClipTo(node2);
		node2.ClipTo(node);
		node2.Invert();
		node2.ClipTo(node);
		node2.Invert();
		node.Build(node2.AllPolygons());
		node.Invert();
		return new Node(node.AllPolygons());
	}

	public static Node Intersect(Node a1, Node b1)
	{
		Node node = a1.Clone();
		Node node2 = b1.Clone();
		node.Invert();
		node2.ClipTo(node);
		node2.Invert();
		node.ClipTo(node2);
		node2.ClipTo(node);
		node.Build(node2.AllPolygons());
		node.Invert();
		return new Node(node.AllPolygons());
	}
}
