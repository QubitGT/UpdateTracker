using System;
using Unity.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class FloatTweenableVariable : TweenableVariableAsyncBase<float>
{
	protected override JobHandle ScheduleTweenJob(ref TweenJobData<float> jobData)
	{
		FloatTweenJob jobData2 = default(FloatTweenJob);
		jobData2.jobData = jobData;
		return jobData2.Schedule();
	}
}
