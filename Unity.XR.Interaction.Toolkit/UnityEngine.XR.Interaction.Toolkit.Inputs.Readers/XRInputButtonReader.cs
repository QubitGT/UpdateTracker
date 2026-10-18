using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[Serializable]
public class XRInputButtonReader : IXRInputButtonReader, IXRInputValueReader<float>, IXRInputValueReader
{
	private struct BypassScope : IDisposable
	{
		private readonly XRInputButtonReader m_Reader;

		public BypassScope(XRInputButtonReader reader)
		{
			m_Reader = reader;
			m_Reader.m_CallingBypass = true;
		}

		public void Dispose()
		{
			m_Reader.m_CallingBypass = false;
		}
	}

	public enum InputSourceMode
	{
		Unused,
		InputAction,
		InputActionReference,
		ObjectReference,
		ManualValue
	}

	[SerializeField]
	private InputSourceMode m_InputSourceMode = InputSourceMode.InputActionReference;

	[SerializeField]
	private InputAction m_InputActionPerformed;

	[SerializeField]
	private InputAction m_InputActionValue;

	[SerializeField]
	private InputActionReference m_InputActionReferencePerformed;

	[SerializeField]
	private InputActionReference m_InputActionReferenceValue;

	[SerializeField]
	[RequireInterface(typeof(IXRInputButtonReader))]
	private Object m_ObjectReferenceObject;

	[SerializeField]
	private bool m_ManualPerformed;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_ManualValue;

	[SerializeField]
	private bool m_ManualQueuePerformed;

	[SerializeField]
	private bool m_ManualQueueWasPerformedThisFrame;

	[SerializeField]
	private bool m_ManualQueueWasCompletedThisFrame;

	[SerializeField]
	private float m_ManualQueueValue;

	[SerializeField]
	private int m_ManualQueueTargetFrame;

	private int m_ManualFramePerformed;

	private int m_ManualFrameCompleted;

	private bool m_CallingBypass;

	private readonly UnityObjectReferenceCache<IXRInputButtonReader, Object> m_ObjectReference = new UnityObjectReferenceCache<IXRInputButtonReader, Object>();

	private readonly UnityObjectReferenceCache<InputActionReference> m_InputActionReferencePerformedCache = new UnityObjectReferenceCache<InputActionReference>();

	private readonly UnityObjectReferenceCache<InputActionReference> m_InputActionReferenceValueCache = new UnityObjectReferenceCache<InputActionReference>();

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

	public InputAction inputActionPerformed
	{
		get
		{
			return m_InputActionPerformed;
		}
		set
		{
			m_InputActionPerformed = value;
		}
	}

	public InputAction inputActionValue
	{
		get
		{
			return m_InputActionValue;
		}
		set
		{
			m_InputActionValue = value;
		}
	}

	public InputActionReference inputActionReferencePerformed
	{
		get
		{
			return m_InputActionReferencePerformed;
		}
		set
		{
			m_InputActionReferencePerformed = value;
		}
	}

	public InputActionReference inputActionReferenceValue
	{
		get
		{
			return m_InputActionReferenceValue;
		}
		set
		{
			m_InputActionReferenceValue = value;
		}
	}

	public bool manualPerformed
	{
		get
		{
			return m_ManualPerformed;
		}
		set
		{
			m_ManualPerformed = value;
		}
	}

	public float manualValue
	{
		get
		{
			return m_ManualValue;
		}
		set
		{
			m_ManualValue = value;
		}
	}

	public int manualFramePerformed
	{
		get
		{
			return m_ManualFramePerformed;
		}
		set
		{
			m_ManualFramePerformed = value;
		}
	}

	public int manualFrameCompleted
	{
		get
		{
			return m_ManualFrameCompleted;
		}
		set
		{
			m_ManualFrameCompleted = value;
		}
	}

	public IXRInputButtonReader bypass { get; set; }

	public XRInputButtonReader()
	{
	}

	public XRInputButtonReader(string name = null, string valueName = null, bool wantsInitialStateCheck = false, InputSourceMode inputSourceMode = InputSourceMode.InputActionReference)
	{
		m_InputActionPerformed = InputActionUtility.CreateButtonAction(name, wantsInitialStateCheck);
		m_InputActionValue = InputActionUtility.CreateValueAction(typeof(float), valueName ?? ((name != null) ? (name + " Value") : null));
		m_InputSourceMode = inputSourceMode;
	}

