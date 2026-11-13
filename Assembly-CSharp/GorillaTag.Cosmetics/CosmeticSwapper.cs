using System.Collections.Generic;
using GorillaGameModes;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics;

public class CosmeticSwapper : MonoBehaviour, ITickSystemTick
{
	private enum SwapMode
	{
		AllAtOnce,
		StepByStep,
		Random
	}

	private struct CosmeticState
	{
		public string cosmeticId;

		public string itemName;

		public CosmeticsController.CosmeticSlots slot;

		public bool claimed;

		public bool alreadyWorn;
	}

	[SerializeField]
	private List<string> cosmeticIDs = new List<string>();

	[SerializeField]
	private SwapMode swapMode = SwapMode.StepByStep;

	[Tooltip("Optional. When assigned, TriggerSwap sources the cosmetic ID from the cycle controller's active sub-item instead of the cosmeticIDs list. Use SwapMode.Random to call CycleRandom() automatically on each hit before reading the active sub-item.")]
	[SerializeField]
	private SubCosmeticCycleController cycleController;

	[SerializeField]
	private float stepTimeout = 10f;

	[Tooltip("Hold final step as long as the swapper is being called within the timeframe")]
	[SerializeField]
	private bool holdFinalStep = true;

	[SerializeField]
	private UnityEvent<VRRig> OnSwappingSequenceCompleted;

	[SerializeField]
	private List<GameModeType> gameModeExclusion = new List<GameModeType>();

	[Tooltip("Invoked on every client by SwapInCosmeticIfNotSwapped when the target is not already wearing any of this swapper's cosmetics. Hook one-shot hit effects (sound, particles) here so they don't replay while touching someone who is already swapped.")]
	[SerializeField]
	private UnityEvent<VRRig> onSwapAccepted;

	private CosmeticsController controller;

	private Stack<CosmeticState> newSwappedCosmetics = new Stack<CosmeticState>();

	private float lastCosmeticSwapTime = float.PositiveInfinity;

	private bool isAtFinalCosmeticStep;

	private static readonly Dictionary<string, int> s_pendingSwapClaims = new Dictionary<string, int>();

	private int CosmeticStepIndex => newSwappedCosmetics.Count;

	private bool HasPendingRestore
	{
		get
		{
			if (newSwappedCosmetics.Count > 0)
			{
				return swapMode != SwapMode.Random;
			}
			return false;
		}
	}

	public bool TickRunning { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void StaticReset()
	{
		s_pendingSwapClaims.Clear();
	}

	private static void AddClaim(string itemName)
	{
		s_pendingSwapClaims.TryGetValue(itemName, out var value);
		s_pendingSwapClaims[itemName] = value + 1;
	}

	private static bool ReleaseClaim(string itemName)
	{
		if (!s_pendingSwapClaims.TryGetValue(itemName, out var value))
		{
			return true;
		}
		if (value <= 1)
		{
			s_pendingSwapClaims.Remove(itemName);
			return true;
		}
		s_pendingSwapClaims[itemName] = value - 1;
		return false;
	}

	private void Awake()
	{
		controller = CosmeticsController.instance;
	}

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
		PlayerCosmeticsSystem.UnlockTemporaryCosmeticsGlobal(cosmeticIDs);
	}

	private void OnDisable()
	{
		PlayerCosmeticsSystem.LockTemporaryCosmeticsGlobal(cosmeticIDs);
		if (!HasPendingRestore)
		{
			TickSystem<object>.RemoveTickCallback(this);
		}
	}

	private void OnDestroy()
	{
		TickSystem<object>.RemoveTickCallback(this);
		if (controller != null && HasPendingRestore)
		{
			RestoreAllPreviousCosmetics();
		}
	}

	public void SwapInCosmetic(VRRig vrRig)
	{
		TriggerSwap(vrRig);
	}

