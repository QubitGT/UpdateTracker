using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[HelpURL("https://developer.oculus.com/documentation/unity/controller-animations/")]
public class OVRControllerHelper : MonoBehaviour, OVRInputModule.InputSource
{
	private enum ControllerType
	{
		QuestAndRiftS = 1,
		Rift,
		Quest2,
		TouchPro,
		TouchPlus
	}

	public GameObject m_modelOculusTouchQuestAndRiftSLeftController;

	public GameObject m_modelOculusTouchQuestAndRiftSRightController;

	public GameObject m_modelOculusTouchRiftLeftController;

	public GameObject m_modelOculusTouchRiftRightController;

	public GameObject m_modelOculusTouchQuest2LeftController;

	public GameObject m_modelOculusTouchQuest2RightController;

	public GameObject m_modelMetaTouchProLeftController;

	public GameObject m_modelMetaTouchProRightController;

	public GameObject m_modelMetaTouchPlusLeftController;

	public GameObject m_modelMetaTouchPlusRightController;

	public OVRInput.Controller m_controller;

	public OVRInput.InputDeviceShowState m_showState = OVRInput.InputDeviceShowState.ControllerInHandOrNoHand;

	public bool showWhenHandsArePoweredByNaturalControllerPoses;

	private Animator m_animator;

	public OVRRayHelper RayHelper;

	private GameObject m_activeController;

	private bool m_controllerModelsInitialized;

	private bool m_hasInputFocus = true;

	private bool m_hasInputFocusPrev;

	private bool m_isActive;

	private ControllerType activeControllerType = ControllerType.Rift;

	private bool m_prevControllerConnected;

	private bool m_prevControllerConnectedCached;

	private OVRInput.ControllerInHandState m_prevControllerInHandState;

	private void Start()
	{
		if (OVRManager.OVRManagerinitialized)
		{
			InitializeControllerModels();
		}
	}

	private void OnEnable()
	{
		OVRInputModule.TrackInputSource(this);
		SceneManager.activeSceneChanged += OnSceneChanged;
	}

	private void OnDisable()
	{
		OVRInputModule.UntrackInputSource(this);
		SceneManager.activeSceneChanged -= OnSceneChanged;
	}

	private void OnSceneChanged(Scene unloading, Scene loading)
	{
		OVRInputModule.TrackInputSource(this);
	}

