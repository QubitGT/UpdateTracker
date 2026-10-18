using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[AddComponentMenu("Affordance System/Receiver/Primitives/Vector4 Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.Vector4AffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector4AffordanceReceiver : BaseAsyncAffordanceStateReceiver<float4>
{
	[SerializeField]
	[Tooltip("Vector4 Affordance Theme datum property used to map affordance state to a Vector4 affordance value. Can store an asset or a serialized value.")]
	private Vector4AffordanceThemeDatumProperty m_AffordanceThemeDatum;

	[SerializeField]
	[Tooltip("The event that is called when the current affordance value is updated.")]
	private Vector4UnityEvent m_ValueUpdated;

	public Vector4AffordanceThemeDatumProperty affordanceThemeDatum
	{
		get
		{
			return m_AffordanceThemeDatum;
		}
		set
		{
			m_AffordanceThemeDatum = value;
		}
	}

	public Vector4UnityEvent valueUpdated
	{
		get
		{
			return m_ValueUpdated;
		}
		set
		{
			m_ValueUpdated = value;
		}
	}

	protected override BaseAffordanceTheme<float4> defaultAffordanceTheme
	{
		get
		{
			if (m_AffordanceThemeDatum == null)
			{
				return null;
			}
			return m_AffordanceThemeDatum.Value;
		}
	}

	protected override BindableVariable<float4> affordanceValue { get; } = new BindableVariable<float4>();


	protected override JobHandle ScheduleTweenJob(ref TweenJobData<float4> jobData)
	{
		Float4TweenJob jobData2 = default(Float4TweenJob);
		jobData2.jobData = jobData;
		return jobData2.Schedule();
	}

	protected override BaseAffordanceTheme<float4> GenerateNewAffordanceThemeInstance()
	{
		return new Vector4AffordanceTheme();
	}

	protected override void OnAffordanceValueUpdated(float4 newValue)
	{
		m_ValueUpdated?.Invoke(newValue);
	}
}
