using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabUseInteractor : Interactor<HandGrabUseInteractor, HandGrabUseInteractable>, IHandGrabState
{
	[Tooltip("The hand to use.")]
	[SerializeField]
	[Optional]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("API that gets the finger use strength.")]
	[SerializeField]
	[Interface(typeof(IFingerUseAPI), new Type[] { })]
	private UnityEngine.Object _useAPI;

	private HandPose _relaxedHandPose = new HandPose();

	private HandPose _tightHandPose = new HandPose();

	private HandPose _cachedRelaxedHandPose = new HandPose();

	private HandPose _cachedTightHandPose = new HandPose();

	private HandFingerFlags _fingersInUse;

	private float[] _fingerUseStrength = new float[5];

	private bool _usesHandPose;

	private bool _handUseShouldSelect;

	private bool _handUseShouldUnselect;

	public IHand Hand { get; private set; }

	public IFingerUseAPI UseAPI { get; private set; }

	public HandGrabTarget HandGrabTarget { get; } = new HandGrabTarget();


	public bool IsGrabbing => base.SelectedInteractable != null;

	public float WristStrength => 0f;

	public float FingersStrength
	{
		get
		{
			if (!IsGrabbing)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public Pose WristToGrabPoseOffset => Pose.identity;

	public Action<IHandGrabState> WhenHandGrabStarted { get; set; } = delegate
	{
	};


	public Action<IHandGrabState> WhenHandGrabEnded { get; set; } = delegate
	{
	};


	protected override bool ComputeShouldSelect()
	{
		return _handUseShouldSelect;
	}

	protected override bool ComputeShouldUnselect()
	{
		if (!_handUseShouldUnselect)
		{
			return base.SelectedInteractable == null;
		}
		return true;
	}

	protected override void Awake()
	{
		base.Awake();
		Hand = _hand as IHand;
		UseAPI = _useAPI as IFingerUseAPI;
		_nativeId = 5208257256664429413uL;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void InteractableSelected(HandGrabUseInteractable interactable)
	{
		base.InteractableSelected(interactable);
		StartUsing();
	}

	protected override void InteractableUnselected(HandGrabUseInteractable interactable)
	{
		base.InteractableUnselected(interactable);
		_fingersInUse = HandFingerFlags.None;
	}

	private void StartUsing()
	{
		HandGrabResult handGrabResult = new HandGrabResult
		{
			HasHandPose = true,
			HandPose = _relaxedHandPose
		};
		HandGrabTarget.Set(base.SelectedInteractable.transform, HandAlignType.AlignOnGrab, GrabTypeFlags.None, handGrabResult);
	}

	protected override void DoHoverUpdate()
	{
		base.DoHoverUpdate();
		_handUseShouldSelect = IsUsingInteractable(base.Interactable);
	}

	protected override void DoSelectUpdate()
	{
		base.DoSelectUpdate();
		if (!(base.SelectedInteractable == null))
		{
			float strength = CalculateUseStrength(ref _fingerUseStrength);
			float useProgress = base.SelectedInteractable.ComputeUseStrength(strength);
			_handUseShouldUnselect = !IsUsingInteractable(base.Interactable);
			if (_usesHandPose && !_handUseShouldUnselect)
			{
				MoveFingers(ref _fingerUseStrength, useProgress);
			}
		}
	}

	private bool IsUsingInteractable(HandGrabUseInteractable interactable)
	{
		if (interactable == null)
		{
			return false;
		}
		for (int i = 0; i < 5; i++)
		{
			HandFinger handFinger = (HandFinger)i;
			if (interactable.UseFingers[handFinger] != 0 && UseAPI.GetFingerUseStrength(handFinger) > interactable.StrengthDeadzone)
			{
				return true;
			}
		}
		return false;
	}

	private float CalculateUseStrength(ref float[] fingerUseStrength)
	{
		float num = 1f;
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < 5; i++)
		{
			HandFinger handFinger = (HandFinger)i;
			if (base.SelectedInteractable.UseFingers[handFinger] == FingerRequirement.Ignored)
			{
				fingerUseStrength[i] = 0f;
				continue;
			}
			float fingerUseStrength2 = UseAPI.GetFingerUseStrength(handFinger);
			fingerUseStrength[i] = Mathf.Clamp01((fingerUseStrength2 - base.SelectedInteractable.UseStrengthDeadZone) / (1f - base.SelectedInteractable.UseStrengthDeadZone));
			if (base.SelectedInteractable.UseFingers[handFinger] == FingerRequirement.Required)
			{
				flag = true;
				num = Mathf.Min(num, fingerUseStrength[i]);
			}
			else if (base.SelectedInteractable.UseFingers[handFinger] == FingerRequirement.Optional)
			{
				num2 = Mathf.Max(num2, fingerUseStrength[i]);
			}
			if (fingerUseStrength[i] > 0f)
			{
				MarkFingerInUse(handFinger);
			}
			else
			{
				UnmarkFingerInUse(handFinger);
			}
		}
		if (!flag)
		{
			return num2;
		}
		return num;
	}

	private void MoveFingers(ref float[] fingerUseProgress, float useProgress)
	{
		for (int i = 0; i < 5; i++)
		{
			HandFinger finger = (HandFinger)i;
			float t = Mathf.Min(useProgress, fingerUseProgress[i]);
			LerpFingerRotation(_relaxedHandPose.JointRotations, _tightHandPose.JointRotations, HandGrabTarget.HandPose.JointRotations, finger, t);
		}
	}

	private void MarkFingerInUse(HandFinger finger)
	{
		_fingersInUse |= (HandFingerFlags)(1 << (int)finger);
	}

	private void UnmarkFingerInUse(HandFinger finger)
	{
		_fingersInUse &= (HandFingerFlags)(~(1 << (int)finger));
	}

	private void LerpFingerRotation(Quaternion[] from, Quaternion[] to, Quaternion[] result, HandFinger finger, float t)
	{
		int[] array = FingersMetadata.FINGER_TO_JOINT_INDEX[(int)finger];
		foreach (int num in array)
		{
			result[num] = Quaternion.Slerp(from[num], to[num], t);
		}
	}

	public HandFingerFlags GrabbingFingers()
	{
		return _fingersInUse;
	}

	protected override HandGrabUseInteractable ComputeCandidate()
	{
		float num = float.NegativeInfinity;
		HandGrabUseInteractable result = null;
		_usesHandPose = false;
		foreach (HandGrabUseInteractable item in Interactable<HandGrabUseInteractor, HandGrabUseInteractable>.Registry.List(this))
		{
			item.FindBestHandPoses((Hand != null) ? Hand.Scale : 1f, ref _cachedRelaxedHandPose, ref _cachedTightHandPose, out var score);
			if (score > num)
			{
				num = score;
				result = item;
				_relaxedHandPose.CopyFrom(_cachedRelaxedHandPose);
				_tightHandPose.CopyFrom(_cachedTightHandPose);
				_usesHandPose = true;
			}
		}
		return result;
	}

	public void InjectAllHandGrabUseInteractor(IFingerUseAPI useApi)
	{
		InjectUseApi(useApi);
	}

	public void InjectUseApi(IFingerUseAPI useApi)
	{
		_useAPI = useApi as UnityEngine.Object;
		UseAPI = useApi;
	}

	public void InjectOptionalHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
