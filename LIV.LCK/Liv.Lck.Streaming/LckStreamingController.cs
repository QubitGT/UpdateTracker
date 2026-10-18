using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Core.Cosmetics;
using Liv.Lck.DependencyInjection;
using Liv.Lck.Tablet;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.Streaming;

public class LckStreamingController : MonoBehaviour
{
	[Tooltip("Enable this to see detailed logs from this controller in the Unity console. Recommended for development.")]
	[SerializeField]
	private bool _showDebugLogs;

	[InjectLck]
	private ILckService _lckService;

	[Tooltip("A reference to the controller responsible for displaying UI notifications (e.g., 'Enter this code:', 'Please subscribe'). Assign this in the Inspector.")]
	[SerializeField]
	private LckNotificationController _notificationController;

	[Tooltip("Reference to the Top Buttons Controller which handles switching to Camera or Stream modes on the tablet UI")]
	[SerializeField]
	private LckTopButtonsController _topButtonsController;

	[Tooltip("This event is invoked when the user presses the stream button but the setup is not yet complete. Use this to trigger visual feedback, like a button shake or an error icon.")]
	[SerializeField]
	private UnityEvent _onStreamButtonError;

	[Tooltip("This event is invoked when the user presses the stream button and the setup is complete, just before streaming starts. Use this to trigger positive feedback, like a button color change.")]
	[SerializeField]
	private UnityEvent _onStreamButtonPressWithCorrectConfig;

	[Header("Game Objects disabled when streaming package removed")]
	[SerializeField]
	private GameObject _topButtonsControllerGameObject;

	[SerializeField]
	private GameObject _livHubButton;

	[InjectLck]
	public ILckCore LckCore { get; private set; }

	[InjectLck]
	public ILckCosmeticsCoordinator LckCosmeticsCoordinator { get; private set; }

	public bool IsConfiguredCorrectly { get; private set; }

	public LckStreamingBaseState CurrentState { get; private set; }

	public LckStreamingGetCurrentState GetCurrentState { get; private set; } = new LckStreamingGetCurrentState();


	public LckStreamingShowCodeState ShowCodeState { get; private set; } = new LckStreamingShowCodeState();


	public LckStreamingWaitingForConfigureState WaitingForConfigureState { get; private set; } = new LckStreamingWaitingForConfigureState();


	public LckStreamingConfiguredCorrectlyState ConfiguredCorrectlyState { get; private set; } = new LckStreamingConfiguredCorrectlyState();


	public LckInternalErrorState InternalErrorState { get; private set; } = new LckInternalErrorState();


	public LckMissingTrackingIdState MissingTrackingIdState { get; private set; } = new LckMissingTrackingIdState();


	public LckInvalidArgumentState InvalidArgumentState { get; private set; } = new LckInvalidArgumentState();


	public LckRateLimiterBackoffState RateLimiterBackoffState { get; private set; } = new LckRateLimiterBackoffState();


	public LckServiceUnavailableState ServiceUnavailableState { get; private set; } = new LckServiceUnavailableState();


	public CancellationTokenSource CancellationTokenSource { get; private set; } = new CancellationTokenSource();


	private void Start()
	{
		if (_lckService != null)
		{
			_lckService.OnStreamingStarted += OnStreamingStarted;
		}
		LckMonoBehaviourMediator.OnApplicationLifecycleEvent += OnApplicationLifecycle;
	}

	private void OnApplicationLifecycle(LckMonoBehaviourMediator.ApplicationLifecycleEventType eventType)
	{
		switch (eventType)
		{
		case LckMonoBehaviourMediator.ApplicationLifecycleEventType.Pause:
			OnSystemPaused();
			break;
		case LckMonoBehaviourMediator.ApplicationLifecycleEventType.Resume:
			OnSystemResumed();
			break;
		case LckMonoBehaviourMediator.ApplicationLifecycleEventType.HMDIdle:
			OnHMDIdle();
			break;
		case LckMonoBehaviourMediator.ApplicationLifecycleEventType.HMDActive:
			OnHMDActive();
			break;
		}
	}

	private void OnSystemPaused()
	{
		Log("[LCK Streaming Controller] System paused - stopping any active polling");
		StopCheckingStates();
		if (_lckService != null && _lckService.IsStreaming().Result)
		{
			Log("[LCK Streaming Controller] Stopping streaming due to system pause");
			_lckService.StopStreaming();
			_notificationController.ShowNotification(NotificationType.InternalError);
		}
	}

	private void OnSystemResumed()
	{
		Log("[LCK Streaming Controller] System resumed");
		if (_lckService != null)
		{
			bool result = _lckService.IsStreaming().Result;
			Log($"[LCK Streaming Controller] Post-resume streaming state: {result}");
		}
		if (IsConfiguredCorrectly)
		{
			CheckCurrentState();
		}
	}

