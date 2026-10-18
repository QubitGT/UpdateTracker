using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice;
using Meta.Voice.Logging;
using Meta.WitAi.Attributes;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Integrations;
using Meta.WitAi.Composer.Interfaces;
using Meta.WitAi.Configuration;
using Meta.WitAi.Events;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Composer;

public abstract class ComposerService : MonoBehaviour, ILogSource
{
	private class CurrentComposerRequest
	{
		private ComposerService _service;

		public readonly ComposerSessionData SessionData;

		public VoiceServiceRequest Request { get; private set; }

		public string SessionId => SessionData?.sessionID;

		public bool IsActive
		{
			get
			{
				if (Request != null)
				{
					return Request.IsActive;
				}
				return false;
			}
		}

		public CurrentComposerRequest(ComposerService service, VoiceServiceRequest request, ComposerSessionData sessionData)
		{
			_service = service;
			SessionData = sessionData;
			SessionData.responseData = new ComposerResponseData();
			Request = request;
			Request.Events.OnSend.AddListener(OnSend);
			Request.Events.OnPartialResponse.AddListener(OnPartial);
			Request.Events.OnValidateResponse.AddListener(OnValidate);
			Request.Events.OnComplete.AddListener(OnComplete);
			if (Request.ResponseData != null)
			{
				OnPartial(Request.ResponseData);
			}
		}

		private void OnSend(VoiceServiceRequest r)
		{
			UpdateResponseData(r.ResponseData);
			_service.OnVoiceRequestSend(SessionData, r);
		}

		private void OnPartial(WitResponseNode r)
		{
			UpdateResponseData(r);
			_service.OnVoicePartialResponse(SessionData);
		}

		private void OnValidate(WitResponseNode r, StringBuilder validationErrors)
		{
			WitResponseClass obj = r?.AsObject;
			if ((object)obj == null || !obj.HasChild("context_map"))
			{
				if (validationErrors.Length > 0)
				{
					validationErrors.Append(", ");
				}
				validationErrors.Append("missing context map");
			}
		}

		private void OnComplete(VoiceServiceRequest r)
		{
			Request.Events.OnSend.RemoveListener(OnSend);
			Request.Events.OnPartialResponse.RemoveListener(OnPartial);
			Request.Events.OnValidateResponse.RemoveListener(OnValidate);
			Request.Events.OnComplete.RemoveListener(OnComplete);
			try
			{
				UpdateResponseData(r.ResponseData);
				_service.OnVoiceRequestComplete(this);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Update Failure\n{arg}");
			}
		}

		private void UpdateResponseData(WitResponseNode r)
		{
			SessionData.responseData = r.GetComposerResponse();
		}
	}

	[Header("Voice Settings")]
	[SerializeField]
	private VoiceService _voiceService;

	[Tooltip("Whether or not to send all voice service requests through composer.  If disabled, composer will only send requests made directly from composer.")]
	[FormerlySerializedAs("RouteVoiceServiceToComposer")]
	[SerializeField]
	private bool _routeVoiceServiceToComposer = true;

	[Tooltip("Whether or not to end the previous session when starting a new one")]
	public bool EndLastSessionOnStart = true;

	[Header("Tts Settings")]
	[Tooltip("Whether or not partial tts responses should be sent to attached speech handlers")]
	[FormerlySerializedAs("_handlePartialTts")]
	[SerializeField]
	public bool handlePartialTts = true;

	[Tooltip("Whether or not final tts responses should be sent to attached speech handlers")]
	[FormerlySerializedAs("handleTts")]
	[FormerlySerializedAs("_handleFinalTts")]
	[SerializeField]
	public bool handleFinalTts;

	[Tooltip("Handles response message load and playback")]
	[SerializeField]
	protected IComposerSpeechHandler[] _speechHandlers;

	[Header("Action Settings")]
	[Tooltip("Whether or not response actions should be handled using the action handlers")]
	[FormerlySerializedAs("_handleActions")]
	[SerializeField]
	public bool handleActions = true;

	[Tooltip("Handles response message action calls")]
	[SerializeField]
	protected IComposerActionHandler _actionHandler;

	private List<CurrentComposerRequest> _requests = new List<CurrentComposerRequest>();

	private object _requestLock = new object();

