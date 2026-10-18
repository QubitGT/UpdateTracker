namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Override Transform")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/OverrideTransform.html")]
public class OverrideTransform : RigConstraint<OverrideTransformJob, OverrideTransformData, OverrideTransformJobBinder<OverrideTransformData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		m_Data.positionWeight = Mathf.Clamp01(m_Data.positionWeight);
		m_Data.rotationWeight = Mathf.Clamp01(m_Data.rotationWeight);
	}
}
