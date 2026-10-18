using System;
using System.Threading.Tasks;
using Modio.Mods;
using Modio.Unity.UI.Components.Localization;
using Modio.Unity.UI.Panels;
using Modio.Unity.UI.Panels.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertySubscriptionToggle : IModProperty
{
	[SerializeField]
	private Button _subscribeButton;

	[SerializeField]
	private Toggle _subscribeToggle;

	[SerializeField]
	private Button _unsubscribeButton;

	[SerializeField]
	private Button _purchaseButton;

	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private ModioUILocalizedText _localisedText;

	[SerializeField]
	private bool _dependenciesAreConfirmed;

	private Mod _mod;

	public void OnModUpdate(Mod mod)
	{
		_mod = mod;
		if (_text != null)
		{
			_text.text = (mod.IsSubscribed ? "UNSUBSCRIBE" : "SUBSCRIBE");
		}
		if (_localisedText != null)
		{
			_localisedText.SetKey(mod.IsSubscribed ? "modio_btn_unsubscribe" : "modio_btn_subscribe");
		}
		bool flag = mod.IsMonetized && !mod.IsPurchased;
		if (_purchaseButton != null)
		{
			_purchaseButton.onClick.RemoveListener(PurchaseButtonClicked);
			_purchaseButton.gameObject.SetActive(flag);
			_purchaseButton.onClick.AddListener(PurchaseButtonClicked);
		}
		if (_subscribeToggle != null)
		{
			_subscribeToggle.onValueChanged.RemoveListener(SubscribeToggleValueChanged);
			_subscribeToggle.isOn = mod.IsSubscribed;
			_subscribeToggle.onValueChanged.AddListener(SubscribeToggleValueChanged);
			_subscribeToggle.gameObject.SetActive(!flag);
		}
		if (_subscribeButton != null)
		{
			_subscribeButton.onClick.RemoveListener(SubscribeButtonClicked);
			_subscribeButton.onClick.AddListener(SubscribeButtonClicked);
			_subscribeButton.gameObject.SetActive(!flag && (_unsubscribeButton == null || !_mod.IsSubscribed));
		}
		if (_unsubscribeButton != null)
		{
			_unsubscribeButton.onClick.RemoveListener(SubscribeButtonClicked);
			_unsubscribeButton.onClick.AddListener(SubscribeButtonClicked);
			_unsubscribeButton.gameObject.SetActive(!flag && _mod.IsSubscribed);
		}
	}

	private void SubscribeButtonClicked()
	{
		UpdateSubscribed(!_mod.IsSubscribed);
	}

	private void SubscribeToggleValueChanged(bool arg0)
	{
		UpdateSubscribed(_subscribeToggle.isOn);
	}

	private void UpdateSubscribed(bool shouldBeSubscribed)
	{
		if (shouldBeSubscribed && _mod.Dependencies.HasDependencies)
		{
			if (_dependenciesAreConfirmed)
			{
				Task<Error> task = _mod.Subscribe();
				ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.MonitorTaskThenOpenPanelIfError(task);
				if (_subscribeToggle != null)
				{
					_subscribeToggle.SetIsOnWithoutNotify(_mod.IsSubscribed);
				}
				return;
			}
			ModDependenciesPanel panelOfType = ModioPanelManager.GetPanelOfType<ModDependenciesPanel>();
			if (panelOfType != null)
			{
				panelOfType.IsSubscribeFlow(isSubscribe: true);
				panelOfType.OpenPanel(_mod);
				if (_subscribeToggle != null)
				{
					_subscribeToggle.SetIsOnWithoutNotify(_mod.IsSubscribed);
				}
				return;
			}
		}
		Task<Error> task2 = (shouldBeSubscribed ? _mod.Subscribe() : _mod.Unsubscribe());
		if (_subscribeToggle != null)
		{
			_subscribeToggle.SetIsOnWithoutNotify(_mod.IsSubscribed);
		}
		ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.MonitorTaskThenOpenPanelIfError(task2);
	}

	private void PurchaseButtonClicked()
	{
		ModioPanelManager.GetPanelOfType<ModioConfirmPurchasePanel>().OpenPanel(_mod);
	}
}
