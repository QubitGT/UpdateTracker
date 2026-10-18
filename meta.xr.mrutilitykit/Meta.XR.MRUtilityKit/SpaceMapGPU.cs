using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Meta.XR.MRUtilityKit;

public class SpaceMapGPU : MonoBehaviour
{
	[Tooltip("When the scene data is loaded, this controls what room(s) the spacemap will run on.")]
	[Header("Scene and Room Settings")]
	public MRUK.RoomFilter CreateOnStart = MRUK.RoomFilter.CurrentRoomOnly;

	[Tooltip("If enabled, updates on scene elements such as rooms and anchors will be handled by this class")]
	internal bool TrackUpdates = true;

	[Space]
	[Header("Textures")]
	[SerializeField]
	[Tooltip("Use this dimension for SpaceMap in X and Y")]
	public int TextureDimension = 512;

	[Tooltip("Colorize the SpaceMap with this Gradient")]
	public Gradient MapGradient = new Gradient();

	[Space]
	[Header("SpaceMap Settings")]
	[SerializeField]
	private Material gradientMaterial;

	[SerializeField]
	private ComputeShader CSSpaceMap;

	[Tooltip("Those Labels will be taken into account when running the SpaceMap")]
	[SerializeField]
	private MRUKAnchor.SceneLabels SceneObjectLabels;

	[Tooltip("Set a color for the inside of an Object")]
	[SerializeField]
	private Color InsideObjectColor;

	[Tooltip("Add this to the border of the capture Camera")]
	[SerializeField]
	private float CameraCaptureBorderBuffer = 0.5f;

	[Space]
	[Header("SpaceMap Debug Settings")]
	[SerializeField]
	[Tooltip("This setting affects your performance. If enabled, the TextureMap will be filled with the SpaceMap")]
	private bool CreateOutputTexture;

	[Tooltip("The Spacemap will be rendered into this Texture.")]
	[SerializeField]
	internal Texture2D OutputTexture;

	[Tooltip("Add here a debug plane")]
	[SerializeField]
	private GameObject DebugPlane;

	[SerializeField]
	private bool ShowDebugPlane;

	private Color _colorFloorWall = Color.red;

	private Color _colorSceneObjects = Color.green;

	private Color _colorVirtualObjects = Color.blue;

	private Material _matFloor;

	private Material _matObjects;

	private bool _isOrthoCameraInitialized;

	private Matrix4x4 _orthoCamProjectionMatrix;

	private Matrix4x4 _orthoCamViewMatrix;

	private Matrix4x4 _orthoCamProjectionViewMatrix;

	private Rect _currentRoomBounds;

	private RenderTexture[] _RTextures = new RenderTexture[2];

	private const string OculusUnlitShader = "Oculus/Unlit";

	private Texture2D _gradientTexture;

	private int _csSpaceMapKernel;

	private int _csFillSpaceMapKernel;

	private int _csPrepareSpaceMapKernel;

	private const string SHADER_GLOBAL_SPACEMAPCAMERAMATRIX = "_SpaceMapProjectionViewMatrix";

	private const float CameraDistance = 10f;

	private const float AspectRatio = 1f;

	private const float NearClipPlane = 0.1f;

	private const float FarClipPlane = 100f;

	[SerializeField]
	private RenderTexture RenderTexture;

	private static readonly int WidthID = Shader.PropertyToID("Width");

	private static readonly int HeightID = Shader.PropertyToID("Height");

	private static readonly int ColorFloorWallID = Shader.PropertyToID("ColorFloorWall");

	private static readonly int ColorSceneObjectsID = Shader.PropertyToID("ColorSceneObjects");

	private static readonly int ColorVirtualObjectsID = Shader.PropertyToID("ColorVirtualObjects");

	private static readonly int StepID = Shader.PropertyToID("Step");

	private static readonly int SourceID = Shader.PropertyToID("Source");

	private static readonly int ResultID = Shader.PropertyToID("Result");

