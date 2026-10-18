using System.Collections.Generic;

namespace g3;

public class SimpleMesh : IDeformableMesh, IMesh, IPointSet
{
	public DVector<double> Vertices;

	public DVector<float> Normals;

	public DVector<float> Colors;

	public DVector<float> UVs;

	public DVector<int> Triangles;

	public DVector<int> FaceGroups;

	private int timestamp;

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

	public int Timestamp => timestamp;

	public int VertexCount => Vertices.Length / 3;

	public int TriangleCount => Triangles.Length / 3;

	public int MaxVertexID => VertexCount;

	public int MaxTriangleID => TriangleCount;

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

	public bool HasTriangleGroups
	{
		get
		{
			if (FaceGroups != null)
			{
				return FaceGroups.Length == Triangles.Length / 3;
			}
			return false;
		}
	}

	public SimpleMesh()
	{
		Initialize();
	}

	public SimpleMesh(IMesh copy)
	{
		Initialize(copy.HasVertexNormals, copy.HasVertexColors, copy.HasVertexUVs, copy.HasTriangleGroups);
		int[] array = new int[copy.MaxVertexID];
		foreach (int item in copy.VertexIndices())
		{
			NewVertexInfo vertexAll = copy.GetVertexAll(item);
			int num = AppendVertex(vertexAll);
			array[item] = num;
		}
		foreach (int item2 in copy.TriangleIndices())
		{
			Index3i triangle = copy.GetTriangle(item2);
			triangle[0] = array[triangle[0]];
			triangle[1] = array[triangle[1]];
			triangle[2] = array[triangle[2]];
			if (copy.HasTriangleGroups)
			{
				AppendTriangle(triangle[0], triangle[1], triangle[2], copy.GetTriangleGroup(item2));
			}
			else
			{
				AppendTriangle(triangle[0], triangle[1], triangle[2]);
			}
		}
	}

	public void Initialize(bool bWantNormals = true, bool bWantColors = true, bool bWantUVs = true, bool bWantFaceGroups = true)
	{
		Vertices = new DVector<double>();
		Normals = (bWantNormals ? new DVector<float>() : null);
		Colors = (bWantColors ? new DVector<float>() : null);
		UVs = (bWantUVs ? new DVector<float>() : null);
		Triangles = new DVector<int>();
		FaceGroups = (bWantFaceGroups ? new DVector<int>() : null);
	}

	public void Initialize(VectorArray3d v, VectorArray3i t, VectorArray3f n = null, VectorArray3f c = null, VectorArray2f uv = null, int[] g = null)
	{
		Vertices = new DVector<double>(v);
		Triangles = new DVector<int>(t);
		Normals = (Colors = (UVs = null));
		FaceGroups = null;
		if (n != null)
		{
			Normals = new DVector<float>(n);
		}
		if (c != null)
		{
			Colors = new DVector<float>(c);
		}
		if (uv != null)
		{
			UVs = new DVector<float>(uv);
		}
		if (g != null)
		{
			FaceGroups = new DVector<int>(g);
		}
	}

	private void updateTimeStamp()
	{
		timestamp++;
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
		updateTimeStamp();
		return result;
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
		updateTimeStamp();
		return result;
	}

	public void AppendVertices(VectorArray3d v, VectorArray3f n = null, VectorArray3f c = null, VectorArray2f uv = null)
	{
		bool hasVertexNormals = HasVertexNormals;
		bool hasVertexColors = HasVertexColors;
		bool hasVertexUVs = HasVertexUVs;
		Vertices.Add(v.array);
		if (n != null && hasVertexNormals)
		{
			Normals.Add(n.array);
		}
		else if (hasVertexNormals)
		{
			Normals.Add(new float[3] { 0f, 1f, 0f }, v.Count);
		}
		if (c != null && hasVertexColors)
		{
			Colors.Add(c.array);
		}
		else if (hasVertexColors)
		{
			Colors.Add(new float[3] { 1f, 1f, 1f }, v.Count);
		}
		if (uv != null && hasVertexUVs)
		{
			UVs.Add(uv.array);
		}
		else if (hasVertexUVs)
		{
			UVs.Add(new float[2], v.Count);
		}
		updateTimeStamp();
	}

	public int AppendTriangle(int i, int j, int k, int g = -1)
	{
		int result = Triangles.Length / 3;
		if (HasTriangleGroups)
		{
			FaceGroups.Add((g != -1) ? g : 0);
		}
		Triangles.Add(i);
		Triangles.Add(j);
		Triangles.Add(k);
		updateTimeStamp();
		return result;
	}

