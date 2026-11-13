using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.VirtualStumpCustomMaps;

public class CustomMapDonationBooth : MonoBehaviour
{
	private enum DonationSize
	{
		Small,
		Medium,
		Large
	}

	private enum OffersState
	{
		NotLoaded,
		Loading,
		Loaded,
		Failed
	}

	private class DonationOffer
	{
		public string OfferId;

		public string DisplayId;

		public int DisplayIndex;

		public bool PurchaseAllowed;

		public string DisplayName;

		public List<DonationOfferDebit> Debits = new List<DonationOfferDebit>();
	}

	private struct DonationOfferDebit
	{
		public string Name;

		public string InGameId;

		public string DisplayName;

		public int Amount;
	}

	private const string VStumpOfferDisplayKey = "vstump";

	private const string GeodeCurrencyId = "geodes";

	private const string GeodeGlyph = "❸";

	[Tooltip("The booth's default art. Hidden when the mapper's placeholder has \"Use Custom Mesh\" set, so they can build their own decorations around the buttons.")]
	[SerializeField]
	private GameObject shell;

	[SerializeField]
	private string smallOfferSuffix = "c_don_small";

	[SerializeField]
	private string mediumOfferSuffix = "c_don_medium";

	[SerializeField]
	private string largeOfferSuffix = "c_don_large";

	[SerializeField]
	private GorillaPressableButton smallButton;

	[SerializeField]
	private GorillaPressableButton mediumButton;

	[SerializeField]
	private GorillaPressableButton largeButton;

	[Tooltip("Applied to buttons whose donation offer is unavailable. Leave empty to keep the button's current material.")]
	[SerializeField]
	private Material disabledButtonMaterial;

	[Tooltip("If fetching the storefront fails, wait this long before trying again.")]
	[SerializeField]
	private float storefrontRetrySeconds = 30f;

	[SerializeField]
	private int maxStorefrontAttempts = 3;

	[SerializeField]
	private UnityEvent onDonationComplete;

	private Dictionary<string, DonationOffer> cachedOffers;

	private long cachedOffersModId = -1L;

	private string storefrontSummary;

	private OffersState offersState;

	private int storefrontAttempts;

	private Coroutine retryCoroutine;

	private bool donationInFlight;

	private readonly Dictionary<GorillaPressableButton, string> defaultButtonLabels = new Dictionary<GorillaPressableButton, string>();

	private void Awake()
	{
		CacheDefaultLabel(smallButton);
		CacheDefaultLabel(mediumButton);
		CacheDefaultLabel(largeButton);
	}

	private void CacheDefaultLabel(GorillaPressableButton button)
	{
		if (button.IsNotNull() && button.myTmpText.IsNotNull())
		{
			defaultButtonLabels[button] = button.myTmpText.text;
		}
	}

	private void OnEnable()
	{
		donationInFlight = false;
		if (offersState == OffersState.Loaded && cachedOffersModId == CustomMapLoader.LoadedMapModId)
		{
			ApplyOfferAvailability();
		}
		else if (offersState != OffersState.Loading)
		{
			LoadOffers();
		}
	}

	private void OnDisable()
	{
		if (retryCoroutine != null)
		{
			StopCoroutine(retryCoroutine);
			retryCoroutine = null;
		}
		if (offersState == OffersState.Failed)
		{
			storefrontAttempts = 0;
		}
	}

	public void InitializeForCustomMap(bool useCustomMesh)
	{
		if (shell.IsNotNull())
		{
			shell.SetActive(!useCustomMesh);
		}
	}

	public void DonateSmall()
	{
		StartDonation(DonationSize.Small);
	}

	public void DonateMedium()
	{
		StartDonation(DonationSize.Medium);
	}

	public void DonateLarge()
	{
		StartDonation(DonationSize.Large);
	}

	private string GetOfferName(DonationSize size, long modId)
	{
		string arg = size switch
		{
			DonationSize.Small => smallOfferSuffix, 
			DonationSize.Medium => mediumOfferSuffix, 
			_ => largeOfferSuffix, 
		};
		return $"mod.{modId}.{arg}";
	}

	private GorillaPressableButton GetButton(DonationSize size)
	{
		return size switch
		{
			DonationSize.Small => smallButton, 
			DonationSize.Medium => mediumButton, 
			_ => largeButton, 
		};
	}

