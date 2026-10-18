using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock.LowLevel;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.DualShock;

[InputControlLayout(stateType = typeof(DualShock4HIDInputReport), hideInUI = true, isNoisy = true)]
public class DualShock4GamepadHID : DualShockGamepad, IEventPreProcessor, IInputStateCallbackReceiver
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct DualShock4HIDGenericInputReport
	{
		[FieldOffset(0)]
		public byte leftStickX;

		[FieldOffset(1)]
		public byte leftStickY;

		[FieldOffset(2)]
		public byte rightStickX;

		[FieldOffset(3)]
		public byte rightStickY;

		[FieldOffset(4)]
		public byte buttons0;

		[FieldOffset(5)]
		public byte buttons1;

		[FieldOffset(6)]
		public byte buttons2;

		[FieldOffset(7)]
		public byte leftTrigger;

		[FieldOffset(8)]
		public byte rightTrigger;

		public static FourCC Format => new FourCC('H', 'I', 'D');

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DualShock4HIDInputReport ToHIDInputReport()
		{
			DualShock4HIDInputReport result = default(DualShock4HIDInputReport);
			result.leftStickX = leftStickX;
			result.leftStickY = leftStickY;
			result.rightStickX = rightStickX;
			result.rightStickY = rightStickY;
			result.leftTrigger = leftTrigger;
			result.rightTrigger = rightTrigger;
			result.buttons1 = buttons0;
			result.buttons2 = buttons1;
			result.buttons3 = buttons2;
			return result;
		}
	}

	private float? m_LowFrequencyMotorSpeed;

	private float? m_HighFrequenceyMotorSpeed;

	private Color? m_LightBarColor;

	internal const byte JitterMaskLow = 120;

	internal const byte JitterMaskHigh = 135;

	public ButtonControl leftTriggerButton { get; protected set; }

	public ButtonControl rightTriggerButton { get; protected set; }

	public ButtonControl playStationButton { get; protected set; }

	protected override void FinishSetup()
	{
		leftTriggerButton = GetChildControl<ButtonControl>("leftTriggerButton");
		rightTriggerButton = GetChildControl<ButtonControl>("rightTriggerButton");
		playStationButton = GetChildControl<ButtonControl>("systemButton");
		base.FinishSetup();
	}

	public override void PauseHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue || m_LightBarColor.HasValue)
		{
			DualShockHIDOutputReport command = DualShockHIDOutputReport.Create(base.hidDescriptor.outputReportSize);
			command.SetMotorSpeeds(0f, 0f);
			if (m_LightBarColor.HasValue)
			{
				command.SetColor(Color.black);
			}
			ExecuteCommand(ref command);
		}
	}

	public override void ResetHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue || m_LightBarColor.HasValue)
		{
			DualShockHIDOutputReport command = DualShockHIDOutputReport.Create(base.hidDescriptor.outputReportSize);
			command.SetMotorSpeeds(0f, 0f);
			if (m_LightBarColor.HasValue)
			{
				command.SetColor(Color.black);
			}
			ExecuteCommand(ref command);
			m_HighFrequenceyMotorSpeed = null;
			m_LowFrequencyMotorSpeed = null;
			m_LightBarColor = null;
		}
	}

	public override void ResumeHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue || m_LightBarColor.HasValue)
		{
			DualShockHIDOutputReport command = DualShockHIDOutputReport.Create(base.hidDescriptor.outputReportSize);
			if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue)
			{
				command.SetMotorSpeeds(m_LowFrequencyMotorSpeed.Value, m_HighFrequenceyMotorSpeed.Value);
			}
			if (m_LightBarColor.HasValue)
			{
				command.SetColor(m_LightBarColor.Value);
			}
			ExecuteCommand(ref command);
		}
	}

	public override void SetLightBarColor(Color color)
	{
		DualShockHIDOutputReport command = DualShockHIDOutputReport.Create(base.hidDescriptor.outputReportSize);
		command.SetColor(color);
		ExecuteCommand(ref command);
		m_LightBarColor = color;
	}

	public override void SetMotorSpeeds(float lowFrequency, float highFrequency)
	{
		DualShockHIDOutputReport command = DualShockHIDOutputReport.Create(base.hidDescriptor.outputReportSize);
		command.SetMotorSpeeds(lowFrequency, highFrequency);
		ExecuteCommand(ref command);
		m_LowFrequencyMotorSpeed = lowFrequency;
		m_HighFrequenceyMotorSpeed = highFrequency;
	}

	public bool SetMotorSpeedsAndLightBarColor(float lowFrequency, float highFrequency, Color color)
	{
		DualShockHIDOutputReport command = DualShockHIDOutputReport.Create(base.hidDescriptor.outputReportSize);
		command.SetMotorSpeeds(lowFrequency, highFrequency);
		command.SetColor(color);
		long num = ExecuteCommand(ref command);
		m_LowFrequencyMotorSpeed = lowFrequency;
		m_HighFrequenceyMotorSpeed = highFrequency;
		m_LightBarColor = color;
		return num >= 0;
	}

	unsafe bool IEventPreProcessor.PreProcessEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type != 1398030676)
		{
			return eventPtr.type != 1145852993;
		}
		StateEvent* ptr = StateEvent.FromUnchecked(eventPtr);
		if (ptr->stateFormat == DualShock4HIDInputReport.Format)
		{
			return true;
		}
		uint stateSizeInBytes = ptr->stateSizeInBytes;
		if (ptr->stateFormat != DualShock4HIDGenericInputReport.Format || stateSizeInBytes < sizeof(DualShock4HIDGenericInputReport))
		{
			return false;
		}
		byte* state = (byte*)ptr->state;
		switch (*state)
		{
		case 1:
		{
			if (stateSizeInBytes < sizeof(DualShock4HIDGenericInputReport) + 1)
			{
				return false;
			}
			DualShock4HIDInputReport dualShock4HIDInputReport2 = ((DualShock4HIDGenericInputReport*)(state + 1))->ToHIDInputReport();
			*(DualShock4HIDInputReport*)ptr->state = dualShock4HIDInputReport2;
			ptr->stateFormat = DualShock4HIDInputReport.Format;
			return true;
		}
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
			if ((state[1] & 0x80u) != 0)
			{
				if (stateSizeInBytes < sizeof(DualShock4HIDGenericInputReport) + 3)
				{
					return false;
				}
				DualShock4HIDInputReport dualShock4HIDInputReport = ((DualShock4HIDGenericInputReport*)(state + 3))->ToHIDInputReport();
				*(DualShock4HIDInputReport*)ptr->state = dualShock4HIDInputReport;
				ptr->stateFormat = DualShock4HIDInputReport.Format;
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public void OnNextUpdate()
	{
	}

	public unsafe void OnStateEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type == 1398030676 && eventPtr.stateFormat == DualShock4HIDInputReport.Format)
		{
			DualShock4HIDInputReport* ptr = (DualShock4HIDInputReport*)((byte*)base.currentStatePtr + m_StateBlock.byteOffset);
			DualShock4HIDInputReport* state = (DualShock4HIDInputReport*)StateEvent.FromUnchecked(eventPtr)->state;
			if (state->leftStickX >= 120 && state->leftStickX <= 135 && state->leftStickY >= 120 && state->leftStickY <= 135 && state->rightStickX >= 120 && state->rightStickX <= 135 && state->rightStickY >= 120 && state->rightStickY <= 135 && state->leftTrigger == ptr->leftTrigger && state->rightTrigger == ptr->rightTrigger && state->buttons1 == ptr->buttons1 && state->buttons2 == ptr->buttons2 && state->buttons3 == ptr->buttons3)
			{
				InputSystem.s_Manager.DontMakeCurrentlyUpdatingDeviceCurrent();
			}
		}
		InputState.Change(this, eventPtr);
	}

	public bool GetStateOffsetForEvent(InputControl control, InputEventPtr eventPtr, ref uint offset)
	{
		return false;
	}
}
