using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Util;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Meta.XR.MRUtilityKit;

[Obsolete("This component is deprecated.Please use the Immersive Debugger fromMeta > Tools > Immersive Debugger")]
[Feature(Feature.Scene)]
public class SceneDebugger : MonoBehaviour
{
	[Tooltip("Material used for visual helpers in debugging")]
	public Material visualHelperMaterial;

	[Tooltip("Visualize anchors")]
	public bool ShowDebugAnchors;

	[Tooltip("On start, place the canvas in front of the user")]
	public bool MoveCanvasInFrontOfCamera = true;

	[Tooltip("When false, use the interaction system already present in the scene")]
	public bool SetupInteractions;

	[Tooltip(" Text field for displaying logs")]
	public TextMeshProUGUI logs;

	[Tooltip("Dropdown to select what surface types to debug")]
	public TMP_Dropdown surfaceTypeDropdown;

	[Tooltip("Dropdown to select whether to export the global mesh with the scene JSON")]
	public TMP_Dropdown exportGlobalMeshJSONDropdown;

	[Tooltip("Dropdown to select what positioning methods to debug")]
	public TMP_Dropdown positioningMethodDropdown;

	[Tooltip("Text field for displaying room details")]
	public TextMeshProUGUI RoomDetails;

	[Tooltip("List of navigable tabs representing sub menus accessible from the top of the debug menu")]
	public List<Image> Tabs = new List<Image>();

	[Tooltip("List of canvas groups for different menus")]
	public List<CanvasGroup> Menus = new List<CanvasGroup>();

	[Tooltip("Helper for ray interactions")]
	public OVRRayHelper RayHelper;

	[Tooltip("Input module for handling VR input")]
	public OVRInputModule InputModule;

	[Tooltip("Raycaster for handling ray interactions")]
	public OVRRaycaster Raycaster;

	[Tooltip("Gaze pointer for VR interactions")]
	public OVRGazePointer GazePointer;

	private readonly Color _foregroundColor = new Color(0.2039f, 0.2549f, 0.2941f, 1f);

	private readonly Color _backgroundColor = new Color(0.11176f, 0.1568f, 0.1843f, 1f);

	private readonly int _srcBlend = Shader.PropertyToID("_SrcBlend");

	private readonly int _dstBlend = Shader.PropertyToID("_DstBlend");

	private readonly int _zWrite = Shader.PropertyToID("_ZWrite");

	private readonly int _cull = Shader.PropertyToID("_Cull");

	private readonly int _color = Shader.PropertyToID("_Color");

	private readonly List<GameObject> _debugAnchors = new List<GameObject>();

	private GameObject _globalMeshGO;

	private Material _debugMaterial;

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

	private Action _debugAction;

	private Canvas _canvas;

	private const float _spawnDistanceFromCamera = 0.75f;

	private SpaceMapGPU _spaceMapGPU;

	private MeshCollider _globalMeshCollider;

	private Material _navMeshMaterial;

	private Material _checkerMeshMaterial;

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

	private void Awake()
	{
		_cameraRig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
		_canvas = GetComponentInChildren<Canvas>();
		if (SetupInteractions)
		{
			SetupInteractionDependencies();
		}
	}

	private void Start()
	{
		MRUK.Instance?.RegisterSceneLoadedCallback(OnSceneLoaded);
		OVRTelemetry.Start(651897568, 0, -1L).Send();
		_currentRoom = MRUK.Instance?.GetCurrentRoom();
		_spaceMapGPU = GetSpaceMapGPU();
		if (MoveCanvasInFrontOfCamera)
		{
			StartCoroutine(SnapCanvasInFrontOfCamera());
		}
		if (_spaceMapGPU == null)
		{
			Menus[0].transform.FindChildRecursive("SpaceMapGPU").gameObject.SetActive(value: false);
		}
		Shader shader = Shader.Find("Meta/Lit");
		_debugMaterial = new Material(shader)
		{
			color = Color.green
		};
		_navMeshMaterial = new Material(shader)
		{
			color = Color.cyan
		};
		SetupCheckerMeshMaterial(shader);
		CreateDebugPrimitives();
	}

