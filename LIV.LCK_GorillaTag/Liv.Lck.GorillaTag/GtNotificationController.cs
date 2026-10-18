using System.Collections;
using System.Collections.Generic;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtNotificationController : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private GameObject _ui;

	[SerializeField]
	private GameObject _questMessage;

	[SerializeField]
	private GameObject _pcMessage;

	[SerializeField]
	private float _notificationShowDuration = 4f;

	[SerializeField]
	private List<GameObject> _hiddenDuringNotification = new List<GameObject>();

	private bool _hiddenObjectsState = true;

	private void Start()
	{
		_questMessage.SetActive(value: false);
		_pcMessage.SetActive(value: true);
	}

	private void OnEnable()
	{
		_ui.SetActive(value: false);
		_lckService.OnRecordingStarted += OnRecordingStarted;
		_lckService.OnRecordingSaved += OnRecordingSaved;
	}

	private void OnDisable()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnRecordingStarted;
			_lckService.OnRecordingSaved -= OnRecordingSaved;
		}
	}

	private void OnRecordingStarted(LckResult result)
	{
		_ui.SetActive(value: false);
		StopAllCoroutines();
		if (!_hiddenObjectsState)
		{
			SetHiddenObjectsState(state: true);
		}
	}

	private void OnRecordingSaved(LckResult<RecordingData> result)
	{
		if (!result.Success)
		{
			Debug.LogWarning("Failed to create notification. Error: " + result.Error.ToString() + " Message: " + result.Message);
			return;
		}
		_ui.SetActive(value: true);
		StartCoroutine(NotificationTimer());
	}

	private IEnumerator NotificationTimer()
	{
		SetHiddenObjectsState(state: false);
		yield return new WaitForSeconds(_notificationShowDuration);
		_ui.SetActive(value: false);
		SetHiddenObjectsState(state: true);
	}

	private void SetHiddenObjectsState(bool state)
	{
		_hiddenObjectsState = state;
		foreach (GameObject item in _hiddenDuringNotification)
		{
			item.SetActive(state);
		}
	}
}