	public void EnableDirectActionIfModeUsed()
	{
		if (m_InputSourceMode == InputSourceMode.InputAction)
		{
			m_InputActionPerformed.Enable();
			m_InputActionValue.Enable();
		}
	}

	public void DisableDirectActionIfModeUsed()
	{
		if (m_InputSourceMode == InputSourceMode.InputAction)
		{
			m_InputActionPerformed.Disable();
			m_InputActionValue.Disable();
		}
	}

	public IXRInputButtonReader GetObjectReference()
	{
		return m_ObjectReference.Get(m_ObjectReferenceObject);
	}

	public void SetObjectReference(IXRInputButtonReader value)
	{
		m_ObjectReference.Set(ref m_ObjectReferenceObject, value);
	}

	public void QueueManualState(bool performed, float value)
	{
		QueueManualState(performed, value, !m_ManualPerformed && performed, m_ManualPerformed && !performed);
	}

	public void QueueManualState(bool performed, float value, bool performedThisFrame, bool completedThisFrame)
	{
		if (m_InputSourceMode != InputSourceMode.ManualValue)
		{
			Debug.LogWarning($"QueueManualState was called but the input source mode is set to {m_InputSourceMode}." + "You may want to set inputSourceMode to ManualValue for the manual state to be effective next frame.");
		}
		m_ManualQueuePerformed = performed;
		m_ManualQueueWasPerformedThisFrame = performedThisFrame;
		m_ManualQueueWasCompletedThisFrame = completedThisFrame;
		m_ManualQueueValue = value;
		m_ManualQueueTargetFrame = Time.frameCount + 1;
	}

	private void RefreshManualIfNeeded()
	{
		if (m_ManualQueueTargetFrame > 0 && Time.frameCount >= m_ManualQueueTargetFrame)
		{
			m_ManualPerformed = m_ManualQueuePerformed;
			if (m_ManualQueueWasPerformedThisFrame)
			{
				m_ManualFramePerformed = Time.frameCount;
			}
			if (m_ManualQueueWasCompletedThisFrame)
			{
				m_ManualFrameCompleted = Time.frameCount;
			}
			m_ManualValue = m_ManualQueueValue;
			m_ManualQueueTargetFrame = 0;
		}
	}

	public bool ReadIsPerformed()
	{
		if (bypass != null && !m_CallingBypass)
		{
			using (new BypassScope(this))
			{
				return bypass.ReadIsPerformed();
			}
		}
		switch (m_InputSourceMode)
		{
		default:
			return false;
		case InputSourceMode.InputAction:
			return IsPerformed(m_InputActionPerformed);
		case InputSourceMode.InputActionReference:
		{
			if (TryGetInputActionReferencePerformed(out var reference))
			{
				return IsPerformed(reference.action);
			}
			return false;
		}
		case InputSourceMode.ObjectReference:
			return GetObjectReference()?.ReadIsPerformed() ?? false;
		case InputSourceMode.ManualValue:
			RefreshManualIfNeeded();
			return m_ManualPerformed;
		}
	}

	public bool ReadWasPerformedThisFrame()
	{
		if (bypass != null && !m_CallingBypass)
		{
			using (new BypassScope(this))
			{
				return bypass.ReadWasPerformedThisFrame();
			}
		}
		switch (m_InputSourceMode)
		{
		default:
			return false;
		case InputSourceMode.InputAction:
			return WasPerformedThisFrame(m_InputActionPerformed);
		case InputSourceMode.InputActionReference:
		{
			if (TryGetInputActionReferencePerformed(out var reference))
			{
				return WasPerformedThisFrame(reference.action);
			}
			return false;
		}
		case InputSourceMode.ObjectReference:
			return GetObjectReference()?.ReadWasPerformedThisFrame() ?? false;
		case InputSourceMode.ManualValue:
			RefreshManualIfNeeded();
			if (m_ManualPerformed)
			{
				return m_ManualFramePerformed == Time.frameCount;
			}
			return false;
		}
	}

