using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace g3;

public class STLReader : IMeshReader
{
	public enum Strategy
	{
		NoProcessing,
		IdenticalVertexWeld,
		TolerantVertexWeld,
		AutoBestResult
	}

	protected class STLSolid
	{
		public string Name;

		public DVectorArray3f Vertices = new DVectorArray3f();

		public DVector<short> TriAttribs;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct stl_triangle
	{
		public float nx;

		public float ny;

		public float nz;

		public float ax;

		public float ay;

		public float az;

		public float bx;

		public float by;

		public float bz;

		public float cx;

		public float cy;

		public float cz;

		public short attrib;
	}

	public Strategy RebuildStrategy = Strategy.AutoBestResult;

	public double WeldTolerance = 9.999999974752427E-07;

	public bool WantPerTriAttribs;

	public static string PerTriAttribMetadataName = "tri_attrib";

	private Dictionary<string, int> warningCount = new Dictionary<string, int>();

	public const string StrategyFlag = "-stl-weld-strategy";

	public const string PerTriAttribFlag = "-want-tri-attrib";

	private List<STLSolid> Objects;

	public event ParsingMessagesHandler warningEvent;

	private void ParseArguments(CommandArgumentSet args)
	{
		if (args.Integers.ContainsKey("-stl-weld-strategy"))
		{
			RebuildStrategy = (Strategy)args.Integers["-stl-weld-strategy"];
		}
		if (args.Flags.ContainsKey("-want-tri-attrib"))
		{
			WantPerTriAttribs = true;
		}
	}

	private void append_vertex(float x, float y, float z)
	{
		Objects.Last().Vertices.Append(x, y, z);
	}

