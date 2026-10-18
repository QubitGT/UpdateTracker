namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Multi-Referential Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/MultiReferentialConstraint.html")]
public class MultiReferentialConstraint : RigConstraint<MultiReferentialConstraintJob, MultiReferentialConstraintData, MultiReferentialConstraintJobBinder<MultiReferentialConstraintData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		m_Data.UpdateDriver();
	}
}
