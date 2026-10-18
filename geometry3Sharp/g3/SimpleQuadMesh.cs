using System.Collections.Generic;
using System.IO;

namespace g3;

public class SimpleQuadMesh
{
	public DVector<double> Vertices;

	public DVector<float> Normals;

	public DVector<float> Colors;

	public DVector<float> UVs;

	public DVector<int> Quads;

	public DVector<int> FaceGroups;

	public MeshComponents Components
	{
		get
		{
			MeshComponents meshComponents = MeshComponents.None;
			if (Normals != null)
			{
				meshComponents |= MeshComponents.VertexNormals;
			}
			if (Colors != null)
			{
				meshComponents |= MeshComponents.VertexColors;
			}
			if (UVs != null)
			{
				meshComponents |= MeshComponents.VertexUVs;
			}
			if (FaceGroups != null)
			{
				meshComponents |= MeshComponents.FaceGroups;
			}
			return meshComponents;
		}
	}

	public int VertexCount => Vertices.Length / 3;

	public int QuadCount => Quads.Length / 4;

	public int MaxVertexID => VertexCount;

	public int MaxQuadID => QuadCount;

	public bool HasVertexColors
	{
		get
		{
			if (Colors != null)
			{
				return Colors.Length == Vertices.Length;
			}
			return false;
		}
	}

	public bool HasVertexNormals
	{
		get
		{
			if (Normals != null)
			{
				return Normals.Length == Vertices.Length;
			}
			return false;
		}
	}

	public bool HasVertexUVs
	{
		get
		{
			if (UVs != null)
			{
				return UVs.Length / 2 == Vertices.Length / 3;
			}
			return false;
		}
	}

	public bool HasFaceGroups
	{
		get
		{
			if (FaceGroups != null)
			{
				return FaceGroups.Length == Quads.Length / 4;
			}
			return false;
		}
	}

	public SimpleQuadMesh()
	{
		Initialize();
	}

	public void Initialize(bool bWantNormals = true, bool bWantColors = true, bool bWantUVs = true, bool bWantFaceGroups = true)
	{
		Vertices = new DVector<double>();
		Normals = (bWantNormals ? new DVector<float>() : null);
		Colors = (bWantColors ? new DVector<float>() : null);
		UVs = (bWantUVs ? new DVector<float>() : null);
		Quads = new DVector<int>();
		FaceGroups = (bWantFaceGroups ? new DVector<int>() : null);
	}

	public int AppendVertex(double x, double y, double z)
	{
		int result = Vertices.Length / 3;
		if (HasVertexNormals)
		{
			Normals.Add(0f);
			Normals.Add(1f);
			Normals.Add(0f);
		}
		if (HasVertexColors)
		{
			Colors.Add(1f);
			Colors.Add(1f);
			Colors.Add(1f);
		}
		if (HasVertexUVs)
		{
			UVs.Add(0f);
			UVs.Add(0f);
		}
		Vertices.Add(x);
		Vertices.Add(y);
		Vertices.Add(z);
		return result;
	}

	public int AppendVertex(Vector3d v)
	{
		return AppendVertex(v.x, v.y, v.z);
	}

	public int AppendVertex(NewVertexInfo info)
	{
		int result = Vertices.Length / 3;
		if (info.bHaveN && HasVertexNormals)
		{
			Normals.Add(info.n[0]);
			Normals.Add(info.n[1]);
			Normals.Add(info.n[2]);
		}
		else if (HasVertexNormals)
		{
			Normals.Add(0f);
			Normals.Add(1f);
			Normals.Add(0f);
		}
		if (info.bHaveC && HasVertexColors)
		{
			Colors.Add(info.c[0]);
			Colors.Add(info.c[1]);
			Colors.Add(info.c[2]);
		}
		else if (HasVertexColors)
		{
			Colors.Add(1f);
			Colors.Add(1f);
			Colors.Add(1f);
		}
		if (info.bHaveUV && HasVertexUVs)
		{
			UVs.Add(info.uv[0]);
			UVs.Add(info.uv[1]);
		}
		else if (HasVertexUVs)
		{
			UVs.Add(0f);
			UVs.Add(0f);
		}
		Vertices.Add(info.v[0]);
		Vertices.Add(info.v[1]);
		Vertices.Add(info.v[2]);
		return result;
	}