	public void SwapInCosmeticIfNotSwapped(VRRig vrRig)
	{
		if (!(vrRig == null) && !IsExcludedGameMode() && (swapMode != 0 || (!IsWearingAnySwapCosmetic(vrRig) && HasAnythingToSwapIn(vrRig))))
		{
			onSwapAccepted?.Invoke(vrRig);
			TriggerSwap(vrRig);
		}
	}

	private bool IsExcludedGameMode()
	{
		if (GorillaGameManager.instance != null)
		{
			return gameModeExclusion.Contains(GorillaGameManager.instance.GameType());
		}
		return false;
	}

	private bool IsWearingAnySwapCosmetic(VRRig rig)
	{
		if (controller == null || cycleController != null || rig.cosmeticSet?.items == null)
		{
			return false;
		}
		bool flag = rig == GorillaTagger.Instance.offlineVRRig;
		CosmeticsController.CosmeticItem[] items = rig.cosmeticSet.items;
		for (int i = 0; i < items.Length; i++)
		{
			CosmeticsController.CosmeticItem cosmeticItem = items[i];
			if (cosmeticItem.isNullItem || (flag ? (controller.currentWornSet.items[i].itemName == cosmeticItem.itemName) : rig.HasCosmetic(cosmeticItem.itemName)))
			{
				continue;
			}
			foreach (string cosmeticID in cosmeticIDs)
			{
				if (FindItem(cosmeticID).itemName == cosmeticItem.itemName)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasAnythingToSwapIn(VRRig rig)
	{
		if (controller == null || cycleController != null || rig.cosmeticSet?.items == null)
		{
			return true;
		}
		CosmeticsController.CosmeticItem[] items = rig.cosmeticSet.items;
		foreach (string cosmeticID in cosmeticIDs)
		{
			string itemName = FindItem(cosmeticID).itemName;
			bool flag = false;
			for (int i = 0; i < items.Length; i++)
			{
				if (!items[i].isNullItem && items[i].itemName == itemName)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	private SwapMode GetCurrentMode()
	{
		return swapMode;
	}

	private bool ShouldHoldFinalStep()
	{
		return holdFinalStep;
	}

	public int GetCurrentStepIndex(VRRig rig)
	{
		if (rig == null)
		{
			return 0;
		}
		return CosmeticStepIndex;
	}

	public int GetNumberOfSteps()
	{
		if (cycleController != null)
		{
			return cycleController.Count;
		}
		return cosmeticIDs.Count;
	}

	private void TriggerSwap(VRRig rig)
	{
		if (IsExcludedGameMode() || rig == null || controller == null || rig != GorillaTagger.Instance.offlineVRRig)
		{
			return;
		}
		if (cycleController != null)
		{
			if (swapMode == SwapMode.Random)
			{
				cycleController.CycleRandom();
			}
			string appliedCosmeticID = cycleController.GetAppliedCosmeticID();
			if (!string.IsNullOrEmpty(appliedCosmeticID))
			{
				CosmeticState? cosmeticState = SwapInCosmeticWithReturn(appliedCosmeticID, rig);
				if (cosmeticState.HasValue)
				{
					AddNewSwappedCosmetic(cosmeticState.Value);
				}
			}
		}
		else
		{
			if (cosmeticIDs.Count == 0)
			{
				return;
			}
			if (swapMode == SwapMode.Random)
			{
				string nameOrId = cosmeticIDs[Random.Range(0, cosmeticIDs.Count)];
				CosmeticState? cosmeticState2 = SwapInCosmeticWithReturn(nameOrId, rig);
				if (cosmeticState2.HasValue)
				{
					AddNewSwappedCosmetic(cosmeticState2.Value);
				}
				return;
			}
			if (swapMode == SwapMode.AllAtOnce)
			{
				if (IsWearingAnySwapCosmetic(rig))
				{
					return;
				}
				if (newSwappedCosmetics.Count > 0)
				{
					if (AnySwappedCosmeticStillApplied())
					{
						return;
					}
					DiscardSwappedCosmetics();
				}
				{
					foreach (string cosmeticID in cosmeticIDs)
					{
						CosmeticState? cosmeticState3 = SwapInCosmeticWithReturn(cosmeticID, rig);
						if (cosmeticState3.HasValue)
						{
							AddNewSwappedCosmetic(cosmeticState3.Value);
						}
					}
					return;
				}
			}
			int cosmeticStepIndex = CosmeticStepIndex;
			if (cosmeticStepIndex >= cosmeticIDs.Count)
			{
				if (holdFinalStep && isAtFinalCosmeticStep)
				{
					MarkFinalCosmeticStep();
				}
			}
			else
			{
				if (cosmeticStepIndex < 0)
				{
					return;
				}
				string nameOrId2 = cosmeticIDs[cosmeticStepIndex];
				CosmeticState? cosmeticState4 = SwapInCosmeticWithReturn(nameOrId2, rig, trackAlreadyWorn: true);
				if (!cosmeticState4.HasValue)
				{
					return;
				}
				AddNewSwappedCosmetic(cosmeticState4.Value);
				if (cosmeticStepIndex == cosmeticIDs.Count - 1)
				{
					if (holdFinalStep)
					{
						MarkFinalCosmeticStep();
					}
					if (OnSwappingSequenceCompleted != null)
					{
						OnSwappingSequenceCompleted.Invoke(rig);
					}
				}
				else
				{
					UnmarkFinalCosmeticStep();
				}
			}
		}
	}

	private CosmeticState? SwapInCosmeticWithReturn(string nameOrId, VRRig rig, bool trackAlreadyWorn = false)
	{
		if (controller == null)
		{
			return null;
		}
		CosmeticsController.CosmeticItem cosmeticItem = FindItem(nameOrId);
		if (cosmeticItem.isNullItem)
		{
			Debug.LogWarning("Cosmetic not found: " + nameOrId);
			return null;
		}
		bool isLeftHand;
		CosmeticsController.CosmeticSlots cosmeticSlot = GetCosmeticSlot(cosmeticItem, out isLeftHand);
		if (cosmeticSlot == CosmeticsController.CosmeticSlots.Count)
		{
			Debug.LogWarning("Could not determine slot for: " + cosmeticItem.displayName);
			return null;
		}
		CosmeticsController.CosmeticItem cosmeticItem2 = controller.currentWornSet.items[(int)cosmeticSlot];
		CosmeticState value;
		if (!cosmeticItem2.isNullItem && cosmeticItem2.itemName == cosmeticItem.itemName)
		{
			if (!trackAlreadyWorn)
			{
				return null;
			}
			value = default(CosmeticState);
			value.cosmeticId = nameOrId;
			value.itemName = cosmeticItem.itemName;
			value.slot = cosmeticSlot;
			value.alreadyWorn = true;
			return value;
		}
		controller.ApplyCosmeticItemToSet(controller.tempUnlockedSet, cosmeticItem, isLeftHand, applyToPlayerPrefs: false);
		controller.UpdateWornCosmetics(sync: true);
		value = default(CosmeticState);
		value.cosmeticId = nameOrId;
		value.itemName = cosmeticItem.itemName;
		value.slot = cosmeticSlot;
		return value;
	}

	private bool AnySwappedCosmeticStillApplied()
	{
		foreach (CosmeticState newSwappedCosmetic in newSwappedCosmetics)
		{
			if (controller.tempUnlockedSet.items[(int)newSwappedCosmetic.slot].itemName == newSwappedCosmetic.itemName)
			{
				return true;
			}
		}
		return false;
	}

	private void DiscardSwappedCosmetics()
	{
		while (newSwappedCosmetics.Count > 0)
		{
			CosmeticState cosmeticState = newSwappedCosmetics.Pop();
			if (cosmeticState.claimed)
			{
				ReleaseClaim(cosmeticState.itemName);
			}
		}
		lastCosmeticSwapTime = float.PositiveInfinity;
		isAtFinalCosmeticStep = false;
	}

	public void RestoreAllPreviousCosmetics()
	{
		while (newSwappedCosmetics.Count > 0)
		{
			CosmeticState state = newSwappedCosmetics.Pop();
			RestorePreviousCosmetic(state);
		}
	}

	private void RestorePreviousCosmetic(CosmeticState state)
	{
		if (!state.alreadyWorn && !(state.claimed ? (!ReleaseClaim(state.itemName)) : s_pendingSwapClaims.ContainsKey(state.itemName)) && !(controller == null))
		{
			CosmeticsController.CosmeticItem cosmeticItem = FindItem(state.cosmeticId);
			if (!cosmeticItem.isNullItem)
			{
				controller.RemoveCosmeticItemFromSet(controller.tempUnlockedSet, cosmeticItem.displayName, applyToPlayerPrefs: false);
				controller.UpdateWornCosmetics(sync: true);
			}
		}
	}

	private CosmeticsController.CosmeticItem FindItem(string nameOrId)
	{
		if (controller.allCosmeticsDict.TryGetValue(nameOrId, out var value))
		{
			return value;
		}
		if (controller.allCosmeticsItemIDsfromDisplayNamesDict.TryGetValue(nameOrId, out var value2))
		{
			return controller.GetItemFromDict(value2);
		}
		return controller.nullItem;
	}

	private CosmeticsController.CosmeticSlots GetCosmeticSlot(CosmeticsController.CosmeticItem item, out bool isLeftHand)
	{
		isLeftHand = false;
		if (item.isHoldable)
		{
			CosmeticsController.CosmeticSet currentWornSet = controller.currentWornSet;
			CosmeticsController.CosmeticItem cosmeticItem = currentWornSet.items[7];
			CosmeticsController.CosmeticItem cosmeticItem2 = currentWornSet.items[8];
			if (cosmeticItem.isNullItem || (!cosmeticItem2.isNullItem && item.itemName == cosmeticItem.itemName))
			{
				isLeftHand = true;
			}
			if (!isLeftHand)
			{
				return CosmeticsController.CosmeticSlots.HandRight;
			}
			return CosmeticsController.CosmeticSlots.HandLeft;
		}
		return CosmeticsController.CategoryToNonTransferrableSlot(item.itemCategory);
	}

	public void Tick()
	{
		if (newSwappedCosmetics.Count > 0)
		{
			if (GetCurrentMode() == SwapMode.StepByStep)
			{
				if (isAtFinalCosmeticStep && ShouldHoldFinalStep())
				{
					if (Time.time - lastCosmeticSwapTime <= stepTimeout)
					{
						return;
					}
					isAtFinalCosmeticStep = false;
				}
				if (Time.time - lastCosmeticSwapTime > stepTimeout)
				{
					RestoreAllPreviousCosmetics();
					isAtFinalCosmeticStep = false;
					lastCosmeticSwapTime = float.PositiveInfinity;
				}
			}
			else if (GetCurrentMode() == SwapMode.AllAtOnce && Time.time - lastCosmeticSwapTime > stepTimeout)
			{
				RestoreAllPreviousCosmetics();
				lastCosmeticSwapTime = float.PositiveInfinity;
				isAtFinalCosmeticStep = false;
			}
		}
		if (!HasPendingRestore && !base.isActiveAndEnabled)
		{
			TickSystem<object>.RemoveTickCallback(this);
		}
	}

	private void AddNewSwappedCosmetic(CosmeticState state)
	{
		state.claimed = swapMode != SwapMode.Random && !state.alreadyWorn;
		if (state.claimed)
		{
			AddClaim(state.itemName);
		}
		newSwappedCosmetics.Push(state);
		lastCosmeticSwapTime = Time.time;
	}

	private void MarkFinalCosmeticStep()
	{
		isAtFinalCosmeticStep = true;
		lastCosmeticSwapTime = Time.time;
	}

	private void UnmarkFinalCosmeticStep()
	{
		isAtFinalCosmeticStep = false;
	}
}
