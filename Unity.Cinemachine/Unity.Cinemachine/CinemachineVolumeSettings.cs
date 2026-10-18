using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[ExecuteAlways]
[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Volume Settings")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineVolumeSettings.html")]
public class CinemachineVolumeSettings : CinemachineExtension
{
	public enum FocusTrackingMode
	{
		None,
		LookAtTarget,
		FollowTarget,
		CustomTarget,
		Camera
	}

	private class VcamExtraState : VcamExtraStateBase
	{
		public VolumeProfile ProfileCopy;

		public void CreateProfileCopy(VolumeProfile source)
		{
			DestroyProfileCopy();
			VolumeProfile volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
			int num = 0;
			while (source != null && num < source.components.Count)
			{
				VolumeComponent item = Object.Instantiate(source.components[num]);
				volumeProfile.components.Add(item);
				volumeProfile.isDirty = true;
				num++;
			}
			ProfileCopy = volumeProfile;
		}

		public void DestroyProfileCopy()
		{
			if (ProfileCopy != null)
			{
				RuntimeUtility.DestroyObject(ProfileCopy);
			}
			ProfileCopy = null;
		}
	}

	public static float s_VolumePriority = 1000f;

	public float Weight = 1f;

	[Tooltip("If the profile has the appropriate overrides, will set the base focus distance to be the distance from the selected target to the camera.The Focus Offset field will then modify that distance.")]
	[FormerlySerializedAs("m_FocusTracking")]
	public FocusTrackingMode FocusTracking;

	[Tooltip("The target to use if Focus Tracks Target is set to Custom Target")]
	[FormerlySerializedAs("m_FocusTarget")]
	public Transform FocusTarget;

	[Tooltip("Offset from target distance, to be used with Focus Tracks Target.  Offsets the sharpest point away from the focus target.")]
	[FormerlySerializedAs("m_FocusOffset")]
	public float FocusOffset;

	[Tooltip("This profile will be applied whenever this virtual camera is live")]
	[FormerlySerializedAs("m_Profile")]
	public VolumeProfile Profile;

	private List<VcamExtraState> m_extraStateCache;

	private const string sVolumeOwnerName = "__CMVolumes";

	private static List<Volume> sVolumes = new List<Volume>();

	public float CalculatedFocusDistance { get; private set; }

	public bool IsValid
	{
		get
		{
			if (Profile != null)
			{
				return Profile.components.Count > 0;
			}
			return false;
		}
	}

	public void InvalidateCachedProfile()
	{
		if (m_extraStateCache == null)
		{
			m_extraStateCache = new List<VcamExtraState>();
		}
		GetAllExtraStates(m_extraStateCache);
		for (int i = 0; i < m_extraStateCache.Count; i++)
		{
			m_extraStateCache[i].DestroyProfileCopy();
		}
	}

	private void OnValidate()
	{
		Weight = Mathf.Max(0f, Weight);
	}

	private void Reset()
	{
		Weight = 1f;
		FocusTracking = FocusTrackingMode.None;
		FocusTarget = null;
		FocusOffset = 0f;
		Profile = null;
	}

	protected override void OnEnable()
	{
		InvalidateCachedProfile();
	}

