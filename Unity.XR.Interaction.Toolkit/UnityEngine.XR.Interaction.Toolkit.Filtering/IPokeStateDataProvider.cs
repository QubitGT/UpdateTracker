using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State")]
public interface IPokeStateDataProvider
{
	IReadOnlyBindableVariable<PokeStateData> pokeStateData { get; }
}
