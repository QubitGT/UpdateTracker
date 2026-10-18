using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Helpers/Cinemachine Input Axis Controller")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineInputAxisController.html")]
public class CinemachineInputAxisController : InputAxisControllerBase<CinemachineInputAxisController.Reader>
{
	internal delegate void SetControlDefaultsForAxis(in IInputAxisOwner.AxisDescriptor axis, ref Controller controller);

	[Serializable]
	public sealed class Reader : IInputAxisReader
	{
		public delegate float ControlValueReader(InputAction action, IInputAxisOwner.AxisDescriptor.Hints hint, UnityEngine.Object context, ControlValueReader defaultReader);

		[Tooltip("Action for the Input package (if used).")]
		public InputActionReference InputAction;

		[Tooltip("The input value is multiplied by this amount prior to processing.  Controls the input power.  Set it to a negative value to invert the input")]
		public float Gain = 1f;

		internal InputAction m_CachedAction;

		[Tooltip("Enable this if the input value is inherently dependent on frame time.  For example, mouse deltas will naturally be bigger for longer frames, so in this case the default deltaTime scaling should be canceled.")]
		public bool CancelDeltaTime;

		public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
		{
			float num = 0f;
			if (InputAction != null && context is CinemachineInputAxisController context2)
			{
				num = ResolveAndReadInputAction(context2, hint) * Gain;
			}
			if (!(Time.deltaTime > 0f) || !CancelDeltaTime)
			{
				return num;
			}
			return num / Time.deltaTime;
		}

		private float ResolveAndReadInputAction(CinemachineInputAxisController context, IInputAxisOwner.AxisDescriptor.Hints hint)
		{
			if (m_CachedAction != null && InputAction.action.id != m_CachedAction.id)
			{
				m_CachedAction = null;
			}
			if (m_CachedAction == null)
			{
				m_CachedAction = InputAction.action;
				if (context.PlayerIndex != -1)
				{
					InputUser user2 = InputUser.all[context.PlayerIndex];
					m_CachedAction = GetFirstMatch(in user2, InputAction);
				}
				if (context.AutoEnableInputs && m_CachedAction != null)
				{
					m_CachedAction.Enable();
				}
			}
			if (m_CachedAction != null && m_CachedAction.enabled != InputAction.action.enabled)
			{
				if (InputAction.action.enabled)
				{
					m_CachedAction.Enable();
				}
				else
				{
					m_CachedAction.Disable();
				}
			}
			if (m_CachedAction != null)
			{
				if (context.ReadControlValueOverride != null)
				{
					return context.ReadControlValueOverride(m_CachedAction, hint, context, ReadInput);
				}
				return ReadInput(m_CachedAction, hint, context, null);
			}
			return 0f;
			static InputAction GetFirstMatch(in InputUser user, InputActionReference aRef)
			{
				IEnumerator<InputAction> enumerator = user.actions.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.id == aRef.action.id)
					{
						return enumerator.Current;
					}
				}
				return null;
			}
		}

		private float ReadInput(InputAction action, IInputAxisOwner.AxisDescriptor.Hints hint, UnityEngine.Object context, ControlValueReader defaultReader)
		{
			InputControl activeControl = action.activeControl;
			if (activeControl != null)
			{
				try
				{
					if (activeControl.valueType == typeof(Vector2) || action.expectedControlType == "Vector2")
					{
						Vector2 vector = action.ReadValue<Vector2>();
						return (hint == IInputAxisOwner.AxisDescriptor.Hints.Y) ? vector.y : vector.x;
					}
					return action.ReadValue<float>();
				}
				catch (InvalidOperationException)
				{
					Debug.LogError("An action in " + context.name + " is mapped to a " + activeControl.valueType.Name + " control.  CinemachineInputAxisController.Reader can only handle float or Vector2 types.  To handle other types you can install a custom handler for CinemachineInputAxisController.ReadControlValueOverride.");
				}
			}
			return 0f;
		}
	}

	[Tooltip("Leave this at -1 for single-player games.  For multi-player games, set this to be the player index, and the actions will be read from that player's controls")]
	public int PlayerIndex = -1;

	[Tooltip("If set, Input Actions will be auto-enabled at start")]
	public bool AutoEnableInputs = true;

	internal static SetControlDefaultsForAxis SetControlDefaults;

	public Reader.ControlValueReader ReadControlValueOverride;

	protected override void Reset()
	{
		base.Reset();
		PlayerIndex = -1;
		AutoEnableInputs = true;
	}

	protected override void InitializeControllerDefaultsForAxis(in IInputAxisOwner.AxisDescriptor axis, Controller controller)
	{
		SetControlDefaults?.Invoke(in axis, ref controller);
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			UpdateControllers();
		}
	}
}
