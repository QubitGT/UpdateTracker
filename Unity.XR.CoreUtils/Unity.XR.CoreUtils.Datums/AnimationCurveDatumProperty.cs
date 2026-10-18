using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums;

[Serializable]
public class AnimationCurveDatumProperty : DatumProperty<AnimationCurve, AnimationCurveDatum>
{
	public AnimationCurveDatumProperty(AnimationCurve value)
		: base(value)
	{
	}

	public AnimationCurveDatumProperty(AnimationCurveDatum datum)
		: base(datum)
	{
	}
}
