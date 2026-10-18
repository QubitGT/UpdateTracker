using System.Linq;
using UnityEngine;

namespace Oculus.Interaction.Input.Compatibility.OVR;

public class HandSkeleton : IReadOnlyHandSkeleton, IReadOnlyHandSkeletonJointList
{
	public HandSkeletonJoint[] joints = new HandSkeletonJoint[24];

	public static readonly HandSkeleton DefaultLeftSkeleton = new HandSkeleton
	{
		joints = new HandSkeletonJoint[24]
		{
			new HandSkeletonJoint
			{
				parent = -1,
				pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, -1f))
			},
			new HandSkeletonJoint
			{
				parent = 0,
				pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, -1f))
			},
			new HandSkeletonJoint
			{
				parent = 0,
				pose = new Pose(new Vector3(-0.0200693f, 0.0115541f, -0.01049652f), new Quaternion(-0.3753869f, 0.4245841f, -0.007778856f, -0.8238644f))
			},
			new HandSkeletonJoint
			{
				parent = 2,
				pose = new Pose(new Vector3(-0.02485256f, -9.31E-10f, -1.863E-09f), new Quaternion(-0.2602303f, 0.02433088f, 0.125678f, -0.9570231f))
			},
			new HandSkeletonJoint
			{
				parent = 3,
				pose = new Pose(new Vector3(-0.03251291f, 5.82E-10f, 1.863E-09f), new Quaternion(0.08270377f, -0.0769617f, -0.08406223f, -0.9900357f))
			},
			new HandSkeletonJoint
			{
				parent = 4,
				pose = new Pose(new Vector3(-0.0337931f, 3.26E-09f, 1.863E-09f), new Quaternion(-0.08350593f, 0.06501573f, -0.05827406f, -0.9926752f))
			},
			new HandSkeletonJoint
			{
				parent = 0,
				pose = new Pose(new Vector3(-0.09599624f, 0.007316455f, -0.02355068f), new Quaternion(-0.03068309f, -0.01885559f, 0.04328144f, -0.9984136f))
			},
			new HandSkeletonJoint
			{
				parent = 6,
				pose = new Pose(new Vector3(-0.0379273f, -5.82E-10f, -5.97E-10f), new Quaternion(0.02585241f, -0.007116061f, 0.003292944f, -0.999635f))
			},
			new HandSkeletonJoint
			{
				parent = 7,
				pose = new Pose(new Vector3(-0.02430365f, -6.73E-10f, -6.75E-10f), new Quaternion(0.016056f, -0.02714872f, -0.072034f, -0.9969034f))
			},
			new HandSkeletonJoint
			{
				parent = 0,
				pose = new Pose(new Vector3(-0.09564661f, 0.002543155f, -0.001725906f), new Quaternion(0.009066326f, -0.05146559f, 0.05183575f, -0.9972874f))
			},
			new HandSkeletonJoint
			{
				parent = 9,
				pose = new Pose(new Vector3(-0.042927f, -8.51E-10f, -1.193E-09f), new Quaternion(0.01122823f, -0.004378874f, -0.001978267f, -0.9999254f))
			},
			new HandSkeletonJoint
			{
				parent = 10,
				pose = new Pose(new Vector3(-0.02754958f, 3.09E-10f, 1.128E-09f), new Quaternion(0.03431955f, -0.004611839f, -0.09300701f, -0.9950631f))
			},
			new HandSkeletonJoint
			{
				parent = 0,
				pose = new Pose(new Vector3(-0.0886938f, 0.006529308f, 0.01746524f), new Quaternion(0.05315936f, -0.1231034f, 0.04981349f, -0.9897162f))
			},
			new HandSkeletonJoint
			{
				parent = 12,
				pose = new Pose(new Vector3(-0.0389961f, 0f, 5.24E-10f), new Quaternion(0.03363252f, -0.00278984f, 0.00567602f, -0.9994143f))
			},
			new HandSkeletonJoint
			{
				parent = 13,
				pose = new Pose(new Vector3(-0.02657339f, 1.281E-09f, 1.63E-09f), new Quaternion(0.003477462f, 0.02917945f, -0.02502854f, -0.9992548f))
			},
			new HandSkeletonJoint
			{
				parent = 0,
				pose = new Pose(new Vector3(-0.03407356f, 0.009419836f, 0.02299858f), new Quaternion(0.207036f, -0.1403428f, 0.0183118f, -0.9680417f))
			},
			new HandSkeletonJoint
			{
				parent = 15,
				pose = new Pose(new Vector3(-0.04565055f, 9.97679E-07f, -2.193963E-06f), new Quaternion(-0.09111304f, 0.00407137f, 0.02812923f, -0.9954349f))
			},
			new HandSkeletonJoint
			{
				parent = 16,
				pose = new Pose(new Vector3(-0.03072042f, 1.048E-09f, -1.75E-10f), new Quaternion(0.03761665f, -0.04293772f, -0.01328605f, -0.9982809f))
			},
			new HandSkeletonJoint
			{
				parent = 17,
				pose = new Pose(new Vector3(-0.02031138f, -2.91E-10f, 9.31E-10f), new Quaternion(-0.0006447434f, 0.04917067f, -0.02401883f, -0.9985014f))
			},
			new HandSkeletonJoint
			{
				parent = 5,
				pose = new Pose(new Vector3(-0.02459077f, -0.001026974f, 0.0006703701f), new Quaternion(0f, 0f, 0f, -1f))
			},
			new HandSkeletonJoint
			{
				parent = 8,
				pose = new Pose(new Vector3(-0.02236338f, -0.00102507f, 0.0002956076f), new Quaternion(0f, 0f, 0f, -1f))
			},
			new HandSkeletonJoint
			{
				parent = 11,
				pose = new Pose(new Vector3(-0.02496492f, -0.001137299f, 0.0003086528f), new Quaternion(0f, 0f, 0f, -1f))
			},
			new HandSkeletonJoint
			{
				parent = 14,
				pose = new Pose(new Vector3(-0.02432613f, -0.001608172f, 0.000257905f), new Quaternion(0f, 0f, 0f, -1f))
			},
			new HandSkeletonJoint
			{
				parent = 18,
				pose = new Pose(new Vector3(-0.02192238f, -0.001216086f, -0.0002464796f), new Quaternion(0f, 0f, 0f, -1f))
			}
		}
	};

	public static readonly HandSkeleton DefaultRightSkeleton = new HandSkeleton
	{
		joints = DefaultLeftSkeleton.joints.Select(delegate(HandSkeletonJoint joint)
		{
			HandSkeletonJoint result = default(HandSkeletonJoint);
			result.parent = joint.parent;
			result.pose = new Pose(-joint.pose.position, joint.pose.rotation);
			return result;
		}).ToArray()
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
