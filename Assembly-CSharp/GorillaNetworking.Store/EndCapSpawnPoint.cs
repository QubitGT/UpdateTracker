using Cosmetics;
using UnityEngine;

namespace GorillaNetworking.Store;

public class EndCapSpawnPoint : MonoBehaviour
{
	[Tooltip("Optional. When set, this end cap is only live while the TitleDataActivation's title data window is active. Use a TitleDataActivation with no children of its own, since it toggles all of its children.")]
	[SerializeField]
	private TitleDataActivation titleDataActivation;

	[Tooltip("Objects turned off while outside the active date range (i.e. the cosmetic stands).")]
	[SerializeField]
	private GameObject[] objectsToDisableWhenInactive;

	[Tooltip("Bundle purchase buttons locked while outside the active date range.")]
	[SerializeField]
	private BundlePurchaseButton[] purchaseButtonsToLockWhenInactive;

	[Tooltip("Also lock any BundlePurchaseButtons parented under this end cap (e.g. on spawned bundle stands).")]
	[SerializeField]
	private bool includeChildPurchaseButtons = true;

	[Tooltip("Other pressable buttons disabled while outside the active date range.")]
	[SerializeField]
	private GorillaPressableButton[] buttonsToDisableWhenInactive;

	[Tooltip("Creator code terminal for this end cap. Auto-filled from children (including inactive) when empty.")]
	[SerializeField]
	private CreatorCodeTerminal creatorCodeTerminal;

	[Tooltip("When off, the creator code terminal is kept disabled, bundle stands here purchase without a creator code, and the missing Creator Code Provider warnings are suppressed for them.")]
	[SerializeField]
	private bool useCreatorCodeTerminal = true;

	private bool HasTitleDataGate => titleDataActivation != null;

	public bool IsEndCapActive
	{
		get
		{
			if (HasTitleDataGate)
			{
				return titleDataActivation.IsActive;
			}
			return true;
		}
	}

	public CreatorCodeTerminal CreatorCodeTerminal => creatorCodeTerminal;

	public bool UsesCreatorCodeTerminal => useCreatorCodeTerminal;

	private void OnEnable()
	{
		if (!HasTitleDataGate)
		{
			SetCreatorCodeTerminalActive(isActive: true);
			return;
		}
		titleDataActivation.OnActivationStateChanged += OnTitleDataActivationChanged;
		ApplyActiveState(titleDataActivation.IsActive);
	}

	private void OnDisable()
	{
		if (HasTitleDataGate)
		{
			titleDataActivation.OnActivationStateChanged -= OnTitleDataActivationChanged;
		}
	}

	private void OnTitleDataActivationChanged(bool isActive)
	{
		ApplyActiveState(isActive);
	}

	private void ApplyActiveState(bool isActive)
	{
		SetObjectsActive(isActive);
		SetPurchaseButtonsLocked(!isActive);
		SetButtonsEnabled(isActive);
		SetCreatorCodeTerminalActive(isActive);
		ApplyTextSwaps(isActive);
	}

	private void SetCreatorCodeTerminalActive(bool isActive)
	{
		if (!(creatorCodeTerminal == null))
		{
			creatorCodeTerminal.gameObject.SetActive(isActive && useCreatorCodeTerminal);
		}
	}

	private void SetObjectsActive(bool isActive)
	{
		if (objectsToDisableWhenInactive == null)
		{
			return;
		}
		for (int i = 0; i < objectsToDisableWhenInactive.Length; i++)
		{
			if (objectsToDisableWhenInactive[i] != null)
			{
				objectsToDisableWhenInactive[i].SetActive(isActive);
			}
		}
	}

	private void SetPurchaseButtonsLocked(bool locked)
	{
		if (purchaseButtonsToLockWhenInactive != null)
		{
			for (int i = 0; i < purchaseButtonsToLockWhenInactive.Length; i++)
			{
				if (purchaseButtonsToLockWhenInactive[i] != null)
				{
					purchaseButtonsToLockWhenInactive[i].SetPurchaseLocked(locked);
				}
			}
		}
		if (includeChildPurchaseButtons)
		{
			BundlePurchaseButton[] componentsInChildren = GetComponentsInChildren<BundlePurchaseButton>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].SetPurchaseLocked(locked);
			}
		}
	}

	private void SetButtonsEnabled(bool isEnabled)
	{
		if (buttonsToDisableWhenInactive == null)
		{
			return;
		}
		for (int i = 0; i < buttonsToDisableWhenInactive.Length; i++)
		{
			GorillaPressableButton gorillaPressableButton = buttonsToDisableWhenInactive[i];
			if (!(gorillaPressableButton == null))
			{
				gorillaPressableButton.enabled = isEnabled;
				if (gorillaPressableButton.TryGetComponent<Collider>(out var component))
				{
					component.enabled = isEnabled;
				}
			}
		}
	}

	private void ApplyTextSwaps(bool isActive)
	{
	}
}
