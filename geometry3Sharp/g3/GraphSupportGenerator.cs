using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class GraphSupportGenerator
{
	public class ImplicitCurve3d : BoundedImplicitFunction3d, ImplicitFunction3d
	{
		public DCurve3 Curve;

		public double Radius;

		public AxisAlignedBox3d Box;

		private DCurve3BoxTree spatial;

		public ImplicitCurve3d(DCurve3 curve, double radius)
		{
			Curve = curve;
			Radius = radius;
			Box = curve.GetBoundingBox();
			Box.Expand(Radius);
			spatial = new DCurve3BoxTree(curve);
		}

		public double Value(ref Vector3d pt)
		{
			return spatial.Distance(pt) - Radius;
		}

		public AxisAlignedBox3d Bounds()
		{
			return Box;
		}
	}

	public DMesh3 Mesh;

	public DMeshAABBTree3 MeshSpatial;

	public double CellSize;

	public double OverhangAngleDeg = 30.0;

	public float ForceMinY = float.MaxValue;

	public bool ProcessBottomUp;

	public double GraphSurfaceDistanceOffset = 1.5;

	public double OverhangAngleOptimizeDeg = 25.0;

	public double OptimizationAlpha = 1.0;

	public int OptimizationRounds = 20;

	public ProgressCancel Progress;

	public bool DebugPrint;

	private Vector3f grid_origin;

	private DenseGrid3f volume_grid;

	public DGraph3 Graph;

	public HashSet<int> TipVertices;

	public HashSet<int> TipBaseVertices;

	public HashSet<int> GroundVertices;

	private const float SUPPORT_GRID_USED = -1f;

	private const float SUPPORT_TIP_TOP = -2f;

	private const float SUPPORT_TIP_BASE = -3f;

	public Vector3i Dimensions => new Vector3i(volume_grid.ni, volume_grid.nj, volume_grid.nk);

	public DenseGrid3f Grid => volume_grid;

	public Vector3f GridOrigin => grid_origin;

	public float this[int i, int j, int k] => volume_grid[i, j, k];

	protected virtual bool Cancelled()
	{
		if (Progress != null)
		{
			return Progress.Cancelled();
		}
		return false;
	}

	public GraphSupportGenerator(DMesh3 mesh, DMeshAABBTree3 spatial, double cellSize)
	{
		Mesh = mesh;
		MeshSpatial = spatial;
		CellSize = cellSize;
	}

	public GraphSupportGenerator(DMesh3 mesh, DMeshAABBTree3 spatial, int grid_resolution)
	{
		Mesh = mesh;
		MeshSpatial = spatial;
		double num = Math.Max(Mesh.CachedBounds.Width, Mesh.CachedBounds.Height);
		CellSize = num / (double)grid_resolution;
	}

	public void Generate()
	{
		AxisAlignedBox3d cachedBounds = Mesh.CachedBounds;
		if (ForceMinY != float.MaxValue)
		{
			cachedBounds.Min.y = ForceMinY;
		}
		float num = 2f * (float)CellSize;
		Vector3f vector3f = new Vector3f(num, 0f, num);
		grid_origin = (Vector3f)cachedBounds.Min - vector3f;
		grid_origin.y += (float)CellSize * 0.5f;
		Vector3f vector3f2 = (Vector3f)cachedBounds.Max + vector3f;
		int ni = (int)((vector3f2.x - grid_origin.x) / (float)CellSize) + 1;
		int nj = (int)((vector3f2.y - grid_origin.y) / (float)CellSize) + 1;
		int nk = (int)((vector3f2.z - grid_origin.z) / (float)CellSize) + 1;
		volume_grid = new DenseGrid3f();
		generate_support(grid_origin, (float)CellSize, ni, nj, nk, volume_grid);
	}

	public Vector3f CellCenter(int i, int j, int k)
	{
		return new Vector3f((double)(float)i * CellSize + (double)grid_origin.x, (double)(float)j * CellSize + (double)grid_origin.y, (double)(float)k * CellSize + (double)grid_origin.z);
	}

	private void generate_support(Vector3f origin, float dx, int ni, int nj, int nk, DenseGrid3f supportGrid)
	{
		supportGrid.resize(ni, nj, nk);
		supportGrid.assign(1f);
		if (DebugPrint)
		{
			Console.WriteLine("start");
		}
		bool flag = false;
		Console.WriteLine("Computing SDF");
		MeshSignedDistanceGrid meshSignedDistanceGrid = new MeshSignedDistanceGrid(Mesh, CellSize)
		{
			ComputeSigns = true,
			ExactBandWidth = 3
		};
		meshSignedDistanceGrid.CancelF = Cancelled;
		meshSignedDistanceGrid.Compute();
		if (Cancelled())
		{
			return;
		}
		DenseGridTrilinearImplicit distanceField = new DenseGridTrilinearImplicit(meshSignedDistanceGrid.Grid, meshSignedDistanceGrid.GridOrigin, meshSignedDistanceGrid.CellSize);
		double num = Math.Cos(MathUtil.Clamp(OverhangAngleDeg, 0.01, 89.99) * (Math.PI / 180.0));
		Console.WriteLine("Marking overhangs");
		double num2 = dx;
		double num3 = origin[0];
		double num4 = origin[1];
		double num5 = origin[2];
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		foreach (int item in Mesh.TriangleIndices())
		{
			if (item % 100 == 0 && Cancelled())
			{
				break;
			}
			Mesh.GetTriVertices(item, ref v, ref v2, ref v3);
			if (MathUtil.Normal(ref v, ref v2, ref v3).Dot(-Vector3d.AxisY) < num)
			{
				continue;
			}
			double a = (v[0] - num3) / num2;
			double a2 = (v[1] - num4) / num2;
			double a3 = (v[2] - num5) / num2;
			double b = (v2[0] - num3) / num2;
			double b2 = (v2[1] - num4) / num2;
			double b3 = (v2[2] - num5) / num2;
			double c = (v3[0] - num3) / num2;
			double c2 = (v3[1] - num4) / num2;
			double c3 = (v3[2] - num5) / num2;
			int num6 = 0;
			int num7 = MathUtil.Clamp((int)MathUtil.Min(a, b, c) - num6, 0, ni - 1);
			int num8 = MathUtil.Clamp((int)MathUtil.Max(a, b, c) + num6 + 1, 0, ni - 1);
			int num9 = MathUtil.Clamp((int)MathUtil.Min(a2, b2, c2) - num6, 0, nj - 1);
			int num10 = MathUtil.Clamp((int)MathUtil.Max(a2, b2, c2) + num6 + 1, 0, nj - 1);
			int num11 = MathUtil.Clamp((int)MathUtil.Min(a3, b3, c3) - num6, 0, nk - 1);
			int num12 = MathUtil.Clamp((int)MathUtil.Max(a3, b3, c3) + num6 + 1, 0, nk - 1);
			if (num9 == 0)
			{
				num9 = 1;
			}
			for (int i = num11; i <= num12; i++)
			{
				for (int j = num9; j <= num10; j++)
				{
					for (int k = num7; k <= num8; k++)
					{
						Vector3d x = new Vector3d((float)k * dx + origin[0], (float)j * dx + origin[1], (float)i * dx + origin[2]);
						float num13 = (float)MeshSignedDistanceGrid.point_triangle_distance(ref x, ref v, ref v2, ref v3);
						if (flag)
						{
							int num14 = ((i % 2 == 0) ? 1 : 0);
							if (k % 2 == num14)
							{
								continue;
							}
						}
						if (num13 < dx / 2f)
						{
							if (j > 1)
							{
								supportGrid[k, j, i] = -2f;
								supportGrid[k, j - 1, i] = -3f;
							}
							else
							{
								supportGrid[k, j, i] = -3f;
							}
						}
					}
				}
			}
		}
		if (!Cancelled())
		{
			generate_graph(supportGrid, distanceField);
			postprocess_graph();
		}
	}

	private Vector3d get_cell_center(Vector3i ijk)
	{
		return new Vector3d((double)ijk.x * CellSize, (double)ijk.y * CellSize, (double)ijk.z * CellSize) + GridOrigin;
	}

	private void generate_graph(DenseGrid3f supportGrid, DenseGridTrilinearImplicit distanceField)
	{
		int ni = supportGrid.ni;
		int nj = supportGrid.nj;
		int nk = supportGrid.nk;
		float num = (float)CellSize;
		Vector3f gridOrigin = GridOrigin;
		float num2 = 0.01f;
		float num3 = 99999f;
		bool flag = true;
		float num4 = 10f * (float)CellSize;
		Vector3i center_idx = new Vector3i(ni / 2, 0, nk / 2);
		bool flag2 = true;
		DenseGrid3f costGrid = new DenseGrid3f(supportGrid);
		foreach (Vector3i item in costGrid.Indices())
		{
			Vector3d pt = new Vector3f((float)item.x * num, (float)item.y * num, (float)item.z * num) + gridOrigin;
			float num5 = (float)distanceField.Value(ref pt);
			if (num5 <= num2)
			{
				num5 = num3;
			}
			else if (flag)
			{
				num5 = 1f;
			}
			else if (num5 > num4)
			{
				num5 = num4;
			}
			costGrid[item] = num5;
		}
		List<Vector3i> list = new List<Vector3i>();
		List<Vector3i> list2 = new List<Vector3i>();
		for (int i = 0; i < nj; i++)
		{
			list2.Clear();
			for (int j = 0; j < nk; j++)
			{
				for (int k = 0; k < ni; k++)
				{
					if (supportGrid[k, i, j] == -3f)
					{
						list2.Add(new Vector3i(k, i, j));
					}
				}
			}
			list2.Sort(delegate(Vector3i a, Vector3i b)
			{
				Vector3i vector3i9 = a;
				vector3i9.y = 0;
				Vector3i vector3i10 = b;
				vector3i10.y = 0;
				int lengthSquared = (vector3i9 - center_idx).LengthSquared;
				int lengthSquared2 = (vector3i10 - center_idx).LengthSquared;
				return lengthSquared.CompareTo(lengthSquared2);
			});
			if (flag2)
			{
				list2.Reverse();
			}
			list.AddRange(list2);
		}
		HashSet<Vector3i> seed_indices = new HashSet<Vector3i>(list);
		if (!ProcessBottomUp)
		{
			list.Reverse();
		}
		Func<int, bool> nodeFilterF = delegate(int a)
		{
			Vector3i vector3i8 = costGrid.to_index(a);
			return vector3i8.x > 0 && vector3i8.z > 0 && vector3i8.x != ni - 1 && vector3i8.y != nj - 1 && vector3i8.z != nk - 1;
		};
		Func<int, int, float> nodeDistanceF = delegate(int a, int b)
		{
			Vector3i vector3i6 = costGrid.to_index(a);
			Vector3i vector3i7 = costGrid.to_index(b);
			if (vector3i7.y >= vector3i6.y)
			{
				return float.MaxValue;
			}
			float num9 = supportGrid[vector3i7];
			if (num9 == -2f)
			{
				return float.MaxValue;
			}
			if (num9 < 0f)
			{
				return -999999f;
			}
			float num10 = costGrid[b];
			float num11 = (float)(Math.Sqrt((vector3i7 - vector3i6).LengthSquared) * CellSize);
			return num10 + num11;
		};
		Func<int, IEnumerable<int>> neighboursF = delegate(int a)
		{
			Vector3i idx = costGrid.to_index(a);
			return down_neighbours(idx, costGrid);
		};
		Func<int, bool> terminatingNodeF = delegate(int a)
		{
			Vector3i vector3i5 = costGrid.to_index(a);
			if (!seed_indices.Contains(vector3i5) && supportGrid[vector3i5] < 0f)
			{
				return true;
			}
			return vector3i5.y == 0;
		};
		DijkstraGraphDistance dijkstraGraphDistance = new DijkstraGraphDistance(ni * nj * nk, bSparse: false, nodeFilterF, nodeDistanceF, neighboursF);
		dijkstraGraphDistance.TrackOrder = true;
		List<int> list3 = new List<int>();
		Graph = new DGraph3();
		Dictionary<Vector3i, int> dictionary = new Dictionary<Vector3i, int>();
		TipVertices = new HashSet<int>();
		TipBaseVertices = new HashSet<int>();
		GroundVertices = new HashSet<int>();
		for (int l = 0; l < list.Count; l++)
		{
			int id = costGrid.to_linear(list[l]);
			dijkstraGraphDistance.Reset();
			dijkstraGraphDistance.AddSeed(id, 0f);
			int num6 = dijkstraGraphDistance.ComputeToNode(terminatingNodeF);
			if (num6 < 0)
			{
				num6 = dijkstraGraphDistance.GetOrder().Last();
			}
			list3.Clear();
			dijkstraGraphDistance.GetPathToSeed(num6, list3);
			int count = list3.Count;
			Vector3i vector3i = supportGrid.to_index(list3[0]);
			if (!dictionary.TryGetValue(vector3i, out var value))
			{
				Vector3d v = get_cell_center(vector3i);
				if (vector3i.y == 0)
				{
					v.y = 0.0;
				}
				value = Graph.AppendVertex(v);
				if (vector3i.y == 0)
				{
					GroundVertices.Add(value);
				}
				dictionary[vector3i] = value;
			}
			int v2 = value;
			for (int m = 0; m < count; m++)
			{
				int i2 = list3[m];
				if (supportGrid[i2] >= 0f)
				{
					supportGrid[i2] = -1f;
				}
				if (m > 0)
				{
					Vector3i vector3i2 = supportGrid.to_index(list3[m]);
					if (!dictionary.TryGetValue(vector3i2, out var value2))
					{
						Vector3d v3 = get_cell_center(vector3i2);
						value2 = (dictionary[vector3i2] = Graph.AppendVertex(v3));
					}
					Graph.AppendEdge(v2, value2);
					v2 = value2;
				}
			}
			if (supportGrid[list3[count - 1]] == -3f)
			{
				Vector3i vector3i3 = supportGrid.to_index(list3[count - 1]);
				TipBaseVertices.Add(dictionary[vector3i3]);
				Vector3i vector3i4 = vector3i3 + Vector3i.AxisY;
				if (!dictionary.TryGetValue(vector3i4, out var value3))
				{
					Vector3d v4 = get_cell_center(vector3i4);
					value3 = (dictionary[vector3i4] = Graph.AppendVertex(v4));
					Graph.AppendEdge(v2, value3);
					TipVertices.Add(value3);
				}
			}
		}
		gParallel.ForEach(TipVertices, delegate(int tip_vid)
		{
			bool flag3 = false;
			Vector3d vector3d = Graph.GetVertex(tip_vid);
			if (MeshQueries.RayHitPointFrame(Mesh, MeshSpatial, new Ray3d(vector3d, Vector3d.AxisY), out var hitPosFrame) && vector3d.Distance(hitPosFrame.Origin) < 2.0 * CellSize)
			{
				vector3d = hitPosFrame.Origin;
				flag3 = true;
			}
			if (!flag3 && MeshQueries.RayHitPointFrame(Mesh, MeshSpatial, new Ray3d(vector3d, -Vector3d.AxisY), out hitPosFrame) && vector3d.Distance(hitPosFrame.Origin) < CellSize)
			{
				vector3d = hitPosFrame.Origin;
				flag3 = true;
			}
			if (!flag3)
			{
				hitPosFrame = MeshQueries.NearestPointFrame(Mesh, MeshSpatial, vector3d);
				if (vector3d.Distance(hitPosFrame.Origin) < 2.0 * CellSize)
				{
					vector3d = hitPosFrame.Origin;
					flag3 = true;
				}
			}
			if (flag3)
			{
				Graph.SetVertex(tip_vid, vector3d);
			}
		});
	}

	protected DMesh3 MakeDebugGraphMesh()
	{
		DMesh3 dMesh = new DMesh3();
		dMesh.EnableVertexColors(Vector3f.One);
		foreach (int item in Graph.VertexIndices())
		{
			if (TipVertices.Contains(item))
			{
				MeshEditor.AppendBox(dMesh, Graph.GetVertex(item), 0.3f, Colorf.Green);
			}
			else if (TipBaseVertices.Contains(item))
			{
				MeshEditor.AppendBox(dMesh, Graph.GetVertex(item), 0.225f, Colorf.Magenta);
			}
			else if (GroundVertices.Contains(item))
			{
				MeshEditor.AppendBox(dMesh, Graph.GetVertex(item), 0.35f, Colorf.Blue);
			}
			else
			{
				MeshEditor.AppendBox(dMesh, Graph.GetVertex(item), 0.15f, Colorf.White);
			}
		}
		foreach (int item2 in Graph.EdgeIndices())
		{
			Segment3d edgeSegment = Graph.GetEdgeSegment(item2);
			MeshEditor.AppendLine(dMesh, edgeSegment, 0.1f);
		}
		return dMesh;
	}

	protected virtual void postprocess_graph()
	{
		double num = MathUtil.Clamp(OptimizationAlpha, 0.0, 1.0);
		if (num != 0.0 && OptimizationRounds != 0)
		{
			constrained_smooth(Graph, GraphSurfaceDistanceOffset, Math.Cos((90.0 - OverhangAngleOptimizeDeg) * (Math.PI / 180.0)), num, OptimizationRounds);
		}
	}

	private IEnumerable<int> down_neighbours(Vector3i idx, DenseGrid3f grid)
	{
		yield return grid.to_linear(idx.x, idx.y - 1, idx.z);
		yield return grid.to_linear(idx.x - 1, idx.y - 1, idx.z);
		yield return grid.to_linear(idx.x + 1, idx.y - 1, idx.z);
		yield return grid.to_linear(idx.x, idx.y - 1, idx.z - 1);
		yield return grid.to_linear(idx.x, idx.y - 1, idx.z + 1);
		yield return grid.to_linear(idx.x - 1, idx.y - 1, idx.z - 1);
		yield return grid.to_linear(idx.x + 1, idx.y - 1, idx.z - 1);
		yield return grid.to_linear(idx.x - 1, idx.y - 1, idx.z + 1);
		yield return grid.to_linear(idx.x + 1, idx.y - 1, idx.z + 1);
	}

	private void constrained_smooth(DGraph3 graph, double surfDist, double dotThresh, double alpha, int rounds)
	{
		int maxVertexID = graph.MaxVertexID;
		Vector3d[] pos = new Vector3d[maxVertexID];
		for (int i = 0; i < rounds; i++)
		{
			gParallel.ForEach(graph.VertexIndices(), delegate(int vid)
			{
				Vector3d vertex = graph.GetVertex(vid);
				if (GroundVertices.Contains(vid) || TipVertices.Contains(vid))
				{
					pos[vid] = vertex;
				}
				else if (TipBaseVertices.Contains(vid))
				{
					pos[vid] = vertex;
				}
				else
				{
					Vector3d zero = Vector3d.Zero;
					int num = 0;
					foreach (int item in graph.VtxVerticesItr(vid))
					{
						zero += graph.GetVertex(item);
						num++;
					}
					if (num == 1)
					{
						pos[vid] = vertex;
					}
					else
					{
						zero /= (double)num;
						Vector3d vector3d = (1.0 - alpha) * vertex + alpha * zero;
						int num2 = 0;
						while (true)
						{
							IL_00fd:
							foreach (int item2 in graph.VtxVerticesItr(vid))
							{
								Vector3d vector3d2 = graph.GetVertex(item2) - vector3d;
								vector3d2.Normalize();
								if (Math.Abs(vector3d2.Dot(Vector3d.AxisY)) < dotThresh)
								{
									if (num2++ >= 3)
									{
										pos[vid] = vertex;
										return;
									}
									vector3d = Vector3d.Lerp(vertex, vector3d, 0.66);
									goto IL_00fd;
								}
							}
							break;
						}
						Frame3f frame3f = MeshQueries.NearestPointFrame(Mesh, MeshSpatial, vector3d, bForceFaceNormal: true);
						Vector3d v = frame3f.Origin;
						double num3 = vector3d.Distance(v);
						if (MeshSpatial.IsInside(vector3d) || num3 < surfDist)
						{
							Vector3d vector3d3 = frame3f.Z;
							if (vector3d3.Dot(Vector3d.AxisY) < 0.0)
							{
								vector3d3.y = 0.0;
								vector3d3.Normalize();
							}
							vector3d = frame3f.Origin + surfDist * vector3d3;
						}
						pos[vid] = vector3d;
					}
				}
			});
			foreach (int item3 in graph.VertexIndices())
			{
				graph.SetVertex(item3, pos[item3]);
			}
		}
	}

	private void process_version2(DenseGrid3f supportGrid, DenseGridTrilinearImplicit distanceField)
	{
		int ni = supportGrid.ni;
		int nj = supportGrid.nj;
		int nk = supportGrid.nk;
		float num = (float)CellSize;
		Vector3f gridOrigin = GridOrigin;
		DenseGrid2f denseGrid2f = supportGrid.get_slice(nj - 1, 1);
		DenseGrid2f tmp = new DenseGrid2f(denseGrid2f);
		Bitmap3 bitmap = new Bitmap3(new Vector3i(ni, nj, nk));
		for (int num2 = nj - 2; num2 >= 0; num2--)
		{
			DenseGrid2i denseGrid2i = binarize(denseGrid2f);
			skeletonize(denseGrid2i, null, 2);
			if (num2 == 0)
			{
				dilate(denseGrid2i, null);
				dilate(denseGrid2i, null);
			}
			for (int i = 1; i < nk - 1; i++)
			{
				for (int j = 1; j < ni - 1; j++)
				{
					bitmap[new Vector3i(j, num2, i)] = denseGrid2i[j, i] == 1;
				}
			}
			smooth(denseGrid2f, tmp, 0.5f, 5);
			DenseGrid2f denseGrid2f2 = supportGrid.get_slice(num2, 1);
			denseGrid2f2.set_min(denseGrid2f);
			for (int k = 1; k < nk - 1; k++)
			{
				for (int l = 1; l < ni - 1; l++)
				{
					float val = ((denseGrid2i[l, k] > 0) ? (-1f) : 2.1474836E+09f);
					denseGrid2f2[l, k] = Math.Min(denseGrid2f2[l, k], val);
					if (denseGrid2f2[l, k] < 0f)
					{
						Vector3d pt = new Vector3f((float)l * num, (float)num2 * num, (float)k * num) + gridOrigin;
						if (distanceField.Value(ref pt) < 0.0 - CellSize)
						{
							denseGrid2f2[l, k] = 1f;
						}
					}
				}
			}
			for (int m = 1; m < nk - 1; m++)
			{
				for (int n = 1; n < ni - 1; n++)
				{
					if (is_loner(denseGrid2i, n, m))
					{
						Vector2i[] gridOffsets = gIndices.GridOffsets8;
						for (int num3 = 0; num3 < gridOffsets.Length; num3++)
						{
							Vector2i vector2i = gridOffsets[num3];
							float num4 = 1f / (float)Math.Sqrt(vector2i.x * vector2i.x + vector2i.y * vector2i.y);
							denseGrid2f2[n + vector2i.x, m + vector2i.y] += -0.25f * num4;
						}
					}
				}
			}
			for (int num5 = 1; num5 < nk - 1; num5++)
			{
				for (int num6 = 1; num6 < ni - 1; num6++)
				{
					supportGrid[num6, num2, num5] = denseGrid2f2[num6, num5];
				}
			}
			denseGrid2f.swap(denseGrid2f2);
		}
		VoxelSurfaceGenerator voxelSurfaceGenerator = new VoxelSurfaceGenerator();
		voxelSurfaceGenerator.Voxels = bitmap;
		voxelSurfaceGenerator.Generate();
		Util.WriteDebugMesh(voxelSurfaceGenerator.Meshes[0], "c:\\scratch\\binary.obj");
	}

	private static DenseGrid2i binarize(DenseGrid2f grid, float thresh = 0f)
	{
		DenseGrid2i denseGrid2i = new DenseGrid2i();
		denseGrid2i.resize(grid.ni, grid.nj);
		int size = denseGrid2i.size;
		for (int i = 0; i < size; i++)
		{
			denseGrid2i[i] = ((grid[i] < thresh) ? 1 : 0);
		}
		return denseGrid2i;
	}

	private static DenseGrid3i binarize(DenseGrid3f grid, float thresh = 0f)
	{
		DenseGrid3i denseGrid3i = new DenseGrid3i();
		denseGrid3i.resize(grid.ni, grid.nj, grid.nk);
		int size = denseGrid3i.size;
		for (int i = 0; i < size; i++)
		{
			denseGrid3i[i] = ((grid[i] < thresh) ? 1 : 0);
		}
		return denseGrid3i;
	}

	private static void smooth(DenseGrid2f grid, DenseGrid2f tmp, float alpha, int rounds)
	{
		if (tmp == null)
		{
			tmp = new DenseGrid2f(grid.ni, grid.nj, 0f);
		}
		int ni = grid.ni;
		int nj = grid.nj;
		for (int i = 0; i < rounds; i++)
		{
			tmp.assign_border(1f, 1);
			for (int j = 1; j < nj - 1; j++)
			{
				for (int k = 1; k < ni - 1; k++)
				{
					float num = grid[k - 1, j] + grid[k - 1, j + 1] + grid[k, j + 1] + grid[k + 1, j + 1] + grid[k + 1, j] + grid[k + 1, j - 1] + grid[k, j - 1] + grid[k - 1, j - 1];
					num /= 8f;
					tmp[k, j] = (1f - alpha) * grid[k, j] + alpha * num;
				}
			}
			grid.copy(tmp);
		}
	}

	private void process_version1(DenseGrid3f supportGrid, DenseGridTrilinearImplicit distanceField)
	{
		int ni = supportGrid.ni;
		int nj = supportGrid.nj;
		int nk = supportGrid.nk;
		float num = (float)CellSize;
		Vector3f gridOrigin = GridOrigin;
		for (int i = 0; i < nk; i++)
		{
			for (int j = 0; j < ni; j++)
			{
				bool flag = false;
				for (int num2 = nj - 1; num2 >= 0; num2--)
				{
					if (supportGrid[j, num2, i] >= 0f)
					{
						Vector3d pt = new Vector3f((float)j * num, (float)num2 * num, (float)i * num) + gridOrigin;
						if (flag)
						{
							if (distanceField.Value(ref pt) < 0.0)
							{
								supportGrid[j, num2, i] = -3f;
								flag = false;
							}
							else
							{
								supportGrid[j, num2, i] = -1f;
							}
						}
					}
					else
					{
						flag = true;
					}
				}
			}
		}
		DenseGrid3i denseGrid3i = new DenseGrid3i(ni, nj, nk, 0);
		foreach (Vector3i item in denseGrid3i.Indices())
		{
			denseGrid3i[item] = ((supportGrid[item] < 0f) ? 1 : 0);
		}
		for (int k = 0; k < nj; k++)
		{
			skeletonize_layer(denseGrid3i, k);
		}
		for (int l = 0; l < nj; l++)
		{
			for (int m = 1; m < nk - 1; m++)
			{
				for (int n = 1; n < ni - 1; n++)
				{
					if (denseGrid3i[n, l, m] > 0)
					{
						supportGrid[n, l, m] = -3f;
					}
				}
			}
		}
		for (int num3 = 0; num3 < nk; num3++)
		{
			for (int num4 = 0; num4 < ni; num4++)
			{
				if (supportGrid[num4, 0, num3] < 0f)
				{
					supportGrid[num4, 0, num3] = -5f;
				}
			}
		}
		DenseGrid3f denseGrid3f = new DenseGrid3f(supportGrid);
		float num5 = 0.5f;
		for (int num6 = 0; num6 < 15; num6++)
		{
			for (int num7 = 0; num7 < nj; num7++)
			{
				for (int num8 = 1; num8 < nk - 1; num8++)
				{
					for (int num9 = 1; num9 < ni - 1; num9++)
					{
						if (denseGrid3i[num9, num7, num8] > 0)
						{
							supportGrid[num9, num7, num8] -= num5 / 25f;
						}
					}
				}
			}
			for (int num10 = 0; num10 < nj; num10++)
			{
				for (int num11 = 1; num11 < nk - 1; num11++)
				{
					for (int num12 = 1; num12 < ni - 1; num12++)
					{
						int num13 = 0;
						float num14 = 0f;
						float num15 = 0f;
						for (int num16 = 0; num16 < 8; num16++)
						{
							int i2 = num12 + gIndices.GridOffsets8[num16].x;
							int k2 = num11 + gIndices.GridOffsets8[num16].y;
							float num17 = supportGrid[i2, num10, k2];
							if (num17 < 0f)
							{
								num13++;
							}
							num14 += num5 * num17;
							num15 += num5;
						}
						if (num13 > -1)
						{
							num14 += supportGrid[num12, num10, num11];
							num15 += 1f;
							denseGrid3f[num12, num10, num11] = num14 / num15;
						}
						else
						{
							denseGrid3f[num12, num10, num11] = supportGrid[num12, num10, num11];
						}
					}
				}
			}
			supportGrid.swap(denseGrid3f);
		}
	}

	private static void skeletonize_pass(DenseGrid2i grid, DenseGrid2i tmp, int iter)
	{
		int ni = grid.ni;
		int nj = grid.nj;
		for (int i = 1; i < ni - 1; i++)
		{
			for (int j = 1; j < nj - 1; j++)
			{
				int num = grid[i - 1, j];
				int num2 = grid[i - 1, j + 1];
				int num3 = grid[i, j + 1];
				int num4 = grid[i + 1, j + 1];
				int num5 = grid[i + 1, j];
				int num6 = grid[i + 1, j - 1];
				int num7 = grid[i, j - 1];
				int num8 = grid[i - 1, j - 1];
				int num9 = ((num == 0 && num2 == 1) ? 1 : 0) + ((num2 == 0 && num3 == 1) ? 1 : 0) + ((num3 == 0 && num4 == 1) ? 1 : 0) + ((num4 == 0 && num5 == 1) ? 1 : 0) + ((num5 == 0 && num6 == 1) ? 1 : 0) + ((num6 == 0 && num7 == 1) ? 1 : 0) + ((num7 == 0 && num8 == 1) ? 1 : 0) + ((num8 == 0 && num == 1) ? 1 : 0);
				int num10 = num + num2 + num3 + num4 + num5 + num6 + num7 + num8;
				int num11 = ((iter == 0) ? (num * num3 * num5) : (num * num3 * num7));
				int num12 = ((iter == 0) ? (num3 * num5 * num7) : (num * num5 * num7));
				if (num9 == 1 && num10 >= 2 && num10 <= 6 && num11 == 0 && num12 == 0)
				{
					tmp[i, j] = 1;
				}
			}
		}
		for (int k = 0; k < ni; k++)
		{
			for (int l = 0; l < nj; l++)
			{
				grid[k, l] &= ~tmp[k, l];
			}
		}
	}

	private static void dilate(DenseGrid2i grid, DenseGrid2i tmp, bool corners = true)
	{
		if (tmp == null)
		{
			tmp = new DenseGrid2i(grid.ni, grid.nj, 0);
		}
		int ni = grid.ni;
		int nj = grid.nj;
		for (int i = 1; i < ni - 1; i++)
		{
			for (int j = 1; j < nj - 1; j++)
			{
				if (grid[i, j] == 1)
				{
					tmp[i, j] = 1;
					tmp[i - 1, j] = 1;
					tmp[i, j + 1] = 1;
					tmp[i + 1, j] = 1;
					tmp[i, j - 1] = 1;
					if (corners)
					{
						tmp[i - 1, j + 1] = 1;
						tmp[i + 1, j + 1] = 1;
						tmp[i + 1, j - 1] = 1;
						tmp[i - 1, j - 1] = 1;
					}
				}
			}
		}
		grid.copy(tmp);
	}

	private static void dilate_loners(DenseGrid2i grid, DenseGrid2i tmp, int mode)
	{
		if (tmp == null)
		{
			tmp = new DenseGrid2i(grid.ni, grid.nj, 0);
		}
		int ni = grid.ni;
		int nj = grid.nj;
		for (int i = 1; i < ni - 1; i++)
		{
			for (int j = 1; j < nj - 1; j++)
			{
				if (grid[i, j] != 1)
				{
					continue;
				}
				tmp[i, j] = 1;
				if (grid[i - 1, j] + grid[i - 1, j + 1] + grid[i, j + 1] + grid[i + 1, j + 1] + grid[i + 1, j] + grid[i + 1, j - 1] + grid[i, j - 1] + grid[i - 1, j - 1] == 0)
				{
					if (mode != 3)
					{
						tmp[i - 1, j] = 1;
						tmp[i + 1, j] = 1;
						tmp[i, j + 1] = 1;
						tmp[i, j - 1] = 1;
					}
					if (mode == 2 || mode == 3)
					{
						tmp[i - 1, j + 1] = 1;
						tmp[i + 1, j + 1] = 1;
						tmp[i + 1, j - 1] = 1;
						tmp[i - 1, j - 1] = 1;
					}
				}
			}
		}
		grid.copy(tmp);
	}

	private bool is_loner(DenseGrid2i grid, int i, int j)
	{
		if (grid[i, j] == 0)
		{
			return false;
		}
		return grid[i - 1, j] + grid[i - 1, j + 1] + grid[i, j + 1] + grid[i + 1, j + 1] + grid[i + 1, j] + grid[i + 1, j - 1] + grid[i, j - 1] + grid[i - 1, j - 1] == 0;
	}

	private static void skeletonize(DenseGrid2i grid, DenseGrid2i tmp, int dilation_rounds = 1)
	{
		if (tmp == null)
		{
			tmp = new DenseGrid2i(grid.ni, grid.nj, 0);
		}
		for (int i = 0; i < dilation_rounds; i++)
		{
			tmp.clear();
			dilate(grid, tmp);
		}
		bool flag = false;
		while (!flag)
		{
			int num = grid.sum();
			tmp.clear();
			skeletonize_pass(grid, tmp, 0);
			tmp.clear();
			skeletonize_pass(grid, tmp, 1);
			int num2 = grid.sum();
			if (num == num2)
			{
				break;
			}
		}
	}

	private static void diffuse(DenseGrid2f grid, float t, Func<int, int, bool> skipF)
	{
		int ni = grid.ni;
		int nj = grid.nj;
		DenseGrid2f denseGrid2f = new DenseGrid2f(grid);
		for (int i = 1; i < nj - 1; i++)
		{
			for (int j = 1; j < ni - 1; j++)
			{
				if ((skipF == null || !skipF(j, i)) && grid[j, i] < 0f)
				{
					Vector2i[] gridOffsets = gIndices.GridOffsets8;
					for (int k = 0; k < gridOffsets.Length; k++)
					{
						Vector2i vector2i = gridOffsets[k];
						float num = ((vector2i.LengthSquared > 1) ? (-1f) : (-0.707f));
						num *= t;
						denseGrid2f[j + vector2i.x, i + vector2i.y] = Math.Min(denseGrid2f[j + vector2i.x, i + vector2i.y], num);
					}
				}
			}
		}
		grid.swap(denseGrid2f);
	}

	private static void skeletonize_layer(DenseGrid3i grid, int j, int dilation_rounds = 1)
	{
		DenseGrid2i denseGrid2i = grid.get_slice(j, 1);
		DenseGrid2i denseGrid2i2 = new DenseGrid2i(denseGrid2i.ni, denseGrid2i.nj, 0);
		for (int i = 0; i < dilation_rounds; i++)
		{
			denseGrid2i2.assign(0);
			dilate(denseGrid2i, denseGrid2i2);
		}
		bool flag = false;
		while (!flag)
		{
			int num = denseGrid2i.sum();
			denseGrid2i2.assign(0);
			skeletonize_pass(denseGrid2i, denseGrid2i2, 0);
			denseGrid2i2.assign(0);
			skeletonize_pass(denseGrid2i, denseGrid2i2, 1);
			int num2 = denseGrid2i.sum();
			if (num == num2)
			{
				break;
			}
		}
		for (int k = 0; k < grid.ni; k++)
		{
			for (int l = 0; l < grid.nk; l++)
			{
				grid[k, j, l] = denseGrid2i[k, l];
			}
		}
	}

	private static void smooth(DenseGrid3f grid, DenseGrid3f tmp, float alpha, int iters, int min_j = 1)
	{
		if (tmp == null)
		{
			tmp = new DenseGrid3f(grid);
		}
		int ni = grid.ni;
		int nj = grid.nj;
		int nk = grid.nk;
		for (int i = 0; i < iters; i++)
		{
			for (int j = min_j; j < nj - 1; j++)
			{
				for (int k = 1; k < nk - 1; k++)
				{
					for (int l = 1; l < ni - 1; l++)
					{
						float num = 0f;
						Vector3i[] gridOffsets = gIndices.GridOffsets26;
						for (int m = 0; m < gridOffsets.Length; m++)
						{
							Vector3i vector3i = gridOffsets[m];
							int i2 = l + vector3i.x;
							int j2 = j + vector3i.y;
							int k2 = k + vector3i.z;
							float num2 = grid[i2, j2, k2];
							num += num2;
						}
						num /= 26f;
						tmp[l, j, k] = (1f - alpha) * grid[l, j, k] + alpha * num;
					}
				}
			}
			grid.swap(tmp);
		}
	}
}
