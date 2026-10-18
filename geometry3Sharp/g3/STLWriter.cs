using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace g3;

public class STLWriter : IMeshWriter
{
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

	public IOWriteResult Write(BinaryWriter writer, List<WriteMesh> vMeshes, WriteOptions options)
	{
		string s = "g3sharp_stl ";
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		byte[] array = new byte[80];
		Array.Clear(array, 0, array.Length);
		Array.Copy(bytes, array, bytes.Length);
		writer.Write(array);
		int num = 0;
		foreach (WriteMesh vMesh in vMeshes)
		{
			num += vMesh.Mesh.TriangleCount;
		}
		writer.Write(num);
		for (int i = 0; i < vMeshes.Count; i++)
		{
			IMesh mesh = vMeshes[i].Mesh;
			if (options.ProgressFunc != null)
			{
				options.ProgressFunc(i, vMeshes.Count - 1);
			}
			Func<int, stl_triangle> producerF = delegate(int ti)
			{
				stl_triangle result = default(stl_triangle);
				Index3i triangle = mesh.GetTriangle(ti);
				Vector3d vertex = mesh.GetVertex(triangle.a);
				Vector3d vertex2 = mesh.GetVertex(triangle.b);
				Vector3d vertex3 = mesh.GetVertex(triangle.c);
				Vector3d vector3d = MathUtil.Normal(vertex, vertex2, vertex3);
				result.nx = (float)vector3d.x;
				result.ny = (float)vector3d.y;
				result.nz = (float)vector3d.z;
				result.ax = (float)vertex.x;
				result.ay = (float)vertex.y;
				result.az = (float)vertex.z;
				result.bx = (float)vertex2.x;
				result.by = (float)vertex2.y;
				result.bz = (float)vertex2.z;
				result.cx = (float)vertex3.x;
				result.cy = (float)vertex3.y;
				result.cz = (float)vertex3.z;
				result.attrib = 0;
				return result;
			};
			Action<stl_triangle> consumerF = delegate(stl_triangle tri)
			{
				byte[] buffer = Util.StructureToByteArray(tri);
				writer.Write(buffer);
			};
			ParallelStream<int, stl_triangle> parallelStream = new ParallelStream<int, stl_triangle>();
			parallelStream.ProducerF = producerF;
			parallelStream.ConsumerF = consumerF;
			parallelStream.Run(mesh.TriangleIndices());
		}
		return new IOWriteResult(IOCode.Ok, "");
	}

	public IOWriteResult Write(TextWriter writer, List<WriteMesh> vMeshes, WriteOptions options)
	{
		if (options.bCombineMeshes)
		{
			writer.WriteLine("solid \"mesh\"");
		}
		string text = Util.MakeVec3FormatString(0, 1, 2, options.RealPrecisionDigits);
		for (int i = 0; i < vMeshes.Count; i++)
		{
			IMesh mesh = vMeshes[i].Mesh;
			if (options.ProgressFunc != null)
			{
				options.ProgressFunc(i, vMeshes.Count - 1);
			}
			string arg = $"mesh_{i}";
			if (!options.bCombineMeshes)
			{
				if (vMeshes[i].Name != null && vMeshes[i].Name.Length > 0)
				{
					arg = vMeshes[i].Name;
				}
				writer.WriteLine("solid \"{0}\"", arg);
			}
			foreach (int item in mesh.TriangleIndices())
			{
				Index3i triangle = mesh.GetTriangle(item);
				Vector3d vertex = mesh.GetVertex(triangle.a);
				Vector3d vertex2 = mesh.GetVertex(triangle.b);
				Vector3d vertex3 = mesh.GetVertex(triangle.c);
				Vector3d vector3d = MathUtil.Normal(vertex, vertex2, vertex3);
				writer.WriteLine("facet normal " + text, vector3d.x, vector3d.y, vector3d.z);
				writer.WriteLine("outer loop" + writer.NewLine + "vertex " + text, vertex.x, vertex.y, vertex.z);
				writer.WriteLine("vertex " + text, vertex2.x, vertex2.y, vertex2.z);
				writer.WriteLine("vertex " + text, vertex3.x, vertex3.y, vertex3.z);
				writer.WriteLine("endloop" + writer.NewLine + "endfacet");
			}
			if (!options.bCombineMeshes)
			{
				writer.WriteLine("endsolid \"{0}\"", arg);
			}
		}
		if (options.bCombineMeshes)
		{
			writer.WriteLine("endsolid \"mesh\"");
		}
		return new IOWriteResult(IOCode.Ok, "");
	}
}
