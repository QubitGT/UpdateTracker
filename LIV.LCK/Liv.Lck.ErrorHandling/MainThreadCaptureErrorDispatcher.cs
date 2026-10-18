using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Liv.Lck.ErrorHandling;

internal class MainThreadCaptureErrorDispatcher : ILckCaptureErrorDispatcher, IDisposable
{
	private static readonly string _updateCoroutineName = "MainThreadCaptureErrorDispatcher:Update";

	private readonly ILckEventBus _eventBus;

	private readonly ConcurrentQueue<LckCaptureError> _errorQueue = new ConcurrentQueue<LckCaptureError>();

	private bool _isMonitoringErrors;

	[Preserve]
	public MainThreadCaptureErrorDispatcher(ILckEventBus eventBus)
	{
		_eventBus = eventBus;
		_eventBus.AddListener<LckEvents.EncoderStartedEvent>(OnEncoderStarted);
		_eventBus.AddListener<LckEvents.EncoderStoppedEvent>(OnEncoderStopped);
	}

	public void PushError(LckCaptureError error)
	{
		LckLog.LogWarning("Capture error occurred: " + error.Message, "PushError", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\ErrorHandling\\MainThreadCaptureErrorDispatcher.cs", 37);
		_errorQueue.Enqueue(error);
	}

	private void OnEncoderStarted(LckEvents.EncoderStartedEvent encoderStartedEvent)
	{
		if (encoderStartedEvent.Result.Success)
		{
			StartMonitoringErrors();
		}
	}

	private void OnEncoderStopped(LckEvents.EncoderStoppedEvent encoderStoppedEvent)
	{
		StopMonitoringErrors();
	}

	private void StartMonitoringErrors()
	{
		_isMonitoringErrors = true;
		LckMonoBehaviourMediator.StartCoroutine(_updateCoroutineName, Update());
	}

	private void StopMonitoringErrors()
	{
		_isMonitoringErrors = false;
		LckMonoBehaviourMediator.StopCoroutineByName(_updateCoroutineName);
	}

	private IEnumerable<LckCaptureError> DrainErrors()
	{
		LckCaptureError result;
		while (_errorQueue.TryDequeue(out result))
		{
			yield return result;
		}
	}

	private IEnumerator Update()
	{
		while (_isMonitoringErrors)
		{
			foreach (LckCaptureError item in DrainErrors())
			{
				_eventBus.Trigger(new LckEvents.CaptureErrorEvent(item));
			}
			yield return null;
		}
	}

	public void Dispose()
	{
		_eventBus.RemoveListener<LckEvents.EncoderStartedEvent>(OnEncoderStarted);
		_eventBus.RemoveListener<LckEvents.EncoderStoppedEvent>(OnEncoderStopped);
		if (_isMonitoringErrors)
		{
			LckMonoBehaviourMediator.StopCoroutineByName(_updateCoroutineName);
		}
	}
}
