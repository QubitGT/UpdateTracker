using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[AddComponentMenu("XR/Locomotion/Grab Move Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.GrabMoveProvider.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class GrabMoveProvider : ConstrainedMoveProvider
{
	[SerializeField]
	private Transform m_ControllerTransform;

	[SerializeField]
	private bool m_EnableMoveWhileSelecting;

	[SerializeField]
	private float m_MoveFactor = 1f;

	[SerializeField]
	private XRInputButtonReader m_GrabMoveInput = new XRInputButtonReader("Grab Move");

	private bool m_IsMoving;

	private Vector3 m_PreviousControllerLocalPosition;

	private readonly List<IXRSelectInteractor> m_ControllerInteractors = new List<IXRSelectInteractor>();

	[SerializeField]
	[Obsolete("m_GrabMoveAction has been deprecated. Please configure input action using m_GrabMoveInput instead.")]
	private InputActionProperty m_GrabMoveAction = new InputActionProperty(new InputAction("Grab Move", InputActionType.Button));

	public Transform controllerTransform
	{
		get
		{
			return m_ControllerTransform;
		}
		set
		{
			m_ControllerTransform = value;
			GatherControllerInteractors();
		}
	}

	public bool enableMoveWhileSelecting
	{
		get
		{
			return m_EnableMoveWhileSelecting;
		}
		set
		{
			m_EnableMoveWhileSelecting = value;
		}
	}

	public float moveFactor
	{
		get
		{
			return m_MoveFactor;
		}
		set
		{
			m_MoveFactor = value;
		}
	}

	public XRInputButtonReader grabMoveInput
	{
		get
		{
			return m_GrabMoveInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_GrabMoveInput, value, this);
		}
	}

	public bool canMove { get; set; } = true;


	[Obsolete("grabMoveAction has been deprecated. Please configure input action using grabMoveInput instead.")]
	public InputActionProperty grabMoveAction
	{
		get
		{
			return m_GrabMoveAction;
		}
		set
		{
			SetInputActionProperty(ref m_GrabMoveAction, value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_ControllerTransform == null)
		{
			m_ControllerTransform = base.transform;
		}
		GatherControllerInteractors();
		if (m_GrabMoveAction.reference != null || (m_GrabMoveAction.action != null && m_GrabMoveAction.action.bindings.Count > 0))
		{
			Debug.LogWarning("Grab Move Action has been deprecated. Please configure input action using Grab Move Input instead.", this);
		}
	}

	protected void OnEnable()
	{
		m_GrabMoveInput.EnableDirectActionIfModeUsed();
		m_GrabMoveAction.EnableDirectAction();
	}

	protected void OnDisable()
	{
		m_GrabMoveInput.DisableDirectActionIfModeUsed();
		m_GrabMoveAction.DisableDirectAction();
	}

	protected override Vector3 ComputeDesiredMove(out bool attemptingMove)
	{
		attemptingMove = false;
		GameObject gameObject = base.mediator.xrOrigin?.Origin;
		bool isMoving = m_IsMoving;
		m_IsMoving = canMove && IsGrabbing() && gameObject != null;
		if (!m_IsMoving)
		{
			return Vector3.zero;
		}
		Vector3 localPosition = controllerTransform.localPosition;
		if (!isMoving && m_IsMoving)
		{
			m_PreviousControllerLocalPosition = localPosition;
			return Vector3.zero;
		}
		attemptingMove = true;
		Vector3 result = gameObject.transform.TransformVector(m_PreviousControllerLocalPosition - localPosition) * m_MoveFactor;
		m_PreviousControllerLocalPosition = localPosition;
		return result;
	}

	public bool IsGrabbing()
	{
		bool flag = m_GrabMoveInput.ReadIsPerformed();
		InputAction action = m_GrabMoveAction.action;
		if (action != null)
		{
			flag |= action.IsPressed();
		}
		if (flag)
		{
			if (!m_EnableMoveWhileSelecting)
			{
				return !ControllerHasSelection();
			}
			return true;
		}
		return false;
	}

	private void GatherControllerInteractors()
	{
		m_ControllerInteractors.Clear();
		if (m_ControllerTransform != null)
		{
			m_ControllerTransform.transform.GetComponentsInChildren(m_ControllerInteractors);
		}
	}

	private bool ControllerHasSelection()
	{
		for (int i = 0; i < m_ControllerInteractors.Count; i++)
		{
			if (m_ControllerInteractors[i].hasSelection)
			{
				return true;
			}
		}
		return false;
	}

	private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
	{
		if (Application.isPlaying)
		{
			property.DisableDirectAction();
		}
		property = value;
		if (Application.isPlaying && base.isActiveAndEnabled)
		{
			property.EnableDirectAction();
		}
	}
}
