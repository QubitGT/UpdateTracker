using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables;

public class ModioUIButton : Button, IModioUISelectable
{
	public IModioUISelectable.SelectionState State { get; private set; }

	public event IModioUISelectable.SelectableStateChangeDelegate StateChanged;

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		State = (IModioUISelectable.SelectionState)state;
		this.StateChanged?.Invoke(State, instant);
	}

	public void DoVisualOnlyStateTransition(IModioUISelectable.SelectionState state, bool instant)
	{
		DoStateTransition((SelectionState)state, instant);
	}
}
