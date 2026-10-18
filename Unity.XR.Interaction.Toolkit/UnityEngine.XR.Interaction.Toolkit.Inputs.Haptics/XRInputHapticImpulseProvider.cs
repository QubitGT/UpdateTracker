using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[Serializable]
public class XRInputHapticImpulseProvider : IXRHapticImpulseProvider
{
	public enum InputSourceMode
	{
		Unused,
		InputAction,
		InputActionReference,
		ObjectReference
	}

	[SerializeField]
	private InputSourceMode m_InputSourceMode = InputSourceMode.InputActionReference;

	[SerializeField]
	private InputAction m_InputAction;

	[SerializeField]
	private InputActionReference m_InputActionReference;

	[SerializeField]
	[RequireInterface(typeof(IXRHapticImpulseProvider))]
	private Object m_ObjectReferenceObject;

	private readonly UnityObjectReferenceCache<IXRHapticImpulseProvider, Object> m_ObjectReference = new UnityObjectReferenceCache<IXRHapticImpulseProvider, Object>();

	private HapticControlActionManager m_HapticControlActionManager;

	public InputSourceMode inputSourceMode
	{
		get
		{
			return m_InputSourceMode;
		}
		set
		{
			m_InputSourceMode = value;
		}
	}

	public InputAction inputAction
	{
		get
		{
			return m_InputAction;
		}
		set
		{
			m_InputAction = value;
		}
	}

	public InputActionReference inputActionReference
	{
		get
		{
			return m_InputActionReference;
		}
		set
		{
			m_InputActionReference = value;
		}
	}

	public XRInputHapticImpulseProvider()
	{
	}

	public XRInputHapticImpulseProvider(string name = null, bool wantsInitialStateCheck = false, InputSourceMode inputSourceMode = InputSourceMode.InputActionReference)
	{
		m_InputAction = InputActionUtility.CreatePassThroughAction(null, name, wantsInitialStateCheck);
		m_InputSourceMode = inputSourceMode;
	}

	public void EnableDirectActionIfModeUsed()
	{
		if (m_InputSourceMode == InputSourceMode.InputAction)
		{
			m_InputAction.Enable();
		}
	}

	public void DisableDirectActionIfModeUsed()
	{
		if (m_InputSourceMode == InputSourceMode.InputAction)
		{
			m_InputAction.Disable();
		}
	}

	public IXRHapticImpulseProvider GetObjectReference()
	{
		return m_ObjectReference.Get(m_ObjectReferenceObject);
	}

	public void SetObjectReference(IXRHapticImpulseProvider value)
	{
		m_ObjectReference.Set(ref m_ObjectReferenceObject, value);
	}

	public IXRHapticImpulseChannelGroup GetChannelGroup()
	{
		switch (m_InputSourceMode)
		{
		default:
			return null;
		case InputSourceMode.InputAction:
			if (m_HapticControlActionManager == null)
			{
				m_HapticControlActionManager = new HapticControlActionManager();
			}
			return m_HapticControlActionManager.GetChannelGroup(m_InputAction);
		case InputSourceMode.InputActionReference:
			if (m_InputActionReference != null)
			{
				if (m_HapticControlActionManager == null)
				{
					m_HapticControlActionManager = new HapticControlActionManager();
				}
				return m_HapticControlActionManager.GetChannelGroup(m_InputActionReference.action);
			}
			return null;
		case InputSourceMode.ObjectReference:
			return GetObjectReference()?.GetChannelGroup();
		}
	}
}
