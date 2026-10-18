using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.Extensions;
using Modio.Monetization;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUITokenPurchase : MonoBehaviour
{
	[SerializeField]
	private ModioUITokenPack _referencePack;

	private readonly List<ModioUITokenPack> _currentPacks = new List<ModioUITokenPack>();

	private void Start()
	{
		_referencePack.gameObject.SetActive(value: false);
		ModioClient.OnInitialized += OnPluginInitialized;
	}

	private void OnDestroy()
	{
		ModioClient.OnInitialized -= OnPluginInitialized;
	}

	private void OnPluginInitialized()
	{
		GetCurrencyPacks().ForgetTaskSafely();
	}

	private async Task GetCurrencyPacks()
	{
		if (ModioServices.TryResolve<IModioVirtualCurrencyProviderService>(out var result))
		{
			var (error, sku) = await result.GetCurrencyPackSkus();
			if ((bool)error)
			{
				ModioLog.Error?.Log(error);
			}
			ShowTokenPacks(sku);
		}
	}

	private void ShowTokenPacks(PortalSku[] sku)
	{
		foreach (PortalSku pack in sku)
		{
			ModioUITokenPack modioUITokenPack;
			if (_currentPacks.Count > 0)
			{
				modioUITokenPack = Object.Instantiate(_referencePack, _referencePack.transform.parent);
			}
			else
			{
				modioUITokenPack = _referencePack;
				_referencePack.gameObject.SetActive(value: true);
			}
			modioUITokenPack.SetPack(pack);
			_currentPacks.Add(modioUITokenPack);
		}
		if (_currentPacks.Count == 0)
		{
			Debug.LogError("Unable to find any token packs for the current platform. They must be setup on the Game Admin Settings ");
			_referencePack.gameObject.SetActive(value: false);
		}
	}
}
