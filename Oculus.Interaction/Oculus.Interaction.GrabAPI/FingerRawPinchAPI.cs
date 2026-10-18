using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class FingerRawPinchAPI : IFingerAPI
{
	private class FingerPinchData
	{
		private readonly HandFinger _finger;

		private readonly HandJointId _tipId;

		public float PinchStrength;

		public bool IsPinching;

		public bool IsPinchingChanged { get; private set; }

		public Vector3 TipPosition { get; private set; }

		public FingerPinchData(HandFinger fingerId)
		{
			_finger = fingerId;
			_tipId = HandJointUtils.GetHandFingerTip(fingerId);
		}

		private void UpdateTipPosition(IHand hand)
		{
			if (hand.GetJointPoseFromWrist(_tipId, out var pose))
			{
				TipPosition = pose.position;
			}
		}

		public void UpdateIsPinching(IHand hand)
		{
			UpdateTipPosition(hand);
			PinchStrength = hand.GetFingerPinchStrength(_finger);
			bool fingerIsPinching = hand.GetFingerIsPinching(_finger);
			if (fingerIsPinching != IsPinching)
			{
				IsPinchingChanged = true;
			}
			IsPinching = fingerIsPinching;
		}

		public void ClearState()
		{
			IsPinchingChanged = false;
		}
	}

	private readonly FingerPinchData[] _fingersPinchData = new FingerPinchData[5]
	{
		new FingerPinchData(HandFinger.Thumb),
		new FingerPinchData(HandFinger.Index),
		new FingerPinchData(HandFinger.Middle),
		new FingerPinchData(HandFinger.Ring),
		new FingerPinchData(HandFinger.Pinky)
	};

	public bool GetFingerIsGrabbing(HandFinger finger)
	{
		return _fingersPinchData[(int)finger].IsPinching;
	}

	public Vector3 GetWristOffsetLocal()
	{
		float num = float.NegativeInfinity;
		Vector3 tipPosition = _fingersPinchData[0].TipPosition;
		Vector3 result = tipPosition;
		for (int i = 1; i < 5; i++)
		{
			float pinchStrength = _fingersPinchData[i].PinchStrength;
			if (pinchStrength > num)
			{
				num = pinchStrength;
				Vector3 tipPosition2 = _fingersPinchData[i].TipPosition;
				result = (tipPosition + tipPosition2) * 0.5f;
			}
		}
		return result;
	}

	public bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetPinchState)
	{
		if (_fingersPinchData[(int)finger].IsPinchingChanged)
		{
			return _fingersPinchData[(int)finger].IsPinching == targetPinchState;
		}
		return false;
	}

	public float GetFingerGrabScore(HandFinger finger)
	{
		return _fingersPinchData[(int)finger].PinchStrength;
	}

	public void Update(IHand hand)
	{
		ClearState();
		for (int i = 0; i < 5; i++)
		{
			_fingersPinchData[i].UpdateIsPinching(hand);
		}
	}

	private void ClearState()
	{
		for (int i = 0; i < 5; i++)
		{
			_fingersPinchData[i].ClearState();
		}
	}
}
