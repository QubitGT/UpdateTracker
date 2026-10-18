using System;

namespace Oculus.Interaction.Input.Compatibility.OVR;

[Flags]
public enum HandFingerJointFlags
{
	None = 0,
	Wrist = 1,
	ForearmStub = 2,
	Thumb0 = 4,
	Thumb1 = 8,
	Thumb2 = 0x10,
	Thumb3 = 0x20,
	Index1 = 0x40,
	Index2 = 0x80,
	Index3 = 0x100,
	Middle1 = 0x200,
	Middle2 = 0x400,
	Middle3 = 0x800,
	Ring1 = 0x1000,
	Ring2 = 0x2000,
	Ring3 = 0x4000,
	Pinky0 = 0x8000,
	Pinky1 = 0x10000,
	Pinky2 = 0x20000,
	Pinky3 = 0x40000,
	HandMaxSkinnable = 0x80000,
	ThumbTip = 0x80000,
	IndexTip = 0x100000,
	MiddleTip = 0x200000,
	RingTip = 0x400000,
	PinkyTip = 0x800000,
	All = 0xFFFFFF
}
