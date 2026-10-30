using TMPro;
using UnityEngine;

[AddComponentMenu("GorillaTag/Critters/Menagerie Slot")]
public class MenagerieSlot : MonoBehaviour
{
	public Transform critterMountPoint;

	public TMP_Text label;

	public MenagerieCritter critter;

	private void Reset()
	{
		critterMountPoint = base.transform;
	}
}
