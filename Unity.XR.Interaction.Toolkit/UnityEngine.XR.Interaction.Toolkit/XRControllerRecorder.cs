using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Debug/XR Controller Recorder", 11)]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-30000)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRControllerRecorder.html")]
public class XRControllerRecorder : MonoBehaviour
{
	private class ButtonBypass : IXRInputButtonReader, IXRInputValueReader<float>, IXRInputValueReader
	{
		public InteractionState state { get; set; }

		public bool ReadIsPerformed()
		{
			return state.active;
		}

		public bool ReadWasPerformedThisFrame()
		{
			return state.activatedThisFrame;
		}

		public bool ReadWasCompletedThisFrame()
		{
			return state.deactivatedThisFrame;
		}

		public float ReadValue()
		{
			return state.value;
		}

		public bool TryReadValue(out float value)
		{
			value = state.value;
			return true;
		}
	}

	private class ValueBypass<TValue> : IXRInputValueReader<TValue>, IXRInputValueReader where TValue : struct
	{
		public TValue state { get; set; }

		public TValue ReadValue()
		{
			return state;
		}

		public bool TryReadValue(out TValue value)
		{
			value = state;
			return true;
		}
	}

	[Header("Input Recording/Playback")]
	[SerializeField]
	[Tooltip("Controls whether this recording will start playing when the component's Awake() method is called.")]
	private bool m_PlayOnStart;

	[SerializeField]
	[Tooltip("Controller Recording asset for recording and playback of controller events.")]
	private XRControllerRecording m_Recording;

	[SerializeField]
	[Tooltip("Interactor whose input will be recorded and played back.")]
	[RequireInterface(typeof(IXRInteractor))]
	private Object m_InteractorObject;

	[SerializeField]
	[Tooltip("If true, every frame of the recording must be visited even if a larger time period has passed.")]
	private bool m_VisitEachFrame;

	private double m_CurrentTime;

	private readonly UnityObjectReferenceCache<IXRInteractor, Object> m_Interactor = new UnityObjectReferenceCache<IXRInteractor, Object>();

	private bool m_IsRecording;

	private bool m_IsPlaying;

	private double m_LastPlaybackTime;

	private int m_LastFrameIdx;

	private bool m_PrevEnableInputActions;

	private bool m_PrevEnableInputTracking;

	private IXRInputButtonReader m_PrevSelectBypass;

	private IXRInputButtonReader m_PrevActivateBypass;

	private IXRInputButtonReader m_PrevUIPressBypass;

	private IXRInputValueReader<Vector2> m_PrevUIScrollBypass;

	private readonly ButtonBypass m_SelectBypass = new ButtonBypass();

	private readonly ButtonBypass m_ActivateBypass = new ButtonBypass();

	private readonly ButtonBypass m_UIPressBypass = new ButtonBypass();

	private readonly ValueBypass<Vector2> m_UIScrollBypass = new ValueBypass<Vector2>();

	[SerializeField]
	[Tooltip("(Deprecated) XR Controller whose output will be recorded and played back.")]
	[Obsolete("m_XRController has been deprecated in version 3.0.0.")]
	private XRBaseController m_XRController;

	public bool playOnStart
	{
		get
		{
			return m_PlayOnStart;
		}
		set
		{
			m_PlayOnStart = value;
		}
	}

	public XRControllerRecording recording
	{
		get
		{
			return m_Recording;
		}
		set
		{
			m_Recording = value;
		}
	}

	public bool visitEachFrame
	{
		get
		{
			return m_VisitEachFrame;
		}
		set
		{
			m_VisitEachFrame = value;
		}
	}

	public bool isRecording
	{
		get
		{
			return m_IsRecording;
		}
		set
		{
			if (m_IsRecording == value)
			{
				return;
			}
			recordingStartTime = Time.time;
			isPlaying = false;
			m_CurrentTime = 0.0;
			if ((bool)m_Recording)
			{
				if (value)
				{
					m_Recording.InitRecording();
				}
				else
				{
					m_Recording.SaveRecording();
				}
			}
			m_IsRecording = value;
		}
	}

	public bool isPlaying
	{
		get
		{
			return m_IsPlaying;
		}
		set
		{
			if (m_IsPlaying != value)
			{
				isRecording = false;
				if ((bool)m_Recording)
				{
					ResetPlayback();
				}
				m_CurrentTime = 0.0;
				m_IsPlaying = value;
				if (value)
				{
					StartPlaying();
				}
				else
				{
					StopPlaying();
				}
			}
		}
	}

	public double currentTime => m_CurrentTime;

	public double duration
	{
		get
		{
			if (!(m_Recording != null))
			{
				return 0.0;
			}
			return m_Recording.duration;
		}
	}

