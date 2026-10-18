using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandPointerPose : MonoBehaviour, IActiveState
{
	[Tooltip("The hand used for ray interaction")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("How much the ray origin is offset relative to the hand.")]
	[SerializeField]
	private Vector3 _offset;

	protected bool _started;

	public IHand Hand { get; private set; }

	public bool Active => Hand.IsPointerPoseValid;

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += HandleHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
		}
	}

	private void HandleHandUpdated()
	{
		if (Hand.GetPointerPose(out var pose))
		{
			pose.position += pose.rotation * _offset;
			base.transform.SetPose(in pose);
		}
	}

	public void InjectAllHandPointerPose(IHand hand, Vector3 offset)
	{
		InjectHand(hand);
		InjectOffset(offset);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectOffset(Vector3 offset)
	{
		_offset = offset;
	}
}
