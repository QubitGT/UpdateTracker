using System.Threading.Tasks;
using Modio.Errors;
using Modio.Mods;
using Modio.Unity.UI.Components;
using UnityEngine;

namespace Modio.Unity.UI.Panels.Monetization;

public class ModioConfirmPurchasePanel : ModioPanelBase
{
	private ModioUIMod _modioUIMod;

	[SerializeField]
	private bool _subscribeOnPurchase = true;

	protected override void Awake()
	{
		base.Awake();
		_modioUIMod = GetComponent<ModioUIMod>();
	}

	public void OpenPanel(Mod mod)
	{
		OpenPanel();
		_modioUIMod.SetMod(mod);
	}

	public void ConfirmPurchase()
	{
		ConfirmPurchaseFlow();
	}

	private async void ConfirmPurchaseFlow()
	{
		Task<Error> task = _modioUIMod.Mod.Purchase(_subscribeOnPurchase);
		Error error = await ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>().OpenAndWaitForAsync(task);
		if (error.Code != ErrorCode.NONE)
		{
			ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>().OpenPanel(error);
		}
		else
		{
			ClosePanel();
		}
	}
}
