using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

public abstract class CinemachineVirtualCameraBase : MonoBehaviour, ICinemachineCamera
{
	public enum StandbyUpdateMode
	{
		Never,
		Always,
		RoundRobin
	}

	[NoSaveDuringPlay]
	[Tooltip("Priority can be used to control which Cm Camera is live when multiple CM Cameras are active simultaneously.  The most-recently-activated CinemachineCamera will take control, unless there is another Cm Camera active with a higher priority.  In general, the most-recently-activated highest-priority CinemachineCamera will control the main camera. \n\nThe default priority is value 0.  Often it is sufficient to leave the default setting.  In special cases where you want a CinemachineCamera to have a higher or lower priority value than 0, you can set it here.")]
	[EnabledProperty("Enabled", "(using default)")]
	public PrioritySettings Priority;

	[NoSaveDuringPlay]
	[Tooltip("The output channel functions like Unity layers.  Use it to filter the output of CinemachineCameras to different CinemachineBrains, for instance in a multi-screen environemnt.")]
	public OutputChannels OutputChannel = OutputChannels.Default;

	internal int ActivationId;

	private int m_QueuePriority = int.MaxValue;

	[NonSerialized]
	public float FollowTargetAttachment;

	[NonSerialized]
	public float LookAtTargetAttachment;

	[Tooltip("When the virtual camera is not live, this is how often the virtual camera will be updated.  Set this to tune for performance. Most of the time Never is fine, unless the virtual camera is doing shot evaluation.")]
	[FormerlySerializedAs("m_StandbyUpdate")]
	public StandbyUpdateMode StandbyUpdate = StandbyUpdateMode.RoundRobin;

	private string m_CachedName;

	private bool m_WasStarted;

	private bool m_ChildStatusUpdated;

	private CinemachineVirtualCameraBase m_ParentVcam;

	private Transform m_CachedFollowTarget;

	private CinemachineVirtualCameraBase m_CachedFollowTargetVcam;

	private ICinemachineTargetGroup m_CachedFollowTargetGroup;

	private Transform m_CachedLookAtTarget;

	private CinemachineVirtualCameraBase m_CachedLookAtTargetVcam;

	private ICinemachineTargetGroup m_CachedLookAtTargetGroup;

	[HideInInspector]
	[SerializeField]
	[NoSaveDuringPlay]
	private int m_StreamingVersion;

	[HideInInspector]
	[SerializeField]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_Priority")]
	private int m_LegacyPriority;

	protected internal virtual bool IsDprecated => false;

	internal List<CinemachineExtension> Extensions { get; private set; }

	public string Name
	{
		get
		{
			if (m_CachedName == null)
			{
				m_CachedName = (IsValid ? base.name : "(deleted)");
			}
			return m_CachedName;
		}
	}

	public virtual string Description => "";

	public bool IsValid => !(this == null);

	public abstract CameraState State { get; }

	public ICinemachineMixer ParentCamera
	{
		get
		{
			if (!m_ChildStatusUpdated || !Application.isPlaying)
			{
				UpdateStatusAsChild();
			}
			return m_ParentVcam as ICinemachineMixer;
		}
	}

	public abstract Transform LookAt { get; set; }

	public abstract Transform Follow { get; set; }

	public virtual bool PreviousStateIsValid { get; set; }

	public bool FollowTargetChanged { get; private set; }

	public bool LookAtTargetChanged { get; private set; }

	public ICinemachineTargetGroup FollowTargetAsGroup => m_CachedFollowTargetGroup;

	public CinemachineVirtualCameraBase FollowTargetAsVcam => m_CachedFollowTargetVcam;

	public ICinemachineTargetGroup LookAtTargetAsGroup => m_CachedLookAtTargetGroup;

	public CinemachineVirtualCameraBase LookAtTargetAsVcam => m_CachedLookAtTargetVcam;

	public bool IsLive => CinemachineCore.IsLive(this);

	protected internal virtual void PerformLegacyUpgrade(int streamedVersion)
	{
		if (streamedVersion < 20220601 && m_LegacyPriority != 0)
		{
			Priority.Value = m_LegacyPriority;
			m_LegacyPriority = 0;
		}
	}

	public virtual float GetMaxDampTime()
	{
		float num = 0f;
		if (Extensions != null)
		{
			for (int i = 0; i < Extensions.Count; i++)
			{
				num = Mathf.Max(num, Extensions[i].GetMaxDampTime());
			}
		}
		return num;
	}

