using System;
using Unity.Jobs;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[AddComponentMenu("Affordance System/Receiver/Primitives/Float Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.FloatAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace has ")]
public class FloatAffordanceReceiver : BaseAsyncAffordanceStateReceiver<float>
{
	[SerializeField]
	[Tooltip("Float Affordance Theme datum property used to map affordance state to a float affordance value. Can store an asset or a serialized value.")]
	private FloatAffordanceThemeDatumProperty m_AffordanceThemeDatum;

	[SerializeField]
	[Tooltip("The event that is called when the current affordance value is updated.")]
	private FloatUnityEvent m_ValueUpdated;

	public FloatAffordanceThemeDatumProperty affordanceThemeDatum
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

	public FloatUnityEvent valueUpdated
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

	protected override BaseAffordanceTheme<float> defaultAffordanceTheme
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

	protected override BindableVariable<float> affordanceValue { get; } = new BindableVariable<float>(0f);


	protected override JobHandle ScheduleTweenJob(ref TweenJobData<float> jobData)
	{
		FloatTweenJob jobData2 = default(FloatTweenJob);
		jobData2.jobData = jobData;
		return jobData2.Schedule();
	}

	protected override BaseAffordanceTheme<float> GenerateNewAffordanceThemeInstance()
	{
		return new FloatAffordanceTheme();
	}

	protected override void OnAffordanceValueUpdated(float newValue)
	{
		m_ValueUpdated?.Invoke(newValue);
	}
}
