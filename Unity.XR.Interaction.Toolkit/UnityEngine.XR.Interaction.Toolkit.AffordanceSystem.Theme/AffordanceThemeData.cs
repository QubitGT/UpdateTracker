using System;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public sealed class AffordanceThemeData<T>
{
	[Tooltip("Name of the affordance state the theme data is for.\nThis value is optional and does not serve a functional purpose.")]
	public string stateName;

	[Tooltip("Target value for the curve at 0")]
	public T animationStateStartValue;

	[Tooltip("Target value for the curve at 1.")]
	public T animationStateEndValue;
}
