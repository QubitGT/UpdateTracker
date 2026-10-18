using System;
using UnityEngine;

namespace Meta.XR.BuildingBlocks;

public class PassthroughProjectionSurfaceBuildingBlock : MonoBehaviour
{
	public MeshFilter projectionObject;

	private void Start()
	{
		OVRPassthroughLayer[] array = UnityEngine.Object.FindObjectsByType<OVRPassthroughLayer>(FindObjectsSortMode.None);
		bool flag = false;
		OVRPassthroughLayer[] array2 = array;
		foreach (OVRPassthroughLayer oVRPassthroughLayer in array2)
		{
			if ((bool)oVRPassthroughLayer.GetComponent<BuildingBlock>())
			{
				flag = true;
				oVRPassthroughLayer.AddSurfaceGeometry(projectionObject.gameObject, updateTransform: true);
			}
		}
		if (flag)
		{
			projectionObject.GetComponent<MeshRenderer>().enabled = false;
			return;
		}
		throw new InvalidOperationException("A Building Block with the passthrough overlay layer was not found");
	}
}
