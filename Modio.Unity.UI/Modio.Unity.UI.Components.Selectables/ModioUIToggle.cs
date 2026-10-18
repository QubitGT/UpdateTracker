using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables;

public class ModioUIToggle : Toggle, IModioUISelectable
{
	public IModioUISelectable.SelectionState State { get; private set; }

	public event IModioUISelectable.SelectableStateChangeDelegate StateChanged;

	protected override void Awake()
	{
		base.Awake();
		onValueChanged.AddListener(delegate
		{
			DoStateTransition((SelectionState)State, instant: false);
		});
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		State = (IModioUISelectable.SelectionState)state;
		this.StateChanged?.Invoke(State, instant);
	}

	public void FakeClicked()
	{
		if (IsActive() && IsInteractable())
		{
			base.isOn = !base.isOn;
		}
	}
}
