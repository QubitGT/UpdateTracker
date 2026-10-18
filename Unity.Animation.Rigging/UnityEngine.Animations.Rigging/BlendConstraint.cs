namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Blend Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/BlendConstraint.html")]
public class BlendConstraint : RigConstraint<BlendConstraintJob, BlendConstraintData, BlendConstraintJobBinder<BlendConstraintData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		m_Data.positionWeight = Mathf.Clamp01(m_Data.positionWeight);
		m_Data.rotationWeight = Mathf.Clamp01(m_Data.rotationWeight);
	}
}
