using System;
using Unity.Mathematics;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[AddComponentMenu("Affordance System/Receiver/Primitives/Quaternion Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.QuaternionAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class QuaternionAffordanceReceiver : Vector4AffordanceReceiver
{
	[SerializeField]
	[Tooltip("The event that is called when the current affordance value is updated, expressed as a quaternion.")]
	private QuaternionUnityEvent m_QuaternionValueUpdated;

	public QuaternionUnityEvent quaternionValueUpdated
	{
		get
		{
			return m_QuaternionValueUpdated;
		}
		set
		{
			m_QuaternionValueUpdated = value;
		}
	}

	protected override void OnAffordanceValueUpdated(float4 newValue)
	{
		base.OnAffordanceValueUpdated(newValue);
		m_QuaternionValueUpdated?.Invoke(new Quaternion(newValue.x, newValue.y, newValue.z, newValue.w));
	}
}
