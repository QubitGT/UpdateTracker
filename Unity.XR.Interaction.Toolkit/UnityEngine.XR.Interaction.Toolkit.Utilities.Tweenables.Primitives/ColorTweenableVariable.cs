using System;
using Unity.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class ColorTweenableVariable : TweenableVariableAsyncBase<Color>
{
	protected override JobHandle ScheduleTweenJob(ref TweenJobData<Color> jobData)
	{
		ColorTweenJob jobData2 = default(ColorTweenJob);
		jobData2.jobData = jobData;
		jobData2.colorBlendAmount = 1f;
		jobData2.colorBlendMode = 0;
		return jobData2.Schedule();
	}
}
