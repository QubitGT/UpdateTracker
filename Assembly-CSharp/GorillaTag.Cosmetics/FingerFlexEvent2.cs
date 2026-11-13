using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace GorillaTag.Cosmetics;

public class FingerFlexEvent2 : MonoBehaviour, ITickSystemTick
{
	[Serializable]
	public class FlexEvent
	{
		public enum TriggerType
		{
			OnFlex = 0,
			OnRelease = 2
		}

		public enum FingerType
		{
			Thumb,
			Index,
			Middle,
			IndexAndMiddle,
			IndexOrMiddle,
			StickLeft,
			StickRight,
			StickUp,
			StickDown
		}

		public enum HandType
		{
			HeldItemHand,
			EquippedSide,
			LeftHand,
			RightHand
		}

		private enum RangeState
		{
			Below,
			Within,
			Above
		}

		public TriggerType triggerType;

		public bool tryLink = true;

		[HideInInspector]
		public int linkIndex = -1;

		[Space]
		public FingerType fingerType = FingerType.Index;

		[Space]
		public HandType handType;

		private const string ADVANCED = "Advanced Properties";

		[Tooltip("When this is checked, all players in the room will fire the event. Otherwise, only the local player will fire it. You should usually leave this on, unless you're using it for something local like controller haptics.")]
		public bool networked = true;

		[Range(0.01f, 0.75f)]
		public float flexThreshold = 0.75f;

		[Range(0.01f, 1f)]
		public float releaseThreshold = 0.01f;

		public ContinuousPropertyArray continuousProperties;

		public UnityEvent<bool, float> unityEvent;

		[NonSerialized]
		public bool wasHeld;

		[NonSerialized]
		public bool marginError;

		private RangeState currentState;

		private RangeState lastState;

		private float lastThresholdTime = -100000f;

		public bool IsFlexTrigger => triggerType == TriggerType.OnFlex;

		public bool IsReleaseTrigger => triggerType == TriggerType.OnRelease;

		public bool RequiresHeldItem
		{
			get
			{
				HandType handType = this.handType;
				return handType == HandType.HeldItemHand || handType == HandType.EquippedSide;
			}
		}

		public bool IsStick
		{
			get
			{
				FingerType fingerType = this.fingerType;
				return fingerType == FingerType.StickLeft || fingerType == FingerType.StickRight || fingerType == FingerType.StickUp || fingerType == FingerType.StickDown;
			}
		}

		public bool HasValidLink => linkIndex >= 0;

		public bool IsLinked
		{
			get
			{
				if (tryLink)
				{
					return linkIndex >= 0;
				}
				return false;
			}
		}

		private bool ShowMainProperties
		{
			get
			{
				if (IsLinked)
				{
					return IsFlexTrigger;
				}
				return true;
			}
		}

		private bool ShowFlexThreshold => ShowMainProperties;

		private bool ShowReleaseThreshold
		{
			get
			{
				if (!IsLinked || IsReleaseTrigger)
				{
					return !IsFlexTrigger;
				}
				return false;
			}
		}

		public void ProcessState(bool leftHand, float flexValue)
		{
			currentState = ((!(flexValue < releaseThreshold)) ? ((!(flexValue >= flexThreshold)) ? RangeState.Within : RangeState.Above) : RangeState.Below);
			if (ShowMainProperties && currentState != lastState && continuousProperties != null && continuousProperties.Count > 0)
			{
				float f = Mathf.InverseLerp(releaseThreshold, flexThreshold, flexValue);
				continuousProperties.ApplyAll(f);
			}
			if (currentState == RangeState.Above && lastState == RangeState.Below)
			{
				lastThresholdTime = Time.time;
				lastState = RangeState.Above;
				if (IsFlexTrigger)
				{
					unityEvent?.Invoke(leftHand, flexValue);
				}
			}
			else if (currentState == RangeState.Below && lastState == RangeState.Above)
			{
				lastThresholdTime = Time.time;
				lastState = RangeState.Below;
				if (IsReleaseTrigger)
				{
					unityEvent?.Invoke(leftHand, flexValue);
				}
			}
		}
	}

	public FlexEvent[] list;

