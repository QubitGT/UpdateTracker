using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[AddComponentMenu("Affordance System/Receiver/Primitives/Vector3 Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.Vector3AffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector3AffordanceReceiver : BaseAsyncAffordanceStateReceiver<float3>
{
	[SerializeField]
	[Tooltip("Vector3 Affordance Theme datum property used to map affordance state to a Vector3 affordance value. Can store an asset or a serialized value.")]
	private Vector3AffordanceThemeDatumProperty m_AffordanceThemeDatum;

	[SerializeField]
	[Tooltip("The event that is called when the current affordance value is updated.")]
	private Vector3UnityEvent m_ValueUpdated;

	public Vector3AffordanceThemeDatumProperty affordanceThemeDatum
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

	public Vector3UnityEvent valueUpdated
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

	protected override BaseAffordanceTheme<float3> defaultAffordanceTheme
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

	protected override BindableVariable<float3> affordanceValue { get; } = new BindableVariable<float3>();


	protected override JobHandle ScheduleTweenJob(ref TweenJobData<float3> jobData)
	{
		Float3TweenJob jobData2 = default(Float3TweenJob);
		jobData2.jobData = jobData;
		return jobData2.Schedule();
	}

	protected override BaseAffordanceTheme<float3> GenerateNewAffordanceThemeInstance()
	{
		return new Vector3AffordanceTheme();
	}

	protected override void OnAffordanceValueUpdated(float3 newValue)
	{
		m_ValueUpdated?.Invoke(newValue);
	}
}
