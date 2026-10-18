#define DEBUG
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Photon.Realtime;
using Fusion.Photon.Realtime.Async;
using UnityEngine;

namespace Fusion;

internal static class FusionRealtimeProxy
{
	private const float REGION_INFO_CACHE_TIME = 10f;

	private static float _lastRegionRequestTime;

	private static List<RegionInfo> _cachedRegionInfo;

	internal static async Task<List<RegionInfo>> GetEnabledRegions(string appId, CancellationToken cancellationToken)
	{
		if (PhotonAppSettings.TryGetGlobal(out var global) && appId == null)
		{
			appId = global.AppSettings.AppIdFusion;
		}
		if (appId == null)
		{
			InternalLogStreams.LogDebug?.Warn("Could not get enabled regions. Provided App id is not valid.");
			return null;
		}
		if (Time.time <= _lastRegionRequestTime + 10f && _cachedRegionInfo != null)
		{
			return await Task.FromResult(_cachedRegionInfo);
		}
		LoadBalancingClient client = new LoadBalancingClient
		{
			AppId = appId
		};
		RegionHandler regionHandler = await client.GetRegionsAsync(throwOnError: true, createServiceTask: true, cancellationToken);
		await client.LeaveRoomAsync(createServiceTask: true, cancellationToken);
		await client.DisconnectAsync(createServiceTask: true, cancellationToken);
		List<RegionInfo> list = new List<RegionInfo>();
		if (regionHandler == null)
		{
			return list;
		}
		foreach (Region region in regionHandler.EnabledRegions)
		{
			list.Add(new RegionInfo
			{
				RegionCode = region.Code,
				RegionPing = region.Ping
			});
		}
		_cachedRegionInfo = new List<RegionInfo>(list);
		_lastRegionRequestTime = Time.unscaledTime;
		return list;
	}
}
