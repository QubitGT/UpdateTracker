using Modio.Platforms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables;

public class ModioUIInputField : TMP_InputField, IModioUISelectable
{
	[SerializeField]
	private int _layoutPriority = 1;

	public override int layoutPriority => _layoutPriority;

	public IModioUISelectable.SelectionState State { get; private set; }

	public event IModioUISelectable.SelectableStateChangeDelegate StateChanged;

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (ModioServices.TryResolve<IVirtualKeyboardHandler>(out var result))
		{
			ModioVirtualKeyboardType virtualKeyboardType = ModioVirtualKeyboardType.Default;
			if (base.contentType == ContentType.EmailAddress)
			{
				virtualKeyboardType = ModioVirtualKeyboardType.EmailAddress;
			}
			result.OpenVirtualKeyboard(null, null, base.text, virtualKeyboardType, base.characterLimit, base.multiLine, delegate(string s)
			{
				base.text = s;
				OnSubmit(null);
				OnDeselect(null);
			});
		}
	}

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
