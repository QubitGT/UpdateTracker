using UnityEngine;

[CreateAssetMenu(fileName = "DayCycleTextures", menuName = "GorillaTag/Day Cycle Textures", order = 0)]
public class DayCycleTexturesSO : ScriptableObject
{
	public DayCycleTextureMoment[] moments = new DayCycleTextureMoment[10];
}
