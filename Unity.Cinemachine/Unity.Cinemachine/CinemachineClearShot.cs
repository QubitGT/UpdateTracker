using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[DisallowMultipleComponent]
[ExecuteAlways]
[ExcludeFromPreset]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Cinemachine ClearShot")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineClearShot.html")]
public class CinemachineClearShot : CinemachineCameraManagerBase
{
	private struct Pair
	{
		public int a;

		public float b;
	}

	[Tooltip("Wait this many seconds before activating a new child camera")]
	[FormerlySerializedAs("m_ActivateAfter")]
	public float ActivateAfter;

	[Tooltip("An active camera must be active for at least this many seconds")]
	[FormerlySerializedAs("m_MinDuration")]
	public float MinDuration;

	[Tooltip("If checked, camera choice will be randomized if multiple cameras are equally desirable.  Otherwise, child list order and child camera priority will be used.")]
	[FormerlySerializedAs("m_RandomizeChoice")]
	public bool RandomizeChoice;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_LookAt")]
	private Transform m_LegacyLookAt;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_Follow")]
	private Transform m_LegacyFollow;

	private float m_ActivationTime;

	private float m_PendingActivationTime;

	private CinemachineVirtualCameraBase m_PendingCamera;

	private bool m_RandomizeNow;

	private List<CinemachineVirtualCameraBase> m_RandomizedChildren;

	protected override void Reset()
	{
		base.Reset();
		ActivateAfter = 0f;
		MinDuration = 0f;
		RandomizeChoice = false;
		DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);
		CustomBlends = null;
	}

	protected internal override void PerformLegacyUpgrade(int streamedVersion)
	{
		base.PerformLegacyUpgrade(streamedVersion);
		if (streamedVersion < 20220721 && (m_LegacyLookAt != null || m_LegacyFollow != null))
		{
			DefaultTarget = new DefaultTargetSettings
			{
				Enabled = true,
				Target = new CameraTarget
				{
					LookAtTarget = m_LegacyLookAt,
					TrackingTarget = m_LegacyFollow,
					CustomLookAtTarget = (m_LegacyLookAt != m_LegacyFollow)
				}
			};
			m_LegacyLookAt = (m_LegacyFollow = null);
		}
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		if (RandomizeChoice && !base.IsBlending)
		{
			m_RandomizedChildren = null;
		}
		base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
	}

	public void ResetRandomization()
	{
		m_RandomizedChildren = null;
		m_RandomizeNow = true;
	}

	protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
	{
		if (!PreviousStateIsValid)
		{
			m_ActivationTime = 0f;
			m_PendingActivationTime = 0f;
			m_PendingCamera = null;
			m_RandomizedChildren = null;
		}
		CinemachineVirtualCameraBase cinemachineVirtualCameraBase = base.LiveChild as CinemachineVirtualCameraBase;
		if (base.ChildCameras == null || base.ChildCameras.Count == 0)
		{
			m_ActivationTime = 0f;
			return null;
		}
		List<CinemachineVirtualCameraBase> list = base.ChildCameras;
		if (!RandomizeChoice)
		{
			m_RandomizedChildren = null;
		}
		else if (list.Count > 1)
		{
			if (m_RandomizedChildren == null)
			{
				m_RandomizedChildren = Randomize(list);
			}
			list = m_RandomizedChildren;
		}
		if (cinemachineVirtualCameraBase != null && (!cinemachineVirtualCameraBase.IsValid || !cinemachineVirtualCameraBase.gameObject.activeSelf))
		{
			cinemachineVirtualCameraBase = null;
		}
		CinemachineVirtualCameraBase cinemachineVirtualCameraBase2 = cinemachineVirtualCameraBase;
		for (int i = 0; i < list.Count; i++)
		{
			CinemachineVirtualCameraBase cinemachineVirtualCameraBase3 = list[i];
			if (cinemachineVirtualCameraBase3 != null && cinemachineVirtualCameraBase3.gameObject.activeInHierarchy && (cinemachineVirtualCameraBase2 == null || cinemachineVirtualCameraBase3.State.ShotQuality > cinemachineVirtualCameraBase2.State.ShotQuality || (cinemachineVirtualCameraBase3.State.ShotQuality == cinemachineVirtualCameraBase2.State.ShotQuality && cinemachineVirtualCameraBase3.Priority.Value > cinemachineVirtualCameraBase2.Priority.Value) || (RandomizeChoice && m_RandomizeNow && cinemachineVirtualCameraBase3 != cinemachineVirtualCameraBase && cinemachineVirtualCameraBase3.State.ShotQuality == cinemachineVirtualCameraBase2.State.ShotQuality && cinemachineVirtualCameraBase3.Priority.Value == cinemachineVirtualCameraBase2.Priority.Value)))
			{
				cinemachineVirtualCameraBase2 = cinemachineVirtualCameraBase3;
			}
		}
		m_RandomizeNow = false;
		float currentTime = CinemachineCore.CurrentTime;
		if (m_ActivationTime != 0f)
		{
			if (cinemachineVirtualCameraBase == cinemachineVirtualCameraBase2)
			{
				m_PendingActivationTime = 0f;
				m_PendingCamera = null;
				return cinemachineVirtualCameraBase2;
			}
			if (PreviousStateIsValid && m_PendingActivationTime != 0f && m_PendingCamera == cinemachineVirtualCameraBase2)
			{
				if (currentTime - m_PendingActivationTime > ActivateAfter && currentTime - m_ActivationTime > MinDuration)
				{
					m_RandomizedChildren = null;
					m_ActivationTime = currentTime;
					m_PendingActivationTime = 0f;
					m_PendingCamera = null;
					return cinemachineVirtualCameraBase2;
				}
				return cinemachineVirtualCameraBase;
			}
		}
		m_PendingActivationTime = 0f;
		m_PendingCamera = null;
		if (PreviousStateIsValid && m_ActivationTime > 0f && (ActivateAfter > 0f || currentTime - m_ActivationTime < MinDuration))
		{
			m_PendingCamera = cinemachineVirtualCameraBase2;
			m_PendingActivationTime = currentTime;
			return cinemachineVirtualCameraBase;
		}
		m_RandomizedChildren = null;
		m_ActivationTime = currentTime;
		return cinemachineVirtualCameraBase2;
	}

	private static List<CinemachineVirtualCameraBase> Randomize(List<CinemachineVirtualCameraBase> src)
	{
		List<Pair> list = new List<Pair>();
		for (int i = 0; i < src.Count; i++)
		{
			Pair pair = default(Pair);
			pair.a = i;
			pair.b = Random.Range(0f, 1000f);
			Pair item = pair;
			list.Add(item);
		}
		list.Sort((Pair p1, Pair p2) => (int)p1.b - (int)p2.b);
		List<CinemachineVirtualCameraBase> list2 = new List<CinemachineVirtualCameraBase>(src.Count);
		for (int j = 0; j < src.Count; j++)
		{
			list2.Add(src[list[j].a]);
		}
		return list2;
	}
}