	private bool TryGetAvailableOffer(DonationSize size, out string offerName, out DonationOffer offer)
	{
		offerName = GetOfferName(size, cachedOffersModId);
		offer = null;
		if (offersState == OffersState.Loaded && cachedOffers != null && cachedOffers.TryGetValue(offerName, out offer))
		{
			return offer.PurchaseAllowed;
		}
		return false;
	}

	private void SetButtonInteractable(GorillaPressableButton button, bool interactable)
	{
		if (!button.IsNull())
		{
			button.enabled = interactable;
			if (interactable)
			{
				button.SetUnpressedMaterial();
			}
			else if (disabledButtonMaterial.IsNotNull())
			{
				button.SetRendererMaterial(disabledButtonMaterial);
			}
		}
	}

	private void SetAllButtonsInteractable(bool interactable)
	{
		SetButtonInteractable(smallButton, interactable);
		SetButtonInteractable(mediumButton, interactable);
		SetButtonInteractable(largeButton, interactable);
	}

	private void SetButtonLabel(GorillaPressableButton button, DonationOffer offer)
	{
		if (button.IsNull() || button.myTmpText.IsNull())
		{
			return;
		}
		int num = 0;
		if (offer != null)
		{
			foreach (DonationOfferDebit debit in offer.Debits)
			{
				if (IsGeodeCurrency(debit.InGameId))
				{
					num += debit.Amount;
				}
			}
		}
		string value;
		if (num > 0)
		{
			button.myTmpText.text = string.Format("{0} {1}", num, "❸");
		}
		else if (defaultButtonLabels.TryGetValue(button, out value))
		{
			button.myTmpText.text = value;
		}
	}

	private void ResetAllButtonLabels()
	{
		SetButtonLabel(smallButton, null);
		SetButtonLabel(mediumButton, null);
		SetButtonLabel(largeButton, null);
	}

	private void ApplyOfferAvailability()
	{
		int num = 0;
		foreach (DonationSize value in Enum.GetValues(typeof(DonationSize)))
		{
			string offerName;
			DonationOffer offer;
			bool flag = TryGetAvailableOffer(value, out offerName, out offer);
			SetButtonInteractable(GetButton(value), flag);
			SetButtonLabel(GetButton(value), flag ? offer : null);
			if (flag)
			{
				num++;
				continue;
			}
			Debug.LogWarning("[CustomMapDonationBooth] Donation offer \"" + offerName + "\" is " + ((offer == null) ? "missing" : "not purchasable") + ", disabling its button");
		}
		if (num < Enum.GetValues(typeof(DonationSize)).Length && !storefrontSummary.IsNullOrEmpty())
		{
			Debug.LogWarning("[CustomMapDonationBooth] " + storefrontSummary);
		}
	}

