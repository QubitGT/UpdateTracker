namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Multi-Parent Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/MultiParentConstraint.html")]
public class MultiParentConstraint : RigConstraint<MultiParentConstraintJob, MultiParentConstraintData, MultiParentConstraintJobBinder<MultiParentConstraintData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		WeightedTransformArray array = m_Data.sourceObjects;
		WeightedTransformArray.OnValidate(ref array);
		m_Data.sourceObjects = array;
	}
}
