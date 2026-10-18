#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Fusion;

public sealed class NetworkRunnerUpdaterDefault : INetworkRunnerUpdater
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NetworkRunnerUpdate
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NetworkRunnerRender
	{
	}

	private class PlayerLoopSystemRegistration : IDisposable
	{
		public NetworkRunnerUpdaterDefaultInvokeSettings UpdateSettings;

		public NetworkRunnerUpdaterDefaultInvokeSettings RenderSettings;

		public PlayerLoopSystemRegistration(NetworkRunnerUpdaterDefaultInvokeSettings updateSettings, NetworkRunnerUpdaterDefaultInvokeSettings renderSettings)
		{
			UpdateSettings = updateSettings;
			RenderSettings = renderSettings;
			if (UpdateSettings.ReferencePlayerLoopSystem == null)
			{
				throw new ArgumentException("UpdateSettings");
			}
			if (RenderSettings.ReferencePlayerLoopSystem == null)
			{
				throw new ArgumentException("RenderSettings");
			}
			InternalLogStreams.LogDebug?.Log($"Registering in PlayerLoop, Update:{UpdateSettings} Render:{RenderSettings}");
			PlayerLoopSystem parentSystem = PlayerLoop.GetCurrentPlayerLoop();
			UnityPlayerLoopSystemUtils.AddToPlayerLoop(ref parentSystem, UpdateSettings.ReferencePlayerLoopSystem, UpdateSettings.AddMode, typeof(NetworkRunnerUpdate), InvokeUpdate);
			UnityPlayerLoopSystemUtils.AddToPlayerLoop(ref parentSystem, RenderSettings.ReferencePlayerLoopSystem, RenderSettings.AddMode, typeof(NetworkRunnerRender), InvokeRender);
			PlayerLoop.SetPlayerLoop(parentSystem);
		}

		public void Dispose()
		{
			InternalLogStreams.LogDebug?.Log("Unregistering from PlayerLoop");
			PlayerLoopSystem parentSystem = PlayerLoop.GetCurrentPlayerLoop();
			UnityPlayerLoopSystemUtils.RemoveFromPlayerLoop(ref parentSystem, typeof(NetworkRunnerUpdate));
			UnityPlayerLoopSystemUtils.RemoveFromPlayerLoop(ref parentSystem, typeof(NetworkRunnerRender));
			PlayerLoop.SetPlayerLoop(parentSystem);
		}
	}

	private static List<NetworkRunner> _instances = new List<NetworkRunner>();

	private static int _instanceCount = -1;

	private static PlayerLoopSystemRegistration _registration;

	public NetworkRunnerUpdaterDefaultInvokeSettings UpdateSettings = new NetworkRunnerUpdaterDefaultInvokeSettings
	{
		ReferencePlayerLoopSystem = typeof(Update.ScriptRunBehaviourUpdate),
		AddMode = UnityPlayerLoopSystemAddMode.Before
	};

	public NetworkRunnerUpdaterDefaultInvokeSettings RenderSettings = new NetworkRunnerUpdaterDefaultInvokeSettings
	{
		ReferencePlayerLoopSystem = typeof(Update.ScriptRunBehaviourUpdate),
		AddMode = UnityPlayerLoopSystemAddMode.After
	};

	[RuntimeInitializeOnLoadMethod]
	private static void ClearStatics()
	{
		_instances.Clear();
		_instanceCount = -1;
	}

	public static bool RegisterInPlayerLoop(NetworkRunnerUpdaterDefaultInvokeSettings updateSettings, NetworkRunnerUpdaterDefaultInvokeSettings renderSettings)
	{
		if (_registration == null)
		{
			_registration = new PlayerLoopSystemRegistration(updateSettings, renderSettings);
			return true;
		}
		if (_registration.UpdateSettings != updateSettings || _registration.RenderSettings != renderSettings)
		{
			InternalLogStreams.LogDebug?.Warn("PlayerLoopSystemRegistration already exists with different settings (" + $"Update: {_registration.UpdateSettings} vs {updateSettings}, " + $"Render: {_registration.RenderSettings} vs {renderSettings}. " + "If you intend to change the timings, please call UnregisterFromPlayerLoop first.");
		}
		return false;
	}

	public static bool UnregisterFromPlayerLoop()
	{
		if (_registration == null)
		{
			return false;
		}
		_registration.Dispose();
		_registration = null;
		return true;
	}

	void INetworkRunnerUpdater.Initialize(NetworkRunner runner)
	{
		InternalLogStreams.LogDebug?.Log(runner, "Adding to the default updater");
		RegisterInPlayerLoop(UpdateSettings, RenderSettings);
		_instances.Add(runner);
	}

	void INetworkRunnerUpdater.Shutdown(NetworkRunner runner)
	{
		int num = _instances.IndexOf(runner);
		if (num >= 0)
		{
			if (_instanceCount >= 0)
			{
				InternalLogStreams.LogDebug?.Log(_instances[num], "Removing from the default updater");
				_instances[num] = null;
			}
			else
			{
				InternalLogStreams.LogDebug?.Log(_instances[num], "Removing from the default updater (deferred)");
				_instances.RemoveAt(num);
			}
		}
	}

	private static void InvokeUpdate()
	{
		Assert.Check(_instanceCount < 0, "Expected _instanceCount being negative {0}", _instanceCount);
		_instanceCount = _instances.Count;
		for (int i = 0; i < _instanceCount; i++)
		{
			NetworkRunner networkRunner = _instances[i];
			if (BehaviourUtils.IsAlive(networkRunner))
			{
				networkRunner.UpdateInternal(networkRunner._simulation.Config.SimulationUpdateTimeMode switch
				{
					SimulationConfig.SimulationTimeMode.UnscaledDeltaTime => Time.unscaledDeltaTime, 
					SimulationConfig.SimulationTimeMode.DeltaTime => Time.deltaTime, 
					_ => throw new NotImplementedException(), 
				});
			}
		}
	}

	private static void InvokeRender()
	{
		Assert.Check(_instanceCount >= 0, "Expected _instanceCount not being negative {0}", _instanceCount);
		Assert.Check(_instanceCount <= _instances.Count);
		bool flag = false;
		try
		{
			for (int i = 0; i < _instanceCount; i++)
			{
				NetworkRunner networkRunner = _instances[i];
				if (BehaviourUtils.IsAlive(networkRunner))
				{
					networkRunner.RenderInternal();
				}
				else
				{
					flag = true;
				}
			}
		}
		finally
		{
			_instanceCount = -1;
			if (flag)
			{
				_instances.RemoveAll((NetworkRunner x) => BehaviourUtils.IsNotAlive(x));
			}
		}
	}
}
