namespace UnityEngine.Animations.Rigging;

public interface ITwoBoneIKConstraintData
{
	Transform root { get; }

	Transform mid { get; }

	Transform tip { get; }

	Transform target { get; }

	Transform hint { get; }

	bool maintainTargetPositionOffset { get; }

	bool maintainTargetRotationOffset { get; }

	string targetPositionWeightFloatProperty { get; }

	string targetRotationWeightFloatProperty { get; }

	string hintWeightFloatProperty { get; }
}
