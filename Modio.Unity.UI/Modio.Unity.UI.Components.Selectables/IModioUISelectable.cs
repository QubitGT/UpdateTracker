namespace Modio.Unity.UI.Components.Selectables;

public interface IModioUISelectable
{
	public enum SelectionState
	{
		Normal,
		Highlighted,
		Pressed,
		Selected,
		Disabled
	}

	public delegate void SelectableStateChangeDelegate(SelectionState state, bool instant);

	SelectionState State { get; }

	event SelectableStateChangeDelegate StateChanged;
}
