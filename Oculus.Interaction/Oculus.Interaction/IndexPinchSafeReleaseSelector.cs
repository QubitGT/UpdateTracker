using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class IndexPinchSafeReleaseSelector : MonoBehaviour, ISelector, IActiveState
{
	[Tooltip("The hand to check.")]
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[Tooltip("If checked, the selector will select during the frame when the pinch is released as opposed to when it's pinching.")]
	[SerializeField]
	private bool _selectOnRelease = true;

	[Tooltip("Indicates how extended the index needs to be in order to be safe to unpinch.")]
	[SerializeField]
	[Range(-1f, 1f)]
	private float _safeReleaseThreshold = 0.5f;

	private bool _wasPinching;

	private bool _active;

	private bool _pendingUnselect;

	protected bool _started;

	public IHand Hand { get; private set; }

	public bool SelectOnRelease
	{
		get
		{
			return _selectOnRelease;
		}
		set
		{
			_selectOnRelease = value;
		}
	}

	public float SafeReleaseThreshold
	{
		get
		{
			return _safeReleaseThreshold;
		}
		set
		{
			_safeReleaseThreshold = value;
		}
	}

	public bool Active => _active;

	public event Action WhenSelected = delegate
	{
	};

	public event Action WhenUnselected = delegate
	{
	};

	protected virtual void Awake()
	{
		if (Hand == null)
		{
			Hand = _hand as IHand;
		}
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
			_wasPinching = Hand.GetIndexFingerIsPinching();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
			if (_active)
			{
				_active = false;
				this.WhenUnselected();
			}
			_pendingUnselect = false;
		}
	}

	private void HandleHandUpdated()
	{
		if (_selectOnRelease && _pendingUnselect)
		{
			_pendingUnselect = false;
			this.WhenUnselected();
		}
		bool indexFingerIsPinching = Hand.GetIndexFingerIsPinching();
		if (_wasPinching != indexFingerIsPinching)
		{
			_wasPinching = indexFingerIsPinching;
			if (indexFingerIsPinching)
			{
				_active = true;
				if (!_selectOnRelease)
				{
					this.WhenSelected();
				}
			}
		}
		if (_active && !indexFingerIsPinching && IsIndexExtended())
		{
			if (_selectOnRelease)
			{
				this.WhenSelected();
				_pendingUnselect = true;
			}
			else
			{
				this.WhenUnselected();
			}
			_active = false;
		}
	}

	protected virtual bool IsIndexExtended()
	{
		if (Hand.GetFingerPinchStrength(HandFinger.Index) == 0f)
		{
			return true;
		}
		if (!Hand.GetJointPoseFromWrist(HandJointId.HandIndex1, out var pose) || !Hand.GetJointPoseFromWrist(HandJointId.HandIndex2, out var pose2) || !Hand.GetJointPoseFromWrist(HandJointId.HandIndexTip, out var pose3))
		{
			return true;
		}
		Vector3 normalized = (pose2.position - pose.position).normalized;
		Vector3 normalized2 = (pose3.position - pose2.position).normalized;
		return Vector3.Dot(normalized, normalized2) >= _safeReleaseThreshold;
	}

	[Obsolete("Disable the component to Cancel any ongoing pinch")]
	public void Cancel()
	{
	}

	public void InjectAllIndexPinchSafeReleaseSelector(IHand hand)
	{
		InjectHand(hand);
	}

	[Obsolete("Use SelectOnRelease setter instead.")]
	public void InjectSelectOnRelease(bool selectOnRelease)
	{
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}
