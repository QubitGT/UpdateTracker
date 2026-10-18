using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityEngine.ProBuilder.Stl;

internal static class pb_Stl_Importer
{
	private class Facet
	{
		public Vector3 normal;

		public Vector3 a;

		public Vector3 b;

		public Vector3 c;

		public override string ToString()
		{
			return $"{normal:F2}: {a:F2}, {b:F2}, {c:F2}";
		}
	}

	private const int MAX_FACETS_PER_MESH = 21845;

	private const int SOLID = 1;

	private const int FACET = 2;

	private const int OUTER = 3;

	private const int VERTEX = 4;

	private const int ENDLOOP = 5;

	private const int ENDFACET = 6;

	private const int ENDSOLID = 7;

	private const int EMPTY = 0;

	public static Mesh[] Import(string path)
	{
		if (IsBinary(path))
		{
			try
			{
				return ImportBinary(path);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Failed importing mesh at path {path}.\n{ex.ToString()}");
				return null;
			}
		}
		return ImportAscii(path);
	}

	private static Mesh[] ImportBinary(string path)
	{
		Facet[] array;
		using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			using BinaryReader binaryReader = new BinaryReader(input, new ASCIIEncoding());
			binaryReader.ReadBytes(80);
			uint num = binaryReader.ReadUInt32();
			array = new Facet[num];
			for (uint num2 = 0u; num2 < num; num2++)
			{
				array[num2] = binaryReader.GetFacet();
			}
		}
		return CreateMeshWithFacets(array);
	}

	private static Facet GetFacet(this BinaryReader binaryReader)
	{
		Facet result = new Facet
		{
			normal = binaryReader.GetVector3(),
			a = binaryReader.GetVector3(),
			c = binaryReader.GetVector3(),
			b = binaryReader.GetVector3()
		};
		binaryReader.ReadUInt16();
		return result;
	}

	private static Vector3 GetVector3(this BinaryReader binaryReader)
	{
		Vector3 vector = default(Vector3);
		for (int i = 0; i < 3; i++)
		{
			vector[i] = binaryReader.ReadSingle();
		}
		return vector.UnityCoordTrafo();
	}

	private static Vector3 UnityCoordTrafo(this Vector3 vector3)
	{
		return new Vector3(0f - vector3.y, vector3.z, vector3.x);
	}

	private static int ReadState(string line)
	{
		if (line.StartsWith("solid"))
		{
			return 1;
		}
		if (line.StartsWith("facet"))
		{
			return 2;
		}
		if (line.StartsWith("outer"))
		{
			return 3;
		}
		if (line.StartsWith("vertex"))
		{
			return 4;
		}
		if (line.StartsWith("endloop"))
		{
			return 5;
		}
		if (line.StartsWith("endfacet"))
		{
			return 6;
		}
		if (line.StartsWith("endsolid"))
		{
			return 7;
		}
		return 0;
	}

	private static Mesh[] ImportAscii(string path)
	{
		List<Facet> list = new List<Facet>();
		using (StreamReader streamReader = new StreamReader(path))
		{
			int num = 0;
			int num2 = 0;
			Facet facet = null;
			bool flag = false;
			while (streamReader.Peek() > 0 && !flag)
			{
				string text = streamReader.ReadLine().Trim();
				switch (ReadState(text))
				{
				case 2:
					facet = new Facet();
					facet.normal = StringToVec3(text.Replace("facet normal ", ""));
					break;
				case 3:
					num2 = 0;
					break;
				case 4:
					switch (num2)
					{
					case 0:
						facet.a = StringToVec3(text.Replace("vertex ", ""));
						break;
					case 2:
						facet.c = StringToVec3(text.Replace("vertex ", ""));
						break;
					case 1:
						facet.b = StringToVec3(text.Replace("vertex ", ""));
						break;
					}
					num2++;
					break;
				case 6:
					list.Add(facet);
					break;
				case 7:
					flag = true;
					break;
				}
			}
		}
		return CreateMeshWithFacets(list);
	}

	private static Vector3 StringToVec3(string str)
	{
		string[] array = str.Trim().Split((char[])null);
		Vector3 vector = default(Vector3);
		float.TryParse(array[0], out vector.x);
		float.TryParse(array[1], out vector.y);
		float.TryParse(array[2], out vector.z);
		return vector.UnityCoordTrafo();
	}

	private static bool IsBinary(string path)
	{
		FileInfo fileInfo = new FileInfo(path);
		if (fileInfo.Length < 130)
		{
			return false;
		}
		bool flag = false;
		using (FileStream stream = fileInfo.OpenRead())
		{
			using BufferedStream bufferedStream = new BufferedStream(stream);
			for (long num = 0L; num < 80; num++)
			{
				if (bufferedStream.ReadByte() == 0)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			using FileStream stream2 = fileInfo.OpenRead();
			using BufferedStream bufferedStream2 = new BufferedStream(stream2);
			byte[] array = new byte[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = (byte)bufferedStream2.ReadByte();
			}
			flag = Encoding.UTF8.GetString(array) != "solid ";
		}
		return flag;
	}

	private static Mesh[] CreateMeshWithFacets(IList<Facet> facets)
	{
		int count = facets.Count;
		int num = 0;
		int val = 65535;
		Mesh[] array = new Mesh[count / 21845 + 1];
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = Math.Min(val, (count - num) * 3);
			Vector3[] array2 = new Vector3[num2];
			Vector3[] array3 = new Vector3[num2];
			int[] array4 = new int[num2];
			for (int j = 0; j < num2; j += 3)
			{
				array2[j] = facets[num].a;
				array2[j + 1] = facets[num].b;
				array2[j + 2] = facets[num].c;
				array3[j] = facets[num].normal;
				array3[j + 1] = facets[num].normal;
				array3[j + 2] = facets[num].normal;
				array4[j] = j;
				array4[j + 1] = j + 1;
				array4[j + 2] = j + 2;
				num++;
			}
			array[i] = new Mesh();
			array[i].vertices = array2;
			array[i].normals = array3;
			array[i].triangles = array4;
		}
		return array;
	}
}
