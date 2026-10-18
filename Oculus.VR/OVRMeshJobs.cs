using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class OVRMeshJobs
{
	public struct TransformToUnitySpaceJob : IJobParallelFor
	{
		public NativeArray<Vector3> Vertices;

		public NativeArray<Vector3> Normals;

		public NativeArray<Vector2> UV;

		public NativeArray<BoneWeight> BoneWeights;

		public NativeArray<OVRPlugin.Vector3f> MeshVerticesPosition;

		public NativeArray<OVRPlugin.Vector3f> MeshNormals;

		public NativeArray<OVRPlugin.Vector2f> MeshUV;

		public NativeArray<OVRPlugin.Vector4f> MeshBoneWeights;

		public NativeArray<OVRPlugin.Vector4s> MeshBoneIndices;

		public void Execute(int index)
		{
			Vertices[index] = MeshVerticesPosition[index].FromFlippedXVector3f();
			Normals[index] = MeshNormals[index].FromFlippedXVector3f();
			UV[index] = new Vector2
			{
				x = MeshUV[index].x,
				y = 0f - MeshUV[index].y
			};
			OVRPlugin.Vector4f vector4f = MeshBoneWeights[index];
			OVRPlugin.Vector4s vector4s = MeshBoneIndices[index];
			BoneWeights[index] = new BoneWeight
			{
				boneIndex0 = vector4s.x,
				weight0 = vector4f.x,
				boneIndex1 = vector4s.y,
				weight1 = vector4f.y,
				boneIndex2 = vector4s.z,
				weight2 = vector4f.z,
				boneIndex3 = vector4s.w,
				weight3 = vector4f.w
			};
		}
	}

	public struct TransformTrianglesJob : IJobParallelFor
	{
		public NativeArray<uint> Triangles;

		[ReadOnly]
		public NativeArray<short> MeshIndices;

		public int NumIndices;

		public void Execute(int index)
		{
			Triangles[index] = (uint)MeshIndices[NumIndices - index - 1];
		}
	}

	public struct NativeArrayHelper<T> : IDisposable where T : struct
	{
		public NativeArray<T> UnityNativeArray;

		private GCHandle _handle;

		public unsafe NativeArrayHelper(T[] ovrArray, int length)
		{
			_handle = GCHandle.Alloc(ovrArray, GCHandleType.Pinned);
			IntPtr intPtr = _handle.AddrOfPinnedObject();
			UnityNativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)intPtr, length, Allocator.None);
		}

		public void Dispose()
		{
			_handle.Free();
		}
	}
}
