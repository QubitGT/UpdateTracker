using System;
using System.Threading;
using AOT;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

namespace Oculus.Haptics;

public class Haptics : IDisposable
{
	protected static Haptics instance;

	public const string HapticsSDKTelemetryName = "haptics_sdk";

	private static SynchronizationContext syncContext;

	public static bool IsPCMHaptics { get; private set; }

	public static Haptics Instance
	{
		get
		{
			if (!IsSupportedPlatform())
			{
				Debug.LogError("Error: This platform is not supported for haptics");
				instance = null;
				return null;
			}
			if (instance == null)
			{
				instance = new Haptics();
			}
			if (!EnsureInitialized())
			{
				instance = null;
			}
			return instance;
		}
	}

	private static bool IsSupportedPlatform()
	{
		return true;
	}

	private static bool IsPcmHapticsExtensionEnabled()
	{
		string[] enabledExtensions = OpenXRRuntime.GetEnabledExtensions();
		for (int i = 0; i < enabledExtensions.Length; i++)
		{
			if (enabledExtensions[i].Equals("XR_FB_haptic_pcm"))
			{
				return true;
			}
		}
		return false;
	}

	[MonoPInvokeCallback(typeof(Ffi.HapticsSdkPlayCallback))]
	private static void PlayCallback(IntPtr context, Ffi.Controller controller, float duration, float amplitude)
	{
		syncContext.Post(delegate
		{
			switch (controller)
			{
			case Ffi.Controller.Left:
				InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).SendHapticImpulse(0u, amplitude, duration);
				break;
			case Ffi.Controller.Right:
				InputDevices.GetDeviceAtXRNode(XRNode.RightHand).SendHapticImpulse(0u, amplitude, duration);
				break;
			}
		}, null);
	}

	protected Haptics()
	{
	}

	private static bool EnsureInitialized()
	{
		if (IsInitialized())
		{
			return true;
		}
		if (IsPcmHapticsExtensionEnabled() && Ffi.Succeeded(Ffi.initialize_with_ovr_plugin("Unity", Application.unityVersion, "78.0.0-mainline.0")))
		{
			Debug.Log("Initialized with OVRPlugin backend");
			IsPCMHaptics = true;
			return true;
		}
		if (Ffi.Succeeded(Ffi.initialize_with_callback_backend(IntPtr.Zero, PlayCallback)))
		{
			Debug.Log("Initialized with callback backend");
			syncContext = SynchronizationContext.Current;
			return true;
		}
		Debug.LogError("Error: " + Ffi.error_message());
		return false;
	}

	private static bool IsInitialized()
	{
		if (Ffi.Failed(Ffi.initialized(out var initialized)))
		{
			Debug.LogError("Failed to get initialization state");
			return false;
		}
		return initialized;
	}

	public int LoadClip(string clipJson)
	{
		int clip_id_out = -1;
		return Ffi.load_clip(clipJson, out clip_id_out) switch
		{
			Ffi.Result.LoadClipFailed => throw new FormatException("Invalid format for clip: " + clipJson + "."), 
			Ffi.Result.InvalidUtf8 => throw new FormatException("Invalid UTF8 encoding for clip: " + clipJson + "."), 
			_ => clip_id_out, 
		};
	}

	public bool ReleaseClip(int clipId)
	{
		return Ffi.Succeeded(Ffi.release_clip(clipId));
	}

	public int CreateHapticPlayer()
	{
		int player_id = -1;
		Ffi.create_player(out player_id);
		return player_id;
	}

	public void SetHapticPlayerClip(int playerId, int clipId)
	{
		switch (Ffi.player_set_clip(playerId, clipId))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.ClipIdInvalid:
			throw new ArgumentException($"Invalid clipId: {clipId}.");
		}
	}

	public void PlayHapticPlayer(int playerId, Controller controller)
	{
		Ffi.Controller controller2 = Utils.ControllerToFfiController(controller);
		switch (Ffi.player_play(playerId, controller2))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.NoClipLoaded:
			throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
		}
	}

	public void PauseHapticPlayer(int playerId)
	{
		switch (Ffi.player_pause(playerId))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.NoClipLoaded:
			throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
		}
	}

	public void ResumeHapticPlayer(int playerId)
	{
		switch (Ffi.player_resume(playerId))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.NoClipLoaded:
			throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
		}
	}

	public void StopHapticPlayer(int playerId)
	{
		switch (Ffi.player_stop(playerId))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.NoClipLoaded:
			throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
		}
	}

	public void SeekPlaybackPositionHapticPlayer(int playerId, float time)
	{
		switch (Ffi.player_seek(playerId, time))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.NoClipLoaded:
			throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
		case Ffi.Result.PlayerInvalidSeekPosition:
			throw new ArgumentOutOfRangeException($"Invalid time: {time} for player {playerId}." + "Make sure the value is positive and within the playback duration of the currently loaded clip.");
		}
	}

	public float GetClipDuration(int clipId)
	{
		float clip_duration = 0f;
		if (Ffi.Result.ClipIdInvalid == Ffi.clip_duration(clipId, out clip_duration))
		{
			throw new ArgumentException($"Invalid clip ID: {clipId}.");
		}
		return clip_duration;
	}

	public void LoopHapticPlayer(int playerId, bool enabled)
	{
		if (Ffi.Result.PlayerIdInvalid == Ffi.player_set_looping_enabled(playerId, enabled))
		{
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		}
	}

	public bool IsHapticPlayerLooping(int playerId)
	{
		bool looping_enabled = false;
		if (Ffi.Result.PlayerIdInvalid == Ffi.player_looping_enabled(playerId, out looping_enabled))
		{
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		}
		return looping_enabled;
	}

	public void SetAmplitudeHapticPlayer(int playerId, float amplitude)
	{
		switch (Ffi.player_set_amplitude(playerId, amplitude))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.PlayerInvalidAmplitude:
			throw new ArgumentOutOfRangeException($"Invalid amplitude: {amplitude} for player {playerId}." + "Make sure the value is non-negative.");
		}
	}

	public float GetAmplitudeHapticPlayer(int playerId)
	{
		float amplitude = 1f;
		if (Ffi.Result.PlayerIdInvalid == Ffi.player_amplitude(playerId, out amplitude))
		{
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		}
		return amplitude;
	}

	public void SetFrequencyShiftHapticPlayer(int playerId, float amount)
	{
		switch (Ffi.player_set_frequency_shift(playerId, amount))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.PlayerInvalidFrequencyShift:
			throw new ArgumentOutOfRangeException($"Invalid frequency shift amount: {amount} for player {playerId}." + "Make sure the value is on the range -1.0 to 1.0 (inclusive).");
		}
	}

	public float GetFrequencyShiftHapticPlayer(int playerId)
	{
		float frequency_shift = 0f;
		if (Ffi.Result.PlayerIdInvalid == Ffi.player_frequency_shift(playerId, out frequency_shift))
		{
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		}
		return frequency_shift;
	}

	private static uint MapPriority(uint input, int inMin, int inMax, int outMin, int outMax)
	{
		try
		{
			return checked((uint)Math.Round(Utils.Map((int)input, inMin, inMax, outMin, outMax)));
		}
		catch (OverflowException)
		{
			throw new ArgumentOutOfRangeException($"Invalid priority value: {input}. " + "Make sure the value is within the range 0 to 255 (inclusive).");
		}
	}

	public void SetPriorityHapticPlayer(int playerId, uint value)
	{
		switch (Ffi.player_set_priority(playerId, MapPriority(value, 0, 255, 1024, 0)))
		{
		case Ffi.Result.PlayerIdInvalid:
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		case Ffi.Result.PlayerInvalidPriority:
			throw new ArgumentOutOfRangeException($"Invalid priority value: {value} for player {playerId}. " + "Make sure the value is within the range 0 to 255 (inclusive).");
		}
	}

	public uint GetPriorityHapticPlayer(int playerId)
	{
		uint priority = 128u;
		if (Ffi.Result.PlayerIdInvalid == Ffi.player_priority(playerId, out priority))
		{
			throw new ArgumentException($"Invalid player ID: {playerId}.");
		}
		return MapPriority(priority, 0, 1024, 255, 0);
	}

	public bool ReleaseHapticPlayer(int playerId)
	{
		return Ffi.Succeeded(Ffi.release_player(playerId));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (instance != null)
		{
			if (IsInitialized() && Ffi.Failed(Ffi.uninitialize()))
			{
				Debug.LogError("Error: " + Ffi.error_message());
			}
			instance = null;
		}
	}

	~Haptics()
	{
		Dispose(disposing: false);
	}
}
