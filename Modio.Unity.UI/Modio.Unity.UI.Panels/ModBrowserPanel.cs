using System.Threading.Tasks;
using Modio.Extensions;
using Modio.Unity.UI.Components.Localization;
using Modio.Unity.UI.Input;
using Modio.Unity.UI.Navigation;
using Modio.Unity.UI.Panels.Authentication;
using Modio.Unity.UI.Search;
using Modio.Users;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Panels;

public class ModBrowserPanel : ModioPanelBase
{
	[SerializeField]
	private ModioInputFieldSelectionWrapper _searchField;

	[SerializeField]
	private UnityEvent _openingPanelFromClosed;

	private static bool _isWaitingBeforeAuthFlow;

	protected override void Start()
	{
		base.Start();
		if (!ModioUILocalizationManager.LocalizationExists)
		{
			Debug.LogWarning("Your scene doesn't appear to have a ModioUILocalizationManager or custom localization handler. Consider adding the 'ModioUI_Localisation' prefab to your scene");
		}
	}

	public override void OnGainedFocus(GainedFocusCause selectionBehaviour)
	{
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.Search, OpenSearch);
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.Filter, OpenFilter);
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.Sort, OpenSort);
		ModioUISearch.Default.OnSearchUpdatedUnityEvent.AddListener(HookUpCancelOrClearFilter);
		base.OnGainedFocus(selectionBehaviour);
		HookUpCancelOrClearFilter();
		if (!ModioClient.IsInitialized || User.Current == null || !User.Current.IsAuthenticated)
		{
			if (_isWaitingBeforeAuthFlow)
			{
				return;
			}
			if (selectionBehaviour == GainedFocusCause.RegainingFocusFromStackedPanel)
			{
				ModioLog.Message?.Log("Closing ModBrowserPanel after regaining focus from cancelled login attempt");
				ClosePanel();
				return;
			}
			OpenAuthFlowAfterWaitingIfNeeded().ForgetTaskSafely();
		}
		if (selectionBehaviour == GainedFocusCause.OpeningFromClosed)
		{
			_openingPanelFromClosed.Invoke();
		}
	}

	private async Task OpenAuthFlowAfterWaitingIfNeeded()
	{
		ModioWaitingPanelGeneric waitingPanel = null;
		if (!ModioClient.IsInitialized)
		{
			waitingPanel = ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>();
			waitingPanel?.OpenPanel();
			ModioLog.Warning?.Log("Attempting to open ModBrowserPanel before initializing the plugin and AutoInitialize is disabled");
			Error error = await ModioClient.Init();
			if ((bool)error)
			{
				ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.OpenPanel(error);
				waitingPanel?.ClosePanel();
				return;
			}
		}
		if (User.Current != null)
		{
			if (waitingPanel == null)
			{
				waitingPanel = ModioPanelManager.GetPanelOfType<ModioWaitingPanelGeneric>();
				waitingPanel?.OpenPanel();
			}
			while (User.Current.IsUpdating)
			{
				await Task.Yield();
			}
			_isWaitingBeforeAuthFlow = false;
			if (User.Current.IsAuthenticated)
			{
				waitingPanel?.ClosePanel();
				return;
			}
			if (!ModioClient.IsInitialized)
			{
				waitingPanel?.ClosePanel();
				return;
			}
		}
		_isWaitingBeforeAuthFlow = false;
		ModioPanelManager.GetPanelOfType<ModioAuthenticationPanel>().OpenAuthFlow();
	}

	public override void OnLostFocus()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Search, OpenSearch);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Filter, OpenFilter);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Sort, OpenSort);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.SearchClear, ClearSearch);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Cancel, CancelPressed);
		if (ModioUISearch.Default != null)
		{
			ModioUISearch.Default.OnSearchUpdatedUnityEvent.RemoveListener(HookUpCancelOrClearFilter);
		}
		base.OnLostFocus();
	}

	private void HookUpCancelOrClearFilter()
	{
		if (base.HasFocus)
		{
			ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Cancel, CancelPressed);
			ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.SearchClear, ClearSearch);
			if (ModioUISearch.Default.HasCustomSearch())
			{
				ModioUIInput.AddHandler(ModioUIInput.ModioAction.SearchClear, ClearSearch);
			}
			else
			{
				ModioUIInput.AddHandler(ModioUIInput.ModioAction.Cancel, CancelPressed);
			}
		}
	}

	private void OpenSearch()
	{
		if (_searchField != null)
		{
			_searchField.SelectInputField();
		}
	}

	private void ClearSearch()
	{
		ModioUISearch.Default.ClearSearch();
	}

	private void OpenFilter()
	{
		ModioPanelManager.GetPanelOfType<ModFilterPanel>()?.OpenPanel();
	}

	private void OpenSort()
	{
		ModioPanelManager.GetPanelOfType<ModSortPanel>()?.OpenPanel();
	}
}
