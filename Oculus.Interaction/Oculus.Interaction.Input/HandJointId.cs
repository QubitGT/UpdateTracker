using UnityEngine;

namespace Oculus.Interaction.Input;

public enum HandJointId
{
	Invalid = -1,
	HandStart = 0,
	[Tooltip("Palm")]
	HandPalm = 0,
	[Tooltip("Wrist Joint")]
	HandWristRoot = 1,
	[Tooltip("Thumb Metacarpal Joint")]
	HandThumb1 = 2,
	[Tooltip("Thumb Proximal Joint")]
	HandThumb2 = 3,
	[Tooltip("Thumb Distal Joint")]
	HandThumb3 = 4,
	[Tooltip("Thumb Tip")]
	HandThumbTip = 5,
	[Tooltip("Index Finger Metacarpal Joint")]
	HandIndex0 = 6,
	[Tooltip("Index Finger Proximal Joint")]
	HandIndex1 = 7,
	[Tooltip("Index Finger Intermediate Joint")]
	HandIndex2 = 8,
	[Tooltip("Index Finger Distal Joint")]
	HandIndex3 = 9,
	[Tooltip("Index Finger Tip")]
	HandIndexTip = 10,
	[Tooltip("Middle Finger Metacarpal Joint")]
	HandMiddle0 = 11,
	[Tooltip("Middle Finger Proximal Joint")]
	HandMiddle1 = 12,
	[Tooltip("Middle Finger Intermediate Joint")]
	HandMiddle2 = 13,
	[Tooltip("Middle Finger Distal Joint")]
	HandMiddle3 = 14,
	[Tooltip("Middle Finger Tip")]
	HandMiddleTip = 15,
	[Tooltip("Ring Finger Metacarpal Joint")]
	HandRing0 = 16,
	[Tooltip("Ring Finger Proximal Joint")]
	HandRing1 = 17,
	[Tooltip("Ring Finger Intermediate Joint")]
	HandRing2 = 18,
	[Tooltip("Ring Finger Distal Joint")]
	HandRing3 = 19,
	[Tooltip("Ring Finger Tip")]
	HandRingTip = 20,
	[Tooltip("Pinky Finger Metacarpal Joint")]
	HandPinky0 = 21,
	[Tooltip("Pinky Finger Proximal Joint")]
	HandPinky1 = 22,
	[Tooltip("Pinky Finger Intermediate Joint")]
	HandPinky2 = 23,
	[Tooltip("Pinky Finger Distal Joint")]
	HandPinky3 = 24,
	[Tooltip("Pinky Finger Tip")]
	HandPinkyTip = 25,
	HandEnd = 26,
	HandMaxSkinnable = 26
}