	protected float recordingStartTime { get; set; }

	[Obsolete("xrController has been deprecated in version 3.0.0. Use interactor to allow the recorder to read and playback button input instead.")]
	public XRBaseController xrController
	{
		get
		{
			return m_XRController;
		}
		set
		{
			m_XRController = value;
		}
	}

	protected void Awake()
	{
		if (m_XRController == null)
		{
			m_XRController = GetComponentInParent<XRBaseController>(includeInactive: true);
		}
		if (m_InteractorObject == null)
		{
			m_InteractorObject = GetComponentInParent<IXRInteractor>(includeInactive: true) as Object;
		}
		m_CurrentTime = 0.0;
		if (m_PlayOnStart)
		{
			isPlaying = true;
		}
	}

	protected virtual void Update()
	{
		if (isRecording)
		{
			IXRInteractor interactor = GetInteractor();
			float num = Time.time - recordingStartTime;
			XRControllerState xRControllerState;
			if (m_XRController != null)
			{
				xRControllerState = new XRControllerState(m_XRController.currentControllerState);
			}
			else if (interactor != null)
			{
				Pose localPose = interactor.transform.GetLocalPose();
				xRControllerState = new XRControllerState
				{
					inputTrackingState = InputTrackingState.All,
					isTracked = true,
					position = localPose.position,
					rotation = localPose.rotation
				};
			}
			else
			{
				xRControllerState = new XRControllerState();
			}
			xRControllerState.time = num;
			if (interactor != null)
			{
				if (interactor is XRBaseInputInteractor { selectInput: var selectInput } xRBaseInputInteractor)
				{
					xRControllerState.selectInteractionState = new InteractionState
					{
						value = selectInput.ReadValue(),
						active = selectInput.ReadIsPerformed(),
						activatedThisFrame = selectInput.ReadWasPerformedThisFrame(),
						deactivatedThisFrame = selectInput.ReadWasCompletedThisFrame()
					};
					XRInputButtonReader activateInput = xRBaseInputInteractor.activateInput;
					xRControllerState.activateInteractionState = new InteractionState
					{
						value = activateInput.ReadValue(),
						active = activateInput.ReadIsPerformed(),
						activatedThisFrame = activateInput.ReadWasPerformedThisFrame(),
						deactivatedThisFrame = activateInput.ReadWasCompletedThisFrame()
					};
				}
				else
				{
					xRControllerState.selectInteractionState = default(InteractionState);
					xRControllerState.activateInteractionState = default(InteractionState);
				}
				if (interactor is XRRayInteractor { uiPressInput: var uiPressInput } xRRayInteractor)
				{
					xRControllerState.uiPressInteractionState = new InteractionState
					{
						value = uiPressInput.ReadValue(),
						active = uiPressInput.ReadIsPerformed(),
						activatedThisFrame = uiPressInput.ReadWasPerformedThisFrame(),
						deactivatedThisFrame = uiPressInput.ReadWasCompletedThisFrame()
					};
					xRControllerState.uiScrollValue = xRRayInteractor.uiScrollInput.ReadValue();
				}
				else
				{
					xRControllerState.uiPressInteractionState = default(InteractionState);
					xRControllerState.uiScrollValue = Vector2.zero;
				}
			}
			m_Recording.AddRecordingFrameNonAlloc(xRControllerState);
		}
		else if (isPlaying)
		{
			UpdatePlaybackTime(m_CurrentTime);
		}
		if (isRecording || isPlaying)
		{
			m_CurrentTime += Time.deltaTime;
		}
		if (isPlaying && m_CurrentTime > m_Recording.duration && (!m_VisitEachFrame || m_LastFrameIdx >= m_Recording.frames.Count - 1))
		{
			isPlaying = false;
		}
	}

	protected void OnDestroy()
	{
		isRecording = false;
		isPlaying = false;
	}

	public IXRInteractor GetInteractor()
	{
		return m_Interactor.Get(m_InteractorObject);
	}

	public void SetInteractor(IXRInteractor interactor)
	{
		m_Interactor.Set(ref m_InteractorObject, interactor);
	}

	public void ResetPlayback()
	{
		m_LastPlaybackTime = 0.0;
		m_LastFrameIdx = 0;
	}

