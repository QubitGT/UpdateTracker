using UnityEngine;

namespace Meta.XR;

internal interface IEnvironmentRaycastProvider
{
	bool IsSupported { get; }

	bool IsReady { get; }

	void SetEnabled(bool isEnabled);

	bool Raycast(Ray ray, out EnvironmentRaycastHit hit, float maxDistance = 100f, bool reconstructNormal = true, bool allowOccludedRayOrigin = true);
}