	[Header("Composer Settings")]
	public float continueDelay;

	[Tooltip("A configurable flag for use in the Composer graph to differentiate activations to the server without text/voice input, such as a context map update. In such cases, this will be set to true. \nFor voice and text activations, this will be set to false.")]
	[SerializeField]
	public string contextMapEventKey = "state_event";

	public bool expectInputAutoActivation = true;

	public bool endSessionOnCompletion;

	public bool clearContextMapOnCompletion;

	[SerializeField]
	public bool debug;

	[Obsolete("Use WitConfiguration.editorVersionTag instead.")]
	[SerializeField]
	[HideInInspector]
	public string editorVersionTag;

	[Obsolete("Use WitConfiguration.buildVersionTag instead.")]
	[SerializeField]
	[HideInInspector]
	public string buildVersionTag;

	[Tooltip("Events that will fire before, during and after an activation")]
	[SerializeField]
	private ComposerEvents _events = new ComposerEvents();

	[Tooltip("Events that will fire during Context Map change")]
	[SerializeField]
	private ContextEvents _contextEvents = new ContextEvents();

	[ObjectType(typeof(IComposerSessionProvider), new Type[] { })]
	[SerializeField]
	private UnityEngine.Object _sessionProvider;

	private bool _ttsHandled;

	private HashSet<string> _actionsHandled = new HashSet<string>();

	private bool _enabled;

