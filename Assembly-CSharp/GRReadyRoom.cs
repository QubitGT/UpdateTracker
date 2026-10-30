using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Ready Room")]
public class GRReadyRoom : MonoBehaviour
{
	public List<GRNameDisplayPlate> nameDisplayPlates;

	public void RefreshRigs(List<VRRig> vrRigs)
	{
		for (int i = 0; i < nameDisplayPlates.Count; i++)
		{
			if (nameDisplayPlates != null)
			{
				if (i < vrRigs.Count && vrRigs[i] != null && vrRigs[i].Creator != null)
				{
					nameDisplayPlates[i].RefreshPlayerName(vrRigs[i]);
				}
				else
				{
					nameDisplayPlates[i].Clear();
				}
			}
		}
	}
}
