using System;
using System.Threading.Tasks;
using Modio.Mods;
using Modio.Unity.UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyRatingsToggles : IModProperty
{
	[SerializeField]
	private Toggle _positiveVoteToggle;

	[SerializeField]
	private Toggle _negativeVoteToggle;

	private Mod _mod;

	public void OnModUpdate(Mod mod)
	{
		_mod = mod;
		ModRating currentUserRating = mod.CurrentUserRating;
		_positiveVoteToggle.onValueChanged.RemoveListener(PositiveToggleValueChanged);
		_negativeVoteToggle.onValueChanged.RemoveListener(NegativeToggleValueChanged);
		_positiveVoteToggle.isOn = currentUserRating == ModRating.Positive;
		_negativeVoteToggle.isOn = currentUserRating == ModRating.Negative;
		_positiveVoteToggle.onValueChanged.AddListener(PositiveToggleValueChanged);
		_negativeVoteToggle.onValueChanged.AddListener(NegativeToggleValueChanged);
	}

	private void PositiveToggleValueChanged(bool arg0)
	{
		Task<Error> task = _mod.RateMod(_positiveVoteToggle.isOn ? ModRating.Positive : ModRating.None);
		ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.MonitorTaskThenOpenPanelIfError(task);
	}

	private void NegativeToggleValueChanged(bool toggleValue)
	{
		Task<Error> task = _mod.RateMod(_negativeVoteToggle.isOn ? ModRating.Negative : ModRating.None);
		ModioPanelManager.GetPanelOfType<ModioErrorPanelGeneric>()?.MonitorTaskThenOpenPanelIfError(task);
	}
}
