using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[StructLayout(LayoutKind.Explicit, Size = 117)]
public struct XRSimulatedHMDState : IInputStateTypeInfo
{
	[FieldOffset(0)]
	[InputControl(usage = "LeftEyePosition", offset = 0u)]
	public Vector3 leftEyePosition;

	[FieldOffset(12)]
	[InputControl(usage = "LeftEyeRotation", offset = 12u)]
	public Quaternion leftEyeRotation;

	[FieldOffset(28)]
	[InputControl(usage = "RightEyePosition", offset = 28u)]
	public Vector3 rightEyePosition;

	[FieldOffset(40)]
	[InputControl(usage = "RightEyeRotation", offset = 40u)]
	public Quaternion rightEyeRotation;

	[FieldOffset(56)]
	[InputControl(usage = "CenterEyePosition", offset = 56u)]
	public Vector3 centerEyePosition;

	[FieldOffset(68)]
	[InputControl(usage = "CenterEyeRotation", offset = 68u)]
	public Quaternion centerEyeRotation;

	[FieldOffset(84)]
	[InputControl(usage = "TrackingState", layout = "Integer", offset = 84u)]
	public int trackingState;

	[FieldOffset(88)]
	[InputControl(usage = "IsTracked", layout = "Button", offset = 88u)]
	public bool isTracked;

	[FieldOffset(89)]
	[InputControl(usage = "DevicePosition", offset = 89u)]
	public Vector3 devicePosition;

	[FieldOffset(101)]
	[InputControl(usage = "DeviceRotation", offset = 101u)]
	public Quaternion deviceRotation;

	public static FourCC formatId => new FourCC('X', 'R', 'S', 'H');

	public FourCC format => formatId;

	public void Reset()
	{
		leftEyePosition = default(Vector3);
		leftEyeRotation = Quaternion.identity;
		rightEyePosition = default(Vector3);
		rightEyeRotation = Quaternion.identity;
		centerEyePosition = default(Vector3);
		centerEyeRotation = Quaternion.identity;
		trackingState = 0;
		isTracked = false;
		devicePosition = default(Vector3);
		deviceRotation = Quaternion.identity;
	}
}
