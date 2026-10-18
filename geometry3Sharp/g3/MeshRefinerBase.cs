using System;

namespace g3;

public class MeshRefinerBase
{
	protected DMesh3 mesh;

	protected MeshConstraints constraints;

	public bool AllowCollapseFixedVertsWithSameSetID = true;

	protected double edge_flip_tol;

	public ProgressCancel Progress;

	public double EdgeFlipTolerance
	{
		get
		{
			return edge_flip_tol;
		}
		set
		{
			edge_flip_tol = MathUtil.Clamp(value, -1.0, 1.0);
		}
	}

	public DMesh3 Mesh => mesh;

	public MeshConstraints Constraints => constraints;

	public MeshRefinerBase(DMesh3 mesh)
	{
		this.mesh = mesh;
	}

	protected MeshRefinerBase()
	{
	}

	public void SetExternalConstraints(MeshConstraints cons)
	{
		constraints = cons;
	}

	protected virtual bool Cancelled()
	{
		if (Progress != null)
		{
			return Progress.Cancelled();
		}
		return false;
	}

	protected double edge_flip_metric(ref Vector3d n0, ref Vector3d n1)
	{
		if (edge_flip_tol == 0.0)
		{
			return n0.Dot(n1);
		}
		return n0.Normalized.Dot(n1.Normalized);
	}

	protected bool collapse_creates_flip_or_invalid(int vid, int vother, ref Vector3d newv, int tc, int td)
	{
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		foreach (int item in mesh.VtxTrianglesItr(vid))
		{
			if (item == tc || item == td)
			{
				continue;
			}
			Index3i triangle = mesh.GetTriangle(item);
			if (triangle.a == vother || triangle.b == vother || triangle.c == vother)
			{
				return true;
			}
			mesh.GetTriVertices(item, ref v, ref v2, ref v3);
			Vector3d n = (v2 - v).Cross(v3 - v);
			double num = 0.0;
			if (triangle.a == vid)
			{
				Vector3d n2 = (v2 - newv).Cross(v3 - newv);
				num = edge_flip_metric(ref n, ref n2);
			}
			else if (triangle.b == vid)
			{
				Vector3d n3 = (newv - v).Cross(v3 - v);
				num = edge_flip_metric(ref n, ref n3);
			}
			else
			{
				if (triangle.c != vid)
				{
					throw new Exception("should never be here!");
				}
				Vector3d n4 = (v2 - v).Cross(newv - v);
				num = edge_flip_metric(ref n, ref n4);
			}
			if (num <= edge_flip_tol)
			{
				return true;
			}
		}
		return false;
	}

	protected bool flip_inverts_normals(int a, int b, int c, int d, int t0)
	{
		Vector3d v = mesh.GetVertex(c);
		Vector3d v2 = mesh.GetVertex(d);
		Index3i tri_verts = mesh.GetTriangle(t0);
		int a2 = a;
		int b2 = b;
		IndexUtil.orient_tri_edge(ref a2, ref b2, ref tri_verts);
		Vector3d v3 = mesh.GetVertex(a2);
		Vector3d v4 = mesh.GetVertex(b2);
		Vector3d n = MathUtil.FastNormalDirection(ref v3, ref v4, ref v);
		Vector3d n2 = MathUtil.FastNormalDirection(ref v4, ref v3, ref v2);
		Vector3d n3 = MathUtil.FastNormalDirection(ref v, ref v2, ref v4);
		if (edge_flip_metric(ref n, ref n3) <= edge_flip_tol || edge_flip_metric(ref n2, ref n3) <= edge_flip_tol)
		{
			return true;
		}
		Vector3d n4 = MathUtil.FastNormalDirection(ref v2, ref v, ref v3);
		if (edge_flip_metric(ref n, ref n4) <= edge_flip_tol || edge_flip_metric(ref n2, ref n4) <= edge_flip_tol)
		{
			return true;
		}
		return false;
	}

	protected bool can_collapse_constraints(int eid, int a, int b, int c, int d, int tc, int td, out int collapse_to)
	{
		collapse_to = -1;
		if (constraints == null)
		{
			return true;
		}
		if (!can_collapse_vtx(eid, a, b, out collapse_to))
		{
			return false;
		}
		int vA = ((collapse_to == a) ? b : a);
		if (c != -1)
		{
			int eid2 = mesh.FindEdgeFromTri(vA, c, tc);
			if (!constraints.GetEdgeConstraint(eid2).IsUnconstrained)
			{
				return false;
			}
		}
		if (d != -1)
		{
			int eid3 = mesh.FindEdgeFromTri(vA, d, td);
			if (!constraints.GetEdgeConstraint(eid3).IsUnconstrained)
			{
				return false;
			}
		}
		return true;
	}

	protected bool can_collapse_vtx(int eid, int a, int b, out int collapse_to)
	{
		collapse_to = -1;
		if (constraints == null)
		{
			return true;
		}
		VertexConstraint vertexConstraint = constraints.GetVertexConstraint(a);
		VertexConstraint vertexConstraint2 = constraints.GetVertexConstraint(b);
		if (!vertexConstraint.Fixed && !vertexConstraint2.Fixed && vertexConstraint.Target == null && vertexConstraint2.Target == null)
		{
			return true;
		}
		if (vertexConstraint.Fixed && !vertexConstraint2.Fixed)
		{
			if (vertexConstraint2.Target != null && vertexConstraint2.Target != vertexConstraint.Target)
			{
				return false;
			}
			collapse_to = a;
			return true;
		}
		if (vertexConstraint2.Fixed && !vertexConstraint.Fixed)
		{
			if (vertexConstraint.Target != null && vertexConstraint.Target != vertexConstraint2.Target)
			{
				return false;
			}
			collapse_to = b;
			return true;
		}
		if (AllowCollapseFixedVertsWithSameSetID && vertexConstraint.FixedSetID >= 0 && vertexConstraint.FixedSetID == vertexConstraint2.FixedSetID)
		{
			return true;
		}
		if (vertexConstraint.Target != null && vertexConstraint2.Target == null)
		{
			collapse_to = a;
			return true;
		}
		if (vertexConstraint2.Target != null && vertexConstraint.Target == null)
		{
			collapse_to = b;
			return true;
		}
		if (vertexConstraint2.Target != null && vertexConstraint.Target != null && vertexConstraint.Target == vertexConstraint2.Target && constraints.GetEdgeConstraint(eid).Target == vertexConstraint.Target)
		{
			return true;
		}
		return false;
	}

	protected bool vertex_is_fixed(int vid)
	{
		if (constraints != null && constraints.GetVertexConstraint(vid).Fixed)
		{
			return true;
		}
		return false;
	}

	protected bool vertex_is_constrained(int vid)
	{
		if (constraints != null)
		{
			VertexConstraint vertexConstraint = constraints.GetVertexConstraint(vid);
			if (vertexConstraint.Fixed || vertexConstraint.Target != null)
			{
				return true;
			}
		}
		return false;
	}

	protected VertexConstraint get_vertex_constraint(int vid)
	{
		if (constraints != null)
		{
			return constraints.GetVertexConstraint(vid);
		}
		return VertexConstraint.Unconstrained;
	}

	protected bool get_vertex_constraint(int vid, ref VertexConstraint vc)
	{
		if (constraints != null)
		{
			return constraints.GetVertexConstraint(vid, ref vc);
		}
		return false;
	}
}
