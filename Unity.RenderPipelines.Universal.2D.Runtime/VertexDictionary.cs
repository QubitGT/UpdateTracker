using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct VertexDictionary
{
	private static Dictionary<Vector3, int> m_VertexDictionary = new Dictionary<Vector3, int>();

	public NativeArray<int> GetIndexRemap(NativeArray<Vector3> vertices, NativeArray<int> indices)
	{
		NativeArray<int> nativeArray = new NativeArray<int>(vertices.Length, Allocator.Temp);
		m_VertexDictionary.Clear();
		m_VertexDictionary.EnsureCapacity(vertices.Length);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 key = vertices[i];
			if (!m_VertexDictionary.ContainsKey(key))
			{
				nativeArray[i] = i;
				m_VertexDictionary.Add(key, i);
			}
			else
			{
				nativeArray[i] = m_VertexDictionary[key];
			}
		}
		NativeArray<int> result = new NativeArray<int>(indices.Length, Allocator.Temp);
		for (int j = 0; j < indices.Length; j++)
		{
			result[j] = nativeArray[indices[j]];
		}
		return result;
	}
}
