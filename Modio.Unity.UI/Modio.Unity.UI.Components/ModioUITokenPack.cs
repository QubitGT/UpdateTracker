using System;
using System.Threading.Tasks;
using Modio.Errors;
using Modio.Monetization;
using Modio.Unity.UI.Panels;
using Modio.Unity.UI.Panels.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components;

public class ModioUITokenPack : MonoBehaviour
{
	[Serializable]
	private class ValueImageMap
	{
		public int value;

		public Sprite image;
	}

	[SerializeField]
	private TMP_Text _amount;

	[SerializeField]
	private TMP_Text _price;

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private Image _icon;

	private PortalSku _tokenPack;

	[SerializeField]
	private ValueImageMap[] _valuesToImages;

	public void SetPack(PortalSku sku)
	{
		_tokenPack = sku;
		if (_amount != null)
		{
			_amount.text = _tokenPack.Value.ToString();
		}
		if (_price != null)
		{
			_price.text = sku.FormattedPrice;
		}
		if (_name != null)
		{
			_name.text = sku.Name;
		}
		if (_icon != null)
		{
			_icon.sprite = GetImageForValue(sku.Value);
		}
	}

	public void OnPressedPurchase()
	{
		if (!ModioServices.TryResolve<IModioVirtualCurrencyProviderService>(out var result))
		{
			ModioPanelManager.GetPanelOfType<ModioBuyTokensPanel>().ClosePanel();
			return;
		}
		Task<Error> task = result.OpenCheckoutFlow(_tokenPack);
		if (task == null)
		{
			return;
		}
		ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>()?.OpenAndWaitFor(task, delegate(Error error)
		{
			if ((bool)error)
			{
				if (error.Code == ErrorCode.OPERATION_CANCELLED)
				{
					return;
				}
				ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.OpenPanel(error);
			}
			ModioPanelManager.GetPanelOfType<ModioBuyTokensPanel>()?.ClosePanel();
		});
	}

	private Sprite GetImageForValue(int amount)
	{
		ValueImageMap[] valuesToImages = _valuesToImages;
		foreach (ValueImageMap valueImageMap in valuesToImages)
		{
			if (valueImageMap.value == amount)
			{
				return valueImageMap.image;
			}
		}
		ModioLog.Warning?.Log($"No image mapped for token pack value [{amount}]!");
		return null;
	}
}
