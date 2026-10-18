using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Feedback;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public abstract class XRBaseInputInteractor : XRBaseInteractor, IXRActivateInteractor, IXRInteractor
{
	public enum InputTriggerType
	{
		State,
		StateChange,
		Toggle,
		Sticky
	}

	public class LogicalInputState
	{
		private InputTriggerType m_Mode;

		private bool m_HasSelection;

		private float m_TimeAtPerformed;

		private float m_TimeAtCompleted;

		private bool m_ToggleActive;

		private bool m_ToggleDeactivatedThisFrame;

		private bool m_WaitingForDeactivate;

		public bool active { get; private set; }

		public InputTriggerType mode
		{
			get
			{
				return m_Mode;
			}
			set
			{
				if (m_Mode != value)
				{
					m_Mode = value;
					Refresh();
				}
			}
		}

		public bool isPerformed { get; private set; }

		public bool wasPerformedThisFrame { get; private set; }

		public bool wasCompletedThisFrame { get; private set; }

		[Obsolete("wasUnperformedThisFrame has been deprecated in version 3.0.0-pre.2. It has been renamed to wasCompletedThisFrame. (UnityUpgradable) -> wasCompletedThisFrame")]
		public bool wasUnperformedThisFrame => wasCompletedThisFrame;

		internal void UpdateInput(bool performed, bool performedThisFrame, bool completedThisFrame, bool hasSelection)
		{
			UpdateInput(performed, performedThisFrame, completedThisFrame, hasSelection, Time.realtimeSinceStartup);
		}

		private void UpdateInput(bool performed, bool performedThisFrame, bool completedThisFrame, bool hasSelection, float realtime)
		{
			isPerformed = performed;
			wasPerformedThisFrame = performedThisFrame;
			wasCompletedThisFrame = completedThisFrame;
			m_HasSelection = hasSelection;
			if (wasPerformedThisFrame)
			{
				m_TimeAtPerformed = realtime;
			}
			if (wasCompletedThisFrame)
			{
				m_TimeAtCompleted = realtime;
			}
			m_ToggleDeactivatedThisFrame = false;
			if (mode == InputTriggerType.Toggle || mode == InputTriggerType.Sticky)
			{
				if (m_ToggleActive && performedThisFrame)
				{
					m_ToggleActive = false;
					m_ToggleDeactivatedThisFrame = true;
					m_WaitingForDeactivate = true;
				}
				if (wasCompletedThisFrame)
				{
					m_WaitingForDeactivate = false;
				}
			}
			Refresh();
		}

		internal void UpdateHasSelection(bool hasSelection)
		{
			if (m_HasSelection != hasSelection)
			{
				m_HasSelection = hasSelection;
				m_ToggleActive = hasSelection;
				m_WaitingForDeactivate = false;
				Refresh();
			}
		}

		private void Refresh()
		{
			switch (mode)
			{
			case InputTriggerType.State:
				active = isPerformed;
				break;
			case InputTriggerType.StateChange:
				active = wasPerformedThisFrame || (m_HasSelection && !wasCompletedThisFrame);
				break;
			case InputTriggerType.Toggle:
				active = m_ToggleActive || (wasPerformedThisFrame && !m_ToggleDeactivatedThisFrame);
				break;
			case InputTriggerType.Sticky:
				active = m_ToggleActive || m_WaitingForDeactivate || wasPerformedThisFrame;
				break;
			}
		}
	}

	[Obsolete("InputCompatibilityMode introduced in version 3.0.0 is marked for removal. This is only used for backwards compatibility and will be eventually removed in a future version.")]
	public enum InputCompatibilityMode
	{
		Automatic,
		ForceDeprecatedInput,
		ForceInputReaders
	}

	[SerializeField]
	private XRInputButtonReader m_SelectInput = new XRInputButtonReader("Select");

	[SerializeField]
	private XRInputButtonReader m_ActivateInput = new XRInputButtonReader("Activate");

	[SerializeField]
	private InputTriggerType m_SelectActionTrigger = InputTriggerType.StateChange;

	[SerializeField]
	private bool m_AllowHoveredActivate;

	[SerializeField]
	private TargetPriorityMode m_TargetPriorityMode;

	private bool m_AllowActivate = true;

	private readonly LinkedPool<ActivateEventArgs> m_ActivateEventArgs = new LinkedPool<ActivateEventArgs>(() => new ActivateEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<DeactivateEventArgs> m_DeactivateEventArgs = new LinkedPool<DeactivateEventArgs>(() => new DeactivateEventArgs(), null, null, null, collectionCheck: false);

	private static readonly List<IXRActivateInteractable> s_ActivateTargets = new List<IXRActivateInteractable>();

	private readonly LogicalInputState m_LogicalSelectState = new LogicalInputState();

	private readonly LogicalInputState m_LogicalActivateState = new LogicalInputState();

	private SimpleAudioFeedback m_AudioFeedback;

	private SimpleHapticFeedback m_HapticFeedback;

	private AudioSource m_AudioSource;

	private HapticImpulsePlayer m_HapticImpulsePlayer;

	[SerializeField]
	private bool m_HideControllerOnSelect;

	[SerializeField]
	[Obsolete("m_InputCompatibilityMode introduced in version 3.0.0 is marked for removal. This is only used for backwards compatibility and will be eventually removed in a future version.")]
	private InputCompatibilityMode m_InputCompatibilityMode;

	[Obsolete("m_Controller has been deprecated in version 3.0.0.")]
	private XRBaseController m_Controller;

	private bool m_HasXRController;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnSelectEnter")]
	private bool m_PlayAudioClipOnSelectEntered;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnSelectEnter")]
	private AudioClip m_AudioClipForOnSelectEntered;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnSelectExit")]
	private bool m_PlayAudioClipOnSelectExited;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnSelectExit")]
	private AudioClip m_AudioClipForOnSelectExited;

	[SerializeField]
	private bool m_PlayAudioClipOnSelectCanceled;

	[SerializeField]
	private AudioClip m_AudioClipForOnSelectCanceled;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnHoverEnter")]
	private bool m_PlayAudioClipOnHoverEntered;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnHoverEnter")]
	private AudioClip m_AudioClipForOnHoverEntered;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnHoverExit")]
	private bool m_PlayAudioClipOnHoverExited;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnHoverExit")]
	private AudioClip m_AudioClipForOnHoverExited;

	[SerializeField]
	private bool m_PlayAudioClipOnHoverCanceled;

	[SerializeField]
	private AudioClip m_AudioClipForOnHoverCanceled;

	[SerializeField]
	private bool m_AllowHoverAudioWhileSelecting = true;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnSelectEnter")]
	private bool m_PlayHapticsOnSelectEntered;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticSelectEnterIntensity;

	[SerializeField]
	private float m_HapticSelectEnterDuration;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnSelectExit")]
	private bool m_PlayHapticsOnSelectExited;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticSelectExitIntensity;

	[SerializeField]
	private float m_HapticSelectExitDuration;

	[SerializeField]
	private bool m_PlayHapticsOnSelectCanceled;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticSelectCancelIntensity;

	[SerializeField]
	private float m_HapticSelectCancelDuration;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnHoverEnter")]
	private bool m_PlayHapticsOnHoverEntered;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticHoverEnterIntensity;

	[SerializeField]
	private float m_HapticHoverEnterDuration;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnHoverExit")]
	private bool m_PlayHapticsOnHoverExited;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticHoverExitIntensity;

	[SerializeField]
	private float m_HapticHoverExitDuration;

	[SerializeField]
	private bool m_PlayHapticsOnHoverCanceled;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticHoverCancelIntensity;

	[SerializeField]
	private float m_HapticHoverCancelDuration;

	[SerializeField]
	private bool m_AllowHoverHapticsWhileSelecting = true;

	public XRInputButtonReader selectInput
	{
		get
		{
			return m_SelectInput;
		}
		set
		{
			SetInputProperty(ref m_SelectInput, value);
		}
	}

	public XRInputButtonReader activateInput
	{
		get
		{
			return m_ActivateInput;
		}
		set
		{
			SetInputProperty(ref m_ActivateInput, value);
		}
	}

	public InputTriggerType selectActionTrigger
	{
		get
		{
			return m_SelectActionTrigger;
		}
		set
		{
			m_SelectActionTrigger = value;
		}
	}

	public bool allowHoveredActivate
	{
		get
		{
			return m_AllowHoveredActivate;
		}
		set
		{
			m_AllowHoveredActivate = value;
		}
	}

	public override TargetPriorityMode targetPriorityMode
	{
		get
		{
			return m_TargetPriorityMode;
		}
		set
		{
			m_TargetPriorityMode = value;
		}
	}

	public bool allowActivate
	{
		get
		{
			return m_AllowActivate;
		}
		set
		{
			m_AllowActivate = value;
		}
	}

	public override bool isSelectActive
	{
		get
		{
			if (!base.isSelectActive)
			{
				return false;
			}
			if (base.isPerformingManualInteraction)
			{
				return true;
			}
			m_LogicalSelectState.mode = m_SelectActionTrigger;
			return m_LogicalSelectState.active;
		}
	}

	public virtual bool shouldActivate
	{
		get
		{
			if (m_AllowActivate && (base.hasSelection || (m_AllowHoveredActivate && base.hasHover)))
			{
				return m_LogicalActivateState.wasPerformedThisFrame;
			}
			return false;
		}
	}

	public virtual bool shouldDeactivate
	{
		get
		{
			if (m_AllowActivate && (base.hasSelection || (m_AllowHoveredActivate && base.hasHover)))
			{
				return m_LogicalActivateState.wasCompletedThisFrame;
			}
			return false;
		}
	}

	public LogicalInputState logicalSelectState => m_LogicalSelectState;

	public LogicalInputState logicalActivateState => m_LogicalActivateState;

	protected List<XRInputButtonReader> buttonReaders { get; } = new List<XRInputButtonReader>();


	protected List<XRInputValueReader> valueReaders { get; } = new List<XRInputValueReader>();


	[Obsolete("hideControllerOnSelect has been deprecated in version 3.0.0.")]
	public bool hideControllerOnSelect
	{
		get
		{
			return m_HideControllerOnSelect;
		}
		set
		{
			m_HideControllerOnSelect = value;
			if (!m_HideControllerOnSelect && m_Controller != null)
			{
				m_Controller.hideControllerModel = false;
			}
		}
	}

	[Obsolete("inputCompatibilityMode introduced in version 3.0.0 is marked for removal. This is only used for backwards compatibility and will be eventually removed in a future version.")]
	public InputCompatibilityMode inputCompatibilityMode
	{
		get
		{
			return m_InputCompatibilityMode;
		}
		set
		{
			m_InputCompatibilityMode = value;
		}
	}

	[Obsolete("forceDeprecatedInput introduced in version 3.0.0 is marked for removal. This is only used for backwards compatibility and will be eventually removed in a future version.")]
	public bool forceDeprecatedInput
	{
		get
		{
			if (!m_HasXRController || m_InputCompatibilityMode != 0)
			{
				return m_InputCompatibilityMode == InputCompatibilityMode.ForceDeprecatedInput;
			}
			return true;
		}
		set
		{
			m_InputCompatibilityMode = (value ? InputCompatibilityMode.ForceDeprecatedInput : InputCompatibilityMode.ForceInputReaders);
		}
	}

	[Obsolete("xrController has been deprecated in version 3.0.0.")]
	public XRBaseController xrController
	{
		get
		{
			return m_Controller;
		}
		set
		{
			if (m_Controller != value)
			{
				m_Controller = value;
				OnXRControllerChanged();
			}
		}
	}

	[Obsolete("isUISelectActive has been deprecated in version 3.0.0. Use a serialized XRInputButtonReader to read button input instead. Some derived interactors have a uiPressInput property that can be used instead.")]
	protected virtual bool isUISelectActive
	{
		get
		{
			if (m_Controller != null)
			{
				return m_Controller.uiPressInteractionState.active;
			}
			return false;
		}
	}

	[Obsolete("uiScrollValue has been deprecated in version 3.0.0. Use a serialized XRInputValueReader<Vector2> to read scroll input instead. Some derived interactors have a uiScrollInput property that can be used instead.")]
	protected Vector2 uiScrollValue
	{
		get
		{
			if (!(m_Controller != null))
			{
				return Vector2.zero;
			}
			return m_Controller.uiScrollValue;
		}
	}

	[Obsolete("playAudioClipOnSelectEntered has been deprecated in version 3.0.0. Use SimpleAudioFeedback.playSelectEntered instead.")]
	public bool playAudioClipOnSelectEntered
	{
		get
		{
			return m_PlayAudioClipOnSelectEntered;
		}
		set
		{
			m_PlayAudioClipOnSelectEntered = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.playSelectEntered = value;
			}
		}
	}

	[Obsolete("audioClipForOnSelectEntered has been deprecated in version 3.0.0. Use SimpleAudioFeedback.selectEnteredClip instead.")]
	public AudioClip audioClipForOnSelectEntered
	{
		get
		{
			return m_AudioClipForOnSelectEntered;
		}
		set
		{
			m_AudioClipForOnSelectEntered = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.selectEnteredClip = value;
			}
		}
	}

	[Obsolete("playAudioClipOnSelectExited has been deprecated in version 3.0.0. Use SimpleAudioFeedback.playSelectExited instead.")]
	public bool playAudioClipOnSelectExited
	{
		get
		{
			return m_PlayAudioClipOnSelectExited;
		}
		set
		{
			m_PlayAudioClipOnSelectExited = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.playSelectExited = value;
			}
		}
	}

	[Obsolete("audioClipForOnSelectExited has been deprecated in version 3.0.0. Use SimpleAudioFeedback.selectExitedClip instead.")]
	public AudioClip audioClipForOnSelectExited
	{
		get
		{
			return m_AudioClipForOnSelectExited;
		}
		set
		{
			m_AudioClipForOnSelectExited = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.selectExitedClip = value;
			}
		}
	}

	[Obsolete("playAudioClipOnSelectCanceled has been deprecated in version 3.0.0. Use SimpleAudioFeedback.playSelectCanceled instead.")]
	public bool playAudioClipOnSelectCanceled
	{
		get
		{
			return m_PlayAudioClipOnSelectCanceled;
		}
		set
		{
			m_PlayAudioClipOnSelectCanceled = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.playSelectCanceled = value;
			}
		}
	}

	[Obsolete("audioClipForOnSelectCanceled has been deprecated in version 3.0.0. Use SimpleAudioFeedback.selectCanceledClip instead.")]
	public AudioClip audioClipForOnSelectCanceled
	{
		get
		{
			return m_AudioClipForOnSelectCanceled;
		}
		set
		{
			m_AudioClipForOnSelectCanceled = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.selectCanceledClip = value;
			}
		}
	}

	[Obsolete("playAudioClipOnHoverEntered has been deprecated in version 3.0.0. Use SimpleAudioFeedback.playHoverEntered instead.")]
	public bool playAudioClipOnHoverEntered
	{
		get
		{
			return m_PlayAudioClipOnHoverEntered;
		}
		set
		{
			m_PlayAudioClipOnHoverEntered = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.playHoverEntered = value;
			}
		}
	}

	[Obsolete("audioClipForOnHoverEntered has been deprecated in version 3.0.0. Use SimpleAudioFeedback.hoverEnteredClip instead.")]
	public AudioClip audioClipForOnHoverEntered
	{
		get
		{
			return m_AudioClipForOnHoverEntered;
		}
		set
		{
			m_AudioClipForOnHoverEntered = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.hoverEnteredClip = value;
			}
		}
	}

	[Obsolete("playAudioClipOnHoverExited has been deprecated in version 3.0.0. Use SimpleAudioFeedback.playHoverExited instead.")]
	public bool playAudioClipOnHoverExited
	{
		get
		{
			return m_PlayAudioClipOnHoverExited;
		}
		set
		{
			m_PlayAudioClipOnHoverExited = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.playHoverExited = value;
			}
		}
	}

	[Obsolete("audioClipForOnHoverExited has been deprecated in version 3.0.0. Use SimpleAudioFeedback.hoverExitedClip instead.")]
	public AudioClip audioClipForOnHoverExited
	{
		get
		{
			return m_AudioClipForOnHoverExited;
		}
		set
		{
			m_AudioClipForOnHoverExited = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.hoverExitedClip = value;
			}
		}
	}

	[Obsolete("playAudioClipOnHoverCanceled has been deprecated in version 3.0.0. Use SimpleAudioFeedback.playHoverCanceled instead.")]
	public bool playAudioClipOnHoverCanceled
	{
		get
		{
			return m_PlayAudioClipOnHoverCanceled;
		}
		set
		{
			m_PlayAudioClipOnHoverCanceled = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.playHoverCanceled = value;
			}
		}
	}

	[Obsolete("audioClipForOnHoverCanceled has been deprecated in version 3.0.0. Use SimpleAudioFeedback.hoverCanceledClip instead.")]
	public AudioClip audioClipForOnHoverCanceled
	{
		get
		{
			return m_AudioClipForOnHoverCanceled;
		}
		set
		{
			m_AudioClipForOnHoverCanceled = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.hoverCanceledClip = value;
			}
		}
	}

	[Obsolete("allowHoverAudioWhileSelecting has been deprecated in version 3.0.0. Use SimpleAudioFeedback.allowHoverAudioWhileSelecting instead.")]
	public bool allowHoverAudioWhileSelecting
	{
		get
		{
			return m_AllowHoverAudioWhileSelecting;
		}
		set
		{
			m_AllowHoverAudioWhileSelecting = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateAudioFeedback();
				m_AudioFeedback.allowHoverAudioWhileSelecting = value;
			}
		}
	}

	[Obsolete("playHapticsOnSelectEntered has been deprecated in version 3.0.0. Use SimpleHapticFeedback.playSelectEntered instead.")]
	public bool playHapticsOnSelectEntered
	{
		get
		{
			return m_PlayHapticsOnSelectEntered;
		}
		set
		{
			m_PlayHapticsOnSelectEntered = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.playSelectEntered = value;
			}
		}
	}

	[Obsolete("hapticSelectEnterIntensity has been deprecated in version 3.0.0. Use SimpleHapticFeedback.selectEnteredData.amplitude instead.")]
	public float hapticSelectEnterIntensity
	{
		get
		{
			return m_HapticSelectEnterIntensity;
		}
		set
		{
			m_HapticSelectEnterIntensity = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.selectEnteredData != null)
				{
					m_HapticFeedback.selectEnteredData.amplitude = value;
					return;
				}
				m_HapticFeedback.selectEnteredData = new HapticImpulseData
				{
					amplitude = value,
					duration = m_HapticSelectEnterDuration
				};
			}
		}
	}

	[Obsolete("hapticSelectEnterDuration has been deprecated in version 3.0.0. Use SimpleHapticFeedback.selectEnteredData.duration instead.")]
	public float hapticSelectEnterDuration
	{
		get
		{
			return m_HapticSelectEnterDuration;
		}
		set
		{
			m_HapticSelectEnterDuration = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.selectEnteredData != null)
				{
					m_HapticFeedback.selectEnteredData.duration = value;
					return;
				}
				m_HapticFeedback.selectEnteredData = new HapticImpulseData
				{
					amplitude = m_HapticSelectEnterIntensity,
					duration = value
				};
			}
		}
	}

	[Obsolete("playHapticsOnSelectExited has been deprecated in version 3.0.0. Use SimpleHapticFeedback.playSelectExited instead.")]
	public bool playHapticsOnSelectExited
	{
		get
		{
			return m_PlayHapticsOnSelectExited;
		}
		set
		{
			m_PlayHapticsOnSelectExited = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.playSelectExited = value;
			}
		}
	}

	[Obsolete("hapticSelectExitIntensity has been deprecated in version 3.0.0. Use SimpleHapticFeedback.selectExitedData.amplitude instead.")]
	public float hapticSelectExitIntensity
	{
		get
		{
			return m_HapticSelectExitIntensity;
		}
		set
		{
			m_HapticSelectExitIntensity = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.selectExitedData != null)
				{
					m_HapticFeedback.selectExitedData.amplitude = value;
					return;
				}
				m_HapticFeedback.selectExitedData = new HapticImpulseData
				{
					amplitude = value,
					duration = m_HapticSelectExitDuration
				};
			}
		}
	}

	[Obsolete("hapticSelectExitDuration has been deprecated in version 3.0.0. Use SimpleHapticFeedback.selectExitedData.duration instead.")]
	public float hapticSelectExitDuration
	{
		get
		{
			return m_HapticSelectExitDuration;
		}
		set
		{
			m_HapticSelectExitDuration = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.selectExitedData != null)
				{
					m_HapticFeedback.selectExitedData.duration = value;
					return;
				}
				m_HapticFeedback.selectExitedData = new HapticImpulseData
				{
					amplitude = m_HapticSelectExitIntensity,
					duration = value
				};
			}
		}
	}

	[Obsolete("playHapticsOnSelectCanceled has been deprecated in version 3.0.0. Use SimpleHapticFeedback.playSelectCanceled instead.")]
	public bool playHapticsOnSelectCanceled
	{
		get
		{
			return m_PlayHapticsOnSelectCanceled;
		}
		set
		{
			m_PlayHapticsOnSelectCanceled = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.playSelectCanceled = value;
			}
		}
	}

	[Obsolete("hapticSelectCancelIntensity has been deprecated in version 3.0.0. Use SimpleHapticFeedback.selectCanceledData.amplitude instead.")]
	public float hapticSelectCancelIntensity
	{
		get
		{
			return m_HapticSelectCancelIntensity;
		}
		set
		{
			m_HapticSelectCancelIntensity = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.selectCanceledData != null)
				{
					m_HapticFeedback.selectCanceledData.amplitude = value;
					return;
				}
				m_HapticFeedback.selectCanceledData = new HapticImpulseData
				{
					amplitude = value,
					duration = m_HapticSelectCancelDuration
				};
			}
		}
	}

	[Obsolete("hapticSelectCancelDuration has been deprecated in version 3.0.0. Use SimpleHapticFeedback.selectCanceledData.duration instead.")]
	public float hapticSelectCancelDuration
	{
		get
		{
			return m_HapticSelectCancelDuration;
		}
		set
		{
			m_HapticSelectCancelDuration = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.selectCanceledData != null)
				{
					m_HapticFeedback.selectCanceledData.duration = value;
					return;
				}
				m_HapticFeedback.selectCanceledData = new HapticImpulseData
				{
					amplitude = m_HapticSelectCancelIntensity,
					duration = value
				};
			}
		}
	}

	[Obsolete("playHapticsOnHoverEntered has been deprecated in version 3.0.0. Use SimpleHapticFeedback.playHoverEntered instead.")]
	public bool playHapticsOnHoverEntered
	{
		get
		{
			return m_PlayHapticsOnHoverEntered;
		}
		set
		{
			m_PlayHapticsOnHoverEntered = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.playHoverEntered = value;
			}
		}
	}

	[Obsolete("hapticHoverEnterIntensity has been deprecated in version 3.0.0. Use SimpleHapticFeedback.hoverEnteredData.amplitude instead.")]
	public float hapticHoverEnterIntensity
	{
		get
		{
			return m_HapticHoverEnterIntensity;
		}
		set
		{
			m_HapticHoverEnterIntensity = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.hoverEnteredData != null)
				{
					m_HapticFeedback.hoverEnteredData.amplitude = value;
					return;
				}
				m_HapticFeedback.hoverEnteredData = new HapticImpulseData
				{
					amplitude = value,
					duration = m_HapticHoverEnterDuration
				};
			}
		}
	}

	[Obsolete("hapticHoverEnterDuration has been deprecated in version 3.0.0. Use SimpleHapticFeedback.hoverEnteredData.duration instead.")]
	public float hapticHoverEnterDuration
	{
		get
		{
			return m_HapticHoverEnterDuration;
		}
		set
		{
			m_HapticHoverEnterDuration = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.hoverEnteredData != null)
				{
					m_HapticFeedback.hoverEnteredData.duration = value;
					return;
				}
				m_HapticFeedback.hoverEnteredData = new HapticImpulseData
				{
					amplitude = m_HapticHoverEnterIntensity,
					duration = value
				};
			}
		}
	}

	[Obsolete("playHapticsOnHoverExited has been deprecated in version 3.0.0. Use SimpleHapticFeedback.playHoverExited instead.")]
	public bool playHapticsOnHoverExited
	{
		get
		{
			return m_PlayHapticsOnHoverExited;
		}
		set
		{
			m_PlayHapticsOnHoverExited = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.playHoverExited = value;
			}
		}
	}

	[Obsolete("hapticHoverExitIntensity has been deprecated in version 3.0.0. Use SimpleHapticFeedback.hoverExitedData.amplitude instead.")]
	public float hapticHoverExitIntensity
	{
		get
		{
			return m_HapticHoverExitIntensity;
		}
		set
		{
			m_HapticHoverExitIntensity = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.hoverExitedData != null)
				{
					m_HapticFeedback.hoverExitedData.amplitude = value;
					return;
				}
				m_HapticFeedback.hoverExitedData = new HapticImpulseData
				{
					amplitude = value,
					duration = m_HapticHoverExitDuration
				};
			}
		}
	}

	[Obsolete("hapticHoverExitDuration has been deprecated in version 3.0.0. Use SimpleHapticFeedback.hoverExitedData.duration instead.")]
	public float hapticHoverExitDuration
	{
		get
		{
			return m_HapticHoverExitDuration;
		}
		set
		{
			m_HapticHoverExitDuration = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.hoverExitedData != null)
				{
					m_HapticFeedback.hoverExitedData.duration = value;
					return;
				}
				m_HapticFeedback.hoverExitedData = new HapticImpulseData
				{
					amplitude = m_HapticHoverExitIntensity,
					duration = value
				};
			}
		}
	}

	[Obsolete("playHapticsOnHoverCanceled has been deprecated in version 3.0.0. Use SimpleHapticFeedback.playHoverCanceled instead.")]
	public bool playHapticsOnHoverCanceled
	{
		get
		{
			return m_PlayHapticsOnHoverCanceled;
		}
		set
		{
			m_PlayHapticsOnHoverCanceled = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.playHoverCanceled = value;
			}
		}
	}

	[Obsolete("hapticHoverCancelIntensity has been deprecated in version 3.0.0. Use SimpleHapticFeedback.hoverCanceledData.amplitude instead.")]
	public float hapticHoverCancelIntensity
	{
		get
		{
			return m_HapticHoverCancelIntensity;
		}
		set
		{
			m_HapticHoverCancelIntensity = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.hoverCanceledData != null)
				{
					m_HapticFeedback.hoverCanceledData.amplitude = value;
					return;
				}
				m_HapticFeedback.hoverCanceledData = new HapticImpulseData
				{
					amplitude = value,
					duration = m_HapticHoverCancelDuration
				};
			}
		}
	}

	[Obsolete("hapticHoverCancelDuration has been deprecated in version 3.0.0. Use SimpleHapticFeedback.hoverCanceledData.duration instead.")]
	public float hapticHoverCancelDuration
	{
		get
		{
			return m_HapticHoverCancelDuration;
		}
		set
		{
			m_HapticHoverCancelDuration = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				if (m_HapticFeedback.hoverCanceledData != null)
				{
					m_HapticFeedback.hoverCanceledData.duration = value;
					return;
				}
				m_HapticFeedback.hoverCanceledData = new HapticImpulseData
				{
					amplitude = m_HapticHoverCancelIntensity,
					duration = value
				};
			}
		}
	}

	[Obsolete("allowHoverHapticsWhileSelecting has been deprecated in version 3.0.0. Use SimpleHapticFeedback.allowHoverHapticsWhileSelecting instead.")]
	public bool allowHoverHapticsWhileSelecting
	{
		get
		{
			return m_AllowHoverHapticsWhileSelecting;
		}
		set
		{
			m_AllowHoverHapticsWhileSelecting = value;
			if (Application.isPlaying)
			{
				GetOrCreateAndMigrateHapticFeedback();
				m_HapticFeedback.allowHoverHapticsWhileSelecting = value;
			}
		}
	}

	protected override void Awake()
	{
		targetsForSelection = new List<IXRSelectInteractable>();
		base.Awake();
		buttonReaders.Add(m_SelectInput);
		buttonReaders.Add(m_ActivateInput);
		xrController = base.gameObject.GetComponentInParent<XRBaseController>(includeInactive: true);
		if (m_HideControllerOnSelect && m_Controller == null)
		{
			Debug.LogWarning("Hide Controller On Select is deprecated and being used by this interactor. It is only functional if a deprecated XR Controller component is added to this GameObject or a parent GameObject. Use the Select Entered and Select Exited events to hide the controller instead.", this);
		}
		if ((m_PlayAudioClipOnSelectEntered && m_AudioClipForOnSelectEntered != null) || (m_PlayAudioClipOnSelectExited && m_AudioClipForOnSelectExited != null) || (m_PlayAudioClipOnSelectCanceled && m_AudioClipForOnSelectCanceled != null) || (m_PlayAudioClipOnHoverEntered && m_AudioClipForOnHoverEntered != null) || (m_PlayAudioClipOnHoverExited && m_AudioClipForOnHoverExited != null) || (m_PlayAudioClipOnHoverCanceled && m_AudioClipForOnHoverCanceled != null))
		{
			Debug.LogWarning("Audio Events are deprecated and being used by this interactor. Use the SimpleAudioFeedback component instead.", this);
			GetOrCreateAndMigrateAudioFeedback();
		}
		if (m_PlayHapticsOnSelectEntered || m_PlayHapticsOnSelectExited || m_PlayHapticsOnSelectCanceled || m_PlayHapticsOnHoverEntered || m_PlayHapticsOnHoverExited || m_PlayHapticsOnHoverCanceled)
		{
			Debug.LogWarning("Haptic Events are deprecated and being used by this interactor. Use the SimpleHapticFeedback component instead.", this);
			GetOrCreateAndMigrateHapticFeedback();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		buttonReaders.ForEach(delegate(XRInputButtonReader reader)
		{
			reader?.EnableDirectActionIfModeUsed();
		});
		valueReaders.ForEach(delegate(XRInputValueReader reader)
		{
			reader?.EnableDirectActionIfModeUsed();
		});
		WarnMixedInputConfiguration();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		buttonReaders.ForEach(delegate(XRInputButtonReader reader)
		{
			reader?.DisableDirectActionIfModeUsed();
		});
		valueReaders.ForEach(delegate(XRInputValueReader reader)
		{
			reader?.DisableDirectActionIfModeUsed();
		});
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			return;
		}
		if (forceDeprecatedInput)
		{
			if (m_Controller != null)
			{
				InteractionState selectInteractionState = m_Controller.selectInteractionState;
				m_LogicalSelectState.UpdateInput(selectInteractionState.active, selectInteractionState.activatedThisFrame, selectInteractionState.deactivatedThisFrame, base.hasSelection);
				InteractionState activateInteractionState = m_Controller.activateInteractionState;
				m_LogicalActivateState.UpdateInput(activateInteractionState.active, activateInteractionState.activatedThisFrame, activateInteractionState.deactivatedThisFrame, base.hasSelection);
			}
		}
		else
		{
			m_LogicalSelectState.UpdateInput(m_SelectInput.ReadIsPerformed(), m_SelectInput.ReadWasPerformedThisFrame(), m_SelectInput.ReadWasCompletedThisFrame(), base.hasSelection);
			m_LogicalActivateState.UpdateInput(m_ActivateInput.ReadIsPerformed(), m_ActivateInput.ReadWasPerformedThisFrame(), m_ActivateInput.ReadWasCompletedThisFrame(), base.hasSelection);
		}
	}

	public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractor(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || !m_AllowActivate)
		{
			return;
		}
		bool flag = shouldActivate;
		bool flag2 = shouldDeactivate;
		if (flag || flag2)
		{
			GetActivateTargets(s_ActivateTargets);
			if (flag)
			{
				SendActivateEvent(s_ActivateTargets);
			}
			if (flag2)
			{
				SendDeactivateEvent(s_ActivateTargets);
			}
		}
	}

	protected void SetInputProperty(ref XRInputButtonReader property, XRInputButtonReader value)
	{
		XRInputReaderUtility.SetInputProperty(ref property, value, this, buttonReaders);
	}

	protected void SetInputProperty<TValue>(ref XRInputValueReader<TValue> property, XRInputValueReader<TValue> value) where TValue : struct
	{
		XRInputReaderUtility.SetInputProperty(ref property, value, this, valueReaders);
	}

	private void SendActivateEvent(List<IXRActivateInteractable> targets)
	{
		foreach (IXRActivateInteractable target in targets)
		{
			if (target != null && !(target as Object == null))
			{
				ActivateEventArgs v;
				using (m_ActivateEventArgs.Get(out v))
				{
					v.interactorObject = this;
					v.interactableObject = target;
					target.OnActivated(v);
				}
			}
		}
	}

	private void SendDeactivateEvent(List<IXRActivateInteractable> targets)
	{
		foreach (IXRActivateInteractable target in targets)
		{
			if (target != null && !(target as Object == null))
			{
				DeactivateEventArgs v;
				using (m_DeactivateEventArgs.Get(out v))
				{
					v.interactorObject = this;
					v.interactableObject = target;
					target.OnDeactivated(v);
				}
			}
		}
	}

	public virtual void GetActivateTargets(List<IXRActivateInteractable> targets)
	{
		targets.Clear();
		if (base.hasSelection)
		{
			foreach (IXRSelectInteractable item3 in base.interactablesSelected)
			{
				if (item3 is IXRActivateInteractable item)
				{
					targets.Add(item);
				}
			}
			return;
		}
		if (!m_AllowHoveredActivate || !base.hasHover)
		{
			return;
		}
		foreach (IXRHoverInteractable item4 in base.interactablesHovered)
		{
			if (item4 is IXRActivateInteractable item2)
			{
				targets.Add(item2);
			}
		}
	}

	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		base.OnSelectEntering(args);
		m_LogicalSelectState.UpdateHasSelection(hasSelection: true);
		if (m_HideControllerOnSelect && m_Controller != null)
		{
			m_Controller.hideControllerModel = true;
		}
	}

	protected override void OnSelectExiting(SelectExitEventArgs args)
	{
		base.OnSelectExiting(args);
		if (!base.hasSelection)
		{
			m_LogicalSelectState.UpdateHasSelection(hasSelection: false);
			if (m_HideControllerOnSelect && m_Controller != null)
			{
				m_Controller.hideControllerModel = false;
			}
		}
	}

	public bool SendHapticImpulse(float amplitude, float duration)
	{
		if (m_HapticImpulsePlayer == null)
		{
			GetOrCreateHapticImpulsePlayer();
		}
		return m_HapticImpulsePlayer.SendHapticImpulse(amplitude, duration);
	}

	protected virtual void PlayAudio(AudioClip audioClip)
	{
		if (!(audioClip == null))
		{
			if (m_AudioSource == null)
			{
				GetOrCreateAudioSource();
			}
			m_AudioSource.PlayOneShot(audioClip);
		}
	}

	private void GetOrCreateAudioSource()
	{
		if (!TryGetComponent<AudioSource>(out m_AudioSource))
		{
			m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		}
		m_AudioSource.loop = false;
		m_AudioSource.playOnAwake = false;
	}

	private void GetOrCreateHapticImpulsePlayer()
	{
		m_HapticImpulsePlayer = HapticImpulsePlayer.GetOrCreateInHierarchy(base.gameObject);
	}

	private void GetOrCreateAndMigrateAudioFeedback()
	{
		if (!(m_AudioFeedback != null) && !TryGetComponent<SimpleAudioFeedback>(out m_AudioFeedback))
		{
			m_AudioFeedback = base.gameObject.AddComponent<SimpleAudioFeedback>();
			m_AudioFeedback.playSelectEntered = m_PlayAudioClipOnSelectEntered;
			m_AudioFeedback.selectEnteredClip = m_AudioClipForOnSelectEntered;
			m_AudioFeedback.playSelectExited = m_PlayAudioClipOnSelectExited;
			m_AudioFeedback.selectExitedClip = m_AudioClipForOnSelectExited;
			m_AudioFeedback.playSelectCanceled = m_PlayAudioClipOnSelectCanceled;
			m_AudioFeedback.selectCanceledClip = m_AudioClipForOnSelectCanceled;
			m_AudioFeedback.playHoverEntered = m_PlayAudioClipOnHoverEntered;
			m_AudioFeedback.hoverEnteredClip = m_AudioClipForOnHoverEntered;
			m_AudioFeedback.playHoverExited = m_PlayAudioClipOnHoverExited;
			m_AudioFeedback.hoverExitedClip = m_AudioClipForOnHoverExited;
			m_AudioFeedback.playHoverCanceled = m_PlayAudioClipOnHoverCanceled;
			m_AudioFeedback.hoverCanceledClip = m_AudioClipForOnHoverCanceled;
			m_AudioFeedback.allowHoverAudioWhileSelecting = m_AllowHoverAudioWhileSelecting;
			m_AudioFeedback.SetInteractorSource(this);
		}
	}

	private void GetOrCreateAndMigrateHapticFeedback()
	{
		if (!(m_HapticFeedback != null) && !TryGetComponent<SimpleHapticFeedback>(out m_HapticFeedback))
		{
			m_HapticFeedback = base.gameObject.AddComponent<SimpleHapticFeedback>();
			m_HapticFeedback.playSelectEntered = m_PlayHapticsOnSelectEntered;
			SimpleHapticFeedback hapticFeedback = m_HapticFeedback;
			if (hapticFeedback.selectEnteredData == null)
			{
				HapticImpulseData hapticImpulseData2 = (hapticFeedback.selectEnteredData = new HapticImpulseData());
			}
			m_HapticFeedback.selectEnteredData.amplitude = m_HapticSelectEnterIntensity;
			m_HapticFeedback.selectEnteredData.duration = m_HapticSelectEnterDuration;
			m_HapticFeedback.playSelectExited = m_PlayHapticsOnSelectExited;
			hapticFeedback = m_HapticFeedback;
			if (hapticFeedback.selectExitedData == null)
			{
				HapticImpulseData hapticImpulseData2 = (hapticFeedback.selectExitedData = new HapticImpulseData());
			}
			m_HapticFeedback.selectExitedData.amplitude = m_HapticSelectExitIntensity;
			m_HapticFeedback.selectExitedData.duration = m_HapticSelectExitDuration;
			m_HapticFeedback.playSelectCanceled = m_PlayHapticsOnSelectCanceled;
			hapticFeedback = m_HapticFeedback;
			if (hapticFeedback.selectCanceledData == null)
			{
				HapticImpulseData hapticImpulseData2 = (hapticFeedback.selectCanceledData = new HapticImpulseData());
			}
			m_HapticFeedback.selectCanceledData.amplitude = m_HapticSelectCancelIntensity;
			m_HapticFeedback.selectCanceledData.duration = m_HapticSelectCancelDuration;
			m_HapticFeedback.playHoverEntered = m_PlayHapticsOnHoverEntered;
			hapticFeedback = m_HapticFeedback;
			if (hapticFeedback.hoverEnteredData == null)
			{
				HapticImpulseData hapticImpulseData2 = (hapticFeedback.hoverEnteredData = new HapticImpulseData());
			}
			m_HapticFeedback.hoverEnteredData.amplitude = m_HapticHoverEnterIntensity;
			m_HapticFeedback.hoverEnteredData.duration = m_HapticHoverEnterDuration;
			m_HapticFeedback.playHoverExited = m_PlayHapticsOnHoverExited;
			hapticFeedback = m_HapticFeedback;
			if (hapticFeedback.hoverExitedData == null)
			{
				HapticImpulseData hapticImpulseData2 = (hapticFeedback.hoverExitedData = new HapticImpulseData());
			}
			m_HapticFeedback.hoverExitedData.amplitude = m_HapticHoverExitIntensity;
			m_HapticFeedback.hoverExitedData.duration = m_HapticHoverExitDuration;
			m_HapticFeedback.playHoverCanceled = m_PlayHapticsOnHoverCanceled;
			hapticFeedback = m_HapticFeedback;
			if (hapticFeedback.hoverCanceledData == null)
			{
				HapticImpulseData hapticImpulseData2 = (hapticFeedback.hoverCanceledData = new HapticImpulseData());
			}
			m_HapticFeedback.hoverCanceledData.amplitude = m_HapticHoverCancelIntensity;
			m_HapticFeedback.hoverCanceledData.duration = m_HapticHoverCancelDuration;
			m_HapticFeedback.allowHoverHapticsWhileSelecting = m_AllowHoverHapticsWhileSelecting;
			m_HapticFeedback.SetInteractorSource(this);
		}
	}

	[Obsolete("OnXRControllerChanged has been deprecated in version 3.0.0.")]
	private protected virtual void OnXRControllerChanged()
	{
		m_HasXRController = m_Controller != null;
	}

	private void WarnMixedInputConfiguration()
	{
		if (!forceDeprecatedInput)
		{
			return;
		}
		foreach (XRInputButtonReader buttonReader in buttonReaders)
		{
			if ((buttonReader.inputSourceMode == XRInputButtonReader.InputSourceMode.InputActionReference && (buttonReader.inputActionReferencePerformed != null || buttonReader.inputActionReferenceValue != null)) || (buttonReader.inputSourceMode != XRInputButtonReader.InputSourceMode.InputActionReference && buttonReader.inputSourceMode != 0))
			{
				Debug.LogWarning("The interactor has input properties configured to be used but the interactor is set to read input through the deprecated XR Controller component instead. If you want to force the input readers to be used even when an XR Controller component is present, set Input Compatibility Mode to Force Input Readers.", this);
				return;
			}
		}
		foreach (XRInputValueReader valueReader in valueReaders)
		{
			if ((valueReader.inputSourceMode == XRInputValueReader.InputSourceMode.InputActionReference && valueReader.inputActionReference != null) || (valueReader.inputSourceMode != XRInputValueReader.InputSourceMode.InputActionReference && valueReader.inputSourceMode != 0))
			{
				Debug.LogWarning("The interactor has input properties configured to be used but the interactor is set to read input through the deprecated XR Controller component instead. If you want to force the input readers to be used even when an XR Controller component is present, set Input Compatibility Mode to Force Input Readers.", this);
				break;
			}
		}
	}

	[Obsolete("CreateEffectsAudioSource has been deprecated in version 3.0.0.")]
	private void CreateEffectsAudioSource()
	{
	}

	[Obsolete("CanPlayHoverAudio has been deprecated in version 3.0.0.")]
	private bool CanPlayHoverAudio(IXRHoverInteractable hoveredInteractable)
	{
		if (!m_AllowHoverAudioWhileSelecting)
		{
			return !IsSelecting(hoveredInteractable);
		}
		return true;
	}

	[Obsolete("CanPlayHoverHaptics has been deprecated in version 3.0.0.")]
	private bool CanPlayHoverHaptics(IXRHoverInteractable hoveredInteractable)
	{
		if (!m_AllowHoverHapticsWhileSelecting)
		{
			return !IsSelecting(hoveredInteractable);
		}
		return true;
	}

	[Obsolete("HandleSelecting has been deprecated in version 3.0.0.")]
	private void HandleSelecting()
	{
	}

	[Obsolete("HandleDeselecting has been deprecated in version 3.0.0.")]
	private void HandleDeselecting()
	{
	}

	protected override void OnHoverEntering(HoverEnterEventArgs args)
	{
		base.OnHoverEntering(args);
	}

	protected override void OnHoverExiting(HoverExitEventArgs args)
	{
		base.OnHoverExiting(args);
	}

	private static ActivateEventArgs CreateActivateEventArgs()
	{
		return new ActivateEventArgs();
	}

	private static DeactivateEventArgs CreateDeactivateEventArgs()
	{
		return new DeactivateEventArgs();
	}

	Transform IXRInteractor.get_transform()
	{
		return base.transform;
	}
}