	private static readonly int SpaceMapCameraMatrixID = Shader.PropertyToID("_SpaceMapProjectionViewMatrix");

	private Dictionary<MRUKRoom, RenderTexture> _roomTextures = new Dictionary<MRUKRoom, RenderTexture>();

	[field: SerializeField]
	public UnityEvent SpaceMapCreatedEvent { get; private set; } = new UnityEvent();


	public UnityEvent<MRUKRoom> SpaceMapRoomCreatedEvent { get; private set; } = new UnityEvent<MRUKRoom>();


	[field: SerializeField]
	public UnityEvent SpaceMapUpdatedEvent { get; private set; } = new UnityEvent();


	public RenderTexture GetSpaceMap(MRUKRoom room = null)
	{
		if (room == null)
		{
			return RenderTexture;
		}
		if (!_roomTextures.TryGetValue(room, out var value))
		{
			Debug.Log($"Rendertexture for room {room} not found, returning default texture. Call StartSpaceMap(room) to create a texture for a specific room.");
			return RenderTexture;
		}
		return value;
	}

	public void StartSpaceMap(MRUK.RoomFilter roomFilter)
	{
		List<MRUKRoom> rooms;
		switch (roomFilter)
		{
		case MRUK.RoomFilter.None:
			return;
		case MRUK.RoomFilter.CurrentRoomOnly:
			rooms = new List<MRUKRoom> { MRUK.Instance.GetCurrentRoom() };
			break;
		case MRUK.RoomFilter.AllRooms:
			rooms = MRUK.Instance.Rooms;
			break;
		default:
			throw new ArgumentOutOfRangeException("roomFilter", roomFilter, null);
		}
		StartSpaceMapInternal(rooms, RenderTexture);
		SpaceMapCreatedEvent.Invoke();
	}

	public void StartSpaceMap(MRUKRoom room)
	{
		if (!_roomTextures.TryGetValue(room, out var value))
		{
			value = CreateNewRenderTexture(RenderTexture.width);
			_roomTextures[room] = value;
		}
		StartSpaceMapInternal(new List<MRUKRoom> { room }, value);
		SpaceMapRoomCreatedEvent.Invoke(room);
	}

	public Color GetColorAtPosition(Vector3 worldPosition)
	{
		if (_currentRoomBounds.size.x <= 0f)
		{
			return Color.black;
		}
		Vector2 vector = Rect.PointToNormalized(_currentRoomBounds, new Vector2(worldPosition.x, worldPosition.z));
		Color pixelBilinear = OutputTexture.GetPixelBilinear(vector.x, vector.y);
		float num = 1f - pixelBilinear.r;
		if (!(pixelBilinear.b > 0f))
		{
			return MapGradient.Evaluate((num >= 0f && num <= 1f) ? num : 0f);
		}
		return InsideObjectColor;
	}

	private void Awake()
	{
		_csSpaceMapKernel = CSSpaceMap.FindKernel("SpaceMap");
		_csFillSpaceMapKernel = CSSpaceMap.FindKernel("FillSpaceMap");
		_csPrepareSpaceMapKernel = CSSpaceMap.FindKernel("PrepareSpaceMap");
		_matFloor = new Material(Shader.Find("Oculus/Unlit"));
		_matObjects = new Material(Shader.Find("Oculus/Unlit"));
		_matFloor.color = _colorFloorWall;
		_matObjects.color = _colorSceneObjects;
	}

	private void Start()
	{
		InitUpdateGradientTexture();
		ApplyMaterial();
		OVRTelemetry.Start(651896914, 0, -1L).Send();
	}

