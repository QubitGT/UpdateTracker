using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public struct MinMaxPair
{
	[SerializeField]
	private bool _useRandomRange;

	[SerializeField]
	private float _min;

	[SerializeField]
	private float _max;

	public bool UseRandomRange => _useRandomRange;

	public float Min => _min;

	public float Max => _max;
}
