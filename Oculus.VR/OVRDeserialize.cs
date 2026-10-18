using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

internal static class OVRDeserialize
{
	public struct DisplayRefreshRateChangedData
	{
		public float FromRefreshRate;

		public float ToRefreshRate;
	}

	public struct SpaceQueryResultsData
	{
		public ulong RequestId;
	}

	public struct SpaceQueryCompleteData
	{
		public ulong RequestId;

		public int Result;
	}

	public struct SceneCaptureCompleteData
	{
		public ulong RequestId;

		public int Result;
	}

	public struct SpatialAnchorCreateCompleteData
	{
		public ulong RequestId;

		public int Result;

		public ulong Space;

		public Guid Uuid;
	}

	public struct SpaceSetComponentStatusCompleteData
	{
		public ulong RequestId;

		public int Result;

		public ulong Space;

		public Guid Uuid;

		public OVRPlugin.SpaceComponentType ComponentType;

		public int Enabled;
	}

	public struct SpaceSaveCompleteData
	{
		public ulong RequestId;

		public ulong Space;

		public int Result;

		public Guid Uuid;
	}

	public struct SpaceEraseCompleteData
	{
		public ulong RequestId;

		public int Result;

		public Guid Uuid;

		public OVRPlugin.SpaceStorageLocation Location;
	}

	public struct SpaceShareResultData
	{
		public ulong RequestId;

		public int Result;
	}

	public struct SpaceListSaveResultData
	{
		public ulong RequestId;

		public int Result;
	}

	public struct StartColocationSessionAdvertisementCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;

		public Guid AdvertisementUuid;
	}

	public struct StopColocationSessionAdvertisementCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;
	}

	public struct StartColocationSessionDiscoveryCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;
	}

	public struct StopColocationSessionDiscoveryCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;
	}

	public struct ColocationSessionDiscoveryResultData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public Guid AdvertisementUuid;

		public uint AdvertisementMetadataCount;

		public unsafe fixed byte AdvertisementMetadata[1024];
	}

	public struct ColocationSessionAdvertisementCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;
	}

	public struct ColocationSessionDiscoveryCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;
	}

	public struct ShareSpacesToGroupsCompleteData
	{
		public OVRPlugin.EventType EventType;

		public ulong RequestId;

		public OVRPlugin.Result Result;
	}

	public struct SpaceDiscoveryCompleteData
	{
		public ulong RequestId;

		public int Result;
	}

	public struct SpaceDiscoveryResultsData
	{
		public ulong RequestId;
	}

	public struct SpacesSaveResultData
	{
		public ulong RequestId;

		public OVRAnchor.SaveResult Result;
	}

	public struct SpacesEraseResultData
	{
		public ulong RequestId;

		public OVRAnchor.EraseResult Result;
	}

	public struct PassthroughLayerResumedData
	{
		public int LayerId;
	}

	public struct BoundaryVisibilityChangedData
	{
		public OVRPlugin.BoundaryVisibility BoundaryVisibility;
	}

	public struct CreateDynamicObjectTrackerResultData
	{
		public OVRPlugin.EventType EventType;

		public ulong Tracker;

		public OVRPlugin.Result Result;
	}

	public struct SetDynamicObjectTrackedClassesResultData
	{
		public OVRPlugin.EventType EventType;

		public ulong Tracker;

		public OVRPlugin.Result Result;
	}

	public struct EventDataReferenceSpaceChangePending
	{
		public OVRPlugin.EventType EventType;

		public OVRPlugin.TrackingOrigin ReferenceSpaceType;

		public double ChangeTime;

		public OVRPlugin.Bool PoseValid;

		public OVRPlugin.Posef PoseInPreviousSpace;
	}

	public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
	{
		GCHandle gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		try
		{
			return (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
		}
		finally
		{
			gCHandle.Free();
		}
	}

	public unsafe static T MarshalEntireStructAs<T>(this OVRPlugin.EventDataBuffer eventDataBuffer, Allocator allocator = Allocator.Temp)
	{
		using NativeArray<byte> nativeArray = new NativeArray<byte>(eventDataBuffer.EventData.Length + 4, allocator);
		byte* unsafePtr = (byte*)nativeArray.GetUnsafePtr();
		fixed (byte* source = eventDataBuffer.EventData)
		{
			*(OVRPlugin.EventType*)unsafePtr = eventDataBuffer.EventType;
			UnsafeUtility.MemCpy(unsafePtr + 4, source, eventDataBuffer.EventData.Length);
			return Marshal.PtrToStructure<T>(new IntPtr(unsafePtr));
		}
	}
}
