using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class HandPhysicsCapsules : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHandVisual), new Type[] { })]
	[Obsolete("Replaced by _hand")]
	private UnityEngine.Object _handVisual;

	[Obsolete("Replaced by Hand")]
	private IHandVisual HandVisual;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IHand Hand;

	[Tooltip("Indicates how \"thick\" the fingers are at each bone. This information creates a capsule collider that wraps the bones accurately.")]
	[SerializeField]
	private JointsRadiusFeature _jointsRadiusFeature;

	[Space]
	[SerializeField]
	[Tooltip("If  checked, capsules will be generated as triggers.")]
	private bool _asTriggers;

	[SerializeField]
	[Tooltip("Capsules will be generated in this layer. The default layer is 0.")]
	private int _useLayer;

	[SerializeField]
	[Tooltip("A joint. Capsules reaching this joint will not be generated.")]
	private HandFingerJointFlags _mask = HandFingerJointFlags.All;

	private Action _whenCapsulesGenerated = delegate
	{
	};

	private Transform _rootTransform;

	private List<BoneCapsule> _capsules;

	private Rigidbody[] _rigidbodies;

	private bool _capsulesAreActive;

	private bool _capsulesGenerated;

	protected bool _started;

	public Transform RootTransform => _rootTransform;

	public IList<BoneCapsule> Capsules { get; private set; }

	public event Action WhenCapsulesGenerated
	{
		add
		{
			_whenCapsulesGenerated = (Action)Delegate.Combine(_whenCapsulesGenerated, value);
			if (_capsulesGenerated)
			{
				value();
			}
		}
		remove
		{
			_whenCapsulesGenerated = (Action)Delegate.Remove(_whenCapsulesGenerated, value);
		}
	}

	protected virtual void Reset()
	{
		_useLayer = base.gameObject.layer;
	}

	protected virtual void Awake()
	{
		HandVisual = _handVisual as IHandVisual;
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (Hand == null && HandVisual != null)
		{
			Hand = HandVisual.Hand;
		}
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_capsulesAreActive = true;
			Hand.WhenHandUpdated += HandleHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
			DisableRigidbodies();
		}
	}

	private void GenerateCapsules()
	{
		if (!Hand.IsTrackedDataValid)
		{
			return;
		}
		_rigidbodies = new Rigidbody[26];
		Transform transform = new GameObject("Capsules").transform;
		transform.SetParent(base.transform, worldPositionStays: false);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.gameObject.layer = _useLayer;
		int capacity = 26;
		_capsules = new List<BoneCapsule>(capacity);
		Capsules = _capsules.AsReadOnly();
		for (int i = 2; i < 26; i++)
		{
			HandJointId handJointId = (HandJointId)i;
			HandJointId handJointId2 = HandJointUtils.JointParentList[i];
			if (handJointId2 != HandJointId.Invalid && ((uint)(1 << (int)handJointId) & (uint)_mask) != 0)
			{
				Hand.GetJointPose(handJointId2, out var pose);
				if (!TryGetJointRigidbody(handJointId2, out var body))
				{
					body = CreateJointRigidbody(handJointId2, transform, pose);
					_rigidbodies[(int)handJointId2] = body;
				}
				string text = $"{handJointId2}-{handJointId} CapsuleCollider";
				float jointRadius = _jointsRadiusFeature.GetJointRadius(handJointId2);
				float offset = (HandJointUtils.IsFingerTip(handJointId) ? (0f - jointRadius) : ((handJointId2 == HandJointId.HandStart) ? jointRadius : 0f));
				Hand.GetJointPose(handJointId, out var pose2);
				CapsuleCollider collider = CreateCollider(text, body.transform, pose.position, pose2.position, jointRadius, offset);
				BoneCapsule item = new BoneCapsule(handJointId2, handJointId, body, collider);
				_capsules.Add(item);
			}
		}
		IgnoreSelfCollisions();
		_capsulesGenerated = true;
		_whenCapsulesGenerated();
	}

	private void IgnoreSelfCollisions()
	{
		for (int i = 0; i < _capsules.Count; i++)
		{
			for (int j = i + 1; j < _capsules.Count; j++)
			{
				Physics.IgnoreCollision(_capsules[i].CapsuleCollider, _capsules[j].CapsuleCollider);
			}
		}
	}

	private bool TryGetJointRigidbody(HandJointId joint, out Rigidbody body)
	{
		if (_rigidbodies == null || joint < HandJointId.HandStart || (int)joint >= _rigidbodies.Length)
		{
			body = null;
			return false;
		}
		body = _rigidbodies[(int)joint];
		return body != null;
	}

	private Rigidbody CreateJointRigidbody(HandJointId joint, Transform holder, Pose pose)
	{
		Rigidbody rigidbody = new GameObject($"{joint} Rigidbody").AddComponent<Rigidbody>();
		rigidbody.mass = 1f;
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		rigidbody.transform.SetParent(holder, worldPositionStays: false);
		rigidbody.transform.SetPose(in pose);
		rigidbody.Sleep();
		rigidbody.gameObject.SetActive(value: false);
		rigidbody.gameObject.layer = _useLayer;
		return rigidbody;
	}

	private CapsuleCollider CreateCollider(string name, Transform holder, Vector3 from, Vector3 to, float radius, float offset)
	{
		CapsuleCollider capsuleCollider = new GameObject(name).AddComponent<CapsuleCollider>();
		capsuleCollider.isTrigger = _asTriggers;
		Vector3 forward = to - from;
		Quaternion rotation = Quaternion.LookRotation(forward);
		float num = forward.magnitude - Mathf.Abs(offset);
		capsuleCollider.radius = radius;
		capsuleCollider.height = num + radius * 2f;
		capsuleCollider.direction = 2;
		capsuleCollider.center = Vector3.forward * (num * 0.5f + Mathf.Max(0f, offset));
		Transform obj = capsuleCollider.transform;
		obj.SetParent(holder, worldPositionStays: false);
		obj.SetPositionAndRotation(from, rotation);
		capsuleCollider.gameObject.layer = _useLayer;
		return capsuleCollider;
	}

	private void DisableRigidbodies()
	{
		if (!_capsulesAreActive)
		{
			return;
		}
		for (HandJointId handJointId = HandJointId.HandStart; handJointId < HandJointId.HandEnd; handJointId++)
		{
			if (TryGetJointRigidbody(handJointId, out var body))
			{
				body.Sleep();
				body.gameObject.SetActive(value: false);
			}
		}
		_capsulesAreActive = false;
	}

	private void HandleHandUpdated()
	{
		if (!_capsulesGenerated)
		{
			GenerateCapsules();
		}
		if (_capsulesGenerated)
		{
			UpdateRigidbodies();
			UpdateColliders();
		}
	}

	private void UpdateColliders()
	{
		foreach (BoneCapsule capsule in _capsules)
		{
			capsule.CapsuleCollider.radius = _jointsRadiusFeature.GetJointRadius(capsule.StartJoint);
		}
	}

	private void UpdateRigidbodies()
	{
		for (HandJointId handJointId = HandJointId.HandStart; handJointId < HandJointId.HandEnd; handJointId++)
		{
			if (!TryGetJointRigidbody(handJointId, out var body))
			{
				continue;
			}
			GameObject gameObject = body.gameObject;
			if (_capsulesAreActive && Hand.GetJointPose(handJointId, out var pose))
			{
				if (!gameObject.activeSelf)
				{
					Rigidbody rigidbody = body;
					Vector3 position2 = (gameObject.transform.position = pose.position);
					rigidbody.position = position2;
					Rigidbody rigidbody2 = body;
					Quaternion rotation2 = (gameObject.transform.rotation = pose.rotation);
					rigidbody2.rotation = rotation2;
					gameObject.SetActive(value: true);
					body.WakeUp();
				}
				else
				{
					body.MovePosition(pose.position);
					body.MoveRotation(pose.rotation);
				}
			}
			else if (gameObject.activeSelf)
			{
				body.Sleep();
				gameObject.SetActive(value: false);
			}
		}
	}

	public void InjectAllOVRHandPhysicsCapsules(IHand hand, bool asTriggers, int useLayer)
	{
		InjectHand(hand);
		InjectAsTriggers(asTriggers);
		InjectUseLayer(useLayer);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectAsTriggers(bool asTriggers)
	{
		_asTriggers = asTriggers;
	}

	public void InjectUseLayer(int useLayer)
	{
		_useLayer = useLayer;
	}

	public void InjectMask(HandFingerJointFlags mask)
	{
		_mask = mask;
	}

	public void InjectJointsRadiusFeature(JointsRadiusFeature jointsRadiusFeature)
	{
		_jointsRadiusFeature = jointsRadiusFeature;
	}
}
