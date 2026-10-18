using System;
using Unity.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public interface IAsyncAffordanceStateReceiver : IAffordanceStateReceiver
{
	JobHandle HandleTween(float tweenTarget);

	void UpdateStateFromCompletedJob();
}
