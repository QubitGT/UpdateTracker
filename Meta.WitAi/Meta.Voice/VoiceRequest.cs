using System;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.Voice.TelemetryUtilities;
using Meta.WitAi;
using Meta.WitAi.Data;
using UnityEngine.Events;

namespace Meta.Voice;

[LogCategory(LogCategory.Requests)]
public abstract class VoiceRequest<TUnityEvent, TOptions, TEvents, TResults> : ILogSource where TUnityEvent : UnityEventBase where TOptions : IVoiceRequestOptions where TEvents : VoiceRequestEvents<TUnityEvent> where TResults : IVoiceRequestResults
{
	public static SimulatedResponse simulatedResponse;

	public virtual IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Requests);


	public VoiceRequestState State { get; private set; } = (VoiceRequestState)(-1);


	public bool IsActive
	{
		get
		{
			if (State != (VoiceRequestState)(-1) && State != 0)
			{
				return State == VoiceRequestState.Transmitting;
			}
			return true;
		}
	}

	public TaskCompletionSource<bool> Completion { get; private set; } = new TaskCompletionSource<bool>();


	public Task HoldTask { get; set; }

	public float DownloadProgress { get; private set; }

	public float UploadProgress { get; private set; }

	public TOptions Options { get; }

	public TEvents Events { get; }

	public TResults Results { get; }

	public bool CanSend => string.IsNullOrEmpty(GetSendError());

	public VoiceRequest(TOptions newOptions, TEvents newEvents)
	{
		Options = ((newOptions != null) ? newOptions : Activator.CreateInstance<TOptions>());
		Events = Activator.CreateInstance<TEvents>();
		if (newEvents != null)
		{
			AddEventListeners(newEvents);
		}
		Results = GetNewResults();
		SetState(VoiceRequestState.Initialized);
	}

	protected virtual TResults GetNewResults()
	{
		return Activator.CreateInstance<TResults>();
	}

	public void AddEventListeners(TEvents newEvents)
	{
		SetEventListeners(newEvents, addListeners: true);
	}

	public void RemoveEventListeners(TEvents newEvents)
	{
		SetEventListeners(newEvents, addListeners: false);
	}

	protected abstract void SetEventListeners(TEvents newEvents, bool addListeners);

	protected abstract void RaiseEvent(TUnityEvent requestEvent);

	protected virtual void OnInit()
	{
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnInit : null);
		SetUploadProgress(0f);
		SetDownloadProgress(0f);
	}

	protected virtual void SetState(VoiceRequestState newState)
	{
		if (State == newState)
		{
			return;
		}
		State = newState;
		OnStateChange();
		bool flag = false;
		switch (State)
		{
		case VoiceRequestState.Initialized:
			try
			{
				OnInit();
			}
			catch (Exception e4)
			{
				LogE("OnInit Exception Caught", e4);
			}
			break;
		case VoiceRequestState.Transmitting:
			try
			{
				OnSend();
			}
			catch (Exception e2)
			{
				LogE("OnSend Exception Caught", e2);
			}
			WaitForHold(HoldSend);
			break;
		case VoiceRequestState.Canceled:
			try
			{
				HandleCancel();
			}
			catch (Exception e5)
			{
				LogE("HandleCancel Exception Caught", e5);
			}
			try
			{
				OnCancel();
			}
			catch (Exception e6)
			{
				LogE("OnCancel Exception Caught", e6);
			}
			flag = true;
			break;
		case VoiceRequestState.Failed:
			try
			{
				OnFailed();
			}
			catch (Exception e3)
			{
				LogE("OnFailed Exception Caught", e3);
			}
			flag = true;
			break;
		case VoiceRequestState.Successful:
			try
			{
				OnSuccess();
			}
			catch (Exception e)
			{
				LogE("OnSuccess Exception Caught", e);
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			return;
		}
		try
		{
			OnComplete();
		}
		catch (Exception e7)
		{
			LogE("OnComplete Exception Caught", e7);
		}
	}

	protected virtual void OnStateChange()
	{
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnStateChange : null);
	}

	protected void WaitForHold(Action onReady)
	{
		ThreadUtility.BackgroundAsync(Logger, async delegate
		{
			if (HoldTask != null)
			{
				await HoldTask;
			}
			await ThreadUtility.CallOnMainThread(delegate
			{
				onReady?.Invoke();
			});
		}).WrapErrors();
	}

	protected virtual void HoldSend()
	{
		if (State == VoiceRequestState.Transmitting && !OnSimulateResponse())
		{
			HandleSend();
		}
	}

	protected void SetDownloadProgress(float newProgress)
	{
		if (!DownloadProgress.Equals(newProgress))
		{
			DownloadProgress = newProgress;
			TEvents events = Events;
			RaiseEvent((events != null) ? events.OnDownloadProgressChange : null);
		}
	}

	protected void SetUploadProgress(float newProgress)
	{
		if (!UploadProgress.Equals(newProgress))
		{
			UploadProgress = newProgress;
			TEvents events = Events;
			RaiseEvent((events != null) ? events.OnUploadProgressChange : null);
		}
	}

	protected virtual void Log(string log, VLoggerVerbosity logLevel = VLoggerVerbosity.Info)
	{
		IVLogger logger = Logger;
		CorrelationID correlationID = Logger.CorrelationID;
		object[] obj = new object[3] { log, null, null };
		TOptions options = Options;
		obj[1] = ((options != null) ? options.RequestId : null);
		obj[2] = State;
		logger.Log(correlationID, logLevel, "{0}\nRequest Id: {1}\nRequest State: {2}", obj);
	}

	protected void LogW(string log)
	{
		Log(log, VLoggerVerbosity.Warning);
	}

	protected void LogE(string log, Exception e)
	{
		Log($"{log}\n\n{e}", VLoggerVerbosity.Error);
	}

	protected virtual string GetSendError()
	{
		if (State != 0)
		{
			return $"Cannot send request in '{State}' state.";
		}
		TOptions options = Options;
		if (string.IsNullOrEmpty((options != null) ? options.RequestId : null))
		{
			return "Cannot send request without a request id.";
		}
		return string.Empty;
	}

	public virtual void Send()
	{
		if (State != 0)
		{
			LogW("Request Send Ignored\nReason: Invalid state");
			return;
		}
		string sendError = GetSendError();
		if (!string.IsNullOrEmpty(sendError))
		{
			HandleFailure(sendError);
		}
		else
		{
			SetState(VoiceRequestState.Transmitting);
		}
	}

	protected virtual void OnSend()
	{
		Log("Request Transmitting");
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnSend : null);
	}

	protected abstract void HandleSend();

	protected virtual bool OnSimulateResponse()
	{
		return false;
	}

	protected virtual void HandleFailure(string error)
	{
		HandleFailure(-1, error);
	}

	protected virtual void HandleFailure(int errorStatusCode, string errorMessage)
	{
		if (!IsActive)
		{
			LogW("Request Failure Ignored\nReason: Request is already complete");
			return;
		}
		if (string.Equals("Cancelled", errorMessage))
		{
			Cancel();
			return;
		}
		if (ShouldIgnoreError(errorStatusCode, errorMessage))
		{
			HandleSuccess();
			return;
		}
		Results.SetError(errorStatusCode, errorMessage);
		SetState(VoiceRequestState.Failed);
	}

	protected virtual bool ShouldIgnoreError(int errorStatusCode, string errorMessage)
	{
		return string.IsNullOrEmpty(errorMessage);
	}

	protected virtual void OnFailed()
	{
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnFailed : null);
	}

	protected virtual void HandleSuccess()
	{
		if (!IsActive)
		{
			LogW("Request Success Ignored\nReason: Request is already complete");
		}
		else
		{
			SetState(VoiceRequestState.Successful);
		}
	}

	protected virtual void OnSuccess()
	{
		Log($"Request Success\nResults: {Results != null}");
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnSuccess : null);
	}

	public virtual void Cancel(string reason = "Request was cancelled.")
	{
		if (!IsActive)
		{
			LogW("Request Cancel Ignored\nReason: Request is already complete");
			return;
		}
		Results.SetCancel(reason);
		SetState(VoiceRequestState.Canceled);
	}

	protected abstract void HandleCancel();

	protected virtual void OnCancel()
	{
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnCancel : null);
	}

	protected virtual void OnComplete()
	{
		Completion.SetResult(State != VoiceRequestState.Failed);
		TEvents events = Events;
		RaiseEvent((events != null) ? events.OnComplete : null);
		switch (State)
		{
		case VoiceRequestState.Canceled:
			RuntimeTelemetry.Instance.LogEventTermination((OperationID)Options.OperationId, TerminationReason.Canceled);
			break;
		case VoiceRequestState.Failed:
			RuntimeTelemetry.Instance.LogEventTermination((OperationID)Options.OperationId, TerminationReason.Failed);
			break;
		default:
			RuntimeTelemetry.Instance.LogEventTermination((OperationID)Options.OperationId, TerminationReason.Undetermined);
			break;
		case VoiceRequestState.Successful:
			break;
		}
	}

	protected void MainThreadCallback(Action action)
	{
		ThreadUtility.CallOnMainThread(action);
	}
}