	private void OnHMDIdle()
	{
		Log("[LCK Streaming Controller] HMD idle detected - stopping polling but keeping stream active");
		StopCheckingStates();
	}

	private void OnHMDActive()
	{
		Log("[LCK Streaming Controller] HMD active again");
		if (IsConfiguredCorrectly)
		{
			CheckCurrentState();
		}
	}

	public void CheckCurrentState()
	{
		SwitchState(GetCurrentState);
	}

	public void StopCheckingStates()
	{
		CancellationTokenSource.Cancel();
		CancellationTokenSource.Dispose();
		CancellationTokenSource = new CancellationTokenSource();
	}

	public void SwitchState(LckStreamingBaseState state)
	{
		if (CurrentState == state)
		{
			Log("[LCK Streaming Controller] tried switching to the same state! Current State: <color=#FF0000>" + CurrentState.GetType().Name + "</color> to: <color=#FF0000>" + state.GetType().Name + "</color>");
			return;
		}
		CancellationTokenSource.Cancel();
		CancellationTokenSource.Dispose();
		CancellationTokenSource = new CancellationTokenSource();
		Log((CurrentState != null) ? ("[LCK Streaming Controller] changing states from: <color=#42f542>" + CurrentState.GetType().Name + "</color> to: <color=#42f542>" + state.GetType().Name + "</color>") : ("[LCK Streaming Controller] changing states from: <color=#42f542>null</color> to: <color=#42f542>" + state.GetType().Name + "</color>"));
		CurrentState = state;
		IsConfiguredCorrectly = CurrentState is LckStreamingConfiguredCorrectlyState;
		CurrentState.EnterState(this);
	}

	public void StartStreaming()
	{
		if (!IsConfiguredCorrectly)
		{
			_onStreamButtonError.Invoke();
		}
		else
		{
			StartStreamIfNoLivHubChanges();
		}
	}

	private async Task StartStreamIfNoLivHubChanges()
	{
		Result<bool> result = await LckCore.HasUserConfiguredStreaming();
		if (result.IsOk && result.Ok)
		{
			_onStreamButtonPressWithCorrectConfig.Invoke();
			_lckService.StartStreaming();
		}
		else if (!result.IsOk && result.Err != CoreError.UserNotLoggedIn)
		{
			SwitchState(InternalErrorState);
			_onStreamButtonError.Invoke();
		}
		else
		{
			CheckCurrentState();
			_onStreamButtonError.Invoke();
		}
	}

	public void StopStreaming()
	{
		if (_lckService == null)
		{
			LckLog.LogWarning("LCK Could not get Service", "StopStreaming", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Streaming\\LckStreamingController.cs", 290);
		}
		else if (_lckService.IsStreaming().Result)
		{
			_lckService.StopStreaming();
		}
	}

	private void OnStreamingStarted(LckResult result)
	{
		if (!result.Success)
		{
			_notificationController.ShowNotification(NotificationType.UnknownStreamingError);
		}
	}

	public void GoToErrorState()
	{
		SwitchState(InternalErrorState);
	}

	public void LogError(string error)
	{
		if (_showDebugLogs)
		{
			Debug.LogError("[LCK Streaming Controller] " + error);
		}
	}

	public void Log(string message)
	{
		if (_showDebugLogs)
		{
			Debug.Log("[LCK Streaming Controller] " + message);
		}
	}

	public void ShowNotification(NotificationType type)
	{
		_notificationController.ShowNotification(type);
	}

	public void HideNotifications()
	{
		_notificationController.HideNotifications();
	}

	public void SetNotificationStreamCode(string code)
	{
		_notificationController.SetNotificationStreamCode(code);
	}

	public void ToggleCameraPage()
	{
		_topButtonsController.ToggleCameraPage(state: true);
		_topButtonsController.SetCameraPageVisualsManually();
	}

	private void OnValidate()
	{
		if ((bool)_topButtonsControllerGameObject && !_topButtonsControllerGameObject.activeSelf)
		{
			_topButtonsControllerGameObject.SetActive(value: true);
		}
		if ((bool)_livHubButton && !_livHubButton.activeSelf)
		{
			_livHubButton.SetActive(value: true);
		}
	}

	private void OnDestroy()
	{
		CancellationTokenSource.Cancel();
		CancellationTokenSource.Dispose();
		LckMonoBehaviourMediator.OnApplicationLifecycleEvent -= OnApplicationLifecycle;
		if (_lckService != null)
		{
			if (_lckService.IsStreaming().Result)
			{
				_lckService.StopStreaming();
			}
			_lckService.OnStreamingStarted -= OnStreamingStarted;
		}
	}
}
