using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_destructible_mesh_component")]
public class DestructibleMeshComponent : MonoBehaviour
{
	public struct MeshSegment
	{
		public Vector3[] positions;

		public int[] indices;

		public Vector2[] uv;

		public Vector4[] tangents;

		public Color[] colors;
	}

	public struct MeshSegmentationResult
	{
		public List<MeshSegment> segments;

		public MeshSegment reservedSegment;
	}

	public UnityEvent<DestructibleMeshComponent> OnDestructibleMeshCreated;

	public Func<MeshSegmentationResult, MeshSegmentationResult> OnSegmentationCompleted;

	[SerializeField]
	private Material _destructibleMeshMaterial;

	[SerializeField]
	private float _reservedTop = -1f;

	[SerializeField]
	private float _reservedBottom = -1f;

	private Task<MeshSegmentationResult> _segmentationTask;

	private readonly List<GameObject> _segments = new List<GameObject>();

	public Material GlobalMeshMaterial
	{
		get
		{
			return _destructibleMeshMaterial;
		}
		set
		{
			_destructibleMeshMaterial = value;
		}
	}

	public float ReservedTop
	{
		get
		{
			return _reservedTop;
		}
		set
		{
			_reservedTop = value;
		}
	}

	public float ReservedBottom
	{
		get
		{
			return _reservedBottom;
		}
		set
		{
			_reservedBottom = value;
		}
	}

	public GameObject ReservedSegment { get; private set; }

	public unsafe void SegmentMesh(Vector3[] meshPositions, uint[] meshIndices, Vector3[] segmentationPoints)
	{
		Vector3 reservedMin = new Vector3(-1f, -1f, ReservedBottom);
		Vector3 reservedMax = new Vector3(-1f, -1f, ReservedTop);
		_segmentationTask = Task.Run(delegate
		{
			if (MRUKNativeFuncs.ComputeMeshSegmentation(meshPositions, (uint)meshPositions.Length, meshIndices, (uint)meshIndices.Length, segmentationPoints, (uint)segmentationPoints.Length, reservedMin, reservedMax, out var meshSegments, out var numSegments, out var reservedSegment) == MRUKNativeFuncs.MrukResult.Success)
			{
				MeshSegmentationResult result = ProcessSegments(meshSegments, numSegments, reservedSegment);
				MRUKNativeFuncs.FreeMeshSegmentation(meshSegments, numSegments, ref reservedSegment);
				return result;
			}
			MRUKNativeFuncs.FreeMeshSegmentation(meshSegments, numSegments, ref reservedSegment);
			throw new Exception($"Failed to segment the mesh: {MRUKNativeFuncs.MrukResult.ErrorInvalidArgs}");
		});
		_segmentationTask.ContinueWith(OnSegmentationTaskCompleted, TaskScheduler.FromCurrentSynchronizationContext());
	}

	public int GetDestructibleMeshSegmentsCount()
	{
		return base.transform.childCount;
	}

	public void GetDestructibleMeshSegments<T>(T segments) where T : IList<GameObject>
	{
		if (segments == null)
		{
			throw new ArgumentNullException("segments", "Cannot populate the managed container with the global mesh segments as it was never initialized.");
		}
		if (segments.IsReadOnly)
		{
			throw new NotSupportedException("The segments collection is read-only and cannot be modified.");
		}
		segments.Clear();
		foreach (Transform item in base.transform)
		{
			segments.Add(item.gameObject);
		}
	}

	public void GetDestructibleMeshSegments(GameObject[] segments)
	{
		if (segments == null)
		{
			throw new ArgumentNullException("segments", "Cannot populate the array with the global mesh segments as it was never initialized.");
		}
		int num = 0;
		foreach (Transform item in base.transform)
		{
			if (num >= segments.Length)
			{
				throw new ArgumentException("The provided array does not have enough space to hold all segments.", "segments");
			}
			segments[num++] = item.gameObject;
		}
	}

	public void DestroySegment(GameObject segment)
	{
		GetDestructibleMeshSegments(_segments);
		if (!_segments.Contains(segment))
		{
			Debug.LogError("The segment that has been requested to be destroyed does not belong to the destructible mesh anymore.This could be due to the segment being already been destroyed or it had its parent changed.");
			return;
		}
		if (segment == ReservedSegment)
		{
			Debug.LogWarning("The segment that has been requested to be destroyed is the reserved segment and it should not be destroyed.In case the deletion is intended destroy the ReservedSegment game object directly, together with its mesh and material.");
			return;
		}
		if (segment.TryGetComponent<MeshFilter>(out var component) && component.mesh != null)
		{
			UnityEngine.Object.Destroy(component.sharedMesh);
		}
		if (segment.TryGetComponent<MeshRenderer>(out var component2) && component2.material != null)
		{
			UnityEngine.Object.Destroy(component2.material);
		}
		UnityEngine.Object.Destroy(segment);
	}

	private void OnSegmentationTaskCompleted(Task<MeshSegmentationResult> task)
	{
		if (task.Status == TaskStatus.RanToCompletion)
		{
			try
			{
				MeshSegmentationResult result = OnSegmentationCompleted?.Invoke(task.Result) ?? task.Result;
				CreateDestructibleMesh(result);
				OnDestructibleMeshCreated?.Invoke(this);
				return;
			}
			catch (Exception ex)
			{
				Debug.LogError("Error processing segmentation results: " + ex.Message);
				return;
			}
		}
		if (task.IsFaulted)
		{
			Debug.LogError("Segmentation task failed: " + task.Exception?.InnerException?.Message);
		}
		else if (task.IsCanceled)
		{
			Debug.LogWarning("Segmentation task was canceled.");
		}
	}

