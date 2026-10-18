using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

internal static class MRUKNativeFuncs
{
	public enum MrukSceneModel
	{
		V2FallbackV1,
		V1,
		V2
	}

	public enum MrukLogLevel
	{
		Debug,
		Info,
		Warn,
		Error
	}

	public enum MrukResult
	{
		Success,
		ErrorInvalidArgs,
		ErrorUnknown,
		ErrorInternal,
		ErrorDiscoveryOngoing,
		ErrorInvalidJson,
		ErrorNoRoomsFound,
		ErrorInsufficientResources,
		ErrorStorageAtCapacity,
		ErrorInsufficientView,
		ErrorPermissionInsufficient,
		ErrorRateLimited,
		ErrorTooDark,
		ErrorTooBright
	}

	public enum MrukSurfaceType
	{
		None = 0,
		Plane = 1,
		Volume = 2,
		Mesh = 4,
		All = 7
	}

	public enum MrukLabel
	{
		Floor = 1,
		Ceiling = 2,
		WallFace = 4,
		Table = 8,
		Couch = 0x10,
		DoorFrame = 0x20,
		WindowFrame = 0x40,
		Other = 0x80,
		Storage = 0x100,
		Bed = 0x200,
		Screen = 0x400,
		Lamp = 0x800,
		Plant = 0x1000,
		WallArt = 0x2000,
		SceneMesh = 0x4000,
		InvisibleWallFace = 0x8000,
		Unknown = 0x20000,
		InnerWallFace = 0x40000,
		Tabletop = 0x80000,
		SittingArea = 0x100000,
		SleepingArea = 0x200000,
		StorageTop = 0x400000
	}

	public enum MrukEnvironmentRaycastStatus
	{
		Hit = 1,
		NoHit = 2,
		HitPointOccluded = 3,
		HitPointOutsideFov = 4,
		RayOccluded = 5,
		InvalidOrientation = 6,
		Max = int.MaxValue
	}

	public unsafe delegate void LogPrinter(MrukLogLevel logLevel, char* message, uint length);

	public delegate void MrukOnPreRoomAnchorAdded(ref MrukRoomAnchor roomAnchor, IntPtr userContext);

	public delegate void MrukOnRoomAnchorAdded(ref MrukRoomAnchor roomAnchor, IntPtr userContext);

	public delegate void MrukOnRoomAnchorUpdated(ref MrukRoomAnchor roomAnchor, ref Guid oldRoomAnchorUuid, [MarshalAs(UnmanagedType.U1)] bool significantChange, IntPtr userContext);

	public delegate void MrukOnRoomAnchorRemoved(ref MrukRoomAnchor roomAnchor, IntPtr userContext);

	public delegate void MrukOnSceneAnchorAdded(ref MrukSceneAnchor sceneAnchor, IntPtr userContext);

	public delegate void MrukOnSceneAnchorUpdated(ref MrukSceneAnchor sceneAnchor, [MarshalAs(UnmanagedType.U1)] bool significantChange, IntPtr userContext);

	public delegate void MrukOnSceneAnchorRemoved(ref MrukSceneAnchor sceneAnchor, IntPtr userContext);

	public delegate void MrukOnDiscoveryFinished(MrukResult result, IntPtr userContext);

	public delegate void MrukOnEnvironmentRaycasterCreated(MrukResult result, IntPtr userContext);

	public delegate Pose TrackingSpacePoseGetter();

	public delegate void TrackingSpacePoseSetter(Pose pose);

	public struct MrukLabelFilter
	{
		public uint surfaceType;

		public uint includedLabels;

		[MarshalAs(UnmanagedType.U1)]
		public bool includedLabelsSet;
	}

	public struct MrukPolygon2f
	{
		public Vector2[] points;

		public uint numPoints;
	}

	public struct MrukMesh2f
	{
		public unsafe Vector2* vertices;

		public uint numVertices;

		public unsafe uint* indices;

		public uint numIndices;
	}

	public struct MrukMesh3f
	{
		public unsafe Vector3* vertices;

		public uint numVertices;

		public unsafe uint* indices;

		public uint numIndices;
	}

	public struct MrukVolume
	{
		public Vector3 min;

		public Vector3 max;
	}

	public struct MrukPlane
	{
		public float x;

		public float y;

		public float width;

		public float height;
	}

	public struct MrukSceneAnchor
	{
		public ulong space;

		public Guid uuid;

		public Guid roomUuid;

		public Pose pose;

		public MrukVolume volume;

		public MrukPlane plane;

		public MrukLabel semanticLabel;

