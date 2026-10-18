using System;
using UnityEngine;

namespace Oculus.Interaction;

public class VirtualSelector : MonoBehaviour, ISelector
{
	[Tooltip("Toggles the selector from within the Unity inspector.")]
	[SerializeField]
	private bool _selectFlag;

	private bool _currentlySelected;

	public event Action WhenSelected = delegate
	{
	};

	public event Action WhenUnselected = delegate
	{
	};

	public void Select()
	{
		_selectFlag = true;
		UpdateSelection();
	}

	public void Unselect()
	{
		_selectFlag = false;
		UpdateSelection();
	}

	protected virtual void OnValidate()
	{
		UpdateSelection();
	}

	protected void UpdateSelection()
	{
		if (_currentlySelected != _selectFlag)
		{
			_currentlySelected = _selectFlag;
			if (_currentlySelected)
			{
				this.WhenSelected();
			}
			else
			{
				this.WhenUnselected();
			}
		}
	}
}
