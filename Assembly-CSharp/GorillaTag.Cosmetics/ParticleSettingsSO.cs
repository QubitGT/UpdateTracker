using UnityEngine;

namespace GorillaTag.Cosmetics;

[CreateAssetMenu(fileName = "Particle Settings", menuName = "ScriptableObjects/Visuals and Audio/ParticleSettings")]
public class ParticleSettingsSO : ScriptableObject
{
	public Color startColor;

	public float startSize;
}
