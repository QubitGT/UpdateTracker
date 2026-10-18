using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Samples.PalmMenu;

[Obsolete("Use a combination of DominantHandRef and HandJoint instead")]
public class MatchNonDominantPalmWorldSpaceTransform : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _leftHand;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _rightHand;

	[SerializeField]
	private Vector3 _leftAnchorPoint = new Vector3(-0.060860332f, 0.0095398445f, 0.0002581277f);

	[SerializeField]
	private Vector3 _leftAimPoint = new Vector3(-0.07492584f, 0.08930927f, 0.0002581277f);

	[SerializeField]
	private Vector3 _rightAnchorPoint = new Vector3(0.065260336f, -0.011439844f, -0.004558128f);

	[SerializeField]
	private Vector3 _rightAimPoint = new Vector3(0.07932585f, -0.09120928f, -0.004558128f);

	private IHand LeftHand { get; set; }

	private IHand RightHand { get; set; }

	protected virtual void Awake()
	{
		LeftHand = _leftHand as IHand;
		RightHand = _rightHand as IHand;
	}

	private void Update()
	{
		Vector3 position = (LeftHand.IsDominantHand ? _rightAnchorPoint : _leftAnchorPoint);
		Vector3 position2 = (LeftHand.IsDominantHand ? _rightAimPoint : _leftAimPoint);
		if ((LeftHand.IsDominantHand ? RightHand : LeftHand).GetJointPose(HandJointId.HandWristRoot, out var pose))
		{
			Pose transformedBy = new Pose(position, Quaternion.identity).GetTransformedBy(pose);
			Pose transformedBy2 = new Pose(position2, Quaternion.identity).GetTransformedBy(pose);
			base.transform.SetPositionAndRotation(transformedBy.position, Quaternion.LookRotation((transformedBy2.position - transformedBy.position).normalized));
		}
	}
}