	public float DetachedFollowTargetDamp(float initial, float dampTime, float deltaTime)
	{
		dampTime = Mathf.Lerp(Mathf.Max(1f, dampTime), dampTime, FollowTargetAttachment);
		deltaTime = Mathf.Lerp(0f, deltaTime, FollowTargetAttachment);
		return Damper.Damp(initial, dampTime, deltaTime);
	}

	public Vector3 DetachedFollowTargetDamp(Vector3 initial, Vector3 dampTime, float deltaTime)
	{
		dampTime = Vector3.Lerp(Vector3.Max(Vector3.one, dampTime), dampTime, FollowTargetAttachment);
		deltaTime = Mathf.Lerp(0f, deltaTime, FollowTargetAttachment);
		return Damper.Damp(initial, dampTime, deltaTime);
	}

	public Vector3 DetachedFollowTargetDamp(Vector3 initial, float dampTime, float deltaTime)
	{
		dampTime = Mathf.Lerp(Mathf.Max(1f, dampTime), dampTime, FollowTargetAttachment);
		deltaTime = Mathf.Lerp(0f, deltaTime, FollowTargetAttachment);
		return Damper.Damp(initial, dampTime, deltaTime);
	}

	public float DetachedLookAtTargetDamp(float initial, float dampTime, float deltaTime)
	{
		dampTime = Mathf.Lerp(Mathf.Max(1f, dampTime), dampTime, LookAtTargetAttachment);
		deltaTime = Mathf.Lerp(0f, deltaTime, LookAtTargetAttachment);
		return Damper.Damp(initial, dampTime, deltaTime);
	}

	public Vector3 DetachedLookAtTargetDamp(Vector3 initial, Vector3 dampTime, float deltaTime)
	{
		dampTime = Vector3.Lerp(Vector3.Max(Vector3.one, dampTime), dampTime, LookAtTargetAttachment);
		deltaTime = Mathf.Lerp(0f, deltaTime, LookAtTargetAttachment);
		return Damper.Damp(initial, dampTime, deltaTime);
	}

	public Vector3 DetachedLookAtTargetDamp(Vector3 initial, float dampTime, float deltaTime)
	{
		dampTime = Mathf.Lerp(Mathf.Max(1f, dampTime), dampTime, LookAtTargetAttachment);
		deltaTime = Mathf.Lerp(0f, deltaTime, LookAtTargetAttachment);
		return Damper.Damp(initial, dampTime, deltaTime);
	}

	internal void AddExtension(CinemachineExtension extension)
	{
		if (Extensions == null)
		{
			Extensions = new List<CinemachineExtension>();
		}
		else
		{
			Extensions.Remove(extension);
		}
		Extensions.Add(extension);
	}

	internal void RemoveExtension(CinemachineExtension extension)
	{
		if (Extensions != null)
		{
			Extensions.Remove(extension);
		}
	}

