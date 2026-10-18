using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class SimpleStore
{
	public List<DMesh3> Meshes = new List<DMesh3>();

	public List<Vector3d> Points = new List<Vector3d>();

	public List<string> Strings = new List<string>();

	public List<List<int>> IntLists = new List<List<int>>();

	public SimpleStore()
	{
	}

	public SimpleStore(object[] objs)
	{
		Add(objs);
	}

	public void Add(object[] objs)
	{
		foreach (object obj in objs)
		{
			if (obj is DMesh3)
			{
				Meshes.Add(obj as DMesh3);
				continue;
			}
			if (obj is string)
			{
				Strings.Add(obj as string);
				continue;
			}
			if (obj is List<int>)
			{
				IntLists.Add(obj as List<int>);
				continue;
			}
			if (obj is IEnumerable<int>)
			{
				IntLists.Add(new List<int>(obj as IEnumerable<int>));
				continue;
			}
			if (obj is Vector3d)
			{
				Points.Add((Vector3d)obj);
				continue;
			}
			throw new Exception("SimpleStore: unknown type " + obj.GetType().ToString());
		}
	}

	public static void Store(string sPath, object[] objs)
	{
		SimpleStore s = new SimpleStore(objs);
		Store(sPath, s);
	}

	public static void Store(string sPath, SimpleStore s)
	{
		using FileStream output = new FileStream(sPath, FileMode.Create);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(s.Meshes.Count);
		for (int i = 0; i < s.Meshes.Count; i++)
		{
			gSerialization.Store(s.Meshes[i], binaryWriter);
		}
		binaryWriter.Write(s.Points.Count);
		for (int j = 0; j < s.Points.Count; j++)
		{
			gSerialization.Store(s.Points[j], binaryWriter);
		}
		binaryWriter.Write(s.Strings.Count);
		for (int k = 0; k < s.Strings.Count; k++)
		{
			gSerialization.Store(s.Strings[k], binaryWriter);
		}
		binaryWriter.Write(s.IntLists.Count);
		for (int l = 0; l < s.IntLists.Count; l++)
		{
			gSerialization.Store(s.IntLists[l], binaryWriter);
		}
	}

	public static SimpleStore Restore(string sPath)
	{
		SimpleStore simpleStore = new SimpleStore();
		using FileStream input = new FileStream(sPath, FileMode.Open);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			DMesh3 dMesh = new DMesh3();
			gSerialization.Restore(dMesh, binaryReader);
			simpleStore.Meshes.Add(dMesh);
		}
		int num2 = binaryReader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			Vector3d v = Vector3d.Zero;
			gSerialization.Restore(ref v, binaryReader);
			simpleStore.Points.Add(v);
		}
		int num3 = binaryReader.ReadInt32();
		for (int k = 0; k < num3; k++)
		{
			string s = null;
			gSerialization.Restore(ref s, binaryReader);
			simpleStore.Strings.Add(s);
		}
		int num4 = binaryReader.ReadInt32();
		for (int l = 0; l < num4; l++)
		{
			List<int> list = new List<int>();
			gSerialization.Restore(list, binaryReader);
			simpleStore.IntLists.Add(list);
		}
		return simpleStore;
	}
}
