using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Voxels;

public static class MeshUtilities
{
	public struct MeshData : IDisposable
	{
		public NativeList<float3> Vertices;

		public NativeList<int> Triangles;

		public NativeList<float3> Normals;

		public void Dispose()
		{
			Vertices.Dispose();
			Triangles.Dispose();
			Normals.Dispose();
		}
	}

	public struct VoxelMeshData : IDisposable
	{
		public NativeList<float3> Vertices;

		public NativeList<byte> Materials;

		public NativeList<int> Triangles;

		public NativeList<float3> Normals;

		public void Dispose()
		{
			Vertices.Dispose();
			Materials.Dispose();
			Triangles.Dispose();
			Normals.Dispose();
		}
	}

	[BurstCompile]
	public struct FaceNormalJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<float3> Verts;

		[ReadOnly]
		public NativeArray<int> Tris;

		[WriteOnly]
		public NativeArray<float3> FaceN;

		public void Execute(int index)
		{
			int num = index * 3;
			float3 @float = Verts[Tris[num]];
			float3 float2 = Verts[Tris[num + 1]];
			float3 float3 = Verts[Tris[num + 2]];
			FaceN[index] = math.normalize(math.cross(float2 - @float, float3 - @float));
		}
	}

	[BurstCompile]
	public struct SplitJob : IJob
	{
		private struct Bucket
		{
			public int next;

			public int newIdx;

			public float3 repN;
		}

		public float CosThresh;

		[ReadOnly]
		public NativeArray<float3> SrcVerts;

		[ReadOnly]
		public NativeArray<int> SrcTris;

		[ReadOnly]
		public NativeArray<float3> FaceN;

		public NativeList<float3> DstVerts;

		public NativeList<int> DstTris;

		public void Execute()
		{
			int length = SrcVerts.Length;
			int num = SrcTris.Length / 3;
			NativeArray<int> nativeArray = new NativeArray<int>(length, Allocator.Temp);
			for (int i = 0; i < length; i++)
			{
				nativeArray[i] = -1;
			}
			NativeList<Bucket> nativeList = new NativeList<Bucket>(length, Allocator.Temp);
			DstVerts.Clear();
			DstTris.ResizeUninitialized(SrcTris.Length);
			for (int j = 0; j < num; j++)
			{
				float3 @float = FaceN[j];
				for (int k = 0; k < 3; k++)
				{
					int index = SrcTris[j * 3 + k];
					int num2 = -1;
					for (int num3 = nativeArray[index]; num3 != -1; num3 = nativeList[num3].next)
					{
						Bucket bucket = nativeList[num3];
						if (math.dot(bucket.repN, @float) >= CosThresh)
						{
							num2 = bucket.newIdx;
							break;
						}
					}
					if (num2 == -1)
					{
						num2 = DstVerts.Length;
						ref NativeList<float3> dstVerts = ref DstVerts;
						float3 value = SrcVerts[index];
						dstVerts.Add(in value);
						Bucket bucket2 = default(Bucket);
						bucket2.next = nativeArray[index];
						bucket2.newIdx = num2;
						bucket2.repN = @float;
						Bucket value2 = bucket2;
						nativeArray[index] = nativeList.Length;
						nativeList.Add(in value2);
					}
					DstTris[j * 3 + k] = num2;
				}
			}
			nativeArray.Dispose();
			nativeList.Dispose();
		}
	}

	[BurstCompile]
	public struct SplitVoxelMeshJob : IJob
	{
		private struct Bucket
		{
			public int next;

			public int newIdx;

			public float3 repN;
		}

		public float CosThresh;

		[ReadOnly]
		public NativeArray<float3> SrcVerts;

		[ReadOnly]
		public NativeArray<byte> SrcMats;

		[ReadOnly]
		public NativeArray<int> SrcTris;

		[ReadOnly]
		public NativeArray<float3> FaceN;

		public NativeList<float3> DstVerts;

		public NativeList<byte> DstMats;

		public NativeList<int> DstTris;

		public void Execute()
		{
			int length = SrcVerts.Length;
			int num = SrcTris.Length / 3;
			NativeArray<int> nativeArray = new NativeArray<int>(length, Allocator.Temp);
			for (int i = 0; i < length; i++)
			{
				nativeArray[i] = -1;
			}
			NativeList<Bucket> nativeList = new NativeList<Bucket>(length, Allocator.Temp);
			DstVerts.Clear();
			DstMats.Clear();
			DstTris.ResizeUninitialized(SrcTris.Length);
			for (int j = 0; j < num; j++)
			{
				float3 @float = FaceN[j];
				for (int k = 0; k < 3; k++)
				{
					int index = SrcTris[j * 3 + k];
					int num2 = -1;
					for (int num3 = nativeArray[index]; num3 != -1; num3 = nativeList[num3].next)
					{
						Bucket bucket = nativeList[num3];
						if (math.dot(bucket.repN, @float) >= CosThresh)
						{
							num2 = bucket.newIdx;
							break;
						}
					}
					if (num2 == -1)
					{
						num2 = DstVerts.Length;
						ref NativeList<float3> dstVerts = ref DstVerts;
						float3 value = SrcVerts[index];
						dstVerts.Add(in value);
						ref NativeList<byte> dstMats = ref DstMats;
						byte value2 = SrcMats[index];
						dstMats.Add(in value2);
						Bucket bucket2 = default(Bucket);
						bucket2.next = nativeArray[index];
						bucket2.newIdx = num2;
						bucket2.repN = @float;
						Bucket value3 = bucket2;
						nativeArray[index] = nativeList.Length;
						nativeList.Add(in value3);
					}
					DstTris[j * 3 + k] = num2;
				}
			}
			nativeArray.Dispose();
			nativeList.Dispose();
		}
	}

	[BurstCompile]
	private struct TriNormalJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<float3> V;

		[ReadOnly]
		public NativeArray<int> T;

		[WriteOnly]
		public NativeArray<float3> Out;

		public int AreaWeight;

		public void Execute(int i)
		{
			int num = i * 3;
			float3 @float = V[T[num]];
			float3 float2 = V[T[num + 1]];
			float3 float3 = math.cross(y: V[T[num + 2]] - @float, x: float2 - @float);
			Out[i] = ((AreaWeight == 0) ? math.normalize(float3) : float3);
		}
	}

	[BurstCompile]
	private struct BuildAdjJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<int> T;

		public NativeParallelMultiHashMap<int, int>.ParallelWriter MapW;

		public void Execute(int triIdx)
		{
			int num = triIdx * 3;
			MapW.Add(T[num], triIdx);
			MapW.Add(T[num + 1], triIdx);
			MapW.Add(T[num + 2], triIdx);
		}
	}

	[BurstCompile]
	private struct VertexNormalJob : IJobParallelFor
	{
		public int AreaWeight;

		[ReadOnly]
		public NativeArray<float3> TriN;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> V2T;

		[WriteOnly]
		public NativeArray<float3> Out;

		public void Execute(int v)
		{
			NativeParallelMultiHashMap<int, int>.Enumerator valuesForKey = V2T.GetValuesForKey(v);
			if (!valuesForKey.MoveNext())
			{
				Out[v] = float3.zero;
				return;
			}
			int current = valuesForKey.Current;
			float3 @float = TriN[current];
			float3 float2 = ((AreaWeight == 0) ? @float : math.normalize(@float));
			float3 zero = float3.zero;
			NativeParallelMultiHashMap<int, int>.Enumerator valuesForKey2 = V2T.GetValuesForKey(v);
			while (valuesForKey2.MoveNext())
			{
				float3 float3 = TriN[valuesForKey2.Current];
				if (AreaWeight != 0)
				{
					math.normalize(float3);
				}
				zero += float3;
			}
			Out[v] = ((math.lengthsq(zero) < 1E-09f) ? float2 : math.normalize(zero));
		}
	}

	public static void SplitByAngle(this Mesh mesh, float angleDeg, bool areaWeight = true, Allocator allocator = Allocator.TempJob)
	{
		NativeArray<float3> srcVerts = new NativeArray<float3>(mesh.vertexCount, allocator);
		List<Vector3> list = new List<Vector3>(mesh.vertexCount);
		mesh.GetVertices(list);
		for (int i = 0; i < list.Count; i++)
		{
			srcVerts[i] = list[i];
		}
		new NativeArray<byte>(mesh.vertexCount, allocator);
		NativeArray<int> srcTris = new NativeArray<int>(mesh.triangles, allocator);
		MeshData meshData = SplitByAngle(srcVerts, srcTris, angleDeg, areaWeight, allocator);
		mesh.Clear();
		mesh.SetVertices(meshData.Vertices.AsArray());
		mesh.SetTriangles(meshData.Triangles.AsArray().ToArray(), 0, calculateBounds: false);
		mesh.SetNormals(meshData.Normals.AsArray());
		mesh.RecalculateBounds();
		srcVerts.Dispose();
		srcTris.Dispose();
		meshData.Dispose();
	}

	public static MeshData SplitByAngle(NativeArray<float3> srcVerts, NativeArray<int> srcTris, float angleDeg, bool areaWeight = true, Allocator allocator = Allocator.TempJob)
	{
		NativeArray<float3> faceN = new NativeArray<float3>(srcTris.Length / 3, allocator);
		FaceNormalJob jobData = default(FaceNormalJob);
		jobData.Verts = srcVerts;
		jobData.Tris = srcTris;
		jobData.FaceN = faceN;
		IJobParallelForExtensions.Schedule(jobData, faceN.Length, 64).Complete();
		NativeList<float3> nativeList = new NativeList<float3>(srcVerts.Length, allocator);
		NativeList<int> nativeList2 = new NativeList<int>(srcTris.Length, allocator);
		SplitJob jobData2 = default(SplitJob);
		jobData2.CosThresh = math.cos(math.radians(angleDeg));
		jobData2.SrcVerts = srcVerts;
		jobData2.SrcTris = srcTris;
		jobData2.FaceN = faceN;
		jobData2.DstVerts = nativeList;
		jobData2.DstTris = nativeList2;
		jobData2.Run();
		NativeList<float3> outNormals = new NativeList<float3>(nativeList.Length, allocator);
		outNormals.ResizeUninitialized(nativeList.Length);
		RecalcNormalsJobified(nativeList, nativeList2, areaWeight, allocator, ref outNormals);
		faceN.Dispose();
		MeshData result = default(MeshData);
		result.Vertices = nativeList;
		result.Triangles = nativeList2;
		result.Normals = outNormals;
		return result;
	}

	public static VoxelMeshData SplitByAngle(NativeArray<float3> srcVerts, NativeArray<byte> srcMats, NativeArray<int> srcTris, float angleDeg, bool areaWeight = true, Allocator allocator = Allocator.TempJob)
	{
		NativeArray<float3> faceN = new NativeArray<float3>(srcTris.Length / 3, allocator);
		FaceNormalJob jobData = default(FaceNormalJob);
		jobData.Verts = srcVerts;
		jobData.Tris = srcTris;
		jobData.FaceN = faceN;
		IJobParallelForExtensions.Schedule(jobData, faceN.Length, 64).Complete();
		NativeList<float3> nativeList = new NativeList<float3>(srcVerts.Length, allocator);
		NativeList<byte> nativeList2 = new NativeList<byte>(srcVerts.Length, allocator);
		NativeList<int> nativeList3 = new NativeList<int>(srcTris.Length, allocator);
		SplitVoxelMeshJob jobData2 = default(SplitVoxelMeshJob);
		jobData2.CosThresh = math.cos(math.radians(angleDeg));
		jobData2.SrcVerts = srcVerts;
		jobData2.SrcMats = srcMats;
		jobData2.SrcTris = srcTris;
		jobData2.FaceN = faceN;
		jobData2.DstVerts = nativeList;
		jobData2.DstMats = nativeList2;
		jobData2.DstTris = nativeList3;
		jobData2.Run();
		NativeList<float3> outNormals = new NativeList<float3>(nativeList.Length, allocator);
		outNormals.ResizeUninitialized(nativeList.Length);
		RecalcNormalsJobified(nativeList, nativeList3, areaWeight, allocator, ref outNormals);
		faceN.Dispose();
		VoxelMeshData result = default(VoxelMeshData);
		result.Vertices = nativeList;
		result.Materials = nativeList2;
		result.Triangles = nativeList3;
		result.Normals = outNormals;
		return result;
	}

	private static void RecalcNormalsJobified(NativeList<float3> verts, NativeList<int> tris, bool areaWeight, Allocator alloc, ref NativeList<float3> outNormals)
	{
		int length = verts.Length;
		int num = tris.Length / 3;
		NativeArray<float3> nativeArray = new NativeArray<float3>(num, alloc);
		TriNormalJob jobData = default(TriNormalJob);
		jobData.V = verts.AsArray();
		jobData.T = tris.AsArray();
		jobData.Out = nativeArray;
		jobData.AreaWeight = (areaWeight ? 1 : 0);
		IJobParallelForExtensions.Schedule(jobData, num, 64).Complete();
		NativeParallelMultiHashMap<int, int> v2T = new NativeParallelMultiHashMap<int, int>(tris.Length, alloc);
		BuildAdjJob jobData2 = default(BuildAdjJob);
		jobData2.T = tris.AsArray();
		jobData2.MapW = v2T.AsParallelWriter();
		IJobParallelForExtensions.Schedule(jobData2, num, 64).Complete();
		VertexNormalJob jobData3 = default(VertexNormalJob);
		jobData3.AreaWeight = (areaWeight ? 1 : 0);
		jobData3.TriN = nativeArray;
		jobData3.V2T = v2T;
		jobData3.Out = outNormals.AsArray();
		IJobParallelForExtensions.Schedule(jobData3, length, 64).Complete();
		nativeArray.Dispose();
		v2T.Dispose();
	}
}