	protected void InvokePostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState newState, float deltaTime)
	{
		if (Extensions != null)
		{
			for (int i = 0; i < Extensions.Count; i++)
			{
				CinemachineExtension cinemachineExtension = Extensions[i];
				if (cinemachineExtension == null)
				{
					Extensions.RemoveAt(i);
					i--;
				}
				else if (cinemachineExtension.enabled)
				{
					cinemachineExtension.InvokePostPipelineStageCallback(vcam, stage, ref newState, deltaTime);
				}
			}
		}
		if (ParentCamera is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
		{
			cinemachineVirtualCameraBase.InvokePostPipelineStageCallback(vcam, stage, ref newState, deltaTime);
		}
	}

	protected void InvokePrePipelineMutateCameraStateCallback(CinemachineVirtualCameraBase vcam, ref CameraState newState, float deltaTime)
	{
		if (Extensions != null)
		{
			for (int i = 0; i < Extensions.Count; i++)
			{
				CinemachineExtension cinemachineExtension = Extensions[i];
				if (cinemachineExtension == null)
				{
					Extensions.RemoveAt(i);
					i--;
				}
				else if (cinemachineExtension.enabled)
				{
					cinemachineExtension.PrePipelineMutateCameraStateCallback(vcam, ref newState, deltaTime);
				}
			}
		}
		if (ParentCamera is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
		{
			cinemachineVirtualCameraBase.InvokePrePipelineMutateCameraStateCallback(vcam, ref newState, deltaTime);
		}
	}

	protected bool InvokeOnTransitionInExtensions(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		bool result = false;
		if (Extensions != null)
		{
			for (int i = 0; i < Extensions.Count; i++)
			{
				CinemachineExtension cinemachineExtension = Extensions[i];
				if (cinemachineExtension == null)
				{
					Extensions.RemoveAt(i);
					i--;
				}
				else if (cinemachineExtension.enabled && cinemachineExtension.OnTransitionFromCamera(fromCam, worldUp, deltaTime))
				{
					result = true;
				}
			}
		}
		return result;
	}

	public void UpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		CameraUpdateManager.UpdateVirtualCamera(this, worldUp, deltaTime);
	}

	public abstract void InternalUpdateCameraState(Vector3 worldUp, float deltaTime);

	public virtual void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
	{
		if (evt.IncomingCamera == this)
		{
			OnTransitionFromCamera(evt.OutgoingCamera, evt.WorldUp, evt.DeltaTime);
		}
	}

	public virtual void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			PreviousStateIsValid = false;
		}
	}

	internal void EnsureStarted()
	{
		if (!m_WasStarted)
		{
			m_WasStarted = true;
			if (m_StreamingVersion < 20241001)
			{
				PerformLegacyUpgrade(m_StreamingVersion);
			}
			m_StreamingVersion = 20241001;
			CinemachineExtension[] componentsInChildren = GetComponentsInChildren<CinemachineExtension>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].EnsureStarted();
			}
		}
	}

	protected virtual void OnTransformParentChanged()
	{
		CameraUpdateManager.CameraDisabled(this);
		CameraUpdateManager.CameraEnabled(this);
		UpdateStatusAsChild();
		UpdateVcamPoolStatus();
	}

	protected virtual void OnDestroy()
	{
		CameraUpdateManager.CameraDestroyed(this);
	}

	protected virtual void Start()
	{
		m_WasStarted = true;
		if (m_StreamingVersion < 20241001)
		{
			PerformLegacyUpgrade(m_StreamingVersion);
		}
		m_StreamingVersion = 20241001;
	}

	protected virtual void OnEnable()
	{
		UpdateStatusAsChild();
		UpdateVcamPoolStatus();
		if (!CinemachineCore.IsLive(this))
		{
			PreviousStateIsValid = false;
		}
		CameraUpdateManager.CameraEnabled(this);
		InvalidateCachedTargets();
		CinemachineVirtualCameraBase[] components = GetComponents<CinemachineVirtualCameraBase>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].enabled && components[i] != this)
			{
				CinemachineVirtualCameraBase cinemachineVirtualCameraBase = (components[i].IsDprecated ? components[i] : this);
				if (!cinemachineVirtualCameraBase.IsDprecated)
				{
					Debug.LogWarning(Name + " has multiple CinemachineVirtualCameraBase-derived components.  Disabling " + cinemachineVirtualCameraBase.GetType().Name);
				}
				cinemachineVirtualCameraBase.enabled = false;
			}
		}
	}

	protected virtual void OnDisable()
	{
		UpdateVcamPoolStatus();
		CameraUpdateManager.CameraDisabled(this);
	}

	protected virtual void Update()
	{
		if (Priority.Value != m_QueuePriority)
		{
			UpdateVcamPoolStatus();
		}
	}

	private void UpdateStatusAsChild()
	{
		m_ChildStatusUpdated = true;
		m_ParentVcam = null;
		Transform parent = base.transform.parent;
		if (parent != null)
		{
			parent.TryGetComponent<CinemachineVirtualCameraBase>(out m_ParentVcam);
		}
	}

	public Transform ResolveLookAt(Transform localLookAt)
	{
		Transform transform = localLookAt;
		if (transform == null && ParentCamera is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
		{
			transform = cinemachineVirtualCameraBase.LookAt;
		}
		return transform;
	}

	public Transform ResolveFollow(Transform localFollow)
	{
		Transform transform = localFollow;
		if (transform == null && ParentCamera is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
		{
			transform = cinemachineVirtualCameraBase.Follow;
		}
		return transform;
	}

	private void UpdateVcamPoolStatus()
	{
		CameraUpdateManager.RemoveActiveCamera(this);
		if (m_ParentVcam == null && base.isActiveAndEnabled)
		{
			CameraUpdateManager.AddActiveCamera(this);
		}
		m_QueuePriority = Priority.Value;
	}

	[Obsolete("Please use Prioritize()")]
	public void MoveToTopOfPrioritySubqueue()
	{
		Prioritize();
	}

	public void Prioritize()
	{
		UpdateVcamPoolStatus();
	}

	public virtual void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		OnTargetObjectWarped(this, target, positionDelta);
	}

	private void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
	{
		int? num = Extensions?.Count;
		for (int i = 0; i < num; i++)
		{
			Extensions[i].OnTargetObjectWarped(vcam, target, positionDelta);
		}
		if (ParentCamera is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
		{
			cinemachineVirtualCameraBase.OnTargetObjectWarped(vcam, target, positionDelta);
		}
	}

	public virtual void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		ForceCameraPosition(this, pos, rot);
	}

	private void ForceCameraPosition(CinemachineVirtualCameraBase vcam, Vector3 pos, Quaternion rot)
	{
		int? num = Extensions?.Count;
		for (int i = 0; i < num; i++)
		{
			Extensions[i].ForceCameraPosition(vcam, pos, rot);
			Extensions[i].ForceCameraPosition(pos, rot);
		}
		if (ParentCamera is CinemachineVirtualCameraBase cinemachineVirtualCameraBase)
		{
			cinemachineVirtualCameraBase.ForceCameraPosition(vcam, pos, rot);
		}
	}

	protected CameraState PullStateFromVirtualCamera(Vector3 worldUp, ref LensSettings lens)
	{
		CameraState @default = CameraState.Default;
		@default.RawPosition = TargetPositionCache.GetTargetPosition(base.transform);
		@default.RawOrientation = TargetPositionCache.GetTargetRotation(base.transform);
		@default.ReferenceUp = worldUp;
		CinemachineBrain cinemachineBrain = CinemachineCore.FindPotentialTargetBrain(this);
		if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
		{
			lens.PullInheritedPropertiesFromCamera(cinemachineBrain.OutputCamera);
		}
		@default.Lens = lens;
		return @default;
	}

	private void InvalidateCachedTargets()
	{
		m_CachedFollowTarget = null;
		m_CachedFollowTargetVcam = null;
		m_CachedFollowTargetGroup = null;
		m_CachedLookAtTarget = null;
		m_CachedLookAtTargetVcam = null;
		m_CachedLookAtTargetGroup = null;
	}

	public void UpdateTargetCache()
	{
		Transform transform = ResolveFollow(Follow);
		FollowTargetChanged = transform != m_CachedFollowTarget;
		if (FollowTargetChanged)
		{
			m_CachedFollowTarget = transform;
			m_CachedFollowTargetVcam = null;
			m_CachedFollowTargetGroup = null;
			if (m_CachedFollowTarget != null)
			{
				transform.TryGetComponent<CinemachineVirtualCameraBase>(out m_CachedFollowTargetVcam);
				transform.TryGetComponent<ICinemachineTargetGroup>(out m_CachedFollowTargetGroup);
			}
		}
		transform = ResolveLookAt(LookAt);
		LookAtTargetChanged = transform != m_CachedLookAtTarget;
		if (LookAtTargetChanged)
		{
			m_CachedLookAtTarget = transform;
			m_CachedLookAtTargetVcam = null;
			m_CachedLookAtTargetGroup = null;
			if (transform != null)
			{
				transform.TryGetComponent<CinemachineVirtualCameraBase>(out m_CachedLookAtTargetVcam);
				transform.TryGetComponent<ICinemachineTargetGroup>(out m_CachedLookAtTargetGroup);
			}
		}
	}

	public virtual CinemachineComponentBase GetCinemachineComponent(CinemachineCore.Stage stage)
	{
		return null;
	}

	public bool IsParticipatingInBlend()
	{
		if (IsLive)
		{
			CinemachineCameraManagerBase cinemachineCameraManagerBase = ParentCamera as CinemachineCameraManagerBase;
			if (cinemachineCameraManagerBase != null)
			{
				if (cinemachineCameraManagerBase.ActiveBlend == null || !cinemachineCameraManagerBase.ActiveBlend.Uses(this))
				{
					return cinemachineCameraManagerBase.IsParticipatingInBlend();
				}
				return true;
			}
			CinemachineBrain cinemachineBrain = CinemachineCore.FindPotentialTargetBrain(this);
			if (cinemachineBrain != null)
			{
				if (cinemachineBrain.ActiveBlend != null)
				{
					return cinemachineBrain.ActiveBlend.Uses(this);
				}
				return false;
			}
		}
		return false;
	}

	public void CancelDamping(bool updateNow = false)
	{
		PreviousStateIsValid = false;
		if (updateNow)
		{
			Vector3 worldUp = State.ReferenceUp;
			CinemachineBrain cinemachineBrain = CinemachineCore.FindPotentialTargetBrain(this);
			if (cinemachineBrain != null)
			{
				worldUp = cinemachineBrain.DefaultWorldUp;
			}
			InternalUpdateCameraState(worldUp, -1f);
		}
	}
}