	private ConcurrentQueue<Task> _eventTtsTasks = new ConcurrentQueue<Task>();

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Composer);


	public ConcurrentDictionary<string, IComposerSession> ActiveSessions { get; } = new ConcurrentDictionary<string, IComposerSession>();


	public string LastSessionId { get; private set; }

	public ComposerContextMap CurrentContextMap { get; } = new ComposerContextMap();


	public bool RouteVoiceServiceToComposer
	{
		get
		{
			return _routeVoiceServiceToComposer;
		}
		set
		{
			_routeVoiceServiceToComposer = value;
			Events.OnComposerActiveChange?.Invoke(this, value);
		}
	}

	public VoiceService VoiceService => _voiceService;

	public bool IsComposerActive => _requests.Count > 0;

	public ComposerEvents Events => _events;

	public ContextEvents ContextMapEvents => _contextEvents;

	public IComposerSessionProvider SessionProvider
	{
		get
		{
			return _sessionProvider as IComposerSessionProvider;
		}
		set
		{
			if (value is UnityEngine.Object sessionProvider)
			{
				_sessionProvider = sessionProvider;
				return;
			}
			if (value != null)
			{
				Logger.Error("Set invalid IComposerSessionProvider type ({0})\nReason: Must inherit from {1}", value?.GetType().Name ?? "Null", "Object");
				throw new ArgumentException("Set invalid IComposerSessionProvider type");
			}
			_sessionProvider = null;
		}
	}

	[Obsolete("Use 'LastSessionId' or 'ActiveSessions' instead.")]
	public string SessionID => LastSessionId;

	[Obsolete("Use 'ActiveSessions' instead.")]
	public DateTime SessionStart
	{
		get
		{
			if (ActiveSessions.TryGetValue(LastSessionId, out var value))
			{
				return value.SessionStart;
			}
			return DateTime.MinValue;
		}
	}

	[Obsolete("Use 'ActiveSessions' instead.")]
	public TimeSpan SessionElapsed => SessionStart - DateTime.UtcNow;

	protected abstract IComposerRequestHandler GetRequestHandler();

	protected virtual void Awake()
	{
		if (_voiceService == null)
		{
			_voiceService = base.gameObject.GetComponentInChildren<VoiceService>();
			if (_voiceService == null)
			{
				Log("No Voice Service found", error: true);
			}
		}
		if (_speechHandlers == null)
		{
			_speechHandlers = base.gameObject.GetComponentsInChildren<IComposerSpeechHandler>();
		}
		if (_actionHandler == null)
		{
			_actionHandler = base.gameObject.GetComponentInChildren<IComposerActionHandler>();
		}
	}

	protected virtual void OnEnable()
	{
		_enabled = true;
		if (_voiceService != null)
		{
			VoiceEvents voiceEvents = _voiceService.VoiceEvents;
			voiceEvents.OnRequestFinalize = (Action<VoiceServiceRequest>)Delegate.Combine(voiceEvents.OnRequestFinalize, new Action<VoiceServiceRequest>(RouteVoiceServiceActivation));
		}
		CurrentContextMap.OnContextMapValueChanged.AddListener(RaiseOnContextMapValueChanged);
		CurrentContextMap.OnContextMapValueRemoved.AddListener(RaiseOnContextMapValueRemoved);
	}

	protected virtual void OnDisable()
	{
		_enabled = false;
		if (_voiceService != null)
		{
			VoiceEvents voiceEvents = _voiceService.VoiceEvents;
			voiceEvents.OnRequestFinalize = (Action<VoiceServiceRequest>)Delegate.Remove(voiceEvents.OnRequestFinalize, new Action<VoiceServiceRequest>(RouteVoiceServiceActivation));
		}
		CurrentContextMap.OnContextMapValueChanged.RemoveListener(RaiseOnContextMapValueChanged);
		CurrentContextMap.OnContextMapValueRemoved.RemoveListener(RaiseOnContextMapValueRemoved);
	}

	protected virtual void OnDestroy()
	{
	}

	private void LogState(string state, VoiceServiceRequest request)
	{
		if (debug)
		{
			Logger.Verbose("{0} [{1}]", state, request?.Options?.RequestId ?? "Unknown ID", null, null, "LogState", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 343);
		}
	}

	protected void Log(string comment, bool error = false)
	{
		if (error || debug)
		{
			if (error)
			{
				Logger.Error("{0}\nScript: {1}\nGameObject: {2}\nActive Sessions: {3}", comment, GetType().Name, (base.gameObject == null) ? "Null" : base.gameObject.name, ActiveSessions.Count);
			}
			else if (debug)
			{
				Logger.Verbose("{0}\nScript: {1}\nGameObject: {2}\nActive Sessions: {3}", comment, GetType().Name, (base.gameObject == null) ? "Null" : base.gameObject.name, ActiveSessions.Count, "Log", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 364);
			}
		}
	}

	public bool IsSessionActive(string sessionId)
	{
		if (!string.IsNullOrEmpty(sessionId))
		{
			return ActiveSessions.ContainsKey(sessionId);
		}
		return false;
	}

	public IComposerSession StartSession(string sessionId = null)
	{
		if (string.IsNullOrEmpty(sessionId))
		{
			sessionId = GetDefaultSessionID();
		}
		if (ActiveSessions.TryGetValue(sessionId, out var value))
		{
			return value;
		}
		if (EndLastSessionOnStart && !string.IsNullOrEmpty(LastSessionId))
		{
			EndSession(LastSessionId);
		}
		Logger.Verbose("Start Composer Session\nSession Id: {0}", sessionId, null, null, null, "StartSession", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 405);
		value = GenerateSession(sessionId);
		ActiveSessions[sessionId] = value;
		LastSessionId = sessionId;
		value.StartSession();
		Events?.OnComposerSessionBegin?.Invoke(GetSessionData(value));
		return value;
	}

	public bool EndSession(string sessionId)
	{
		if (string.IsNullOrEmpty(sessionId))
		{
			return false;
		}
		if (!ActiveSessions.TryRemove(sessionId, out var value))
		{
			return false;
		}
		Logger.Verbose("End Composer Session\nSession Id: {0}", sessionId, null, null, null, "EndSession", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 435);
		if (string.Equals(LastSessionId, sessionId))
		{
			LastSessionId = null;
		}
		ComposerSessionData sessionData = GetSessionData(value);
		value.EndSession();
		Events?.OnComposerSessionEnd?.Invoke(sessionData);
		return true;
	}

	protected virtual IComposerSession GenerateSession(string sessionId)
	{
		return new BaseComposerSession(sessionId, CurrentContextMap);
	}

	protected virtual string GetSessionId(VoiceServiceRequest request)
	{
		if (request.ResponseData != null)
		{
			string value = request.ResponseData["session_id"].Value;
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}
		}
		string text = SessionProvider?.GetComposerSessionId(this, request);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (!string.IsNullOrEmpty(LastSessionId))
		{
			return LastSessionId;
		}
		return GetDefaultSessionID();
	}

	public virtual string GetDefaultSessionID()
	{
		return WitConstants.GetUniqueId();
	}

	protected virtual ComposerSessionData GetSessionData(IComposerSession session)
	{
		return new ComposerSessionData
		{
			session = session,
			composer = this,
			responseData = null
		};
	}

	protected virtual void UpdateContextMap(ComposerSessionData sessionData)
	{
		if (CurrentContextMap.UpdateData(sessionData.responseData.witResponse))
		{
			RaiseContextMapChanged(sessionData);
		}
	}

	protected virtual void RaiseContextMapChanged(ComposerSessionData sessionData)
	{
		if (Events.OnComposerContextMapChange != null)
		{
			ThreadUtility.CallOnMainThread(Logger, delegate
			{
				Events.OnComposerContextMapChange.Invoke(sessionData);
			});
		}
	}

	protected virtual void RaiseOnContextMapValueChanged(string key, string oldValue, string newValue)
	{
		if (ContextMapEvents.OnContextMapValueChanged != null)
		{
			ThreadUtility.CallOnMainThread(Logger, delegate
			{
				ContextMapEvents.OnContextMapValueChanged.Invoke(key, oldValue, newValue);
			});
		}
	}

	protected virtual void RaiseOnContextMapValueRemoved(string key)
	{
		if (ContextMapEvents.OnContextMapValueRemoved != null)
		{
			ThreadUtility.CallOnMainThread(Logger, delegate
			{
				ContextMapEvents.OnContextMapValueRemoved.Invoke(key);
			});
		}
	}

	public void Activate(string message)
	{
		_voiceService?.Activate(message);
	}

	public void Activate()
	{
		_voiceService?.Activate();
	}

	public void ActivateImmediately()
	{
		_voiceService?.ActivateImmediately();
	}

	public void Deactivate()
	{
		_voiceService?.Deactivate();
	}

	public void DeactivateAndAbortRequest()
	{
		_voiceService?.DeactivateAndAbortRequest();
	}

	public void SendContextMapEvent()
	{
		SendEvent(string.Empty);
	}

	public Task SendContextMapEvent(WitRequestOptions requestOptions)
	{
		return SendEvent(string.Empty, requestOptions);
	}

	public void SendEvent(string eventJson)
	{
		if (!(_voiceService == null))
		{
			_voiceService.Activate(eventJson);
		}
	}

	public async Task SendEvent(string eventJson, WitRequestOptions requestOptions)
	{
		if (_voiceService == null)
		{
			throw new Exception("Cannot SendEvent without VoiceService");
		}
		if (requestOptions == null)
		{
			requestOptions = new WitRequestOptions();
		}
		VoiceServiceRequest voiceServiceRequest = await _voiceService.Activate(eventJson, requestOptions);
		await voiceServiceRequest.Completion.Task;
		Task[] ttsTasks = _eventTtsTasks.ToArray();
		_eventTtsTasks.Clear();
		if (voiceServiceRequest.State == VoiceRequestState.Failed)
		{
			throw new Exception(voiceServiceRequest.Results.Message);
		}
		if (voiceServiceRequest.State == VoiceRequestState.Canceled)
		{
			throw new Exception("Cancelled");
		}
		foreach (Task ttsTask in ttsTasks)
		{
			if (ttsTask != null)
			{
				await ttsTask;
				if (ttsTask.Exception != null && ttsTask.Exception.InnerException != null)
				{
					throw ttsTask.Exception.InnerException;
				}
			}
		}
	}

	public void AddEventTtsTask(Task ttsTask)
	{
		if (_eventTtsTasks != null)
		{
			_eventTtsTasks.Enqueue(ttsTask);
		}
	}

	private bool IsRequestTracked(VoiceServiceRequest request)
	{
		return _requests.FirstOrDefault((CurrentComposerRequest compRequest) => compRequest?.Request != null && compRequest.Request.Equals(request)) != null;
	}

	protected virtual void RouteVoiceServiceActivation(VoiceServiceRequest request)
	{
		if (!RouteVoiceServiceToComposer || IsRequestTracked(request))
		{
			return;
		}
		if (!_enabled)
		{
			request.Cancel("Composer disabled");
			return;
		}
		string sessionId = GetSessionId(request);
		if (!ActiveSessions.TryGetValue(sessionId, out var value))
		{
			value = StartSession(sessionId);
		}
		ComposerSessionData sessionData = GetSessionData(value);
		CurrentComposerRequest item = new CurrentComposerRequest(this, request, sessionData);
		lock (_requestLock)
		{
			request.HoldTask = Task.WhenAll(_requests.Select((CurrentComposerRequest check) => string.Equals(sessionData.sessionID, check?.SessionId) ? check?.Request?.Completion?.Task : null));
			_requests.Add(item);
		}
		RaiseRequestCreated(sessionData, request);
		SetupComposerRequest(sessionData, request);
	}

	protected virtual void SetupComposerRequest(ComposerSessionData sessionData, VoiceServiceRequest request)
	{
		GetRequestHandler()?.OnComposerRequestSetup(sessionData, request);
		RaiseRequestSetup(sessionData, request);
	}

	protected virtual void RaiseRequestCreated(ComposerSessionData sessionData, VoiceServiceRequest request)
	{
		LogState("Request Setup", request);
		Events?.OnComposerRequestCreated?.Invoke(sessionData, request);
		ThreadUtility.CallOnMainThread(delegate
		{
			Events?.OnComposerActivation?.Invoke(sessionData);
		}).WrapErrors();
	}

	protected virtual void RaiseRequestSetup(ComposerSessionData sessionData, VoiceServiceRequest request)
	{
		LogState("Request Setup", request);
		Events.OnComposerRequestInit?.Invoke(sessionData);
		Events.OnComposerRequestSetup?.Invoke(sessionData, request);
	}

	protected virtual void OnVoiceRequestSend(ComposerSessionData sessionData, VoiceServiceRequest request)
	{
		LogState("Request Send", request);
		Events.OnComposerRequestBegin?.Invoke(sessionData);
	}

	protected virtual void OnVoicePartialResponse(ComposerSessionData sessionData)
	{
		UpdateContextMap(sessionData);
		if (!string.IsNullOrEmpty(sessionData.responseData.responsePhrase))
		{
			_ttsHandled |= OnComposerSpeakPhrase(sessionData);
		}
		string text = sessionData.responseData?.actionID;
		if (!_actionsHandled.Contains(text) && !string.IsNullOrEmpty(text) && OnComposerPerformAction(sessionData))
		{
			_actionsHandled.Add(text);
		}
		ComposerResponseData responseData = sessionData.responseData;
		if (responseData != null && responseData.witResponse["FULL_COMPOSER"].AsBool)
		{
			_actionsHandled.Clear();
		}
	}

	private void OnVoiceRequestComplete(CurrentComposerRequest composerRequest)
	{
		ThreadUtility.BackgroundAsync(Logger, async delegate
		{
			lock (_requestLock)
			{
				if (!_requests.Remove(composerRequest))
				{
					Logger.Warning("Completed composer request not found\nId: {0}", composerRequest?.Request?.Options?.RequestId ?? "Null");
				}
			}
			await ThreadUtility.CallOnMainThread(delegate
			{
				if (composerRequest.Request.State == VoiceRequestState.Canceled)
				{
					OnComposerCanceled(composerRequest.SessionData, composerRequest.Request.Results.Message);
				}
				else if (composerRequest.Request.State == VoiceRequestState.Failed)
				{
					OnComposerError(composerRequest.SessionData, composerRequest.Request.Results.Message);
				}
				else if (composerRequest.Request.State == VoiceRequestState.Successful)
				{
					OnComposerResponse(composerRequest.SessionData, composerRequest.Request.ResponseData);
				}
				LogState("Request Complete", composerRequest.Request);
			});
		});
	}

	protected virtual void OnComposerCanceled(ComposerSessionData sessionData, string reason)
	{
		sessionData.responseData = new ComposerResponseData(reason);
		if (debug)
		{
			Logger.Verbose($"Request Canceled\nReason: {0}", sessionData.responseData.error, null, null, null, "OnComposerCanceled", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 849);
		}
		Events.OnComposerCanceled?.Invoke(sessionData);
	}

	protected virtual void OnComposerError(ComposerSessionData sessionData, string error)
	{
		sessionData.responseData = new ComposerResponseData(error);
		if (debug)
		{
			Logger.Error("Request Error\nError: {0}\n{1}", sessionData.responseData.error, sessionData.responseData.witResponse);
		}
		Events.OnComposerError?.Invoke(sessionData);
	}

	protected virtual void OnComposerResponse(ComposerSessionData sessionData, WitResponseNode response)
	{
		string text = sessionData.responseData?.actionID;
		if (response != sessionData.responseData?.witResponse)
		{
			sessionData.responseData = response.GetComposerResponse();
			OnVoicePartialResponse(sessionData);
		}
		else if (!string.IsNullOrEmpty(text) && !_actionsHandled.Contains(text) && OnComposerPerformAction(sessionData))
		{
			_actionsHandled.Add(text);
		}
		Log("Request Success");
		Events.OnComposerResponse?.Invoke(sessionData);
		bool flag = _ttsHandled || _actionsHandled.Count > 0;
		_ttsHandled = false;
		_actionsHandled.Clear();
		if (sessionData.responseData != null && sessionData.responseData.expectsInput)
		{
			flag = true;
		}
		if (flag)
		{
			CoroutineUtility.StartCoroutine(WaitToContinue(sessionData));
		}
	}

	protected virtual bool OnComposerSpeakPhrase(ComposerSessionData sessionData)
	{
		bool responseIsFinal = sessionData.responseData.responseIsFinal;
		if (!responseIsFinal && !handlePartialTts)
		{
			return false;
		}
		if (responseIsFinal && !handleFinalTts)
		{
			return false;
		}
		if (debug)
		{
			Logger.Verbose($"Perform Speak\nPhrase: {0}\nFinal Response: {1}", sessionData.responseData.responsePhrase, responseIsFinal, null, null, "OnComposerSpeakPhrase", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 920);
		}
		Events.OnComposerSpeakPhrase?.Invoke(sessionData);
		int num = 0;
		while (_speechHandlers != null && num < _speechHandlers.Length)
		{
			_speechHandlers[num].SpeakPhrase(sessionData);
			num++;
		}
		return true;
	}

	protected virtual bool OnComposerPerformAction(ComposerSessionData sessionData)
	{
		if (!handleActions)
		{
			return false;
		}
		if (debug)
		{
			Logger.Verbose("Perform Action\nAction: {0}", sessionData?.responseData?.actionID, null, null, null, "OnComposerPerformAction", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Features\\Composer\\Composer\\Lib\\Wit.ai\\Features\\composer\\Scripts\\Runtime\\ComposerService.cs", 942);
		}
		Events.OnComposerPerformAction?.Invoke(sessionData);
		if (_actionHandler != null)
		{
			_actionHandler.PerformAction(sessionData);
		}
		return true;
	}

	protected virtual void OnComposerExpectsInput(ComposerSessionData sessionData)
	{
		Log("Expects Input");
		Events.OnComposerExpectsInput?.Invoke(sessionData);
		if (expectInputAutoActivation && _voiceService != null)
		{
			_voiceService.Activate();
		}
	}

	protected virtual void OnComposerComplete(ComposerSessionData sessionData)
	{
		Log("Graph Complete");
		Events.OnComposerComplete?.Invoke(sessionData);
		if (endSessionOnCompletion)
		{
			EndSession(sessionData.sessionID);
		}
		if (clearContextMapOnCompletion)
		{
			CurrentContextMap.ClearAllNonReservedData();
		}
	}

	private IEnumerator WaitToContinue(ComposerSessionData sessionData)
	{
		Log("Wait to Continue - Begin");
		yield return null;
		yield return new WaitUntil(() => IsContinueAllowed(sessionData));
		yield return new WaitForSeconds(continueDelay);
		Log("Wait to Continue - Complete");
		if (sessionData.responseData.expectsInput)
		{
			OnComposerExpectsInput(sessionData);
		}
		else
		{
			OnComposerComplete(sessionData);
		}
	}

	protected virtual bool IsContinueAllowed(ComposerSessionData sessionData)
	{
		if (_voiceService.IsRequestActive)
		{
			return false;
		}
		int num = 0;
		while (_speechHandlers != null && num < _speechHandlers.Length)
		{
			if (_speechHandlers[num].IsSpeaking(sessionData))
			{
				return false;
			}
			num++;
		}
		if (_actionHandler != null && _actionHandler.IsPerformingAction(sessionData))
		{
			return false;
		}
		return true;
	}
}
