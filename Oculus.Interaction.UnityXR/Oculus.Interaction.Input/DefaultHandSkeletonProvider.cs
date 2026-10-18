using UnityEngine;

namespace Oculus.Interaction.Input;

public class DefaultHandSkeletonProvider : MonoBehaviour, IHandSkeletonProvider
{
	private readonly HandSkeleton[] _skeletons = new HandSkeleton[2]
	{
		HandSkeleton.DefaultLeftSkeleton,
		HandSkeleton.DefaultRightSkeleton
	};

	public HandSkeleton this[Handedness handedness] => _skeletons[(int)handedness];
}