	public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder)
	{
		if (options.CustomFlags != null)
		{
			ParseArguments(options.CustomFlags);
		}
		reader.ReadBytes(80);
		int num = reader.ReadInt32();
		Objects = new List<STLSolid>();
		Objects.Add(new STLSolid());
		int num2 = 50;
		IntPtr intPtr = Marshal.AllocHGlobal(num2);
		Type type = default(stl_triangle).GetType();
		DVector<short> dVector = new DVector<short>();
		try
		{
			for (int i = 0; i < num; i++)
			{
				byte[] array = reader.ReadBytes(50);
				if (array.Length >= 50)
				{
					Marshal.Copy(array, 0, intPtr, num2);
					stl_triangle stl_triangle = (stl_triangle)Marshal.PtrToStructure(intPtr, type);
					append_vertex(stl_triangle.ax, stl_triangle.ay, stl_triangle.az);
					append_vertex(stl_triangle.bx, stl_triangle.by, stl_triangle.bz);
					append_vertex(stl_triangle.cx, stl_triangle.cy, stl_triangle.cz);
					dVector.Add(stl_triangle.attrib);
					continue;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			return new IOReadResult(IOCode.GenericReaderError, "exception: " + ex.Message);
		}
		Marshal.FreeHGlobal(intPtr);
		if (Objects.Count == 1)
		{
			Objects[0].TriAttribs = dVector;
		}
		foreach (STLSolid @object in Objects)
		{
			BuildMesh(@object, builder);
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder)
	{
		if (options.CustomFlags != null)
		{
			ParseArguments(options.CustomFlags);
		}
		bool flag = false;
		Objects = new List<STLSolid>();
		int num = 0;
		while (reader.Peek() >= 0)
		{
			string text = reader.ReadLine();
			num++;
			string[] array = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0)
			{
				continue;
			}
			if (array[0].Equals("vertex", StringComparison.OrdinalIgnoreCase))
			{
				float x = ((array.Length > 1) ? float.Parse(array[1]) : 0f);
				float y = ((array.Length > 2) ? float.Parse(array[2]) : 0f);
				float z = ((array.Length > 3) ? float.Parse(array[3]) : 0f);
				append_vertex(x, y, z);
			}
			else if (array[0].Equals("facet", StringComparison.OrdinalIgnoreCase))
			{
				if (!flag)
				{
					Objects.Add(new STLSolid
					{
						Name = "unknown_solid"
					});
					flag = true;
				}
			}
			else if (array[0].Equals("solid", StringComparison.OrdinalIgnoreCase))
			{
				STLSolid sTLSolid = new STLSolid();
				if (array.Length == 2)
				{
					sTLSolid.Name = array[1];
				}
				else
				{
					sTLSolid.Name = "object_" + Objects.Count;
				}
				Objects.Add(sTLSolid);
				flag = true;
			}
			else if (array[0].Equals("endsolid", StringComparison.OrdinalIgnoreCase))
			{
				flag = false;
			}
		}
		foreach (STLSolid @object in Objects)
		{
			BuildMesh(@object, builder);
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	protected virtual void BuildMesh(STLSolid solid, IMeshBuilder builder)
	{
		if (RebuildStrategy == Strategy.AutoBestResult)
		{
			DMesh3 existingMesh = BuildMesh_Auto(solid);
			builder.AppendNewMesh(existingMesh);
		}
		else if (RebuildStrategy == Strategy.IdenticalVertexWeld)
		{
			DMesh3 existingMesh2 = BuildMesh_IdenticalWeld(solid);
			builder.AppendNewMesh(existingMesh2);
		}
		else if (RebuildStrategy == Strategy.TolerantVertexWeld)
		{
			DMesh3 existingMesh3 = BuildMesh_TolerantWeld(solid, WeldTolerance);
			builder.AppendNewMesh(existingMesh3);
		}
		else
		{
			BuildMesh_NoMerge(solid, builder);
		}
		if (WantPerTriAttribs && solid.TriAttribs != null && builder.SupportsMetaData)
		{
			builder.AppendMetaData(PerTriAttribMetadataName, solid.TriAttribs);
		}
	}

	protected virtual void BuildMesh_NoMerge(STLSolid solid, IMeshBuilder builder)
	{
		builder.AppendNewMesh(bHaveVtxNormals: false, bHaveVtxColors: false, bHaveVtxUVs: false, bHaveFaceGroups: false);
		DVectorArray3f vertices = solid.Vertices;
		int num = vertices.Count / 3;
		for (int i = 0; i < num; i++)
		{
			Vector3f vector3f = vertices[3 * i];
			int i2 = builder.AppendVertex(vector3f.x, vector3f.y, vector3f.z);
			Vector3f vector3f2 = vertices[3 * i + 1];
			int j = builder.AppendVertex(vector3f2.x, vector3f2.y, vector3f2.z);
			Vector3f vector3f3 = vertices[3 * i + 2];
			int k = builder.AppendVertex(vector3f3.x, vector3f3.y, vector3f3.z);
			builder.AppendTriangle(i2, j, k);
		}
	}

	protected virtual DMesh3 BuildMesh_Auto(STLSolid solid)
	{
		DMesh3 dMesh = BuildMesh_IdenticalWeld(solid);
		if (check_for_cracks(dMesh, out var boundary_edge_count, WeldTolerance))
		{
			DMesh3 dMesh2 = BuildMesh_TolerantWeld(solid, WeldTolerance);
			if (count_boundary_edges(dMesh2) < boundary_edge_count)
			{
				return dMesh2;
			}
			return dMesh;
		}
		return dMesh;
	}

	protected int count_boundary_edges(DMesh3 mesh)
	{
		int num = 0;
		foreach (int item in mesh.BoundaryEdgeIndices())
		{
			_ = item;
			num++;
		}
		return num;
	}

	protected bool check_for_cracks(DMesh3 mesh, out int boundary_edge_count, double crack_tol = 9.999999974752427E-07)
	{
		boundary_edge_count = 0;
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(mesh);
		foreach (int item in mesh.BoundaryEdgeIndices())
		{
			Index2i edgeV = mesh.GetEdgeV(item);
			meshVertexSelection.Select(edgeV.a);
			meshVertexSelection.Select(edgeV.b);
			boundary_edge_count++;
		}
		if (meshVertexSelection.Count == 0)
		{
			return false;
		}
		PointHashGrid3d<int> pointHashGrid3d = new PointHashGrid3d<int>(mesh.CachedBounds.MaxDim / 128.0, -1);
		foreach (int item2 in meshVertexSelection)
		{
			Vector3d v = mesh.GetVertex(item2);
			if (pointHashGrid3d.FindNearestInRadius(v, crack_tol, (int existing_vid) => v.Distance(mesh.GetVertex(existing_vid))).Key != -1)
			{
				return true;
			}
			pointHashGrid3d.InsertPoint(item2, v);
		}
		return false;
	}

	protected virtual DMesh3 BuildMesh_IdenticalWeld(STLSolid solid)
	{
		DMesh3Builder dMesh3Builder = new DMesh3Builder();
		dMesh3Builder.AppendNewMesh(bHaveVtxNormals: false, bHaveVtxColors: false, bHaveVtxUVs: false, bHaveFaceGroups: false);
		DVectorArray3f vertices = solid.Vertices;
		int count = vertices.Count;
		int[] array = new int[count];
		Dictionary<Vector3f, int> dictionary = new Dictionary<Vector3f, int>();
		for (int i = 0; i < count; i++)
		{
			Vector3f key = vertices[i];
			if (dictionary.TryGetValue(key, out var value))
			{
				array[i] = value;
				continue;
			}
			int num2 = (dictionary[key] = dMesh3Builder.AppendVertex(key.x, key.y, key.z));
			array[i] = num2;
		}
		append_mapped_triangles(solid, dMesh3Builder, array);
		return dMesh3Builder.Meshes[0];
	}

	protected virtual DMesh3 BuildMesh_TolerantWeld(STLSolid solid, double weld_tolerance)
	{
		DMesh3Builder dMesh3Builder = new DMesh3Builder();
		dMesh3Builder.AppendNewMesh(bHaveVtxNormals: false, bHaveVtxColors: false, bHaveVtxUVs: false, bHaveFaceGroups: false);
		DVectorArray3f vertices = solid.Vertices;
		int count = vertices.Count;
		int[] array = new int[count];
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		for (int i = 0; i < count; i++)
		{
			empty.Contain(vertices[i]);
		}
		int num = 256;
		if (count > 100000)
		{
			num = 512;
		}
		if (count > 1000000)
		{
			num = 1024;
		}
		if (count > 2000000)
		{
			num = 2048;
		}
		if (count > 5000000)
		{
			num = 4096;
		}
		PointHashGrid3d<int> pointHashGrid3d = new PointHashGrid3d<int>(empty.MaxDim / (double)(float)num, -1);
		Vector3f[] pos = new Vector3f[count];
		for (int j = 0; j < count; j++)
		{
			Vector3f v = vertices[j];
			KeyValuePair<int, double> keyValuePair = pointHashGrid3d.FindNearestInRadius(v, weld_tolerance, (int vid) => v.Distance(pos[vid]));
			if (keyValuePair.Key == -1)
			{
				int num2 = dMesh3Builder.AppendVertex(v.x, v.y, v.z);
				pointHashGrid3d.InsertPoint(num2, v);
				array[j] = num2;
				pos[num2] = v;
			}
			else
			{
				array[j] = keyValuePair.Key;
			}
		}
		append_mapped_triangles(solid, dMesh3Builder, array);
		return dMesh3Builder.Meshes[0];
	}

	private void append_mapped_triangles(STLSolid solid, DMesh3Builder builder, int[] mapV)
	{
		int num = solid.Vertices.Count / 3;
		for (int i = 0; i < num; i++)
		{
			int num2 = mapV[3 * i];
			int num3 = mapV[3 * i + 1];
			int num4 = mapV[3 * i + 2];
			if (num2 != num3 && num2 != num4 && num3 != num4)
			{
				builder.AppendTriangle(num2, num3, num4);
			}
		}
	}

	private void emit_warning(string sMessage)
	{
		string key = sMessage.Substring(0, 15);
		int num = (warningCount.ContainsKey(key) ? warningCount[key] : 0);
		num++;
		warningCount[key] = num;
		if (num <= 10)
		{
			if (num == 10)
			{
				sMessage += " (additional message surpressed)";
			}
			this.warningEvent?.Invoke(sMessage, null);
		}
	}
}
