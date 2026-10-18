using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Meta.XR.MRUtilityKit;

[Feature(Feature.Scene)]
public class ImmersiveSceneDebugger : MonoBehaviour
{
	private readonly struct DebugAction
	{
		private readonly Action _setup;

		private readonly Action _cleanup;

		private readonly Action _execute;

		public DebugAction(Action setup, Action execute, Action cleanup)
		{
			_setup = setup;
			_execute = execute;
			_cleanup = cleanup;
		}

		public void Setup()
		{
			_setup?.Invoke();
		}

		public void Cleanup()
		{
			_cleanup?.Invoke();
		}

		public void Execute()
		{
			_execute?.Invoke();
		}

		public bool Equals(DebugAction other)
		{
			if (_setup == other._setup && _execute == other._execute)
			{
				return _cleanup == other._cleanup;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DebugAction other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((17 * 23 + (_setup?.GetHashCode() ?? 0)) * 23 + (_execute?.GetHashCode() ?? 0)) * 23 + (_cleanup?.GetHashCode() ?? 0);
		}

		public static bool operator ==(DebugAction left, DebugAction right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DebugAction left, DebugAction right)
		{
			return !left.Equals(right);
		}
	}

	[Tooltip("Visualize anchors")]
	public bool ShowDebugAnchors;

	[SerializeField]
	private Material visualHelperMaterial;

	[SerializeField]
	private Shader _debugShader;

	private readonly int _srcBlend = Shader.PropertyToID("_SrcBlend");

	private readonly int _dstBlend = Shader.PropertyToID("_DstBlend");

	private readonly int _zWrite = Shader.PropertyToID("_ZWrite");

	private readonly int _cull = Shader.PropertyToID("_Cull");

	private readonly int _color = Shader.PropertyToID("_Color");

	private readonly List<GameObject> _debugAnchors = new List<GameObject>();

	private GameObject _globalMeshGO;

	private OVRCameraRig _cameraRig;

	private MRUKRoom _currentRoom;

	private GameObject _debugCube;

	private GameObject _debugSphere;

	private GameObject _debugNormal;

	private GameObject _navMeshViz;

	private GameObject _debugAnchor;

	private bool _previousShowDebugAnchors;

	private Mesh _debugCheckerMesh;

	private MRUKAnchor _previousShownDebugAnchor;

	private MRUKAnchor _globalMeshAnchor;

	private NavMeshTriangulation _navMeshTriangulation;

	private SpaceMapGPU _spaceMapGPU;

	private MeshCollider _globalMeshCollider;

	private Material _navMeshMaterial;

	private string _debugMessage = "";

	private string _currentDebugMessage = "";

	private string _sceneDetails = "";

	private Material _debugMaterial;

	private Material _checkerMeshMaterial;

	private DebugAction _isPositionInRoom;

	private DebugAction _showDebugAnchorsDebugAction;

	private DebugAction _raycastDebugger;

	private MRUK.PositioningMethod _positioningMethod = MRUK.PositioningMethod.CENTER;

	private DebugAction _getBestPoseFromRaycastDebugger;

	private DebugAction _getKeyWallDebugger;

	private DebugAction _getLaunchSpaceSetupDebugger;

	private MRUKAnchor.SceneLabels _largestSurfaceFilter = MRUKAnchor.SceneLabels.TABLE;

	private DebugAction _getLargestSurfaceDebugger;

	private DebugAction _getClosestSeatPoseDebugger;

	private DebugAction _getClosestSurfacePositionDebugger;

	private bool exportGlobalMeshJSON = true;

	private DebugAction? _currentDebugAction;

	private bool _shouldDisplayGlobalMesh;

	private bool _shouldToggleGlobalMeshCollision;

	private bool _shouldDisplayNavMesh;

	private bool _roomHasChanged
	{
		get
		{
			if (_currentRoom == MRUK.Instance.GetCurrentRoom())
			{
				return false;
			}
			_currentRoom = MRUK.Instance.GetCurrentRoom();
			_globalMeshAnchor = _currentRoom.GlobalMeshAnchor;
			return true;
		}
	}

	private bool ShouldDisplayGlobalMesh
	{
		get
		{
			return _shouldDisplayGlobalMesh;
		}
		set
		{
			_shouldDisplayGlobalMesh = value;
			DisplayGlobalMesh(value);
		}
	}

	private bool ShouldToggleGlobalMeshCollision
	{
		get
		{
			return _shouldToggleGlobalMeshCollision;
		}
		set
		{
			_shouldToggleGlobalMeshCollision = value;
			ToggleGlobalMeshCollisions(value);
		}
	}

	private bool ShouldDisplayNavMesh
	{
		get
		{
			return _shouldDisplayNavMesh;
		}
		set
		{
			_shouldDisplayNavMesh = value;
			DisplayNavMesh(value);
		}
	}

	internal static ImmersiveSceneDebugger Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			Instance = this;
		}
		_cameraRig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
		_isPositionInRoom = IsPositionInRoomDebugger();
		_showDebugAnchorsDebugAction = ShowDebugAnchorsDebugger();
		_raycastDebugger = RayCastDebugger();
		_getBestPoseFromRaycastDebugger = GetBestPoseFromRaycastDebugger();
		_getKeyWallDebugger = GetKeyWallDebugger();
		_getLaunchSpaceSetupDebugger = GetLaunchSpaceSetupDebugger();
		_getLargestSurfaceDebugger = GetLargestSurfaceDebugger();
		_getClosestSeatPoseDebugger = GetClosestSeatPoseDebugger();
		_getClosestSurfacePositionDebugger = GetClosestSurfacePositionDebugger();
	}

	private void Start()
	{
		MRUK.Instance?.RegisterSceneLoadedCallback(OnSceneLoaded);
		OVRTelemetry.Start(651897568, 0, -1L).Send();
		_currentRoom = MRUK.Instance?.GetCurrentRoom();
		_sceneDetails = ShowRoomDetails();
		_debugMaterial = new Material(_debugShader)
		{
			color = Color.green
		};
		_navMeshMaterial = new Material(_debugShader)
		{
			color = Color.cyan
		};
		SetupCheckerMeshMaterial(_debugShader);
		CreateDebugPrimitives();
	}

	private void Update()
	{
		_currentDebugAction?.Execute();
		if (!_currentDebugMessage.Equals(_debugMessage))
		{
			_currentDebugMessage = _debugMessage;
			Debug.Log(_currentDebugMessage);
		}
		if (ShowDebugAnchors == _previousShowDebugAnchors)
		{
			return;
		}
		if (ShowDebugAnchors)
		{
			foreach (MRUKRoom room in MRUK.Instance.Rooms)
			{
				foreach (MRUKAnchor anchor in room.Anchors)
				{
					GameObject item = GenerateDebugAnchor(anchor);
					_debugAnchors.Add(item);
				}
			}
		}
		else
		{
			foreach (GameObject debugAnchor in _debugAnchors)
			{
				UnityEngine.Object.Destroy(debugAnchor.gameObject);
			}
		}
		_previousShowDebugAnchors = ShowDebugAnchors;
	}

	private void OnDisable()
	{
		_currentDebugAction = null;
	}

	public void OnDestroy()
	{
		MRUK.Instance?.SceneLoadedEvent.RemoveListener(OnSceneLoaded);
	}

	private void OnSceneLoaded()
	{
		CreateDebugPrimitives();
		if ((bool)MRUK.Instance && (bool)MRUK.Instance.GetCurrentRoom() && !_globalMeshAnchor)
		{
			_globalMeshAnchor = MRUK.Instance.GetCurrentRoom().GlobalMeshAnchor;
		}
	}

	private void IsPositionInRoom()
	{
		SetDebugAction(_isPositionInRoom);
	}

	private void DisplayDebugAnchors()
	{
		SetDebugAction(_showDebugAnchorsDebugAction);
	}

	private void Raycast()
	{
		SetDebugAction(_raycastDebugger);
	}

	private void GetBestPoseFromRayCast()
	{
		SetDebugAction(_getBestPoseFromRaycastDebugger);
	}

	private void GetKeyWall()
	{
		SetDebugAction(_getKeyWallDebugger);
	}

	private void GetLaunchSpaceSetup()
	{
		SetDebugAction(_getLaunchSpaceSetupDebugger);
	}

	private void GetLargestSurface()
	{
		SetDebugAction(_getLargestSurfaceDebugger);
	}

	private void GetClosestSeatPose()
	{
		SetDebugAction(_getClosestSeatPoseDebugger);
	}

	private void GetClosestSurfacePosition()
	{
		SetDebugAction(_getClosestSurfacePositionDebugger);
	}

	private void SetDebugAction(DebugAction newDebugAction)
	{
		_currentDebugAction?.Cleanup();
		if (_currentDebugAction == newDebugAction)
		{
			_currentDebugAction = null;
			return;
		}
		_currentDebugAction = newDebugAction;
		_currentDebugAction?.Setup();
	}

	private Ray GetControllerRay()
	{
		Vector3 position;
		Vector3 forward;
		if (OVRInput.activeControllerType == OVRInput.Controller.Touch || OVRInput.activeControllerType == OVRInput.Controller.RTouch)
		{
			position = _cameraRig.rightHandOnControllerAnchor.position;
			forward = _cameraRig.rightHandOnControllerAnchor.forward;
		}
		else if (OVRInput.activeControllerType == OVRInput.Controller.LTouch)
		{
			position = _cameraRig.leftHandOnControllerAnchor.position;
			forward = _cameraRig.leftHandOnControllerAnchor.forward;
		}
		else
		{
			OVRHand componentInChildren = _cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>();
			if (componentInChildren != null)
			{
				position = componentInChildren.PointerPose.position;
				forward = componentInChildren.PointerPose.forward;
			}
			else
			{
				position = _cameraRig.centerEyeAnchor.position;
				forward = _cameraRig.centerEyeAnchor.forward;
			}
		}
		return new Ray(position, forward);
	}

	private DebugAction GetKeyWallDebugger()
	{
		return new DebugAction(delegate
		{
			_debugCube.SetActive(value: true);
			Vector2 wallScale = Vector2.zero;
			MRUKAnchor mRUKAnchor = MRUK.Instance?.GetCurrentRoom()?.GetKeyWall(out wallScale);
			if (mRUKAnchor != null && _debugCube != null)
			{
				_debugCube.transform.localScale = new Vector3(wallScale.x, wallScale.y, 0.05f);
				_debugCube.transform.position = mRUKAnchor.transform.position;
				_debugCube.transform.rotation = mRUKAnchor.transform.rotation;
			}
			_debugMessage = string.Format("[{0}] Size: {1}", "GetKeyWallDebugger", wallScale);
		}, delegate
		{
		}, delegate
		{
			_debugCube.SetActive(value: false);
		});
	}

	private DebugAction GetLaunchSpaceSetupDebugger()
	{
		return new DebugAction(async delegate
		{
			if (await OVRScene.RequestSpaceSetup())
			{
				await MRUK.Instance.LoadSceneFromDevice(requestSceneCaptureIfNoDataFound: false);
			}
		}, delegate
		{
		}, delegate
		{
		});
	}

	private DebugAction GetLargestSurfaceDebugger()
	{
		return new DebugAction(delegate
		{
			_debugCube.SetActive(value: true);
		}, delegate
		{
			MRUKAnchor mRUKAnchor = MRUK.Instance?.GetCurrentRoom()?.FindLargestSurface(_largestSurfaceFilter);
			if (mRUKAnchor != null)
			{
				if (_debugCube != null)
				{
					Vector3 vector = (mRUKAnchor.PlaneRect.HasValue ? new Vector3(mRUKAnchor.PlaneRect.Value.width, mRUKAnchor.PlaneRect.Value.height, 0.01f) : mRUKAnchor.VolumeBounds.Value.size);
					_debugCube.transform.localScale = vector + new Vector3(0.01f, 0.01f, 0.01f);
					_debugCube.transform.position = (mRUKAnchor.PlaneRect.HasValue ? mRUKAnchor.transform.position : mRUKAnchor.transform.TransformPoint(mRUKAnchor.VolumeBounds.Value.center));
					_debugCube.transform.rotation = mRUKAnchor.transform.rotation;
				}
				_debugMessage = string.Format("[{0}] Anchor: {1} Type: {2}", "GetLargestSurface", mRUKAnchor.name, mRUKAnchor.Label);
			}
			else
			{
				_debugMessage = "[GetLargestSurface] Cannot get surface area for this label in this scene.";
				_debugCube.SetActive(value: false);
			}
		}, delegate
		{
			_debugCube.SetActive(value: false);
		});
	}

	private DebugAction GetClosestSeatPoseDebugger()
	{
		return new DebugAction(delegate
		{
			MRUKAnchor couch = null;
			Pose seatPose = default(Pose);
			Ray controllerRay = GetControllerRay();
			MRUK.Instance?.GetCurrentRoom()?.TryGetClosestSeatPose(controllerRay, out seatPose, out couch);
			if ((bool)couch)
			{
				_debugCube.SetActive(value: true);
				if (_debugCube != null)
				{
					_debugCube.transform.localRotation = couch.transform.localRotation;
					_debugCube.transform.position = seatPose.position;
					_debugCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				}
				_debugMessage = string.Format("[{0}] Seat: {1} Position: {2}", "GetClosestSeatPoseDebugger", couch.name, seatPose.position) + "Distance: " + Vector3.Distance(seatPose.position, controllerRay.origin).ToString("0.##");
			}
			else
			{
				_debugMessage = "[GetClosestSeatPoseDebugger]  No seat found in the scene.";
			}
		}, delegate
		{
		}, delegate
		{
			_debugCube.SetActive(value: false);
		});
	}

	private DebugAction GetClosestSurfacePositionDebugger()
	{
		return new DebugAction(delegate
		{
			_debugNormal.SetActive(value: true);
		}, delegate
		{
			Vector3 origin = GetControllerRay().origin;
			Vector3 surfacePosition = Vector3.zero;
			Vector3 normal = Vector3.up;
			MRUKAnchor closestAnchor = null;
			MRUK.Instance?.GetCurrentRoom()?.TryGetClosestSurfacePosition(origin, out surfacePosition, out closestAnchor, out normal);
			ShowHitNormal(surfacePosition, normal);
			if (closestAnchor != null)
			{
				_debugMessage = string.Format("[{0}] Anchor: {1} Surface Position: {2} Distance: {3}", "GetClosestSurfacePosition", closestAnchor.name, surfacePosition, Vector3.Distance(origin, surfacePosition).ToString("0.##"));
			}
		}, delegate
		{
			_debugNormal.SetActive(value: false);
		});
	}

	private DebugAction GetBestPoseFromRaycastDebugger()
	{
		return new DebugAction(delegate
		{
			_debugCube.SetActive(value: true);
		}, delegate
		{
			Ray controllerRay = GetControllerRay();
			MRUKAnchor sceneAnchor = null;
			Pose? pose = MRUK.Instance?.GetCurrentRoom()?.GetBestPoseFromRaycast(controllerRay, float.PositiveInfinity, default(LabelFilter), out sceneAnchor, _positioningMethod);
			if (pose.HasValue && (bool)sceneAnchor && (bool)_debugCube)
			{
				_debugCube.transform.position = pose.Value.position;
				_debugCube.transform.rotation = pose.Value.rotation;
				_debugCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				_debugMessage = string.Format("[{0}] Anchor: {1} Pose Position: {2} Pose Rotation: {3}", "GetBestPoseFromRayCast", sceneAnchor.name, pose.Value.position, pose.Value.rotation);
			}
		}, delegate
		{
			_debugCube.SetActive(value: false);
		});
	}

	private DebugAction RayCastDebugger()
	{
		return new DebugAction(delegate
		{
			_debugNormal.SetActive(value: true);
		}, delegate
		{
			Ray controllerRay = GetControllerRay();
			RaycastHit hit = default(RaycastHit);
			MRUKAnchor anchor = null;
			MRUK.Instance?.GetCurrentRoom()?.Raycast(controllerRay, float.PositiveInfinity, out hit, out anchor);
			ShowHitNormal(hit.point, hit.normal);
			if (anchor != null)
			{
				_debugMessage = string.Format("[{0}] Anchor: {1} Hit point: {2} Hit normal: {3}", "Raycast", anchor.name, hit.point, hit.normal);
			}
		}, delegate
		{
			_debugNormal.SetActive(value: false);
		});
	}

	private DebugAction IsPositionInRoomDebugger()
	{
		return new DebugAction(null, delegate
		{
			Ray controllerRay = GetControllerRay();
			if (_debugSphere != null)
			{
				_debugSphere.SetActive(value: true);
				bool? flag = MRUK.Instance?.GetCurrentRoom()?.IsPositionInRoom(_debugSphere.transform.position);
				_debugSphere.transform.position = controllerRay.GetPoint(0.2f);
				_debugSphere.GetComponent<Renderer>().material.color = ((flag.HasValue && flag.Value) ? Color.green : Color.red);
				_debugMessage = string.Format("[{0}] Position: {1} ", "IsPositionInRoom", _debugSphere.transform.position) + $"Is inside the Room: {flag}";
			}
		}, delegate
		{
			_debugSphere.SetActive(value: false);
		});
	}

	private DebugAction ShowDebugAnchorsDebugger()
	{
		return new DebugAction(null, delegate
		{
			Ray controllerRay = GetControllerRay();
			RaycastHit hit = default(RaycastHit);
			MRUKAnchor anchor = null;
			MRUK.Instance?.GetCurrentRoom()?.Raycast(controllerRay, float.PositiveInfinity, out hit, out anchor);
			if (_previousShownDebugAnchor != anchor && anchor != null)
			{
				UnityEngine.Object.Destroy(_debugAnchor);
				_debugAnchor = GenerateDebugAnchor(anchor);
				_previousShownDebugAnchor = anchor;
			}
			ShowHitNormal(hit.point, hit.normal);
			_debugMessage = string.Format("[{0}] Hit point: {1} Hit normal: {2}", "ShowDebugAnchorsDebugger", hit.point, hit.normal);
		}, delegate
		{
			UnityEngine.Object.Destroy(_debugAnchor);
			_debugAnchor = null;
			if (_debugNormal != null)
			{
				_debugNormal.SetActive(value: false);
			}
		});
	}

	public void DisplayGlobalMesh(bool isOn)
	{
		if (!_globalMeshAnchor)
		{
			Debug.Log("[DisplayGlobalMesh] No global mesh anchor found in the scene.");
		}
		else if (isOn)
		{
			if (_roomHasChanged || !_globalMeshGO)
			{
				if ((bool)_globalMeshGO)
				{
					UnityEngine.Object.DestroyImmediate(_globalMeshGO);
				}
				InstantiateGlobalMesh(delegate(GameObject globalMeshSegmentGO, Mesh mesh)
				{
					globalMeshSegmentGO.AddComponent<MeshRenderer>().material = visualHelperMaterial;
				});
			}
			else
			{
				_globalMeshGO.GetComponent<MeshRenderer>().enabled = true;
			}
		}
		else if ((bool)_globalMeshGO)
		{
			_globalMeshGO.GetComponent<MeshRenderer>().enabled = false;
		}
	}

	public void ToggleGlobalMeshCollisions(bool isOn)
	{
		if (!_globalMeshAnchor)
		{
			Debug.Log("[ToggleGlobalMeshCollisions] No global mesh anchor found in the scene.");
		}
		else if (isOn)
		{
			if (_roomHasChanged || !_globalMeshCollider)
			{
				if ((bool)_globalMeshCollider)
				{
					UnityEngine.Object.DestroyImmediate(_globalMeshCollider);
				}
				GameObject gameObject = new GameObject("_globalMeshCollider");
				gameObject.transform.SetParent(_globalMeshAnchor.transform, worldPositionStays: false);
				_globalMeshCollider = gameObject.AddComponent<MeshCollider>();
				_globalMeshCollider.sharedMesh = _globalMeshAnchor.GlobalMesh;
			}
			else
			{
				_globalMeshCollider.enabled = true;
			}
		}
		else if ((bool)_globalMeshCollider)
		{
			_globalMeshCollider.enabled = false;
		}
	}

	private void InstantiateGlobalMesh(Action<GameObject, Mesh> onMeshSegmentInstantiated)
	{
		Mesh mesh = Utilities.AddBarycentricCoordinatesToMesh(_globalMeshAnchor.Mesh);
		_globalMeshGO = new GameObject("_globalMeshViz");
		_globalMeshGO.transform.SetParent(MRUK.Instance.GetCurrentRoom().GlobalMeshAnchor.transform, worldPositionStays: false);
		_globalMeshGO.AddComponent<MeshFilter>().mesh = mesh;
		onMeshSegmentInstantiated?.Invoke(_globalMeshGO, mesh);
	}

	public void ExportJSON()
	{
		string text = "";
		try
		{
			string contents = MRUK.Instance.SaveSceneToJsonString(exportGlobalMeshJSON);
			string path = "MRUK_Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
			text = Path.Combine(Application.persistentDataPath, path);
			File.WriteAllText(text, contents);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not save Scene JSON to " + text + ". Exception: " + ex.Message);
			return;
		}
		Debug.Log("Saved Scene JSON to " + text);
	}

	public void DisplayNavMesh(bool isOn)
	{
		if (isOn)
		{
			NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
			if (navMeshTriangulation.areas.Length != 0 || !_navMeshTriangulation.Equals(navMeshTriangulation))
			{
				MeshRenderer meshRenderer;
				MeshFilter meshFilter;
				if (!_navMeshViz)
				{
					_navMeshViz = new GameObject("_navMeshViz");
					meshRenderer = _navMeshViz.AddComponent<MeshRenderer>();
					meshFilter = _navMeshViz.AddComponent<MeshFilter>();
				}
				else
				{
					meshRenderer = _navMeshViz.GetComponent<MeshRenderer>();
					meshFilter = _navMeshViz.GetComponent<MeshFilter>();
					UnityEngine.Object.DestroyImmediate(meshFilter.mesh);
					meshFilter.mesh = null;
				}
				Mesh mesh = new Mesh
				{
					indexFormat = ((navMeshTriangulation.indices.Length > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16)
				};
				mesh.SetVertices(navMeshTriangulation.vertices);
				mesh.SetIndices(navMeshTriangulation.indices, MeshTopology.Triangles, 0);
				meshRenderer.material = _navMeshMaterial;
				meshFilter.mesh = mesh;
				_navMeshTriangulation = navMeshTriangulation;
			}
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(_navMeshViz);
		}
	}

	private string ShowRoomDetails()
	{
		string arg = "N/A";
		int num = 0;
		int num2 = 0;
		if ((bool)MRUK.Instance)
		{
			num2 = ((MRUK.Instance.Rooms != null) ? MRUK.Instance.Rooms.Count : 0);
			if ((bool)MRUK.Instance.GetCurrentRoom())
			{
				arg = ((MRUK.Instance.GetCurrentRoom() != null) ? MRUK.Instance.GetCurrentRoom().name : "N/A");
				num = ((MRUK.Instance.GetCurrentRoom().Anchors != null) ? MRUK.Instance.GetCurrentRoom().Anchors.Count : 0);
			}
		}
		return $"Room Details: Number of rooms: {num2}; Current room: {arg}; Number room anchors:{num}";
	}

	private GameObject GenerateDebugAnchor(MRUKAnchor anchor)
	{
		Vector3 position = anchor.transform.position;
		Quaternion rotation = anchor.transform.rotation;
		Vector3 localScale;
		if (anchor.VolumeBounds.HasValue)
		{
			CreateDebugPrefabSource(anchor);
			Bounds value = anchor.VolumeBounds.Value;
			localScale = value.size;
			position += rotation * value.center;
		}
		else
		{
			CreateDebugPrefabSource(anchor);
			localScale = Vector3.zero;
			if (anchor.PlaneRect.HasValue)
			{
				Vector2 size = anchor.PlaneRect.Value.size;
				localScale = new Vector3(size.x, size.y, 1f);
			}
		}
		_debugAnchor.transform.position = position;
		_debugAnchor.transform.rotation = rotation;
		ScaleChildren(_debugAnchor.transform, localScale);
		_debugAnchor.transform.parent = null;
		_debugAnchor.SetActive(value: true);
		return _debugAnchor;
	}

	private void ScaleChildren(Transform parent, Vector3 localScale)
	{
		foreach (Transform item in parent)
		{
			item.localScale = localScale;
		}
	}

	private void CreateDebugPrefabSource(MRUKAnchor anchor)
	{
		string text = ((!anchor.VolumeBounds.HasValue) ? "PlanePrefab" : "VolumePrefab");
		_debugAnchor = new GameObject(text);
		GameObject obj = new GameObject("MeshParent");
		obj.transform.SetParent(_debugAnchor.transform);
		obj.SetActive(value: false);
		GameObject gameObject = new GameObject("Pivot");
		gameObject.transform.SetParent(_debugAnchor.transform);
		if (anchor.VolumeBounds.HasValue)
		{
			CreateGridPattern(gameObject.transform, new Vector3(0f, 0f, 0.5f), Quaternion.identity);
			CreateGridPattern(gameObject.transform, new Vector3(0f, 0f, -0.5f), Quaternion.Euler(180f, 0f, 0f));
			CreateGridPattern(gameObject.transform, new Vector3(0f, 0.5f, 0f), Quaternion.Euler(-90f, 0f, 0f));
			CreateGridPattern(gameObject.transform, new Vector3(0f, -0.5f, 0f), Quaternion.Euler(90f, 0f, 0f));
			CreateGridPattern(gameObject.transform, new Vector3(-0.5f, 0f, 0f), Quaternion.Euler(0f, -90f, 90f));
			CreateGridPattern(gameObject.transform, new Vector3(0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 90f));
		}
		else
		{
			CreateGridPattern(gameObject.transform, Vector3.zero, Quaternion.identity);
		}
	}

	private void CreateGridPattern(Transform parentTransform, Vector3 localOffset, Quaternion localRotation)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
		gameObject.name = "Checker";
		gameObject.transform.SetParent(parentTransform, worldPositionStays: false);
		gameObject.transform.localPosition = localOffset;
		gameObject.transform.localRotation = localRotation;
		gameObject.transform.localScale = Vector3.one;
		UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<Collider>());
		if (_debugCheckerMesh == null)
		{
			_debugCheckerMesh = new Mesh();
			float num = 0.1f;
			float num2 = -0.5f;
			float num3 = -0.5f;
			int num4 = 50 * 4;
			int num5 = 50 * 6;
			Vector3[] array = new Vector3[num4];
			Vector2[] array2 = new Vector2[num4];
			Color32[] array3 = new Color32[num4];
			Vector3[] array4 = new Vector3[num4];
			Vector4[] array5 = new Vector4[num4];
			int[] array6 = new int[num5];
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			for (int i = 0; i < 10; i++)
			{
				bool flag = i % 2 == 0;
				for (int j = 0; j < 10; j++)
				{
					if (flag)
					{
						for (int k = 0; k < 4; k++)
						{
							Vector3 vector = new Vector3(num2 + (float)i * num, num3 + (float)j * num, 0.001f);
							switch (k)
							{
							case 1:
								vector += new Vector3(0f, num, 0f);
								break;
							case 2:
								vector += new Vector3(num, num, 0f);
								break;
							case 3:
								vector += new Vector3(num, 0f, 0f);
								break;
							}
							array[num6] = vector;
							array2[num6] = Vector2.zero;
							array3[num6] = Color.black;
							array4[num6] = Vector3.forward;
							array5[num6] = Vector3.right;
							num6++;
						}
						int num9 = num8 * 4;
						array6[num7++] = num9;
						array6[num7++] = num9 + 2;
						array6[num7++] = num9 + 1;
						array6[num7++] = num9;
						array6[num7++] = num9 + 3;
						array6[num7++] = num9 + 2;
						num8++;
					}
					flag = !flag;
				}
			}
			_debugCheckerMesh.Clear();
			_debugCheckerMesh.name = "CheckerMesh";
			_debugCheckerMesh.vertices = array;
			_debugCheckerMesh.uv = array2;
			_debugCheckerMesh.colors32 = array3;
			_debugCheckerMesh.triangles = array6;
			_debugCheckerMesh.normals = array4;
			_debugCheckerMesh.tangents = array5;
			_debugCheckerMesh.RecalculateNormals();
			_debugCheckerMesh.RecalculateTangents();
		}
		gameObject.GetComponent<MeshFilter>().mesh = _debugCheckerMesh;
		gameObject.GetComponent<MeshRenderer>().material = _checkerMeshMaterial;
	}

	private void SetupCheckerMeshMaterial(Shader debugShader)
	{
		_checkerMeshMaterial = new Material(debugShader);
		_checkerMeshMaterial.SetOverrideTag("RenderType", "Transparent");
		_checkerMeshMaterial.SetInt(_srcBlend, 5);
		_checkerMeshMaterial.SetInt(_dstBlend, 1);
		_checkerMeshMaterial.SetInt(_zWrite, 0);
		_checkerMeshMaterial.SetInt(_cull, 2);
		_checkerMeshMaterial.DisableKeyword("_ALPHATEST_ON");
		_checkerMeshMaterial.EnableKeyword("_ALPHABLEND_ON");
		_checkerMeshMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		_checkerMeshMaterial.renderQueue = 3000;
	}

	private void CreateDebugPrimitives()
	{
		_debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_debugCube.name = "SceneDebugger_Cube";
		Renderer component = _debugCube.GetComponent<Renderer>();
		if ((bool)component)
		{
			component.material = _debugMaterial;
			component.shadowCastingMode = ShadowCastingMode.Off;
			component.receiveShadows = false;
		}
		_debugCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		_debugCube.GetComponent<Collider>().enabled = false;
		_debugCube.SetActive(value: false);
		_debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_debugSphere.name = "SceneDebugger_Sphere";
		Renderer component2 = _debugSphere.GetComponent<Renderer>();
		if ((bool)component2)
		{
			component2.material = _debugMaterial;
			component2.shadowCastingMode = ShadowCastingMode.Off;
			component2.receiveShadows = false;
		}
		_debugSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		_debugSphere.GetComponent<Collider>().enabled = false;
		_debugSphere.SetActive(value: false);
		_debugNormal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		_debugNormal.name = "SceneDebugger_Normal";
		Renderer component3 = _debugNormal.GetComponent<Renderer>();
		if ((bool)component3)
		{
			component3.material = _debugMaterial;
			component3.shadowCastingMode = ShadowCastingMode.Off;
			component3.receiveShadows = false;
		}
		_debugNormal.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
		_debugNormal.GetComponent<Collider>().enabled = false;
		_debugNormal.SetActive(value: false);
	}

	private void ShowHitNormal(Vector3 position, Vector3 normal)
	{
		if (!(_debugNormal == null))
		{
			if (position != Vector3.zero && normal != Vector3.zero)
			{
				_debugNormal.SetActive(value: true);
				_debugNormal.transform.rotation = Quaternion.FromToRotation(-Vector3.up, normal);
				_debugNormal.transform.position = position + -_debugNormal.transform.up * _debugNormal.transform.localScale.y;
			}
			else
			{
				_debugNormal.SetActive(value: false);
			}
		}
	}
}
