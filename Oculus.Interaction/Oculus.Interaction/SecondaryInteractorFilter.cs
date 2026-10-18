using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Oculus.Interaction;

public class SecondaryInteractorFilter : MonoBehaviour, IGameObjectFilter
{
	[SerializeField]
	[Interface(typeof(IInteractable), new Type[] { })]
	private UnityEngine.Object _primaryInteractable;

	[SerializeField]
	[Interface(typeof(IInteractable), new Type[] { })]
	private UnityEngine.Object _secondaryInteractable;

	[SerializeField]
	private bool _selectRequired;

	private Dictionary<int, List<int>> _primaryToSecondaryMap;

	protected bool _started;

	public IInteractable PrimaryInteractable { get; private set; }

	public IInteractable SecondaryInteractable { get; private set; }

	protected virtual void Awake()
	{
		PrimaryInteractable = _primaryInteractable as IInteractable;
		SecondaryInteractable = _secondaryInteractable as IInteractable;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			if (_selectRequired)
			{
				PrimaryInteractable.WhenSelectingInteractorViewAdded += HandleInteractorAdded;
				PrimaryInteractable.WhenSelectingInteractorViewRemoved += HandleInteractorRemoved;
			}
			else
			{
				PrimaryInteractable.WhenInteractorViewAdded += HandleInteractorAdded;
				PrimaryInteractable.WhenInteractorViewRemoved += HandleInteractorRemoved;
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			if (_selectRequired)
			{
				PrimaryInteractable.WhenSelectingInteractorViewAdded -= HandleInteractorAdded;
				PrimaryInteractable.WhenSelectingInteractorViewRemoved -= HandleInteractorRemoved;
			}
			else
			{
				PrimaryInteractable.WhenInteractorViewAdded -= HandleInteractorAdded;
				PrimaryInteractable.WhenInteractorViewRemoved -= HandleInteractorRemoved;
			}
		}
	}

	public bool Filter(GameObject gameObject)
	{
		if (_primaryToSecondaryMap == null)
		{
			return false;
		}
		if (!gameObject.TryGetComponent<SecondaryInteractorConnection>(out var component))
		{
			return false;
		}
		int identifier = component.PrimaryInteractor.Identifier;
		if (!_primaryToSecondaryMap.ContainsKey(identifier))
		{
			return false;
		}
		List<int> list = _primaryToSecondaryMap[identifier];
		if (!list.Contains(component.SecondaryInteractor.Identifier))
		{
			list.Add(component.SecondaryInteractor.Identifier);
		}
		return true;
	}

	private void HandleInteractorAdded(IInteractorView interactor)
	{
		if (_primaryToSecondaryMap == null)
		{
			_primaryToSecondaryMap = CollectionPool<Dictionary<int, List<int>>, KeyValuePair<int, List<int>>>.Get();
		}
		_primaryToSecondaryMap.Add(interactor.Identifier, CollectionPool<List<int>, int>.Get());
	}

	private void HandleInteractorRemoved(IInteractorView primaryInteractor)
	{
		foreach (int item in _primaryToSecondaryMap[primaryInteractor.Identifier])
		{
			SecondaryInteractable.RemoveInteractorByIdentifier(item);
		}
		CollectionPool<List<int>, int>.Release(_primaryToSecondaryMap[primaryInteractor.Identifier]);
		_primaryToSecondaryMap.Remove(primaryInteractor.Identifier);
		if (_primaryToSecondaryMap.Count == 0)
		{
			CollectionPool<Dictionary<int, List<int>>, KeyValuePair<int, List<int>>>.Release(_primaryToSecondaryMap);
			_primaryToSecondaryMap = null;
		}
	}

	public void InjectAllSecondaryInteractorFilter(IInteractable primaryInteractable, IInteractable secondaryInteractable, bool selectRequired = false)
	{
		InjectPrimaryInteractable(primaryInteractable);
		InjectSecondaryInteractable(secondaryInteractable);
		InjectSelectRequired(selectRequired);
	}

	public void InjectPrimaryInteractable(IInteractable interactableView)
	{
		PrimaryInteractable = interactableView;
		_primaryInteractable = interactableView as UnityEngine.Object;
	}

	public void InjectSecondaryInteractable(IInteractable interactable)
	{
		SecondaryInteractable = interactable;
		_secondaryInteractable = interactable as UnityEngine.Object;
	}

	public void InjectSelectRequired(bool selectRequired)
	{
		_selectRequired = selectRequired;
	}
}
