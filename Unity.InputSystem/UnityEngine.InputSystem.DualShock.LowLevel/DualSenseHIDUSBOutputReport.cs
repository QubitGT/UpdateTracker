using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.DualShock.LowLevel;

[StructLayout(LayoutKind.Explicit, Size = 56)]
internal struct DualSenseHIDUSBOutputReport : IInputDeviceCommandInfo
{
	internal const int kSize = 56;

	[FieldOffset(0)]
	public InputDeviceCommand baseCommand;

	[FieldOffset(8)]
	public byte reportId;

	[FieldOffset(9)]
	public DualSenseHIDOutputReportPayload payload;

	public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

	public FourCC typeStatic => Type;

	public static DualSenseHIDUSBOutputReport Create(DualSenseHIDOutputReportPayload payload, int outputReportSize)
	{
		DualSenseHIDUSBOutputReport result = default(DualSenseHIDUSBOutputReport);
		result.baseCommand = new InputDeviceCommand(Type, 8 + outputReportSize);
		result.reportId = 2;
		result.payload = payload;
		return result;
	}
}
