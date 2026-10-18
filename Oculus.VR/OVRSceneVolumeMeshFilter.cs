using System;
using System.Collections;
using Meta.XR.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[RequireComponent(typeof(MeshFilter))]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRSceneVolumeMeshFilter : MonoBehaviour
{
	private struct GetTriangleMeshCountsJob : IJob
	{
		public OVRSpace Space;

		[WriteOnly]
		public NativeArray<int> Results;

		public void Execute()
		{
			Results[0] = -1;
			Results[1] = -1;
			if (OVRPlugin.GetSpaceTriangleMeshCounts(Space, out var vertexCount, out var triangleCount))
			{
				Results[0] = vertexCount;
				Results[1] = triangleCount;
			}
		}
	}

	private struct GetTriangleMeshJob : IJob
	{
		public OVRSpace Space;

		[WriteOnly]
		public NativeArray<Vector3> Vertices;

		[WriteOnly]
		public NativeArray<int> Triangles;

		public void Execute()
		{
			OVRPlugin.GetSpaceTriangleMesh(Space, Vertices, Triangles);
		}
	}

	private struct PopulateMeshDataJob : IJob
	{
		[ReadOnly]
		public NativeArray<Vector3> Vertices;

		[ReadOnly]
		public NativeArray<int> Triangles;

		[WriteOnly]
		public Mesh.MeshData MeshData;

		public void Execute()
		{
			MeshData.SetVertexBufferParams(Vertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1));
			NativeArray<Vector3> vertexData = MeshData.GetVertexData<Vector3>();
			for (int i = 0; i < vertexData.Length; i++)
			{
				Vector3 vector = Vertices[i];
				vertexData[i] = new Vector3(0f - vector.x, vector.y, vector.z);
			}
			MeshData.SetIndexBufferParams(Triangles.Length, IndexFormat.UInt32);
			NativeArray<int> indexData = MeshData.GetIndexData<int>();
			for (int j = 0; j < indexData.Length; j += 3)
			{
				indexData[j] = Triangles[j];
				indexData[j + 1] = Triangles[j + 2];
				indexData[j + 2] = Triangles[j + 1];
			}
			MeshData.subMeshCount = 1;
			MeshData.SetSubMesh(0, new SubMeshDescriptor(0, Triangles.Length));
		}
	}

	private struct BakeMeshJob : IJob
	{
		public int MeshID;

		public bool Convex;

		public void Execute()
		{
			Physics.BakeMesh(MeshID, Convex);
		}
	}

	private Mesh _mesh;

	private MeshFilter _meshFilter;

	public bool IsCompleted { get; private set; }

	private void Start()
	{
		_mesh = new Mesh
		{
			name = "OVRSceneVolumeMeshFilter (anonymous)"
		};
		_meshFilter = GetComponent<MeshFilter>();
		_meshFilter.sharedMesh = _mesh;
		StartCoroutine(CreateVolumeMesh());
	}

	private IEnumerator CreateVolumeMesh()
	{
		if (!TryGetComponent<OVRSceneAnchor>(out var sceneAnchor))
		{
			IsCompleted = true;
			yield break;
		}
		int num;
		int num2;
		using (NativeArray<int> meshCountResults = new NativeArray<int>(2, Allocator.TempJob))
		{
			JobHandle job2 = new GetTriangleMeshCountsJob
			{
				Space = sceneAnchor.Space,
				Results = meshCountResults
			}.Schedule();
			while (!IsJobDone(job2))
			{
				yield return null;
			}
			num = meshCountResults[0];
			num2 = meshCountResults[1];
		}
		if (num == -1)
		{
			IsCompleted = true;
			yield break;
		}
		NativeArray<Vector3> vertices = new NativeArray<Vector3>(num, Allocator.Persistent);
		NativeArray<int> triangles = new NativeArray<int>(num2 * 3, Allocator.Persistent);
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		GetTriangleMeshJob jobData = default(GetTriangleMeshJob);
		jobData.Space = sceneAnchor.Space;
		jobData.Vertices = vertices;
		jobData.Triangles = triangles;
		JobHandle dependsOn = jobData.Schedule();
		PopulateMeshDataJob jobData2 = default(PopulateMeshDataJob);
		jobData2.Vertices = vertices;
		jobData2.Triangles = triangles;
		jobData2.MeshData = meshDataArray[0];
		JobHandle inputDeps = jobData2.Schedule(dependsOn);
		JobHandle disposeVerticesJob = JobHandle.CombineDependencies(vertices.Dispose(inputDeps), triangles.Dispose(inputDeps));
		while (!IsJobDone(disposeVerticesJob))
		{
			yield return null;
		}
		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _mesh);
		_mesh.RecalculateNormals();
		_mesh.RecalculateBounds();
		if (TryGetComponent<MeshCollider>(out var collider))
		{
			JobHandle job2 = new BakeMeshJob
			{
				MeshID = _mesh.GetInstanceID(),
				Convex = collider.convex
			}.Schedule();
			while (!IsJobDone(job2))
			{
				yield return null;
			}
			collider.sharedMesh = _mesh;
		}
		IsCompleted = true;
	}

	private static bool IsJobDone(JobHandle job)
	{
		bool isCompleted = job.IsCompleted;
		if (isCompleted)
		{
			job.Complete();
		}
		return isCompleted;
	}
}
