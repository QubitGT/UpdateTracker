using System;
using System.Linq;
using System.Threading.Tasks;
using Modio.Mods;
using Modio.Unity.UI.Components;
using Modio.Unity.UI.Search;
using UnityEngine.UI;

namespace Modio.Unity.UI.Panels;

public class ModSortPanel : ModioPanelBase
{
	private Toggle[] _toggles;

	protected override void Awake()
	{
		base.Awake();
		_toggles = GetComponentsInChildren<Toggle>(includeInactive: true);
	}

	public override void DoDefaultSelection()
	{
		SetSelectedGameObject(_toggles.FirstOrDefault((Toggle t) => t.isOn)?.gameObject ?? _toggles.First().gameObject);
	}

	public override void OnGainedFocus(GainedFocusCause selectionBehaviour)
	{
		SortModsBy sortBy = ModioUISearch.Default.LastSearchFilter.SortBy;
		Toggle[] toggles = _toggles;
		foreach (Toggle obj in toggles)
		{
			bool isOn = obj.GetComponent<ModioUISortModsToggle>().SortModsBy == sortBy;
			obj.isOn = isOn;
		}
		base.OnGainedFocus(selectionBehaviour);
	}

	public async void ApplySort()
	{
		await Task.Yield();
		ModioUISortModsToggle modioUISortModsToggle = _toggles.FirstOrDefault((Toggle toggle) => toggle.isOn)?.GetComponent<ModioUISortModsToggle>();
		if (!(modioUISortModsToggle == null) && modioUISortModsToggle.SortModsBy != ModioUISearch.Default.LastSearchFilter.SortBy)
		{
			ClosePanel();
			bool ascending = modioUISortModsToggle.SortModsBy switch
			{
				SortModsBy.Name => true, 
				SortModsBy.Price => false, 
				SortModsBy.Rating => true, 
				SortModsBy.Popular => false, 
				SortModsBy.Downloads => true, 
				SortModsBy.Subscribers => true, 
				SortModsBy.DateSubmitted => false, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			ModioUISearch.Default.ApplySortBy(modioUISortModsToggle.SortModsBy, ascending);
		}
	}
}
