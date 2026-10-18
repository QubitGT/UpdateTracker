namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Multi-Rotation Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/MultiRotationConstraint.html")]
public class MultiRotationConstraint : RigConstraint<MultiRotationConstraintJob, MultiRotationConstraintData, MultiRotationConstraintJobBinder<MultiRotationConstraintData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		WeightedTransformArray array = m_Data.sourceObjects;
		WeightedTransformArray.OnValidate(ref array);
		m_Data.sourceObjects = array;
	}
}
