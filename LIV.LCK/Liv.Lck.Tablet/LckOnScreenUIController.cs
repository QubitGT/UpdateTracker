using System.Collections.Generic;
using Liv.Lck.DependencyInjection;
using Liv.Lck.UI;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckOnScreenUIController : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private List<GameObject> _allOnscreenUI = new List<GameObject>();

	private void OnEnable()
	{
		_lckService.OnRecordingStarted += OnRecordingStarted;
	}

	private void OnDisable()
	{
		_lckService.OnRecordingStarted -= OnRecordingStarted;
		SetAllOnscreenButtonsState(state: true);
	}

	private void OnRecordingStarted(LckResult result)
	{
		if (result.Success)
		{
			SetAllOnscreenButtonsState(state: true);
		}
	}

	public void OnNotificationStarted()
	{
		SetAllOnscreenButtonsState(state: false);
	}

	public void OnNotificationEnded()
	{
		SetAllOnscreenButtonsState(state: true);
		SetAllOnscreenButtonsToDefaultVisual(_allOnscreenUI);
	}

	private void SetAllOnscreenButtonsState(bool state)
	{
		SetObjectsState(_allOnscreenUI, state);
	}

	private void SetObjectsState(List<GameObject> objectList, bool state)
	{
		foreach (GameObject @object in objectList)
		{
			@object.SetActive(state);
		}
	}

	private void SetAllOnscreenButtonsToDefaultVisual(List<GameObject> objectList)
	{
		foreach (GameObject @object in objectList)
		{
			if (@object.TryGetComponent<LckScreenButton>(out var component))
			{
				component.SetDefaultButtonColors();
			}
		}
	}
}
