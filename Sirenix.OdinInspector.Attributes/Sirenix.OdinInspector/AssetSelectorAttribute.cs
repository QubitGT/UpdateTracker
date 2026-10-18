using System;
using System.Diagnostics;
using System.Linq;

namespace Sirenix.OdinInspector;

[Conditional("UNITY_EDITOR")]
public class AssetSelectorAttribute : Attribute
{
	[LabelWidth(200f)]
	public bool IsUniqueList = true;

	[LabelWidth(200f)]
	public bool DrawDropdownForListElements = true;

	[LabelWidth(200f)]
	public bool DisableListAddButtonBehaviour;

	[LabelWidth(200f)]
	public bool ExcludeExistingValuesInList;

	[LabelWidth(200f)]
	public bool ExpandAllMenuItems = true;

	[LabelWidth(200f)]
	public bool FlattenTreeView;

	public int DropdownWidth;

	public int DropdownHeight;

	public string DropdownTitle;

	public string[] SearchInFolders;

	public string Filter;

	[ShowInInspector]
	[DelayedProperty]
	[OdinDesignerBinding(new string[] { "SearchInFolders" })]
	public string Paths
	{
		get
		{
			if (SearchInFolders != null)
			{
				return string.Join(",", SearchInFolders);
			}
			return null;
		}
		set
		{
			SearchInFolders = (from x in value.Split(new char[1] { '|' })
				select x.Trim().Trim('/', '\\')).ToArray();
		}
	}
}