	private void InitializeControllerModels()
	{
		if (m_controllerModelsInitialized)
		{
			return;
		}
		OVRPlugin.SystemHeadset systemHeadsetType = OVRPlugin.GetSystemHeadsetType();
		OVRPlugin.Hand hand = ((m_controller != OVRInput.Controller.LTouch) ? OVRPlugin.Hand.HandRight : OVRPlugin.Hand.HandLeft);
		OVRPlugin.InteractionProfile interactionProfile = OVRPlugin.GetCurrentInteractionProfile(hand);
		if (OVRPlugin.IsMultimodalHandsControllersSupported())
		{
			OVRPlugin.InteractionProfile currentDetachedInteractionProfile = OVRPlugin.GetCurrentDetachedInteractionProfile(hand);
			if (currentDetachedInteractionProfile != 0)
			{
				interactionProfile = currentDetachedInteractionProfile;
			}
		}
		switch (systemHeadsetType)
		{
		case OVRPlugin.SystemHeadset.Rift_CV1:
			activeControllerType = ControllerType.Rift;
			break;
		case OVRPlugin.SystemHeadset.Oculus_Quest_2:
			if (interactionProfile == OVRPlugin.InteractionProfile.TouchPro)
			{
				activeControllerType = ControllerType.TouchPro;
			}
			else
			{
				activeControllerType = ControllerType.Quest2;
			}
			break;
		case OVRPlugin.SystemHeadset.Oculus_Link_Quest_2:
			if (interactionProfile == OVRPlugin.InteractionProfile.TouchPro)
			{
				activeControllerType = ControllerType.TouchPro;
			}
			else
			{
				activeControllerType = ControllerType.Quest2;
			}
			break;
		case OVRPlugin.SystemHeadset.Meta_Quest_Pro:
			activeControllerType = ControllerType.TouchPro;
			break;
		case OVRPlugin.SystemHeadset.Meta_Link_Quest_Pro:
			activeControllerType = ControllerType.TouchPro;
			break;
		case OVRPlugin.SystemHeadset.Meta_Quest_3:
		case OVRPlugin.SystemHeadset.Meta_Quest_3S:
		case OVRPlugin.SystemHeadset.Meta_Link_Quest_3:
		case OVRPlugin.SystemHeadset.Meta_Link_Quest_3S:
			if (interactionProfile == OVRPlugin.InteractionProfile.TouchPro)
			{
				activeControllerType = ControllerType.TouchPro;
			}
			else
			{
				activeControllerType = ControllerType.TouchPlus;
			}
			break;
		default:
			activeControllerType = ControllerType.QuestAndRiftS;
			break;
		}
		Debug.LogFormat("OVRControllerHelp: Active controller type: {0} for product {1} (headset {2}, hand {3})", activeControllerType, OVRPlugin.productName, systemHeadsetType, hand);
		m_modelOculusTouchQuestAndRiftSLeftController.SetActive(value: false);
		m_modelOculusTouchQuestAndRiftSRightController.SetActive(value: false);
		m_modelOculusTouchRiftLeftController.SetActive(value: false);
		m_modelOculusTouchRiftRightController.SetActive(value: false);
		m_modelOculusTouchQuest2LeftController.SetActive(value: false);
		m_modelOculusTouchQuest2RightController.SetActive(value: false);
		m_modelMetaTouchProLeftController.SetActive(value: false);
		m_modelMetaTouchProRightController.SetActive(value: false);
		m_modelMetaTouchPlusLeftController.SetActive(value: false);
		m_modelMetaTouchPlusRightController.SetActive(value: false);
		OVRManager.InputFocusAcquired += InputFocusAquired;
		OVRManager.InputFocusLost += InputFocusLost;
		m_controllerModelsInitialized = true;
	}