	public bool ReadWasCompletedThisFrame()
	{
		if (bypass != null && !m_CallingBypass)
		{
			using (new BypassScope(this))
			{
				return bypass.ReadWasCompletedThisFrame();
			}
		}
		switch (m_InputSourceMode)
		{
		default:
			return false;
		case InputSourceMode.InputAction:
			return WasCompletedThisFrame(m_InputActionPerformed);
		case InputSourceMode.InputActionReference:
		{
			if (TryGetInputActionReferencePerformed(out var reference))
			{
				return WasCompletedThisFrame(reference.action);
			}
			return false;
		}
		case InputSourceMode.ObjectReference:
			return GetObjectReference()?.ReadWasCompletedThisFrame() ?? false;
		case InputSourceMode.ManualValue:
			RefreshManualIfNeeded();
			if (!m_ManualPerformed)
			{
				return m_ManualFrameCompleted == Time.frameCount;
			}
			return false;
		}
	}

	public float ReadValue()
	{
		if (bypass != null && !m_CallingBypass)
		{
			using (new BypassScope(this))
			{
				return bypass.ReadValue();
			}
		}
		switch (m_InputSourceMode)
		{
		default:
			return 0f;
		case InputSourceMode.InputAction:
			return ReadValueToFloat(m_InputActionValue);
		case InputSourceMode.InputActionReference:
		{
			if (!TryGetInputActionReferenceValue(out var reference))
			{
				return 0f;
			}
			return ReadValueToFloat(reference.action);
		}
		case InputSourceMode.ObjectReference:
			return GetObjectReference()?.ReadValue() ?? 0f;
		case InputSourceMode.ManualValue:
			RefreshManualIfNeeded();
			return m_ManualValue;
		}
	}

	public bool TryReadValue(out float value)
	{
		if (bypass != null && !m_CallingBypass)
		{
			using (new BypassScope(this))
			{
				return bypass.TryReadValue(out value);
			}
		}
		switch (m_InputSourceMode)
		{
		default:
			value = 0f;
			return false;
		case InputSourceMode.InputAction:
			return TryReadValue(m_InputActionValue, out value);
		case InputSourceMode.InputActionReference:
		{
			if (TryGetInputActionReferenceValue(out var reference))
			{
				return TryReadValue(reference.action, out value);
			}
			value = 0f;
			return false;
		}
		case InputSourceMode.ObjectReference:
		{
			IXRInputButtonReader objectReference = GetObjectReference();
			if (objectReference != null)
			{
				return objectReference.TryReadValue(out value);
			}
			value = 0f;
			return false;
		}
		case InputSourceMode.ManualValue:
			RefreshManualIfNeeded();
			value = m_ManualValue;
			return true;
		}
	}

	private static bool IsPerformed(InputAction action)
	{
		if (action == null)
		{
			return false;
		}
		return action.phase switch
		{
			InputActionPhase.Disabled => false, 
			InputActionPhase.Performed => true, 
			_ => action.WasPerformedThisFrame(), 
		};
	}

	private static bool WasPerformedThisFrame(InputAction action)
	{
		return action?.WasPerformedThisFrame() ?? false;
	}

	private static bool WasCompletedThisFrame(InputAction action)
	{
		return action?.WasCompletedThisFrame() ?? false;
	}

	private float ReadValueToFloat(InputAction action)
	{
		if (action == null)
		{
			return 0f;
		}
		Type activeValueType = action.activeValueType;
		if (activeValueType == null || activeValueType == typeof(float))
		{
			return action.ReadValue<float>();
		}
		if (activeValueType == typeof(Vector2))
		{
			return action.ReadValue<Vector2>().magnitude;
		}
		return Mathf.Max(action.GetControlMagnitude(), 0f);
	}

	private bool TryReadValue(InputAction action, out float value)
	{
		if (action == null)
		{
			value = 0f;
			return false;
		}
		value = ReadValueToFloat(action);
		return action.IsInProgress();
	}

	private bool TryGetInputActionReferencePerformed(out InputActionReference reference)
	{
		return m_InputActionReferencePerformedCache.TryGet(m_InputActionReferencePerformed, out reference);
	}

	private bool TryGetInputActionReferenceValue(out InputActionReference reference)
	{
		return m_InputActionReferenceValueCache.TryGet(m_InputActionReferenceValue, out reference);
	}
}
