using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public readonly struct OVRTriangleMesh : IOVRAnchorComponent<OVRTriangleMesh>, IEquatable<OVRTriangleMesh>
{
	private struct GetMeshJob : IJob
	{
		public ulong Space;

		public NativeArray<Vector3> Positions;

		public NativeArray<int> Indices;

		public unsafe void Execute()
		{
			if (!OVRPlugin.GetSpaceTriangleMesh(Space, Positions, Indices))
			{
				UnsafeUtility.MemSet(Indices.GetUnsafePtr(), 0, Indices.Length * 4);
			}
		}
	}

	private struct Triangle
	{
		public int A;

		public int B;

		public int C;
	}

	private struct FlipTriangleWindingJob : IJobParallelFor
	{
		public NativeArray<Triangle> Triangles;

		public void Execute(int index)
		{
			Triangle triangle = Triangles[index];
			Triangles[index] = new Triangle
			{
				A = triangle.A,
				B = triangle.C,
				C = triangle.B
			};
		}
	}

	private struct NegateXJob : IJobParallelFor
	{
		public NativeArray<Vector3> Positions;

		public void Execute(int index)
		{
			Vector3 vector = Positions[index];
			Positions[index] = new Vector3(0f - vector.x, vector.y, vector.z);
		}
	}

	public static readonly OVRTriangleMesh Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRTriangleMesh>.Type => Type;

	ulong IOVRAnchorComponent<OVRTriangleMesh>.Handle => Handle;

	public bool IsNull => Handle == 0;

	public bool IsEnabled
	{
		get
		{
			bool enabled = default(bool);
			bool changePending = default(bool);
			if (!IsNull && OVRPlugin.GetSpaceComponentStatus(Handle, Type, out enabled, out changePending) && enabled)
			{
				return !changePending;
			}
			return false;
		}
	}

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.TriangleMesh;

	internal ulong Handle { get; }

	OVRTriangleMesh IOVRAnchorComponent<OVRTriangleMesh>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRTriangleMesh(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRTriangleMesh>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The TriangleMesh component cannot be enabled or disabled.");
	}

	public bool Equals(OVRTriangleMesh other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRTriangleMesh lhs, OVRTriangleMesh rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRTriangleMesh lhs, OVRTriangleMesh rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRTriangleMesh other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() * 486187739 + ((int)Type).GetHashCode();
	}

	public override string ToString()
	{
		return $"{Handle}.TriangleMesh";
	}

	private OVRTriangleMesh(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	public bool TryGetCounts(out int vertexCount, out int triangleCount)
	{
		return OVRPlugin.GetSpaceTriangleMeshCounts(Handle, out vertexCount, out triangleCount);
	}

	public bool TryGetMeshRawUntransformed(NativeArray<Vector3> positions, NativeArray<int> indices)
	{
		return OVRPlugin.GetSpaceTriangleMesh(Handle, positions, indices);
	}

	public bool TryGetMesh(NativeArray<Vector3> positions, NativeArray<int> indices)
	{
		if (!TryGetMeshRawUntransformed(positions, indices))
		{
			return false;
		}
		for (int i = 0; i < positions.Length; i++)
		{
			Vector3 vector = positions[i];
			positions[i] = new Vector3(0f - vector.x, vector.y, vector.z);
		}
		NativeArray<Triangle> nativeArray = indices.Reinterpret<Triangle>(4);
		for (int j = 0; j < nativeArray.Length; j++)
		{
			Triangle triangle = nativeArray[j];
			nativeArray[j] = new Triangle
			{
				A = triangle.A,
				B = triangle.C,
				C = triangle.B
			};
		}
		return true;
	}

	public JobHandle ScheduleGetMeshJob(NativeArray<Vector3> positions, NativeArray<int> indices, JobHandle dependencies = default(JobHandle))
	{
		GetMeshJob jobData = default(GetMeshJob);
		jobData.Positions = positions;
		jobData.Indices = indices;
		jobData.Space = Handle;
		JobHandle dependsOn = jobData.Schedule(dependencies);
		NativeArray<Triangle> triangles = indices.Reinterpret<Triangle>(4);
		NegateXJob jobData2 = default(NegateXJob);
		jobData2.Positions = positions;
		return JobHandle.CombineDependencies(jobData2.Schedule(positions.Length, 32, dependsOn), new FlipTriangleWindingJob
		{
			Triangles = triangles
		}.Schedule(triangles.Length, 32, dependsOn));
	}
}
