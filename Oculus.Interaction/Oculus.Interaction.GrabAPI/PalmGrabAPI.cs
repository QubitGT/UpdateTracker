using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class PalmGrabAPI : IFingerAPI
{
	private class FingerGrabData
	{
		private readonly HandFinger _fingerID;

		private readonly Vector2 _curlNormalizationParams;

		public float GrabStrength;

		public bool IsGrabbing;

		public bool IsGrabbingChanged { get; private set; }

		public FingerGrabData(HandFinger fingerId)
		{
			_fingerID = fingerId;
			Vector2 vector = CURL_RANGE[(int)_fingerID];
			_curlNormalizationParams = new Vector2(vector.x, vector.y - vector.x);
		}

		public void UpdateGrabStrength(IHand hand, FingerShapes fingerShapes)
		{
			float num = fingerShapes.GetCurlValue(_fingerID, hand);
			if (_fingerID != 0)
			{
				num = (num * 2f + fingerShapes.GetFlexionValue(_fingerID, hand)) / 3f;
			}
			GrabStrength = Mathf.Clamp01((num - _curlNormalizationParams.x) / _curlNormalizationParams.y);
		}

		public void UpdateIsGrabbing(float startThreshold, float releaseThreshold)
		{
			if (GrabStrength > startThreshold)
			{
				if (!IsGrabbing)
				{
					IsGrabbing = true;
					IsGrabbingChanged = true;
				}
			}
			else if (GrabStrength < releaseThreshold && IsGrabbing)
			{
				IsGrabbing = false;
				IsGrabbingChanged = true;
			}
		}

		public void ClearState()
		{
			IsGrabbingChanged = false;
		}
	}

	private Vector3 _poseVolumeCenterOffset = Vector3.zero;

	private static readonly Vector3 POSE_VOLUME_OFFSET = new Vector3(0.07f, -0.03f, 0f);

	private static readonly float START_THRESHOLD = 0.9f;

	private static readonly float RELEASE_THRESHOLD = 0.6f;

	private static readonly Vector2[] CURL_RANGE = new Vector2[5]
	{
		new Vector2(190f, 220f),
		new Vector2(180f, 250f),
		new Vector2(180f, 250f),
		new Vector2(180f, 250f),
		new Vector2(180f, 245f)
	};

	private FingerShapes _fingerShapes = new FingerShapes();

	private readonly FingerGrabData[] _fingersGrabData = new FingerGrabData[5]
	{
		new FingerGrabData(HandFinger.Thumb),
		new FingerGrabData(HandFinger.Index),
		new FingerGrabData(HandFinger.Middle),
		new FingerGrabData(HandFinger.Ring),
		new FingerGrabData(HandFinger.Pinky)
	};

	public bool GetFingerIsGrabbing(HandFinger finger)
	{
		return _fingersGrabData[(int)finger].IsGrabbing;
	}

	public bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetGrabState)
	{
		if (_fingersGrabData[(int)finger].IsGrabbingChanged)
		{
			return _fingersGrabData[(int)finger].IsGrabbing == targetGrabState;
		}
		return false;
	}

	public float GetFingerGrabScore(HandFinger finger)
	{
		return _fingersGrabData[(int)finger].GrabStrength;
	}

	public void Update(IHand hand)
	{
		ClearState();
		if (hand != null && hand.IsTrackedDataValid)
		{
			UpdateVolumeCenter(hand);
			for (int i = 0; i < 5; i++)
			{
				_fingersGrabData[i].UpdateGrabStrength(hand, _fingerShapes);
				_fingersGrabData[i].UpdateIsGrabbing(START_THRESHOLD, RELEASE_THRESHOLD);
			}
		}
	}

	private void UpdateVolumeCenter(IHand hand)
	{
		_poseVolumeCenterOffset = ((hand.Handedness == Handedness.Left) ? (Constants.LeftDistal * POSE_VOLUME_OFFSET.x + Constants.LeftDorsal * POSE_VOLUME_OFFSET.y + Constants.LeftThumbSide * POSE_VOLUME_OFFSET.z) : (Constants.RightDistal * POSE_VOLUME_OFFSET.x + Constants.RightDorsal * POSE_VOLUME_OFFSET.y + Constants.RightThumbSide * POSE_VOLUME_OFFSET.z));
	}

	private void ClearState()
	{
		for (int i = 0; i < 5; i++)
		{
			_fingersGrabData[i].ClearState();
		}
	}

	public Vector3 GetWristOffsetLocal()
	{
		return _poseVolumeCenterOffset;
	}
}
