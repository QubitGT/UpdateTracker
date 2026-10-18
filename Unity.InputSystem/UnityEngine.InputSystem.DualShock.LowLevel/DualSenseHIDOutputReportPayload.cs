using System.Runtime.InteropServices;

namespace UnityEngine.InputSystem.DualShock.LowLevel;

[StructLayout(LayoutKind.Explicit, Size = 47)]
internal struct DualSenseHIDOutputReportPayload
{
	[FieldOffset(0)]
	public byte enableFlags1;

	[FieldOffset(1)]
	public byte enableFlags2;

	[FieldOffset(2)]
	public byte highFrequencyMotorSpeed;

	[FieldOffset(3)]
	public byte lowFrequencyMotorSpeed;

	[FieldOffset(44)]
	public byte redColor;

	[FieldOffset(45)]
	public byte greenColor;

	[FieldOffset(46)]
	public byte blueColor;
}
