namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Chain IK Constraint")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/ChainIKConstraint.html")]
public class ChainIKConstraint : RigConstraint<ChainIKConstraintJob, ChainIKConstraintData, ChainIKConstraintJobBinder<ChainIKConstraintData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		m_Data.chainRotationWeight = Mathf.Clamp01(m_Data.chainRotationWeight);
		m_Data.tipRotationWeight = Mathf.Clamp01(m_Data.tipRotationWeight);
		m_Data.maxIterations = Mathf.Clamp(m_Data.maxIterations, 1, 50);
		m_Data.tolerance = Mathf.Clamp(m_Data.tolerance, 0f, 0.01f);
	}
}
