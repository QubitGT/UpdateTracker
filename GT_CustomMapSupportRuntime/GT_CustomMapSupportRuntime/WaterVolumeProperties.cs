using System.Collections.Generic;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public struct WaterVolumeProperties
{
	public Transform? surfacePlane;

	public List<MeshCollider> surfaceColliders;

	public CMSZoneShaderSettings.EZoneLiquidType liquidType;
}