	public void AppendTriangles(int[] vTriangles, int[] vertexMap, int g = -1)
	{
		for (int i = 0; i < vTriangles.Length; i++)
		{
			Triangles.Add(vertexMap[vTriangles[i]]);
		}
		if (HasTriangleGroups)
		{
			for (int j = 0; j < vTriangles.Length / 3; j++)
			{
				FaceGroups.Add((g != -1) ? g : 0);
			}
		}
		updateTimeStamp();
	}

	public void AppendTriangles(IndexArray3i t, int[] groups = null)
	{
		Triangles.Add(t.array);
		if (HasTriangleGroups)
		{
			if (groups != null)
			{
				FaceGroups.Add(groups);
			}
			else
			{
				FaceGroups.Add(0, t.Count);
			}
		}
		updateTimeStamp();
	}

	public void Translate(double tx, double ty, double tz)
	{
		int vertexCount = VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			Vertices[3 * i] += tx;
			Vertices[3 * i + 1] += ty;
			Vertices[3 * i + 2] += tz;
		}
		updateTimeStamp();
	}

	public void Scale(double sx, double sy, double sz)
	{
		int vertexCount = VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			Vertices[3 * i] *= sx;
			Vertices[3 * i + 1] *= sy;
			Vertices[3 * i + 2] *= sz;
		}
		updateTimeStamp();
	}

	public void Scale(double s)
	{
		Scale(s, s, s);
		updateTimeStamp();
	}

	public bool IsVertex(int vID)
	{
		return vID * 3 < Vertices.Length;
	}

	public bool IsTriangle(int tID)
	{
		return tID * 3 < Triangles.Length;
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

	public Index3i GetTriangle(int i)
	{
		return new Index3i(Triangles[3 * i], Triangles[3 * i + 1], Triangles[3 * i + 2]);
	}

	public int GetTriangleGroup(int i)
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

	public IEnumerable<Index3i> TrianglesItr()
	{
		int N = TriangleCount;
		int i = 0;
		while (i < N)
		{
			yield return new Index3i(Triangles[3 * i], Triangles[3 * i + 1], Triangles[3 * i + 2]);
			int num = i + 1;
			i = num;
		}
	}

	public IEnumerable<int> TriangleGroupsItr()
	{
		int N = TriangleCount;
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

	public IEnumerable<int> TriangleIndices()
	{
		int N = TriangleCount;
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
		updateTimeStamp();
	}

	public void SetVertexNormal(int i, Vector3f n)
	{
		Normals[3 * i] = n.x;
		Normals[3 * i + 1] = n.y;
		Normals[3 * i + 2] = n.z;
		updateTimeStamp();
	}

	public void SetVertexColor(int i, Vector3f c)
	{
		Colors[3 * i] = c.x;
		Colors[3 * i + 1] = c.y;
		Colors[3 * i + 2] = c.z;
		updateTimeStamp();
	}

	public void SetVertexUV(int i, Vector2f uv)
	{
		UVs[2 * i] = uv.x;
		UVs[2 * i + 1] = uv.y;
		updateTimeStamp();
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

	public int[] GetTriangleArray()
	{
		return Triangles.GetBuffer();
	}

	public int[] GetFaceGroupsArray()
	{
		if (!HasTriangleGroups)
		{
			return null;
		}
		return FaceGroups.GetBuffer();
	}

	public unsafe void GetVertexBuffer(double* pBuffer)
	{
		DVector<double>.FastGetBuffer(Vertices, pBuffer);
	}

	public unsafe void GetVertexNormalBuffer(float* pBuffer)
	{
		if (HasVertexNormals)
		{
			DVector<float>.FastGetBuffer(Normals, pBuffer);
		}
	}

	public unsafe void GetVertexColorBuffer(float* pBuffer)
	{
		if (HasVertexColors)
		{
			DVector<float>.FastGetBuffer(Colors, pBuffer);
		}
	}

	public unsafe void GetVertexUVBuffer(float* pBuffer)
	{
		if (HasVertexUVs)
		{
			DVector<float>.FastGetBuffer(UVs, pBuffer);
		}
	}

	public unsafe void GetTriangleBuffer(int* pBuffer)
	{
		DVector<int>.FastGetBuffer(Triangles, pBuffer);
	}

	public unsafe void GetFaceGroupsBuffer(int* pBuffer)
	{
		if (HasTriangleGroups)
		{
			DVector<int>.FastGetBuffer(FaceGroups, pBuffer);
		}
	}
}
