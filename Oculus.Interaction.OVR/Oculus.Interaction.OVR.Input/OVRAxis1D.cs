using System;
using Meta.XR.Util;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.OVR.Input;

[Feature(Feature.Interaction)]
public class OVRAxis1D : MonoBehaviour, IAxis1D
{
	[Serializable]
	public class RemapConfig
	{
		public bool Enabled;

		public AnimationCurve Curve;
	}

	[SerializeField]
	private OVRInput.Controller _controller;

	[SerializeField]
	private OVRInput.Axis1D _axis1D;

	[SerializeField]
	private RemapConfig _remapConfig = new RemapConfig
	{
		Enabled = false,
		Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
	};

	public float Value()
	{
		float num = OVRInput.Get(_axis1D, _controller);
		if (_remapConfig.Enabled)
		{
			num = _remapConfig.Curve.Evaluate(num);
		}
		return num;
	}
}