	public int AppendQuad(int i, int j, int k, int l, int g = -1)
	{
		int result = Quads.Length / 4;
		if (HasFaceGroups)
		{
			FaceGroups.Add((g != -1) ? g : 0);
		}
		Quads.Add(i);
		Quads.Add(j);
		Quads.Add(k);
		Quads.Add(l);
		return result;
	}

	public bool IsVertex(int vID)
	{
		return vID * 3 < Vertices.Length;
	}

	public bool IsQuad(int qID)
	{
		return qID * 4 < Quads.Length;
	}

	public Vector3d GetVertex(int i)
	{
		return new Vector3d(Vertices[3 * i], Vertices[3 * i + 1], Vertices[3 * i + 2]);
	}

	public Vector3f GetVertexNormal(int i)
	{
		return new Vector3f(Normals[3 * i], Normals[3 * i + 1], Normals[3 * i + 2]);
	}

	public Vector3f GetVertexColor(int i)
	{
		return new Vector3f(Colors[3 * i], Colors[3 * i + 1], Colors[3 * i + 2]);
	}

	public Vector2f GetVertexUV(int i)
	{
		return new Vector2f(UVs[2 * i], UVs[2 * i + 1]);
	}

	public NewVertexInfo GetVertexAll(int i)
	{
		NewVertexInfo result = default(NewVertexInfo);
		result.v = GetVertex(i);
		if (HasVertexNormals)
		{
			result.bHaveN = true;
			result.n = GetVertexNormal(i);
		}
		else
		{
			result.bHaveN = false;
		}
		if (HasVertexColors)
		{
			result.bHaveC = true;
			result.c = GetVertexColor(i);
		}
		else
		{
			result.bHaveC = false;
		}
		if (HasVertexUVs)
		{
			result.bHaveUV = true;
			result.uv = GetVertexUV(i);
		}
		else
		{
			result.bHaveUV = false;
		}
		return result;
	}

	public Index4i GetQuad(int i)
	{
		return new Index4i(Quads[4 * i], Quads[4 * i + 1], Quads[4 * i + 2], Quads[4 * i + 3]);
	}

	public int GetFaceGroup(int i)
	{
		return FaceGroups[i];
	}

