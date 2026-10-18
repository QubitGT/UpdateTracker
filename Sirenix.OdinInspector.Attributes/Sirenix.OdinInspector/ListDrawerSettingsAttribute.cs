using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
[Conditional("UNITY_EDITOR")]
[DontApplyToListElements]
public sealed class ListDrawerSettingsAttribute : Attribute
{
	public bool HideAddButton;

	public bool HideRemoveButton;

	public string ListElementLabelName;

	public string CustomAddFunction;

	[LabelWidth(200f)]
	public string CustomRemoveIndexFunction;

	[LabelWidth(200f)]
	public string CustomRemoveElementFunction;

	public string OnBeginListElementGUI;

	public string OnEndListElementGUI;

	public bool AlwaysAddDefaultValue;

	public bool AddCopiesLastElement;

	[ColorResolver]
	public string ElementColor;

	private string onTitleBarGUI;

	private int numberOfItemsPerPage;

	private bool paging;

	private bool draggable;

	private bool isReadOnly;

	private bool showItemCount;

	private bool pagingHasValue;

	private bool draggableHasValue;

	private bool isReadOnlyHasValue;

	private bool showItemCountHasValue;

	private bool numberOfItemsPerPageHasValue;

	private bool showIndexLabels;

	private bool showIndexLabelsHasValue;

	private bool defaultExpandedStateHasValue;

	private bool defaultExpandedState;

	public bool ShowFoldout = true;

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "paging", "pagingHasValue" })]
	public bool ShowPaging
	{
		get
		{
			return paging;
		}
		set
		{
			paging = value;
			pagingHasValue = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "draggable", "draggableHasValue" })]
	public bool DraggableItems
	{
		get
		{
			return draggable;
		}
		set
		{
			draggable = value;
			draggableHasValue = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "numberOfItemsPerPage", "numberOfItemsPerPageHasValue" })]
	public int NumberOfItemsPerPage
	{
		get
		{
			return numberOfItemsPerPage;
		}
		set
		{
			numberOfItemsPerPage = value;
			numberOfItemsPerPageHasValue = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "isReadOnly", "isReadOnlyHasValue" })]
	public bool IsReadOnly
	{
		get
		{
			return isReadOnly;
		}
		set
		{
			isReadOnly = value;
			isReadOnlyHasValue = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "showItemCount", "showItemCountHasValue" })]
	public bool ShowItemCount
	{
		get
		{
			return showItemCount;
		}
		set
		{
			showItemCount = value;
			showItemCountHasValue = true;
		}
	}

	[Obsolete("Use ShowFoldout instead, which is what Expanded has always done. If you want to control the default expanded state, use DefaultExpandedState. Expanded has been implemented wrong for a long time.", false)]
	public bool Expanded
	{
		get
		{
			return !ShowFoldout;
		}
		set
		{
			ShowFoldout = !value;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "defaultExpandedState", "defaultExpandedStateHasValue" })]
	public bool DefaultExpandedState
	{
		get
		{
			return defaultExpandedState;
		}
		set
		{
			defaultExpandedStateHasValue = true;
			defaultExpandedState = value;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "showIndexLabels", "showIndexLabelsHasValue" })]
	public bool ShowIndexLabels
	{
		get
		{
			return showIndexLabels;
		}
		set
		{
			showIndexLabels = value;
			showIndexLabelsHasValue = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "onTitleBarGUI" })]
	public string OnTitleBarGUI
	{
		get
		{
			return onTitleBarGUI;
		}
		set
		{
			onTitleBarGUI = value;
		}
	}

	public bool PagingHasValue => pagingHasValue;

	public bool ShowItemCountHasValue => showItemCountHasValue;

	public bool NumberOfItemsPerPageHasValue => numberOfItemsPerPageHasValue;

	public bool DraggableHasValue => draggableHasValue;

	public bool IsReadOnlyHasValue => isReadOnlyHasValue;

	public bool ShowIndexLabelsHasValue => showIndexLabelsHasValue;

	public bool DefaultExpandedStateHasValue => defaultExpandedStateHasValue;
}
