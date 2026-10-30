using System;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTagScripts;

public class CosmeticsThrottler : MonoBehaviour, IGorillaSliceableSimple
{
	public enum RigDrawState
	{
		All = 0,
		Partial = 1,
		Min = 2,
		Startup = -1,
		Unknown = -2
	}

	public const float ExcludedSqrDistance = float.MaxValue;

	public float DrawAllDistance = 5f;

	public float MaxDrawDistance = 10f;

	public bool DrawOnPlayerCount = true;

	public int DrawAllCount = 6;

	public int DrawMaxCount = 14;

	public int ThrottlePlayerCountThreshold = 11;

	[Tooltip("Players below ThrottlePlayerCountThreshold the room must drop to before throttling turns off again, so a room hovering around the threshold doesn't flip it on and off.")]
	public int ThrottlePlayerCountHysteresis = 2;

	[Tooltip("Meters past DrawAllDistance/MaxDrawDistance a rig must move before it changes state.")]
	public float DistanceHysteresis = 0.5f;

	[Tooltip("Ranks a rig must clear past DrawAllCount/DrawMaxCount before it's upgraded. Downgrades still happen at the count, so the counts are never exceeded.")]
	public int CountHysteresis = 2;

	[Tooltip("Meters past the camera's side plane a rig must move before it's treated as behind (or back in front of) the camera. Stops rigs beside the player flickering with small head turns.")]
	public float BehindCameraHysteresis = 0.5f;

	[Tooltip("Max rigs that change draw state per slice (0 = unlimited). Spreads bursts, like turning around or throttling switching on/off, over several slices instead of one hitch. Closest rigs are handled first.")]
	public int MaxTransitionsPerSlice = 4;

	[Tooltip("Throttle at all times, regardless of ThrottlePlayerCountThreshold. For testing and profiling the throttler in small rooms. Can be toggled during play.")]
	public bool ForceThrottle;

	private bool _throttleActive;

	private bool _wasThrottling;

	private bool _restorePending;

	public CosmeticsController.CosmeticSlots[] ToggleSlots;

	private int _toggleSlotMask;

	[NonSerialized]
	private GorillaRigHelper[] _rigHelpers;

	private Camera _mainCamera;

	private bool _warnedNoCamera;

	private bool IsThrottling
	{
		get
		{
			if (!ForceThrottle)
			{
				return _throttleActive;
			}
			return true;
		}
	}

	private void Awake()
	{
		BuildToggleSlotMask();
		if (ToggleSlots == null || ToggleSlots.Length == 0)
		{
			Debug.LogWarning("CosmeticsThrottler on '" + base.name + "' has no ToggleSlots, so the Partial state only turns off particles.", this);
		}
		if (VRRigCache.isInitialized)
		{
			InitializeRigHelpers();
		}
		else
		{
			VRRigCache.OnPostInitialize += OnRigCacheInitialized;
		}
		RoomSystem.JoinedRoomEvent += new Action(UpdatePlayerCount);
		RoomSystem.LeftRoomEvent += new Action(UpdatePlayerCount);
		RoomSystem.PlayersChangedEvent += new Action(UpdatePlayerCount);
		UpdatePlayerCount();
	}

	private void OnRigCacheInitialized()
	{
		VRRigCache.OnPostInitialize -= OnRigCacheInitialized;
		InitializeRigHelpers();
	}

	private void InitializeRigHelpers()
	{
		if (_rigHelpers == null)
		{
			VRRig[] allRigs = VRRigCache.Instance.GetAllRigs();
			_rigHelpers = new GorillaRigHelper[allRigs.Length];
			for (int i = 0; i < allRigs.Length; i++)
			{
				_rigHelpers[i] = new GorillaRigHelper
				{
					rig = allRigs[i],
					rigTransform = allRigs[i].transform,
					state = RigDrawState.Startup,
					sqrDistance = float.MaxValue
				};
				VRRig obj = allRigs[i];
				obj.OnCosmeticsActivated = (Action<VRRig>)Delegate.Combine(obj.OnCosmeticsActivated, new Action<VRRig>(OnRigCosmeticsActivated));
			}
		}
	}

	private void OnDestroy()
	{
		VRRigCache.OnPostInitialize -= OnRigCacheInitialized;
		RoomSystem.JoinedRoomEvent -= new Action(UpdatePlayerCount);
		RoomSystem.LeftRoomEvent -= new Action(UpdatePlayerCount);
		RoomSystem.PlayersChangedEvent -= new Action(UpdatePlayerCount);
		if (_rigHelpers == null)
		{
			return;
		}
		for (int i = 0; i < _rigHelpers.Length; i++)
		{
			if (_rigHelpers[i].rig != null)
			{
				VRRig rig = _rigHelpers[i].rig;
				rig.OnCosmeticsActivated = (Action<VRRig>)Delegate.Remove(rig.OnCosmeticsActivated, new Action<VRRig>(OnRigCosmeticsActivated));
			}
		}
	}

