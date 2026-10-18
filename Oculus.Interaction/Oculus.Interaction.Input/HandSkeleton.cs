using UnityEngine;

namespace Oculus.Interaction.Input;

public class HandSkeleton : IReadOnlyHandSkeleton, IReadOnlyHandSkeletonJointList
{
	public HandSkeletonJoint[] joints = new HandSkeletonJoint[26];

	public static readonly HandSkeleton DefaultLeftSkeleton = new HandSkeleton
	{
		joints = new HandSkeletonJoint[26]
		{
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(0.000863f, -0.001272f, 0.047823f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = -1,
				pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(0.030218f, -0.016084f, 0.034498f), new Quaternion(-0.06665304f, 0.3969201f, -0.5750258f, 0.71229f))
			},
			new HandSkeletonJoint
			{
				parent = 2,
				pose = new Pose(new Vector3(-1.152053E-06f, -4.860463E-06f, 0.03251399f), new Quaternion(0.2201137f, -0.05016914f, 0.08162888f, 0.9707573f))
			},
			new HandSkeletonJoint
			{
				parent = 3,
				pose = new Pose(new Vector3(-1.023574E-06f, -1.828242E-06f, 0.03379434f), new Quaternion(0.1129198f, 0.05065549f, -0.0791567f, 0.9891499f))
			},
			new HandSkeletonJoint
			{
				parent = 4,
				pose = new Pose(new Vector3(-0.0006706798f, 0.001025644f, 0.02459195f), new Quaternion(-9.62965E-35f, -2.775558E-17f, -3.469447E-18f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(0.019819f, -0.009505f, 0.036448f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 6,
				pose = new Pose(new Vector3(0.003732f, 0.002189f, 0.059548f), new Quaternion(0.151882f, 0.07698268f, -0.0411778f, 0.9845354f))
			},
			new HandSkeletonJoint
			{
				parent = 7,
				pose = new Pose(new Vector3(1.45847E-06f, -1.820559E-06f, 0.03792747f), new Quaternion(0.1307591f, -0.003759917f, 0.02628858f, 0.9910585f))
			},
			new HandSkeletonJoint
			{
				parent = 8,
				pose = new Pose(new Vector3(9.285007E-07f, 1.593788E-07f, 0.02430516f), new Quaternion(-0.003017978f, -0.02607772f, 0.0164322f, 0.9995203f))
			},
			new HandSkeletonJoint
			{
				parent = 9,
				pose = new Pose(new Vector3(-0.0002949532f, 0.001025431f, 0.02236465f), new Quaternion(0f, 4.336809E-19f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(0.00361f, -0.007648f, 0.034286f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 11,
				pose = new Pose(new Vector3(-0.001884f, 0.005105f, 0.061361f), new Quaternion(0.1896454f, -0.0117154f, 0.009750124f, 0.9817344f))
			},
			new HandSkeletonJoint
			{
				parent = 12,
				pose = new Pose(new Vector3(-3.311234E-07f, -8.804644E-07f, 0.04292654f), new Quaternion(0.2042747f, -0.001967482f, 0.0123084f, 0.9788343f))
			},
			new HandSkeletonJoint
			{
				parent = 13,
				pose = new Pose(new Vector3(1.703013E-07f, 5.871987E-07f, 0.02754843f), new Quaternion(-0.03223448f, -0.001938704f, 0.040453f, 0.9986595f))
			},
			new HandSkeletonJoint
			{
				parent = 14,
				pose = new Pose(new Vector3(-0.0003095091f, 0.001137151f, 0.02496384f), new Quaternion(2.775558E-17f, 3.469447E-18f, 6.938894E-18f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(-0.014992f, -0.006016f, 0.034776f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 16,
				pose = new Pose(new Vector3(-0.002473f, -0.000513f, 0.053918f), new Quaternion(0.08123156f, -0.08615339f, 0.05587993f, 0.9913912f))
			},
			new HandSkeletonJoint
			{
				parent = 17,
				pose = new Pose(new Vector3(5.266108E-07f, 3.652638E-07f, 0.03899503f), new Quaternion(0.3017412f, 0.007293773f, 0.03955524f, 0.9525411f))
			},
			new HandSkeletonJoint
			{
				parent = 18,
				pose = new Pose(new Vector3(-1.039646E-06f, -5.423256E-07f, 0.02657356f), new Quaternion(0.09175414f, 0.02957179f, 0.008965106f, 0.9953021f))
			},
			new HandSkeletonJoint
			{
				parent = 19,
				pose = new Pose(new Vector3(-0.0002563861f, 0.001608112f, 0.02432607f), new Quaternion(-2.942483E-05f, -2.775558E-17f, -8.16703E-22f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(-0.022999f, -0.009419999f, 0.034074f), new Quaternion(0.01833355f, -0.1403366f, 0.2070356f, 0.9680423f))
			},
			new HandSkeletonJoint
			{
				parent = 21,
				pose = new Pose(new Vector3(2.530307E-06f, 1.160611E-06f, 0.04565198f), new Quaternion(-0.06267674f, -0.05101484f, -0.09903724f, 0.9917967f))
			},
			new HandSkeletonJoint
			{
				parent = 22,
				pose = new Pose(new Vector3(1.720091E-07f, -6.646861E-07f, 0.03071994f), new Quaternion(0.3602582f, -0.025497f, 0.06776039f, 0.930039f))
			},
			new HandSkeletonJoint
			{
				parent = 23,
				pose = new Pose(new Vector3(1.354028E-07f, 6.386686E-07f, 0.02031132f), new Quaternion(0.1151107f, 0.04873112f, -0.001109484f, 0.992156f))
			},
			new HandSkeletonJoint
			{
				parent = 24,
				pose = new Pose(new Vector3(0.0002463258f, 0.001215198f, 0.02192333f), new Quaternion(-2.775558E-17f, 2.775558E-17f, -1.387779E-17f, 1f))
			}
		}
	};

	public static readonly HandSkeleton DefaultRightSkeleton = new HandSkeleton
	{
		joints = new HandSkeletonJoint[26]
		{
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(-0.000863f, -0.001272f, 0.047823f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = -1,
				pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(-0.030218f, -0.016084f, 0.034498f), new Quaternion(-0.127078f, -0.4597233f, 0.5512751f, 0.6845447f))
			},
			new HandSkeletonJoint
			{
				parent = 2,
				pose = new Pose(new Vector3(0.0009862719f, -0.005712928f, 0.03199217f), new Quaternion(0.3039057f, 0.06966191f, -0.09680318f, 0.9452078f))
			},
			new HandSkeletonJoint
			{
				parent = 3,
				pose = new Pose(new Vector3(1.384091E-06f, -6.110277E-06f, 0.03379152f), new Quaternion(0.1129837f, -0.05061896f, 0.07914509f, 0.9891453f))
			},
			new HandSkeletonJoint
			{
				parent = 4,
				pose = new Pose(new Vector3(0.0006700723f, 0.001027423f, 0.0245905f), new Quaternion(1.734723E-18f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(-0.019819f, -0.009505f, 0.036448f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 6,
				pose = new Pose(new Vector3(-0.003732f, 0.002189f, 0.059548f), new Quaternion(0.151882f, -0.07698268f, 0.0411778f, 0.9845354f))
			},
			new HandSkeletonJoint
			{
				parent = 7,
				pose = new Pose(new Vector3(-1.294385E-06f, -1.527845E-06f, 0.03792841f), new Quaternion(0.1307591f, 0.003759917f, -0.02628858f, 0.9910585f))
			},
			new HandSkeletonJoint
			{
				parent = 8,
				pose = new Pose(new Vector3(-9.285007E-07f, 1.593788E-07f, 0.02430516f), new Quaternion(-0.003017978f, 0.02607772f, -0.0164322f, 0.9995203f))
			},
			new HandSkeletonJoint
			{
				parent = 9,
				pose = new Pose(new Vector3(0.0002948791f, 0.001024898f, 0.0223638f), new Quaternion(0f, -4.336809E-19f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(-0.00361f, -0.007648f, 0.034286f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 11,
				pose = new Pose(new Vector3(0.001884f, 0.005105f, 0.061361f), new Quaternion(0.1896454f, 0.0117154f, -0.009750124f, 0.9817344f))
			},
			new HandSkeletonJoint
			{
				parent = 12,
				pose = new Pose(new Vector3(3.311234E-07f, -8.804644E-07f, 0.04292654f), new Quaternion(0.2042747f, 0.001967482f, -0.0123084f, 0.9788343f))
			},
			new HandSkeletonJoint
			{
				parent = 13,
				pose = new Pose(new Vector3(-1.703013E-07f, 5.871987E-07f, 0.02754843f), new Quaternion(-0.03223448f, 0.001938704f, -0.040453f, 0.9986595f))
			},
			new HandSkeletonJoint
			{
				parent = 14,
				pose = new Pose(new Vector3(0.0003095091f, 0.001137151f, 0.02496384f), new Quaternion(2.775558E-17f, -3.469447E-18f, -6.938894E-18f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(0.014992f, -0.006016f, 0.034776f), new Quaternion(0f, 0f, 0f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 16,
				pose = new Pose(new Vector3(0.002473f, -0.000513f, 0.053918f), new Quaternion(0.08123156f, 0.08615339f, -0.05587993f, 0.9913912f))
			},
			new HandSkeletonJoint
			{
				parent = 17,
				pose = new Pose(new Vector3(-5.266108E-07f, 3.652638E-07f, 0.03899503f), new Quaternion(0.3017412f, -0.007293773f, -0.03955524f, 0.9525411f))
			},
			new HandSkeletonJoint
			{
				parent = 18,
				pose = new Pose(new Vector3(1.039646E-06f, -5.423256E-07f, 0.02657356f), new Quaternion(0.09172485f, -0.02957153f, -0.008965976f, 0.9953048f))
			},
			new HandSkeletonJoint
			{
				parent = 19,
				pose = new Pose(new Vector3(0.0002563861f, 0.001606592f, 0.02432617f), new Quaternion(2.775558E-17f, 1.387779E-17f, -3.85186E-34f, 1f))
			},
			new HandSkeletonJoint
			{
				parent = 1,
				pose = new Pose(new Vector3(0.022999f, -0.009419999f, 0.034074f), new Quaternion(0.01833355f, 0.1403366f, -0.2070356f, 0.9680423f))
			},
			new HandSkeletonJoint
			{
				parent = 21,
				pose = new Pose(new Vector3(-2.530307E-06f, 1.160611E-06f, 0.04565198f), new Quaternion(-0.06267674f, 0.05101484f, 0.09903724f, 0.9917967f))
			},
			new HandSkeletonJoint
			{
				parent = 22,
				pose = new Pose(new Vector3(-1.720091E-07f, -6.646861E-07f, 0.03071994f), new Quaternion(0.3602582f, 0.025497f, -0.06776039f, 0.930039f))
			},
			new HandSkeletonJoint
			{
				parent = 23,
				pose = new Pose(new Vector3(-1.354028E-07f, 6.386686E-07f, 0.02031132f), new Quaternion(0.1151107f, -0.04873112f, 0.001109484f, 0.992156f))
			},
			new HandSkeletonJoint
			{
				parent = 24,
				pose = new Pose(new Vector3(-0.0002463258f, 0.001215198f, 0.02192333f), new Quaternion(-2.775558E-17f, -2.775558E-17f, 1.387779E-17f, 1f))
			}
		}
	};

	public IReadOnlyHandSkeletonJointList Joints => this;

	public ref readonly HandSkeletonJoint this[int jointId] => ref joints[jointId];

	public static HandSkeleton FromJoints(Transform[] joints)
	{
		HandSkeletonJoint[] array = new HandSkeletonJoint[joints.Length];
		for (int i = 0; i < joints.Length; i++)
		{
			Pose pose = joints[i].GetPose(Space.Self);
			array[i] = new HandSkeletonJoint
			{
				parent = FindParentIndex(i),
				pose = pose
			};
		}
		return new HandSkeleton
		{
			joints = array
		};
		int FindParentIndex(int jointIndex)
		{
			Transform parent = joints[jointIndex].parent;
			if (parent == null)
			{
				return -1;
			}
			for (int num = jointIndex - 1; num >= 0; num--)
			{
				if (joints[num] == parent)
				{
					return num;
				}
			}
			return -1;
		}
	}
}
