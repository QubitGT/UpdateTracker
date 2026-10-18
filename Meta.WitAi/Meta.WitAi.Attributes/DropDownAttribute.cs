using System;
using UnityEngine;

namespace Meta.WitAi.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class DropDownAttribute : PropertyAttribute
{
	public string OptionListGetterName { get; }

	public bool RefreshOnRepaint { get; }

	public bool AllowInvalid { get; }

	public bool ShowPropertyIfListIsEmpty { get; }

	public bool ShowRefreshButton { get; }

	public string RefreshMethodName { get; }

	public bool ShowSearch { get; }

	public DropDownAttribute(string optionListGetterName, bool refreshOnRepaint = false, bool allowInvalid = false, bool showPropertyIfListIsEmpty = true, bool showRefreshButton = true, string refreshMethodName = null, bool showSearch = false)
	{
		OptionListGetterName = optionListGetterName;
		RefreshOnRepaint = refreshOnRepaint;
		AllowInvalid = allowInvalid;
		ShowPropertyIfListIsEmpty = showPropertyIfListIsEmpty;
		ShowRefreshButton = showRefreshButton;
		RefreshMethodName = refreshMethodName;
		ShowSearch = showSearch;
	}
}
