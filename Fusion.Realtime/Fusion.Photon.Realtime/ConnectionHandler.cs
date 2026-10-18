using System;
using System.Diagnostics;
using System.Threading;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Fusion.Photon.Realtime;

internal class ConnectionHandler : MonoBehaviour
{
	public bool DisconnectAfterKeepAlive = false;

	public int KeepAliveInBackground = 60000;

	public bool ApplyDontDestroyOnLoad = true;

	[NonSerialized]
	public static bool AppQuits;

	[NonSerialized]
	public static bool AppPause;

	[NonSerialized]
	public static bool AppPauseRecent;

	[NonSerialized]
	public static bool AppOutOfFocus;

	[NonSerialized]
	public static bool AppOutOfFocusRecent;

	private bool didSendAcks;

	private readonly Stopwatch backgroundStopwatch = new Stopwatch();

	private System.Threading.Timer stateTimer;

	public LoadBalancingClient Client { get; set; }

	public int CountSendAcksOnly { get; private set; }

	public bool FallbackThreadRunning { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void StaticReset()
	{
		AppQuits = false;
		AppPause = false;
		AppPauseRecent = false;
		AppOutOfFocus = false;
		AppOutOfFocusRecent = false;
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
				Client.Disconnect(DisconnectCause.ApplicationQuit);
				Client.LoadBalancingPeer.StopThread();
				Client.LoadBalancingPeer.IsSimulationEnabled = false;
			}
			SupportClass.StopAllBackgroundCalls();
		}
	}

	public void OnApplicationQuit()
	{
		AppQuits = true;
	}

	public void OnApplicationPause(bool pause)
	{
		AppPause = pause;
		if (pause)
		{
			AppPauseRecent = true;
			CancelInvoke("ResetAppPauseRecent");
		}
		else
		{
			Invoke("ResetAppPauseRecent", 5f);
		}
	}

	private void ResetAppPauseRecent()
	{
		AppPauseRecent = false;
	}

	public void OnApplicationFocus(bool focus)
	{
		AppOutOfFocus = !focus;
		if (!focus)
		{
			AppOutOfFocusRecent = true;
			CancelInvoke("ResetAppOutOfFocusRecent");
		}
		else
		{
			Invoke("ResetAppOutOfFocusRecent", 5f);
		}
	}

	private void ResetAppOutOfFocusRecent()
	{
		AppOutOfFocusRecent = false;
	}

	public static bool IsNetworkReachableUnity()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	public void StartFallbackSendAckThread()
	{
		if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL)
		{
			if (!FallbackThreadRunning)
			{
				InvokeRepeating("RealtimeFallbackInvoke", 0.05f, 0.05f);
			}
		}
		else
		{
			if (stateTimer != null)
			{
				return;
			}
			stateTimer = new System.Threading.Timer(RealtimeFallback, null, 50, 50);
		}
		FallbackThreadRunning = true;
	}

	public void StopFallbackSendAckThread()
	{
		if (RuntimeUnityFlagsSetup.IsUNITY_WEBGL)
		{
			if (FallbackThreadRunning)
			{
				CancelInvoke("RealtimeFallbackInvoke");
			}
		}
		else if (stateTimer != null)
		{
			stateTimer.Dispose();
			stateTimer = null;
		}
		FallbackThreadRunning = false;
	}

	public void RealtimeFallbackInvoke()
	{
		RealtimeFallback();
	}

	public void RealtimeFallback(object state = null)
	{
		if (Client == null)
		{
			return;
		}
		if (Client.IsConnected && Client.LoadBalancingPeer.ConnectionTime - Client.LoadBalancingPeer.LastSendOutgoingTime > 100)
		{
			if (!didSendAcks)
			{
				backgroundStopwatch.Reset();
				backgroundStopwatch.Start();
			}
			if (backgroundStopwatch.ElapsedMilliseconds > KeepAliveInBackground)
			{
				if (DisconnectAfterKeepAlive && Client.State != ClientState.Disconnecting)
				{
					Client.Disconnect();
				}
			}
			else
			{
				didSendAcks = true;
				CountSendAcksOnly++;
				Client.LoadBalancingPeer.SendAcksOnly();
			}
		}
		else
		{
			if (backgroundStopwatch.IsRunning)
			{
				backgroundStopwatch.Reset();
			}
			didSendAcks = false;
		}
	}
}
