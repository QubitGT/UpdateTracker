using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class OVRColocationSession
{
	public struct Data
	{
		public const int MaxMetadataSize = 1024;

		public Guid AdvertisementUuid { get; internal set; }

		public byte[] Metadata { get; internal set; }
	}

	[OVRResultStatus]
	public enum Result
	{
		Success = 0,
		AlreadyAdvertising = 3001,
		AlreadyDiscovering = 3002,
		Failure = -1000,
		Unsupported = -1004,
		OperationFailed = -1006,
		InvalidData = -1008,
		NetworkFailed = -3002,
		NoDiscoveryMethodAvailable = -3003
	}

	public static event Action<Data> ColocationSessionDiscovered;

	public unsafe static OVRTask<OVRResult<Guid, Result>> StartAdvertisementAsync(ReadOnlySpan<byte> colocationSessionData)
	{
		if (colocationSessionData.Length > 1024)
		{
			throw new ArgumentException($"Colocation Session Advertisement can only store up to {1024} bytes of data");
		}
		OVRPlugin.ColocationSessionStartAdvertisementInfo info = default(OVRPlugin.ColocationSessionStartAdvertisementInfo);
		fixed (byte* groupMetadata = colocationSessionData)
		{
			info.GroupMetadata = groupMetadata;
			info.PeerMetadataCount = (uint)colocationSessionData.Length;
			ulong requestId;
			return OVRTask.Build(OVRPlugin.StartColocationSessionAdvertisement(info, out requestId), requestId).ToTask<Guid, Result>();
		}
	}

	public static OVRTask<OVRResult<Result>> StopAdvertisementAsync()
	{
		ulong requestId;
		return OVRTask.Build(OVRPlugin.StopColocationSessionAdvertisement(out requestId), requestId).ToResultTask<Result>();
	}

	public static OVRTask<OVRResult<Result>> StartDiscoveryAsync()
	{
		ulong requestId;
		return OVRTask.Build(OVRPlugin.StartColocationSessionDiscovery(out requestId), requestId).ToResultTask<Result>();
	}

	public static OVRTask<OVRResult<Result>> StopDiscoveryAsync()
	{
		ulong requestId;
		return OVRTask.Build(OVRPlugin.StopColocationSessionDiscovery(out requestId), requestId).ToResultTask<Result>();
	}

	internal static void OnColocationSessionStartAdvertisementComplete(ulong requestId, OVRPlugin.Result result, Guid uuid)
	{
		OVRTask.SetResult(requestId, OVRResult<Guid, Result>.From(uuid, (Result)result));
	}

	internal static void OnColocationSessionStopAdvertisementComplete(ulong requestId, OVRPlugin.Result result)
	{
		OVRTask.SetResult(requestId, OVRResult<Result>.From((Result)result));
	}

	internal static void OnColocationSessionStartDiscoveryComplete(ulong requestId, OVRPlugin.Result result)
	{
		OVRTask.SetResult(requestId, OVRResult<Result>.From((Result)result));
	}

	internal static void OnColocationSessionStopDiscoveryComplete(ulong requestId, OVRPlugin.Result result)
	{
		OVRTask.SetResult(requestId, OVRResult<Result>.From((Result)result));
	}

	internal unsafe static void OnColocationSessionDiscoveryResult(ulong requestId, Guid uuid, uint metaDataCount, byte* metaDataPtr)
	{
		byte[] array = new byte[metaDataCount];
		Marshal.Copy((IntPtr)metaDataPtr, array, 0, (int)metaDataCount);
		Data data = default(Data);
		data.AdvertisementUuid = uuid;
		data.Metadata = array;
		Data obj = data;
		OVRColocationSession.ColocationSessionDiscovered?.Invoke(obj);
	}

	internal static void OnColocationSessionAdvertisementComplete(ulong requestId, OVRPlugin.Result result)
	{
		if (result != 0)
		{
			Debug.LogWarning($"Colocation Session Advertisement unexpectedly completed with result: {result}");
		}
	}

	internal static void OnColocationSessionDiscoveryComplete(ulong requestId, OVRPlugin.Result result)
	{
		if (result != 0)
		{
			Debug.LogWarning($"Colocation Session Discovery unexpectedly completed with result: {result}");
		}
	}
}
