using System.Threading.Tasks;
using Modio.Mods;
using Modio.Unity.UI.Components;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModDependenciesPanel : ModioPanelBase
{
	private ModioUIMod _modioUIMod;

	[SerializeField]
	private GameObject[] _showForSubscribeFlowOnly;

	[SerializeField]
	private GameObject[] _hideForSubscribeFlow;

	protected override void Awake()
	{
		base.Awake();
		_modioUIMod = GetComponent<ModioUIMod>();
		IsSubscribeFlow(isSubscribe: false);
	}

	public void OpenPanel(ModioUIMod mod)
	{
		OpenPanel(mod.Mod);
	}

	public void OpenPanel(Mod mod)
	{
		OpenPanel();
		_modioUIMod.SetMod(mod);
	}

	public void IsSubscribeFlow(bool isSubscribe)
	{
		GameObject[] showForSubscribeFlowOnly = _showForSubscribeFlowOnly;
		for (int i = 0; i < showForSubscribeFlowOnly.Length; i++)
		{
			showForSubscribeFlowOnly[i].SetActive(isSubscribe);
		}
		showForSubscribeFlowOnly = _hideForSubscribeFlow;
		for (int i = 0; i < showForSubscribeFlowOnly.Length; i++)
		{
			showForSubscribeFlowOnly[i].SetActive(!isSubscribe);
		}
	}

	public void ConfirmPressed()
	{
		SubscribeWithDependenciesAndHandleResult();
	}

	private async void SubscribeWithDependenciesAndHandleResult()
	{
		Task<Error> task = _modioUIMod.Mod.Subscribe();
		ModioWaitingPanelGeneric panelOfType = ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>();
		Error error = ((!(panelOfType != null)) ? (await task) : (await panelOfType.OpenAndWaitForAsync(task)));
		ClosePanel();
		if ((bool)error)
		{
			ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.OpenPanel(error);
		}
	}
}
