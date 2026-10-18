using System.Diagnostics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Feedback;

[AddComponentMenu("XR/Feedback/Simple Audio Feedback", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Feedback.SimpleAudioFeedback.html")]
public class SimpleAudioFeedback : MonoBehaviour
{
	[SerializeField]
	[RequireInterface(typeof(IXRInteractor))]
	private Object m_InteractorSourceObject;

	[SerializeField]
	private AudioSource m_AudioSource;

	[SerializeField]
	private bool m_PlaySelectEntered;

	[SerializeField]
	private AudioClip m_SelectEnteredClip;

	[SerializeField]
	private bool m_PlaySelectExited;

	[SerializeField]
	private AudioClip m_SelectExitedClip;

	[SerializeField]
	private bool m_PlaySelectCanceled;

	[SerializeField]
	private AudioClip m_SelectCanceledClip;

	[SerializeField]
	private bool m_PlayHoverEntered;

	[SerializeField]
	private AudioClip m_HoverEnteredClip;

	[SerializeField]
	private bool m_PlayHoverExited;

	[SerializeField]
	private AudioClip m_HoverExitedClip;

	[SerializeField]
	private bool m_PlayHoverCanceled;

	[SerializeField]
	private AudioClip m_HoverCanceledClip;

	[SerializeField]
	private bool m_AllowHoverAudioWhileSelecting;

	private readonly UnityObjectReferenceCache<IXRInteractor, Object> m_InteractorSource = new UnityObjectReferenceCache<IXRInteractor, Object>();

	public AudioSource audioSource
	{
		get
		{
			return m_AudioSource;
		}
		set
		{
			m_AudioSource = value;
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

	public AudioClip selectEnteredClip
	{
		get
		{
			return m_SelectEnteredClip;
		}
		set
		{
			m_SelectEnteredClip = value;
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

	public AudioClip selectExitedClip
	{
		get
		{
			return m_SelectExitedClip;
		}
		set
		{
			m_SelectExitedClip = value;
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

	public AudioClip selectCanceledClip
	{
		get
		{
			return m_SelectCanceledClip;
		}
		set
		{
			m_SelectCanceledClip = value;
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

	public AudioClip hoverEnteredClip
	{
		get
		{
			return m_HoverEnteredClip;
		}
		set
		{
			m_HoverEnteredClip = value;
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

	public AudioClip hoverExitedClip
	{
		get
		{
			return m_HoverExitedClip;
		}
		set
		{
			m_HoverExitedClip = value;
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

	public AudioClip hoverCanceledClip
	{
		get
		{
			return m_HoverCanceledClip;
		}
		set
		{
			m_HoverCanceledClip = value;
		}
	}

	public bool allowHoverAudioWhileSelecting
	{
		get
		{
			return m_AllowHoverAudioWhileSelecting;
		}
		set
		{
			m_AllowHoverAudioWhileSelecting = value;
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
		if ((m_PlaySelectEntered || m_PlaySelectExited || m_PlaySelectCanceled || m_PlayHoverEntered || m_PlayHoverExited || m_PlayHoverCanceled) && m_AudioSource == null)
		{
			CreateAudioSource();
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

	protected void PlayAudio(AudioClip clip)
	{
		if (!(clip == null))
		{
			if (m_AudioSource == null)
			{
				CreateAudioSource();
			}
			m_AudioSource.PlayOneShot(clip);
		}
	}

	private void CreateAudioSource()
	{
		if (!TryGetComponent<AudioSource>(out m_AudioSource))
		{
			m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		}
		m_AudioSource.loop = false;
		m_AudioSource.playOnAwake = false;
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
			PlayAudio(m_SelectEnteredClip);
		}
	}

	private void OnSelectExited(SelectExitEventArgs args)
	{
		if (m_PlaySelectCanceled && args.isCanceled)
		{
			PlayAudio(m_SelectCanceledClip);
		}
		if (m_PlaySelectExited && !args.isCanceled)
		{
			PlayAudio(m_SelectExitedClip);
		}
	}

	private void OnHoverEntered(HoverEnterEventArgs args)
	{
		if (m_PlayHoverEntered && IsHoverAudioAllowed(args.interactorObject, args.interactableObject))
		{
			PlayAudio(m_HoverEnteredClip);
		}
	}

	private void OnHoverExited(HoverExitEventArgs args)
	{
		if (IsHoverAudioAllowed(args.interactorObject, args.interactableObject))
		{
			if (m_PlayHoverCanceled && args.isCanceled)
			{
				PlayAudio(m_HoverCanceledClip);
			}
			if (m_PlayHoverExited && !args.isCanceled)
			{
				PlayAudio(m_HoverExitedClip);
			}
		}
	}

	private bool IsHoverAudioAllowed(IXRInteractor interactor, IXRInteractable interactable)
	{
		if (!m_AllowHoverAudioWhileSelecting)
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
