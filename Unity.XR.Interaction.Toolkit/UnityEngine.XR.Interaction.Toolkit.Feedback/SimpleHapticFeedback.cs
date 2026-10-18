using System.Diagnostics;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Feedback;

[AddComponentMenu("XR/Feedback/Simple Haptic Feedback", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Feedback.SimpleHapticFeedback.html")]
public class SimpleHapticFeedback : MonoBehaviour
{
	[SerializeField]
	[RequireInterface(typeof(IXRInteractor))]
	private Object m_InteractorSourceObject;

	[SerializeField]
	private HapticImpulsePlayer m_HapticImpulsePlayer;

	[SerializeField]
	private bool m_PlaySelectEntered;

	[SerializeField]
	private HapticImpulseData m_SelectEnteredData = new HapticImpulseData
	{
		amplitude = 0.5f,
		duration = 0.1f
	};

	[SerializeField]
	private bool m_PlaySelectExited;

	[SerializeField]
	private HapticImpulseData m_SelectExitedData = new HapticImpulseData
	{
		amplitude = 0.5f,
		duration = 0.1f
	};

	[SerializeField]
	private bool m_PlaySelectCanceled;

	[SerializeField]
	private HapticImpulseData m_SelectCanceledData = new HapticImpulseData
	{
		amplitude = 0.5f,
		duration = 0.1f
	};

	[SerializeField]
	private bool m_PlayHoverEntered;

	[SerializeField]
	private HapticImpulseData m_HoverEnteredData = new HapticImpulseData
	{
		amplitude = 0.25f,
		duration = 0.1f
	};

	[SerializeField]
	private bool m_PlayHoverExited;

	[SerializeField]
	private HapticImpulseData m_HoverExitedData = new HapticImpulseData
	{
		amplitude = 0.25f,
		duration = 0.1f
	};

	[SerializeField]
	private bool m_PlayHoverCanceled;

	[SerializeField]
	private HapticImpulseData m_HoverCanceledData = new HapticImpulseData
	{
		amplitude = 0.25f,
		duration = 0.1f
	};

	[SerializeField]
	private bool m_AllowHoverHapticsWhileSelecting;

	private readonly UnityObjectReferenceCache<IXRInteractor, Object> m_InteractorSource = new UnityObjectReferenceCache<IXRInteractor, Object>();

	public HapticImpulsePlayer hapticImpulsePlayer
	{
		get
		{
			return m_HapticImpulsePlayer;
		}
		set
		{
			m_HapticImpulsePlayer = value;
		}
	}

	public bool playSelectEntered
	{
		get
		{
			return m_PlaySelectEntered;
		}
		set
		{
			m_PlaySelectEntered = value;
		}
	}

	public HapticImpulseData selectEnteredData
	{
		get
		{
			return m_SelectEnteredData;
		}
		set
		{
			m_SelectEnteredData = value;
		}
	}

	public bool playSelectExited
	{
		get
		{
			return m_PlaySelectExited;
		}
		set
		{
			m_PlaySelectExited = value;
		}
	}

	public HapticImpulseData selectExitedData
	{
		get
		{
			return m_SelectExitedData;
		}
		set
		{
			m_SelectExitedData = value;
		}
	}

	public bool playSelectCanceled
	{
		get
		{
			return m_PlaySelectCanceled;
		}
		set
		{
			m_PlaySelectCanceled = value;
		}
	}

	public HapticImpulseData selectCanceledData
	{
		get
		{
			return m_SelectCanceledData;
		}
		set
		{
			m_SelectCanceledData = value;
		}
	}

	public bool playHoverEntered
	{
		get
		{
			return m_PlayHoverEntered;
		}
		set
		{
			m_PlayHoverEntered = value;
		}
	}

	public HapticImpulseData hoverEnteredData
	{
		get
		{
			return m_HoverEnteredData;
		}
		set
		{
			m_HoverEnteredData = value;
		}
	}

	public bool playHoverExited
	{
		get
		{
			return m_PlayHoverExited;
		}
		set
		{
			m_PlayHoverExited = value;
		}
	}

	public HapticImpulseData hoverExitedData
	{
		get
		{
			return m_HoverExitedData;
		}
		set
		{
			m_HoverExitedData = value;
		}
	}

	public bool playHoverCanceled
	{
		get
		{
			return m_PlayHoverCanceled;
		}
		set
		{
			m_PlayHoverCanceled = value;
		}
	}

	public HapticImpulseData hoverCanceledData
	{
		get
		{
			return m_HoverCanceledData;
		}
		set
		{
			m_HoverCanceledData = value;
		}
	}

	public bool allowHoverHapticsWhileSelecting
	{
		get
		{
			return m_AllowHoverHapticsWhileSelecting;
		}
		set
		{
			m_AllowHoverHapticsWhileSelecting = value;
		}
	}

	[Conditional("UNITY_EDITOR")]
	protected void Reset()
	{
	}

	protected void Awake()
	{
		if (m_InteractorSourceObject == null)
		{
			m_InteractorSourceObject = GetComponentInParent<IXRInteractor>(includeInactive: true) as Object;
		}
		if ((m_PlaySelectEntered || m_PlaySelectExited || m_PlaySelectCanceled || m_PlayHoverEntered || m_PlayHoverExited || m_PlayHoverCanceled) && m_HapticImpulsePlayer == null)
		{
			CreateHapticImpulsePlayer();
		}
	}

	protected void OnEnable()
	{
		Subscribe(GetInteractorSource());
	}

	protected void OnDisable()
	{
		Unsubscribe(GetInteractorSource());
	}

	public IXRInteractor GetInteractorSource()
	{
		return m_InteractorSource.Get(m_InteractorSourceObject);
	}

	public void SetInteractorSource(IXRInteractor interactor)
	{
		if (Application.isPlaying && base.isActiveAndEnabled)
		{
			Unsubscribe(m_InteractorSource.Get(m_InteractorSourceObject));
		}
		m_InteractorSource.Set(ref m_InteractorSourceObject, interactor);
		if (Application.isPlaying && base.isActiveAndEnabled)
		{
			Subscribe(interactor);
		}
	}

	protected bool SendHapticImpulse(HapticImpulseData data)
	{
		if (data != null)
		{
			return SendHapticImpulse(data.amplitude, data.duration, data.frequency);
		}
		return false;
	}

	protected bool SendHapticImpulse(float amplitude, float duration, float frequency)
	{
		if (m_HapticImpulsePlayer == null)
		{
			CreateHapticImpulsePlayer();
		}
		return m_HapticImpulsePlayer.SendHapticImpulse(amplitude, duration, frequency);
	}

	private void CreateHapticImpulsePlayer()
	{
		m_HapticImpulsePlayer = HapticImpulsePlayer.GetOrCreateInHierarchy(base.gameObject);
	}

	private void Subscribe(IXRInteractor interactor)
	{
		if (interactor != null && (!(interactor is Object @object) || !(@object == null)))
		{
			if (interactor is IXRSelectInteractor iXRSelectInteractor)
			{
				iXRSelectInteractor.selectEntered.AddListener(OnSelectEntered);
				iXRSelectInteractor.selectExited.AddListener(OnSelectExited);
			}
			if (interactor is IXRHoverInteractor iXRHoverInteractor)
			{
				iXRHoverInteractor.hoverEntered.AddListener(OnHoverEntered);
				iXRHoverInteractor.hoverExited.AddListener(OnHoverExited);
			}
		}
	}

	private void Unsubscribe(IXRInteractor interactor)
	{
		if (interactor != null && (!(interactor is Object @object) || !(@object == null)))
		{
			if (interactor is IXRSelectInteractor iXRSelectInteractor)
			{
				iXRSelectInteractor.selectEntered.RemoveListener(OnSelectEntered);
				iXRSelectInteractor.selectExited.RemoveListener(OnSelectExited);
			}
			if (interactor is IXRHoverInteractor iXRHoverInteractor)
			{
				iXRHoverInteractor.hoverEntered.RemoveListener(OnHoverEntered);
				iXRHoverInteractor.hoverExited.RemoveListener(OnHoverExited);
			}
		}
	}

	private void OnSelectEntered(SelectEnterEventArgs args)
	{
		if (m_PlaySelectEntered)
		{
			SendHapticImpulse(m_SelectEnteredData);
		}
	}

	private void OnSelectExited(SelectExitEventArgs args)
	{
		if (m_PlaySelectCanceled && args.isCanceled)
		{
			SendHapticImpulse(m_SelectCanceledData);
		}
		if (m_PlaySelectExited && !args.isCanceled)
		{
			SendHapticImpulse(m_SelectExitedData);
		}
	}

	private void OnHoverEntered(HoverEnterEventArgs args)
	{
		if (m_PlayHoverEntered && IsHoverHapticsAllowed(args.interactorObject, args.interactableObject))
		{
			SendHapticImpulse(m_HoverEnteredData);
		}
	}

	private void OnHoverExited(HoverExitEventArgs args)
	{
		if (IsHoverHapticsAllowed(args.interactorObject, args.interactableObject))
		{
			if (m_PlayHoverCanceled && args.isCanceled)
			{
				SendHapticImpulse(m_HoverCanceledData);
			}
			if (m_PlayHoverExited && !args.isCanceled)
			{
				SendHapticImpulse(m_HoverExitedData);
			}
		}
	}

	private bool IsHoverHapticsAllowed(IXRInteractor interactor, IXRInteractable interactable)
	{
		if (!m_AllowHoverHapticsWhileSelecting)
		{
			return !IsSelecting(interactor, interactable);
		}
		return true;
	}

	private static bool IsSelecting(IXRInteractor interactor, IXRInteractable interactable)
	{
		if (interactor is IXRSelectInteractor iXRSelectInteractor && interactable is IXRSelectInteractable interactable2)
		{
			return iXRSelectInteractor.IsSelecting(interactable2);
		}
		return false;
	}
}
