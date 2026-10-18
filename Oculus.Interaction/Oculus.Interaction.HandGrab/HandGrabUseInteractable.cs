using System;
using System.Collections.Generic;
using Oculus.Interaction.GrabAPI;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabUseInteractable : Interactable<HandGrabUseInteractor, HandGrabUseInteractable>
{
	[SerializeField]
	[Interface(typeof(IHandGrabUseDelegate), new Type[] { })]
	[Optional(OptionalAttribute.Flag.DontHide)]
	private UnityEngine.Object _handUseDelegate;

	[SerializeField]
	private GrabbingRule _useFingers;

	[SerializeField]
	[Range(0f, 1f)]
	private float _strengthDeadzone = 0.2f;

	[SerializeField]
	[Optional(OptionalAttribute.Flag.DontHide)]
	private List<HandGrabPose> _relaxedHandGrabPoses = new List<HandGrabPose>();

	[SerializeField]
	[Optional(OptionalAttribute.Flag.DontHide)]
	private List<HandGrabPose> _tightHandGrabPoses = new List<HandGrabPose>();

	private IHandGrabUseDelegate HandUseDelegate { get; set; }

	public GrabbingRule UseFingers
	{
		get
		{
			return _useFingers;
		}
		set
		{
			_useFingers = value;
		}
	}

	public float StrengthDeadzone
	{
		get
		{
			return _strengthDeadzone;
		}
		set
		{
			_strengthDeadzone = value;
		}
	}

	public float UseProgress { get; private set; }

	public List<HandGrabPose> RelaxGrabPoints => _relaxedHandGrabPoses;

	public List<HandGrabPose> TightGrabPoints => _tightHandGrabPoses;

	public float UseStrengthDeadZone => _strengthDeadzone;

	protected virtual void Reset()
	{
		HandGrabInteractable componentInParent = GetComponentInParent<HandGrabInteractable>();
		if (componentInParent != null)
		{
			_relaxedHandGrabPoses = new List<HandGrabPose>(componentInParent.HandGrabPoses);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		HandUseDelegate = _handUseDelegate as IHandGrabUseDelegate;
	}

	protected override void SelectingInteractorAdded(HandGrabUseInteractor interactor)
	{
		base.SelectingInteractorAdded(interactor);
		HandUseDelegate?.BeginUse();
	}

	protected override void SelectingInteractorRemoved(HandGrabUseInteractor interactor)
	{
		base.SelectingInteractorRemoved(interactor);
		HandUseDelegate?.EndUse();
	}

	public float ComputeUseStrength(float strength)
	{
		UseProgress = ((HandUseDelegate != null) ? HandUseDelegate.ComputeUseStrength(strength) : strength);
		return UseProgress;
	}

	public bool FindBestHandPoses(float handScale, ref HandPose relaxedHandPose, ref HandPose tightHandPose, out float score)
	{
		if (FindScaledHandPose(_relaxedHandGrabPoses, handScale, ref relaxedHandPose) && FindScaledHandPose(_tightHandGrabPoses, handScale, ref tightHandPose))
		{
			score = 1f;
			return true;
		}
		score = 0f;
		return false;
	}

	private bool FindScaledHandPose(List<HandGrabPose> _handGrabPoses, float handScale, ref HandPose handPose)
	{
		if (_handGrabPoses.Count == 1 && _handGrabPoses[0].HandPose != null)
		{
			handPose.CopyFrom(_handGrabPoses[0].HandPose);
			return true;
		}
		if (_handGrabPoses.Count > 1)
		{
			GrabPoseFinder.FindInterpolationRange(handScale / base.transform.lossyScale.x, _handGrabPoses, out var from, out var to, out var t);
			if (from.HandPose != null && to.HandPose != null)
			{
				HandPose from2 = from.HandPose;
				HandPose to2 = to.HandPose;
				HandPose.Lerp(in from2, in to2, t, ref handPose);
				return true;
			}
			if (from.HandPose != null)
			{
				handPose.CopyFrom(from.HandPose);
				return true;
			}
			if (to.HandPose != null)
			{
				handPose.CopyFrom(to.HandPose);
				return true;
			}
			return false;
		}
		return false;
	}

	public void InjectOptionalForwardUseDelegate(IHandGrabUseDelegate useDelegate)
	{
		_handUseDelegate = useDelegate as UnityEngine.Object;
		HandUseDelegate = useDelegate;
	}

	public void InjectOptionalRelaxedHandGrabPoints(List<HandGrabPose> relaxedHandGrabPoints)
	{
		_relaxedHandGrabPoses = relaxedHandGrabPoints;
	}

	public void InjectOptionalTightHandGrabPoints(List<HandGrabPose> tightHandGrabPoints)
	{
		_tightHandGrabPoses = tightHandGrabPoints;
	}
}
