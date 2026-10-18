using System;

namespace Oculus.Interaction.Input;

public class FingersMetadata
{
	public static readonly HandJointId[] HAND_JOINT_IDS = new HandJointId[19]
	{
		HandJointId.HandThumb1,
		HandJointId.HandThumb2,
		HandJointId.HandThumb3,
		HandJointId.HandIndex0,
		HandJointId.HandIndex1,
		HandJointId.HandIndex2,
		HandJointId.HandIndex3,
		HandJointId.HandMiddle0,
		HandJointId.HandMiddle1,
		HandJointId.HandMiddle2,
		HandJointId.HandMiddle3,
		HandJointId.HandRing0,
		HandJointId.HandRing1,
		HandJointId.HandRing2,
		HandJointId.HandRing3,
		HandJointId.HandPinky0,
		HandJointId.HandPinky1,
		HandJointId.HandPinky2,
		HandJointId.HandPinky3
	};

	public static readonly HandJointId[][] FINGER_TO_JOINTS = InitializeFingerToJoint();

	public static readonly int[][] FINGER_TO_JOINT_INDEX = InitializeFingerToJointIndex();

	public static readonly bool[] HAND_JOINT_CAN_SPREAD = InitializeCanSpread();

	public static readonly bool[] HAND_JOINT_CAN_MOVE = InitializeCanMove();

	public static readonly int[] JOINT_TO_FINGER_INDEX = InitializeJointToFingerIndex();

	[Obsolete("Use JointToFingerList instead")]
	public static readonly HandFinger[] JOINT_TO_FINGER = null;

	private static int[] JOINT_TO_INDEX = InitializeHandJointIdToIndex();

	public static JointFreedom[] DefaultFingersFreedom()
	{
		return new JointFreedom[5]
		{
			JointFreedom.Locked,
			JointFreedom.Locked,
			JointFreedom.Constrained,
			JointFreedom.Constrained,
			JointFreedom.Free
		};
	}

	public static int HandJointIdToIndex(HandJointId id)
	{
		return JOINT_TO_INDEX[(int)id];
	}

	private static int[] InitializeHandJointIdToIndex()
	{
		int[] array = new int[26];
		HandJointId jointId;
		for (jointId = HandJointId.HandStart; jointId < HandJointId.HandEnd; jointId++)
		{
			array[(int)jointId] = Array.FindIndex(HAND_JOINT_IDS, (HandJointId joint) => joint == jointId);
		}
		return array;
	}

	private static HandJointId[][] InitializeFingerToJoint()
	{
		HandJointId[][] array = new HandJointId[HandJointUtils.FingerToJointList.Count][];
		for (int i = 0; i < HandJointUtils.FingerToJointList.Count; i++)
		{
			int num = HandJointUtils.FingerToJointList[i].Length - 1;
			array[i] = new HandJointId[num];
			Array.Copy(HandJointUtils.FingerToJointList[i], array[i], num);
		}
		return array;
	}

	private static int[][] InitializeFingerToJointIndex()
	{
		HandJointId[][] array = InitializeFingerToJoint();
		int[] array2 = InitializeHandJointIdToIndex();
		int[][] array3 = new int[array.Length][];
		for (int i = 0; i < array.Length; i++)
		{
			int[] array4 = new int[array[i].Length];
			for (int j = 0; j < array[i].Length; j++)
			{
				array4[j] = array2[(int)array[i][j]];
			}
			array3[i] = array4;
		}
		return array3;
	}

	private static int[] InitializeJointToFingerIndex()
	{
		int[] array = new int[26];
		for (HandJointId handJointId = HandJointId.HandStart; handJointId < HandJointId.HandEnd; handJointId++)
		{
			int num = -1;
			HandJointId handJointId2 = handJointId;
			while (HandJointUtils.JointToFingerList[(int)handJointId2] != HandFinger.Invalid)
			{
				num++;
				handJointId2 = HandJointUtils.JointParentList[(int)handJointId2];
			}
			array[(int)handJointId] = num;
		}
		return array;
	}

	private static bool[] InitializeCanSpread()
	{
		int[] array = InitializeJointToFingerIndex();
		bool[] array2 = new bool[HAND_JOINT_IDS.Length];
		for (int i = 0; i < HAND_JOINT_IDS.Length; i++)
		{
			HandJointId handJointId = HAND_JOINT_IDS[i];
			int num = array[(int)handJointId];
			array2[i] = num <= 1 && handJointId != HandJointId.HandThumb2;
		}
		return array2;
	}

	private static bool[] InitializeCanMove()
	{
		bool[] array = new bool[HAND_JOINT_IDS.Length];
		for (int i = 0; i < HAND_JOINT_IDS.Length; i++)
		{
			HandJointId handJointId = HAND_JOINT_IDS[i];
			array[i] = handJointId != HandJointId.HandIndex0 && handJointId != HandJointId.HandMiddle0 && handJointId != HandJointId.HandRing0;
		}
		return array;
	}
}
