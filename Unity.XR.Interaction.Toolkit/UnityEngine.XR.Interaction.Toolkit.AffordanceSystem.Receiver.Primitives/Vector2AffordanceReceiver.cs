using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[AddComponentMenu("Affordance System/Receiver/Primitives/Vector2 Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.Vector2AffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector2AffordanceReceiver : BaseAsyncAffordanceStateReceiver<float2>
{
	[SerializeField]
	[Tooltip("Vector2 Affordance Theme datum property used to map affordance state to a Vector2 affordance value. Can store an asset or a serialized value.")]
	private Vector2AffordanceThemeDatumProperty m_AffordanceThemeDatum;

	[SerializeField]
	[Tooltip("The event that is called when the current affordance value is updated.")]
	private Vector2UnityEvent m_ValueUpdated;

	public Vector2AffordanceThemeDatumProperty affordanceThemeDatum
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

	public Vector2UnityEvent valueUpdated
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

	protected override BaseAffordanceTheme<float2> defaultAffordanceTheme
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

	protected override BindableVariable<float2> affordanceValue { get; } = new BindableVariable<float2>();


	protected override JobHandle ScheduleTweenJob(ref TweenJobData<float2> jobData)
	{
		Float2TweenJob jobData2 = default(Float2TweenJob);
		jobData2.jobData = jobData;
		return jobData2.Schedule();
	}

	protected override BaseAffordanceTheme<float2> GenerateNewAffordanceThemeInstance()
	{
		return new Vector2AffordanceTheme();
	}

	protected override void OnAffordanceValueUpdated(float2 newValue)
	{
		m_ValueUpdated?.Invoke(newValue);
	}
}
