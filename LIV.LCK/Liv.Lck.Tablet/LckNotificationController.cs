using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Recorder;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckNotificationController : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[Tooltip("Configure the list of all possible notifications. Drag your notification prefabs here and assign them a type.")]
	[SerializeField]
	private List<InitializerNotification> _notificationsInitializer = new List<InitializerNotification>();

	private readonly Dictionary<NotificationType, LckBaseNotification> _notifications = new Dictionary<NotificationType, LckBaseNotification>();

	private LckBaseNotification _currentNotification;

	[Tooltip("The default duration in seconds that a notification will remain on screen before automatically hiding. This can be overridden by the notification itself.")]
	[SerializeField]
	private float _notificationShowDuration = 3f;

	[Tooltip("The parent Transform under which all notification prefabs will be instantiated.")]
	[SerializeField]
	private Transform _notificationsTransform;

	[Tooltip("A reference to a higher-level UI controller that may need to react when notifications appear or disappear (e.g., to adjust layout).")]
	[SerializeField]
	private LckOnScreenUIController _onScreenUIController;

	private void Awake()
	{
		InitializeNotifications();
	}

	private void Start()
	{
		CheckInitializationAfterDelay();
	}

	private async void CheckInitializationAfterDelay()
	{
		await Task.Delay(1000);
		if (this == null)
		{
			return;
		}
		Result<bool> lckCoreInitializationResult = LckCoreHandler.LckCoreInitializationResult;
		if (lckCoreInitializationResult != null && !lckCoreInitializationResult.IsOk)
		{
			if (lckCoreInitializationResult.Err == CoreError.MissingTrackingId)
			{
				StartCoroutine(CreateNotification(NotificationType.MissingTrackingId));
			}
			else if (lckCoreInitializationResult.Err == CoreError.InvalidTrackingId)
			{
				StartCoroutine(CreateNotification(NotificationType.InvalidTrackingId));
			}
		}
	}

	private void OnValidate()
	{
		foreach (InitializerNotification item in _notificationsInitializer)
		{
			item.Name = ((item.prefab != null) ? item.Type.ToString() : null);
		}
	}

	private void OnEnable()
	{
		_lckService.OnRecordingStarted += OnCaptureStarted;
		_lckService.OnStreamingStarted += OnCaptureStarted;
		_lckService.OnRecordingSaved += OnRecordingSaved;
		_lckService.OnEchoSaved += OnEchoSaved;
		_lckService.OnEchoDisabled += OnEchoDisabled;
		if (_currentNotification != null && _currentNotification.RemainOnScreen)
		{
			_onScreenUIController.OnNotificationStarted();
		}
	}

	private void OnDisable()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnCaptureStarted;
			_lckService.OnStreamingStarted -= OnCaptureStarted;
			_lckService.OnRecordingSaved -= OnRecordingSaved;
			_lckService.OnEchoSaved -= OnEchoSaved;
			_lckService.OnEchoDisabled -= OnEchoDisabled;
			if (_currentNotification != null && !_currentNotification.RemainOnScreen)
			{
				HideNotifications();
			}
		}
	}

	public void SetNotificationStreamCode(string code)
	{
		if (_notifications.TryGetValue(NotificationType.EnterStreamCode, out var value))
		{
			if (value is LckNormalNotification lckNormalNotification)
			{
				lckNormalNotification.Text.text = code;
			}
		}
		else
		{
			Debug.LogError("No 'EnterStreamCode' notification prefab is configured in the LckNotificationController.");
		}
	}

	private void OnCaptureStarted(LckResult result)
	{
		if (result.Success)
		{
			HideNotifications();
		}
	}

	private void OnRecordingSaved(LckResult<RecordingData> result)
	{
		if (result.Success)
		{
			ShowNotification(NotificationType.VideoSaved);
		}
		else
		{
			Debug.LogWarning($"Failed to show 'VideoSaved' notification. Error: {result.Error}, Message: {result.Message}");
		}
	}

	private void OnEchoDisabled(LckResult result, EchoDisableReason reason)
	{
		switch (reason)
		{
		case EchoDisableReason.LowStorage:
			ShowNotification(NotificationType.EchoLowStorage);
			break;
		case EchoDisableReason.Error:
			ShowNotification(NotificationType.EchoError);
			break;
		}
	}

	private void OnEchoSaved(LckResult<RecordingData> result)
	{
		if (result.Success)
		{
			ShowNotification(NotificationType.VideoSaved);
		}
		else
		{
			Debug.LogWarning($"Failed to show 'VideoSaved' notification for echo save. Error: {result.Error}, Message: {result.Message}");
		}
	}

	public void HideNotifications()
	{
		StopAllCoroutines();
		if (_currentNotification != null)
		{
			_onScreenUIController.OnNotificationEnded();
		}
		_currentNotification = null;
		foreach (KeyValuePair<NotificationType, LckBaseNotification> notification in _notifications)
		{
			notification.Value.HideNotification();
		}
	}

	public void InitializeNotifications()
	{
		DestroyNotifications();
		foreach (InitializerNotification item in _notificationsInitializer)
		{
			GameObject gameObject = Object.Instantiate(item.prefab);
			gameObject.SetActive(value: false);
			gameObject.transform.SetParent(_notificationsTransform, worldPositionStays: false);
			LckBaseNotification component = gameObject.GetComponent<LckBaseNotification>();
			if (component != null)
			{
				_notifications.Add(item.Type, component);
				component.SetSpawnedGameObject(gameObject);
			}
			else
			{
				Debug.LogError($"Prefab for notification type '{item.Type}' is missing a component that inherits from LckBaseNotification.", gameObject);
			}
		}
	}

	public void DestroyNotifications()
	{
		_notifications.Clear();
		if (_notificationsTransform.childCount <= 0)
		{
			return;
		}
		for (int num = _notificationsTransform.childCount - 1; num >= 0; num--)
		{
			GameObject obj = _notificationsTransform.GetChild(num).gameObject;
			if (Application.isPlaying)
			{
				Object.Destroy(obj);
			}
			else
			{
				Object.DestroyImmediate(obj);
			}
		}
	}

	public void ShowNotification(NotificationType type)
	{
		Result<bool> lckCoreInitializationResult = LckCoreHandler.LckCoreInitializationResult;
		if (lckCoreInitializationResult != null && !lckCoreInitializationResult.IsOk)
		{
			Debug.LogError("Failed to show notification: " + type.ToString() + " LckCore failed initialization");
			return;
		}
		HideNotifications();
		StartCoroutine(CreateNotification(type));
	}

	private IEnumerator CreateNotification(NotificationType type)
	{
		_onScreenUIController.OnNotificationStarted();
		if (_notifications.TryGetValue(type, out _currentNotification))
		{
			_currentNotification.ShowNotification();
			if (!_currentNotification.RemainOnScreen)
			{
				if (_currentNotification.ShowDuration != _notificationShowDuration)
				{
					yield return new WaitForSeconds(_currentNotification.ShowDuration);
				}
				else
				{
					yield return new WaitForSeconds(_notificationShowDuration);
				}
				_currentNotification.HideNotification();
				_onScreenUIController.OnNotificationEnded();
				_currentNotification = null;
			}
		}
		else
		{
			Debug.LogError("No notification found with type: " + type);
			_onScreenUIController.OnNotificationEnded();
		}
	}
}
