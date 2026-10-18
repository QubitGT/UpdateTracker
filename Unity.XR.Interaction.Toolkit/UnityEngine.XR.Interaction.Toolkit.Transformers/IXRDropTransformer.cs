using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

public interface IXRDropTransformer : IXRGrabTransformer
{
	bool canProcessOnDrop { get; }

	void OnDrop(XRGrabInteractable grabInteractable, DropEventArgs args);
}
