using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.HandGrab.Visuals;

[RequireComponent(typeof(HandPuppet))]
public class HandGhost : MonoBehaviour
{
	[SerializeField]
	private HandPuppet _puppet;

	[SerializeField]
	[Optional]
	private Transform _root;

	[SerializeField]
	[Optional]
	[FormerlySerializedAs("_handGrabPoint")]
	private HandGrabPose _handGrabPose;

	public Transform Root => _root;

	protected virtual void Reset()
	{
		_puppet = GetComponent<HandPuppet>();
		_handGrabPose = GetComponentInParent<HandGrabPose>();
	}

	protected virtual void OnValidate()
	{
		if (_puppet == null)
		{
			return;
		}
		if (_handGrabPose == null)
		{
			HandGrabPose componentInParent = GetComponentInParent<HandGrabPose>();
			if (componentInParent != null)
			{
				SetPose(componentInParent);
			}
		}
		else if (_handGrabPose != null)
		{
			SetPose(_handGrabPose);
		}
	}

	protected virtual void Start()
	{
		if (_root == null)
		{
			_root = base.transform;
		}
	}

	public void SetPose(HandGrabPose handGrabPose)
	{
		HandPose handPose = handGrabPose.HandPose;
		if (handPose != null)
		{
			HandPuppet puppet = _puppet;
			Quaternion[] jointRotations = handPose.JointRotations;
			puppet.SetJointRotations(in jointRotations);
			SetRootPose(handGrabPose.RelativePose, handGrabPose.RelativeTo);
		}
	}

	public void SetPose(HandPose userPose, Pose rootPose)
	{
		HandPuppet puppet = _puppet;
		Quaternion[] jointRotations = userPose.JointRotations;
		puppet.SetJointRotations(in jointRotations);
		_puppet.SetRootPose(in rootPose);
	}

	public void SetRootPose(Pose rootPose, Transform relativeTo)
	{
		Pose rootPose2 = rootPose;
		if (relativeTo != null)
		{
			rootPose2 = PoseUtils.GlobalPoseScaled(relativeTo, rootPose);
		}
		_puppet.SetRootPose(in rootPose2);
	}

	public void InjectAllHandGhost(HandPuppet puppet)
	{
		InjectHandPuppet(puppet);
	}

	public void InjectHandPuppet(HandPuppet puppet)
	{
		_puppet = puppet;
	}

	public void InjectOptionalHandGrabPose(HandGrabPose handGrabPose)
	{
		_handGrabPose = handGrabPose;
	}

	public void InjectOptionalRoot(Transform root)
	{
		_root = root;
	}
}
