using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Input Axis Binder")]
[VFXBinder("Input/Axis")]
internal class VFXInputAxisBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "System.Single" })]
	[SerializeField]
	[FormerlySerializedAs("m_AxisParameter")]
	protected ExposedProperty m_AxisProperty = "Axis";

	public string AxisName = "Horizontal";

	public float AccumulateSpeed = 1f;

	public bool Accumulate = true;

	public string AxisProperty
	{
		get
		{
			return (string)m_AxisProperty;
		}
		set
		{
			m_AxisProperty = value;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		return component.HasFloat(m_AxisProperty);
	}

	public override void UpdateBinding(VisualEffect component)
	{
	}

	public override string ToString()
	{
		return $"Input Axis: '{m_AxisProperty}' -> {AxisName.ToString()}";
	}
}
