using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ShoulderEstimatePosition : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private static readonly Vector3 ShoulderOffset = new Vector3(0.13f, -0.25f, -0.13f);

	protected bool _started;

	private IHmd Hmd { get; set; }

	private IHand Hand { get; set; }

	protected virtual void Awake()
	{
		Hmd = _hmd as IHmd;
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
			Hmd.WhenUpdated += HandleHmdUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hmd.WhenUpdated -= HandleHmdUpdated;
		}
	}

	protected virtual void HandleHmdUpdated()
	{
		Hmd.TryGetRootPose(out var pose);
		Quaternion quaternion = Quaternion.Euler(0f, pose.rotation.eulerAngles.y, 0f);
		Vector3 vector = ShoulderOffset * Hand.Scale;
		if (Hand.Handedness == Handedness.Left)
		{
			vector.x = 0f - vector.x;
		}
		Vector3 position = pose.position + quaternion * vector;
		base.transform.SetPositionAndRotation(position, quaternion);
	}

	public void InjectAllShoulderPosition(IHmd hmd, IHand hand)
	{
		InjectHmd(hmd);
		InjectHand(hand);
	}

	public void InjectHmd(IHmd hmd)
	{
		_hmd = hmd as UnityEngine.Object;
		Hmd = hmd;
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
