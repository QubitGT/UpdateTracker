using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRScaleValueProvider
{
	ScaleMode scaleMode { get; set; }

	float scaleValue { get; }
}