	[Tooltip("Stick input isn't synced by the rig, so stick events normally only run for the local player. When checked, the holding hand's stick is sent to other players through the TransferrableObject's item state (bits 0-5), so stick events run on everyone's copy. Only use this when nothing else uses the item state (Networked State Events off, and no subclass writing it).")]
	[SerializeField]
	private bool syncStickInput;

	private const int STICK_X_SHIFT = 0;

	private const int STICK_Y_SHIFT = 3;

	private const int STICK_AXIS_MASK = 7;

	private const int STICK_BITS_MASK = 63;

	private const float STICK_STEPS = 3f;

	private VRRig myRig;

	private TransferrableObject myTransferrable;

	private IHeldItem myHeldItem;

	private bool CanSyncSticks
	{
		get
		{
			if (syncStickInput)
			{
				return !myTransferrable.IsNull();
			}
			return false;
		}
	}

	public bool TickRunning { get; set; }

	private bool TryLinkToNextEvent(int index)
	{
		if (index < list.Length - 1)
		{
			if (list[index].IsFlexTrigger && list[index + 1].IsReleaseTrigger)
			{
				list[index].linkIndex = index + 1;
				list[index + 1].linkIndex = index;
				return true;
			}
			list[index + 1].linkIndex = -1;
		}
		list[index].linkIndex = -1;
		return false;
	}

	private void Awake()
	{
		myRig = GetComponentInParent<VRRig>();
		myTransferrable = GetComponentInParent<TransferrableObject>();
		myHeldItem = GetComponentInParent<IHeldItem>();
		for (int i = 0; i < list.Length; i++)
		{
			FlexEvent flexEvent = list[i];
			if (myTransferrable.IsNull() && flexEvent.RequiresHeldItem)
			{
				myTransferrable = GetComponentInParent<TransferrableObject>();
			}
			if (flexEvent.tryLink && TryLinkToNextEvent(i))
			{
				FlexEvent flexEvent2 = list[i + 1];
				flexEvent.releaseThreshold = flexEvent2.releaseThreshold;
				flexEvent2.flexThreshold = flexEvent.flexThreshold;
				flexEvent2.fingerType = flexEvent.fingerType;
				flexEvent2.handType = flexEvent.handType;
				flexEvent2.networked = flexEvent.networked;
				i++;
			}
		}
	}