	public IEnumerable<Vector3d> VerticesItr()
	{
		int N = VertexCount;
		int i = 0;
		while (i < N)
		{
			yield return new Vector3d(Vertices[3 * i], Vertices[3 * i + 1], Vertices[3 * i + 2]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<Vector3f> NormalsItr()
	{
		int N = VertexCount;
		int i = 0;
		while (i < N)
		{
			yield return new Vector3f(Normals[3 * i], Normals[3 * i + 1], Normals[3 * i + 2]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<Vector3f> ColorsItr()
	{
		int N = VertexCount;
		int i = 0;
		while (i < N)
		{
			yield return new Vector3f(Colors[3 * i], Colors[3 * i + 1], Colors[3 * i + 2]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<Vector2f> UVsItr()
	{
		int N = VertexCount;
		int i = 0;
		while (i < N)
		{
			yield return new Vector2f(UVs[2 * i], UVs[2 * i + 1]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<Index4i> QuadsItr()
	{
		int N = QuadCount;
		int i = 0;
		while (i < N)
		{
			yield return new Index4i(Quads[4 * i], Quads[4 * i + 1], Quads[4 * i + 2], Quads[4 * i + 3]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<int> FaceGroupsItr()
	{
		int N = QuadCount;
		int i = 0;
		while (i < N)
		{
			yield return FaceGroups[i];
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<int> VertexIndices()
	{
		int N = VertexCount;
		int i = 0;
		while (i < N)
		{
			yield return i;
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<int> QuadIndices()
	{
		int N = QuadCount;
		int i = 0;
		while (i < N)
		{
			yield return i;
			int num = i + 1;
			i = num;
		}
	}

	public void SetVertex(int i, Vector3d v)
	{
		Vertices[3 * i] = v.x;
		Vertices[3 * i + 1] = v.y;
		Vertices[3 * i + 2] = v.z;
	}

	public void SetVertexNormal(int i, Vector3f n)
	{
		Normals[3 * i] = n.x;
		Normals[3 * i + 1] = n.y;
		Normals[3 * i + 2] = n.z;
	}

	public void SetVertexColor(int i, Vector3f c)
	{
		Colors[3 * i] = c.x;
		Colors[3 * i + 1] = c.y;
		Colors[3 * i + 2] = c.z;
	}

	public void SetVertexUV(int i, Vector2f uv)
	{
		UVs[2 * i] = uv.x;
		UVs[2 * i + 1] = uv.y;
	}

	public double[] GetVertexArray()
	{
		return Vertices.GetBuffer();
	}

	public float[] GetVertexArrayFloat()
	{
		float[] array = new float[Vertices.Length];
		for (int i = 0; i < Vertices.Length; i++)
		{
			array[i] = (float)Vertices[i];
		}
		return array;
	}

	public float[] GetVertexNormalArray()
	{
		if (!HasVertexNormals)
		{
			return null;
		}
		return Normals.GetBuffer();
	}

	public float[] GetVertexColorArray()
	{
		if (!HasVertexColors)
		{
			return null;
		}
		return Colors.GetBuffer();
	}

	public float[] GetVertexUVArray()
	{
		if (!HasVertexUVs)
		{
			return null;
		}
		return UVs.GetBuffer();
	}

	public int[] GetQuadArray()
	{
		return Quads.GetBuffer();
	}

	public int[] GetFaceGroupsArray()
	{
		if (!HasFaceGroups)
		{
			return null;
		}
		return FaceGroups.GetBuffer();
	}

	public static IOWriteResult WriteOBJ(SimpleQuadMesh mesh, string sPath, WriteOptions options)
	{
		StreamWriter streamWriter = new StreamWriter(sPath);
		if (streamWriter.BaseStream == null)
		{
			return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + sPath + " for writing");
		}
		bool flag = options.bPerVertexColors && mesh.HasVertexColors;
		bool flag2 = options.bPerVertexNormals && mesh.HasVertexNormals;
		bool flag3 = options.bPerVertexUVs && mesh.HasVertexUVs;
		if (mesh.UVs != null)
		{
			flag3 = false;
		}
		int[] array = new int[mesh.MaxVertexID];
		int num = 1;
		foreach (int item in mesh.VertexIndices())
		{
			array[item] = num++;
			Vector3d vertex = mesh.GetVertex(item);
			if (flag)
			{
				Vector3d vector3d = mesh.GetVertexColor(item);
				streamWriter.WriteLine("v {0} {1} {2} {3:F8} {4:F8} {5:F8}", vertex[0], vertex[1], vertex[2], vector3d[0], vector3d[1], vector3d[2]);
			}
			else
			{
				streamWriter.WriteLine("v {0} {1} {2}", vertex[0], vertex[1], vertex[2]);
			}
			if (flag2)
			{
				Vector3d vector3d2 = mesh.GetVertexNormal(item);
				streamWriter.WriteLine("vn {0:F10} {1:F10} {2:F10}", vector3d2[0], vector3d2[1], vector3d2[2]);
			}
			if (flag3)
			{
				Vector2f vertexUV = mesh.GetVertexUV(item);
				streamWriter.WriteLine("vt {0:F10} {1:F10}", vertexUV.x, vertexUV.y);
			}
		}
		foreach (int item2 in mesh.QuadIndices())
		{
			Index4i q = mesh.GetQuad(item2);
			q[0] = array[q[0]];
			q[1] = array[q[1]];
			q[2] = array[q[2]];
			q[3] = array[q[3]];
			write_quad(streamWriter, ref q, flag2, flag3, ref q);
		}
		streamWriter.Close();
		return IOWriteResult.Ok;
	}

	private static void write_quad(TextWriter writer, ref Index4i q, bool bNormals, bool bUVs, ref Index4i tuv)
	{
		if (!bNormals && !bUVs)
		{
			writer.WriteLine("f {0} {1} {2} {3}", q[0], q[1], q[2], q[3]);
		}
		else if (bNormals && !bUVs)
		{
			writer.WriteLine("f {0}//{0} {1}//{1} {2}//{2} {3}//{3}", q[0], q[1], q[2], q[3]);
		}
		else if (!bNormals && bUVs)
		{
			writer.WriteLine("f {0}/{4} {1}/{5} {2}/{6} {3}/{7}", q[0], q[1], q[2], q[3], tuv[0], tuv[1], tuv[2], tuv[3]);
		}
		else
		{
			writer.WriteLine("f {0}/{4}/{0} {1}/{5}/{1} {2}/{6}/{2} {3}/{7}/{3}", q[0], q[1], q[2], q[3], tuv[0], tuv[1], tuv[2], tuv[3]);
		}
	}
}
