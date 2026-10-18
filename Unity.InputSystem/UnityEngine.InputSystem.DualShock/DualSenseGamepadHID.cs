using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock.LowLevel;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.DualShock;

[InputControlLayout(stateType = typeof(DualSenseHIDInputReport), displayName = "DualSense HID")]
public class DualSenseGamepadHID : DualShockGamepad, IEventMerger, IEventPreProcessor, IInputStateCallbackReceiver
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct DualSenseHIDGenericInputReport
	{
		[FieldOffset(0)]
		public byte reportId;

		public static FourCC Format => new FourCC('H', 'I', 'D');
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct DualSenseHIDUSBInputReport
	{
		public const int ExpectedReportId = 1;

		[FieldOffset(0)]
		public byte reportId;

		[FieldOffset(1)]
		public byte leftStickX;

		[FieldOffset(2)]
		public byte leftStickY;

		[FieldOffset(3)]
		public byte rightStickX;

		[FieldOffset(4)]
		public byte rightStickY;

		[FieldOffset(5)]
		public byte leftTrigger;

		[FieldOffset(6)]
		public byte rightTrigger;

		[FieldOffset(8)]
		public byte buttons0;

		[FieldOffset(9)]
		public byte buttons1;

		[FieldOffset(10)]
		public byte buttons2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DualSenseHIDInputReport ToHIDInputReport()
		{
			DualSenseHIDInputReport result = default(DualSenseHIDInputReport);
			result.leftStickX = leftStickX;
			result.leftStickY = leftStickY;
			result.rightStickX = rightStickX;
			result.rightStickY = rightStickY;
			result.leftTrigger = leftTrigger;
			result.rightTrigger = rightTrigger;
			result.buttons0 = buttons0;
			result.buttons1 = buttons1;
			result.buttons2 = (byte)(buttons2 & 7u);
			return result;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct DualSenseHIDBluetoothInputReport
	{
		public const int ExpectedReportId = 49;

		[FieldOffset(0)]
		public byte reportId;

		[FieldOffset(2)]
		public byte leftStickX;

		[FieldOffset(3)]
		public byte leftStickY;

		[FieldOffset(4)]
		public byte rightStickX;

		[FieldOffset(5)]
		public byte rightStickY;

		[FieldOffset(6)]
		public byte leftTrigger;

		[FieldOffset(7)]
		public byte rightTrigger;

		[FieldOffset(9)]
		public byte buttons0;

		[FieldOffset(10)]
		public byte buttons1;

		[FieldOffset(11)]
		public byte buttons2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DualSenseHIDInputReport ToHIDInputReport()
		{
			DualSenseHIDInputReport result = default(DualSenseHIDInputReport);
			result.leftStickX = leftStickX;
			result.leftStickY = leftStickY;
			result.rightStickX = rightStickX;
			result.rightStickY = rightStickY;
			result.leftTrigger = leftTrigger;
			result.rightTrigger = rightTrigger;
			result.buttons0 = buttons0;
			result.buttons1 = buttons1;
			result.buttons2 = (byte)(buttons2 & 7u);
			return result;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct DualSenseHIDMinimalInputReport
	{
		public static int ExpectedSize1 = 10;

		public static int ExpectedSize2 = 78;

		[FieldOffset(0)]
		public byte reportId;

		[FieldOffset(1)]
		public byte leftStickX;

		[FieldOffset(2)]
		public byte leftStickY;

		[FieldOffset(3)]
		public byte rightStickX;

		[FieldOffset(4)]
		public byte rightStickY;

		[FieldOffset(5)]
		public byte buttons0;

		[FieldOffset(6)]
		public byte buttons1;

		[FieldOffset(7)]
		public byte buttons2;

		[FieldOffset(8)]
		public byte leftTrigger;

		[FieldOffset(9)]
		public byte rightTrigger;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DualSenseHIDInputReport ToHIDInputReport()
		{
			DualSenseHIDInputReport result = default(DualSenseHIDInputReport);
			result.leftStickX = leftStickX;
			result.leftStickY = leftStickY;
			result.rightStickX = rightStickX;
			result.rightStickY = rightStickY;
			result.leftTrigger = leftTrigger;
			result.rightTrigger = rightTrigger;
			result.buttons0 = buttons0;
			result.buttons1 = buttons1;
			result.buttons2 = (byte)(buttons2 & 3u);
			return result;
		}
	}

	private float? m_LowFrequencyMotorSpeed;

	private float? m_HighFrequenceyMotorSpeed;

	protected Color? m_LightBarColor;

	private byte outputSequenceId;

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
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue)
		{
			SetMotorSpeedsAndLightBarColor(0f, 0f, m_LightBarColor);
		}
	}

	public override void ResetHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue)
		{
			m_HighFrequenceyMotorSpeed = null;
			m_LowFrequencyMotorSpeed = null;
			SetMotorSpeedsAndLightBarColor(m_LowFrequencyMotorSpeed, m_HighFrequenceyMotorSpeed, m_LightBarColor);
		}
	}

	public override void ResumeHaptics()
	{
		if (m_LowFrequencyMotorSpeed.HasValue || m_HighFrequenceyMotorSpeed.HasValue)
		{
			SetMotorSpeedsAndLightBarColor(m_LowFrequencyMotorSpeed, m_HighFrequenceyMotorSpeed, m_LightBarColor);
		}
	}

	public override void SetLightBarColor(Color color)
	{
		m_LightBarColor = color;
		SetMotorSpeedsAndLightBarColor(m_LowFrequencyMotorSpeed, m_HighFrequenceyMotorSpeed, m_LightBarColor);
	}

	public override void SetMotorSpeeds(float lowFrequency, float highFrequency)
	{
		m_LowFrequencyMotorSpeed = lowFrequency;
		m_HighFrequenceyMotorSpeed = highFrequency;
		SetMotorSpeedsAndLightBarColor(m_LowFrequencyMotorSpeed, m_HighFrequenceyMotorSpeed, m_LightBarColor);
	}

	public bool SetMotorSpeedsAndLightBarColor(float? lowFrequency, float? highFrequency, Color? color)
	{
		float value = (lowFrequency.HasValue ? lowFrequency.Value : 0f);
		float value2 = (highFrequency.HasValue ? highFrequency.Value : 0f);
		Color color2 = (color.HasValue ? color.Value : Color.black);
		DualSenseHIDOutputReportPayload payload = default(DualSenseHIDOutputReportPayload);
		payload.enableFlags1 = 3;
		payload.enableFlags2 = 4;
		payload.lowFrequencyMotorSpeed = (byte)NumberHelpers.NormalizedFloatToUInt(value, 0u, 255u);
		payload.highFrequencyMotorSpeed = (byte)NumberHelpers.NormalizedFloatToUInt(value2, 0u, 255u);
		payload.redColor = (byte)NumberHelpers.NormalizedFloatToUInt(color2.r, 0u, 255u);
		payload.greenColor = (byte)NumberHelpers.NormalizedFloatToUInt(color2.g, 0u, 255u);
		payload.blueColor = (byte)NumberHelpers.NormalizedFloatToUInt(color2.b, 0u, 255u);
		DualSenseHIDUSBOutputReport command = DualSenseHIDUSBOutputReport.Create(payload, base.hidDescriptor.outputReportSize);
		return ExecuteCommand(ref command) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static bool MergeForward(DualSenseHIDUSBInputReport* currentState, DualSenseHIDUSBInputReport* nextState)
	{
		if (currentState->buttons0 == nextState->buttons0 && currentState->buttons1 == nextState->buttons1)
		{
			return currentState->buttons2 == nextState->buttons2;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static bool MergeForward(DualSenseHIDBluetoothInputReport* currentState, DualSenseHIDBluetoothInputReport* nextState)
	{
		if (currentState->buttons0 == nextState->buttons0 && currentState->buttons1 == nextState->buttons1)
		{
			return currentState->buttons2 == nextState->buttons2;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static bool MergeForward(DualSenseHIDMinimalInputReport* currentState, DualSenseHIDMinimalInputReport* nextState)
	{
		if (currentState->buttons0 == nextState->buttons0 && currentState->buttons1 == nextState->buttons1)
		{
			return currentState->buttons2 == nextState->buttons2;
		}
		return false;
	}

	unsafe bool IEventMerger.MergeForward(InputEventPtr currentEventPtr, InputEventPtr nextEventPtr)
	{
		if (currentEventPtr.type != 1398030676 || nextEventPtr.type != 1398030676)
		{
			return false;
		}
		StateEvent* ptr = StateEvent.FromUnchecked(currentEventPtr);
		StateEvent* ptr2 = StateEvent.FromUnchecked(nextEventPtr);
		if (ptr->stateFormat != DualSenseHIDGenericInputReport.Format || ptr2->stateFormat != DualSenseHIDGenericInputReport.Format)
		{
			return false;
		}
		if (ptr->stateSizeInBytes != ptr2->stateSizeInBytes)
		{
			return false;
		}
		DualSenseHIDGenericInputReport* state = (DualSenseHIDGenericInputReport*)ptr->state;
		DualSenseHIDGenericInputReport* state2 = (DualSenseHIDGenericInputReport*)ptr2->state;
		if (state->reportId != state2->reportId)
		{
			return false;
		}
		if (state->reportId == 1)
		{
			if (ptr->stateSizeInBytes == DualSenseHIDMinimalInputReport.ExpectedSize1 || ptr->stateSizeInBytes == DualSenseHIDMinimalInputReport.ExpectedSize2)
			{
				DualSenseHIDMinimalInputReport* state3 = (DualSenseHIDMinimalInputReport*)ptr->state;
				DualSenseHIDMinimalInputReport* state4 = (DualSenseHIDMinimalInputReport*)ptr2->state;
				return MergeForward(state3, state4);
			}
			DualSenseHIDUSBInputReport* state5 = (DualSenseHIDUSBInputReport*)ptr->state;
			DualSenseHIDUSBInputReport* state6 = (DualSenseHIDUSBInputReport*)ptr2->state;
			return MergeForward(state5, state6);
		}
		if (state->reportId == 49)
		{
			DualSenseHIDBluetoothInputReport* state7 = (DualSenseHIDBluetoothInputReport*)ptr->state;
			DualSenseHIDBluetoothInputReport* state8 = (DualSenseHIDBluetoothInputReport*)ptr2->state;
			return MergeForward(state7, state8);
		}
		return false;
	}

	unsafe bool IEventPreProcessor.PreProcessEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type != 1398030676)
		{
			return eventPtr.type != 1145852993;
		}
		StateEvent* ptr = StateEvent.FromUnchecked(eventPtr);
		if (ptr->stateFormat == DualSenseHIDInputReport.Format)
		{
			return true;
		}
		uint stateSizeInBytes = ptr->stateSizeInBytes;
		if (ptr->stateFormat != DualSenseHIDGenericInputReport.Format || stateSizeInBytes < sizeof(DualSenseHIDInputReport))
		{
			return false;
		}
		DualSenseHIDGenericInputReport* state = (DualSenseHIDGenericInputReport*)ptr->state;
		if (state->reportId == 1)
		{
			if (ptr->stateSizeInBytes == DualSenseHIDMinimalInputReport.ExpectedSize1 || ptr->stateSizeInBytes == DualSenseHIDMinimalInputReport.ExpectedSize2)
			{
				DualSenseHIDInputReport dualSenseHIDInputReport = ((DualSenseHIDMinimalInputReport*)ptr->state)->ToHIDInputReport();
				*(DualSenseHIDInputReport*)ptr->state = dualSenseHIDInputReport;
			}
			else
			{
				DualSenseHIDInputReport dualSenseHIDInputReport2 = ((DualSenseHIDUSBInputReport*)ptr->state)->ToHIDInputReport();
				*(DualSenseHIDInputReport*)ptr->state = dualSenseHIDInputReport2;
			}
			ptr->stateFormat = DualSenseHIDInputReport.Format;
			return true;
		}
		if (state->reportId == 49)
		{
			DualSenseHIDInputReport dualSenseHIDInputReport3 = ((DualSenseHIDBluetoothInputReport*)ptr->state)->ToHIDInputReport();
			*(DualSenseHIDInputReport*)ptr->state = dualSenseHIDInputReport3;
			ptr->stateFormat = DualSenseHIDInputReport.Format;
			return true;
		}
		return false;
	}

	public void OnNextUpdate()
	{
	}

	public unsafe void OnStateEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type == 1398030676 && eventPtr.stateFormat == DualSenseHIDInputReport.Format)
		{
			DualSenseHIDInputReport* ptr = (DualSenseHIDInputReport*)((byte*)base.currentStatePtr + m_StateBlock.byteOffset);
			DualSenseHIDInputReport* state = (DualSenseHIDInputReport*)StateEvent.FromUnchecked(eventPtr)->state;
			if (state->leftStickX >= 120 && state->leftStickX <= 135 && state->leftStickY >= 120 && state->leftStickY <= 135 && state->rightStickX >= 120 && state->rightStickX <= 135 && state->rightStickY >= 120 && state->rightStickY <= 135 && state->leftTrigger == ptr->leftTrigger && state->rightTrigger == ptr->rightTrigger && state->buttons0 == ptr->buttons0 && state->buttons1 == ptr->buttons1 && state->buttons2 == ptr->buttons2)
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
