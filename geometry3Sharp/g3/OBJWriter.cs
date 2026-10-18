using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class OBJWriter : IMeshWriter
{
	public Func<string, Stream> OpenStreamF = (string sFilename) => File.Open(sFilename, FileMode.Create);

	public Action<Stream> CloseStreamF = delegate(Stream stream)
	{
		stream.Dispose();
	};

	public string GroupNamePrefix = "mmGroup";

	public Func<int, string> GroupNameF;

	public IOWriteResult Write(BinaryWriter writer, List<WriteMesh> vMeshes, WriteOptions options)
	{
		throw new NotImplementedException();
	}

	public IOWriteResult Write(TextWriter writer, List<WriteMesh> vMeshes, WriteOptions options)
	{
		if (options.groupNamePrefix != null)
		{
			GroupNamePrefix = options.groupNamePrefix;
		}
		if (options.GroupNameF != null)
		{
			GroupNameF = options.GroupNameF;
		}
		int num = 1;
		int num2 = 1;
		string text = "";
		int num3 = 0;
		if (options.bWriteMaterials && options.MaterialFilePath.Length > 0)
		{
			List<GenericMaterial> vMaterials = MeshIOUtil.FindUniqueMaterialList(vMeshes);
			if (write_materials(vMaterials, options).code == IOCode.Ok)
			{
				text = Path.GetFileName(options.MaterialFilePath);
				num3 = vMeshes.Count;
			}
		}
		if (options.AsciiHeaderFunc != null)
		{
			writer.WriteLine(options.AsciiHeaderFunc());
		}
		if (text != "")
		{
			writer.WriteLine("mtllib {0}", text);
		}
		for (int i = 0; i < vMeshes.Count; i++)
		{
			IMesh mesh = vMeshes[i].Mesh;
			if (options.ProgressFunc != null)
			{
				options.ProgressFunc(i, vMeshes.Count);
			}
			bool flag = options.bPerVertexColors && mesh.HasVertexColors;
			bool flag2 = options.bPerVertexNormals && mesh.HasVertexNormals;
			bool flag3 = options.bPerVertexUVs && mesh.HasVertexUVs;
			if (vMeshes[i].UVs != null)
			{
				flag3 = false;
			}
			int[] array = new int[mesh.MaxVertexID];
			foreach (int item in mesh.VertexIndices())
			{
				array[item] = num++;
				Vector3d vertex = mesh.GetVertex(item);
				if (flag)
				{
					Vector3d vector3d = mesh.GetVertexColor(item);
					writer.WriteLine("v {0} {1} {2} {3:F8} {4:F8} {5:F8}", vertex[0], vertex[1], vertex[2], vector3d[0], vector3d[1], vector3d[2]);
				}
				else
				{
					writer.WriteLine("v {0} {1} {2}", vertex[0], vertex[1], vertex[2]);
				}
				if (flag2)
				{
					Vector3d vector3d2 = mesh.GetVertexNormal(item);
					writer.WriteLine("vn {0:F10} {1:F10} {2:F10}", vector3d2[0], vector3d2[1], vector3d2[2]);
				}
				if (flag3)
				{
					Vector2f vertexUV = mesh.GetVertexUV(item);
					writer.WriteLine("vt {0:F10} {1:F10}", vertexUV.x, vertexUV.y);
				}
			}
			IIndexMap mapUV = (flag3 ? new IdentityIndexMap() : null);
			DenseUVMesh denseUVMesh = null;
			if (vMeshes[i].UVs != null)
			{
				denseUVMesh = vMeshes[i].UVs;
				int length = denseUVMesh.UVs.Length;
				IndexMap indexMap = new IndexMap(bForceSparse: false, length);
				for (int j = 0; j < length; j++)
				{
					writer.WriteLine("vt {0:F8} {1:F8}", denseUVMesh.UVs[j].x, denseUVMesh.UVs[j].y);
					indexMap[j] = num2++;
				}
				mapUV = indexMap;
			}
			bool bMaterials = num3 > 0 && vMeshes[i].TriToMaterialMap != null && vMeshes[i].Materials != null;
			if (options.bWriteGroups && mesh.HasTriangleGroups)
			{
				write_triangles_bygroup(writer, mesh, array, denseUVMesh, mapUV, flag2);
			}
			else
			{
				write_triangles_flat(writer, vMeshes[i], array, denseUVMesh, mapUV, flag2, bMaterials);
			}
			if (options.ProgressFunc != null)
			{
				options.ProgressFunc(i + 1, vMeshes.Count);
			}
		}
		return new IOWriteResult(IOCode.Ok, "");
	}

	private void write_triangles_bygroup(TextWriter writer, IMesh mesh, int[] mapV, DenseUVMesh uvSet, IIndexMap mapUV, bool bNormals)
	{
		bool flag = mapUV != null;
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int item in mesh.TriangleIndices())
		{
			hashSet.Add(mesh.GetTriangleGroup(item));
		}
		List<int> list = new List<int>(hashSet);
		list.Sort();
		foreach (int item2 in list)
		{
			string groupNamePrefix = GroupNamePrefix;
			groupNamePrefix = ((GroupNameF == null) ? $"{GroupNamePrefix}{item2}" : GroupNameF(item2));
			writer.WriteLine("g " + groupNamePrefix);
			foreach (int item3 in mesh.TriangleIndices())
			{
				if (mesh.GetTriangleGroup(item3) == item2)
				{
					Index3i t = mesh.GetTriangle(item3);
					t[0] = mapV[t[0]];
					t[1] = mapV[t[1]];
					t[2] = mapV[t[2]];
					if (flag)
					{
						Index3i tuv = uvSet?.TriangleUVs[item3] ?? t;
						tuv[0] = mapUV[tuv[0]];
						tuv[1] = mapUV[tuv[1]];
						tuv[2] = mapUV[tuv[2]];
						write_tri(writer, ref t, bNormals, bUVs: true, ref tuv);
					}
					else
					{
						write_tri(writer, ref t, bNormals, bUVs: false, ref t);
					}
				}
			}
		}
	}

	private void write_triangles_flat(TextWriter writer, WriteMesh write_mesh, int[] mapV, DenseUVMesh uvSet, IIndexMap mapUV, bool bNormals, bool bMaterials)
	{
		bool flag = mapUV != null;
		int cur_material = -1;
		IMesh mesh = write_mesh.Mesh;
		foreach (int item in mesh.TriangleIndices())
		{
			if (bMaterials)
			{
				set_current_material(writer, item, write_mesh, ref cur_material);
			}
			Index3i t = mesh.GetTriangle(item);
			t[0] = mapV[t[0]];
			t[1] = mapV[t[1]];
			t[2] = mapV[t[2]];
			if (flag)
			{
				Index3i tuv = uvSet?.TriangleUVs[item] ?? t;
				tuv[0] = mapUV[tuv[0]];
				tuv[1] = mapUV[tuv[1]];
				tuv[2] = mapUV[tuv[2]];
				write_tri(writer, ref t, bNormals, bUVs: true, ref tuv);
			}
			else
			{
				write_tri(writer, ref t, bNormals, bUVs: false, ref t);
			}
		}
	}

	public void set_current_material(TextWriter writer, int ti, WriteMesh mesh, ref int cur_material)
	{
		int num = mesh.TriToMaterialMap[ti];
		if (num != cur_material && num >= 0 && num < mesh.Materials.Count)
		{
			writer.WriteLine("usemtl " + mesh.Materials[num].name);
			cur_material = num;
		}
	}

	private void write_tri(TextWriter writer, ref Index3i t, bool bNormals, bool bUVs, ref Index3i tuv)
	{
		if (!bNormals && !bUVs)
		{
			writer.WriteLine("f {0} {1} {2}", t[0], t[1], t[2]);
		}
		else if (bNormals && !bUVs)
		{
			writer.WriteLine("f {0}//{0} {1}//{1} {2}//{2}", t[0], t[1], t[2]);
		}
		else if (!bNormals && bUVs)
		{
			writer.WriteLine("f {0}/{3} {1}/{4} {2}/{5}", t[0], t[1], t[2], tuv[0], tuv[1], tuv[2]);
		}
		else
		{
			writer.WriteLine("f {0}/{3}/{0} {1}/{4}/{1} {2}/{5}/{2}", t[0], t[1], t[2], tuv[0], tuv[1], tuv[2]);
		}
	}

	private IOWriteResult write_materials(List<GenericMaterial> vMaterials, WriteOptions options)
	{
		Stream stream = OpenStreamF(options.MaterialFilePath);
		if (stream == null)
		{
			return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + options.MaterialFilePath + " for writing");
		}
		try
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			foreach (GenericMaterial vMaterial in vMaterials)
			{
				if (vMaterial is OBJMaterial)
				{
					OBJMaterial oBJMaterial = vMaterial as OBJMaterial;
					streamWriter.WriteLine("newmtl {0}", oBJMaterial.name);
					if (oBJMaterial.Ka != GenericMaterial.Invalid)
					{
						streamWriter.WriteLine("Ka {0} {1} {2}", oBJMaterial.Ka.x, oBJMaterial.Ka.y, oBJMaterial.Ka.z);
					}
					if (oBJMaterial.Kd != GenericMaterial.Invalid)
					{
						streamWriter.WriteLine("Kd {0} {1} {2}", oBJMaterial.Kd.x, oBJMaterial.Kd.y, oBJMaterial.Kd.z);
					}
					if (oBJMaterial.Ks != GenericMaterial.Invalid)
					{
						streamWriter.WriteLine("Ks {0} {1} {2}", oBJMaterial.Ks.x, oBJMaterial.Ks.y, oBJMaterial.Ks.z);
					}
					if (oBJMaterial.Ke != GenericMaterial.Invalid)
					{
						streamWriter.WriteLine("Ke {0} {1} {2}", oBJMaterial.Ke.x, oBJMaterial.Ke.y, oBJMaterial.Ke.z);
					}
					if (oBJMaterial.Tf != GenericMaterial.Invalid)
					{
						streamWriter.WriteLine("Tf {0} {1} {2}", oBJMaterial.Tf.x, oBJMaterial.Tf.y, oBJMaterial.Tf.z);
					}
					if (oBJMaterial.d != float.MinValue)
					{
						streamWriter.WriteLine("d {0}", oBJMaterial.d);
					}
					if (oBJMaterial.Ns != float.MinValue)
					{
						streamWriter.WriteLine("Ns {0}", oBJMaterial.Ns);
					}
					if (oBJMaterial.Ni != float.MinValue)
					{
						streamWriter.WriteLine("Ni {0}", oBJMaterial.Ni);
					}
					if (oBJMaterial.sharpness != float.MinValue)
					{
						streamWriter.WriteLine("sharpness {0}", oBJMaterial.sharpness);
					}
					if (oBJMaterial.illum != -1)
					{
						streamWriter.WriteLine("illum {0}", oBJMaterial.illum);
					}
					if (oBJMaterial.map_Ka != null && oBJMaterial.map_Ka != "")
					{
						streamWriter.WriteLine("map_Ka {0}", oBJMaterial.map_Ka);
					}
					if (oBJMaterial.map_Kd != null && oBJMaterial.map_Kd != "")
					{
						streamWriter.WriteLine("map_Kd {0}", oBJMaterial.map_Kd);
					}
					if (oBJMaterial.map_Ks != null && oBJMaterial.map_Ks != "")
					{
						streamWriter.WriteLine("map_Ks {0}", oBJMaterial.map_Ks);
					}
					if (oBJMaterial.map_Ke != null && oBJMaterial.map_Ke != "")
					{
						streamWriter.WriteLine("map_Ke {0}", oBJMaterial.map_Ke);
					}
					if (oBJMaterial.map_d != null && oBJMaterial.map_d != "")
					{
						streamWriter.WriteLine("map_d {0}", oBJMaterial.map_d);
					}
					if (oBJMaterial.map_Ns != null && oBJMaterial.map_Ns != "")
					{
						streamWriter.WriteLine("map_Ns {0}", oBJMaterial.map_Ns);
					}
					if (oBJMaterial.bump != null && oBJMaterial.bump != "")
					{
						streamWriter.WriteLine("bump {0}", oBJMaterial.bump);
					}
					if (oBJMaterial.disp != null && oBJMaterial.disp != "")
					{
						streamWriter.WriteLine("disp {0}", oBJMaterial.disp);
					}
					if (oBJMaterial.decal != null && oBJMaterial.decal != "")
					{
						streamWriter.WriteLine("decal {0}", oBJMaterial.decal);
					}
					if (oBJMaterial.refl != null && oBJMaterial.refl != "")
					{
						streamWriter.WriteLine("refl {0}", oBJMaterial.refl);
					}
				}
			}
		}
		finally
		{
			CloseStreamF(stream);
		}
		return IOWriteResult.Ok;
	}
}