		public unsafe Vector2* planeBoundary;

		public unsafe uint* globalMeshIndices;

		public unsafe Vector3* globalMeshPositions;

		public uint planeBoundaryCount;

		public uint globalMeshIndicesCount;

		public uint globalMeshPositionsCount;

		[MarshalAs(UnmanagedType.U1)]
		public bool hasVolume;

		[MarshalAs(UnmanagedType.U1)]
		public bool hasPlane;
	}

	public struct MrukRoomAnchor
	{
		public ulong space;

		public Guid uuid;

		public Pose pose;
	}

	public struct MrukEventListener
	{
		public MrukOnPreRoomAnchorAdded onPreRoomAnchorAdded;

		public MrukOnRoomAnchorAdded onRoomAnchorAdded;

		public MrukOnRoomAnchorUpdated onRoomAnchorUpdated;

		public MrukOnRoomAnchorRemoved onRoomAnchorRemoved;

		public MrukOnSceneAnchorAdded onSceneAnchorAdded;

		public MrukOnSceneAnchorUpdated onSceneAnchorUpdated;

		public MrukOnSceneAnchorRemoved onSceneAnchorRemoved;

		public MrukOnDiscoveryFinished onDiscoveryFinished;

		public MrukOnEnvironmentRaycasterCreated onEnvironmentRaycasterCreated;

		public IntPtr userContext;
	}

	public struct MrukHit
	{
		public Guid roomAnchorUuid;

		public Guid sceneAnchorUuid;

		public float hitDistance;

		public Vector3 hitPosition;

		public Vector3 hitNormal;
	}

	public struct MrukSharedRoomsData
	{
		public Guid groupUuid;

		public unsafe Guid* roomUuids;

		public uint numRoomUuids;

		public Guid alignmentRoomUuid;

		public Pose roomWorldPoseOnHost;
	}

	public struct _MrukUuidAlignmentTest
	{
		public byte padding;

		public Guid uuid;
	}

	public struct MrukEnvironmentRaycastHitPointGetInfo
	{
		public Vector3 startPoint;

		public Vector3 direction;

		public uint filterCount;

		public float maxDistance;
	}

	public struct MrukEnvironmentRaycastHitPoint
	{
		public MrukEnvironmentRaycastStatus status;

		public Vector3 point;

		public Quaternion orientation;

		public Vector3 normal;
	}

	internal delegate void SetLogPrinterDelegate(LogPrinter printer);

	internal delegate MrukResult AnchorStoreCreateDelegate(ulong xrInstance, ulong xrSession, IntPtr xrInstanceProcAddrFunc, ulong baseSpace, string[] availableOpenXrExtensions, uint availableOpenXrExtensionsCount);

	internal delegate MrukResult AnchorStoreCreateWithoutOpenXrDelegate();

	internal delegate void AnchorStoreShutdownOpenXrDelegate();

	internal delegate void AnchorStoreDestroyDelegate();

	internal delegate void AnchorStoreSetBaseSpaceDelegate(ulong baseSpace);

	internal delegate MrukResult AnchorStoreStartDiscoveryDelegate([MarshalAs(UnmanagedType.U1)] bool shouldRemoveMissingRooms, MrukSceneModel sceneModel);

	internal delegate MrukResult AnchorStoreStartQueryByLocalGroupDelegate(MrukSharedRoomsData sharedRoomsData, [MarshalAs(UnmanagedType.U1)] bool shouldRemoveMissingRooms, MrukSceneModel sceneModel);

	internal delegate MrukResult AnchorStoreLoadSceneFromJsonDelegate(string jsonString, [MarshalAs(UnmanagedType.U1)] bool shouldRemoveMissingRooms, MrukSceneModel sceneModel);

	internal unsafe delegate char* AnchorStoreSaveSceneToJsonDelegate([MarshalAs(UnmanagedType.U1)] bool includeGlobalMesh, Guid[] roomUuids, uint numRoomUuids);

	internal unsafe delegate void AnchorStoreFreeJsonDelegate(char* jsonString);

	internal unsafe delegate MrukResult AnchorStoreLoadSceneFromPrefabDelegate(MrukRoomAnchor* roomAnchors, uint numRoomAnchors, MrukSceneAnchor* sceneAnchors, uint numSceneAnchors);

	internal delegate void AnchorStoreClearRoomsDelegate();

	internal delegate void AnchorStoreClearRoomDelegate(Guid roomUuid);

	internal delegate void AnchorStoreOnOpenXrEventDelegate(IntPtr baseEventHeader);

