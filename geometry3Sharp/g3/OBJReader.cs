using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace g3;

public class OBJReader : IMeshReader
{
	private DVector<double> vPositions;

	private DVector<float> vNormals;

	private DVector<float> vUVs;

	private DVector<float> vColors;

	private DVector<Triangle> vTriangles;

	private Dictionary<string, OBJMaterial> Materials;

	private Dictionary<int, string> UsedMaterials;

	private bool m_bOBJHasPerVertexColors;

	private int m_nUVComponents;

	private bool m_bOBJHasTriangleGroups;

	private int m_nSetInvalidGroupsTo;

	private string[] splitDoubleSlash;

	private char[] splitSlash;

	private int nWarningLevel;

	private Dictionary<string, int> warningCount = new Dictionary<string, int>();

	public List<string> MTLFileSearchPaths { get; set; }

	public bool HasPerVertexColors => m_bOBJHasPerVertexColors;

	public int UVDimension => m_nUVComponents;

	public bool HasTriangleGroups => m_bOBJHasTriangleGroups;

	public bool HasComplexVertices { get; set; }

	public event ParsingMessagesHandler warningEvent;

	public OBJReader()
	{
		splitDoubleSlash = new string[1] { "//" };
		splitSlash = new char[1] { '/' };
		MTLFileSearchPaths = new List<string>();
	}

