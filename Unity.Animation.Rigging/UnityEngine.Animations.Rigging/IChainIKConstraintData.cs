namespace UnityEngine.Animations.Rigging;

public interface IChainIKConstraintData
{
	Transform root { get; }

	Transform tip { get; }

	Transform target { get; }

	int maxIterations { get; }

	float tolerance { get; }

	bool maintainTargetPositionOffset { get; }

	bool maintainTargetRotationOffset { get; }

	string chainRotationWeightFloatProperty { get; }

	string tipRotationWeightFloatProperty { get; }
}
