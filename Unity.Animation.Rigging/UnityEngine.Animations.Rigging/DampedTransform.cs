namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Damped Transform")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/constraints/DampedTransform.html")]
public class DampedTransform : RigConstraint<DampedTransformJob, DampedTransformData, DampedTransformJobBinder<DampedTransformData>>
{
	protected override void OnValidate()
	{
		base.OnValidate();
		m_Data.dampPosition = Mathf.Clamp01(m_Data.dampPosition);
		m_Data.dampRotation = Mathf.Clamp01(m_Data.dampRotation);
	}
}
