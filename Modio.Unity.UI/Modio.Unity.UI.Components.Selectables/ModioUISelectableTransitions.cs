using System;
using System.Linq;
using Modio.Unity.UI.Components.Selectables.Transitions;
using Modio.Unity.UI.Input;
using UnityEngine;

namespace Modio.Unity.UI.Components.Selectables;

public class ModioUISelectableTransitions : MonoBehaviour
{
	public enum ToggleFilter
	{
		Any = 3,
		OnlyOn = 1,
		OnlyOff = 2
	}

	[SerializeField]
	[Tooltip("Use to limit transitions to a toggle value.\ne.g. \"Only On\" will only trigger if the toggle is on. ")]
	private ToggleFilter _toggleFilter = ToggleFilter.Any;

	[SerializeReference]
	private ISelectableTransition[] _transitions;

	private IPropertyMonoBehaviourEvents[] _monoBehaviourEvents;

	private IModioUISelectable _owner;

	private ModioUIToggle _toggle;

	private void Awake()
	{
		_owner = GetComponentInParent<IModioUISelectable>();
		_toggle = _owner as ModioUIToggle;
		_monoBehaviourEvents = (_transitions.Any((ISelectableTransition property) => property is IPropertyMonoBehaviourEvents) ? _transitions.OfType<IPropertyMonoBehaviourEvents>().ToArray() : Array.Empty<IPropertyMonoBehaviourEvents>());
		if (_owner == null && base.enabled)
		{
			Debug.Log(GetType().Name + " " + base.gameObject.name + " could not find an IModioUISelectable, disabling.", this);
			base.enabled = false;
		}
	}

	private void Start()
	{
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].Start();
		}
	}

	private void OnEnable()
	{
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].OnEnable();
		}
		if (_owner != null)
		{
			_owner.StateChanged += OnSelectionStateChanged;
			OnSelectionStateChanged(_owner.State, instant: true);
		}
	}

	private void OnDisable()
	{
		if (_owner != null)
		{
			_owner.StateChanged -= OnSelectionStateChanged;
		}
		ModioUIInput.SwappedControlScheme -= OnSwappedToController;
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].OnDisable();
		}
	}

	private void OnDestroy()
	{
		if (_owner != null)
		{
			_owner.StateChanged -= OnSelectionStateChanged;
		}
		ModioUIInput.SwappedControlScheme -= OnSwappedToController;
		IPropertyMonoBehaviourEvents[] monoBehaviourEvents = _monoBehaviourEvents;
		for (int i = 0; i < monoBehaviourEvents.Length; i++)
		{
			monoBehaviourEvents[i].OnDestroy();
		}
	}

	private void OnSelectionStateChanged(IModioUISelectable.SelectionState state, bool instant)
	{
		if (_toggle != null)
		{
			ToggleFilter toggleFilter = (_toggle.isOn ? ToggleFilter.OnlyOn : ToggleFilter.OnlyOff);
			if (!_toggleFilter.HasFlag(toggleFilter))
			{
				return;
			}
		}
		else if (_toggleFilter == ToggleFilter.OnlyOn)
		{
			return;
		}
		ModioUIInput.SwappedControlScheme -= OnSwappedToController;
		if (state == IModioUISelectable.SelectionState.Highlighted)
		{
			if (ModioUIInput.IsUsingGamepad)
			{
				state = IModioUISelectable.SelectionState.Normal;
			}
			ModioUIInput.SwappedControlScheme += OnSwappedToController;
		}
		ISelectableTransition[] transitions = _transitions;
		for (int i = 0; i < transitions.Length; i++)
		{
			transitions[i].OnSelectionStateChanged(state, instant);
		}
	}

	private void OnSwappedToController(bool isController)
	{
		if (_owner != null && _owner.State == IModioUISelectable.SelectionState.Highlighted)
		{
			OnSelectionStateChanged(IModioUISelectable.SelectionState.Highlighted, instant: false);
		}
	}
}
