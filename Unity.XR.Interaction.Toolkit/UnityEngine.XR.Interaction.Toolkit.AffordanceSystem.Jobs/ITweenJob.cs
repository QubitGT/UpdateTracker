using System;
using Unity.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public interface ITweenJob<T> : IJob where T : struct
{
	TweenJobData<T> jobData { get; set; }

	T Lerp(T from, T to, float t);

	bool IsNearlyEqual(T from, T to);
}
