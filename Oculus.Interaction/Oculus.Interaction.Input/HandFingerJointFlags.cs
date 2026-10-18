using System;

namespace Oculus.Interaction.Input;

[Flags]
public enum HandFingerJointFlags
{
	None = 0,
	Palm = 1,
	Wrist = 2,
	Thumb1 = 4,
	Thumb2 = 8,
	Thumb3 = 0x10,
	ThumbTip = 0x20,
	Index0 = 0x40,
	Index1 = 0x80,
	Index2 = 0x100,
	Index3 = 0x200,
	IndexTip = 0x400,
	Middle0 = 0x800,
	Middle1 = 0x1000,
	Middle2 = 0x2000,
	Middle3 = 0x4000,
	MiddleTip = 0x8000,
	Ring0 = 0x10000,
	Ring1 = 0x20000,
	Ring2 = 0x40000,
	Ring3 = 0x80000,
	RingTip = 0x100000,
	Pinky0 = 0x200000,
	Pinky1 = 0x400000,
	Pinky2 = 0x800000,
	Pinky3 = 0x1000000,
	PinkyTip = 0x2000000,
	HandMaxSkinnable = 0x4000000,
	All = 0x3FFFFFF
}
