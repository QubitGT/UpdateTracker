using Meta.XR.Util;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class OVRSkeletonData
{
	public static readonly OVRPlugin.Skeleton2 LeftSkeleton = new OVRPlugin.Skeleton2
	{
		Type = OVRPlugin.SkeletonType.XRHandLeft,
		NumBones = 26u,
		NumBoneCapsules = 19u,
		Bones = new OVRPlugin.Bone[26]
		{
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Start,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.002120435f,
						y = -0.00547956f,
						z = -0.0653313f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_ForearmStub,
				ParentBoneIndex = -1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = 0f,
						z = 0f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.7071068f,
						y = 3.090862E-08f,
						z = -0.7071068f,
						w = -3.090862E-08f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb0,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0280285f,
						y = -0.01915772f,
						z = -0.03595843f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.005259067f,
						y = -0.3771799f,
						z = -0.6271985f,
						w = 0.6814173f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb1,
				ParentBoneIndex = 2,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = 4.190952E-09f,
						z = -0.0325129f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.08406219f,
						y = 0.07696168f,
						z = 0.08270374f,
						w = 0.9900356f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb2,
				ParentBoneIndex = 3,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -2.793968E-09f,
						y = -5.587935E-09f,
						z = -0.03379309f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.05827412f,
						y = -0.06501578f,
						z = -0.08350593f,
						w = 0.9926752f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb3,
				ParentBoneIndex = 4,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.000670366f,
						y = 0.001026981f,
						z = -0.02459078f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 1.490116E-08f,
						z = 0f,
						w = 1f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Index1,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.01872439f,
						y = -0.01104215f,
						z = -0.03717846f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Index2,
				ParentBoneIndex = 6,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.004826289f,
						y = 0.003725696f,
						z = -0.05881777f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.04328144f,
						y = 0.01885557f,
						z = -0.03068309f,
						w = 0.9984136f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Index3,
				ParentBoneIndex = 7,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 1.862645E-09f,
						y = 4.656613E-10f,
						z = -0.0379273f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.003292942f,
						y = 0.007116079f,
						z = 0.02585241f,
						w = 0.9996351f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Middle1,
				ParentBoneIndex = 8,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -5.587935E-09f,
						y = 1.629815E-09f,
						z = -0.02430366f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.07203402f,
						y = 0.02714875f,
						z = 0.016056f,
						w = 0.9969035f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Middle2,
				ParentBoneIndex = 9,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.0002956167f,
						y = 0.00102507f,
						z = -0.02236339f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = -9.313226E-10f,
						w = 1f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Middle3,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.002514964f,
						y = -0.008415965f,
						z = -0.03501599f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Ring1,
				ParentBoneIndex = 11,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.0007890596f,
						y = 0.005872811f,
						z = -0.06063062f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.05183575f,
						y = 0.0514656f,
						z = 0.009066325f,
						w = 0.9972874f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Ring2,
				ParentBoneIndex = 12,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 1.303852E-08f,
						y = 4.656613E-10f,
						z = -0.042927f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.001978267f,
						y = 0.004378915f,
						z = 0.01122824f,
						w = 0.9999254f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Ring3,
				ParentBoneIndex = 13,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -2.793968E-09f,
						y = -4.656613E-10f,
						z = -0.02754958f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.093007f,
						y = 0.00461179f,
						z = 0.03431955f,
						w = 0.995063f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky0,
				ParentBoneIndex = 14,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.000308645f,
						y = 0.001137298f,
						z = -0.02496493f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = -4.656613E-10f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky1,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.01499234f,
						y = -0.00601578f,
						z = -0.03477554f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky2,
				ParentBoneIndex = 16,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.002472909f,
						y = -0.0005135289f,
						z = -0.05391826f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.04981351f,
						y = 0.1231034f,
						z = 0.05315936f,
						w = 0.9897162f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky3,
				ParentBoneIndex = 17,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -9.313226E-10f,
						y = 1.629815E-09f,
						z = -0.03899609f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.005676018f,
						y = 0.002789885f,
						z = 0.03363252f,
						w = 0.9994141f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_MaxSkinnable,
				ParentBoneIndex = 18,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -5.587935E-09f,
						y = -9.313226E-10f,
						z = -0.02657339f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.02502854f,
						y = -0.02917945f,
						z = 0.003477454f,
						w = 0.9992546f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_IndexTip,
				ParentBoneIndex = 19,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.0002579107f,
						y = 0.001608172f,
						z = -0.02432613f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 1f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_MiddleTip,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.02299858f,
						y = -0.009419838f,
						z = -0.03407356f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.0183118f,
						y = 0.1403429f,
						z = 0.207036f,
						w = 0.9680417f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_RingTip,
				ParentBoneIndex = 21,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 2.196059E-06f,
						y = -9.993091E-07f,
						z = -0.04565054f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.02812923f,
						y = -0.004071385f,
						z = -0.09111302f,
						w = 0.9954348f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_PinkyTip,
				ParentBoneIndex = 22,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = 4.656613E-10f,
						z = -0.0307204f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.01328605f,
						y = 0.04293776f,
						z = 0.03761665f,
						w = 0.9982808f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_End,
				ParentBoneIndex = 23,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 1.490116E-08f,
						y = -1.862645E-09f,
						z = -0.02031136f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.02401882f,
						y = -0.04917067f,
						z = -0.0006447211f,
						w = 0.9985011f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.XRHand_LittleTip,
				ParentBoneIndex = 24,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0002464727f,
						y = 0.001216088f,
						z = -0.02192238f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999997f
					}
				}
			}
		},
		BoneCapsules = new OVRPlugin.BoneCapsule[19]
		{
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.01822828f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0.01685145f,
					y = -0.01404148f,
					z = -0.02755879f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.02178326f,
					y = -0.009090677f,
					z = -0.07794081f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.02323196f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0.006531342f,
					y = -0.008661011f,
					z = -0.02632602f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.003326345f,
					y = -0.004580691f,
					z = -0.07255958f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.01608828f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -0.01111641f,
					y = -0.009206061f,
					z = -0.0297035f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.01574543f,
					y = -0.007254404f,
					z = -0.07271415f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.02346085f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -0.01446979f,
					y = -0.008827155f,
					z = -0.02844799f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.02133043f,
					y = -0.009573799f,
					z = -0.06036391f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 2,
				Radius = 0.01838252f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = -3.259629E-09f,
					z = 3.72529E-09f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 4.656613E-09f,
					y = 3.259629E-09f,
					z = -0.03251291f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 3,
				Radius = 0.01028295f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 1.862645E-09f,
					y = -5.587935E-09f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 8.381903E-09f,
					y = -9.313226E-10f,
					z = -0.03379309f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 4,
				Radius = 0.009768805f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 5.587935E-09f,
					y = -3.72529E-09f,
					z = -7.450581E-09f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.0005929563f,
					y = 0.0006525218f,
					z = -0.01500077f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 7,
				Radius = 0.01029526f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 3.72529E-09f,
					y = -2.328306E-10f,
					z = 7.450581E-09f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 5.587935E-09f,
					y = -1.629815E-09f,
					z = -0.03792728f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 8,
				Radius = 0.008038102f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 6.984919E-10f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -5.587935E-09f,
					y = 1.164153E-09f,
					z = -0.02430364f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 9,
				Radius = 0.007636196f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = 0f,
					z = 1.490116E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -6.049871E-05f,
					y = 0.0005028695f,
					z = -0.01507758f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 12,
				Radius = 0.01117394f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = 1.396984E-09f,
					z = -7.450581E-09f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 1.303852E-08f,
					y = 3.259629E-09f,
					z = -0.042927f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 13,
				Radius = 0.008030958f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 7.450581E-09f,
					y = -4.656613E-10f,
					z = -2.980232E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 4.656613E-09f,
					y = -1.396984E-09f,
					z = -0.02754961f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 14,
				Radius = 0.007629411f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 3.72529E-09f,
					z = -2.980232E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.0004036417f,
					y = 0.0007450115f,
					z = -0.01719157f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 17,
				Radius = 0.009922137f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = -6.984919E-10f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 1.024455E-08f,
					y = 1.629815E-09f,
					z = -0.03899611f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 18,
				Radius = 0.007611672f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = 0f,
					z = -1.490116E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -5.587935E-09f,
					y = -9.313226E-10f,
					z = -0.02657339f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 19,
				Radius = 0.007231089f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 9.313226E-10f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.0001235802f,
					y = 0.001288095f,
					z = -0.01632452f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 22,
				Radius = 0.008483353f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 0f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 2.328306E-09f,
					z = -0.0307204f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 23,
				Radius = 0.006764194f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 1.862645E-09f,
					z = 1.490116E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 3.72529E-09f,
					y = 0f,
					z = -0.02031135f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 24,
				Radius = 0.006425985f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 0f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 2.491102E-05f,
					y = 0.000605626f,
					z = -0.01507002f
				}
			}
		}
	};

	public static readonly OVRPlugin.Skeleton2 RightSkeleton = new OVRPlugin.Skeleton2
	{
		Type = OVRPlugin.SkeletonType.XRHandRight,
		NumBones = 26u,
		NumBoneCapsules = 19u,
		Bones = new OVRPlugin.Bone[26]
		{
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Start,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.002120435f,
						y = -0.00547956f,
						z = -0.0653313f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_ForearmStub,
				ParentBoneIndex = -1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = 0f,
						z = 0f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0.7071068f,
						z = 0f,
						w = 0.7071068f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb0,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.0280285f,
						y = -0.01915772f,
						z = -0.03595843f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.005259037f,
						y = 0.3771799f,
						z = 0.6271985f,
						w = 0.6814173f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb1,
				ParentBoneIndex = 2,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = -9.313226E-10f,
						z = -0.03251291f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.08406219f,
						y = -0.07696167f,
						z = -0.0827037f,
						w = 0.9900356f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb2,
				ParentBoneIndex = 3,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -1.862645E-09f,
						y = -6.519258E-09f,
						z = -0.03379311f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.05827411f,
						y = 0.06501578f,
						z = 0.08350589f,
						w = 0.9926752f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Thumb3,
				ParentBoneIndex = 4,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0006703734f,
						y = 0.001026981f,
						z = -0.02459077f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = -1.490116E-08f,
						z = -3.72529E-09f,
						w = 1f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Index1,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.01872439f,
						y = -0.01104215f,
						z = -0.03717846f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Index2,
				ParentBoneIndex = 6,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.004826291f,
						y = 0.003725695f,
						z = -0.05881777f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.04328144f,
						y = -0.01885557f,
						z = 0.03068309f,
						w = 0.9984136f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Index3,
				ParentBoneIndex = 7,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = -2.328306E-10f,
						z = -0.0379273f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.003292941f,
						y = -0.007116065f,
						z = -0.02585241f,
						w = 0.999635f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Middle1,
				ParentBoneIndex = 8,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 1.117587E-08f,
						y = 2.095476E-09f,
						z = -0.02430366f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.07203402f,
						y = -0.02714874f,
						z = -0.016056f,
						w = 0.9969035f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Middle2,
				ParentBoneIndex = 9,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0002956092f,
						y = 0.00102507f,
						z = -0.02236339f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = -1.688022E-09f,
						z = 0f,
						w = 1f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Middle3,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.002514964f,
						y = -0.008415964f,
						z = -0.03501599f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Ring1,
				ParentBoneIndex = 11,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0007890593f,
						y = 0.00587281f,
						z = -0.06063062f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.05183575f,
						y = -0.0514656f,
						z = -0.009066325f,
						w = 0.9972874f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Ring2,
				ParentBoneIndex = 12,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -7.450581E-09f,
						y = 9.313226E-10f,
						z = -0.042927f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.001978267f,
						y = -0.004378904f,
						z = -0.01122824f,
						w = 0.9999253f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Ring3,
				ParentBoneIndex = 13,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 7.450581E-09f,
						y = 0f,
						z = -0.02754958f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.09300701f,
						y = -0.004611799f,
						z = -0.03431955f,
						w = 0.995063f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky0,
				ParentBoneIndex = 14,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0003086478f,
						y = 0.001137299f,
						z = -0.02496493f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 5.209586E-09f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky1,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.01499234f,
						y = -0.006015779f,
						z = -0.03477554f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 0f,
						z = 0f,
						w = 0.9999999f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky2,
				ParentBoneIndex = 16,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.002472909f,
						y = -0.0005135285f,
						z = -0.05391826f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.04981349f,
						y = -0.1231034f,
						z = -0.05315936f,
						w = 0.9897162f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_Pinky3,
				ParentBoneIndex = 17,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0f,
						y = 9.313226E-10f,
						z = -0.0389961f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.005676013f,
						y = -0.002789889f,
						z = -0.03363252f,
						w = 0.9994141f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_MaxSkinnable,
				ParentBoneIndex = 18,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -7.450581E-09f,
						y = -1.862645E-09f,
						z = -0.02657339f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.02502853f,
						y = 0.02917946f,
						z = -0.003477456f,
						w = 0.9992546f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_IndexTip,
				ParentBoneIndex = 19,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.0002579093f,
						y = 0.001608171f,
						z = -0.02432612f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = -1.117587E-08f,
						z = 0f,
						w = 1f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_MiddleTip,
				ParentBoneIndex = 1,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 0.02299858f,
						y = -0.009419835f,
						z = -0.03407356f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.01831179f,
						y = -0.1403429f,
						z = -0.207036f,
						w = 0.9680417f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_RingTip,
				ParentBoneIndex = 21,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -2.194196E-06f,
						y = -1.00024E-06f,
						z = -0.04565054f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = -0.02812924f,
						y = 0.004071368f,
						z = 0.09111303f,
						w = 0.9954348f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_PinkyTip,
				ParentBoneIndex = 22,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = 1.862645E-09f,
						y = 0f,
						z = -0.03072041f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.01328605f,
						y = -0.04293777f,
						z = -0.03761665f,
						w = 0.9982808f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.Hand_End,
				ParentBoneIndex = 23,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -3.72529E-09f,
						y = -2.793968E-09f,
						z = -0.02031137f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0.02401883f,
						y = 0.04917067f,
						z = 0.0006447285f,
						w = 0.9985011f
					}
				}
			},
			new OVRPlugin.Bone
			{
				Id = OVRPlugin.BoneId.XRHand_LittleTip,
				ParentBoneIndex = 24,
				Pose = new OVRPlugin.Posef
				{
					Position = new OVRPlugin.Vector3f
					{
						x = -0.0002464727f,
						y = 0.001216089f,
						z = -0.02192238f
					},
					Orientation = new OVRPlugin.Quatf
					{
						x = 0f,
						y = 2.793968E-09f,
						z = 0f,
						w = 0.9999997f
					}
				}
			}
		},
		BoneCapsules = new OVRPlugin.BoneCapsule[19]
		{
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.01822828f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -0.01685145f,
					y = -0.01404148f,
					z = -0.02755879f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.02178326f,
					y = -0.009090677f,
					z = -0.07794081f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.02323196f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -0.006531343f,
					y = -0.008661012f,
					z = -0.02632602f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -0.003326345f,
					y = -0.004580691f,
					z = -0.07255958f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.01608828f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0.01111641f,
					y = -0.00920606f,
					z = -0.0297035f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.01574543f,
					y = -0.007254402f,
					z = -0.07271415f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 1,
				Radius = 0.02346085f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0.01446979f,
					y = -0.008827152f,
					z = -0.02844799f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.02133043f,
					y = -0.009573797f,
					z = -0.06036392f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 2,
				Radius = 0.01838251f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 5.587935E-09f,
					y = 9.313226E-10f,
					z = 3.72529E-09f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -2.793968E-09f,
					y = -9.313226E-10f,
					z = -0.03251291f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 3,
				Radius = 0.01028296f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -7.450581E-09f,
					y = 0f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -1.490116E-08f,
					y = -2.793968E-09f,
					z = -0.03379309f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 4,
				Radius = 0.009768807f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -1.862645E-09f,
					y = 3.72529E-09f,
					z = -7.450581E-09f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.0005929582f,
					y = 0.0006525703f,
					z = -0.0150008f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 7,
				Radius = 0.01029526f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = 2.328306E-10f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = -1.396984E-09f,
					z = -0.03792728f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 8,
				Radius = 0.008038101f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 9.313226E-10f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = 2.328306E-09f,
					z = -0.02430366f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 9,
				Radius = 0.007636196f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 0f,
					z = 2.980232E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 6.053597E-05f,
					y = 0.0005028695f,
					z = -0.01507759f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 12,
				Radius = 0.01117394f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 3.72529E-09f,
					y = 0f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -7.450581E-09f,
					y = 1.396984E-09f,
					z = -0.042927f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 13,
				Radius = 0.008030958f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -7.450581E-09f,
					y = 0f,
					z = -2.980232E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 0f,
					z = -0.02754962f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 14,
				Radius = 0.00762941f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 3.72529E-09f,
					z = -1.490116E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.0004036501f,
					y = 0.0007450059f,
					z = -0.01719154f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 17,
				Radius = 0.009922139f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = -2.328306E-10f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -1.117587E-08f,
					y = 9.313226E-10f,
					z = -0.03899612f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 18,
				Radius = 0.007611674f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 3.72529E-09f,
					y = 1.862645E-09f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -7.450581E-09f,
					y = 4.656613E-10f,
					z = -0.02657339f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 19,
				Radius = 0.00723109f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 1.862645E-09f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = 0.0001235828f,
					y = 0.001288087f,
					z = -0.01632456f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 22,
				Radius = 0.008483353f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 0f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -1.862645E-09f,
					y = 2.793968E-09f,
					z = -0.03072041f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 23,
				Radius = 0.006764191f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = -1.117587E-08f,
					y = 0f,
					z = 1.490116E-08f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -3.72529E-09f,
					y = -9.313226E-10f,
					z = -0.02031137f
				}
			},
			new OVRPlugin.BoneCapsule
			{
				BoneIndex = 24,
				Radius = 0.006425982f,
				StartPoint = new OVRPlugin.Vector3f
				{
					x = 0f,
					y = 0f,
					z = 0f
				},
				EndPoint = new OVRPlugin.Vector3f
				{
					x = -2.489984E-05f,
					y = 0.0006056232f,
					z = -0.01507002f
				}
			}
		}
	};
}
