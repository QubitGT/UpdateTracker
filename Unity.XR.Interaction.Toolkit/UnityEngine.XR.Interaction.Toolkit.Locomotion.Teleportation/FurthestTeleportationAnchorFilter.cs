using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[CreateAssetMenu(fileName = "FurthestTeleportationAnchorFilter", menuName = "XR/Locomotion/Furthest Teleportation Anchor Filter")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.FurthestTeleportationAnchorFilter.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class FurthestTeleportationAnchorFilter : ScriptableObject, ITeleportationVolumeAnchorFilter
{
	public int GetDestinationAnchorIndex(TeleportationMultiAnchorVolume teleportationVolume)
	{
		int result = -1;
		float num = -1f;
		Vector3 cameraFloorWorldPosition = teleportationVolume.teleportationProvider.mediator.xrOrigin.GetCameraFloorWorldPosition();
		List<Transform> anchorTransforms = teleportationVolume.anchorTransforms;
		for (int i = 0; i < anchorTransforms.Count; i++)
		{
			float sqrMagnitude = (anchorTransforms[i].position - cameraFloorWorldPosition).sqrMagnitude;
			if (sqrMagnitude > num)
			{
				result = i;
				num = sqrMagnitude;
			}
		}
		return result;
	}
}
