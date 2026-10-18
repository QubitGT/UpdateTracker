using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MeshConstraints
{
	private Dictionary<int, EdgeConstraint> Edges = new Dictionary<int, EdgeConstraint>();

	private int set_id_counter;

	private Dictionary<int, VertexConstraint> Vertices = new Dictionary<int, VertexConstraint>();

	public bool HasConstraints
	{
		get
		{
			if (Edges.Count <= 0)
			{
				return Vertices.Count > 0;
			}
			return true;
		}
	}

	public MeshConstraints()
	{
		set_id_counter = 0;
	}

	public int AllocateSetID()
	{
		return set_id_counter++;
	}

	public bool HasEdgeConstraint(int eid)
	{
		return Edges.ContainsKey(eid);
	}

	public EdgeConstraint GetEdgeConstraint(int eid)
	{
		if (Edges.TryGetValue(eid, out var value))
		{
			return value;
		}
		return EdgeConstraint.Unconstrained;
	}

	public void SetOrUpdateEdgeConstraint(int eid, EdgeConstraint ec)
	{
		Edges[eid] = ec;
	}

	public void ClearEdgeConstraint(int eid)
	{
		Edges.Remove(eid);
	}

	public List<int> FindConstrainedEdgesBySetID(int setID)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, EdgeConstraint> edge in Edges)
		{
			if (edge.Value.TrackingSetID == setID)
			{
				list.Add(edge.Key);
			}
		}
		return list;
	}

	public bool HasVertexConstraint(int vid)
	{
		return Vertices.ContainsKey(vid);
	}

	public VertexConstraint GetVertexConstraint(int vid)
	{
		if (Vertices.TryGetValue(vid, out var value))
		{
			return value;
		}
		return VertexConstraint.Unconstrained;
	}

	public bool GetVertexConstraint(int vid, ref VertexConstraint vc)
	{
		return Vertices.TryGetValue(vid, out vc);
	}

	public void SetOrUpdateVertexConstraint(int vid, VertexConstraint vc)
	{
		Vertices[vid] = vc;
	}

	public void ClearVertexConstraint(int vid)
	{
		Vertices.Remove(vid);
	}

	public IEnumerable VertexConstraintsItr()
	{
		foreach (KeyValuePair<int, VertexConstraint> vertex in Vertices)
		{
			yield return vertex;
		}
	}
}
