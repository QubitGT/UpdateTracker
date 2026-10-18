using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

internal static class CameraUpdateManager
{
	private class UpdateStatus
	{
		public int lastUpdateFrame;

		public int lastUpdateFixedFrame;

		public UpdateTracker.UpdateClock lastUpdateMode;
	}

	public enum UpdateFilter
	{
		Fixed = 1,
		Late = 2,
		Smart = 8,
		SmartFixed = 9,
		SmartLate = 10
	}

	private static readonly VirtualCameraRegistry s_CameraRegistry = new VirtualCameraRegistry();

	private static int s_RoundRobinIndex = 0;

	private static int s_RoundRobinSubIndex = 0;

	private static object s_LastFixedUpdateContext;

	private static float s_LastUpdateTime = 0f;

	private static int s_FixedFrameCount = 0;

	private static Dictionary<CinemachineVirtualCameraBase, UpdateStatus> s_UpdateStatus;

	public static UpdateFilter s_CurrentUpdateFilter;

	public static int VirtualCameraCount => s_CameraRegistry.ActiveCameraCount;

	[RuntimeInitializeOnLoadMethod]
	private static void InitializeModule()
	{
		s_UpdateStatus = new Dictionary<CinemachineVirtualCameraBase, UpdateStatus>();
	}

	public static CinemachineVirtualCameraBase GetVirtualCamera(int index)
	{
		return s_CameraRegistry.GetActiveCamera(index);
	}

	public static void AddActiveCamera(CinemachineVirtualCameraBase vcam)
	{
		s_CameraRegistry.AddActiveCamera(vcam);
	}

	public static void RemoveActiveCamera(CinemachineVirtualCameraBase vcam)
	{
		s_CameraRegistry.RemoveActiveCamera(vcam);
	}

	public static void CameraDestroyed(CinemachineVirtualCameraBase vcam)
	{
		s_CameraRegistry.CameraDestroyed(vcam);
		if (s_UpdateStatus != null && s_UpdateStatus.ContainsKey(vcam))
		{
			s_UpdateStatus.Remove(vcam);
		}
	}

	public static void CameraEnabled(CinemachineVirtualCameraBase vcam)
	{
		s_CameraRegistry.CameraEnabled(vcam);
	}

	public static void CameraDisabled(CinemachineVirtualCameraBase vcam)
	{
		s_CameraRegistry.CameraDisabled(vcam);
	}

	public static void ForgetContext(object context)
	{
		if (s_LastFixedUpdateContext == context)
		{
			s_LastFixedUpdateContext = null;
		}
	}

