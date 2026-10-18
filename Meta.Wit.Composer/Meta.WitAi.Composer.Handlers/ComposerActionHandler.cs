using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Composer.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Composer.Handlers;

public class ComposerActionHandler : MonoBehaviour, IComposerActionHandler
{
	[SerializeField]
	private ComposerActionEventData[] _actionEvents;

	private int _highestIndex;

	public Func<ComposerSessionData, IEnumerator> HandleActionAsync;

	private Dictionary<ComposerSessionData, bool> _actionCoroutines = new Dictionary<ComposerSessionData, bool>();

	public ComposerActionEventData[] ActionEvents => _actionEvents;

	protected virtual void Start()
	{
		_highestIndex = Math.Max(0, _actionEvents.Length - 1);
	}

	public void AddEvent(ComposerActionEventData actionEvent)
	{
		if (_highestIndex >= _actionEvents.Length - 1)
		{
			Array.Resize(ref _actionEvents, 1 + _actionEvents.Length * 2);
		}
		_actionEvents[_highestIndex++] = actionEvent;
	}

	public void PerformAction(ComposerSessionData sessionData)
	{
		string actionID = sessionData.responseData.actionID;
		int actionEventIndex = GetActionEventIndex(actionID);
		if (actionEventIndex != -1)
		{
			_actionEvents[actionEventIndex].actionEvent?.Invoke(sessionData);
		}
		if (HandleActionAsync != null)
		{
			StartCoroutine(PerformActionAsync(sessionData, HandleActionAsync));
		}
	}

	private IEnumerator PerformActionAsync(ComposerSessionData sessionData, Func<ComposerSessionData, IEnumerator> actionAsync)
	{
		_actionCoroutines[sessionData] = true;
		Delegate[] invocationList = actionAsync.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Func<ComposerSessionData, IEnumerator> func = (Func<ComposerSessionData, IEnumerator>)invocationList[i];
			yield return func(sessionData);
		}
		if (_actionCoroutines.ContainsKey(sessionData))
		{
			_actionCoroutines.Remove(sessionData);
		}
	}

	public bool IsPerformingAction(ComposerSessionData sessionData)
	{
		if (_actionCoroutines != null)
		{
			return _actionCoroutines.ContainsKey(sessionData);
		}
		return false;
	}

	public int GetActionEventIndex(string actionID)
	{
		if (_actionEvents != null)
		{
			return Array.FindIndex(_actionEvents, (ComposerActionEventData a) => string.Equals(a.actionID, actionID, StringComparison.CurrentCultureIgnoreCase));
		}
		return -1;
	}
}