	public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder)
	{
		throw new NotImplementedException();
	}

	public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder)
	{
		Materials = new Dictionary<string, OBJMaterial>();
		UsedMaterials = new Dictionary<int, string>();
		HasComplexVertices = false;
		if (nWarningLevel >= 1)
		{
			emit_warning("[OBJReader] starting parse");
		}
		IOReadResult result = ParseInput(reader, options);
		if (result.code != 0)
		{
			return result;
		}
		if (nWarningLevel >= 1)
		{
			emit_warning("[OBJReader] completed parse. building.");
		}
		IOReadResult result2 = ((UsedMaterials.Count > 1 || HasComplexVertices) ? BuildMeshes_ByMaterial(options, builder) : BuildMeshes_Simple(options, builder));
		if (nWarningLevel >= 1)
		{
			emit_warning("[OBJReader] build complete.");
		}
		if (result2.code != 0)
		{
			return result2;
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	private int append_vertex(IMeshBuilder builder, Index3i vertIdx, bool bHaveNormals, bool bHaveColors, bool bHaveUVs)
	{
		int num = 3 * vertIdx.a;
		if (vertIdx.a < 0 || vertIdx.a >= vPositions.Length / 3)
		{
			emit_warning("[OBJReader] append_vertex() referencing invalid vertex " + vertIdx.a);
			return -1;
		}
		if (!bHaveNormals && !bHaveColors && !bHaveUVs)
		{
			return builder.AppendVertex(vPositions[num], vPositions[num + 1], vPositions[num + 2]);
		}
		NewVertexInfo info = default(NewVertexInfo);
		info.bHaveC = (info.bHaveN = (info.bHaveUV = false));
		info.v = new Vector3d(vPositions[num], vPositions[num + 1], vPositions[num + 2]);
		if (bHaveNormals)
		{
			info.bHaveN = true;
			int num2 = 3 * vertIdx.b;
			info.n = new Vector3f(vNormals[num2], vNormals[num2 + 1], vNormals[num2 + 2]);
		}
		if (bHaveColors)
		{
			info.bHaveC = true;
			info.c = new Vector3f(vColors[num], vColors[num + 1], vColors[num + 2]);
		}
		if (bHaveUVs)
		{
			info.bHaveUV = true;
			int num3 = 2 * vertIdx.c;
			info.uv = new Vector2f(vUVs[num3], vUVs[num3 + 1]);
		}
		return builder.AppendVertex(info);
	}

	private int append_triangle(IMeshBuilder builder, int nTri, int[] mapV)
	{
		Triangle triangle = vTriangles[nTri];
		int num = mapV[triangle.vIndices[0] - 1];
		int num2 = mapV[triangle.vIndices[1] - 1];
		int num3 = mapV[triangle.vIndices[2] - 1];
		if (num == -1 || num2 == -1 || num3 == -1)
		{
			emit_warning($"[OBJReader] invalid triangle:  {triangle.vIndices[0]} {triangle.vIndices[1]} {triangle.vIndices[2]}  mapped to {num} {num2} {num3}");
			return -1;
		}
		int g = ((vTriangles[nTri].nGroupID == -1) ? m_nSetInvalidGroupsTo : vTriangles[nTri].nGroupID);
		return builder.AppendTriangle(num, num2, num3, g);
	}

	private int append_triangle(IMeshBuilder builder, Triangle t)
	{
		if (t.vIndices[0] < 0 || t.vIndices[1] < 0 || t.vIndices[2] < 0)
		{
			emit_warning($"[OBJReader] invalid triangle:  {t.vIndices[0]} {t.vIndices[1]} {t.vIndices[2]}");
			return -1;
		}
		int g = ((t.nGroupID == -1) ? m_nSetInvalidGroupsTo : t.nGroupID);
		return builder.AppendTriangle(t.vIndices[0], t.vIndices[1], t.vIndices[2], g);
	}

	private IOReadResult BuildMeshes_Simple(ReadOptions options, IMeshBuilder builder)
	{
		if (vPositions.Length == 0)
		{
			return new IOReadResult(IOCode.GarbageDataError, "No vertices in file");
		}
		if (vTriangles.Length == 0)
		{
			return new IOReadResult(IOCode.GarbageDataError, "No triangles in file");
		}
		bool flag = vNormals.Length == vPositions.Length;
		bool flag2 = vColors.Length == vPositions.Length;
		bool flag3 = vUVs.Length / 2 == vPositions.Length / 3;
		int num = vPositions.Length / 3;
		int[] array = new int[num];
		int meshID = builder.AppendNewMesh(flag, flag2, flag3, m_bOBJHasTriangleGroups);
		for (int i = 0; i < num; i++)
		{
			Index3i vertIdx = new Index3i(i, i, i);
			array[i] = append_vertex(builder, vertIdx, flag, flag2, flag3);
		}
		for (int j = 0; j < vTriangles.Length; j++)
		{
			append_triangle(builder, j, array);
		}
		if (UsedMaterials.Count == 1)
		{
			int key = UsedMaterials.Keys.First();
			string key2 = UsedMaterials[key];
			OBJMaterial m = Materials[key2];
			int materialID = builder.BuildMaterial(m);
			builder.AssignMaterial(materialID, meshID);
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	private IOReadResult BuildMeshes_ByMaterial(ReadOptions options, IMeshBuilder builder)
	{
		if (vPositions.Length == 0)
		{
			return new IOReadResult(IOCode.GarbageDataError, "No vertices in file");
		}
		if (vTriangles.Length == 0)
		{
			return new IOReadResult(IOCode.GarbageDataError, "No triangles in file");
		}
		bool flag = vNormals.Length > 0;
		bool flag2 = vColors.Length > 0;
		bool flag3 = vUVs.Length > 0;
		foreach (int item in new List<int>(UsedMaterials.Keys) { -1 })
		{
			int num = -1;
			if (item != -1)
			{
				string key = UsedMaterials[item];
				OBJMaterial m = Materials[key];
				num = builder.BuildMaterial(m);
			}
			bool flag4 = item != -1 && flag3;
			int num2 = -1;
			Dictionary<Index3i, int> dictionary = new Dictionary<Index3i, int>();
			for (int i = 0; i < vTriangles.Length; i++)
			{
				Triangle triangle = vTriangles[i];
				if (triangle.nMaterialID == item)
				{
					if (num2 == -1)
					{
						num2 = builder.AppendNewMesh(flag, flag2, flag4, bHaveFaceGroups: false);
					}
					Triangle t = default(Triangle);
					for (int j = 0; j < 3; j++)
					{
						Index3i index3i = new Index3i(triangle.vIndices[j] - 1, triangle.vNormals[j] - 1, triangle.vUVs[j] - 1);
						int num3 = -1;
						num3 = (dictionary.ContainsKey(index3i) ? dictionary[index3i] : (dictionary[index3i] = append_vertex(builder, index3i, flag, flag2, flag4)));
						t.vIndices[j] = num3;
					}
					append_triangle(builder, t);
				}
			}
			if (num != -1)
			{
				builder.AssignMaterial(num, num2);
			}
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	public IOReadResult ParseInput(TextReader reader, ReadOptions options)
	{
		vPositions = new DVector<double>();
		vNormals = new DVector<float>();
		vUVs = new DVector<float>();
		vColors = new DVector<float>();
		vTriangles = new DVector<Triangle>();
		bool bOBJHasPerVertexColors = false;
		int num = 0;
		OBJMaterial oBJMaterial = null;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num2 = 0;
		int num3 = -1;
		int num4 = 0;
		while (reader.Peek() >= 0)
		{
			string text = reader.ReadLine();
			num4++;
			string[] array = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0)
			{
				continue;
			}
			try
			{
				if (array[0][0] == 'v')
				{
					if (array[0].Length == 1)
					{
						if (array.Length == 7)
						{
							vPositions.Add(double.Parse(array[1]));
							vPositions.Add(double.Parse(array[2]));
							vPositions.Add(double.Parse(array[3]));
							vColors.Add(float.Parse(array[4]));
							vColors.Add(float.Parse(array[5]));
							vColors.Add(float.Parse(array[6]));
							bOBJHasPerVertexColors = true;
						}
						else if (array.Length >= 4)
						{
							vPositions.Add(double.Parse(array[1]));
							vPositions.Add(double.Parse(array[2]));
							vPositions.Add(double.Parse(array[3]));
						}
						if (array.Length != 4 && array.Length != 7)
						{
							emit_warning("[OBJReader] vertex has unknown format: " + text);
						}
					}
					else if (array[0][1] == 'n')
					{
						if (array.Length >= 4)
						{
							vNormals.Add(float.Parse(array[1]));
							vNormals.Add(float.Parse(array[2]));
							vNormals.Add(float.Parse(array[3]));
						}
						if (array.Length != 4)
						{
							emit_warning("[OBJReader] normal has more than 3 coordinates: " + text);
						}
					}
					else if (array[0][1] == 't')
					{
						if (array.Length >= 3)
						{
							vUVs.Add(float.Parse(array[1]));
							vUVs.Add(float.Parse(array[2]));
							num = Math.Max(num, array.Length);
						}
						if (array.Length != 3)
						{
							emit_warning("[OBJReader] UV has unknown format: " + text);
						}
					}
				}
				else if (array[0][0] == 'f')
				{
					if (array.Length < 4)
					{
						emit_warning("[OBJReader] degenerate face specified : " + text);
					}
					else if (array.Length == 4)
					{
						Triangle t = default(Triangle);
						parse_triangle(array, ref t);
						t.nGroupID = num3;
						if (oBJMaterial != null)
						{
							t.nMaterialID = oBJMaterial.id;
							UsedMaterials[oBJMaterial.id] = oBJMaterial.name;
						}
						vTriangles.Add(t);
						if (t.is_complex())
						{
							HasComplexVertices = true;
						}
					}
					else
					{
						append_face(array, oBJMaterial, num3);
					}
				}
				else if (array[0][0] == 'g')
				{
					string key = ((array.Length == 2) ? array[1] : text.Substring(text.IndexOf(array[1])));
					if (dictionary.ContainsKey(key))
					{
						num3 = dictionary[key];
						continue;
					}
					num3 = num2;
					dictionary[key] = num2++;
				}
				else
				{
					if (array[0][0] == 'o')
					{
						continue;
					}
					if (array[0] == "mtllib" && options.ReadMaterials)
					{
						if (MTLFileSearchPaths.Count == 0)
						{
							emit_warning("Materials requested but Material Search Paths not initialized!");
						}
						string text2 = ((array.Length == 2) ? array[1] : text.Substring(text.IndexOf(array[1])));
						string text3 = FindMTLFile(text2);
						if (text3 != null)
						{
							IOReadResult iOReadResult = ReadMaterials(text3);
							if (iOReadResult.code != 0)
							{
								emit_warning("error parsing " + text3 + " : " + iOReadResult.message);
							}
						}
						else
						{
							emit_warning("material file " + text2 + " could not be found in material search paths");
						}
					}
					else if (array[0] == "usemtl" && options.ReadMaterials)
					{
						oBJMaterial = find_material(array[1]);
					}
					continue;
				}
			}
			catch (Exception ex)
			{
				emit_warning("error parsing line " + num4 + ": " + text + ", exception " + ex.Message);
			}
		}
		m_bOBJHasPerVertexColors = bOBJHasPerVertexColors;
		m_bOBJHasTriangleGroups = num3 != -1;
		m_nSetInvalidGroupsTo = num2++;
		m_nUVComponents = num;
		return new IOReadResult(IOCode.Ok, "");
	}

	private int parse_v(string sToken)
	{
		int num = int.Parse(sToken);
		if (num < 0)
		{
			num = vPositions.Length / 3 + num + 1;
		}
		return num;
	}

	private int parse_n(string sToken)
	{
		int num = int.Parse(sToken);
		if (num < 0)
		{
			num = vNormals.Length / 3 + num + 1;
		}
		return num;
	}

	private int parse_u(string sToken)
	{
		int num = int.Parse(sToken);
		if (num < 0)
		{
			num = vUVs.Length / 2 + num + 1;
		}
		return num;
	}

	private void append_face(string[] tokens, OBJMaterial activeMaterial, int nActiveGroup)
	{
		int num = 0;
		if (tokens[1].IndexOf("//") != -1)
		{
			num = 1;
		}
		else if (tokens[1].IndexOf('/') != -1)
		{
			num = 2;
		}
		Triangle value = default(Triangle);
		value.clear();
		for (int i = 0; i < tokens.Length - 1; i++)
		{
			int num2 = ((i < 3) ? i : 2);
			if (i >= 3)
			{
				value.move_vertex(2, 1);
			}
			switch (num)
			{
			case 0:
				value.set_vertex(num2, parse_v(tokens[i + 1]));
				break;
			case 1:
			{
				string[] array2 = tokens[i + 1].Split(splitDoubleSlash, StringSplitOptions.RemoveEmptyEntries);
				value.set_vertex(num2, parse_v(array2[0]), parse_n(array2[1]));
				break;
			}
			case 2:
			{
				string[] array = tokens[i + 1].Split(splitSlash, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 2)
				{
					value.set_vertex(num2, parse_v(array[0]), -1, parse_u(array[1]));
				}
				else if (array.Length == 3)
				{
					value.set_vertex(num2, parse_v(array[0]), parse_n(array[2]), parse_u(array[1]));
				}
				else
				{
					emit_warning("parse_triangle unexpected face component " + tokens[num2]);
				}
				break;
			}
			}
			if (i >= 2)
			{
				if (activeMaterial != null)
				{
					value.nMaterialID = activeMaterial.id;
					UsedMaterials[activeMaterial.id] = activeMaterial.name;
				}
				value.nGroupID = nActiveGroup;
				vTriangles.Add(value);
				if (value.is_complex())
				{
					HasComplexVertices = true;
				}
			}
		}
	}

	private void parse_triangle(string[] tokens, ref Triangle t)
	{
		int num = 0;
		if (tokens[1].IndexOf("//") != -1)
		{
			num = 1;
		}
		else if (tokens[1].IndexOf('/') != -1)
		{
			num = 2;
		}
		t.clear();
		for (int i = 0; i < 3; i++)
		{
			switch (num)
			{
			case 0:
				t.set_vertex(i, parse_v(tokens[i + 1]));
				break;
			case 1:
			{
				string[] array2 = tokens[i + 1].Split(splitDoubleSlash, StringSplitOptions.RemoveEmptyEntries);
				t.set_vertex(i, parse_v(array2[0]), parse_n(array2[1]));
				break;
			}
			case 2:
			{
				string[] array = tokens[i + 1].Split(splitSlash, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 2)
				{
					t.set_vertex(i, parse_v(array[0]), -1, parse_u(array[1]));
				}
				else if (array.Length == 3)
				{
					t.set_vertex(i, parse_v(array[0]), parse_n(array[2]), parse_u(array[1]));
				}
				else
				{
					emit_warning("parse_triangle unexpected face component " + tokens[i]);
				}
				break;
			}
			}
		}
	}

	private string FindMTLFile(string sMTLFilePath)
	{
		foreach (string mTLFileSearchPath in MTLFileSearchPaths)
		{
			string text = Path.Combine(mTLFileSearchPath, sMTLFilePath);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return null;
	}

	public IOReadResult ReadMaterials(string sPath)
	{
		if (nWarningLevel >= 1)
		{
			emit_warning("[OBJReader] ReadMaterials " + sPath);
		}
		StreamReader streamReader;
		try
		{
			streamReader = new StreamReader(sPath);
			if (streamReader.EndOfStream)
			{
				return new IOReadResult(IOCode.FileAccessError, "");
			}
		}
		catch
		{
			return new IOReadResult(IOCode.FileAccessError, "");
		}
		OBJMaterial oBJMaterial = null;
		while (streamReader.Peek() >= 0)
		{
			string text = streamReader.ReadLine();
			string[] array = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0 || array[0][0] == '#')
			{
				continue;
			}
			if (array[0] == "newmtl")
			{
				oBJMaterial = new OBJMaterial();
				oBJMaterial.name = array[1];
				oBJMaterial.id = Materials.Count;
				if (Materials.ContainsKey(oBJMaterial.name))
				{
					emit_warning("Material file " + sPath + " / material " + oBJMaterial.name + " : already exists in Material set. Replacing.");
				}
				if (nWarningLevel >= 1)
				{
					emit_warning("[OBJReader] parsing material " + oBJMaterial.name);
				}
				Materials[oBJMaterial.name] = oBJMaterial;
			}
			else if (array[0] == "Ka")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Ka = parse_mtl_color(array);
				}
			}
			else if (array[0] == "Kd")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Kd = parse_mtl_color(array);
				}
			}
			else if (array[0] == "Ks")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Ks = parse_mtl_color(array);
				}
			}
			else if (array[0] == "Ke")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Ke = parse_mtl_color(array);
				}
			}
			else if (array[0] == "Tf")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Tf = parse_mtl_color(array);
				}
			}
			else if (array[0] == "illum")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.illum = int.Parse(array[1]);
				}
			}
			else if (array[0] == "d")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.d = float.Parse(array[1]);
				}
			}
			else if (array[0] == "Tr")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.d = 1f - float.Parse(array[1]);
				}
			}
			else if (array[0] == "Ns")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Ns = float.Parse(array[1]);
				}
			}
			else if (array[0] == "sharpness")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.sharpness = float.Parse(array[1]);
				}
			}
			else if (array[0] == "Ni")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.Ni = float.Parse(array[1]);
				}
			}
			else if (array[0] == "map_Ka")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.map_Ka = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "map_Kd")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.map_Kd = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "map_Ks")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.map_Ks = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "map_Ke")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.map_Ke = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "map_d")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.map_d = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "map_Ns")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.map_Ns = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "bump" || array[0] == "map_bump")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.bump = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "disp")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.disp = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "decal")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.decal = parse_mtl_path(text, array);
				}
			}
			else if (array[0] == "refl")
			{
				if (oBJMaterial != null)
				{
					oBJMaterial.refl = parse_mtl_path(text, array);
				}
			}
			else
			{
				emit_warning("unknown material command " + array[0]);
			}
		}
		if (nWarningLevel >= 1)
		{
			emit_warning("[OBJReader] ReadMaterials completed");
		}
		return new IOReadResult(IOCode.Ok, "ok");
	}

	private string parse_mtl_path(string line, string[] tokens)
	{
		if (tokens.Length == 2)
		{
			return tokens[1];
		}
		return line.Substring(line.IndexOf(tokens[1]));
	}

	private Vector3f parse_mtl_color(string[] tokens)
	{
		if (tokens[1] == "spectral")
		{
			emit_warning("OBJReader::parse_material_color : spectral color not supported!");
			return new Vector3f(1f, 0f, 0f);
		}
		if (tokens[1] == "xyz")
		{
			emit_warning("OBJReader::parse_material_color : xyz color not supported!");
			return new Vector3f(1f, 0f, 0f);
		}
		float x = float.Parse(tokens[1]);
		float y = float.Parse(tokens[2]);
		float z = float.Parse(tokens[3]);
		return new Vector3f(x, y, z);
	}

	private OBJMaterial find_material(string sName)
	{
		if (Materials.ContainsKey(sName))
		{
			return Materials[sName];
		}
		try
		{
			return Materials.First((KeyValuePair<string, OBJMaterial> x) => string.Equals(x.Key, sName, StringComparison.OrdinalIgnoreCase)).Value;
		}
		catch
		{
		}
		emit_warning("unknown material " + sName + " referenced");
		return null;
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