	protected override void OnDestroy()
	{
		InvalidateCachedProfile();
		base.OnDestroy();
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != CinemachineCore.Stage.Finalize)
		{
			return;
		}
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		if (!IsValid)
		{
			extraState.DestroyProfileCopy();
			return;
		}
		VolumeProfile volumeProfile = Profile;
		if (FocusTracking == FocusTrackingMode.None)
		{
			extraState.DestroyProfileCopy();
		}
		else
		{
			if (extraState.ProfileCopy == null)
			{
				extraState.CreateProfileCopy(Profile);
			}
			volumeProfile = extraState.ProfileCopy;
			if (volumeProfile.TryGet<DepthOfField>(out var component))
			{
				float num = FocusOffset;
				if (FocusTracking == FocusTrackingMode.LookAtTarget)
				{
					num += (state.GetFinalPosition() - state.ReferenceLookAt).magnitude;
				}
				else
				{
					Transform transform = null;
					switch (FocusTracking)
					{
					case FocusTrackingMode.FollowTarget:
						transform = vcam.Follow;
						break;
					case FocusTrackingMode.CustomTarget:
						transform = FocusTarget;
						break;
					}
					if (transform != null)
					{
						num += (state.GetFinalPosition() - transform.position).magnitude;
					}
				}
				num = (CalculatedFocusDistance = Mathf.Max(0f, num));
				component.focusDistance.value = num;
				state.Lens.PhysicalProperties.FocusDistance = num;
				volumeProfile.isDirty = true;
			}
		}
		state.AddCustomBlendable(new CameraState.CustomBlendableItems.Item
		{
			Custom = volumeProfile,
			Weight = Weight
		});
	}

	private static void OnCameraCut(ICinemachineCamera.ActivationEventParams evt)
	{
		if (evt.IsCut)
		{
			CinemachineBrain cinemachineBrain = evt.Origin as CinemachineBrain;
			Camera camera = ((cinemachineBrain == null) ? null : cinemachineBrain.OutputCamera);
			if (camera != null && camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
			{
				component.resetHistory = true;
			}
		}
	}

	private static void ApplyPostFX(CinemachineBrain brain)
	{
		CameraState state = brain.State;
		int numCustomBlendables = state.GetNumCustomBlendables();
		List<Volume> dynamicBrainVolumes = GetDynamicBrainVolumes(brain, numCustomBlendables);
		for (int i = 0; i < dynamicBrainVolumes.Count; i++)
		{
			dynamicBrainVolumes[i].weight = 0f;
			dynamicBrainVolumes[i].sharedProfile = null;
			dynamicBrainVolumes[i].profile = null;
		}
		Volume volume = null;
		int num = 0;
		for (int j = 0; j < numCustomBlendables; j++)
		{
			CameraState.CustomBlendableItems.Item customBlendable = state.GetCustomBlendable(j);
			VolumeProfile volumeProfile = customBlendable.Custom as VolumeProfile;
			if (!(volumeProfile == null))
			{
				Volume volume2 = dynamicBrainVolumes[j];
				if (volume == null)
				{
					volume = volume2;
				}
				volume2.sharedProfile = volumeProfile;
				volume2.isGlobal = true;
				volume2.priority = s_VolumePriority - (float)(numCustomBlendables - j) - 1f;
				volume2.weight = customBlendable.Weight;
				num++;
			}
			if (num > 1)
			{
				volume.weight = 1f;
			}
		}
	}

	private static List<Volume> GetDynamicBrainVolumes(CinemachineBrain brain, int minVolumes)
	{
		GameObject gameObject = null;
		Transform transform = brain.transform;
		int childCount = transform.childCount;
		sVolumes.Clear();
		int num = 0;
		while (gameObject == null && num < childCount)
		{
			GameObject gameObject2 = transform.GetChild(num).gameObject;
			if (gameObject2.hideFlags == HideFlags.HideAndDontSave)
			{
				gameObject2.GetComponents(sVolumes);
				if (sVolumes.Count > 0)
				{
					gameObject = gameObject2;
				}
			}
			num++;
		}
		if (minVolumes > 0)
		{
			if (gameObject == null)
			{
				gameObject = new GameObject("__CMVolumes");
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				gameObject.transform.parent = transform;
			}
			brain.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var component);
			if (component != null)
			{
				int num2 = component.volumeLayerMask;
				for (int i = 0; i < 32; i++)
				{
					if ((num2 & (1 << i)) != 0)
					{
						gameObject.layer = i;
						break;
					}
				}
			}
			while (sVolumes.Count < minVolumes)
			{
				sVolumes.Add(gameObject.gameObject.AddComponent<Volume>());
			}
		}
		return sVolumes;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void InitializeModule()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(ApplyPostFX);
		CinemachineCore.CameraUpdatedEvent.AddListener(ApplyPostFX);
		CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraCut);
		CinemachineCore.CameraActivatedEvent.AddListener(OnCameraCut);
	}
}