	internal delegate void AnchorStoreTickDelegate(ulong nextPredictedDisplayTime);

	internal delegate void AnchorStoreRegisterEventListenerDelegate(MrukEventListener listener);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool AnchorStoreRaycastRoomDelegate(Guid roomUuid, Vector3 origin, Vector3 direction, float maxDistance, MrukLabelFilter labelFilter, ref MrukHit outHit);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool AnchorStoreRaycastRoomAllDelegate(Guid roomUuid, Vector3 origin, Vector3 direction, float maxDistance, MrukLabelFilter labelFilter, ref MrukHit outHits, ref uint outHitsCount);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool AnchorStoreRaycastAnchorDelegate(Guid sceneAnchorUuid, Vector3 origin, Vector3 direction, float maxDistance, uint surfaceTypes, ref MrukHit outHit);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool AnchorStoreRaycastAnchorAllDelegate(Guid sceneAnchorUuid, Vector3 origin, Vector3 direction, float maxDistance, uint surfaceTypes, ref MrukHit outHits, ref uint outHitsCount);

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool AnchorStoreIsDiscoveryRunningDelegate();

	[return: MarshalAs(UnmanagedType.U1)]
	internal delegate bool AnchorStoreGetWorldLockOffsetDelegate(Guid roomUuid, ref Pose offset);

	internal delegate Vector3 AddVectorsDelegate(Vector3 a, Vector3 b);

	internal delegate MrukMesh2f TriangulatePolygonDelegate(MrukPolygon2f[] polygons, uint numPolygons);

	internal delegate void FreeMeshDelegate(ref MrukMesh2f mesh);

	internal unsafe delegate MrukResult ComputeMeshSegmentationDelegate(Vector3[] vertices, uint numVertices, uint[] indices, uint numIndices, Vector3[] segmentationPoints, uint numSegmentationPoints, Vector3 reservedMin, Vector3 reservedMax, out MrukMesh3f* meshSegments, out uint numSegments, out MrukMesh3f reservedSegment);

	internal unsafe delegate void FreeMeshSegmentationDelegate(MrukMesh3f* meshSegments, uint numSegments, ref MrukMesh3f reservedSegment);

	internal delegate Guid _TestUuidMarshallingDelegate(_MrukUuidAlignmentTest packedUuid);

	internal delegate MrukLabel StringToMrukLabelDelegate(string label);

	internal delegate void CreateEnvironmentRaycasterDelegate();

	internal delegate void DestroyEnvironmentRaycasterDelegate();

	internal delegate void PerformEnvironmentRaycastDelegate(ref MrukEnvironmentRaycastHitPointGetInfo info, ref MrukEnvironmentRaycastHitPoint hitPoint);

	internal delegate void SetTrackingSpacePoseGetterDelegate(TrackingSpacePoseGetter getter);

	internal delegate void SetTrackingSpacePoseSetterDelegate(TrackingSpacePoseSetter setter);

	internal static SetLogPrinterDelegate SetLogPrinter;

	internal static AnchorStoreCreateDelegate AnchorStoreCreate;

	internal static AnchorStoreCreateWithoutOpenXrDelegate AnchorStoreCreateWithoutOpenXr;

	internal static AnchorStoreShutdownOpenXrDelegate AnchorStoreShutdownOpenXr;

	internal static AnchorStoreDestroyDelegate AnchorStoreDestroy;

	internal static AnchorStoreSetBaseSpaceDelegate AnchorStoreSetBaseSpace;

	internal static AnchorStoreStartDiscoveryDelegate AnchorStoreStartDiscovery;

	internal static AnchorStoreStartQueryByLocalGroupDelegate AnchorStoreStartQueryByLocalGroup;

	internal static AnchorStoreLoadSceneFromJsonDelegate AnchorStoreLoadSceneFromJson;

	internal static AnchorStoreSaveSceneToJsonDelegate AnchorStoreSaveSceneToJson;

	internal static AnchorStoreFreeJsonDelegate AnchorStoreFreeJson;

	internal static AnchorStoreLoadSceneFromPrefabDelegate AnchorStoreLoadSceneFromPrefab;

	internal static AnchorStoreClearRoomsDelegate AnchorStoreClearRooms;

	internal static AnchorStoreClearRoomDelegate AnchorStoreClearRoom;

	internal static AnchorStoreOnOpenXrEventDelegate AnchorStoreOnOpenXrEvent;

	internal static AnchorStoreTickDelegate AnchorStoreTick;

	internal static AnchorStoreRegisterEventListenerDelegate AnchorStoreRegisterEventListener;

