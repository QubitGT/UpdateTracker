using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class CustomMapReviveStation : MonoBehaviour
{
	[Tooltip("Sets the SFX, if any, that play when a player is revived.")]
	public AudioSource? audioSource;

	[Tooltip("Sets the particle effects, if any, that play when a player is revived.")]
	public ParticleSystem[]? particleEffects;

	[Tooltip("How long (in seconds) before the revive station can be used again. A value of 0 means it can always be used")]
	public double reviveCooldownSeconds;
}