	private void OnEnable()
	{
		if ((object)MRUK.Instance != null)
		{
			MRUK.Instance.RegisterSceneLoadedCallback(SceneLoaded);
			if (TrackUpdates)
			{
				MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveCreatedRoom);
				MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRemovedRoom);
				MRUK.Instance.RoomUpdatedEvent.AddListener(ReceiveUpdatedRoom);
			}
		}
	}

	private void OnDisable()
	{
		if (!(MRUK.Instance == null))
		{
			MRUK.Instance.SceneLoadedEvent.RemoveListener(SceneLoaded);
			if (TrackUpdates)
			{
				MRUK.Instance.RoomCreatedEvent.RemoveListener(ReceiveCreatedRoom);
				MRUK.Instance.RoomRemovedEvent.RemoveListener(ReceiveRemovedRoom);
				MRUK.Instance.RoomUpdatedEvent.RemoveListener(ReceiveUpdatedRoom);
			}
		}
	}

	private void Update()
	{
		Shader.SetGlobalMatrix(SpaceMapCameraMatrixID, _orthoCamProjectionViewMatrix);
		if (DebugPlane != null && DebugPlane.activeSelf != ShowDebugPlane)
		{
			DebugPlane.SetActive(ShowDebugPlane);
		}
	}

	private void StartSpaceMapInternal(List<MRUKRoom> rooms, RenderTexture rt)
	{
		InitializeOrthoCameraMatrixParameters(GetBoundingBox(rooms));
		UpdateBuffer(rooms, rt);
	}

	private void SceneLoaded()
	{
		if (CreateOnStart != 0)
		{
			StartSpaceMap(CreateOnStart);
		}
	}

	private bool IsInitialized()
	{
		if (_RTextures[0] != null)
		{
			return _isOrthoCameraInitialized;
		}
		return false;
	}

	private void UpdateBuffer(MRUKRoom room)
	{
		if (!_roomTextures.TryGetValue(room, out var value))
		{
			value = CreateNewRenderTexture(RenderTexture.width);
			_roomTextures[room] = value;
		}
		UpdateBuffer(new List<MRUKRoom> { room }, value);
	}

	private void UpdateBuffer(List<MRUKRoom> rooms, RenderTexture rt)
	{
		CommandBuffer commandBuffer = new CommandBuffer
		{
			name = "SpaceMap"
		};
		RenderTexture.active = rt;
		GL.Clear(clearDepth: true, clearColor: true, new Color(1f, 1f, 1f, 1f));
		RenderTexture.active = null;
		commandBuffer.SetRenderTarget(rt);
		int textureDimension = TextureDimension;
		if (_RTextures[0] == null || _RTextures[0].width != textureDimension || _RTextures[0].height != textureDimension)
		{
			TryReleaseRT(_RTextures[0]);
			TryReleaseRT(_RTextures[1]);
			_RTextures[0] = CreateNewRenderTexture(textureDimension);
			_RTextures[1] = CreateNewRenderTexture(textureDimension);
		}
		commandBuffer.SetViewProjectionMatrices(_orthoCamViewMatrix, _orthoCamProjectionMatrix);
		DrawRoomsIntoCB(commandBuffer, rooms);
		Graphics.ExecuteCommandBuffer(commandBuffer);
		RunSpaceMap(rt);
		if (CreateOutputTexture)
		{
			RenderTexture.active = rt;
			OutputTexture.ReadPixels(new Rect(0f, 0f, TextureDimension, TextureDimension), 0, 0);
			OutputTexture.Apply();
			RenderTexture.active = null;
		}
		commandBuffer.Clear();
		commandBuffer.Dispose();
	}

	private void DrawRoomsIntoCB(CommandBuffer commandBuffer, List<MRUKRoom> rooms)
	{
		foreach (MRUKRoom room in rooms)
		{
			Mesh mesh = Utilities.SetupAnchorMeshGeometry(room.FloorAnchor);
			commandBuffer.DrawMesh(mesh, room.FloorAnchor.transform.localToWorldMatrix, _matFloor);
			foreach (MRUKAnchor anchor in room.Anchors)
			{
				if (anchor.HasAnyLabel(SceneObjectLabels))
				{
					Mesh mesh2 = Utilities.SetupAnchorMeshGeometry(anchor);
					commandBuffer.DrawMesh(mesh2, anchor.transform.localToWorldMatrix, _matObjects);
				}
			}
		}
	}

	private void RunSpaceMap(RenderTexture rt)
	{
		CSSpaceMap.SetInt(WidthID, TextureDimension);
		CSSpaceMap.SetInt(HeightID, TextureDimension);
		CSSpaceMap.SetVector(ColorFloorWallID, _colorFloorWall);
		CSSpaceMap.SetVector(ColorSceneObjectsID, _colorSceneObjects);
		CSSpaceMap.SetVector(ColorVirtualObjectsID, _colorVirtualObjects);
		int threadGroupsX = Mathf.CeilToInt((float)TextureDimension / 8f);
		int threadGroupsY = Mathf.CeilToInt((float)TextureDimension / 8f);
		CSSpaceMap.SetTexture(_csPrepareSpaceMapKernel, SourceID, rt);
		CSSpaceMap.SetTexture(_csPrepareSpaceMapKernel, ResultID, _RTextures[0]);
		CSSpaceMap.Dispatch(_csPrepareSpaceMapKernel, threadGroupsX, threadGroupsY, 1);
		int num = (int)Mathf.Log(TextureDimension, 2f);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			int val = (int)Mathf.Pow(2f, num - i - 1);
			num2 = i % 2;
			num3 = (i + 1) % 2;
			CSSpaceMap.SetInt(StepID, val);
			CSSpaceMap.SetTexture(_csSpaceMapKernel, SourceID, _RTextures[num2]);
			CSSpaceMap.SetTexture(_csSpaceMapKernel, ResultID, _RTextures[num3]);
			CSSpaceMap.Dispatch(_csSpaceMapKernel, threadGroupsX, threadGroupsY, 1);
		}
		CSSpaceMap.SetTexture(_csFillSpaceMapKernel, SourceID, _RTextures[num3]);
		CSSpaceMap.SetTexture(_csFillSpaceMapKernel, ResultID, _RTextures[num2]);
		CSSpaceMap.Dispatch(_csFillSpaceMapKernel, threadGroupsX, threadGroupsY, 1);
		Graphics.Blit(_RTextures[num2], rt);
		gradientMaterial.SetTexture("_MainTex", rt);
		SpaceMapUpdatedEvent.Invoke();
	}

	private void ReceiveUpdatedRoom(MRUKRoom room)
	{
		if (TrackUpdates)
		{
			RegisterAnchorUpdates(room);
			if (IsInitialized())
			{
				UpdateBuffer(room);
			}
		}
	}

	private void ReceiveCreatedRoom(MRUKRoom room)
	{
		if (TrackUpdates && CreateOnStart == MRUK.RoomFilter.AllRooms)
		{
			RegisterAnchorUpdates(room);
			if (IsInitialized())
			{
				UpdateBuffer(room);
			}
		}
	}

	private void ReceiveRemovedRoom(MRUKRoom room)
	{
		UnregisterAnchorUpdates(room);
		_roomTextures.Remove(room);
	}

	private void UnregisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.RemoveListener(ReceiveAnchorCreatedEvent);
		room.AnchorRemovedEvent.RemoveListener(ReceiveAnchorRemovedCallback);
		room.AnchorUpdatedEvent.RemoveListener(ReceiveAnchorUpdatedCallback);
	}

	private void RegisterAnchorUpdates(MRUKRoom room)
	{
		room.AnchorCreatedEvent.AddListener(ReceiveAnchorCreatedEvent);
		room.AnchorRemovedEvent.AddListener(ReceiveAnchorRemovedCallback);
		room.AnchorUpdatedEvent.AddListener(ReceiveAnchorUpdatedCallback);
	}

	private void ReceiveAnchorUpdatedCallback(MRUKAnchor anchor)
	{
		if (TrackUpdates && IsInitialized())
		{
			UpdateBuffer(anchor.Room);
		}
	}

	private void ReceiveAnchorRemovedCallback(MRUKAnchor anchor)
	{
		if (IsInitialized())
		{
			UpdateBuffer(anchor.Room);
		}
	}

	private void ReceiveAnchorCreatedEvent(MRUKAnchor anchor)
	{
		if (TrackUpdates && IsInitialized())
		{
			UpdateBuffer(anchor.Room);
		}
	}

	private static RenderTexture CreateNewRenderTexture(int wh)
	{
		RenderTexture renderTexture = new RenderTexture(wh, wh, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		renderTexture.enableRandomWrite = true;
		renderTexture.Create();
		return renderTexture;
	}

	private static void TryReleaseRT(RenderTexture renderTexture)
	{
		if (renderTexture != null)
		{
			renderTexture.Release();
		}
	}

	private void ApplyMaterial()
	{
		gradientMaterial.SetTexture("_GradientTex", _gradientTexture);
		gradientMaterial.SetColor("_InsideColor", InsideObjectColor);
		if (DebugPlane != null)
		{
			DebugPlane.GetComponent<Renderer>().material = gradientMaterial;
		}
	}

	private void InitUpdateGradientTexture()
	{
		if (_gradientTexture == null)
		{
			_gradientTexture = new Texture2D(256, 1, TextureFormat.RGBA32, mipChain: false);
		}
		for (int i = 0; i <= _gradientTexture.width; i++)
		{
			float time = (float)i / ((float)_gradientTexture.width - 1f);
			_gradientTexture.SetPixel(i, 0, MapGradient.Evaluate(time));
		}
		_gradientTexture.Apply();
	}

	private void InitializeOrthoCameraMatrixParameters(Rect roomBounds)
	{
		_currentRoomBounds = roomBounds;
		float size = Mathf.Max(roomBounds.width, roomBounds.height) / 2f;
		_orthoCamProjectionMatrix = CalculateOrthographicProjMatrix(size, 1f, 0.1f, 100f);
		_orthoCamViewMatrix = CalculateViewMatrix();
		_orthoCamProjectionViewMatrix = _orthoCamProjectionMatrix * _orthoCamViewMatrix;
		_isOrthoCameraInitialized = true;
		HandleDebugPlane(roomBounds);
	}

	private Matrix4x4 CalculateOrthographicProjMatrix(float size, float aspect, float near, float far)
	{
		float num = size * aspect;
		float left = 0f - num;
		float bottom = 0f - size;
		return Matrix4x4.Ortho(left, num, bottom, size, near, far);
	}

	private Matrix4x4 CalculateViewMatrix()
	{
		return Matrix4x4.Inverse(Matrix4x4.TRS(new Vector3(_currentRoomBounds.center.x, 10f, _currentRoomBounds.center.y), Quaternion.Euler(90f, 0f, 0f), new Vector3(1f, 1f, -1f)));
	}

	private Rect GetBoundingBox(List<MRUKRoom> rooms)
	{
		Bounds bounds = default(Bounds);
		foreach (MRUKRoom room in rooms)
		{
			if (bounds.extents != Vector3.zero)
			{
				bounds.Encapsulate(room.GetRoomBounds());
			}
			else
			{
				bounds = room.GetRoomBounds();
			}
		}
		bounds.Expand(CameraCaptureBorderBuffer);
		return Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
	}

	private void HandleDebugPlane(Rect rect)
	{
		if (!(DebugPlane == null))
		{
			float num = rect.size.x / 10f;
			float num2 = rect.size.y / 10f;
			float x = rect.center.x;
			float y = rect.center.y;
			if (!float.IsNaN(x) && !float.IsNaN(y) && num != float.NegativeInfinity && num2 != float.NegativeInfinity)
			{
				DebugPlane.transform.localScale = new Vector3(num, 1f, num2);
				DebugPlane.transform.position = new Vector3(x, DebugPlane.transform.position.y, y);
			}
		}
	}
}