	internal static AnchorStoreRaycastRoomDelegate AnchorStoreRaycastRoom;

	internal static AnchorStoreRaycastRoomAllDelegate AnchorStoreRaycastRoomAll;

	internal static AnchorStoreRaycastAnchorDelegate AnchorStoreRaycastAnchor;

	internal static AnchorStoreRaycastAnchorAllDelegate AnchorStoreRaycastAnchorAll;

	internal static AnchorStoreIsDiscoveryRunningDelegate AnchorStoreIsDiscoveryRunning;

	internal static AnchorStoreGetWorldLockOffsetDelegate AnchorStoreGetWorldLockOffset;

	internal static AddVectorsDelegate AddVectors;

	internal static TriangulatePolygonDelegate TriangulatePolygon;

	internal static FreeMeshDelegate FreeMesh;

	internal static ComputeMeshSegmentationDelegate ComputeMeshSegmentation;

	internal static FreeMeshSegmentationDelegate FreeMeshSegmentation;

	internal static _TestUuidMarshallingDelegate _TestUuidMarshalling;

	internal static StringToMrukLabelDelegate StringToMrukLabel;

	internal static CreateEnvironmentRaycasterDelegate CreateEnvironmentRaycaster;

	internal static DestroyEnvironmentRaycasterDelegate DestroyEnvironmentRaycaster;

	internal static PerformEnvironmentRaycastDelegate PerformEnvironmentRaycast;

	internal static SetTrackingSpacePoseGetterDelegate SetTrackingSpacePoseGetter;

	internal static SetTrackingSpacePoseSetterDelegate SetTrackingSpacePoseSetter;

	internal static void LoadNativeFunctions()
	{
		SetLogPrinter = MRUKNative.LoadFunction<SetLogPrinterDelegate>("SetLogPrinter");
		AnchorStoreCreate = MRUKNative.LoadFunction<AnchorStoreCreateDelegate>("AnchorStoreCreate");
		AnchorStoreCreateWithoutOpenXr = MRUKNative.LoadFunction<AnchorStoreCreateWithoutOpenXrDelegate>("AnchorStoreCreateWithoutOpenXr");
		AnchorStoreShutdownOpenXr = MRUKNative.LoadFunction<AnchorStoreShutdownOpenXrDelegate>("AnchorStoreShutdownOpenXr");
		AnchorStoreDestroy = MRUKNative.LoadFunction<AnchorStoreDestroyDelegate>("AnchorStoreDestroy");
		AnchorStoreSetBaseSpace = MRUKNative.LoadFunction<AnchorStoreSetBaseSpaceDelegate>("AnchorStoreSetBaseSpace");
		AnchorStoreStartDiscovery = MRUKNative.LoadFunction<AnchorStoreStartDiscoveryDelegate>("AnchorStoreStartDiscovery");
		AnchorStoreStartQueryByLocalGroup = MRUKNative.LoadFunction<AnchorStoreStartQueryByLocalGroupDelegate>("AnchorStoreStartQueryByLocalGroup");
		AnchorStoreLoadSceneFromJson = MRUKNative.LoadFunction<AnchorStoreLoadSceneFromJsonDelegate>("AnchorStoreLoadSceneFromJson");
		AnchorStoreSaveSceneToJson = MRUKNative.LoadFunction<AnchorStoreSaveSceneToJsonDelegate>("AnchorStoreSaveSceneToJson");
		AnchorStoreFreeJson = MRUKNative.LoadFunction<AnchorStoreFreeJsonDelegate>("AnchorStoreFreeJson");
		AnchorStoreLoadSceneFromPrefab = MRUKNative.LoadFunction<AnchorStoreLoadSceneFromPrefabDelegate>("AnchorStoreLoadSceneFromPrefab");
		AnchorStoreClearRooms = MRUKNative.LoadFunction<AnchorStoreClearRoomsDelegate>("AnchorStoreClearRooms");
		AnchorStoreClearRoom = MRUKNative.LoadFunction<AnchorStoreClearRoomDelegate>("AnchorStoreClearRoom");
		AnchorStoreOnOpenXrEvent = MRUKNative.LoadFunction<AnchorStoreOnOpenXrEventDelegate>("AnchorStoreOnOpenXrEvent");
		AnchorStoreTick = MRUKNative.LoadFunction<AnchorStoreTickDelegate>("AnchorStoreTick");
		AnchorStoreRegisterEventListener = MRUKNative.LoadFunction<AnchorStoreRegisterEventListenerDelegate>("AnchorStoreRegisterEventListener");
		AnchorStoreRaycastRoom = MRUKNative.LoadFunction<AnchorStoreRaycastRoomDelegate>("AnchorStoreRaycastRoom");
		AnchorStoreRaycastRoomAll = MRUKNative.LoadFunction<AnchorStoreRaycastRoomAllDelegate>("AnchorStoreRaycastRoomAll");
		AnchorStoreRaycastAnchor = MRUKNative.LoadFunction<AnchorStoreRaycastAnchorDelegate>("AnchorStoreRaycastAnchor");
		AnchorStoreRaycastAnchorAll = MRUKNative.LoadFunction<AnchorStoreRaycastAnchorAllDelegate>("AnchorStoreRaycastAnchorAll");
		AnchorStoreIsDiscoveryRunning = MRUKNative.LoadFunction<AnchorStoreIsDiscoveryRunningDelegate>("AnchorStoreIsDiscoveryRunning");
		AnchorStoreGetWorldLockOffset = MRUKNative.LoadFunction<AnchorStoreGetWorldLockOffsetDelegate>("AnchorStoreGetWorldLockOffset");
		AddVectors = MRUKNative.LoadFunction<AddVectorsDelegate>("AddVectors");
		TriangulatePolygon = MRUKNative.LoadFunction<TriangulatePolygonDelegate>("TriangulatePolygon");
		FreeMesh = MRUKNative.LoadFunction<FreeMeshDelegate>("FreeMesh");
		ComputeMeshSegmentation = MRUKNative.LoadFunction<ComputeMeshSegmentationDelegate>("ComputeMeshSegmentation");
		FreeMeshSegmentation = MRUKNative.LoadFunction<FreeMeshSegmentationDelegate>("FreeMeshSegmentation");
		_TestUuidMarshalling = MRUKNative.LoadFunction<_TestUuidMarshallingDelegate>("_TestUuidMarshalling");
		StringToMrukLabel = MRUKNative.LoadFunction<StringToMrukLabelDelegate>("StringToMrukLabel");
		CreateEnvironmentRaycaster = MRUKNative.LoadFunction<CreateEnvironmentRaycasterDelegate>("CreateEnvironmentRaycaster");
		DestroyEnvironmentRaycaster = MRUKNative.LoadFunction<DestroyEnvironmentRaycasterDelegate>("DestroyEnvironmentRaycaster");
		PerformEnvironmentRaycast = MRUKNative.LoadFunction<PerformEnvironmentRaycastDelegate>("PerformEnvironmentRaycast");
		SetTrackingSpacePoseGetter = MRUKNative.LoadFunction<SetTrackingSpacePoseGetterDelegate>("SetTrackingSpacePoseGetter");
		SetTrackingSpacePoseSetter = MRUKNative.LoadFunction<SetTrackingSpacePoseSetterDelegate>("SetTrackingSpacePoseSetter");
	}

