using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

[DefaultExecutionOrder(1)]
public class ActiveStateTracker : MonoBehaviour
{
	[Tooltip("The IActiveState to be tracked.")]
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	[Header("Active state dependents")]
	[SerializeField]
	[Tooltip("If true, all children of this object will be included as dependents.")]
	private bool _includeChildrenAsDependents;

	[SerializeField]
	[Optional]
	[Tooltip("Sets the `active` field on whole GameObjects.")]
	private List<GameObject> _gameObjects;

	[SerializeField]
	[Optional]
	[Tooltip("Sets the `enabled` field on individual components.")]
	private List<MonoBehaviour> _monoBehaviours;

	private bool _active;

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
	}

	protected virtual void Start()
	{
		if (_includeChildrenAsDependents)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				_gameObjects.Add(base.transform.GetChild(i).gameObject);
			}
		}
		SetDependentsActive(active: false);
	}

	protected virtual void Update()
	{
		bool active = ActiveState.Active;
		if (_active != active)
		{
			_active = active;
			SetDependentsActive(active);
		}
	}

	private void SetDependentsActive(bool active)
	{
		for (int i = 0; i < _gameObjects.Count; i++)
		{
			_gameObjects[i].SetActive(active);
		}
		for (int j = 0; j < _monoBehaviours.Count; j++)
		{
			_monoBehaviours[j].enabled = active;
		}
	}

	public void InjectAllActiveStateTracker(IActiveState activeState)
	{
		InjectActiveState(activeState);
	}

	public void InjectActiveState(IActiveState activeState)
	{
		_activeState = activeState as UnityEngine.Object;
		ActiveState = activeState;
	}

	public void InjectOptionalIncludeChildrenAsDependents(bool includeChildrenAsDependents)
	{
		_includeChildrenAsDependents = includeChildrenAsDependents;
	}

	public void InjectOptionalGameObjects(List<GameObject> gameObjects)
	{
		_gameObjects = gameObjects;
	}

	public void InjectOptionalMonoBehaviours(List<MonoBehaviour> monoBehaviours)
	{
		_monoBehaviours = monoBehaviours;
	}
}
