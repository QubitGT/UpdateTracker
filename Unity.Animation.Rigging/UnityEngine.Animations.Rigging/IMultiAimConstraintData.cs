namespace UnityEngine.Animations.Rigging;

public interface IMultiAimConstraintData
{
	Transform constrainedObject { get; }

	WeightedTransformArray sourceObjects { get; }

	bool maintainOffset { get; }

	Vector3 aimAxis { get; }

	Vector3 upAxis { get; }

	int worldUpType { get; }

	Vector3 worldUpAxis { get; }

	Transform worldUpObject { get; }

	bool constrainedXAxis { get; }

	bool constrainedYAxis { get; }

	bool constrainedZAxis { get; }

	string offsetVector3Property { get; }

	string minLimitFloatProperty { get; }

	string maxLimitFloatProperty { get; }

	string sourceObjectsProperty { get; }
}