	private void Update()
	{
		_debugAction?.Invoke();
		if (ShowDebugAnchors != _previousShowDebugAnchors)
		{
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
				_previousShowDebugAnchors = ShowDebugAnchors;
			}
		}
		if (OVRInput.GetDown(OVRInput.RawButton.Start))
		{
			ToggleMenu(!_canvas.gameObject.activeInHierarchy);
		}
		Billboard();
	}

	private void OnDisable()
	{
		_debugAction = null;
	}

	private void OnSceneLoaded()
	{
		if ((bool)MRUK.Instance && (bool)MRUK.Instance.GetCurrentRoom() && !_globalMeshAnchor)
		{
			_globalMeshAnchor = MRUK.Instance.GetCurrentRoom().GlobalMeshAnchor;
		}
	}

	private void SetupInteractionDependencies()
	{
		if (!_cameraRig)
		{
			return;
		}
		GazePointer.rayTransform = _cameraRig.centerEyeAnchor;
		InputModule.rayTransform = _cameraRig.rightControllerAnchor;
		Raycaster.pointer = _cameraRig.rightControllerAnchor.gameObject;
		if (_cameraRig.GetComponentsInChildren<OVRRayHelper>(includeInactive: false).Length == 0)
		{
			OVRControllerHelper componentInChildren = _cameraRig.rightControllerAnchor.GetComponentInChildren<OVRControllerHelper>();
			if ((bool)componentInChildren)
			{
				componentInChildren.RayHelper = UnityEngine.Object.Instantiate(RayHelper, Vector3.zero, Quaternion.identity, componentInChildren.transform);
				componentInChildren.RayHelper.gameObject.SetActive(value: true);
			}
			OVRControllerHelper componentInChildren2 = _cameraRig.leftControllerAnchor.GetComponentInChildren<OVRControllerHelper>();
			if ((bool)componentInChildren2)
			{
				componentInChildren2.RayHelper = UnityEngine.Object.Instantiate(RayHelper, Vector3.zero, Quaternion.identity, componentInChildren2.transform);
				componentInChildren2.RayHelper.gameObject.SetActive(value: true);
			}
			OVRHand[] componentsInChildren = _cameraRig.GetComponentsInChildren<OVRHand>();
			foreach (OVRHand obj in componentsInChildren)
			{
				obj.RayHelper = UnityEngine.Object.Instantiate(RayHelper, Vector3.zero, Quaternion.identity, _cameraRig.trackingSpace);
				obj.RayHelper.gameObject.SetActive(value: true);
			}
		}
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

	public void ShowRoomDetailsDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = (Action)Delegate.Combine(_debugAction, new Action(ShowRoomDetails));
			}
			else
			{
				_debugAction = (Action)Delegate.Remove(_debugAction, new Action(ShowRoomDetails));
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "ShowRoomDetailsDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void GetKeyWallDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				Vector2 wallScale = Vector2.zero;
				MRUKAnchor mRUKAnchor = MRUK.Instance?.GetCurrentRoom()?.GetKeyWall(out wallScale);
				if (mRUKAnchor != null && _debugCube != null)
				{
					_debugCube.transform.localScale = new Vector3(wallScale.x, wallScale.y, 0.05f);
					_debugCube.transform.position = mRUKAnchor.transform.position;
					_debugCube.transform.rotation = mRUKAnchor.transform.rotation;
				}
				SetLogsText("\n[{0}]\nSize: {1}", "GetKeyWallDebugger", wallScale);
			}
			if (_debugCube != null)
			{
				_debugCube.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "GetKeyWallDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void GetLargestSurfaceDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				MRUKAnchor.SceneLabels sceneLabels = MRUKAnchor.SceneLabels.TABLE;
				if ((bool)surfaceTypeDropdown)
				{
					sceneLabels = Utilities.StringLabelToEnum(surfaceTypeDropdown.options[surfaceTypeDropdown.value].text.ToUpper());
				}
				MRUKAnchor mRUKAnchor = MRUK.Instance?.GetCurrentRoom()?.FindLargestSurface(sceneLabels);
				if (!(mRUKAnchor != null))
				{
					SetLogsText("\n[{0}]\n No surface of type {1} found.", "GetLargestSurfaceDebugger", sceneLabels);
					_debugCube.SetActive(value: false);
					return;
				}
				if (_debugCube != null)
				{
					Vector3 vector = (mRUKAnchor.PlaneRect.HasValue ? new Vector3(mRUKAnchor.PlaneRect.Value.width, mRUKAnchor.PlaneRect.Value.height, 0.01f) : mRUKAnchor.VolumeBounds.Value.size);
					_debugCube.transform.localScale = vector + new Vector3(0.01f, 0.01f, 0.01f);
					_debugCube.transform.position = (mRUKAnchor.PlaneRect.HasValue ? mRUKAnchor.transform.position : mRUKAnchor.transform.TransformPoint(mRUKAnchor.VolumeBounds.Value.center));
					_debugCube.transform.rotation = mRUKAnchor.transform.rotation;
				}
			}
			else
			{
				_debugAction = null;
			}
			if (_debugCube != null)
			{
				_debugCube.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "GetLargestSurfaceDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void GetClosestSeatPoseDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
				{
					MRUKAnchor couch = null;
					Pose seatPose = default(Pose);
					Ray controllerRay = GetControllerRay();
					MRUK.Instance?.GetCurrentRoom()?.TryGetClosestSeatPose(controllerRay, out seatPose, out couch);
					if ((bool)couch)
					{
						if (_debugCube != null)
						{
							_debugCube.transform.localRotation = couch.transform.localRotation;
							_debugCube.transform.position = seatPose.position;
							_debugCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						}
						SetLogsText("\n[{0}]\nSeat: {1}\nPosition: {2}\nDistance: {3}", "GetClosestSeatPoseDebugger", couch.name, seatPose.position, Vector3.Distance(seatPose.position, controllerRay.origin).ToString("0.##"));
					}
					else
					{
						SetLogsText("\n[{0}]\n No seat found in the scene.", "GetClosestSeatPoseDebugger");
					}
				};
			}
			else
			{
				_debugAction = null;
			}
			if (_debugCube != null)
			{
				_debugCube.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "GetClosestSeatPoseDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void GetClosestSurfacePositionDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
				{
					Vector3 origin = GetControllerRay().origin;
					Vector3 surfacePosition = Vector3.zero;
					Vector3 normal = Vector3.up;
					MRUKAnchor closestAnchor = null;
					MRUK.Instance?.GetCurrentRoom()?.TryGetClosestSurfacePosition(origin, out surfacePosition, out closestAnchor, out normal);
					ShowHitNormal(surfacePosition, normal);
					if (closestAnchor != null)
					{
						SetLogsText("\n[{0}]\nAnchor: {1}\nSurface Position: {2}\nDistance: {3}", "GetClosestSurfacePositionDebugger", closestAnchor.name, surfacePosition, Vector3.Distance(origin, surfacePosition).ToString("0.##"));
					}
				};
			}
			else
			{
				_debugAction = null;
			}
			if (_debugNormal != null)
			{
				_debugNormal.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "GetClosestSurfacePositionDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void GetBestPoseFromRaycastDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
				{
					Ray controllerRay = GetControllerRay();
					MRUKAnchor sceneAnchor = null;
					MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT;
					if ((bool)positioningMethodDropdown)
					{
						positioningMethod = (MRUK.PositioningMethod)positioningMethodDropdown.value;
					}
					Pose? pose = MRUK.Instance?.GetCurrentRoom()?.GetBestPoseFromRaycast(controllerRay, float.PositiveInfinity, default(LabelFilter), out sceneAnchor, positioningMethod);
					if (pose.HasValue && (bool)sceneAnchor && (bool)_debugCube)
					{
						_debugCube.transform.position = pose.Value.position;
						_debugCube.transform.rotation = pose.Value.rotation;
						_debugCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						SetLogsText("\n[{0}]\nAnchor: {1}\nPose Position: {2}\nPose Rotation: {3}", "GetBestPoseFromRaycastDebugger", sceneAnchor.name, pose.Value.position, pose.Value.rotation);
					}
				};
			}
			else
			{
				_debugAction = null;
			}
			if (_debugCube != null)
			{
				_debugCube.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "GetBestPoseFromRaycastDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void RayCastDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
				{
					Ray controllerRay = GetControllerRay();
					RaycastHit hit = default(RaycastHit);
					MRUKAnchor anchor = null;
					MRUK.Instance?.GetCurrentRoom()?.Raycast(controllerRay, float.PositiveInfinity, out hit, out anchor);
					ShowHitNormal(hit.point, hit.normal);
					if (anchor != null)
					{
						SetLogsText("\n[{0}]\nAnchor: {1}\nHit point: {2}\nHit normal: {3}\n", "RayCastDebugger", anchor.name, hit.point, hit.normal);
					}
				};
			}
			else
			{
				_debugAction = null;
			}
			if (_debugNormal != null)
			{
				_debugNormal.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "RayCastDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void IsPositionInRoomDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
				{
					Ray controllerRay = GetControllerRay();
					if (_debugSphere != null)
					{
						bool? flag = MRUK.Instance?.GetCurrentRoom()?.IsPositionInRoom(_debugSphere.transform.position);
						_debugSphere.transform.position = controllerRay.GetPoint(0.2f);
						_debugSphere.GetComponent<Renderer>().material.color = ((flag.HasValue && flag.Value) ? Color.green : Color.red);
						SetLogsText("\n[{0}]\nPosition: {1}\nIs inside the Room: {2}\n", "IsPositionInRoomDebugger", _debugSphere.transform.position, flag);
					}
				};
			}
			if (_debugSphere != null)
			{
				_debugSphere.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "IsPositionInRoomDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void ShowDebugAnchorsDebugger(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
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
					SetLogsText("\n[{0}]\nHit point: {1}\nHit normal: {2}\n", "ShowDebugAnchorsDebugger", hit.point, hit.normal);
				};
			}
			else
			{
				_debugAction = null;
				UnityEngine.Object.Destroy(_debugAnchor);
				_debugAnchor = null;
			}
			if (_debugNormal != null)
			{
				_debugNormal.SetActive(isOn);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "ShowDebugAnchorsDebugger", ex.Message, ex.StackTrace);
		}
	}

	public void DisplayGlobalMesh(bool isOn)
	{
		try
		{
			if (!_globalMeshAnchor)
			{
				SetLogsText("\n[{0}]\nNo global mesh anchor found in the scene.\n", "DisplayGlobalMesh");
			}
			else if (isOn)
			{
				if (_roomHasChanged || !_globalMeshGO)
				{
					if ((bool)_globalMeshGO)
					{
						UnityEngine.Object.DestroyImmediate(_globalMeshGO);
					}
					InstantiateGlobalMesh(delegate(GameObject globalMeshSegmentGO, Mesh _)
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
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "DisplayGlobalMesh", ex.Message, ex.StackTrace);
		}
	}

	public void ToggleGlobalMeshCollisions(bool isOn)
	{
		try
		{
			if (!_globalMeshAnchor)
			{
				SetLogsText("\n[{0}]\nNo global mesh anchor found in the scene.\n", "ToggleGlobalMeshCollisions");
			}
			else if (isOn)
			{
				if (_roomHasChanged || !_globalMeshCollider)
				{
					GameObject gameObject = new GameObject("_globalMeshCollider");
					gameObject.transform.SetParent(_globalMeshAnchor.transform, worldPositionStays: false);
					_globalMeshCollider = gameObject.AddComponent<MeshCollider>();
					_globalMeshCollider.sharedMesh = _globalMeshAnchor.Mesh;
				}
				else
				{
					_globalMeshCollider.enabled = true;
				}
			}
			else
			{
				_globalMeshCollider.enabled = false;
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "ToggleGlobalMeshCollisions", ex.Message, ex.StackTrace);
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

	public void ExportJSON(bool isOn)
	{
		try
		{
			if (isOn)
			{
				bool includeGlobalMesh = true;
				if ((bool)exportGlobalMeshJSONDropdown)
				{
					includeGlobalMesh = exportGlobalMeshJSONDropdown.options[exportGlobalMeshJSONDropdown.value].text.ToLower() == "true";
				}
				string contents = MRUK.Instance.SaveSceneToJsonString(includeGlobalMesh);
				string path = "MRUK_Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
				string text = Path.Combine(Application.persistentDataPath, path);
				File.WriteAllText(text, contents);
				Debug.Log("Saved Scene JSON to " + text);
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "ExportJSON", ex.Message, ex.StackTrace);
		}
	}

	public static void DebugDestructibleMeshComponent(DestructibleMeshComponent destructibleMeshComponent)
	{
		if (destructibleMeshComponent == null)
		{
			throw new Exception("Can not debug a null DestructibleMeshComponent.");
		}
		destructibleMeshComponent.DebugDestructibleMeshComponent();
	}

	public void DisplaySpaceMap(bool isOn)
	{
	}

	public void DisplayNavMesh(bool isOn)
	{
		try
		{
			if (isOn)
			{
				_debugAction = delegate
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
				};
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_navMeshViz);
				_debugAction = null;
			}
		}
		catch (Exception ex)
		{
			SetLogsText("\n[{0}]\n {1}\n{2}", "DisplayNavMesh", ex.Message, ex.StackTrace);
		}
	}

	private SpaceMapGPU GetSpaceMapGPU()
	{
		SpaceMapGPU[] array = UnityEngine.Object.FindObjectsByType<SpaceMapGPU>(FindObjectsSortMode.None);
		if (array.Length == 0)
		{
			return null;
		}
		return array[0];
	}

	private void ShowRoomDetails()
	{
		string arg = MRUK.Instance?.GetCurrentRoom()?.name ?? "N/A";
		int num = MRUK.Instance?.Rooms.Count ?? 0;
		RoomDetails.text = string.Format("\n[{0}]\nNumber of rooms: {1}\nCurrent room: {2}", "ShowRoomDetailsDebugger", num, arg);
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

	private void CreateDebugPrimitives()
	{
		_debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_debugCube.name = "SceneDebugger_Cube";
		_debugCube.GetComponent<Renderer>().material = _debugMaterial;
		_debugCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		_debugCube.GetComponent<Collider>().enabled = false;
		_debugCube.SetActive(value: false);
		_debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		_debugSphere.name = "SceneDebugger_Sphere";
		_debugSphere.GetComponent<Renderer>().material = _debugMaterial;
		_debugSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		_debugSphere.GetComponent<Collider>().enabled = false;
		_debugSphere.SetActive(value: false);
		_debugNormal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		_debugNormal.name = "SceneDebugger_Normal";
		_debugNormal.GetComponent<Renderer>().material = _debugMaterial;
		_debugNormal.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
		_debugNormal.GetComponent<Collider>().enabled = false;
		_debugNormal.SetActive(value: false);
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

	private void ShowHitNormal(Vector3 position, Vector3 normal)
	{
		if (_debugNormal != null && position != Vector3.zero && normal != Vector3.zero)
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

	private void SetLogsText(string logsText, params object[] args)
	{
		if ((bool)logs)
		{
			logs.text = string.Format(logsText, args);
		}
	}

	public void ActivateTab(Image selectedTab)
	{
		foreach (Image tab in Tabs)
		{
			tab.color = _backgroundColor;
		}
		selectedTab.color = _foregroundColor;
	}

	public void ActivateMenu(CanvasGroup menuToActivate)
	{
		foreach (CanvasGroup menu in Menus)
		{
			ToggleCanvasGroup(menu, shouldShow: false);
		}
		ToggleCanvasGroup(menuToActivate, shouldShow: true);
	}

	private void ToggleCanvasGroup(CanvasGroup canvasGroup, bool shouldShow)
	{
		canvasGroup.interactable = shouldShow;
		canvasGroup.alpha = (shouldShow ? 1f : 0f);
		canvasGroup.blocksRaycasts = shouldShow;
	}

	private void Billboard()
	{
		if ((bool)_canvas)
		{
			Vector3 forward = _canvas.transform.position - _cameraRig.centerEyeAnchor.transform.position;
			if (forward.sqrMagnitude > 0.01f)
			{
				Quaternion rotation = Quaternion.LookRotation(forward);
				_canvas.transform.rotation = rotation;
			}
		}
	}

	private void ToggleMenu(bool active)
	{
		if ((bool)_canvas)
		{
			_canvas.gameObject.SetActive(active);
			StartCoroutine(SnapCanvasInFrontOfCamera());
		}
	}

	private IEnumerator SnapCanvasInFrontOfCamera()
	{
		yield return new WaitUntil(() => (bool)_cameraRig && _cameraRig.centerEyeAnchor.transform.position != Vector3.zero);
		base.transform.position = _cameraRig.centerEyeAnchor.transform.position + _cameraRig.centerEyeAnchor.transform.forward * 0.75f;
	}
}
