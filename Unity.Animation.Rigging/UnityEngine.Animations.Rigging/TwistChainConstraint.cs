namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Twist Chain Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/TwistChainConstraint.html")]
public class TwistChainConstraint : RigConstraint<TwistChainConstraintJob, TwistChainConstraintData, TwistChainConstraintJobBinder<TwistChainConstraintData>>
{
}