	private void CalcFlex(bool disable)
	{
		bool flag = myRig != null && myRig.isOfflineVRRig;
		bool? flag2 = null;
		float num2 = default(float);
		for (int i = 0; i < list.Length; i++)
		{
			FlexEvent flexEvent = list[i];
			if ((!flexEvent.networked && !flag) || (flexEvent.IsStick && !flag && (myRig == null || !CanSyncSticks)) || myRig == null || (flexEvent.RequiresHeldItem && myTransferrable.IsNull() && myHeldItem == null) || (flexEvent.handType == FlexEvent.HandType.EquippedSide && myTransferrable.IsNull()))
			{
				continue;
			}
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			switch (flexEvent.handType)
			{
			case FlexEvent.HandType.LeftHand:
				flag3 = true;
				break;
			case FlexEvent.HandType.RightHand:
				flag4 = true;
				break;
			case FlexEvent.HandType.HeldItemHand:
				if (!myTransferrable.IsNull())
				{
					flag3 = myTransferrable.currentState == TransferrableObject.PositionState.InLeftHand;
					flag4 = myTransferrable.currentState == TransferrableObject.PositionState.InRightHand;
					flag5 = flag3 || flag4;
				}
				else if (myHeldItem != null)
				{
					flag3 = myHeldItem.InLeftHand();
					flag4 = !flag3 && myHeldItem.InHand();
					flag5 = flag3 || flag4;
				}
				break;
			case FlexEvent.HandType.EquippedSide:
				flag3 = (myTransferrable.storedZone & (BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.LeftBack)) != 0;
				flag4 = (myTransferrable.storedZone & (BodyDockPositions.DropPositions.RightArm | BodyDockPositions.DropPositions.RightBack)) != 0;
				break;
			}
			if ((flag3 && flag4) || (!flag3 && !flag4 && !flexEvent.wasHeld))
			{
				continue;
			}
			float num;
			if (disable || (flexEvent.wasHeld && !flag5))
			{
				num = 0f;
			}
			else
			{
				FlexEvent.FingerType fingerType = flexEvent.fingerType;
				switch (fingerType)
				{
				case FlexEvent.FingerType.Thumb:
					num2 = (flag3 ? myRig.leftThumb.calcT : myRig.rightThumb.calcT);
					break;
				case FlexEvent.FingerType.Index:
					num2 = (flag3 ? myRig.leftIndex.calcT : myRig.rightIndex.calcT);
					break;
				case FlexEvent.FingerType.Middle:
					num2 = (flag3 ? myRig.leftMiddle.calcT : myRig.rightMiddle.calcT);
					break;
				case FlexEvent.FingerType.IndexAndMiddle:
					num2 = (flag3 ? Mathf.Min(myRig.leftIndex.calcT, myRig.leftMiddle.calcT) : Mathf.Min(myRig.rightIndex.calcT, myRig.rightMiddle.calcT));
					break;
				case FlexEvent.FingerType.IndexOrMiddle:
					num2 = (flag3 ? Mathf.Max(myRig.leftIndex.calcT, myRig.leftMiddle.calcT) : Mathf.Max(myRig.rightIndex.calcT, myRig.rightMiddle.calcT));
					break;
				case FlexEvent.FingerType.StickLeft:
					num2 = Mathf.Max(0f, 0f - ReadStick(flag3, flag).x);
					break;
				case FlexEvent.FingerType.StickRight:
					num2 = Mathf.Max(0f, ReadStick(flag3, flag).x);
					break;
				case FlexEvent.FingerType.StickUp:
					num2 = Mathf.Max(0f, ReadStick(flag3, flag).y);
					break;
				case FlexEvent.FingerType.StickDown:
					num2 = Mathf.Max(0f, 0f - ReadStick(flag3, flag).y);
					break;
				default:
					global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(fingerType);
					break;
				}
				num = num2;
			}
			float flexValue = num;
			if (flexEvent.IsStick && flag && !disable && (flag3 || flag4))
			{
				flag2 = flag3;
			}
			flexEvent.ProcessState(flag3, flexValue);
			flexEvent.wasHeld = flag5 && !disable;
			if (flexEvent.IsLinked)
			{
				FlexEvent obj = list[i + 1];
				obj.ProcessState(flag3, flexValue);
				obj.wasHeld = flag5;
				i++;
			}
		}
		if (flag && CanSyncSticks)
		{
			Vector2 stick = (flag2.HasValue ? ControllerInputPoller.Primary2DAxis(flag2.Value ? XRNode.LeftHand : XRNode.RightHand) : Vector2.zero);
			int num3 = (int)(myTransferrable.itemState & (TransferrableObject.ItemStates)(-64));
			myTransferrable.itemState = (TransferrableObject.ItemStates)(num3 | EncodeStick(stick));
		}
	}

	private Vector2 ReadStick(bool leftHand, bool isLocalRig)
	{
		if (isLocalRig)
		{
			return ControllerInputPoller.Primary2DAxis(leftHand ? XRNode.LeftHand : XRNode.RightHand);
		}
		return DecodeStick((int)myTransferrable.itemState);
	}

	private static int EncodeStick(Vector2 stick)
	{
		return EncodeStickAxis(stick.x) | (EncodeStickAxis(stick.y) << 3);
	}

	private static Vector2 DecodeStick(int bits)
	{
		return new Vector2(DecodeStickAxis(bits), DecodeStickAxis(bits >> 3));
	}

	private static int EncodeStickAxis(float value)
	{
		return (Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(value) * 3f), 0, 3) << 1) | ((value < 0f) ? 1 : 0);
	}

	private static float DecodeStickAxis(int bits)
	{
		bits &= 7;
		float num = (float)(bits >> 1) / 3f;
		if ((bits & 1) == 0)
		{
			return num;
		}
		return 0f - num;
	}

	public void OnEnable()
	{
		if (myRig == null)
		{
			myRig = GetComponentInParent<VRRig>();
		}
		TickSystem<object>.AddTickCallback(this);
	}

	public void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
		CalcFlex(disable: true);
	}

	public void Tick()
	{
		CalcFlex(disable: false);
	}
}
