using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class RegionHandler
{
	public static Type PingImplementation;

	private string availableRegionCodes;

	private Region bestRegionCache;

	private readonly List<RegionPinger> pingerList = new List<RegionPinger>();

	private Action<RegionHandler> onCompleteCall;

	private int previousPing;

	private string previousSummaryProvided;

	protected internal static ushort PortToPingOverride;

	private float rePingFactor = 1.2f;

	private float pingSimilarityFactor = 1.2f;

	public int BestRegionSummaryPingLimit = 90;

	private MonoBehaviourEmpty emptyMonoBehavior;

	public List<Region> EnabledRegions { get; protected internal set; }

	public Region BestRegion
	{
		get
		{
			if (EnabledRegions == null)
			{
				return null;
			}
			if (bestRegionCache != null)
			{
				return bestRegionCache;
			}
			EnabledRegions.Sort((Region a, Region b) => a.Ping.CompareTo(b.Ping));
			int num = (int)((float)EnabledRegions[0].Ping * pingSimilarityFactor);
			Region region = EnabledRegions[0];
			foreach (Region enabledRegion in EnabledRegions)
			{
				if (enabledRegion.Ping <= num && enabledRegion.Code.CompareTo(region.Code) < 0)
				{
					region = enabledRegion;
				}
			}
			bestRegionCache = region;
			return bestRegionCache;
		}
	}

	public string SummaryToCache
	{
		get
		{
			if (BestRegion != null && BestRegion.Ping < RegionPinger.MaxMillisecondsPerPing)
			{
				return BestRegion.Code + ";" + BestRegion.Ping + ";" + availableRegionCodes;
			}
			return availableRegionCodes;
		}
	}

	public bool IsPinging { get; private set; }

	public bool Aborted { get; private set; }

	public string GetResults()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Region Pinging Result: {0}\n", BestRegion.ToString());
		foreach (RegionPinger pinger in pingerList)
		{
			stringBuilder.AppendLine(pinger.GetResults());
		}
		stringBuilder.AppendFormat("Previous summary: {0}", previousSummaryProvided);
		return stringBuilder.ToString();
	}

	public void SetRegions(OperationResponse opGetRegions, LoadBalancingClient loadBalancingClient = null)
	{
		if (opGetRegions.OperationCode != 220 || opGetRegions.ReturnCode != 0)
		{
			return;
		}
		string[] array = opGetRegions[210] as string[];
		string[] array2 = opGetRegions[230] as string[];
		if (array == null || array2 == null || array.Length != array2.Length)
		{
			loadBalancingClient?.DebugReturn(DebugLevel.ERROR, "RegionHandler.SetRegions() failed. Received regions and servers must be non null and of equal length. Could not read regions.");
			return;
		}
		bestRegionCache = null;
		EnabledRegions = new List<Region>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array2[i];
			if (PortToPingOverride != 0)
			{
				text = LoadBalancingClient.ReplacePortWithAlternative(array2[i], PortToPingOverride);
			}
			if (loadBalancingClient != null && loadBalancingClient.AddressRewriter != null)
			{
				text = loadBalancingClient.AddressRewriter(text, ServerConnection.MasterServer);
			}
			Region region = new Region(array[i], text);
			if (!string.IsNullOrEmpty(region.Code))
			{
				EnabledRegions.Add(region);
			}
		}
		Array.Sort(array);
		availableRegionCodes = string.Join(",", array);
	}

	public RegionHandler(ushort masterServerPortOverride = 0)
	{
		PortToPingOverride = masterServerPortOverride;
	}

	public bool PingMinimumOfRegions(Action<RegionHandler> onCompleteCallback, string previousSummary)
	{
		if (EnabledRegions == null || EnabledRegions.Count == 0)
		{
			return false;
		}
		if (IsPinging)
		{
			return false;
		}
		Aborted = false;
		IsPinging = true;
		previousSummaryProvided = previousSummary;
		if (emptyMonoBehavior != null)
		{
			emptyMonoBehavior.SelfDestroy();
		}
		emptyMonoBehavior = MonoBehaviourEmpty.BuildInstance("RegionHandler");
		emptyMonoBehavior.onCompleteCall = onCompleteCallback;
		onCompleteCall = emptyMonoBehavior.CompleteOnMainThread;
		if (string.IsNullOrEmpty(previousSummary))
		{
			return PingEnabledRegions();
		}
		string[] array = previousSummary.Split(';');
		if (array.Length < 3)
		{
			return PingEnabledRegions();
		}
		if (!int.TryParse(array[1], out var result))
		{
			return PingEnabledRegions();
		}
		string prevBestRegionCode = array[0];
		string value = array[2];
		if (string.IsNullOrEmpty(prevBestRegionCode))
		{
			return PingEnabledRegions();
		}
		if (string.IsNullOrEmpty(value))
		{
			return PingEnabledRegions();
		}
		if (!availableRegionCodes.Equals(value) || !availableRegionCodes.Contains(prevBestRegionCode))
		{
			return PingEnabledRegions();
		}
		if (result >= RegionPinger.PingWhenFailed)
		{
			return PingEnabledRegions();
		}
		previousPing = result;
		Region region = EnabledRegions.Find((Region r) => r.Code.Equals(prevBestRegionCode));
		RegionPinger regionPinger = new RegionPinger(region, OnPreferredRegionPinged);
		lock (pingerList)
		{
			pingerList.Clear();
			pingerList.Add(regionPinger);
		}
		regionPinger.Start();
		return true;
	}

	public void Abort()
	{
		if (Aborted)
		{
			return;
		}
		Aborted = true;
		lock (pingerList)
		{
			foreach (RegionPinger pinger in pingerList)
			{
				pinger.Abort();
			}
		}
		if (emptyMonoBehavior != null)
		{
			emptyMonoBehavior.SelfDestroy();
		}
	}

	private void OnPreferredRegionPinged(Region preferredRegion)
	{
		if (preferredRegion.Ping > BestRegionSummaryPingLimit || (float)preferredRegion.Ping > (float)previousPing * rePingFactor)
		{
			PingEnabledRegions();
			return;
		}
		IsPinging = false;
		onCompleteCall(this);
	}

	private bool PingEnabledRegions()
	{
		if (EnabledRegions == null || EnabledRegions.Count == 0)
		{
			return false;
		}
		lock (pingerList)
		{
			pingerList.Clear();
			foreach (Region enabledRegion in EnabledRegions)
			{
				RegionPinger regionPinger = new RegionPinger(enabledRegion, OnRegionDone);
				pingerList.Add(regionPinger);
				regionPinger.Start();
			}
		}
		return true;
	}

	private void OnRegionDone(Region region)
	{
		lock (pingerList)
		{
			if (!IsPinging)
			{
				return;
			}
			bestRegionCache = null;
			foreach (RegionPinger pinger in pingerList)
			{
				if (!pinger.Done)
				{
					return;
				}
			}
			IsPinging = false;
		}
		if (!Aborted)
		{
			onCompleteCall(this);
		}
	}
}