	private void StartPlaying()
	{
		if (m_XRController != null)
		{
			m_PrevEnableInputActions = m_XRController.enableInputActions;
			m_PrevEnableInputTracking = m_XRController.enableInputTracking;
			m_XRController.enableInputActions = false;
			m_XRController.enableInputTracking = false;
		}
		IXRInteractor interactor = GetInteractor();
		if (interactor != null)
		{
			if (interactor is XRBaseInputInteractor xRBaseInputInteractor)
			{
				m_PrevSelectBypass = xRBaseInputInteractor.selectInput.bypass;
				m_PrevActivateBypass = xRBaseInputInteractor.activateInput.bypass;
				xRBaseInputInteractor.selectInput.bypass = m_SelectBypass;
				xRBaseInputInteractor.activateInput.bypass = m_ActivateBypass;
			}
			else
			{
				m_PrevSelectBypass = null;
				m_PrevActivateBypass = null;
			}
			if (interactor is XRRayInteractor xRRayInteractor)
			{
				m_PrevUIPressBypass = ((xRRayInteractor != null) ? xRRayInteractor.uiPressInput.bypass : null);
				m_PrevUIScrollBypass = ((xRRayInteractor != null) ? xRRayInteractor.uiScrollInput.bypass : null);
				xRRayInteractor.uiPressInput.bypass = m_UIPressBypass;
				xRRayInteractor.uiScrollInput.bypass = m_UIScrollBypass;
			}
			else
			{
				m_PrevUIPressBypass = null;
				m_PrevUIScrollBypass = null;
			}
		}
	}

	private void StopPlaying()
	{
		if (m_XRController != null)
		{
			m_XRController.enableInputActions = m_PrevEnableInputActions;
			m_XRController.enableInputTracking = m_PrevEnableInputTracking;
		}
		IXRInteractor interactor = GetInteractor();
		if (m_Interactor != null)
		{
			if (interactor is XRBaseInputInteractor xRBaseInputInteractor)
			{
				xRBaseInputInteractor.selectInput.bypass = m_PrevSelectBypass;
				xRBaseInputInteractor.activateInput.bypass = m_PrevActivateBypass;
			}
			if (interactor is XRRayInteractor xRRayInteractor)
			{
				xRRayInteractor.uiPressInput.bypass = m_PrevUIPressBypass;
				xRRayInteractor.uiScrollInput.bypass = m_PrevUIScrollBypass;
			}
		}
	}

	private void UpdatePlaybackTime(double playbackTime)
	{
		if (!m_Recording || m_Recording == null || m_Recording.frames.Count == 0 || m_LastFrameIdx >= m_Recording.frames.Count)
		{
			return;
		}
		XRControllerState xRControllerState = m_Recording.frames[m_LastFrameIdx];
		int num = m_LastFrameIdx;
		if (xRControllerState.time < playbackTime)
		{
			while (num < m_Recording.frames.Count && m_Recording.frames[num].time >= m_LastPlaybackTime && m_Recording.frames[num].time <= playbackTime)
			{
				num++;
				if (m_VisitEachFrame)
				{
					if (num < m_Recording.frames.Count)
					{
						playbackTime = m_Recording.frames[num].time;
					}
					break;
				}
			}
		}
		if (num >= m_Recording.frames.Count)
		{
			return;
		}
		XRControllerState xRControllerState2 = m_Recording.frames[num];
		if (m_XRController != null)
		{
			m_XRController.currentControllerState = xRControllerState2;
		}
		IXRInteractor interactor = GetInteractor();
		if (interactor != null)
		{
			m_SelectBypass.state = xRControllerState2.selectInteractionState;
			m_ActivateBypass.state = xRControllerState2.activateInteractionState;
			m_UIPressBypass.state = xRControllerState2.uiPressInteractionState;
			m_UIScrollBypass.state = xRControllerState2.uiScrollValue;
			if (m_XRController == null)
			{
				Transform transform = interactor.transform;
				bool flag = (xRControllerState2.inputTrackingState & InputTrackingState.Position) != 0;
				bool flag2 = (xRControllerState2.inputTrackingState & InputTrackingState.Rotation) != 0;
				if (flag && flag2)
				{
					transform.SetLocalPose(new Pose(xRControllerState2.position, xRControllerState2.rotation));
				}
				else if (flag)
				{
					transform.localPosition = xRControllerState2.position;
				}
				else if (flag2)
				{
					transform.localRotation = xRControllerState2.rotation;
				}
			}
		}
		m_LastFrameIdx = num;
		m_LastPlaybackTime = playbackTime;
	}

	public virtual bool GetControllerState(out XRControllerState controllerState)
	{
		if (isPlaying)
		{
			if (m_Recording.frames.Count > m_LastFrameIdx)
			{
				controllerState = m_Recording.frames[m_LastFrameIdx];
				return true;
			}
		}
		else if (isRecording && m_Recording.frames.Count > 0)
		{
			controllerState = m_Recording.frames[m_Recording.frames.Count - 1];
			return true;
		}
		controllerState = null;
		return false;
	}
}
