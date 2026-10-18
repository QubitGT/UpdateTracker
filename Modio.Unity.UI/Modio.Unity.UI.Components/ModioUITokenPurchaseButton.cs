using System.Threading.Tasks;
using Modio.Monetization;
using Modio.Unity.UI.Input;
using Modio.Unity.UI.Panels;
using Modio.Unity.UI.Panels.Monetization;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUITokenPurchaseButton : MonoBehaviour
{
	private ModioPanelBase _panel;

	private void Awake()
	{
		_panel = GetComponentInParent<ModioPanelBase>();
	}

	private void OnEnable()
	{
		if (_panel != null)
		{
			_panel.OnHasFocusChanged += OnHasFocusChanged;
		}
		else
		{
			OnHasFocusChanged(panelHasFocus: true);
		}
	}

	private void OnDisable()
	{
		if (_panel != null)
		{
			_panel.OnHasFocusChanged -= OnHasFocusChanged;
		}
		OnHasFocusChanged(panelHasFocus: false);
	}

	private void OnHasFocusChanged(bool panelHasFocus)
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.BuyTokens, OpenTokens);
		if (panelHasFocus)
		{
			ModioUIInput.AddHandler(ModioUIInput.ModioAction.BuyTokens, OpenTokens);
		}
	}

	private void OpenTokens()
	{
		if (ModioClient.AuthService == null)
		{
			ModioLog.Error?.Log("No IModioAuthService is bound! Cannot auth");
			return;
		}
		if (ModioServices.TryResolve<IModioVirtualCurrencyProviderService>(out var _))
		{
			ModioBuyTokensPanel panelOfType = ModioPanelManager.GetPanelOfType<ModioBuyTokensPanel>();
			if (panelOfType != null)
			{
				panelOfType.OpenPanel();
				return;
			}
		}
		if (!ModioServices.TryResolve<IModioStorefrontService>(out var result2))
		{
			ModioLog.Error?.Log("No IModioStorefrontService found, unable to open store front.");
			return;
		}
		Task<Error> task = result2.OpenPlatformPurchaseFlow();
		if (task != null)
		{
			ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>()?.OpenAndWaitFor(task, PlatformPurchaseFlowCompleted);
		}
	}

	private void PlatformPurchaseFlowCompleted(Error error)
	{
		if ((bool)error)
		{
			ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.OpenPanel(error);
		}
	}
}
