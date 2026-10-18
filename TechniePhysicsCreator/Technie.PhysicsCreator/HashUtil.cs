using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class HashUtil
{
	public static Hash160 CalcHash(Mesh srcMesh)
	{
		if (srcMesh == null)
		{
			return new Hash160();
		}
		DateTime now = DateTime.Now;
		HashAlgorithm hashAlgorithm = SHA1.Create();
		Vector3[] vertices = srcMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			byte[] array = ToBytes(vertices[i]);
			hashAlgorithm.TransformBlock(array, 0, array.Length, null, 0);
		}
		for (int j = 0; j < srcMesh.subMeshCount; j++)
		{
			int[] triangles = srcMesh.GetTriangles(j);
			for (int k = 0; k < triangles.Length; k++)
			{
				byte[] bytes = BitConverter.GetBytes(triangles[k]);
				hashAlgorithm.TransformBlock(bytes, 0, bytes.Length, null, 0);
			}
		}
		hashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
		byte[] hash = hashAlgorithm.Hash;
		_ = (DateTime.Now - now).TotalSeconds;
		return new Hash160(hash);
	}

	public static Hash160 CalcHash(string input)
	{
		SHA1 sHA = SHA1.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		sHA.TransformFinalBlock(bytes, 0, bytes.Length);
		return new Hash160(sHA.Hash);
	}

	private static byte[] ToBytes(Vector3 vec)
	{
		byte[] array = new byte[12];
		byte[] bytes = BitConverter.GetBytes(vec.x);
		byte[] bytes2 = BitConverter.GetBytes(vec.x);
		byte[] bytes3 = BitConverter.GetBytes(vec.x);
		bytes.CopyTo(array, 0);
		bytes2.CopyTo(array, 4);
		bytes3.CopyTo(array, 8);
		return array;
	}
}
