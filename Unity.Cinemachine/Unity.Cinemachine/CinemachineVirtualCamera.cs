using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[Obsolete("CinemachineVirtualCamera is deprecated. Use CinemachineCamera instead.")]
[DisallowMultipleComponent]
[ExecuteAlways]
[ExcludeFromPreset]
[AddComponentMenu("")]
public class CinemachineVirtualCamera : CinemachineVirtualCameraBase, AxisState.IRequiresInput
{
	[Serializable]
	private struct LegacyTransitionParams
	{
		[FormerlySerializedAs("m_PositionBlending")]
		public int m_BlendHint;

		public bool m_InheritPosition;

		public CinemachineLegacyCameraEvents.OnCameraLiveEvent m_OnCameraLive;
	}

	public delegate Transform CreatePipelineDelegate(CinemachineVirtualCamera vcam, string name, CinemachineComponentBase[] copyFrom);

	public delegate void DestroyPipelineDelegate(GameObject pipeline);

	[Tooltip("The object that the camera wants to look at (the Aim target).  If this is null, then the vcam's Transform orientation will define the camera's orientation.")]
	[NoSaveDuringPlay]
	[VcamTargetProperty]
	public Transform m_LookAt;

	[Tooltip("The object that the camera wants to move with (the Body target).  If this is null, then the vcam's Transform position will define the camera's position.")]
	[NoSaveDuringPlay]
	[VcamTargetProperty]
	public Transform m_Follow;

	[Tooltip("Specifies the lens properties of this Virtual Camera.  This generally mirrors the Unity Camera's lens settings, and will be used to drive the Unity camera when the vcam is active.")]
	[FormerlySerializedAs("m_LensAttributes")]
	public LegacyLensSettings m_Lens = LegacyLensSettings.Default;

	[Tooltip("Hint for transitioning to and from this CinemachineCamera.  Hints can be combined, although not all combinations make sense.  In the case of conflicting hints, Cinemachine will make an arbitrary choice.")]
	public CinemachineCore.BlendHints BlendHint;

	[Tooltip("This event fires when a transition occurs")]
	public CinemachineLegacyCameraEvents.OnCameraLiveEvent m_OnCameraLiveEvent = new CinemachineLegacyCameraEvents.OnCameraLiveEvent();

	[HideInInspector]
	[SerializeField]
	[NoSaveDuringPlay]
	internal string[] m_ExcludedPropertiesInInspector = new string[1] { "m_Script" };

	[HideInInspector]
	[SerializeField]
	[NoSaveDuringPlay]
	internal CinemachineCore.Stage[] m_LockStageInInspector;

	[FormerlySerializedAs("m_Transitions")]
	[SerializeField]
	[HideInInspector]
	private LegacyTransitionParams m_LegacyTransitions;

	private const string PipelineName = "cm";

	public static CreatePipelineDelegate CreatePipelineOverride;

	public static DestroyPipelineDelegate DestroyPipelineOverride;

	private CameraState m_State = CameraState.Default;

	private CinemachineComponentBase[] m_ComponentPipeline;

	[SerializeField]
	[HideInInspector]
	private Transform m_ComponentOwner;

	private LensSettings m_LensSettings;

	private Transform mCachedLookAtTarget;

	private CinemachineVirtualCameraBase mCachedLookAtTargetVcam;

	protected internal override bool IsDprecated => true;

	public override CameraState State => m_State;

	public override Transform LookAt
	{
		get
		{
			return ResolveLookAt(m_LookAt);
		}
		set
		{
			m_LookAt = value;
		}
	}

	public override Transform Follow
	{
		get
		{
			return ResolveFollow(m_Follow);
		}
		set
		{
			m_Follow = value;
		}
	}

	protected internal override void PerformLegacyUpgrade(int streamedVersion)
	{
		base.PerformLegacyUpgrade(streamedVersion);
		if (streamedVersion >= 20221011)
		{
			return;
		}
		if (m_LegacyTransitions.m_BlendHint != 0)
		{
			if (m_LegacyTransitions.m_BlendHint == 3)
			{
				BlendHint = CinemachineCore.BlendHints.ScreenSpaceAimWhenTargetsDiffer;
			}
			else
			{
				BlendHint = (CinemachineCore.BlendHints)m_LegacyTransitions.m_BlendHint;
			}
			m_LegacyTransitions.m_BlendHint = 0;
		}
		if (m_LegacyTransitions.m_InheritPosition)
		{
			BlendHint |= CinemachineCore.BlendHints.InheritPosition;
			m_LegacyTransitions.m_InheritPosition = false;
		}
		if (m_LegacyTransitions.m_OnCameraLive != null)
		{
			m_OnCameraLiveEvent = m_LegacyTransitions.m_OnCameraLive;
			m_LegacyTransitions.m_OnCameraLive = null;
		}
	}

