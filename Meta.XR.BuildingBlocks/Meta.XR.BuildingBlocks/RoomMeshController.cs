using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meta.XR.BuildingBlocks;

public class RoomMeshController : MonoBehaviour
{
	[SerializeField]
	private GameObject _meshPrefab;

	private RoomMeshEvent _roomMeshEvent;

	private RoomMeshAnchor _roomMeshAnchor;

	private void Awake()
	{
		_roomMeshAnchor = GetComponent<RoomMeshAnchor>();
		_roomMeshEvent = Object.FindObjectOfType<RoomMeshEvent>();
	}

	private IEnumerator Start()
	{
		float timeout2 = 10f;
		float startTime2 = Time.time;
		while (!OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.Scene))
		{
			if (Time.time - startTime2 > timeout2)
			{
				Debug.LogWarning("[RoomMeshController] Spatial Data permission is required to load Room Mesh.");
				yield break;
			}
			yield return null;
		}
		yield return LoadRoomMesh();
		yield return UpdateVolume();
		if (_roomMeshAnchor == null)
		{
			yield break;
		}
		timeout2 = 3f;
		startTime2 = Time.time;
		while (!_roomMeshAnchor.IsCompleted)
		{
			if (Time.time - startTime2 > timeout2)
			{
				Debug.LogWarning("[RoomMeshController] Failed to create Room Mesh.");
				yield break;
			}
			yield return null;
		}
		_roomMeshEvent.OnRoomMeshLoadCompleted?.Invoke(_roomMeshAnchor.GetComponent<MeshFilter>());
	}

	private IEnumerator UpdateVolume()
	{
		if (!(_roomMeshAnchor == null))
		{
			while (!_roomMeshAnchor.IsCompleted)
			{
				yield return null;
			}
			MeshFilter component = _roomMeshAnchor.GetComponent<MeshFilter>();
			Mesh sharedMesh = component.sharedMesh;
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			sharedMesh.GetVertices(list);
			sharedMesh.GetTriangles(list2, 0);
			Color[] array = new Color[list2.Count];
			Vector3[] array2 = new Vector3[list2.Count];
			int[] array3 = new int[list2.Count];
			for (int i = 0; i < list2.Count; i++)
			{
				array[i] = new Color((i % 3 == 0) ? 1f : 0f, (i % 3 == 1) ? 1f : 0f, (i % 3 == 2) ? 1f : 0f);
				array2[i] = list[list2[i]];
				array3[i] = i;
			}
			Mesh mesh = new Mesh
			{
				indexFormat = IndexFormat.UInt32
			};
			mesh.SetVertices(array2);
			mesh.SetColors(array);
			mesh.SetIndices(array3, MeshTopology.Triangles, 0, calculateBounds: true, 0);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			component.sharedMesh = mesh;
		}
	}

	private IEnumerator LoadRoomMesh()
	{
		List<OVRAnchor> anchors;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out anchors))
		{
			OVRTask<OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult>> task = OVRAnchor.FetchAnchorsAsync(anchors, new OVRAnchor.FetchOptions
			{
				SingleComponentType = typeof(OVRTriangleMesh)
			});
			while (task.IsPending)
			{
				yield return null;
			}
			if (anchors.Count == 0)
			{
				Debug.LogWarning("[RoomMeshController] No RoomMesh available.");
				yield break;
			}
			foreach (OVRAnchor anchor in anchors)
			{
				if (!anchor.TryGetComponent<OVRLocatable>(out var component))
				{
					Debug.LogWarning("[RoomMeshController] Failed to localize the room mesh anchor.");
					continue;
				}
				OVRTask<bool> localizeTask = component.SetEnabledAsync(enabled: true);
				while (localizeTask.IsPending)
				{
					yield return null;
				}
				InstantiateRoomMesh(anchor, _meshPrefab);
			}
		}
	}

	private void InstantiateRoomMesh(OVRAnchor anchor, GameObject prefab)
	{
		_roomMeshAnchor = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<RoomMeshAnchor>();
		_roomMeshAnchor.gameObject.name = _meshPrefab.name;
		_roomMeshAnchor.gameObject.SetActive(value: true);
		_roomMeshAnchor.Initialize(anchor);
	}
}
