using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class OVRMesh : MonoBehaviour
{
	public interface IOVRMeshDataProvider
	{
		MeshType GetMeshType();
	}

	public enum MeshType
	{
		None = -1,
		[InspectorName("OVR Hand (Left)")]
		HandLeft = 0,
		[InspectorName("OVR Hand (Right)")]
		HandRight = 1,
		[InspectorName("OpenXR Hand (Left)")]
		XRHandLeft = 4,
		[InspectorName("OpenXR Hand (Right)")]
		XRHandRight = 5
	}

	[SerializeField]
	private IOVRMeshDataProvider _dataProvider;

	[SerializeField]
	private MeshType _meshType = MeshType.None;

	private MeshType _loadedMeshType = MeshType.None;

	private Mesh _mesh;

	public bool IsInitialized { get; private set; }

	public Mesh Mesh => _mesh;

	internal MeshType GetMeshType()
	{
		return _meshType;
	}

	internal void SetMeshType(MeshType type)
	{
		_meshType = type;
	}

	private void Awake()
	{
		if (_dataProvider == null)
		{
			_dataProvider = GetComponent<IOVRMeshDataProvider>();
		}
		if (_dataProvider != null)
		{
			_meshType = _dataProvider.GetMeshType();
		}
		if (ShouldInitialize())
		{
			Initialize(_meshType);
		}
	}

	private bool ShouldInitialize()
	{
		if (_loadedMeshType != _meshType)
		{
			return true;
		}
		if (IsInitialized)
		{
			return false;
		}
		if (_meshType == MeshType.None)
		{
			return false;
		}
		_meshType.IsHand();
		return true;
	}

	private void Initialize(MeshType meshType)
	{
		_mesh = new Mesh();
		if (OVRPlugin.GetMesh((OVRPlugin.MeshType)meshType, out var mesh))
		{
			TransformOvrpMesh(mesh, _mesh);
			IsInitialized = true;
		}
		_loadedMeshType = meshType;
	}

	private void TransformOvrpMesh(OVRPlugin.Mesh ovrpMesh, Mesh mesh)
	{
		int numVertices = (int)ovrpMesh.NumVertices;
		int numIndices = (int)ovrpMesh.NumIndices;
		OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector3f> nativeArrayHelper = new OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector3f>(ovrpMesh.VertexPositions, numVertices);
		try
		{
			OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector3f> nativeArrayHelper2 = new OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector3f>(ovrpMesh.VertexNormals, numVertices);
			try
			{
				OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector2f> nativeArrayHelper3 = new OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector2f>(ovrpMesh.VertexUV0, numVertices);
				try
				{
					OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector4f> nativeArrayHelper4 = new OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector4f>(ovrpMesh.BlendWeights, numVertices);
					try
					{
						OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector4s> nativeArrayHelper5 = new OVRMeshJobs.NativeArrayHelper<OVRPlugin.Vector4s>(ovrpMesh.BlendIndices, numVertices);
						try
						{
							OVRMeshJobs.NativeArrayHelper<short> nativeArrayHelper6 = new OVRMeshJobs.NativeArrayHelper<short>(ovrpMesh.Indices, numIndices);
							try
							{
								using NativeArray<Vector3> vertices = new NativeArray<Vector3>(numVertices, Allocator.TempJob);
								using NativeArray<Vector3> normals = new NativeArray<Vector3>(numVertices, Allocator.TempJob);
								using NativeArray<Vector2> uV = new NativeArray<Vector2>(numVertices, Allocator.TempJob);
								using NativeArray<BoneWeight> boneWeights = new NativeArray<BoneWeight>(numVertices, Allocator.TempJob);
								using NativeArray<uint> triangles = new NativeArray<uint>(numIndices, Allocator.TempJob);
								OVRMeshJobs.TransformToUnitySpaceJob transformToUnitySpaceJob = default(OVRMeshJobs.TransformToUnitySpaceJob);
								transformToUnitySpaceJob.Vertices = vertices;
								transformToUnitySpaceJob.Normals = normals;
								transformToUnitySpaceJob.UV = uV;
								transformToUnitySpaceJob.BoneWeights = boneWeights;
								transformToUnitySpaceJob.MeshVerticesPosition = nativeArrayHelper.UnityNativeArray;
								transformToUnitySpaceJob.MeshNormals = nativeArrayHelper2.UnityNativeArray;
								transformToUnitySpaceJob.MeshUV = nativeArrayHelper3.UnityNativeArray;
								transformToUnitySpaceJob.MeshBoneWeights = nativeArrayHelper4.UnityNativeArray;
								transformToUnitySpaceJob.MeshBoneIndices = nativeArrayHelper5.UnityNativeArray;
								OVRMeshJobs.TransformToUnitySpaceJob jobData = transformToUnitySpaceJob;
								OVRMeshJobs.TransformTrianglesJob transformTrianglesJob = default(OVRMeshJobs.TransformTrianglesJob);
								transformTrianglesJob.Triangles = triangles;
								transformTrianglesJob.MeshIndices = nativeArrayHelper6.UnityNativeArray;
								transformTrianglesJob.NumIndices = numIndices;
								OVRMeshJobs.TransformTrianglesJob jobData2 = transformTrianglesJob;
								JobHandle job = jobData.Schedule(numVertices, 20);
								JobHandle job2 = jobData2.Schedule(numIndices, 60);
								JobHandle.CombineDependencies(job, job2).Complete();
								mesh.SetVertices(jobData.Vertices);
								mesh.SetNormals(jobData.Normals);
								mesh.SetUVs(0, jobData.UV);
								mesh.boneWeights = jobData.BoneWeights.ToArray();
								mesh.SetIndexBufferParams(numIndices, IndexFormat.UInt32);
								mesh.SetIndexBufferData(jobData2.Triangles, 0, 0, numIndices);
								mesh.SetSubMesh(0, new SubMeshDescriptor(0, numIndices));
							}
							finally
							{
								((IDisposable)nativeArrayHelper6).Dispose();
							}
						}
						finally
						{
							((IDisposable)nativeArrayHelper5).Dispose();
						}
					}
					finally
					{
						((IDisposable)nativeArrayHelper4).Dispose();
					}
				}
				finally
				{
					((IDisposable)nativeArrayHelper3).Dispose();
				}
			}
			finally
			{
				((IDisposable)nativeArrayHelper2).Dispose();
			}
		}
		finally
		{
			((IDisposable)nativeArrayHelper).Dispose();
		}
	}
}