	public override float GetMaxDampTime()
	{
		float num = base.GetMaxDampTime();
		UpdateComponentPipeline();
		if (m_ComponentPipeline != null)
		{
			for (int i = 0; i < m_ComponentPipeline.Length; i++)
			{
				num = Mathf.Max(num, m_ComponentPipeline[i].GetMaxDampTime());
			}
		}
		return num;
	}

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		UpdateTargetCache();
		if (deltaTime < 0f)
		{
			PreviousStateIsValid = false;
		}
		m_State = CalculateNewState(worldUp, deltaTime);
		m_State.BlendHint = (CameraState.BlendHints)BlendHint;
		if (Follow != null)
		{
			base.transform.position = State.RawPosition;
		}
		if (LookAt != null)
		{
			base.transform.rotation = State.RawOrientation;
		}
		PreviousStateIsValid = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_LensSettings = m_Lens.ToLensSettings();
		m_State = PullStateFromVirtualCamera(Vector3.up, ref m_LensSettings);
		InvalidateComponentPipeline();
	}

	protected override void OnDestroy()
	{
		foreach (Transform item in base.transform)
		{
			if (item.GetComponent<CinemachinePipeline>() != null)
			{
				item.gameObject.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
			}
		}
		base.OnDestroy();
	}

	protected void OnValidate()
	{
		m_Lens.Validate();
	}

	private void OnTransformChildrenChanged()
	{
		InvalidateComponentPipeline();
	}

	private void Reset()
	{
		DestroyPipeline();
		UpdateComponentPipeline();
		Priority = default(PrioritySettings);
		OutputChannel = OutputChannels.Default;
	}

	internal void DestroyPipeline()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in base.transform)
		{
			if (item.GetComponent<CinemachinePipeline>() != null)
			{
				list.Add(item);
			}
		}
		foreach (Transform item2 in list)
		{
			if (DestroyPipelineOverride != null)
			{
				DestroyPipelineOverride(item2.gameObject);
				continue;
			}
			CinemachineComponentBase[] components = item2.GetComponents<CinemachineComponentBase>();
			for (int i = 0; i < components.Length; i++)
			{
				UnityEngine.Object.Destroy(components[i]);
			}
			if (!RuntimeUtility.IsPrefab(base.gameObject))
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		m_ComponentOwner = null;
		InvalidateComponentPipeline();
		PreviousStateIsValid = false;
	}

	internal Transform CreatePipeline(CinemachineVirtualCamera copyFrom)
	{
		CinemachineComponentBase[] copyFrom2 = null;
		if (copyFrom != null)
		{
			copyFrom.InvalidateComponentPipeline();
			copyFrom2 = copyFrom.GetComponentPipeline();
		}
		Transform result = null;
		if (CreatePipelineOverride != null)
		{
			result = CreatePipelineOverride(this, "cm", copyFrom2);
		}
		else if (!RuntimeUtility.IsPrefab(base.gameObject))
		{
			GameObject obj = new GameObject("cm");
			obj.transform.parent = base.transform;
			obj.AddComponent<CinemachinePipeline>();
			result = obj.transform;
		}
		PreviousStateIsValid = false;
		return result;
	}

	public void InvalidateComponentPipeline()
	{
		m_ComponentPipeline = null;
	}

	public Transform GetComponentOwner()
	{
		UpdateComponentPipeline();
		return m_ComponentOwner;
	}

	public CinemachineComponentBase[] GetComponentPipeline()
	{
		UpdateComponentPipeline();
		return m_ComponentPipeline;
	}

	public override CinemachineComponentBase GetCinemachineComponent(CinemachineCore.Stage stage)
	{
		CinemachineComponentBase[] componentPipeline = GetComponentPipeline();
		if (componentPipeline != null)
		{
			CinemachineComponentBase[] array = componentPipeline;
			foreach (CinemachineComponentBase cinemachineComponentBase in array)
			{
				if (cinemachineComponentBase.Stage == stage)
				{
					return cinemachineComponentBase;
				}
			}
		}
		return null;
	}

	public T GetCinemachineComponent<T>() where T : CinemachineComponentBase
	{
		CinemachineComponentBase[] componentPipeline = GetComponentPipeline();
		if (componentPipeline != null)
		{
			CinemachineComponentBase[] array = componentPipeline;
			foreach (CinemachineComponentBase cinemachineComponentBase in array)
			{
				if (cinemachineComponentBase is T)
				{
					return cinemachineComponentBase as T;
				}
			}
		}
		return null;
	}

	public T AddCinemachineComponent<T>() where T : CinemachineComponentBase
	{
		Transform componentOwner = GetComponentOwner();
		if (componentOwner == null)
		{
			return null;
		}
		CinemachineComponentBase[] components = componentOwner.GetComponents<CinemachineComponentBase>();
		T val = componentOwner.gameObject.AddComponent<T>();
		if (val != null && components != null)
		{
			CinemachineCore.Stage stage = val.Stage;
			for (int num = components.Length - 1; num >= 0; num--)
			{
				if (components[num].Stage == stage)
				{
					components[num].enabled = false;
					RuntimeUtility.DestroyObject(components[num]);
				}
			}
		}
		InvalidateComponentPipeline();
		return val;
	}

	public void DestroyCinemachineComponent<T>() where T : CinemachineComponentBase
	{
		CinemachineComponentBase[] componentPipeline = GetComponentPipeline();
		if (componentPipeline == null)
		{
			return;
		}
		CinemachineComponentBase[] array = componentPipeline;
		foreach (CinemachineComponentBase cinemachineComponentBase in array)
		{
			if (cinemachineComponentBase is T)
			{
				cinemachineComponentBase.enabled = false;
				RuntimeUtility.DestroyObject(cinemachineComponentBase);
				InvalidateComponentPipeline();
			}
		}
	}

	private void UpdateComponentPipeline()
	{
		if (m_ComponentOwner != null && m_ComponentPipeline != null)
		{
			return;
		}
		m_ComponentOwner = null;
		List<CinemachineComponentBase> list = new List<CinemachineComponentBase>();
		foreach (Transform item in base.transform)
		{
			if (!(item.GetComponent<CinemachinePipeline>() != null))
			{
				continue;
			}
			CinemachineComponentBase[] components = item.GetComponents<CinemachineComponentBase>();
			foreach (CinemachineComponentBase cinemachineComponentBase in components)
			{
				if (cinemachineComponentBase != null && cinemachineComponentBase.enabled)
				{
					list.Add(cinemachineComponentBase);
				}
			}
			m_ComponentOwner = item;
			break;
		}
		if (m_ComponentOwner == null)
		{
			m_ComponentOwner = CreatePipeline(null);
		}
		if (m_ComponentOwner != null && m_ComponentOwner.gameObject != null)
		{
			list.Sort((CinemachineComponentBase c1, CinemachineComponentBase c2) => c1.Stage - c2.Stage);
			m_ComponentPipeline = list.ToArray();
		}
	}

	internal static void SetFlagsForHiddenChild(GameObject child)
	{
		if (child != null)
		{
			child.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
		}
	}

	private CameraState CalculateNewState(Vector3 worldUp, float deltaTime)
	{
		FollowTargetAttachment = 1f;
		LookAtTargetAttachment = 1f;
		m_LensSettings = m_Lens.ToLensSettings();
		CameraState newState = PullStateFromVirtualCamera(worldUp, ref m_LensSettings);
		Transform lookAt = LookAt;
		if (lookAt != mCachedLookAtTarget)
		{
			mCachedLookAtTarget = lookAt;
			mCachedLookAtTargetVcam = null;
			if (lookAt != null)
			{
				mCachedLookAtTargetVcam = lookAt.GetComponent<CinemachineVirtualCameraBase>();
			}
		}
		if (lookAt != null)
		{
			if (mCachedLookAtTargetVcam != null)
			{
				newState.ReferenceLookAt = mCachedLookAtTargetVcam.State.GetFinalPosition();
			}
			else
			{
				newState.ReferenceLookAt = TargetPositionCache.GetTargetPosition(lookAt);
			}
		}
		UpdateComponentPipeline();
		InvokePrePipelineMutateCameraStateCallback(this, ref newState, deltaTime);
		if (m_ComponentPipeline == null)
		{
			for (CinemachineCore.Stage stage = CinemachineCore.Stage.Body; stage <= CinemachineCore.Stage.Finalize; stage++)
			{
				InvokePostPipelineStageCallback(this, stage, ref newState, deltaTime);
			}
		}
		else
		{
			for (int i = 0; i < m_ComponentPipeline.Length; i++)
			{
				if (m_ComponentPipeline[i] != null)
				{
					m_ComponentPipeline[i].PrePipelineMutateCameraState(ref newState, deltaTime);
				}
			}
			int num = 0;
			CinemachineComponentBase cinemachineComponentBase = null;
			for (CinemachineCore.Stage stage2 = CinemachineCore.Stage.Body; stage2 <= CinemachineCore.Stage.Finalize; stage2++)
			{
				CinemachineComponentBase cinemachineComponentBase2 = ((num < m_ComponentPipeline.Length) ? m_ComponentPipeline[num] : null);
				if (cinemachineComponentBase2 != null && stage2 == cinemachineComponentBase2.Stage)
				{
					num++;
					if (stage2 == CinemachineCore.Stage.Body && cinemachineComponentBase2.BodyAppliesAfterAim)
					{
						cinemachineComponentBase = cinemachineComponentBase2;
						continue;
					}
					cinemachineComponentBase2.MutateCameraState(ref newState, deltaTime);
				}
				InvokePostPipelineStageCallback(this, stage2, ref newState, deltaTime);
				if (stage2 == CinemachineCore.Stage.Aim && cinemachineComponentBase != null)
				{
					cinemachineComponentBase.MutateCameraState(ref newState, deltaTime);
					InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Body, ref newState, deltaTime);
				}
			}
		}
		return newState;
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		if (target == Follow)
		{
			base.transform.position += positionDelta;
			m_State.RawPosition += positionDelta;
		}
		UpdateComponentPipeline();
		if (m_ComponentPipeline != null)
		{
			for (int i = 0; i < m_ComponentPipeline.Length; i++)
			{
				m_ComponentPipeline[i].OnTargetObjectWarped(target, positionDelta);
			}
		}
		base.OnTargetObjectWarped(target, positionDelta);
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		PreviousStateIsValid = true;
		base.transform.SetPositionAndRotation(pos, rot);
		m_State.RawPosition = pos;
		m_State.RawOrientation = rot;
		UpdateComponentPipeline();
		if (m_ComponentPipeline != null)
		{
			for (int i = 0; i < m_ComponentPipeline.Length; i++)
			{
				m_ComponentPipeline[i].ForceCameraPosition(pos, rot);
			}
		}
		base.ForceCameraPosition(pos, rot);
	}

	internal void SetStateRawPosition(Vector3 pos)
	{
		m_State.RawPosition = pos;
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
		InvokeOnTransitionInExtensions(fromCam, worldUp, deltaTime);
		bool flag = false;
		if (fromCam != null && (State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && !CinemachineCore.IsLiveInBlend(this))
		{
			ForceCameraPosition(fromCam.State.GetFinalPosition(), fromCam.State.GetFinalOrientation());
		}
		UpdateComponentPipeline();
		if (m_ComponentPipeline != null)
		{
			for (int i = 0; i < m_ComponentPipeline.Length; i++)
			{
				if (m_ComponentPipeline[i].OnTransitionFromCamera(fromCam, worldUp, deltaTime))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			InternalUpdateCameraState(worldUp, deltaTime);
			InternalUpdateCameraState(worldUp, deltaTime);
		}
		else
		{
			UpdateCameraState(worldUp, deltaTime);
		}
		m_OnCameraLiveEvent?.Invoke(this, fromCam);
	}

	bool AxisState.IRequiresInput.RequiresInput()
	{
		if (base.Extensions != null)
		{
			for (int i = 0; i < base.Extensions.Count; i++)
			{
				if (base.Extensions[i] is AxisState.IRequiresInput)
				{
					return true;
				}
			}
		}
		UpdateComponentPipeline();
		if (m_ComponentPipeline != null)
		{
			for (int j = 0; j < m_ComponentPipeline.Length; j++)
			{
				if (m_ComponentPipeline[j] is AxisState.IRequiresInput)
				{
					return true;
				}
			}
		}
		return false;
	}
}
