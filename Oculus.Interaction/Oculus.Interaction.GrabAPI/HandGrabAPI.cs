using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class HandGrabAPI : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	[Optional]
	private UnityEngine.Object _hmd;

	private IFingerAPI _fingerPinchGrabAPI;

	private IFingerAPI _fingerPalmGrabAPI;

	private bool _started;

	public IHand Hand { get; private set; }

	public IHmd Hmd { get; private set; }

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		Hmd = _hmd as IHmd;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (_fingerPinchGrabAPI == null)
		{
			_fingerPinchGrabAPI = new PinchGrabAPI(Hmd);
		}
		if (_fingerPalmGrabAPI == null)
		{
			_fingerPalmGrabAPI = new PalmGrabAPI();
		}
		this.EndStart(ref _started);
	}

	private void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += OnHandUpdated;
		}
	}

	private void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= OnHandUpdated;
		}
	}

	private void OnHandUpdated()
	{
		_fingerPinchGrabAPI.Update(Hand);
		_fingerPalmGrabAPI.Update(Hand);
	}

	public HandFingerFlags HandPinchGrabbingFingers()
	{
		return HandGrabbingFingers(_fingerPinchGrabAPI);
	}

	public HandFingerFlags HandPalmGrabbingFingers()
	{
		return HandGrabbingFingers(_fingerPalmGrabAPI);
	}

	private HandFingerFlags HandGrabbingFingers(IFingerAPI fingerAPI)
	{
		HandFingerFlags handFingerFlags = HandFingerFlags.None;
		for (int i = 0; i < 5; i++)
		{
			HandFinger finger = (HandFinger)i;
			if (fingerAPI.GetFingerIsGrabbing(finger))
			{
				handFingerFlags = (HandFingerFlags)((int)handFingerFlags | (1 << i));
			}
		}
		return handFingerFlags;
	}

	public bool IsHandPinchGrabbing(in GrabbingRule fingers)
	{
		HandFingerFlags grabbingFingers = HandPinchGrabbingFingers();
		return IsSustainingGrab(in fingers, grabbingFingers);
	}

	public bool IsHandPalmGrabbing(in GrabbingRule fingers)
	{
		HandFingerFlags grabbingFingers = HandPalmGrabbingFingers();
		return IsSustainingGrab(in fingers, grabbingFingers);
	}

	public bool IsSustainingGrab(in GrabbingRule fingers, HandFingerFlags grabbingFingers)
	{
		bool flag = false;
		for (int i = 0; i < 5; i++)
		{
			HandFinger fingerID = (HandFinger)i;
			HandFingerFlags handFingerFlags = (HandFingerFlags)(1 << i);
			bool flag2 = (grabbingFingers & handFingerFlags) != 0;
			if (fingers[fingerID] == FingerRequirement.Required)
			{
				flag = flag || flag2;
				if (fingers.UnselectMode == FingerUnselectMode.AnyReleased && !flag2)
				{
					return false;
				}
				if (fingers.UnselectMode == FingerUnselectMode.AllReleased && flag2)
				{
					return true;
				}
			}
			else if (fingers[fingerID] == FingerRequirement.Optional)
			{
				flag = flag || flag2;
			}
		}
		return flag;
	}

	public bool IsHandSelectPinchFingersChanged(in GrabbingRule fingers)
	{
		return IsHandSelectFingersChanged(in fingers, _fingerPinchGrabAPI);
	}

	public bool IsHandSelectPalmFingersChanged(in GrabbingRule fingers)
	{
		return IsHandSelectFingersChanged(in fingers, _fingerPalmGrabAPI);
	}

	public bool IsHandUnselectPinchFingersChanged(in GrabbingRule fingers)
	{
		return IsHandUnselectFingersChanged(in fingers, _fingerPinchGrabAPI);
	}

	public bool IsHandUnselectPalmFingersChanged(in GrabbingRule fingers)
	{
		return IsHandUnselectFingersChanged(in fingers, _fingerPalmGrabAPI);
	}

	private bool IsHandSelectFingersChanged(in GrabbingRule fingers, IFingerAPI fingerAPI)
	{
		bool selectsWithOptionals = fingers.SelectsWithOptionals;
		bool result = false;
		for (int i = 0; i < 5; i++)
		{
			HandFinger handFinger = (HandFinger)i;
			if (fingers[handFinger] == FingerRequirement.Required)
			{
				if (!fingerAPI.GetFingerIsGrabbing(handFinger))
				{
					return false;
				}
				if (fingerAPI.GetFingerIsGrabbingChanged(handFinger, targetPinchState: true))
				{
					result = true;
				}
			}
			else if (selectsWithOptionals && fingers[handFinger] == FingerRequirement.Optional && fingerAPI.GetFingerIsGrabbingChanged(handFinger, targetPinchState: true))
			{
				return true;
			}
		}
		return result;
	}

	private bool IsHandUnselectFingersChanged(in GrabbingRule fingers, IFingerAPI fingerAPI)
	{
		bool flag = false;
		bool flag2 = false;
		bool selectsWithOptionals = fingers.SelectsWithOptionals;
		for (int i = 0; i < 5; i++)
		{
			HandFinger handFinger = (HandFinger)i;
			if (fingers[handFinger] == FingerRequirement.Ignored)
			{
				continue;
			}
			flag |= fingerAPI.GetFingerIsGrabbing(handFinger);
			if (fingers[handFinger] == FingerRequirement.Required)
			{
				if (fingerAPI.GetFingerIsGrabbingChanged(handFinger, targetPinchState: false))
				{
					flag2 = true;
					if (fingers.UnselectMode == FingerUnselectMode.AnyReleased)
					{
						return true;
					}
				}
			}
			else if (fingers[handFinger] == FingerRequirement.Optional && fingerAPI.GetFingerIsGrabbingChanged(handFinger, targetPinchState: false))
			{
				flag2 = true;
				if (fingers.UnselectMode == FingerUnselectMode.AnyReleased && selectsWithOptionals)
				{
					return true;
				}
			}
		}
		return !flag && flag2;
	}

	public Vector3 GetPinchCenter()
	{
		Vector3 localOffset = Vector3.zero;
		if (_fingerPinchGrabAPI != null)
		{
			localOffset = _fingerPinchGrabAPI.GetWristOffsetLocal();
		}
		return WristOffsetToWorldPoint(localOffset);
	}

	public Vector3 GetPalmCenter()
	{
		Vector3 localOffset = Vector3.zero;
		if (_fingerPalmGrabAPI != null)
		{
			localOffset = _fingerPalmGrabAPI.GetWristOffsetLocal();
		}
		return WristOffsetToWorldPoint(localOffset);
	}

	private Vector3 WristOffsetToWorldPoint(Vector3 localOffset)
	{
		if (!Hand.GetJointPose(HandJointId.HandWristRoot, out var pose))
		{
			return localOffset * Hand.Scale;
		}
		return pose.position + pose.rotation * localOffset * Hand.Scale;
	}

	public float GetHandPinchScore(in GrabbingRule fingers, bool includePinching = true)
	{
		return GetHandGrabScore(in fingers, includePinching, _fingerPinchGrabAPI);
	}

	public float GetHandPalmScore(in GrabbingRule fingers, bool includeGrabbing = true)
	{
		return GetHandGrabScore(in fingers, includeGrabbing, _fingerPalmGrabAPI);
	}

	public float GetFingerPinchStrength(HandFinger finger)
	{
		return _fingerPinchGrabAPI.GetFingerGrabScore(finger);
	}

	public float GetFingerPinchPercent(HandFinger finger)
	{
		if (_fingerPinchGrabAPI is FingerPinchGrabAPI)
		{
			return (_fingerPinchGrabAPI as FingerPinchGrabAPI).GetFingerPinchPercent(finger);
		}
		Debug.LogWarning("GetFingerPinchPercent is not applicable");
		return -1f;
	}

	public float GetFingerPinchDistance(HandFinger finger)
	{
		if (_fingerPinchGrabAPI is FingerPinchGrabAPI)
		{
			return (_fingerPinchGrabAPI as FingerPinchGrabAPI).GetFingerPinchDistance(finger);
		}
		Debug.LogWarning("GetFingerPinchDistance is not applicable");
		return -1f;
	}

	public float GetFingerPalmStrength(HandFinger finger)
	{
		return _fingerPalmGrabAPI.GetFingerGrabScore(finger);
	}

	private float GetHandGrabScore(in GrabbingRule fingers, bool includeGrabbing, IFingerAPI fingerAPI)
	{
		float num = 1f;
		float num2 = 0f;
		bool flag = false;
		bool selectsWithOptionals = fingers.SelectsWithOptionals;
		for (int i = 0; i < 5; i++)
		{
			HandFinger handFinger = (HandFinger)i;
			if ((includeGrabbing || !fingerAPI.GetFingerIsGrabbing((HandFinger)i)) && fingers[handFinger] != 0)
			{
				if (fingers[handFinger] == FingerRequirement.Optional)
				{
					num2 = Mathf.Max(num2, fingerAPI.GetFingerGrabScore(handFinger));
				}
				else if (fingers[handFinger] == FingerRequirement.Required)
				{
					flag = true;
					num = Mathf.Min(num, fingerAPI.GetFingerGrabScore(handFinger));
				}
			}
		}
		if (!selectsWithOptionals)
		{
			if (!flag)
			{
				return 0f;
			}
			return num;
		}
		return num2;
	}

	public void SetPinchGrabParam(PinchGrabParam paramId, float paramVal)
	{
		if (_fingerPinchGrabAPI is FingerPinchGrabAPI fingerPinchGrabAPI)
		{
			fingerPinchGrabAPI.SetPinchGrabParam(paramId, paramVal);
		}
	}

	public float GetPinchGrabParam(PinchGrabParam paramId)
	{
		if (_fingerPinchGrabAPI is FingerPinchGrabAPI fingerPinchGrabAPI)
		{
			return fingerPinchGrabAPI.GetPinchGrabParam(paramId);
		}
		return 0f;
	}

	public bool GetFingerIsGrabbing(HandFinger finger)
	{
		return _fingerPinchGrabAPI.GetFingerIsGrabbing(finger);
	}

	public bool GetFingerIsPalmGrabbing(HandFinger finger)
	{
		return _fingerPalmGrabAPI.GetFingerIsGrabbing(finger);
	}

	public void InjectAllHandGrabAPI(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectOptionalHmd(IHmd hmd)
	{
		Hmd = hmd;
		_hmd = hmd as UnityEngine.Object;
	}

	public void InjectOptionalFingerPinchAPI(IFingerAPI fingerPinchAPI)
	{
		_fingerPinchGrabAPI = fingerPinchAPI;
	}

	public void InjectOptionalFingerGrabAPI(IFingerAPI fingerGrabAPI)
	{
		_fingerPalmGrabAPI = fingerGrabAPI;
	}
}