	private unsafe MeshSegmentationResult ProcessSegments(MRUKNativeFuncs.MrukMesh3f* segments, uint numSegments, MRUKNativeFuncs.MrukMesh3f reservedSegment)
	{
		MeshSegmentationResult meshSegmentationResult = default(MeshSegmentationResult);
		meshSegmentationResult.segments = new List<MeshSegment>();
		meshSegmentationResult.reservedSegment = new MeshSegment
		{
			positions = new Vector3[reservedSegment.numVertices],
			indices = new int[reservedSegment.numIndices]
		};
		MeshSegmentationResult result = meshSegmentationResult;
		for (uint num = 0u; num < numSegments; num++)
		{
			MeshSegment meshSegment = default(MeshSegment);
			meshSegment.positions = new Vector3[segments[num].numVertices];
			meshSegment.indices = new int[segments[num].numIndices];
			MeshSegment item = meshSegment;
			IntPtr intPtr = (IntPtr)segments[num].vertices;
			for (int i = 0; i < segments[num].numVertices; i++)
			{
				float x = Marshal.PtrToStructure<float>(intPtr);
				intPtr = IntPtr.Add(intPtr, 4);
				float y = Marshal.PtrToStructure<float>(intPtr);
				intPtr = IntPtr.Add(intPtr, 4);
				float z = Marshal.PtrToStructure<float>(intPtr);
				intPtr = IntPtr.Add(intPtr, 4);
				item.positions[i] = new Vector3(x, y, z);
			}
			IntPtr intPtr2 = (IntPtr)segments[num].indices;
			for (int j = 0; j < segments[num].numIndices; j++)
			{
				item.indices[j] = Marshal.ReadInt32(intPtr2);
				intPtr2 = IntPtr.Add(intPtr2, 4);
			}
			result.segments.Add(item);
		}
		IntPtr intPtr3 = (IntPtr)reservedSegment.vertices;
		for (int k = 0; k < reservedSegment.numVertices; k++)
		{
			float x2 = Marshal.PtrToStructure<float>(intPtr3);
			intPtr3 = IntPtr.Add(intPtr3, 4);
			float y2 = Marshal.PtrToStructure<float>(intPtr3);
			intPtr3 = IntPtr.Add(intPtr3, 4);
			float z2 = Marshal.PtrToStructure<float>(intPtr3);
			intPtr3 = IntPtr.Add(intPtr3, 4);
			result.reservedSegment.positions[k] = new Vector3(x2, y2, z2);
		}
		IntPtr intPtr4 = (IntPtr)reservedSegment.indices;
		for (int l = 0; l < reservedSegment.numIndices; l++)
		{
			result.reservedSegment.indices[l] = Marshal.ReadInt32(intPtr4);
			intPtr4 = IntPtr.Add(intPtr4, 4);
		}
		return result;
	}

	private void CreateDestructibleMesh(MeshSegmentationResult result)
	{
		foreach (MeshSegment segment in result.segments)
		{
			CreateMeshSegment(segment.positions, segment.indices, segment.uv, segment.tangents, segment.colors);
		}
		if (result.reservedSegment.indices.Length != 0)
		{
			ReservedSegment = CreateMeshSegment(result.reservedSegment.positions, result.reservedSegment.indices, result.reservedSegment.uv, result.reservedSegment.tangents, result.reservedSegment.colors, isReserved: true);
		}
	}

	private GameObject CreateMeshSegment(Vector3[] positions, int[] indices, Vector2[] uv = null, Vector4[] tangents = null, Color[] colors = null, bool isReserved = false)
	{
		if (positions.Length == 0 || indices.Length == 0)
		{
			return null;
		}
		GameObject obj = new GameObject(isReserved ? "ReservedMeshSegment" : "DestructibleMeshSegment");
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshFilter.mesh.indexFormat = ((positions.Length > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		meshFilter.mesh.SetVertices(positions);
		meshFilter.mesh.SetIndices(indices, MeshTopology.Triangles, 0);
		meshFilter.mesh.SetTangents(tangents);
		meshFilter.mesh.SetUVs(0, uv);
		meshFilter.mesh.SetColors(colors);
		meshRenderer.material = GlobalMeshMaterial;
		return obj;
	}

	public void OnDestroy()
	{
		GetDestructibleMeshSegments(_segments);
		for (int num = _segments.Count - 1; num >= 0; num--)
		{
			if (_segments[num].TryGetComponent<MeshFilter>(out var component) && component.mesh != null)
			{
				UnityEngine.Object.Destroy(component.sharedMesh);
			}
			if (_segments[num].TryGetComponent<MeshRenderer>(out var component2) && component2.material != null)
			{
				UnityEngine.Object.Destroy(component2.material);
			}
			UnityEngine.Object.Destroy(_segments[num]);
		}
		_segmentationTask.Dispose();
		_segmentationTask = null;
	}

	public void DebugDestructibleMeshComponent()
	{
		List<GameObject> list = new List<GameObject>();
		GetDestructibleMeshSegments(list);
		foreach (GameObject item in list)
		{
			Material material = new Material(Shader.Find("Meta/Lit"))
			{
				color = UnityEngine.Random.ColorHSV()
			};
			if (item.TryGetComponent<MeshRenderer>(out var component))
			{
				component.material = material;
			}
		}
	}
}
