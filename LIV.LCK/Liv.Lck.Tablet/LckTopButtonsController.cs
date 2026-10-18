using System.Collections;
using System.Collections.Generic;
using Liv.Lck.DependencyInjection;
using Liv.Lck.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.Tablet;

public class LckTopButtonsController : MonoBehaviour
{
	internal enum TopButtonPage
	{
		Null,
		Camera,
		Stream,
		Echo
	}

	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private GameObject _topButtonsControllerGameObject;

	[SerializeField]
	private LckNotificationController _notificationController;

	[SerializeField]
	private LckPhotoModeController _photoModeController;

	[SerializeField]
	private List<GameObject> _cameraPageButtons = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _streamPageButtons = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _echoPageButtons = new List<GameObject>();

	[Header("Top Button Events")]
	[SerializeField]
	private UnityEvent _onCameraPageOpened = new UnityEvent();

	[SerializeField]
	private UnityEvent _onStreamPageOpened = new UnityEvent();

	[SerializeField]
	private UnityEvent _onEchoPageOpened = new UnityEvent();

	private ILckTopButtons _topButtonsHelper;

	private TopButtonPage _currentPage;

	private bool _buttonsDisabled;

	internal TopButtonPage CurrentPage => _currentPage;

	private void Start()
	{
		if (Application.platform != RuntimePlatform.Android && !Application.isEditor && (bool)_topButtonsControllerGameObject)
		{
			_topButtonsControllerGameObject.SetActive(value: false);
		}
		_topButtonsHelper = GetComponent<ILckTopButtons>();
		ToggleCameraPage(state: true);
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			StartCoroutine(ResetAfterApplicationFocus());
		}
	}

	private IEnumerator ResetAfterApplicationFocus()
	{
		yield return 0;
		if (_buttonsDisabled)
		{
			SetTopButtonsIsDisabledState(isDisabled: true);
		}
	}

	public void SetTopButtonsIsDisabledState(bool isDisabled)
	{
		_buttonsDisabled = isDisabled;
		if (_topButtonsHelper == null)
		{
			GetComponent<ILckTopButtons>();
		}
		if (isDisabled)
		{
			_topButtonsHelper?.HideButtons();
		}
		else
		{
			_topButtonsHelper?.ShowButtons();
		}
	}

	public void ToggleCameraPage(bool state)
	{
		if (_currentPage == TopButtonPage.Camera || !state || _buttonsDisabled)
		{
			return;
		}
		DisableEchoIfActive();
		_currentPage = TopButtonPage.Camera;
		_notificationController.HideNotifications();
		_photoModeController.StopAndResetSequence();
		foreach (GameObject cameraPageButton in _cameraPageButtons)
		{
			cameraPageButton.SetActive(value: true);
		}
		foreach (GameObject streamPageButton in _streamPageButtons)
		{
			streamPageButton.SetActive(value: false);
		}
		foreach (GameObject echoPageButton in _echoPageButtons)
		{
			echoPageButton.SetActive(value: false);
		}
		_lckService.SetActiveCaptureType(LckCaptureType.Recording);
		_onCameraPageOpened.Invoke();
	}

	public void ToggleStreamPage(bool state)
	{
		if (_currentPage == TopButtonPage.Stream || !state || _buttonsDisabled)
		{
			return;
		}
		DisableEchoIfActive();
		_currentPage = TopButtonPage.Stream;
		_photoModeController.StopAndResetSequence();
		foreach (GameObject streamPageButton in _streamPageButtons)
		{
			streamPageButton.SetActive(value: true);
		}
		foreach (GameObject cameraPageButton in _cameraPageButtons)
		{
			cameraPageButton.SetActive(value: false);
		}
		foreach (GameObject echoPageButton in _echoPageButtons)
		{
			echoPageButton.SetActive(value: false);
		}
		_lckService.SetActiveCaptureType(LckCaptureType.Streaming);
		_onStreamPageOpened.Invoke();
	}

	public void ToggleEchoPage(bool state)
	{
		if (_currentPage == TopButtonPage.Echo || !state || _buttonsDisabled)
		{
			return;
		}
		_currentPage = TopButtonPage.Echo;
		_notificationController.HideNotifications();
		_photoModeController.StopAndResetSequence();
		foreach (GameObject echoPageButton in _echoPageButtons)
		{
			echoPageButton.SetActive(value: true);
		}
		foreach (GameObject cameraPageButton in _cameraPageButtons)
		{
			cameraPageButton.SetActive(value: false);
		}
		foreach (GameObject streamPageButton in _streamPageButtons)
		{
			streamPageButton.SetActive(value: false);
		}
		_lckService.SetActiveCaptureType(LckCaptureType.Recording);
		LckResult lckResult = _lckService.SetEchoEnabled(enabled: true);
		if (!lckResult.Success)
		{
			LckLog.LogError("Failed to enable echo: " + lckResult.Message, "ToggleEchoPage", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckTopButtonsController.cs", 196);
		}
		_notificationController.ShowNotification(NotificationType.EchoInfo);
		_onEchoPageOpened.Invoke();
	}

	private void DisableEchoIfActive()
	{
		LckResult lckResult = _lckService.SetEchoEnabled(enabled: false);
		if (!lckResult.Success)
		{
			LckLog.LogError("Failed to disable echo: " + lckResult.Message, "DisableEchoIfActive", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckTopButtonsController.cs", 208);
		}
	}

	public void SetCameraPageVisualsManually()
	{
		_topButtonsHelper.SetCameraPageVisualsManually();
	}
}
