using System.Buffers;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class TouchHandGrabInteractorVisual : MonoBehaviour
{
	[SerializeField]
	private TouchHandGrabInteractor _interactor;

	[SerializeField]
	private SyntheticHand _syntheticHand;

	protected bool _started;

	public void InjectSyntheticHand(SyntheticHand syntheticHand)
	{
		_syntheticHand = syntheticHand;
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
			_interactor.WhenFingerLocked += UpdateLocks;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_interactor.WhenFingerLocked -= UpdateLocks;
		}
	}

	private void UpdateLocks()
	{
		bool flag = false;
		for (int i = 0; i < 5; i++)
		{
			HandFinger finger = (HandFinger)i;
			if (_interactor.IsFingerLocked(finger))
			{
				Pose[] fingerJoints = _interactor.GetFingerJoints(finger);
				Quaternion[] array = ArrayPool<Quaternion>.Shared.Rent(fingerJoints.Length);
				for (int j = 0; j < fingerJoints.Length; j++)
				{
					array[j] = fingerJoints[j].rotation;
				}
				_syntheticHand.OverrideFingerRotations(finger, array, 1f);
				SyntheticHand syntheticHand = _syntheticHand;
				JointFreedom freedomLevel = JointFreedom.Locked;
				syntheticHand.SetFingerFreedom(in finger, in freedomLevel, skipAnimation: true);
				ArrayPool<Quaternion>.Shared.Return(array);
				flag = true;
			}
			else
			{
				SyntheticHand syntheticHand2 = _syntheticHand;
				JointFreedom freedomLevel = JointFreedom.Free;
				syntheticHand2.SetFingerFreedom(in finger, in freedomLevel);
			}
		}
		if (flag)
		{
			_syntheticHand.MarkInputDataRequiresUpdate();
		}
	}

	protected virtual void Update()
	{
		UpdateLocks();
	}
}
