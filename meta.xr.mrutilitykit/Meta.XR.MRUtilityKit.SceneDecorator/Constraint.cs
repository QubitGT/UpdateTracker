using System;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Serializable]
public struct Constraint
{
	[SerializeField]
	public string name;

	[SerializeField]
	public bool enabled;

	[SerializeField]
	public Mask mask;

	[SerializeField]
	public ConstraintModeCheck modeCheck;

	[SerializeField]
	public float min;

	[SerializeField]
	public float max;
}