	private void LoadOffers()
	{
		SetAllButtonsInteractable(interactable: false);
		ResetAllButtonLabels();
		long modId = CustomMapLoader.LoadedMapModId;
		if (modId <= 0)
		{
			offersState = OffersState.Loaded;
			cachedOffers = null;
			cachedOffersModId = modId;
			return;
		}
		offersState = OffersState.Loading;
		storefrontAttempts++;
		if (!MothershipClientApiUnity.GetStorefront(new string[0], delegate(MothershipGetStorefrontResponse Response)
		{
			if (!this.IsNull())
			{
				cachedOffers = new Dictionary<string, DonationOffer>(StringComparer.OrdinalIgnoreCase);
				cachedOffersModId = modId;
				string text = $"mod.{modId}.";
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Storefront from " + MothershipClientApiUnity.MothershipBaseUrl + " (title: " + MothershipClientApiUnity.TitleId + ", environment: " + MothershipClientApiUnity.EnvironmentId + ", deployment: " + MothershipClientApiUnity.DeploymentId + "). Booth only reads the \"vstump\" display and offers named \"" + text + "*\".");
				foreach (KeyValuePair<string, MothershipBoundOfferDisplay> result in Response.Results)
				{
					stringBuilder.Append("\n  display \"" + result.Key + "\" (offer_display_id: " + result.Value.offer_display_id + ")");
					foreach (MothershipNormalizedOffer offer in result.Value.offers)
					{
						stringBuilder.Append("\n    name: " + offer.OfferName + ", id: " + offer.OfferId + ", " + $"purchasable: {offer.PurchaseAllowed}, index: {offer.DisplayIndex}");
					}
				}
				storefrontSummary = stringBuilder.ToString();
				foreach (KeyValuePair<string, MothershipBoundOfferDisplay> result2 in Response.Results)
				{
					if (result2.Key.Equals("vstump", StringComparison.OrdinalIgnoreCase))
					{
						foreach (MothershipNormalizedOffer offer2 in result2.Value.offers)
						{
							bool flag = !offer2.OfferName.IsNullOrEmpty() && offer2.OfferName.StartsWith(text, StringComparison.OrdinalIgnoreCase);
							bool flag2 = !offer2.OfferId.IsNullOrEmpty() && offer2.OfferId.StartsWith(text, StringComparison.OrdinalIgnoreCase);
							if (flag || flag2)
							{
								DonationOffer donationOffer = new DonationOffer
								{
									OfferId = offer2.OfferId,
									DisplayId = result2.Value.offer_display_id,
									DisplayIndex = offer2.DisplayIndex,
									PurchaseAllowed = offer2.PurchaseAllowed,
									DisplayName = offer2.DisplayName
								};
								foreach (KeyValuePair<string, MothershipEntitlementDeltaSummary> personalDebit in offer2.PersonalDebits)
								{
									donationOffer.Debits.Add(new DonationOfferDebit
									{
										Name = personalDebit.Value.name,
										InGameId = personalDebit.Value.in_game_id,
										DisplayName = personalDebit.Value.display_name,
										Amount = Mathf.Abs(personalDebit.Value.change)
									});
								}
								if (flag)
								{
									cachedOffers[offer2.OfferName] = donationOffer;
								}
								if (flag2)
								{
									cachedOffers[offer2.OfferId] = donationOffer;
								}
							}
						}
					}
				}
				offersState = OffersState.Loaded;
				storefrontAttempts = 0;
				if (base.isActiveAndEnabled)
				{
					ApplyOfferAvailability();
				}
			}
		}, delegate(MothershipError Error, int StatusCode)
		{
			if (!this.IsNull())
			{
				Debug.LogError("[CustomMapDonationBooth] Failed to fetch storefront: " + Error.Name + ": " + Error.Message + " " + $"{Error.MothershipErrorCode} Http Status: {StatusCode} Trace ID: {Error.TraceId} " + "Session ID: " + MothershipClientApiUnity.SessionId);
				OnLoadOffersFailed();
			}
		}))
		{
			OnLoadOffersFailed();
		}
	}

	private void OnLoadOffersFailed()
	{
		offersState = OffersState.Failed;
		SetAllButtonsInteractable(interactable: false);
		ResetAllButtonLabels();
		if (storefrontAttempts < maxStorefrontAttempts && base.isActiveAndEnabled)
		{
			retryCoroutine = StartCoroutine(RetryLoadOffers());
		}
	}

	private IEnumerator RetryLoadOffers()
	{
		yield return new WaitForSeconds(storefrontRetrySeconds);
		retryCoroutine = null;
		LoadOffers();
	}

	private void StartDonation(DonationSize size)
	{
		if (!donationInFlight && TryGetAvailableOffer(size, out var offerName, out var offer) && !ConsentScreen.IsBusy)
		{
			donationInFlight = true;
			FetchBalanceAndConfirm(offerName, offer);
		}
	}

	private void FetchBalanceAndConfirm(string offerName, DonationOffer offer)
	{
		if (!MothershipClientApiUnity.GetUserInventory(delegate(MothershipGetInventoryResponse Result)
		{
			if (!this.IsNull())
			{
				List<ConsentScreen.ConsentCost> list = new List<ConsentScreen.ConsentCost>(offer.Debits.Count);
				foreach (DonationOfferDebit debit in offer.Debits)
				{
					int num = 0;
					bool hasBalance = false;
					foreach (KeyValuePair<string, MothershipPlayerInventorySummary> result in Result.Results)
					{
						foreach (MothershipInventoryItemSummary entitlement in result.Value.entitlements)
						{
							if ((!debit.InGameId.IsNullOrEmpty()) ? debit.InGameId.Equals(entitlement.in_game_id, StringComparison.OrdinalIgnoreCase) : (debit.Name == entitlement.name))
							{
								num += entitlement.quantity;
								hasBalance = true;
							}
						}
					}
					list.Add(new ConsentScreen.ConsentCost
					{
						DisplayName = debit.DisplayName,
						Amount = debit.Amount,
						CurrentBalance = num,
						HasBalance = hasBalance
					});
				}
				RequestConsent(offerName, offer, list);
			}
		}, delegate(MothershipError Error, int StatusCode)
		{
			if (!this.IsNull())
			{
				Debug.LogError("[CustomMapDonationBooth] Failed to fetch inventory: " + Error.Name + ": " + Error.Message + " " + $"{Error.MothershipErrorCode} Http Status: {StatusCode} Trace ID: {Error.TraceId} " + "Session ID: " + MothershipClientApiUnity.SessionId);
				FinishDonation();
			}
		}))
		{
			FinishDonation();
		}
	}

	private void RequestConsent(string offerName, DonationOffer offer, List<ConsentScreen.ConsentCost> consentCosts)
	{
		if (ConsentScreen.IsBusy)
		{
			FinishDonation();
			return;
		}
		ConsentScreen.StartConsentFlow(offer.DisplayName.IsNullOrEmpty() ? "Donation" : offer.DisplayName, consentCosts, delegate(bool Consented, Action<string> AsyncWorkComplete)
		{
			if (!Consented)
			{
				AsyncWorkComplete("");
				FinishDonation();
			}
			else if (!MothershipClientApiUnity.PurchaseOffer(offer.DisplayId, offer.OfferId, offer.DisplayIndex, delegate(MothershipPurchaseOfferResponse Response)
			{
				GeodeAtm.InvalidateGeodeBalance();
				AsyncWorkComplete("Thank you for your donation!");
				PostDonationTelemetry(offerName, offer, consentCosts, Response);
				if (!this.IsNull())
				{
					FinishDonation();
					onDonationComplete?.Invoke();
				}
			}, delegate(MothershipError Error, int StatusCode)
			{
				Debug.LogError("[CustomMapDonationBooth] Donation failed: " + Error.Name + ": " + Error.Message + " " + $"{Error.MothershipErrorCode} Http Status: {StatusCode} Offer: {offerName} " + "Trace ID: " + Error.TraceId + " Session ID: " + MothershipClientApiUnity.SessionId);
				AsyncWorkComplete("Donation Failed: " + Error.MothershipErrorCode + ": " + Error.Message);
				if (!this.IsNull())
				{
					FinishDonation();
				}
			}))
			{
				AsyncWorkComplete("Donation Failed");
				FinishDonation();
			}
		});
	}

	private static bool IsGeodeCurrency(string id)
	{
		if (!id.IsNullOrEmpty())
		{
			return id.Equals("geodes", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static void PostDonationTelemetry(string offerName, DonationOffer offer, List<ConsentScreen.ConsentCost> consentCosts, MothershipPurchaseOfferResponse response)
	{
		try
		{
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < offer.Debits.Count; i++)
			{
				if (IsGeodeCurrency(offer.Debits[i].InGameId))
				{
					num += offer.Debits[i].Amount;
					if (num2 < 0 && i < consentCosts.Count && consentCosts[i].HasBalance)
					{
						num2 = consentCosts[i].CurrentBalance;
					}
				}
			}
			if (num <= 0)
			{
				return;
			}
			int num3 = -1;
			foreach (KeyValuePair<string, MothershipEntitlementUpdate> change in response.Changes)
			{
				if (IsGeodeCurrency(change.Value.in_game_id))
				{
					num3 = ((num3 < 0) ? change.Value.quantity : (num3 + change.Value.quantity));
				}
			}
			GorillaTelemetry.PostGeodeSpendEvent(offerName, "", num, num2, num3);
		}
		catch (Exception arg)
		{
			Debug.LogError($"[CustomMapDonationBooth] geode_spend failed to post: {arg}");
		}
	}

	private void FinishDonation()
	{
		donationInFlight = false;
	}
}
