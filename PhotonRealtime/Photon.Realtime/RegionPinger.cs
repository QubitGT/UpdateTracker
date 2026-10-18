using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime;

public class RegionPinger
{
	public static int Attempts = 5;

	public static bool IgnoreInitialAttempt = true;

	public static int MaxMilliseconsPerPing = 800;

	public static int PingWhenFailed = Attempts * MaxMilliseconsPerPing;

	private Region region;

	private string regionAddress;

	public int CurrentAttempt;

	private Action<Region> onDoneCall;

	private PhotonPing ping;

	private List<int> rttResults;

	public bool Done { get; private set; }

	public RegionPinger(Region region, Action<Region> onDoneCallback)
	{
		this.region = region;
		this.region.Ping = PingWhenFailed;
		Done = false;
		onDoneCall = onDoneCallback;
	}

	private PhotonPing GetPingImplementation()
	{
		PhotonPing photonPing = null;
		if (RegionHandler.PingImplementation == null || RegionHandler.PingImplementation == typeof(PingMono))
		{
			photonPing = new PingMono();
		}
		if (photonPing == null && RegionHandler.PingImplementation != null)
		{
			photonPing = (PhotonPing)Activator.CreateInstance(RegionHandler.PingImplementation);
		}
		return photonPing;
	}

	public bool Start()
	{
		string text = region.HostAndPort;
		int num = text.LastIndexOf(':');
		if (num > 1)
		{
			text = text.Substring(0, num);
		}
		regionAddress = ResolveHost(text);
		ping = GetPingImplementation();
		Done = false;
		CurrentAttempt = 0;
		rttResults = new List<int>(Attempts);
		bool flag = false;
		try
		{
			flag = ThreadPool.QueueUserWorkItem(RegionPingPooled);
		}
		catch
		{
			flag = false;
		}
		if (!flag)
		{
			SupportClass.StartBackgroundCalls(RegionPingThreaded, 0, "RegionPing_" + region.Code + "_" + region.Cluster);
		}
		return true;
	}

	protected internal void RegionPingPooled(object context)
	{
		RegionPingThreaded();
	}

	protected internal bool RegionPingThreaded()
	{
		region.Ping = PingWhenFailed;
		float num = 0f;
		int num2 = 0;
		Stopwatch stopwatch = new Stopwatch();
		for (CurrentAttempt = 0; CurrentAttempt < Attempts; CurrentAttempt++)
		{
			bool flag = false;
			stopwatch.Reset();
			stopwatch.Start();
			try
			{
				ping.StartPing(regionAddress);
			}
			catch (Exception)
			{
				break;
			}
			while (!ping.Done())
			{
				if (stopwatch.ElapsedMilliseconds >= MaxMilliseconsPerPing)
				{
					flag = true;
					break;
				}
				Thread.Sleep(0);
			}
			stopwatch.Stop();
			int num3 = (int)stopwatch.ElapsedMilliseconds;
			rttResults.Add(num3);
			if ((!IgnoreInitialAttempt || CurrentAttempt != 0) && ping.Successful && !flag)
			{
				num += (float)num3;
				num2++;
				region.Ping = (int)(num / (float)num2);
			}
			Thread.Sleep(10);
		}
		Done = true;
		ping.Dispose();
		onDoneCall(region);
		return false;
	}

	protected internal IEnumerator RegionPingCoroutine()
	{
		region.Ping = PingWhenFailed;
		float rttSum = 0f;
		int replyCount = 0;
		Stopwatch sw = new Stopwatch();
		for (CurrentAttempt = 0; CurrentAttempt < Attempts; CurrentAttempt++)
		{
			bool overtime = false;
			sw.Reset();
			sw.Start();
			try
			{
				ping.StartPing(regionAddress);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("catched: " + ex);
				break;
			}
			while (!ping.Done())
			{
				if (sw.ElapsedMilliseconds >= MaxMilliseconsPerPing)
				{
					overtime = true;
					break;
				}
				yield return 0;
			}
			sw.Stop();
			int num = (int)sw.ElapsedMilliseconds;
			rttResults.Add(num);
			if ((!IgnoreInitialAttempt || CurrentAttempt != 0) && ping.Successful && !overtime)
			{
				rttSum += (float)num;
				replyCount++;
				region.Ping = (int)(rttSum / (float)replyCount);
			}
			yield return new WaitForSeconds(0.1f);
		}
		Done = true;
		ping.Dispose();
		onDoneCall(region);
		yield return null;
	}

	public string GetResults()
	{
		return $"{region.Code}: {region.Ping} ({rttResults.ToStringFull()})";
	}

	public static string ResolveHost(string hostName)
	{
		if (hostName.StartsWith("wss://"))
		{
			hostName = hostName.Substring(6);
		}
		if (hostName.StartsWith("ws://"))
		{
			hostName = hostName.Substring(5);
		}
		string text = string.Empty;
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
			if (hostAddresses.Length == 1)
			{
				return hostAddresses[0].ToString();
			}
			foreach (IPAddress iPAddress in hostAddresses)
			{
				if (iPAddress != null)
				{
					if (iPAddress.ToString().Contains(":"))
					{
						return iPAddress.ToString();
					}
					if (string.IsNullOrEmpty(text))
					{
						text = hostAddresses.ToString();
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return text;
	}
}
