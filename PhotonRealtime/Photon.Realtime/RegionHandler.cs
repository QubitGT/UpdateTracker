using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;

namespace Photon.Realtime;

public class RegionHandler
{
	public static Type PingImplementation;

	private string availableRegionCodes;

	private Region bestRegionCache;

	private List<RegionPinger> pingerList = new List<RegionPinger>();

	private Action<RegionHandler> onCompleteCall;

	private int previousPing;

	private string previousSummaryProvided;

	protected internal static ushort PortToPingOverride;

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
			bestRegionCache = EnabledRegions[0];
			return bestRegionCache;
		}
	}

	public string SummaryToCache
	{
		get
		{
			if (BestRegion != null)
			{
				return BestRegion.Code + ";" + BestRegion.Ping + ";" + availableRegionCodes;
			}
			return availableRegionCodes;
		}
	}

	public bool IsPinging { get; private set; }

	public string GetResults()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Region Pinging Result: {0}\n", BestRegion.ToString());
		foreach (RegionPinger pinger in pingerList)
		{
			stringBuilder.AppendFormat(pinger.GetResults() + "\n");
		}
		stringBuilder.AppendFormat("Previous summary: {0}", previousSummaryProvided);
		return stringBuilder.ToString();
	}

	public void SetRegions(OperationResponse opGetRegions)
	{
		if (opGetRegions.OperationCode != 220 || opGetRegions.ReturnCode != 0)
		{
			return;
		}
		string[] array = opGetRegions[210] as string[];
		string[] array2 = opGetRegions[230] as string[];
		if (array == null || array2 == null || array.Length != array2.Length)
		{
			return;
		}
		bestRegionCache = null;
		EnabledRegions = new List<Region>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			string address = array2[i];
			if (PortToPingOverride != 0)
			{
				address = LoadBalancingClient.ReplacePortWithAlternative(array2[i], PortToPingOverride);
			}
			Region region = new Region(array[i], address);
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
		IsPinging = true;
		onCompleteCall = onCompleteCallback;
		previousSummaryProvided = previousSummary;
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
		RegionPinger regionPinger = new RegionPinger(EnabledRegions.Find((Region r) => r.Code.Equals(prevBestRegionCode)), OnPreferredRegionPinged);
		lock (pingerList)
		{
			pingerList.Add(regionPinger);
		}
		regionPinger.Start();
		return true;
	}

	private void OnPreferredRegionPinged(Region preferredRegion)
	{
		if ((float)preferredRegion.Ping > (float)previousPing * 1.5f)
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
		onCompleteCall(this);
	}
}