	private void Update()
	{
		m_isActive = false;
		if (!m_controllerModelsInitialized)
		{
			if (!OVRManager.OVRManagerinitialized)
			{
				return;
			}
			InitializeControllerModels();
		}
		OVRInput.ControllerInHandState controllerIsInHandState = OVRInput.GetControllerIsInHandState((m_controller != OVRInput.Controller.LTouch) ? OVRInput.Hand.HandRight : OVRInput.Hand.HandLeft);
		bool flag = OVRInput.IsControllerConnected(m_controller);
		if (flag != m_prevControllerConnected || !m_prevControllerConnectedCached || controllerIsInHandState != m_prevControllerInHandState || m_hasInputFocus != m_hasInputFocusPrev)
		{
			if (activeControllerType == ControllerType.Rift)
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(value: false);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(value: false);
				m_modelOculusTouchRiftLeftController.SetActive(flag && m_controller == OVRInput.Controller.LTouch);
				m_modelOculusTouchRiftRightController.SetActive(flag && m_controller == OVRInput.Controller.RTouch);
				m_modelOculusTouchQuest2LeftController.SetActive(value: false);
				m_modelOculusTouchQuest2RightController.SetActive(value: false);
				m_modelMetaTouchProLeftController.SetActive(value: false);
				m_modelMetaTouchProRightController.SetActive(value: false);
				m_modelMetaTouchPlusLeftController.SetActive(value: false);
				m_modelMetaTouchPlusRightController.SetActive(value: false);
				m_animator = ((m_controller == OVRInput.Controller.LTouch) ? m_modelOculusTouchRiftLeftController.GetComponent<Animator>() : m_modelOculusTouchRiftRightController.GetComponent<Animator>());
				m_activeController = ((m_controller == OVRInput.Controller.LTouch) ? m_modelOculusTouchRiftLeftController : m_modelOculusTouchRiftRightController);
			}
			else if (activeControllerType == ControllerType.Quest2)
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(value: false);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(value: false);
				m_modelOculusTouchRiftLeftController.SetActive(value: false);
				m_modelOculusTouchRiftRightController.SetActive(value: false);
				m_modelOculusTouchQuest2LeftController.SetActive(flag && m_controller == OVRInput.Controller.LTouch);
				m_modelOculusTouchQuest2RightController.SetActive(flag && m_controller == OVRInput.Controller.RTouch);
				m_modelMetaTouchProLeftController.SetActive(value: false);
				m_modelMetaTouchProRightController.SetActive(value: false);
				m_modelMetaTouchPlusLeftController.SetActive(value: false);
				m_modelMetaTouchPlusRightController.SetActive(value: false);
				m_animator = ((m_controller == OVRInput.Controller.LTouch) ? m_modelOculusTouchQuest2LeftController.GetComponent<Animator>() : m_modelOculusTouchQuest2RightController.GetComponent<Animator>());
				m_activeController = ((m_controller == OVRInput.Controller.LTouch) ? m_modelOculusTouchQuest2LeftController : m_modelOculusTouchQuest2RightController);
			}
			else if (activeControllerType == ControllerType.QuestAndRiftS)
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(flag && m_controller == OVRInput.Controller.LTouch);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(flag && m_controller == OVRInput.Controller.RTouch);
				m_modelOculusTouchRiftLeftController.SetActive(value: false);
				m_modelOculusTouchRiftRightController.SetActive(value: false);
				m_modelOculusTouchQuest2LeftController.SetActive(value: false);
				m_modelOculusTouchQuest2RightController.SetActive(value: false);
				m_modelMetaTouchProLeftController.SetActive(value: false);
				m_modelMetaTouchProRightController.SetActive(value: false);
				m_modelMetaTouchPlusLeftController.SetActive(value: false);
				m_modelMetaTouchPlusRightController.SetActive(value: false);
				m_animator = ((m_controller == OVRInput.Controller.LTouch) ? m_modelOculusTouchQuestAndRiftSLeftController.GetComponent<Animator>() : m_modelOculusTouchQuestAndRiftSRightController.GetComponent<Animator>());
				m_activeController = ((m_controller == OVRInput.Controller.LTouch) ? m_modelOculusTouchQuestAndRiftSLeftController : m_modelOculusTouchQuestAndRiftSRightController);
			}
			else if (activeControllerType == ControllerType.TouchPro)
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(value: false);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(value: false);
				m_modelOculusTouchRiftLeftController.SetActive(value: false);
				m_modelOculusTouchRiftRightController.SetActive(value: false);
				m_modelOculusTouchQuest2LeftController.SetActive(value: false);
				m_modelOculusTouchQuest2RightController.SetActive(value: false);
				m_modelMetaTouchProLeftController.SetActive(flag && m_controller == OVRInput.Controller.LTouch);
				m_modelMetaTouchProRightController.SetActive(flag && m_controller == OVRInput.Controller.RTouch);
				m_modelMetaTouchPlusLeftController.SetActive(value: false);
				m_modelMetaTouchPlusRightController.SetActive(value: false);
				m_animator = ((m_controller == OVRInput.Controller.LTouch) ? m_modelMetaTouchProLeftController.GetComponent<Animator>() : m_modelMetaTouchProRightController.GetComponent<Animator>());
				m_activeController = ((m_controller == OVRInput.Controller.LTouch) ? m_modelMetaTouchProLeftController : m_modelMetaTouchProRightController);
			}
			else
			{
				m_modelOculusTouchQuestAndRiftSLeftController.SetActive(value: false);
				m_modelOculusTouchQuestAndRiftSRightController.SetActive(value: false);
				m_modelOculusTouchRiftLeftController.SetActive(value: false);
				m_modelOculusTouchRiftRightController.SetActive(value: false);
				m_modelOculusTouchQuest2LeftController.SetActive(value: false);
				m_modelOculusTouchQuest2RightController.SetActive(value: false);
				m_modelMetaTouchProLeftController.SetActive(value: false);
				m_modelMetaTouchProRightController.SetActive(value: false);
				m_modelMetaTouchPlusLeftController.SetActive(flag && m_controller == OVRInput.Controller.LTouch);
				m_modelMetaTouchPlusRightController.SetActive(flag && m_controller == OVRInput.Controller.RTouch);
				m_animator = ((m_controller == OVRInput.Controller.LTouch) ? m_modelMetaTouchPlusLeftController.GetComponent<Animator>() : m_modelMetaTouchPlusRightController.GetComponent<Animator>());
				m_activeController = ((m_controller == OVRInput.Controller.LTouch) ? m_modelMetaTouchPlusLeftController : m_modelMetaTouchPlusRightController);
			}
			m_prevControllerConnected = flag;
			m_prevControllerConnectedCached = true;
			m_prevControllerInHandState = controllerIsInHandState;
			m_hasInputFocusPrev = m_hasInputFocus;
		}
		bool flag2 = m_hasInputFocus && flag;
		switch (m_showState)
		{
		case OVRInput.InputDeviceShowState.ControllerInHandOrNoHand:
			if (controllerIsInHandState == OVRInput.ControllerInHandState.ControllerNotInHand)
			{
				flag2 = false;
			}
			break;
		case OVRInput.InputDeviceShowState.ControllerInHand:
			if (controllerIsInHandState != OVRInput.ControllerInHandState.ControllerInHand)
			{
				flag2 = false;
			}
			break;
		case OVRInput.InputDeviceShowState.ControllerNotInHand:
			if (controllerIsInHandState != OVRInput.ControllerInHandState.ControllerNotInHand)
			{
				flag2 = false;
			}
			break;
		case OVRInput.InputDeviceShowState.NoHand:
			if (controllerIsInHandState != 0)
			{
				flag2 = false;
			}
			break;
		}
		if (!showWhenHandsArePoweredByNaturalControllerPoses && OVRPlugin.IsControllerDrivenHandPosesEnabled() && OVRPlugin.AreControllerDrivenHandPosesNatural())
		{
			flag2 = false;
		}
		m_isActive = flag2;
		if (m_activeController != null)
		{
			m_activeController.SetActive(flag2);
		}
		if (RayHelper != null)
		{
			RayHelper.gameObject.SetActive(flag2);
		}
		if (m_animator != null && m_animator.gameObject.activeSelf)
		{
			m_animator.SetFloat("Button 1", OVRInput.Get(OVRInput.Button.One, m_controller) ? 1f : 0f);
			m_animator.SetFloat("Button 2", OVRInput.Get(OVRInput.Button.Two, m_controller) ? 1f : 0f);
			m_animator.SetFloat("Button 3", OVRInput.Get(OVRInput.Button.Start, m_controller) ? 1f : 0f);
			m_animator.SetFloat("Joy X", OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_controller).x);
			m_animator.SetFloat("Joy Y", OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_controller).y);
			m_animator.SetFloat("Trigger", OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller));
			m_animator.SetFloat("Grip", OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller));
		}
	}

	public void InputFocusAquired()
	{
		m_hasInputFocus = true;
	}

	public void InputFocusLost()
	{
		m_hasInputFocus = false;
	}

	public bool IsPressed()
	{
		return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_controller);
	}

	public bool IsReleased()
	{
		return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, m_controller);
	}

	public Transform GetPointerRayTransform()
	{
		return base.transform;
	}

	public bool IsValid()
	{
		return this != null;
	}

	public bool IsActive()
	{
		return m_isActive;
	}

	public OVRPlugin.Hand GetHand()
	{
		if (m_controller != OVRInput.Controller.LTouch)
		{
			return OVRPlugin.Hand.HandRight;
		}
		return OVRPlugin.Hand.HandLeft;
	}

	public void UpdatePointerRay(OVRInputRayData rayData)
	{
		if ((bool)RayHelper)
		{
			rayData.IsActive = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, m_controller);
			rayData.ActivationStrength = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
			RayHelper.UpdatePointerRay(rayData);
		}
	}
}