	internal static void UnloadNativeFunctions()
	{
		SetLogPrinter = null;
		AnchorStoreCreate = null;
		AnchorStoreCreateWithoutOpenXr = null;
		AnchorStoreShutdownOpenXr = null;
		AnchorStoreDestroy = null;
		AnchorStoreSetBaseSpace = null;
		AnchorStoreStartDiscovery = null;
		AnchorStoreStartQueryByLocalGroup = null;
		AnchorStoreLoadSceneFromJson = null;
		AnchorStoreSaveSceneToJson = null;
		AnchorStoreFreeJson = null;
		AnchorStoreLoadSceneFromPrefab = null;
		AnchorStoreClearRooms = null;
		AnchorStoreClearRoom = null;
		AnchorStoreOnOpenXrEvent = null;
		AnchorStoreTick = null;
		AnchorStoreRegisterEventListener = null;
		AnchorStoreRaycastRoom = null;
		AnchorStoreRaycastRoomAll = null;
		AnchorStoreRaycastAnchor = null;
		AnchorStoreRaycastAnchorAll = null;
		AnchorStoreIsDiscoveryRunning = null;
		AnchorStoreGetWorldLockOffset = null;
		AddVectors = null;
		TriangulatePolygon = null;
		FreeMesh = null;
		ComputeMeshSegmentation = null;
		FreeMeshSegmentation = null;
		_TestUuidMarshalling = null;
		StringToMrukLabel = null;
		CreateEnvironmentRaycaster = null;
		DestroyEnvironmentRaycaster = null;
		PerformEnvironmentRaycast = null;
		SetTrackingSpacePoseGetter = null;
		SetTrackingSpacePoseSetter = null;
	}
}
