using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public struct ImpactAudio
{
	[Tooltip("Hard collision sound will play when impact velocity is above the velocity split value.")]
	[SerializeField]
	private AudioTrigger _hardCollisionSound;

	[Tooltip("Soft collision sound will play when impact velocity is below the velocity split value.")]
	[SerializeField]
	private AudioTrigger _softCollisionSound;

	public AudioTrigger HardCollisionSound => _hardCollisionSound;

	public AudioTrigger SoftCollisionSound => _softCollisionSound;
}
