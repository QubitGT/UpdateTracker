using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class ColliderContainsHandJointActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IHand Hand;

	[SerializeField]
	private Collider[] _entryColliders;

	[SerializeField]
	private Collider[] _exitColliders;

	[SerializeField]
	private HandJointId _jointToTest = HandJointId.HandWristRoot;

	private bool _active;

	public bool Active { get; private set; }

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		Active = false;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if (Hand.GetJointPose(_jointToTest, out var pose))
		{
			Active = JointPassesTests(pose);
		}
		else
		{
			Active = false;
		}
	}

	private bool JointPassesTests(Pose jointPose)
	{
		return _active = ((!_active) ? IsPointWithinColliders(jointPose.position, _entryColliders) : IsPointWithinColliders(jointPose.position, _exitColliders));
	}

	private bool IsPointWithinColliders(Vector3 point, Collider[] colliders)
	{
		foreach (Collider collider in colliders)
		{
			if (!Collisions.IsPointWithinCollider(point, collider))
			{
				return false;
			}
		}
		return true;
	}

	public void InjectAllColliderContainsHandJointActiveState(IHand hand, Collider[] entryColliders, Collider[] exitColliders, HandJointId jointToTest)
	{
		InjectHand(hand);
		InjectEntryColliders(entryColliders);
		InjectExitColliders(exitColliders);
		InjectJointToTest(jointToTest);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectEntryColliders(Collider[] entryColliders)
	{
		_entryColliders = entryColliders;
	}

	public void InjectExitColliders(Collider[] exitColliders)
	{
		_exitColliders = exitColliders;
	}

	public void InjectJointToTest(HandJointId jointToTest)
	{
		_jointToTest = jointToTest;
	}
}
