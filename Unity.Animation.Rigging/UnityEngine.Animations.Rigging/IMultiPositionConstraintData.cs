namespace UnityEngine.Animations.Rigging;

public interface IMultiPositionConstraintData
{
	Transform constrainedObject { get; }

	WeightedTransformArray sourceObjects { get; }

	bool maintainOffset { get; }

	string offsetVector3Property { get; }

	string sourceObjectsProperty { get; }

	bool constrainedXAxis { get; }

	bool constrainedYAxis { get; }

	bool constrainedZAxis { get; }
}
