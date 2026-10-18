using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Cinemachine;

public static class CinemachineCore
{
	public enum Stage
	{
		Body,
		Aim,
		Noise,
		Finalize
	}

	[Flags]
	public enum BlendHints
	{
		SphericalPosition = 1,
		CylindricalPosition = 2,
		ScreenSpaceAimWhenTargetsDiffer = 4,
		InheritPosition = 8,
		IgnoreTarget = 0x10,
		FreezeWhenBlendingOut = 0x20
	}

	public delegate float AxisInputDelegate(string axisName);

	public delegate CinemachineBlendDefinition GetBlendOverrideDelegate(ICinemachineCamera fromVcam, ICinemachineCamera toVcam, CinemachineBlendDefinition defaultBlend, UnityEngine.Object owner);

	public delegate CinemachineBlend.IBlender GetCustomBlenderDelegate(ICinemachineCamera fromCam, ICinemachineCamera toCam);

	[Serializable]
	public class CameraEvent : UnityEvent<ICinemachineMixer, ICinemachineCamera>
	{
	}

	[Serializable]
	public class BrainEvent : UnityEvent<CinemachineBrain>
	{
	}

	public struct BlendEventParams
	{
		public ICinemachineMixer Origin;

		public CinemachineBlend Blend;
	}

	[Serializable]
	public class BlendEvent : UnityEvent<BlendEventParams>
	{
	}

	internal const int kStreamingVersion = 20241001;

	public const string kPackageRoot = "Packages/com.unity.cinemachine";

	internal static float CurrentUnscaledTimeTimeOverride = -1f;

	internal static bool UnitTestMode = false;

	public static AxisInputDelegate GetInputAxis = (string _003Cp0_003E) => 0f;

	public static float UniformDeltaTimeOverride = -1f;

	public static float CurrentTimeOverride = -1f;

	public static GetBlendOverrideDelegate GetBlendOverride;

	public static GetCustomBlenderDelegate GetCustomBlender;

	public static BrainEvent CameraUpdatedEvent = new BrainEvent();

	public static ICinemachineCamera.ActivationEvent CameraActivatedEvent = new ICinemachineCamera.ActivationEvent();

	public static CameraEvent CameraDeactivatedEvent = new CameraEvent();

	public static BlendEvent BlendCreatedEvent = new BlendEvent();

	public static CameraEvent BlendFinishedEvent = new CameraEvent();

	private static ICinemachineCamera s_SoloCamera;

	internal static float CurrentUnscaledTime
	{
		get
		{
			if (!(CurrentUnscaledTimeTimeOverride >= 0f))
			{
				return Time.unscaledTime;
			}
			return CurrentUnscaledTimeTimeOverride;
		}
	}

	public static float DeltaTime
	{
		get
		{
			if (!(UniformDeltaTimeOverride >= 0f))
			{
				return Time.deltaTime;
			}
			return UniformDeltaTimeOverride;
		}
	}

	public static float CurrentTime
	{
		get
		{
			if (!(CurrentTimeOverride >= 0f))
			{
				return Time.time;
			}
			return CurrentTimeOverride;
		}
	}

	public static int CurrentUpdateFrame { get; internal set; }

	public static int VirtualCameraCount => CameraUpdateManager.VirtualCameraCount;

	public static ICinemachineCamera SoloCamera
	{
		get
		{
			return s_SoloCamera;
		}
		set
		{
			if (value != null && !IsLive(value))
			{
				value.OnCameraActivated(new ICinemachineCamera.ActivationEventParams
				{
					Origin = null,
					OutgoingCamera = null,
					IncomingCamera = value,
					IsCut = true,
					WorldUp = Vector3.up,
					DeltaTime = DeltaTime
				});
			}
			s_SoloCamera = value;
		}
	}

	internal static Color SoloGUIColor()
	{
		return Color.Lerp(Color.red, Color.yellow, 0.8f);
	}

	public static CinemachineVirtualCameraBase GetVirtualCamera(int index)
	{
		return CameraUpdateManager.GetVirtualCamera(index);
	}

	public static bool IsLive(ICinemachineCamera vcam)
	{
		if (vcam != null)
		{
			int activeBrainCount = CinemachineBrain.ActiveBrainCount;
			for (int i = 0; i < activeBrainCount; i++)
			{
				CinemachineBrain activeBrain = CinemachineBrain.GetActiveBrain(i);
				if (activeBrain != null && activeBrain.IsLiveChild(vcam))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsLiveInBlend(ICinemachineCamera vcam)
	{
		if (vcam != null)
		{
			int activeBrainCount = CinemachineBrain.ActiveBrainCount;
			for (int i = 0; i < activeBrainCount; i++)
			{
				CinemachineBrain activeBrain = CinemachineBrain.GetActiveBrain(i);
				if (activeBrain != null && activeBrain.IsLiveInBlend(vcam))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static CinemachineBrain FindPotentialTargetBrain(CinemachineVirtualCameraBase vcam)
	{
		if (vcam != null)
		{
			int activeBrainCount = CinemachineBrain.ActiveBrainCount;
			for (int i = 0; i < activeBrainCount; i++)
			{
				CinemachineBrain activeBrain = CinemachineBrain.GetActiveBrain(i);
				if (activeBrain != null && activeBrain.OutputCamera != null && activeBrain.IsLiveChild(vcam))
				{
					return activeBrain;
				}
			}
			uint outputChannel = (uint)vcam.OutputChannel;
			for (int j = 0; j < activeBrainCount; j++)
			{
				CinemachineBrain activeBrain2 = CinemachineBrain.GetActiveBrain(j);
				if (activeBrain2 != null && activeBrain2.OutputCamera != null && ((uint)activeBrain2.ChannelMask & outputChannel) != 0)
				{
					return activeBrain2;
				}
			}
		}
		return null;
	}

	public static void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		int virtualCameraCount = CameraUpdateManager.VirtualCameraCount;
		for (int i = 0; i < virtualCameraCount; i++)
		{
			GetVirtualCamera(i).OnTargetObjectWarped(target, positionDelta);
		}
	}

	public static void ResetCameraState()
	{
		int virtualCameraCount = CameraUpdateManager.VirtualCameraCount;
		for (int i = 0; i < virtualCameraCount; i++)
		{
			GetVirtualCamera(i).PreviousStateIsValid = false;
		}
		int activeBrainCount = CinemachineBrain.ActiveBrainCount;
		for (int j = 0; j < activeBrainCount; j++)
		{
			CinemachineBrain.GetActiveBrain(j).ResetState();
		}
	}
}
