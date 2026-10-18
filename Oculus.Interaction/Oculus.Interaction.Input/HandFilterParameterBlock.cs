using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Serializable]
[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Input/Hand Filter Parameters")]
public class HandFilterParameterBlock : ScriptableObject
{
	[Header("One Euro Filter Parameters")]
	[SerializeField]
	[Tooltip("Smoothing for wrist position")]
	public OneEuroFilterPropertyBlock wristPositionParameters = new OneEuroFilterPropertyBlock
	{
		_beta = 5f,
		_minCutoff = 0.5f,
		_dCutoff = 1f
	};

	[SerializeField]
	[Tooltip("Smoothing for wrist rotation")]
	public OneEuroFilterPropertyBlock wristRotationParameters = new OneEuroFilterPropertyBlock
	{
		_beta = 5f,
		_minCutoff = 0.5f,
		_dCutoff = 1f
	};

	[SerializeField]
	[Tooltip("Smoothing for finger rotation")]
	public OneEuroFilterPropertyBlock fingerRotationParameters = new OneEuroFilterPropertyBlock
	{
		_beta = 1f,
		_minCutoff = 0.5f,
		_dCutoff = 1f
	};

	[SerializeField]
	[Tooltip("Frequency (in frames per sec)")]
	public float frequency = 72f;
}
