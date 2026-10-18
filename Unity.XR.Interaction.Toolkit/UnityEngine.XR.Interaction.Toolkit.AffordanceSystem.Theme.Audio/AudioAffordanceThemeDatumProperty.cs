using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Audio;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class AudioAffordanceThemeDatumProperty : DatumProperty<AudioAffordanceTheme, AudioAffordanceThemeDatum>
{
	public AudioAffordanceThemeDatumProperty(AudioAffordanceTheme value)
		: base(value)
	{
	}

	public AudioAffordanceThemeDatumProperty(AudioAffordanceThemeDatum datum)
		: base(datum)
	{
	}
}
