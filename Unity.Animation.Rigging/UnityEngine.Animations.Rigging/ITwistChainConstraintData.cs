namespace UnityEngine.Animations.Rigging;

public interface ITwistChainConstraintData
{
	Transform root { get; }

	Transform tip { get; }

	Transform rootTarget { get; }

	Transform tipTarget { get; }

	AnimationCurve curve { get; }
}
