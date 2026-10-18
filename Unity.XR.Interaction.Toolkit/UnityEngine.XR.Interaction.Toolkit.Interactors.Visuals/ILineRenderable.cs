using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface ILineRenderable
{
	bool GetLinePoints(ref Vector3[] linePoints, out int numPoints);

	bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget);
}
