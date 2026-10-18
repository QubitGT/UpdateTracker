using System.Threading.Tasks;
using Modio.Mods;
using Modio.Reports;
using Modio.Unity.UI.Components;
using Modio.Unity.UI.Components.Selectables;
using Modio.Unity.UI.Navigation;
using Modio.Users;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Panels.Report;

public class ModioReportDetailsPanel : ModioPanelBase
{
	private ReportType _reportType;

	[SerializeField]
	private TMP_InputField _email;

	[SerializeField]
	private TMP_InputField _description;

	[SerializeField]
	private ModioUIButton _disableWhenInvalidToSubmit;

	private ModioUIMod _modioUIMod;

	private Mod _lastMod;

	protected override void Start()
	{
		base.Start();
		_description.onValueChanged.AddListener(OnDescriptionTextChanged);
		OnDescriptionTextChanged(_description.text);
		_modioUIMod = GetComponentInParent<ModioUIMod>();
		if (_modioUIMod != null)
		{
			_modioUIMod.onModUpdate.AddListener(OnModUpdated);
			OnModUpdated();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_modioUIMod != null)
		{
			_modioUIMod.onModUpdate.RemoveListener(OnModUpdated);
		}
	}

	private void OnModUpdated()
	{
		if (_lastMod != _modioUIMod?.Mod)
		{
			_lastMod = _modioUIMod?.Mod;
			_description.text = "";
		}
	}

	private void OnDescriptionTextChanged(string description)
	{
		_disableWhenInvalidToSubmit.interactable = !string.IsNullOrEmpty(description);
		ModioGridNavigation componentInParent = _disableWhenInvalidToSubmit.GetComponentInParent<ModioGridNavigation>();
		if (componentInParent != null)
		{
			componentInParent.NeedsNavigationCorrection();
		}
	}

	public void OpenPanel(ReportType type)
	{
		_reportType = type;
		OpenPanel();
	}

	public void OnUserPressedBackButton()
	{
		ClosePanel();
		ModioPanelManager.GetPanelOfType<ModioReportTypePanel>().OpenPanel();
	}

	public void OnUserSubmittedReportDetails()
	{
		ClosePanel();
		if (User.Current != null)
		{
			Task<Error> task = _modioUIMod.Mod.Report(_reportType, ModNotWorkingReason.None, _email.text, _description.text);
			ModioPanelManager.GetPanelOfType<ModioReportWaitingPanel>().OpenAndWaitFor(task, ReportCompleted);
		}
	}

	private void ReportCompleted(Error error)
	{
		if ((bool)error)
		{
			ModioPanelManager.GetPanelOfType<ModioReportErrorPanel>()?.OpenPanel(error);
		}
		else
		{
			ModioPanelManager.GetPanelOfType<ModioReportConfirmationPanel>()?.OpenPanel();
		}
	}
}
