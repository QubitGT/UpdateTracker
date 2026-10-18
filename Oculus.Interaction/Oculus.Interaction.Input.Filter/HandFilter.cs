using System.Linq;
using UnityEngine;

namespace Oculus.Interaction.Input.Filter;

public class HandFilter : Hand
{
	[Header("Settings", order = -1)]
	[Tooltip("Applies a One Euro Filter when filter parameters are provided")]
	[SerializeField]
	[Optional]
	private HandFilterParameterBlock _filterParameters;

	private readonly IOneEuroFilter<Quaternion> _rootRotFilter = OneEuroFilter.CreateQuaternion();

	private readonly IOneEuroFilter<Vector3> _rootPosFilter = OneEuroFilter.CreateVector3();

	private readonly IOneEuroFilter<Vector3>[] _jointPosFilter = new IOneEuroFilter<Vector3>[26];

	private readonly IOneEuroFilter<Quaternion>[] _jointRotFilter = new IOneEuroFilter<Quaternion>[26];

	private ShadowHand _shadowHand = new ShadowHand();

	protected virtual void Awake()
	{
		for (int i = 0; i < 26; i++)
		{
			_jointPosFilter[i] = OneEuroFilter.CreateVector3();
			_jointRotFilter[i] = OneEuroFilter.CreateQuaternion();
		}
	}

	protected override void Apply(HandDataAsset handDataAsset)
	{
		base.Apply(handDataAsset);
		if (handDataAsset.IsTracked && UpdateFilterParameters())
		{
			UpdateHandData(handDataAsset);
		}
	}

	protected bool UpdateFilterParameters()
	{
		if (_filterParameters == null)
		{
			return true;
		}
		_rootRotFilter.SetProperties(in _filterParameters.wristRotationParameters);
		_rootPosFilter.SetProperties(in _filterParameters.wristPositionParameters);
		for (int i = 0; i < 26; i++)
		{
			_jointRotFilter[i].SetProperties(in _filterParameters.fingerRotationParameters);
		}
		return true;
	}

	protected bool UpdateHandData(HandDataAsset handDataAsset)
	{
		if (_filterParameters == null)
		{
			return true;
		}
		float deltaTime = 1f / _filterParameters.frequency;
		Pose root = handDataAsset.Root;
		_shadowHand.FromJoints(handDataAsset.JointPoses.ToList(), flipHandedness: false);
		handDataAsset.Root = new Pose(_rootPosFilter.Step(root.position, deltaTime), _rootRotFilter.Step(root.rotation, deltaTime));
		for (int i = 0; i < 26; i++)
		{
			HandJointId handJointId = (HandJointId)i;
			Pose localPose = _shadowHand.GetLocalPose(handJointId);
			Quaternion rotation = _jointRotFilter[i].Step(localPose.rotation, deltaTime);
			localPose.rotation = rotation;
			_shadowHand.SetLocalPose(handJointId, localPose);
		}
		handDataAsset.JointPoses = _shadowHand.GetWorldPoses();
		for (int j = 0; j < 26; j++)
		{
			int num = (int)HandJointUtils.JointParentList[j];
			handDataAsset.Joints[j] = ((num < 0) ? Quaternion.identity : (Quaternion.Inverse(handDataAsset.JointPoses[num].rotation) * handDataAsset.JointPoses[j].rotation));
		}
		return true;
	}
}
