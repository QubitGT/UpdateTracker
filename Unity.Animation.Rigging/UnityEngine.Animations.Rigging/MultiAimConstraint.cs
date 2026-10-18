namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Multi-Aim Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/MultiAimConstraint.html")]
public class MultiAimConstraint : RigConstraint<MultiAimConstraintJob, MultiAimConstraintData, MultiAimConstraintJobBinder<MultiAimConstraintData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		WeightedTransformArray array = m_Data.sourceObjects;
		WeightedTransformArray.OnValidate(ref array);
		m_Data.sourceObjects = array;
		Vector2 limits = m_Data.limits;
		limits.x = Mathf.Clamp(limits.x, -180f, 180f);
		limits.y = Mathf.Clamp(limits.y, -180f, 180f);
		m_Data.limits = limits;
	}
}
