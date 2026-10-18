using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace g3;

public static class Util
{
	public static bool DebugBreakOnDevAssert;

	public static void gBreakToDebugger()
	{
	}

	[Conditional("DEBUG")]
	public static void gDevAssert(bool bValue, string message = "gDevAssert")
	{
		if (!bValue)
		{
			if (!DebugBreakOnDevAssert)
			{
				throw new Exception(message);
			}
			Debugger.Break();
		}
	}

	public static bool IsRunningOnMono()
	{
		return Type.GetType("Mono.Runtime") != null;
	}

	public static bool IsBitSet(byte b, int pos)
	{
		return (b & (1 << pos)) != 0;
	}

	public static bool IsBitSet(int n, int pos)
	{
		return (n & (1 << pos)) != 0;
	}

	public static bool IsTextString(byte[] array)
	{
		foreach (byte b in array)
		{
			if (b > 127)
			{
				return false;
			}
			if (b < 32 && b != 9 && b != 10 && b != 13)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsBinaryFile(string path, int max_search_len = -1)
	{
		using FileStream streamIn = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return IsBinaryStream(streamIn, max_search_len);
	}

	public static bool IsBinaryStream(Stream streamIn, int max_search_len = -1)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StreamReader streamReader = new StreamReader(streamIn);
		bool result = false;
		while (true)
		{
			if ((num2 = streamReader.Read()) != -1)
			{
				if (num++ >= max_search_len)
				{
					break;
				}
				if (!IsASCIIControlChar(num2))
				{
					if (num2 != 0)
					{
						num3 = 0;
						continue;
					}
					num3++;
					if (num3 < 2)
					{
						continue;
					}
				}
			}
			result = true;
			break;
		}
		streamIn.Seek(0L, SeekOrigin.Begin);
		return result;
	}

	public static bool IsASCIIControlChar(int ch)
	{
		if (ch <= 0 || ch > 8)
		{
			if (ch > 13)
			{
				return ch <= 26;
			}
			return false;
		}
		return true;
	}

	public static string ToHexString(byte[] bytes, bool upperCase = false)
	{
		StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
		for (int i = 0; i < bytes.Length; i++)
		{
			stringBuilder.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
		}
		return stringBuilder.ToString();
	}

	public static float ParseInt(string s, int nDefault)
	{
		try
		{
			return int.Parse(s);
		}
		catch
		{
			return nDefault;
		}
	}

	public static float ParseFloat(string s, float fDefault)
	{
		try
		{
			return float.Parse(s);
		}
		catch
		{
			return fDefault;
		}
	}

	public static double ParseDouble(string s, double fDefault)
	{
		try
		{
			return double.Parse(s);
		}
		catch
		{
			return fDefault;
		}
	}

	public static float[] BufferCopy(float[] from, float[] to)
	{
		if (from == null)
		{
			return null;
		}
		if (to.Length != from.Length)
		{
			to = new float[from.Length];
		}
		Buffer.BlockCopy(from, 0, to, 0, from.Length * 4);
		return to;
	}

	public static int[] BufferCopy(int[] from, int[] to)
	{
		if (from == null)
		{
			return null;
		}
		if (to.Length != from.Length)
		{
			to = new int[from.Length];
		}
		Buffer.BlockCopy(from, 0, to, 0, from.Length * 4);
		return to;
	}

	public static string MakeFloatFormatString(int i, int nPrecision)
	{
		return $"{{{i}:F{nPrecision}}}";
	}

	public static string MakeVec3FormatString(int i0, int i1, int i2, int nPrecision)
	{
		return string.Format("{{{0}:F{3}}} {{{1}:F{3}}} {{{2}:F{3}}}", i0, i1, i2, nPrecision);
	}

	public static string ToSecMilli(TimeSpan t)
	{
		return $"{t.TotalSeconds}";
	}

	public static T[] AppendArrays<T>(params object[] args)
	{
		int num = args.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += (args[i] as T[]).Length;
		}
		T[] array = new T[num2];
		int num3 = 0;
		for (int j = 0; j < num; j++)
		{
			T[] array2 = args[j] as T[];
			Array.Copy(array2, 0, array, num3, array2.Length);
			num3 += array2.Length;
		}
		return array;
	}

	public static byte[] StructureToByteArray(object obj)
	{
		int num = Marshal.SizeOf(obj);
		byte[] array = new byte[num];
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		Marshal.StructureToPtr(obj, intPtr, fDeleteOld: true);
		Marshal.Copy(intPtr, array, 0, num);
		Marshal.FreeHGlobal(intPtr);
		return array;
	}

	public static void ByteArrayToStructure(byte[] bytearray, ref object obj)
	{
		int num = Marshal.SizeOf(obj);
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		Marshal.Copy(bytearray, 0, intPtr, num);
		obj = Marshal.PtrToStructure(intPtr, obj.GetType());
		Marshal.FreeHGlobal(intPtr);
	}

	public static void WriteDebugMesh(IMesh mesh, string sPath)
	{
		WriteOptions defaults = WriteOptions.Defaults;
		defaults.bWriteGroups = true;
		defaults.bPerVertexColors = true;
		defaults.bPerVertexNormals = true;
		defaults.bPerVertexUVs = true;
		StandardMeshWriter.WriteFile(sPath, new List<WriteMesh>
		{
			new WriteMesh(mesh)
		}, defaults);
	}

	public static void WriteDebugMeshAndMarkers(IMesh mesh, List<Vector3d> Markers, string sPath)
	{
		WriteOptions defaults = WriteOptions.Defaults;
		defaults.bWriteGroups = true;
		List<WriteMesh> list = new List<WriteMesh>
		{
			new WriteMesh(mesh)
		};
		double num = BoundsUtil.Bounds(mesh).Diagonal.Length * 0.009999999776482582;
		foreach (Vector3d Marker in Markers)
		{
			TrivialBox3Generator trivialBox3Generator = new TrivialBox3Generator();
			trivialBox3Generator.Box = new Box3d(Marker, num * Vector3d.One);
			trivialBox3Generator.Generate();
			DMesh3 dMesh = new DMesh3();
			trivialBox3Generator.MakeMesh(dMesh);
			list.Add(new WriteMesh(dMesh));
		}
		StandardMeshWriter.WriteFile(sPath, list, defaults);
	}
}
