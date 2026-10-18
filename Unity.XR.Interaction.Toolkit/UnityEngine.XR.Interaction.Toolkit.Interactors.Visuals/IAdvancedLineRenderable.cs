using Unity.Collections;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IAdvancedLineRenderable : ILineRenderable
{
	bool GetLinePoints(ref NativeArray<Vector3> linePoints, out int numPoints, Ray? rayOriginOverride = null);

	void GetLineOriginAndDirection(out Vector3 origin, out Vector3 direction);
}
