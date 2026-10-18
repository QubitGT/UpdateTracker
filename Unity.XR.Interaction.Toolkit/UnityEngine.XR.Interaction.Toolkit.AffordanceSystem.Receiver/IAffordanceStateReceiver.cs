using System;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public interface IAffordanceStateReceiver
{
	IReadOnlyBindableVariable<AffordanceStateData> currentAffordanceStateData { get; }

	void OnAffordanceStateUpdated(AffordanceStateData previousState, AffordanceStateData newState);
}
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public interface IAffordanceStateReceiver<T> : IAffordanceStateReceiver where T : struct
{
	BaseAffordanceTheme<T> affordanceTheme { get; }

	IReadOnlyBindableVariable<T> currentAffordanceValue { get; }
}
