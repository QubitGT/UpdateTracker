using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.DualShock.LowLevel;

[StructLayout(LayoutKind.Explicit, Size = 86)]
internal struct DualSenseHIDBluetoothOutputReport : IInputDeviceCommandInfo
{
	internal const int kSize = 86;

	[FieldOffset(0)]
	public InputDeviceCommand baseCommand;

	[FieldOffset(8)]
	public byte reportId;

	[FieldOffset(9)]
	public byte tag1;

	[FieldOffset(10)]
	public byte tag2;

	[FieldOffset(11)]
	public DualSenseHIDOutputReportPayload payload;

	[FieldOffset(82)]
	public uint crc32;

	[FieldOffset(8)]
	public unsafe fixed byte rawData[74];

	public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

	public FourCC typeStatic => Type;

	public static DualSenseHIDBluetoothOutputReport Create(DualSenseHIDOutputReportPayload payload, byte outputSequenceId, int outputReportSize)
	{
		DualSenseHIDBluetoothOutputReport result = default(DualSenseHIDBluetoothOutputReport);
		result.baseCommand = new InputDeviceCommand(Type, 8 + outputReportSize);
		result.reportId = 49;
		result.tag1 = (byte)((outputSequenceId & 0xF) << 4);
		result.tag2 = 16;
		result.payload = payload;
		return result;
	}
}
