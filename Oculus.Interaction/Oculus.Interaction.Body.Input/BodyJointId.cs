using UnityEngine;

namespace Oculus.Interaction.Body.Input;

public enum BodyJointId
{
	Invalid = -1,
	[InspectorName("Body Start")]
	Body_Start = 0,
	[InspectorName("Root")]
	Body_Root = 0,
	[InspectorName("Hips")]
	Body_Hips = 1,
	[InspectorName("Spine Lower")]
	Body_SpineLower = 2,
	[InspectorName("Spine Middle")]
	Body_SpineMiddle = 3,
	[InspectorName("Spine Upper")]
	Body_SpineUpper = 4,
	[InspectorName("Chest")]
	Body_Chest = 5,
	[InspectorName("Neck")]
	Body_Neck = 6,
	[InspectorName("Head")]
	Body_Head = 7,
	[InspectorName("Left Arm/Left Shoulder")]
	Body_LeftShoulder = 8,
	[InspectorName("Left Arm/Left Scapula")]
	Body_LeftScapula = 9,
	[InspectorName("Left Arm/Left Arm Upper")]
	Body_LeftArmUpper = 10,
	[InspectorName("Left Arm/Left Arm Lower")]
	Body_LeftArmLower = 11,
	[InspectorName("Left Arm/Left Hand Wrist Twist")]
	Body_LeftHandWristTwist = 12,
	[InspectorName("Right Arm/Right Shoulder")]
	Body_RightShoulder = 13,
	[InspectorName("Right Arm/Right Scapula")]
	Body_RightScapula = 14,
	[InspectorName("Right Arm/Right Arm Upper")]
	Body_RightArmUpper = 15,
	[InspectorName("Right Arm/Right Arm Lower")]
	Body_RightArmLower = 16,
	[InspectorName("Right Arm/Right Hand Wrist Twist")]
	Body_RightHandWristTwist = 17,
	[InspectorName("Left Hand/Left Hand Palm")]
	Body_LeftHandPalm = 18,
	[InspectorName("Left Hand/Left Hand Wrist")]
	Body_LeftHandWrist = 19,
	[InspectorName("Left Hand/Left Hand Thumb Metacarpal")]
	Body_LeftHandThumbMetacarpal = 20,
	[InspectorName("Left Hand/Left Hand Thumb Proximal")]
	Body_LeftHandThumbProximal = 21,
	[InspectorName("Left Hand/Left Hand Thumb Distal")]
	Body_LeftHandThumbDistal = 22,
	[InspectorName("Left Hand/Left Hand Thumb Tip")]
	Body_LeftHandThumbTip = 23,
	[InspectorName("Left Hand/Left Hand Index Metacarpal")]
	Body_LeftHandIndexMetacarpal = 24,
	[InspectorName("Left Hand/Left Hand Index Proximal")]
	Body_LeftHandIndexProximal = 25,
	[InspectorName("Left Hand/Left Hand Index Intermediate")]
	Body_LeftHandIndexIntermediate = 26,
	[InspectorName("Left Hand/Left Hand Index Distal")]
	Body_LeftHandIndexDistal = 27,
	[InspectorName("Left Hand/Left Hand Index Tip")]
	Body_LeftHandIndexTip = 28,
	[InspectorName("Left Hand/Left Hand Middle Metacarpal")]
	Body_LeftHandMiddleMetacarpal = 29,
	[InspectorName("Left Hand/Left Hand Middle Proximal")]
	Body_LeftHandMiddleProximal = 30,
	[InspectorName("Left Hand/Left Hand Middle Intermediate")]
	Body_LeftHandMiddleIntermediate = 31,
	[InspectorName("Left Hand/Left Hand Middle Distal")]
	Body_LeftHandMiddleDistal = 32,
	[InspectorName("Left Hand/Left Hand Middle Tip")]
	Body_LeftHandMiddleTip = 33,
	[InspectorName("Left Hand/Left Hand Ring Metacarpal")]
	Body_LeftHandRingMetacarpal = 34,
	[InspectorName("Left Hand/Left Hand Ring Proximal")]
	Body_LeftHandRingProximal = 35,
	[InspectorName("Left Hand/Left Hand Ring Intermediate")]
	Body_LeftHandRingIntermediate = 36,
	[InspectorName("Left Hand/Left Hand Ring Distal")]
	Body_LeftHandRingDistal = 37,
	[InspectorName("Left Hand/Left Hand Ring Tip")]
	Body_LeftHandRingTip = 38,
	[InspectorName("Left Hand/Left Hand Little Metacarpal")]
	Body_LeftHandLittleMetacarpal = 39,
	[InspectorName("Left Hand/Left Hand Little Proximal")]
	Body_LeftHandLittleProximal = 40,
	[InspectorName("Left Hand/Left Hand Little Intermediate")]
	Body_LeftHandLittleIntermediate = 41,
	[InspectorName("Left Hand/Left Hand Little Distal")]
	Body_LeftHandLittleDistal = 42,
	[InspectorName("Left Hand/Left Hand Little Tip")]
	Body_LeftHandLittleTip = 43,
	[InspectorName("Right Hand/Right Hand Palm")]
	Body_RightHandPalm = 44,
	[InspectorName("Right Hand/Right Hand Wrist")]
	Body_RightHandWrist = 45,
	[InspectorName("Right Hand/Right Hand Thumb Metacarpal")]
	Body_RightHandThumbMetacarpal = 46,
	[InspectorName("Right Hand/Right Hand Thumb Proximal")]
	Body_RightHandThumbProximal = 47,
	[InspectorName("Right Hand/Right Hand Thumb Distal")]
	Body_RightHandThumbDistal = 48,
	[InspectorName("Right Hand/Right Hand Thumb Tip")]
	Body_RightHandThumbTip = 49,
	[InspectorName("Right Hand/Right Hand Index Metacarpal")]
	Body_RightHandIndexMetacarpal = 50,
	[InspectorName("Right Hand/Right Hand Index Proximal")]
	Body_RightHandIndexProximal = 51,
	[InspectorName("Right Hand/Right Hand Index Intermediate")]
	Body_RightHandIndexIntermediate = 52,
	[InspectorName("Right Hand/Right Hand Index Distal")]
	Body_RightHandIndexDistal = 53,
	[InspectorName("Right Hand/Right Hand Index Tip")]
	Body_RightHandIndexTip = 54,
	[InspectorName("Right Hand/Right Hand Middle Metacarpal")]
	Body_RightHandMiddleMetacarpal = 55,
	[InspectorName("Right Hand/Right Hand Middle Proximal")]
	Body_RightHandMiddleProximal = 56,
	[InspectorName("Right Hand/Right Hand Middle Intermediate")]
	Body_RightHandMiddleIntermediate = 57,
	[InspectorName("Right Hand/Right Hand Middle Distal")]
	Body_RightHandMiddleDistal = 58,
	[InspectorName("Right Hand/Right Hand Middle Tip")]
	Body_RightHandMiddleTip = 59,
	[InspectorName("Right Hand/Right Hand Ring Metacarpal")]
	Body_RightHandRingMetacarpal = 60,
	[InspectorName("Right Hand/Right Hand Ring Proximal")]
	Body_RightHandRingProximal = 61,
	[InspectorName("Right Hand/Right Hand Ring Intermediate")]
	Body_RightHandRingIntermediate = 62,
	[InspectorName("Right Hand/Right Hand Ring Distal")]
	Body_RightHandRingDistal = 63,
	[InspectorName("Right Hand/Right Hand Ring Tip")]
	Body_RightHandRingTip = 64,
	[InspectorName("Right Hand/Right Hand Little Metacarpal")]
	Body_RightHandLittleMetacarpal = 65,
	[InspectorName("Right Hand/Right Hand Little Proximal")]
	Body_RightHandLittleProximal = 66,
	[InspectorName("Right Hand/Right Hand Little Intermediate")]
	Body_RightHandLittleIntermediate = 67,
	[InspectorName("Right Hand/Right Hand Little Distal")]
	Body_RightHandLittleDistal = 68,
	[InspectorName("Right Hand/Right Hand Little Tip")]
	Body_RightHandLittleTip = 69,
	[InspectorName("Left Leg/Left Leg Upper")]
	Body_LeftLegUpper = 70,
	[InspectorName("Left Leg/Left Leg Lower")]
	Body_LeftLegLower = 71,
	[InspectorName("Left Foot/Left Foot Ankle Twist")]
	Body_LeftFootAnkleTwist = 72,
	[InspectorName("Left Foot/Left Foot Ankle")]
	Body_LeftFootAnkle = 73,
	[InspectorName("Left Foot/Left Foot Subtalar")]
	Body_LeftFootSubtalar = 74,
	[InspectorName("Left Foot/Left Foot Transverse")]
	Body_LeftFootTransverse = 75,
	[InspectorName("Left Foot/Left Foot Ball")]
	Body_LeftFootBall = 76,
	[InspectorName("Right Leg/Right Leg Upper")]
	Body_RightLegUpper = 77,
	[InspectorName("Right Leg/Right Leg Lower")]
	Body_RightLegLower = 78,
	[InspectorName("Right Foot/Right Foot Ankle Twist")]
	Body_RightFootAnkleTwist = 79,
	[InspectorName("Right Foot/Right Foot Ankle")]
	Body_RightFootAnkle = 80,
	[InspectorName("Right Foot/Right Foot Subtalar")]
	Body_RightFootSubtalar = 81,
	[InspectorName("Right Foot/Right Foot Transverse")]
	Body_RightFootTransverse = 82,
	[InspectorName("Right Foot/Right Foot Ball")]
	Body_RightFootBall = 83,
	[InspectorName("Body End")]
	Body_End = 84
}
