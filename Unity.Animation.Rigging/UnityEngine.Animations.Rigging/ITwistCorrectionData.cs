namespace UnityEngine.Animations.Rigging;

public interface ITwistCorrectionData
{
	Transform source { get; }

	WeightedTransformArray twistNodes { get; }

	Vector3 twistAxis { get; }

	string twistNodesProperty { get; }
}
