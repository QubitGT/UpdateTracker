namespace UnityEngine.Animations.Rigging;

public interface IMultiParentConstraintData
{
	Transform constrainedObject { get; }

	WeightedTransformArray sourceObjects { get; }

	bool maintainPositionOffset { get; }

	bool maintainRotationOffset { get; }

	bool constrainedPositionXAxis { get; }

	bool constrainedPositionYAxis { get; }

	bool constrainedPositionZAxis { get; }

	bool constrainedRotationXAxis { get; }

	bool constrainedRotationYAxis { get; }

	bool constrainedRotationZAxis { get; }

	string sourceObjectsProperty { get; }
}
