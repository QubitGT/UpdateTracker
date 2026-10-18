using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Switch.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.Switch;

[InputControlLayout(stateType = typeof(SwitchProControllerHIDInputState), displayName = "Switch Pro Controller")]
public class SwitchProControllerHID : Gamepad, IInputStateCallbackReceiver, IEventPreProcessor
{
	[StructLayout(LayoutKind.Explicit, Size = 7)]
	private struct SwitchInputOnlyReport
	{
		public const int kSize = 7;

		[FieldOffset(0)]
		public byte buttons0;

		[FieldOffset(1)]
		public byte buttons1;

		[FieldOffset(2)]
		public byte hat;

		[FieldOffset(3)]
		public byte leftX;

		[FieldOffset(4)]
		public byte leftY;

		[FieldOffset(5)]
		public byte rightX;

		[FieldOffset(6)]
		public byte rightY;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SwitchProControllerHIDInputState ToHIDInputReport()
		{
			SwitchProControllerHIDInputState switchProControllerHIDInputState = default(SwitchProControllerHIDInputState);
			switchProControllerHIDInputState.leftStickX = leftX;
			switchProControllerHIDInputState.leftStickY = leftY;
			switchProControllerHIDInputState.rightStickX = rightX;
			switchProControllerHIDInputState.rightStickY = rightY;
			SwitchProControllerHIDInputState result = switchProControllerHIDInputState;
			result.Set(SwitchProControllerHIDInputState.Button.West, (buttons0 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.South, (buttons0 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.East, (buttons0 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.North, (buttons0 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.L, (buttons0 & 0x10) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.R, (buttons0 & 0x20) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.ZL, (buttons0 & 0x40) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.ZR, (buttons0 & 0x80) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Minus, (buttons1 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Plus, (buttons1 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.StickL, (buttons1 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.StickR, (buttons1 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Home, (buttons1 & 0x10) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Capture, (buttons1 & 0x20) != 0);
			bool state = false;
			bool state2 = false;
			bool state3 = false;
			bool state4 = false;
			switch (hat)
			{
			case 0:
				state2 = true;
				break;
			case 1:
				state2 = true;
				state3 = true;
				break;
			case 2:
				state3 = true;
				break;
			case 3:
				state4 = true;
				state3 = true;
				break;
			case 4:
				state4 = true;
				break;
			case 5:
				state4 = true;
				state = true;
				break;
			case 6:
				state = true;
				break;
			case 7:
				state2 = true;
				state = true;
				break;
			}
			result.Set(SwitchProControllerHIDInputState.Button.Left, state);
			result.Set(SwitchProControllerHIDInputState.Button.Up, state2);
			result.Set(SwitchProControllerHIDInputState.Button.Right, state3);
			result.Set(SwitchProControllerHIDInputState.Button.Down, state4);
			return result;
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 12)]
	private struct SwitchSimpleInputReport
	{
		public const int kSize = 12;

		public const byte ExpectedReportId = 63;

		[FieldOffset(0)]
		public byte reportId;

		[FieldOffset(1)]
		public byte buttons0;

		[FieldOffset(2)]
		public byte buttons1;

		[FieldOffset(3)]
		public byte hat;

		[FieldOffset(4)]
		public ushort leftX;

		[FieldOffset(6)]
		public ushort leftY;

		[FieldOffset(8)]
		public ushort rightX;

		[FieldOffset(10)]
		public ushort rightY;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SwitchProControllerHIDInputState ToHIDInputReport()
		{
			byte leftStickX = (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(leftX, 16u, 8u);
			byte leftStickY = (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(leftY, 16u, 8u);
			byte rightStickX = (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(rightX, 16u, 8u);
			byte rightStickY = (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(rightY, 16u, 8u);
			SwitchProControllerHIDInputState switchProControllerHIDInputState = default(SwitchProControllerHIDInputState);
			switchProControllerHIDInputState.leftStickX = leftStickX;
			switchProControllerHIDInputState.leftStickY = leftStickY;
			switchProControllerHIDInputState.rightStickX = rightStickX;
			switchProControllerHIDInputState.rightStickY = rightStickY;
			SwitchProControllerHIDInputState result = switchProControllerHIDInputState;
			result.Set(SwitchProControllerHIDInputState.Button.South, (buttons0 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.East, (buttons0 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.West, (buttons0 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.North, (buttons0 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.L, (buttons0 & 0x10) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.R, (buttons0 & 0x20) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.ZL, (buttons0 & 0x40) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.ZR, (buttons0 & 0x80) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Minus, (buttons1 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Plus, (buttons1 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.StickL, (buttons1 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.StickR, (buttons1 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Home, (buttons1 & 0x10) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Capture, (buttons1 & 0x20) != 0);
			bool state = false;
			bool state2 = false;
			bool state3 = false;
			bool state4 = false;
			switch (hat)
			{
			case 0:
				state2 = true;
				break;
			case 1:
				state2 = true;
				state3 = true;
				break;
			case 2:
				state3 = true;
				break;
			case 3:
				state4 = true;
				state3 = true;
				break;
			case 4:
				state4 = true;
				break;
			case 5:
				state4 = true;
				state = true;
				break;
			case 6:
				state = true;
				break;
			case 7:
				state2 = true;
				state = true;
				break;
			}
			result.Set(SwitchProControllerHIDInputState.Button.Left, state);
			result.Set(SwitchProControllerHIDInputState.Button.Up, state2);
			result.Set(SwitchProControllerHIDInputState.Button.Right, state3);
			result.Set(SwitchProControllerHIDInputState.Button.Down, state4);
			return result;
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 25)]
	private struct SwitchFullInputReport
	{
		public const int kSize = 25;

		public const byte ExpectedReportId = 48;

		[FieldOffset(0)]
		public byte reportId;

		[FieldOffset(3)]
		public byte buttons0;

		[FieldOffset(4)]
		public byte buttons1;

		[FieldOffset(5)]
		public byte buttons2;

		[FieldOffset(6)]
		public byte left0;

		[FieldOffset(7)]
		public byte left1;

		[FieldOffset(8)]
		public byte left2;

		[FieldOffset(9)]
		public byte right0;

		[FieldOffset(10)]
		public byte right1;

		[FieldOffset(11)]
		public byte right2;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SwitchProControllerHIDInputState ToHIDInputReport()
		{
			uint value = (uint)(left0 | ((left1 & 0xF) << 8));
			uint value2 = (uint)(((left1 & 0xF0) >> 4) | (left2 << 4));
			int value3 = right0 | ((right1 & 0xF) << 8);
			uint value4 = (uint)(((right1 & 0xF0) >> 4) | (right2 << 4));
			byte leftStickX = (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(value, 12u, 8u);
			byte leftStickY = (byte)(255 - (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(value2, 12u, 8u));
			byte rightStickX = (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits((uint)value3, 12u, 8u);
			byte rightStickY = (byte)(255 - (byte)NumberHelpers.RemapUIntBitsToNormalizeFloatToUIntBits(value4, 12u, 8u));
			SwitchProControllerHIDInputState switchProControllerHIDInputState = default(SwitchProControllerHIDInputState);
			switchProControllerHIDInputState.leftStickX = leftStickX;
			switchProControllerHIDInputState.leftStickY = leftStickY;
			switchProControllerHIDInputState.rightStickX = rightStickX;
			switchProControllerHIDInputState.rightStickY = rightStickY;
			SwitchProControllerHIDInputState result = switchProControllerHIDInputState;
			result.Set(SwitchProControllerHIDInputState.Button.West, (buttons0 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.North, (buttons0 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.South, (buttons0 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.East, (buttons0 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.R, (buttons0 & 0x40) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.ZR, (buttons0 & 0x80) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Minus, (buttons1 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Plus, (buttons1 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.StickR, (buttons1 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.StickL, (buttons1 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Home, (buttons1 & 0x10) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Capture, (buttons1 & 0x20) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Down, (buttons2 & 1) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Up, (buttons2 & 2) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Right, (buttons2 & 4) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.Left, (buttons2 & 8) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.L, (buttons2 & 0x40) != 0);
			result.Set(SwitchProControllerHIDInputState.Button.ZL, (buttons2 & 0x80) != 0);
			return result;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct SwitchHIDGenericInputReport
	{
		[FieldOffset(0)]
		public byte reportId;

		public static FourCC Format => new FourCC('H', 'I', 'D');
	}

	[StructLayout(LayoutKind.Explicit, Size = 49)]
	internal struct SwitchMagicOutputReport
	{
		internal enum ReportType
		{
			Magic = 0x80
		}

		public enum CommandIdType
		{
			Status = 1,
			Handshake,
			Highspeed,
			ForceUSB
		}

		public const int kSize = 49;

		public const byte ExpectedReplyInputReportId = 129;

		[FieldOffset(0)]
		public byte reportType;

		[FieldOffset(1)]
		public byte commandId;
	}

	[StructLayout(LayoutKind.Explicit, Size = 57)]
	internal struct SwitchMagicOutputHIDBluetooth : IInputDeviceCommandInfo
	{
		public const int kSize = 57;

		[FieldOffset(0)]
		public InputDeviceCommand baseCommand;

		[FieldOffset(8)]
		public SwitchMagicOutputReport report;

		public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

		public FourCC typeStatic => Type;

		public static SwitchMagicOutputHIDBluetooth Create(SwitchMagicOutputReport.CommandIdType type)
		{
			SwitchMagicOutputHIDBluetooth result = default(SwitchMagicOutputHIDBluetooth);
			result.baseCommand = new InputDeviceCommand(Type, 57);
			result.report = new SwitchMagicOutputReport
			{
				reportType = 128,
				commandId = (byte)type
			};
			return result;
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 72)]
	internal struct SwitchMagicOutputHIDUSB : IInputDeviceCommandInfo
	{
		public const int kSize = 72;

		[FieldOffset(0)]
		public InputDeviceCommand baseCommand;

		[FieldOffset(8)]
		public SwitchMagicOutputReport report;

		public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

		public FourCC typeStatic => Type;

		public static SwitchMagicOutputHIDUSB Create(SwitchMagicOutputReport.CommandIdType type)
		{
			SwitchMagicOutputHIDUSB result = default(SwitchMagicOutputHIDUSB);
			result.baseCommand = new InputDeviceCommand(Type, 72);
			result.report = new SwitchMagicOutputReport
			{
				reportType = 128,
				commandId = (byte)type
			};
			return result;
		}
	}

	private static readonly SwitchMagicOutputReport.CommandIdType[] s_HandshakeSequence = new SwitchMagicOutputReport.CommandIdType[5]
	{
		SwitchMagicOutputReport.CommandIdType.Status,
		SwitchMagicOutputReport.CommandIdType.Handshake,
		SwitchMagicOutputReport.CommandIdType.Highspeed,
		SwitchMagicOutputReport.CommandIdType.Handshake,
		SwitchMagicOutputReport.CommandIdType.ForceUSB
	};

	private int m_HandshakeStepIndex;

	private double m_HandshakeTimer;

	internal const byte JitterMaskLow = 120;

	internal const byte JitterMaskHigh = 135;

	[InputControl(name = "capture", displayName = "Capture")]
	public ButtonControl captureButton { get; protected set; }

	[InputControl(name = "home", displayName = "Home")]
	public ButtonControl homeButton { get; protected set; }

	protected override void OnAdded()
	{
		base.OnAdded();
		captureButton = GetChildControl<ButtonControl>("capture");
		homeButton = GetChildControl<ButtonControl>("home");
		HandshakeRestart();
	}

	private void HandshakeRestart()
	{
		m_HandshakeStepIndex = -1;
		m_HandshakeTimer = InputRuntime.s_Instance.currentTime;
	}

	private void HandshakeTick()
	{
		double currentTime = InputRuntime.s_Instance.currentTime;
		if (currentTime >= m_LastUpdateTimeInternal + 2.0 && currentTime >= m_HandshakeTimer + 2.0)
		{
			m_HandshakeStepIndex = 0;
		}
		else
		{
			if (m_HandshakeStepIndex + 1 >= s_HandshakeSequence.Length || !(currentTime > m_HandshakeTimer + 0.1))
			{
				return;
			}
			m_HandshakeStepIndex++;
		}
		m_HandshakeTimer = currentTime;
		SwitchMagicOutputReport.CommandIdType type = s_HandshakeSequence[m_HandshakeStepIndex];
		SwitchMagicOutputHIDBluetooth command = SwitchMagicOutputHIDBluetooth.Create(type);
		if (ExecuteCommand(ref command) <= 0)
		{
			SwitchMagicOutputHIDUSB command2 = SwitchMagicOutputHIDUSB.Create(type);
			ExecuteCommand(ref command2);
		}
	}

	public void OnNextUpdate()
	{
		HandshakeTick();
	}

	public unsafe void OnStateEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type == 1398030676 && eventPtr.stateFormat == SwitchProControllerHIDInputState.Format)
		{
			SwitchProControllerHIDInputState* ptr = (SwitchProControllerHIDInputState*)((byte*)base.currentStatePtr + m_StateBlock.byteOffset);
			SwitchProControllerHIDInputState* state = (SwitchProControllerHIDInputState*)StateEvent.FromUnchecked(eventPtr)->state;
			if (state->leftStickX >= 120 && state->leftStickX <= 135 && state->leftStickY >= 120 && state->leftStickY <= 135 && state->rightStickX >= 120 && state->rightStickX <= 135 && state->rightStickY >= 120 && state->rightStickY <= 135 && state->buttons1 == ptr->buttons1 && state->buttons2 == ptr->buttons2)
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

	public unsafe bool PreProcessEvent(InputEventPtr eventPtr)
	{
		if (eventPtr.type == 1145852993)
		{
			return DeltaStateEvent.FromUnchecked(eventPtr)->stateFormat == SwitchProControllerHIDInputState.Format;
		}
		if (eventPtr.type != 1398030676)
		{
			return true;
		}
		StateEvent* ptr = StateEvent.FromUnchecked(eventPtr);
		uint stateSizeInBytes = ptr->stateSizeInBytes;
		if (ptr->stateFormat == SwitchProControllerHIDInputState.Format)
		{
			return true;
		}
		if (ptr->stateFormat != SwitchHIDGenericInputReport.Format || stateSizeInBytes < sizeof(SwitchHIDGenericInputReport))
		{
			return false;
		}
		SwitchHIDGenericInputReport* state = (SwitchHIDGenericInputReport*)ptr->state;
		if (state->reportId == 63 && stateSizeInBytes >= 12)
		{
			SwitchProControllerHIDInputState switchProControllerHIDInputState = ((SwitchSimpleInputReport*)ptr->state)->ToHIDInputReport();
			*(SwitchProControllerHIDInputState*)ptr->state = switchProControllerHIDInputState;
			ptr->stateFormat = SwitchProControllerHIDInputState.Format;
			return true;
		}
		if (state->reportId == 48 && stateSizeInBytes >= 25)
		{
			SwitchProControllerHIDInputState switchProControllerHIDInputState2 = ((SwitchFullInputReport*)ptr->state)->ToHIDInputReport();
			*(SwitchProControllerHIDInputState*)ptr->state = switchProControllerHIDInputState2;
			ptr->stateFormat = SwitchProControllerHIDInputState.Format;
			return true;
		}
		if (stateSizeInBytes == 8 || stateSizeInBytes == 9)
		{
			int num = ((stateSizeInBytes == 9) ? 1 : 0);
			SwitchProControllerHIDInputState switchProControllerHIDInputState3 = ((SwitchInputOnlyReport*)((byte*)ptr->state + num))->ToHIDInputReport();
			*(SwitchProControllerHIDInputState*)ptr->state = switchProControllerHIDInputState3;
			ptr->stateFormat = SwitchProControllerHIDInputState.Format;
			return true;
		}
		return false;
	}
}
