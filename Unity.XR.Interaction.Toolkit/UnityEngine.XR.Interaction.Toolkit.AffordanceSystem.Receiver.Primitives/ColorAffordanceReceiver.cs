using System;
using Unity.Jobs;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

[AddComponentMenu("Affordance System/Receiver/Primitives/Color Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.ColorAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class ColorAffordanceReceiver : BaseAsyncAffordanceStateReceiver<Color>
{
	[SerializeField]
	[Tooltip("Color Affordance Theme datum property used to map affordance state to a color affordance value. Can store an asset or a serialized value.")]
	private ColorAffordanceThemeDatumProperty m_AffordanceThemeDatum;

	[SerializeField]
	[Tooltip("The event that is called when the current affordance value is updated.")]
	private ColorUnityEvent m_ValueUpdated;

	public ColorAffordanceThemeDatumProperty affordanceThemeDatum
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

	public ColorUnityEvent valueUpdated
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

	protected override BaseAffordanceTheme<Color> defaultAffordanceTheme
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

	protected override BindableVariable<Color> affordanceValue { get; } = new BindableVariable<Color>();


	protected override JobHandle ScheduleTweenJob(ref TweenJobData<Color> jobData)
	{
		ColorAffordanceTheme colorAffordanceTheme = (ColorAffordanceTheme)base.affordanceTheme;
		ColorTweenJob jobData2 = default(ColorTweenJob);
		jobData2.jobData = jobData;
		jobData2.colorBlendAmount = colorAffordanceTheme.blendAmount;
		jobData2.colorBlendMode = (byte)colorAffordanceTheme.colorBlendMode;
		return jobData2.Schedule();
	}

	protected override BaseAffordanceTheme<Color> GenerateNewAffordanceThemeInstance()
	{
		return new ColorAffordanceTheme();
	}

	protected override void OnAffordanceValueUpdated(Color newValue)
	{
		m_ValueUpdated?.Invoke(newValue);
	}
}