	private void UpdatePlayerCount()
	{
		int count = RoomSystem.PlayersInRoom.Count;
		if (!_throttleActive && count >= ThrottlePlayerCountThreshold)
		{
			_throttleActive = true;
		}
		else if (_throttleActive && count <= ThrottlePlayerCountThreshold - ThrottlePlayerCountHysteresis)
		{
			_throttleActive = false;
		}
	}

	private void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this);
	}

	private void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this);
		if (!ApplicationQuittingState.IsQuitting)
		{
			EnableAllRenderers();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 center = ((_mainCamera != null) ? _mainCamera.transform.position : base.transform.position);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(center, DrawAllDistance);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(center, MaxDrawDistance);
	}

	public void SliceUpdate()
	{
		if (_rigHelpers == null)
		{
			return;
		}
		if (!IsThrottling)
		{
			if (_wasThrottling)
			{
				_wasThrottling = false;
				_restorePending = true;
			}
			if (_restorePending)
			{
				_restorePending = !RestoreRenderers(MaxTransitionsPerSlice);
			}
			return;
		}
		_wasThrottling = true;
		_restorePending = false;
		if (_mainCamera == null)
		{
			_mainCamera = Camera.main;
			if (_mainCamera == null && !_warnedNoCamera)
			{
				_warnedNoCamera = true;
				Debug.LogWarning("CosmeticsThrottler on '" + base.name + "' is throttling but there's no main camera (Camera.main is null), so no rigs are being throttled.", this);
			}
			return;
		}
		Transform obj = _mainCamera.transform;
		Vector3 position = obj.position;
		Vector3 forward = obj.forward;
		for (int i = 0; i < _rigHelpers.Length; i++)
		{
			ref GorillaRigHelper reference = ref _rigHelpers[i];
			bool inactive = reference.inactive;
			reference.inactive = !reference.rig.isActiveAndEnabled;
			if (reference.inactive)
			{
				reference.sqrDistance = float.MaxValue;
				continue;
			}
			if (inactive && (reference.state == RigDrawState.Partial || reference.state == RigDrawState.Min))
			{
				reference.state = RigDrawState.Unknown;
			}
			Vector3 position2 = reference.rigTransform.position;
			float num = Vector3.Dot(position2 - position, forward);
			reference.behindCamera = (reference.behindCamera ? (num <= BehindCameraHysteresis) : (num < 0f - BehindCameraHysteresis));
			if (reference.behindCamera)
			{
				reference.sqrDistance = float.MaxValue;
			}
			else
			{
				reference.sqrDistance = (position2 - position).sqrMagnitude;
			}
		}
		Array.Sort(_rigHelpers);
		float drawAllEnterSqr = SqrClamped(DrawAllDistance - DistanceHysteresis);
		float drawAllExitSqr = SqrClamped(DrawAllDistance + DistanceHysteresis);
		float drawMaxEnterSqr = SqrClamped(MaxDrawDistance - DistanceHysteresis);
		float drawMaxExitSqr = SqrClamped(MaxDrawDistance + DistanceHysteresis);
		int num2 = 0;
		for (int j = 0; j < _rigHelpers.Length; j++)
		{
			ref GorillaRigHelper reference2 = ref _rigHelpers[j];
			RigDrawState targetState = GetTargetState(ref reference2, j, drawAllEnterSqr, drawAllExitSqr, drawMaxEnterSqr, drawMaxExitSqr);
			if (targetState != reference2.state)
			{
				if (MaxTransitionsPerSlice > 0 && num2 >= MaxTransitionsPerSlice)
				{
					break;
				}
				UpdateRigState(ref reference2, targetState);
				num2++;
			}
		}
	}

	private RigDrawState GetTargetState(ref GorillaRigHelper helper, int rank, float drawAllEnterSqr, float drawAllExitSqr, float drawMaxEnterSqr, float drawMaxExitSqr)
	{
		if (helper.inactive)
		{
			return helper.state;
		}
		if (helper.IsExcluded)
		{
			return RigDrawState.Min;
		}
		bool flag = helper.state == RigDrawState.All;
		bool flag2 = flag || helper.state == RigDrawState.Partial;
		if (DrawOnPlayerCount)
		{
			int num = (flag ? DrawAllCount : (DrawAllCount - CountHysteresis));
			if (rank < num)
			{
				return RigDrawState.All;
			}
			int num2 = ((helper.state == RigDrawState.Min) ? (DrawMaxCount - CountHysteresis) : DrawMaxCount);
			if (rank >= num2)
			{
				return RigDrawState.Min;
			}
		}
		if (helper.sqrDistance <= (flag ? drawAllExitSqr : drawAllEnterSqr))
		{
			return RigDrawState.All;
		}
		if (helper.sqrDistance <= (flag2 ? drawMaxExitSqr : drawMaxEnterSqr))
		{
			return RigDrawState.Partial;
		}
		return RigDrawState.Min;
	}

	private void UpdateRigState(ref GorillaRigHelper helper, RigDrawState newState)
	{
		RigDrawState state = helper.state;
		if (newState != state)
		{
			switch (newState)
			{
			case RigDrawState.All:
				ToggleRenderersOnRig(helper.rig, toggle: true);
				helper.rig.ToggleMatParticles(enabled: true);
				break;
			case RigDrawState.Partial:
				SetPartialOnRig(helper.rig, state);
				helper.rig.ToggleMatParticles(enabled: false);
				break;
			case RigDrawState.Min:
				ToggleRenderersOnRig(helper.rig, toggle: false);
				helper.rig.ToggleMatParticles(enabled: false);
				break;
			}
			helper.state = newState;
		}
	}

	private void OnRigCosmeticsActivated(VRRig rig)
	{
		if (_rigHelpers == null)
		{
			return;
		}
		for (int i = 0; i < _rigHelpers.Length; i++)
		{
			ref GorillaRigHelper reference = ref _rigHelpers[i];
			if (reference.rig != rig)
			{
				continue;
			}
			if (reference.state != RigDrawState.Startup && reference.state != 0)
			{
				ToggleRenderersOnSet(rig, rig.prevSet, toggle: true);
				reference.state = RigDrawState.Unknown;
				if (!IsThrottling)
				{
					_restorePending = true;
				}
			}
			break;
		}
	}

	private void ToggleRenderersOnRig(VRRig rig, bool toggle)
	{
		ToggleRenderersOnSet(rig, rig.mergedSet, toggle);
	}

	private void ToggleRenderersOnSet(VRRig rig, CosmeticsController.CosmeticSet set, bool toggle)
	{
		if (set?.items == null)
		{
			return;
		}
		int num = set.items.Length;
		for (int i = 0; i < num; i++)
		{
			CosmeticItemInstance cosmeticItemInstance = rig.cosmeticsObjectRegistry.Cosmetic(set.items[i].displayName);
			if (cosmeticItemInstance != null)
			{
				cosmeticItemInstance.ToggleRenderers(toggle);
				cosmeticItemInstance.ToggleParticles(toggle);
			}
		}
	}

	private void SetPartialOnRig(VRRig rig, RigDrawState prevState)
	{
		bool flag = prevState == RigDrawState.All;
		bool flag2 = prevState == RigDrawState.Min;
		CosmeticsController.CosmeticSet mergedSet = rig.mergedSet;
		if (mergedSet?.items == null)
		{
			return;
		}
		int num = mergedSet.items.Length;
		for (int i = 0; i < num; i++)
		{
			CosmeticItemInstance cosmeticItemInstance = rig.cosmeticsObjectRegistry.Cosmetic(mergedSet.items[i].displayName);
			if (cosmeticItemInstance == null)
			{
				continue;
			}
			if (!flag2)
			{
				cosmeticItemInstance.ToggleParticles(enabled: false);
			}
			if (ContainsSlot(cosmeticItemInstance.ActiveSlot))
			{
				if (!flag2)
				{
					cosmeticItemInstance.ToggleRenderers(enabled: false);
				}
			}
			else if (!flag)
			{
				cosmeticItemInstance.ToggleRenderers(enabled: true);
			}
		}
	}

	private static float SqrClamped(float distance)
	{
		distance = Mathf.Max(distance, 0f);
		return distance * distance;
	}

	private void BuildToggleSlotMask()
	{
		_toggleSlotMask = 0;
		if (ToggleSlots != null)
		{
			for (int i = 0; i < ToggleSlots.Length; i++)
			{
				_toggleSlotMask |= 1 << (int)ToggleSlots[i];
			}
		}
	}

	private bool ContainsSlot(CosmeticsController.CosmeticSlots slot)
	{
		return (_toggleSlotMask & (1 << (int)slot)) != 0;
	}

	public void EnableAllRenderers()
	{
		_restorePending = false;
		RestoreRenderers(0);
	}

	private bool RestoreRenderers(int maxRestores)
	{
		if (_rigHelpers == null)
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < _rigHelpers.Length; i++)
		{
			ref GorillaRigHelper reference = ref _rigHelpers[i];
			if (reference.state == RigDrawState.Startup || reference.rig == null)
			{
				continue;
			}
			if (reference.state != 0)
			{
				if (maxRestores > 0 && num >= maxRestores)
				{
					return false;
				}
				ToggleRenderersOnRig(reference.rig, toggle: true);
				reference.rig.ToggleMatParticles(enabled: true);
				num++;
			}
			reference.state = RigDrawState.Startup;
		}
		return true;
	}
}
