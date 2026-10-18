using System;
using System.Diagnostics;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime;

public class ConnectionHandler : MonoBehaviour
{
	public bool DisconnectAfterKeepAlive;

	public int KeepAliveInBackground = 60000;

	public bool ApplyDontDestroyOnLoad = true;

	[NonSerialized]
	public static bool AppQuits;

	private byte fallbackThreadId = byte.MaxValue;

	private bool didSendAcks;

	private readonly Stopwatch backgroundStopwatch = new Stopwatch();

	public LoadBalancingClient Client { get; set; }

	public int CountSendAcksOnly { get; private set; }

	public bool FallbackThreadRunning => fallbackThreadId < byte.MaxValue;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void StaticReset()
	{
		AppQuits = false;
	}

	protected void OnApplicationQuit()
	{
		AppQuits = true;
	}

	protected virtual void Awake()
	{
		if (ApplyDontDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	protected virtual void OnDisable()
	{
		StopFallbackSendAckThread();
		if (AppQuits)
		{
			if (Client != null && Client.IsConnected)
			{
				Client.Disconnect();
				Client.LoadBalancingPeer.StopThread();
			}
			SupportClass.StopAllBackgroundCalls();
		}
	}

	public void StartFallbackSendAckThread()
	{
		if (!FallbackThreadRunning)
		{
			fallbackThreadId = SupportClass.StartBackgroundCalls(RealtimeFallbackThread, 50, "RealtimeFallbackThread");
		}
	}

	public void StopFallbackSendAckThread()
	{
		if (FallbackThreadRunning)
		{
			SupportClass.StopBackgroundCalls(fallbackThreadId);
			fallbackThreadId = byte.MaxValue;
		}
	}

	public bool RealtimeFallbackThread()
	{
		if (Client != null)
		{
			if (!Client.IsConnected)
			{
				didSendAcks = false;
				return true;
			}
			if (Client.LoadBalancingPeer.ConnectionTime - Client.LoadBalancingPeer.LastSendOutgoingTime > 100)
			{
				if (!didSendAcks)
				{
					backgroundStopwatch.Reset();
					backgroundStopwatch.Start();
				}
				if (backgroundStopwatch.ElapsedMilliseconds > KeepAliveInBackground)
				{
					if (DisconnectAfterKeepAlive)
					{
						Client.Disconnect();
					}
					return true;
				}
				didSendAcks = true;
				CountSendAcksOnly++;
				Client.LoadBalancingPeer.SendAcksOnly();
			}
			else
			{
				didSendAcks = false;
			}
		}
		return true;
	}
}
