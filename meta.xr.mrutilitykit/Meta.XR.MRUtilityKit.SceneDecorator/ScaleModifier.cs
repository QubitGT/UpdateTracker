using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class ScaleModifier : Modifier
{
	[Serializable]
	public struct AxisParameters
	{
		[SerializeField]
		public Mask mask;

		[SerializeField]
		public float limitMin;

		[SerializeField]
		public float limitMax;

		[SerializeField]
		public float scale;

		[SerializeField]
		public float offset;
	}

	[SerializeField]
	private AxisParameters x = new AxisParameters
	{
		limitMin = float.NegativeInfinity,
		limitMax = float.PositiveInfinity,
		scale = 1f
	};

	[SerializeField]
	private AxisParameters y = new AxisParameters
	{
		limitMin = float.NegativeInfinity,
		limitMax = float.PositiveInfinity,
		scale = 1f
	};

	[SerializeField]
	private AxisParameters z = new AxisParameters
	{
		limitMin = float.NegativeInfinity,
		limitMax = float.PositiveInfinity,
		scale = 1f
	};

	public override void ApplyModifier(GameObject decorationGO, MRUKAnchor sceneAnchor, SceneDecoration sceneDecoration, Candidate candidate)
	{
		Vector3 localScale = decorationGO.transform.localScale;
		localScale.x *= x.mask.SampleMask(candidate, x.limitMin, x.limitMax, x.scale, x.offset);
		localScale.y *= y.mask.SampleMask(candidate, y.limitMin, y.limitMax, y.scale, y.offset);
		localScale.z *= z.mask.SampleMask(candidate, z.limitMin, z.limitMax, z.scale, z.offset);
		decorationGO.transform.localScale = localScale;
	}
}
