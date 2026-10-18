using System;
using Meta.XR.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRScenePlaneMeshFilter : MonoBehaviour
{
	private struct TriangulateBoundaryJob : IJob
	{
		private struct NList : IDisposable
		{
			private NativeArray<int> _data;

			public int Count { get; private set; }

			public int this[int index] => _data[index];

			public NList(int capacity, Allocator allocator)
			{
				Count = capacity;
				_data = new NativeArray<int>(capacity, allocator);
				for (int i = 0; i < capacity; i++)
				{
					_data[i] = i;
				}
			}

			public void RemoveAt(int index)
			{
				int count = Count - 1;
				Count = count;
				for (int i = index; i < Count; i++)
				{
					_data[i] = _data[i + 1];
				}
			}

			public int GetAt(int index)
			{
				if (index >= Count)
				{
					return _data[index % Count];
				}
				if (index < 0)
				{
					return _data[index % Count + Count];
				}
				return _data[index];
			}

			public void Dispose()
			{
				_data.Dispose();
			}
		}

		[ReadOnly]
		public NativeArray<Vector2> Boundary;

		[WriteOnly]
		public NativeArray<int> Triangles;

		public void Execute()
		{
			if (Boundary.Length == 0 || float.IsNaN(Boundary[0].x))
			{
				return;
			}
			NList nList = new NList(Boundary.Length, Allocator.Temp);
			using (nList)
			{
				bool flag = true;
				int index = 0;
				while (nList.Count > 3)
				{
					if (!flag)
					{
						Debug.LogError("[OVRScenePlaneMeshFilter] Plane boundary triangulation failed.");
						Triangles[0] = 0;
						Triangles[1] = 0;
						Triangles[2] = 0;
						return;
					}
					flag = false;
					for (int i = 0; i < nList.Count; i++)
					{
						int num = nList[i];
						int at = nList.GetAt(i - 1);
						int at2 = nList.GetAt(i + 1);
						Vector2 vector = Boundary[num];
						Vector2 vector2 = Boundary[at];
						Vector2 vector3 = Boundary[at2];
						Vector2 a = vector2 - vector;
						Vector2 b = vector3 - vector;
						if (Cross(a, b) < 0f)
						{
							continue;
						}
						bool flag2 = true;
						for (int j = 0; j < Boundary.Length; j++)
						{
							if (j != num && j != at && j != at2 && PointInTriangle(Boundary[j], vector, vector2, vector3))
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							Triangles[index++] = at2;
							Triangles[index++] = num;
							Triangles[index++] = at;
							nList.RemoveAt(i);
							flag = true;
							break;
						}
					}
				}
				Triangles[index++] = nList[2];
				Triangles[index++] = nList[1];
				Triangles[index] = nList[0];
			}
		}

		private static float Cross(Vector2 a, Vector2 b)
		{
			return a.x * b.y - a.y * b.x;
		}

		private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			if (Cross(b - a, p - a) >= 0f && Cross(c - b, p - b) >= 0f)
			{
				return Cross(a - c, p - c) >= 0f;
			}
			return false;
		}
	}

	private MeshFilter _meshFilter;

	private Mesh _mesh;

	private JobHandle? _jobHandle;

	private bool _meshRequested;

	private NativeArray<Vector2> _boundary;

	private NativeArray<int> _triangles;

	private void Start()
	{
		_mesh = new Mesh();
		_meshFilter = GetComponent<MeshFilter>();
		_meshFilter.sharedMesh = _mesh;
		OVRSceneAnchor component = GetComponent<OVRSceneAnchor>();
		_mesh.name = (component ? string.Format("{0} {1}", "OVRScenePlaneMeshFilter", component.Uuid) : "OVRScenePlaneMeshFilter (anonymous)");
		RequestMeshGeneration();
	}

	internal void ScheduleMeshGeneration()
	{
		if (_jobHandle.HasValue || !TryGetComponent<OVRScenePlane>(out var component) || component.Boundary.Count < 3)
		{
			return;
		}
		using (new OVRProfilerScope("ScheduleMeshGeneration"))
		{
			int count = component.Boundary.Count;
			using (new OVRProfilerScope("Copy boundary"))
			{
				_boundary = new NativeArray<Vector2>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				for (int i = 0; i < component.Boundary.Count; i++)
				{
					_boundary[i] = component.Boundary[i];
				}
			}
			using (new OVRProfilerScope("Schedule TriangulateBoundaryJob"))
			{
				_triangles = new NativeArray<int>((count - 2) * 3, Allocator.TempJob);
				_jobHandle = new TriangulateBoundaryJob
				{
					Boundary = _boundary,
					Triangles = _triangles
				}.Schedule();
			}
		}
	}

	private void Update()
	{
		ref JobHandle? jobHandle = ref _jobHandle;
		if (!jobHandle.HasValue || !jobHandle.GetValueOrDefault().IsCompleted)
		{
			return;
		}
		_jobHandle.Value.Complete();
		_jobHandle = null;
		if (_boundary.IsCreated && _triangles.IsCreated)
		{
			try
			{
				if (_triangles[0] == 0 && _triangles[1] == 0 && _triangles[2] == 0)
				{
					return;
				}
				using (new OVRProfilerScope("Update mesh"))
				{
					NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(_boundary.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
					NativeArray<Vector3> nativeArray2 = new NativeArray<Vector3>(_boundary.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
					NativeArray<Vector2> nativeArray3 = new NativeArray<Vector2>(_boundary.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
					using (new OVRProfilerScope("Prepare mesh data"))
					{
						for (int i = 0; i < _boundary.Length; i++)
						{
							Vector2 vector = _boundary[i];
							nativeArray[i] = new Vector3(vector.x, vector.y, 0f);
							nativeArray2[i] = new Vector3(0f, 0f, 1f);
							nativeArray3[i] = new Vector2(vector.x, vector.y);
						}
					}
					using (nativeArray)
					{
						using (nativeArray2)
						{
							using (nativeArray3)
							{
								using (new OVRProfilerScope("Set mesh data"))
								{
									_mesh.Clear();
									_mesh.SetVertices(nativeArray);
									_mesh.SetIndices(_triangles, MeshTopology.Triangles, 0);
									_mesh.SetNormals(nativeArray2);
									_mesh.SetUVs(0, nativeArray3);
									return;
								}
							}
						}
					}
				}
			}
			finally
			{
				_boundary.Dispose();
				_triangles.Dispose();
			}
		}
		if (_meshRequested)
		{
			ScheduleMeshGeneration();
		}
	}

	internal void RequestMeshGeneration()
	{
		_meshRequested = true;
		if (base.enabled)
		{
			ScheduleMeshGeneration();
		}
	}

	private void OnDisable()
	{
		if (_triangles.IsCreated)
		{
			_triangles.Dispose(_jobHandle.GetValueOrDefault());
		}
		_triangles = default(NativeArray<int>);
		_jobHandle = null;
	}
}
