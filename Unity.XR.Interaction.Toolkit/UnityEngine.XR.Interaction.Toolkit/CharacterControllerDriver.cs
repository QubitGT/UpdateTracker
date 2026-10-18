using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Locomotion/Legacy/Character Controller Driver", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.CharacterControllerDriver.html")]
[Obsolete("CharacterControllerDriver is deprecated in XRI 3.0.0 and will be removed in a future release. Instead set useCharacterControllerIfExists to true on the instance of XRBodyTransformer in the scene, and then, if at runtime, re-enable the Body Transformer to make the locomotion system drive the CharacterController.", false)]
public class CharacterControllerDriver : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The Locomotion Provider object to listen to.")]
	private LocomotionProvider m_LocomotionProvider;

	[SerializeField]
	[Tooltip("The minimum height of the character's capsule that will be set by this behavior.")]
	private float m_MinHeight;

	[SerializeField]
	[Tooltip("The maximum height of the character's capsule that will be set by this behavior.")]
	private float m_MaxHeight = float.PositiveInfinity;

	private XROrigin m_XROrigin;

	private CharacterController m_CharacterController;

	public LocomotionProvider locomotionProvider
	{
		get
		{
			return m_LocomotionProvider;
		}
		set
		{
			Unsubscribe(m_LocomotionProvider);
			m_LocomotionProvider = value;
			Subscribe(m_LocomotionProvider);
			SetupCharacterController();
			UpdateCharacterController();
		}
	}

	public float minHeight
	{
		get
		{
			return m_MinHeight;
		}
		set
		{
			m_MinHeight = value;
		}
	}

	public float maxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			m_MaxHeight = value;
		}
	}

	protected XROrigin xrOrigin => m_XROrigin;

	[Obsolete("xrRig has been deprecated. Use xrOrigin instead.", true)]
	protected XRRig xrRig => null;

	protected CharacterController characterController => m_CharacterController;

	protected void Awake()
	{
		if (!(m_LocomotionProvider == null))
		{
			return;
		}
		m_LocomotionProvider = GetComponent<ContinuousMoveProviderBase>();
		if (m_LocomotionProvider == null)
		{
			m_LocomotionProvider = ComponentLocatorUtility<ContinuousMoveProviderBase>.FindComponent();
			if (m_LocomotionProvider == null)
			{
				Debug.LogWarning("Unable to drive properties of the Character Controller without the locomotion events of a Locomotion Provider. Set Locomotion Provider or ensure a Continuous Move Provider component is in your scene.", this);
			}
		}
	}

	protected void OnEnable()
	{
		Subscribe(m_LocomotionProvider);
	}

	protected void OnDisable()
	{
		Unsubscribe(m_LocomotionProvider);
	}

	protected void Start()
	{
		SetupCharacterController();
		UpdateCharacterController();
	}

	protected virtual void UpdateCharacterController()
	{
		if (!(m_XROrigin == null) && !(m_CharacterController == null))
		{
			float num = Mathf.Clamp(m_XROrigin.CameraInOriginSpaceHeight, m_MinHeight, m_MaxHeight);
			Vector3 cameraInOriginSpacePos = m_XROrigin.CameraInOriginSpacePos;
			cameraInOriginSpacePos.y = num / 2f + m_CharacterController.skinWidth;
			m_CharacterController.height = num;
			m_CharacterController.center = cameraInOriginSpacePos;
		}
	}

	private void Subscribe(LocomotionProvider provider)
	{
		if (provider != null)
		{
			provider.beginLocomotion += OnBeginLocomotion;
			provider.endLocomotion += OnEndLocomotion;
		}
	}

	private void Unsubscribe(LocomotionProvider provider)
	{
		if (provider != null)
		{
			provider.beginLocomotion -= OnBeginLocomotion;
			provider.endLocomotion -= OnEndLocomotion;
		}
	}

	private void SetupCharacterController()
	{
		if (!(m_LocomotionProvider == null) && !(m_LocomotionProvider.system == null))
		{
			m_XROrigin = m_LocomotionProvider.system.xrOrigin;
			m_CharacterController = ((m_XROrigin != null) ? m_XROrigin.Origin.GetComponent<CharacterController>() : null);
			if (m_CharacterController == null && m_XROrigin != null)
			{
				Debug.LogError($"Could not get CharacterController on {m_XROrigin.Origin}, unable to drive properties." + $" Ensure there is a CharacterController on the \"Rig\" GameObject of {m_XROrigin}.", this);
			}
		}
	}

	private void OnBeginLocomotion(LocomotionSystem system)
	{
		UpdateCharacterController();
	}

	private void OnEndLocomotion(LocomotionSystem system)
	{
		UpdateCharacterController();
	}
}
