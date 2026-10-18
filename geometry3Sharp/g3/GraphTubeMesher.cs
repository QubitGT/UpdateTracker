using System.Collections.Generic;

namespace g3;

public class GraphTubeMesher
{
	public DGraph3 Graph;

	public HashSet<int> TipVertices;

	public HashSet<int> GroundVertices;

	public double PostRadius = 1.25;

	public double TipRadius = 0.5;

	public double GroundRadius = 3.25;

	public double SamplerCellSizeHint;

	public double ActualCellSize;

	public ProgressCancel Progress;

	public DMesh3 ResultMesh;

	protected virtual bool Cancelled()
	{
		if (Progress != null)
		{
			return Progress.Cancelled();
		}
		return false;
	}

	public GraphTubeMesher(DGraph3 graph)
	{
		Graph = graph;
	}

	public GraphTubeMesher(GraphSupportGenerator support_gen)
	{
		Graph = support_gen.Graph;
		TipVertices = support_gen.TipVertices;
		GroundVertices = support_gen.GroundVertices;
		SamplerCellSizeHint = support_gen.CellSize;
	}

	public virtual void Generate()
	{
		AxisAlignedBox3d cachedBounds = Graph.CachedBounds;
		cachedBounds.Expand(2.0 * PostRadius);
		double num = ((SamplerCellSizeHint == 0.0) ? (PostRadius / 5.0) : SamplerCellSizeHint);
		ImplicitFieldSampler3d implicitFieldSampler3d = new ImplicitFieldSampler3d(cachedBounds, num);
		ActualCellSize = num;
		ImplicitLine3d implicitLine3d = new ImplicitLine3d
		{
			Radius = PostRadius
		};
		foreach (int item2 in Graph.EdgeIndices())
		{
			Index2i edgeV = Graph.GetEdgeV(item2);
			Vector3d vertex = Graph.GetVertex(edgeV.a);
			Vector3d vertex2 = Graph.GetVertex(edgeV.b);
			double radius = PostRadius;
			int item = ((vertex.y > vertex2.y) ? edgeV.a : edgeV.b);
			if (TipVertices.Contains(item))
			{
				radius = TipRadius;
			}
			implicitLine3d.Segment = new Segment3d(vertex, vertex2);
			implicitLine3d.Radius = radius;
			implicitFieldSampler3d.Sample(implicitLine3d, implicitLine3d.Radius / 2.0);
		}
		foreach (int groundVertex in GroundVertices)
		{
			Vector3d vertex3 = Graph.GetVertex(groundVertex);
			implicitFieldSampler3d.Sample(new ImplicitSphere3d
			{
				Origin = vertex3 - PostRadius / 2.0 * Vector3d.AxisY,
				Radius = GroundRadius
			});
		}
		ImplicitHalfSpace3d b = new ImplicitHalfSpace3d
		{
			Origin = Vector3d.Zero,
			Normal = Vector3d.AxisY
		};
		ImplicitDifference3d @implicit = new ImplicitDifference3d
		{
			A = implicitFieldSampler3d.ToImplicit(),
			B = b
		};
		MarchingCubes marchingCubes = new MarchingCubes
		{
			Implicit = @implicit,
			Bounds = cachedBounds,
			CubeSize = PostRadius / 3.0
		};
		marchingCubes.Bounds.Min.y = -2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Min.x -= 2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Min.z -= 2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Max.x += 2.0 * marchingCubes.CubeSize;
		marchingCubes.Bounds.Max.z += 2.0 * marchingCubes.CubeSize;
		marchingCubes.CancelF = Cancelled;
		marchingCubes.Generate();
		ResultMesh = marchingCubes.Mesh;
	}
}
