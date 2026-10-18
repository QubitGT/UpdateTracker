using Unity.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public interface ICurveInteractionDataProvider
{
	bool isActive { get; }

	bool hasValidSelect { get; }

	Transform curveOrigin { get; }

	NativeArray<Vector3> samplePoints { get; }

	Vector3 lastSamplePoint { get; }

	EndPointType TryGetCurveEndPoint(out Vector3 endPoint, bool snapToSelectedAttachIfAvailable = false, bool snapToSnapVolumeIfAvailable = false);

	EndPointType TryGetCurveEndNormal(out Vector3 endNormal, bool snapToSelectedAttachIfAvailable = false);
}