	public static void UpdateAllActiveVirtualCameras(uint channelMask, Vector3 worldUp, float deltaTime, object context)
	{
		if ((s_CurrentUpdateFilter & (UpdateFilter)(-9)) == UpdateFilter.Fixed && (s_LastFixedUpdateContext == null || s_LastFixedUpdateContext == context))
		{
			s_FixedFrameCount++;
			s_LastFixedUpdateContext = context;
		}
		List<List<CinemachineVirtualCameraBase>> allCamerasSortedByNestingLevel = s_CameraRegistry.AllCamerasSortedByNestingLevel;
		float currentTime = CinemachineCore.CurrentTime;
		if (currentTime != s_LastUpdateTime)
		{
			s_LastUpdateTime = currentTime;
			if (allCamerasSortedByNestingLevel.Count > 0)
			{
				if (s_RoundRobinIndex >= allCamerasSortedByNestingLevel.Count)
				{
					s_RoundRobinIndex = 0;
				}
				if (++s_RoundRobinSubIndex >= allCamerasSortedByNestingLevel[s_RoundRobinIndex].Count)
				{
					s_RoundRobinSubIndex = 0;
					if (++s_RoundRobinIndex >= allCamerasSortedByNestingLevel.Count)
					{
						s_RoundRobinIndex = 0;
					}
				}
			}
		}
		for (int num = allCamerasSortedByNestingLevel.Count - 1; num >= 0; num--)
		{
			List<CinemachineVirtualCameraBase> list = allCamerasSortedByNestingLevel[num];
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				CinemachineVirtualCameraBase cinemachineVirtualCameraBase = list[num2];
				if (cinemachineVirtualCameraBase == null)
				{
					list.RemoveAt(num2);
				}
				else if (((uint)cinemachineVirtualCameraBase.OutputChannel & channelMask) != 0)
				{
					if (CinemachineCore.IsLive(cinemachineVirtualCameraBase) || cinemachineVirtualCameraBase.StandbyUpdate == CinemachineVirtualCameraBase.StandbyUpdateMode.Always)
					{
						UpdateVirtualCamera(cinemachineVirtualCameraBase, worldUp, deltaTime);
					}
					else if (cinemachineVirtualCameraBase.StandbyUpdate == CinemachineVirtualCameraBase.StandbyUpdateMode.RoundRobin && s_RoundRobinIndex == num && s_RoundRobinSubIndex == num2 && cinemachineVirtualCameraBase.isActiveAndEnabled)
					{
						UpdateVirtualCamera(cinemachineVirtualCameraBase, worldUp, deltaTime);
					}
				}
			}
		}
	}

	public static void UpdateVirtualCamera(CinemachineVirtualCameraBase vcam, Vector3 worldUp, float deltaTime)
	{
		if (vcam == null)
		{
			return;
		}
		bool num = (s_CurrentUpdateFilter & UpdateFilter.Smart) == UpdateFilter.Smart;
		UpdateTracker.UpdateClock updateClock = (UpdateTracker.UpdateClock)(s_CurrentUpdateFilter & (UpdateFilter)(-9));
		if (num)
		{
			Transform updateTarget = GetUpdateTarget(vcam);
			if (updateTarget == null || UpdateTracker.GetPreferredUpdate(updateTarget) != updateClock)
			{
				return;
			}
		}
		if (s_UpdateStatus == null)
		{
			s_UpdateStatus = new Dictionary<CinemachineVirtualCameraBase, UpdateStatus>();
		}
		if (!s_UpdateStatus.TryGetValue(vcam, out var value))
		{
			value = new UpdateStatus
			{
				lastUpdateMode = UpdateTracker.UpdateClock.Late,
				lastUpdateFrame = CinemachineCore.CurrentUpdateFrame + 2,
				lastUpdateFixedFrame = s_FixedFrameCount + 2
			};
			s_UpdateStatus.Add(vcam, value);
		}
		int num2 = ((updateClock == UpdateTracker.UpdateClock.Late) ? (CinemachineCore.CurrentUpdateFrame - value.lastUpdateFrame) : (s_FixedFrameCount - value.lastUpdateFixedFrame));
		if (deltaTime >= 0f)
		{
			if (num2 == 0 && value.lastUpdateMode == updateClock)
			{
				return;
			}
			if (!CinemachineCore.UnitTestMode && num2 > 0)
			{
				deltaTime *= (float)num2;
			}
		}
		vcam.InternalUpdateCameraState(worldUp, deltaTime);
		value.lastUpdateFrame = CinemachineCore.CurrentUpdateFrame;
		value.lastUpdateFixedFrame = s_FixedFrameCount;
		value.lastUpdateMode = updateClock;
	}

	private static Transform GetUpdateTarget(CinemachineVirtualCameraBase vcam)
	{
		if (vcam == null || vcam.gameObject == null)
		{
			return null;
		}
		Transform lookAt = vcam.LookAt;
		if (lookAt != null)
		{
			return lookAt;
		}
		lookAt = vcam.Follow;
		if (lookAt != null)
		{
			return lookAt;
		}
		return vcam.transform;
	}

	public static UpdateTracker.UpdateClock GetVcamUpdateStatus(CinemachineVirtualCameraBase vcam)
	{
		if (s_UpdateStatus == null || !s_UpdateStatus.TryGetValue(vcam, out var value))
		{
			return UpdateTracker.UpdateClock.Late;
		}
		return value.lastUpdateMode;
	}
}
