using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GorillaUtil;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class DebugHudStats : MonoBehaviour
{
	private enum State
	{
		Inactive,
		Active,
		ShowLog,
		ShowError,
		ShowStats,
		ShowRBs,
		TimeAdjust,
		RecordingMode,
		TitleDataMonitor
	}

	public static int FPS_THRESHOLD = 89;

	[SerializeField]
	public TMP_Text text;

	[SerializeField]
	public TMP_Text logging;

	[SerializeField]
	public TMP_Text logPage;

	[SerializeField]
	private TMP_Text fpsWarning;

	[SerializeField]
	private TMP_Text dismiss;

	[SerializeField]
	private float delayUpdateRate = 0.25f;

	private float _updateTimer;

	private float _firstAwake;

	private bool _leftHandTracked;

	private bool _rightHandTracked;

	private StringBuilder _builder;

	private StringBuilder _logBuilder = new StringBuilder(4096);

	private StringBuilder _logTitleBuilder = new StringBuilder(64);

	private Vector3 _averagedVelocity;

	private Vector3 _groundVelocity;

	private Vector3 _centerHeadPos;

	private float _distanceMoved;

	private float _distanceSwam;

	private List<string> _logMessage = new List<string>();

	private List<string> _logError = new List<string>();

	private List<string> _logTD = new List<string>();

	private bool _buttonDown;

	private bool _buttonDownBack;

	private bool _spoofIds;

	private int _lowFps;

	private string _zones;

	private GroupJoinZoneAB _lastGroupJoinZone;

	private ZoneDef _lastZone;

	private GTZone _lastZoneId;

	private GTSubZone _lastSubZoneId;

	private State _currentState = State.Active;

	private ProfilerRecorder _drawCallsRecorder;

	private ProfilerRecorder _trisRecorder;

	private string _pLog;

	private bool _logDirty;

	private bool _button1Down;

	private bool _button2Down;

	private bool _button3Down;

	private bool _button5Down;

	private bool _button6Down;

	private bool _button7Down;

	private bool _button8Down;

	[SerializeField]
	private StringTable betaTitleDataOveride;

	private Array _fixedWeathers;

	private int _fixedWeatherIndex;

	private float _btnDownTime;

	public static DebugHudStats Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = this;
			_fixedWeathers = Enum.GetValues(typeof(BetterDayNightManager.WeatherType));
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			if (_drawCallsRecorder.Valid)
			{
				_drawCallsRecorder.Dispose();
			}
			if (_trisRecorder.Valid)
			{
				_trisRecorder.Dispose();
			}
		}
	}

	private void LateUpdate()
	{
		Camera main = Camera.main;
		if (main != null)
		{
			Vector3 worldUp = ((GTPlayerTransform.Instance != null) ? GTPlayerTransform.Instance.GravityUp : Vector3.up);
			base.transform.LookAt(main.transform.position, worldUp);
		}
		if (!ControllerInputPoller.HandTrackingActive())
		{
			if (_currentState == State.TimeAdjust)
			{
				bool flag = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand);
				bool flag2 = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
				bool flag3 = ControllerInputPoller.TriggerFloat(XRNode.RightHand) > 0.5f;
				bool flag4 = ControllerInputPoller.GripFloat(XRNode.RightHand) > 0.5f;
				Vector2 vector = ControllerInputPoller.Primary2DAxis(XRNode.LeftHand);
				bool flag5 = vector.x > 0.5f;
				bool flag6 = vector.x < -0.5f;
				bool flag7 = vector.y > 0.5f;
				bool flag8 = vector.y < -0.5f;
				if (_button1Down && !flag)
				{
					GorillaComputer.instance.AddSeverTime(flag4 ? (-60) : 60);
				}
				if (_button2Down && !flag2)
				{
					GorillaComputer.instance.AddSeverTime(flag4 ? (-1) : 5);
				}
				if (_button3Down && !flag3)
				{
					GorillaComputer.instance.AddSeverTime(flag4 ? (-1440) : 1440);
				}
				if (!_button5Down && flag5)
				{
					ChangeTOD(1);
				}
				if (!_button6Down && flag6)
				{
					ChangeTOD(-1);
				}
				if (!_button7Down && flag7)
				{
					ChangeWeather(1);
				}
				if (!_button8Down && flag8)
				{
					ChangeWeather(-1);
				}
				_button1Down = flag;
				_button2Down = flag2;
				_button3Down = flag3;
				_button5Down = flag5;
				_button6Down = flag6;
				_button7Down = flag7;
				_button8Down = flag8;
			}
			if (_currentState == State.TitleDataMonitor || _currentState == State.ShowLog || _currentState == State.ShowError)
			{
				bool flag9 = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand);
				bool flag10 = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
				if (_button1Down && !flag9)
				{
					logging.pageToDisplay = ((logging.pageToDisplay >= logging.textInfo.pageCount) ? 1 : (logging.pageToDisplay + 1));
					UpdateLogTitle();
				}
				if (_button2Down && !flag10)
				{
					logging.pageToDisplay = ((logging.pageToDisplay > 1) ? (logging.pageToDisplay - 1) : logging.textInfo.pageCount);
					UpdateLogTitle();
				}
				_button1Down = flag9;
				_button2Down = flag10;
			}
			bool flag11 = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
			bool flag12 = ControllerInputPoller.PrimaryButtonPress(XRNode.LeftHand);
			if ((_buttonDown && !flag11) || (_buttonDownBack && !flag12))
			{
				NextState(_buttonDown);
				if (_currentState == State.ShowStats)
				{
					_distanceMoved = (_distanceSwam = 0f);
					PlayerGameEvents.OnPlayerMoved += OnPlayerMoved;
					PlayerGameEvents.OnPlayerSwam += OnPlayerSwam;
				}
				text.gameObject.SetActive(_currentState != State.Inactive);
				if (RigidbodyHighlighter.Instance != null)
				{
					RigidbodyHighlighter.Instance.Active = _currentState == State.ShowRBs;
				}
				_btnDownTime = 0f;
			}
			_buttonDown = flag11;
			_buttonDownBack = flag12;
		}
		if (_logDirty)
		{
			_logDirty = false;
			RefreshLog();
		}
		if (_firstAwake == 0f)
		{
			_firstAwake = Time.time;
		}
		if (_updateTimer < delayUpdateRate)
		{
			_updateTimer += Time.deltaTime;
			return;
		}
		int num = ((Time.smoothDeltaTime > 0f) ? Mathf.RoundToInt(1f / Time.smoothDeltaTime) : 0);
		if (num < FPS_THRESHOLD)
		{
			_lowFps++;
		}
		else
		{
			_lowFps = 0;
		}
		SetActiveIfChanged(fpsWarning.gameObject, _lowFps > 5 && _currentState == State.Inactive);
		if (_currentState != 0)
		{
			_builder.Clear();
			_builder.Append("gt: ");
			_builder.Append(GorillaComputer.instance.version);
			_builder.Append(":");
			_builder.Append(GorillaComputer.instance.buildCode);
			_builder.AppendLine(_spoofIds ? " <color=\"red\">*Spoofing IDs*</color>" : string.Empty);
			GorillaTagger instance = GorillaTagger.Instance;
			num = Mathf.Min(num, 90);
			_builder.Append((num < FPS_THRESHOLD) ? "<color=\"red\">" : "<color=\"white\">");
			_builder.Append(num);
			_builder.Append(" fps / ");
			_builder.Append(FPS_THRESHOLD + 1);
			_builder.Append(" fps</color> ");
			GorillaTagger.FramerateHealthLevel framerateHealth = instance.FramerateHealth;
			_builder.Append("sfps: ");
			_builder.Append(instance.SmoothedFramerate);
			_builder.Append(" (H: <color=");
			_builder.Append(ColorFromFramerateHealth(framerateHealth));
			_builder.Append('>');
			_builder.Append(FramerateHealthName(framerateHealth));
			_builder.AppendLine("</color>)");
			UniversalRenderPipelineAsset universalRenderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
			_builder.Append("draw calls: ");
			_builder.Append(_drawCallsRecorder.LastValue);
			_builder.Append(" tris: ");
			_builder.Append(_trisRecorder.LastValue);
			_builder.Append(" rs: ");
			_builder.Append(XRSettings.eyeTextureResolutionScale);
			_builder.Append('/');
			_builder.Append(XRSettings.renderViewportScale);
			_builder.Append('/');
			_builder.Append((universalRenderPipelineAsset != null) ? universalRenderPipelineAsset.renderScale : 1f);
			_builder.AppendLine(" ");
			_builder.Append("Memory: ");
			_builder.Append(Profiler.GetMonoUsedSizeLong() / 1048576);
			_builder.AppendLine("M");
			if (GorillaComputer.instance != null)
			{
				DateTime serverTime = GorillaComputer.instance.GetServerTime();
				_builder.Append((serverTime.Year > 2020) ? "<color=#00FFAA>" : "<color=#FF3333>");
				_builder.Append(serverTime.ToString());
				_builder.AppendLine("</color>");
			}
			else
			{
				_builder.AppendLine("<color=#FF3333>Server Time Unavailable</color>");
			}
			ZoneDef currentNode = instance.offlineVRRig.zoneEntity.currentNode;
			if (currentNode != null && (currentNode != _lastZone || currentNode.zoneId != _lastZoneId || currentNode.subZoneId != _lastSubZoneId))
			{
				_lastZone = currentNode;
				_lastZoneId = currentNode.zoneId;
				_lastSubZoneId = currentNode.subZoneId;
				_zones = $"{currentNode.gameObject.name.ToUpperInvariant()}/{currentNode.zoneId}/{currentNode.subZoneId}";
			}
			if (NetworkSystem.Instance.IsMasterClient)
			{
				_builder.Append("H");
			}
			if (NetworkSystem.Instance.InRoom)
			{
				if (NetworkSystem.Instance.SessionIsPrivate)
				{
					_builder.Append("Pri ");
				}
				else
				{
					_builder.Append("Pub ");
				}
			}
			else
			{
				_builder.Append("DC ");
			}
			_builder.Append("z: <color=\"green\">");
			_builder.Append(_zones);
			_builder.AppendLine("</color>");
			if (NetworkSystem.Instance.InRoom)
			{
				GorillaGameManager instance2 = GorillaGameManager.instance;
				if (instance2 != null)
				{
					GorillaTagCompetitiveManager gorillaTagCompetitiveManager = instance2 as GorillaTagCompetitiveManager;
					if (gorillaTagCompetitiveManager != null)
					{
						RankedMultiplayerScore scoring = gorillaTagCompetitiveManager.GetScoring();
						_builder.Append("Ranked Mode ELO: ");
						_builder.Append(scoring.Progression.GetEloScore());
						_builder.Append("  Tier: ");
						_builder.AppendLine(scoring.Progression.GetRankedProgressionTierName());
						RankedMultiplayerScore.PlayerScoreInRound inGameScoreForSelf = scoring.GetInGameScoreForSelf();
						_builder.Append("Tags: ");
						_builder.Append(inGameScoreForSelf.NumTags);
						_builder.Append("  Defense: ");
						_builder.Append(Mathf.RoundToInt(inGameScoreForSelf.PointsOnDefense));
						_builder.Append("  Score: ");
						_builder.Append(Mathf.RoundToInt(scoring.ComputeGameScore(inGameScoreForSelf.NumTags, inGameScoreForSelf.PointsOnDefense)));
						_builder.AppendLine();
						if (gorillaTagCompetitiveManager.ShowDebugPing)
						{
							_builder.AppendLine("Server MatchID Ping!");
						}
					}
				}
			}
			switch (_currentState)
			{
			case State.ShowStats:
			{
				_builder.AppendLine("\nStats:\n");
				Vector3 averagedVelocity = GTPlayer.Instance.AveragedVelocity;
				Vector3 headCenterPosition = GTPlayer.Instance.HeadCenterPosition;
				float magnitude = averagedVelocity.magnitude;
				_groundVelocity = averagedVelocity;
				_groundVelocity.y = 0f;
				_builder.Append("v: ");
				_builder.Append(magnitude.ToString("F1"));
				_builder.Append(" m/s\t\todo: ");
				_builder.Append(_distanceMoved.ToString("F2"));
				_builder.Append("m\tswam: ");
				_builder.Append(_distanceSwam.ToString("F2"));
				_builder.AppendLine("m");
				_builder.Append("ground: ");
				_builder.Append(_groundVelocity.magnitude.ToString("F1"));
				_builder.Append(" m/s\thead: ");
				_builder.AppendLine(headCenterPosition.ToString("F2"));
				break;
			}
			case State.TimeAdjust:
			{
				_builder.AppendLine("\nAdjust Time\n");
				_builder.AppendLine("Press [A] to advance one hour [+ R Grip to go back one hour]");
				_builder.AppendLine("Press [B] to advance five minutes [+ R Grip to go back one minute]");
				_builder.AppendLine("Press [R] Trigger to advance one day [+ R Grip to go back one day]");
				BetterDayNightManager instance3 = BetterDayNightManager.instance;
				_builder.Append("\nAdjust Environment ");
				_builder.Append(instance3.currentTimeIndex + 1);
				_builder.Append('/');
				_builder.Append(instance3.timeOfDayRange.Length);
				_builder.Append(" : ");
				_builder.Append(instance3.CurrentWeather().ToString());
				_builder.AppendLine(" \n");
				_builder.AppendLine("[L STICK L/R] to change Time Of Day. [L STICK U/D] to change Weather.");
				break;
			}
			case State.RecordingMode:
				_builder.AppendLine("\nMo-Cap Recording:\n");
				break;
			case State.ShowRBs:
				_builder.AppendLine("\nRigid Body Locator\n");
				break;
			}
			text.SetText(_builder);
		}
		_updateTimer = 0f;
		if (_buttonDown && _currentState != State.RecordingMode)
		{
			_btnDownTime += Time.deltaTime / delayUpdateRate;
			if (_btnDownTime >= 15f)
			{
				base.gameObject.SetActive(value: false);
			}
			dismiss.SetText("let go of that button in the next {0:1} seconds or the debug hud will vanish forever", 15f - _btnDownTime);
		}
		SetActiveIfChanged(dismiss.gameObject, _buttonDown && _currentState != State.RecordingMode && _btnDownTime > 5f);
	}

	private static void SetActiveIfChanged(GameObject go, bool active)
	{
		if (go.activeSelf != active)
		{
			go.SetActive(active);
		}
	}

	private void ChangeTOD(int v)
	{
		int num = (BetterDayNightManager.instance.currentTimeIndex + BetterDayNightManager.instance.timeOfDayRange.Length + v) % BetterDayNightManager.instance.timeOfDayRange.Length;
		BetterDayNightManager.instance.SetTimeOfDay(num);
		BetterDayNightManager.instance.SetOverrideIndex(num);
		BetterDayNightManager.instance.SetFixedWeather((BetterDayNightManager.WeatherType)_fixedWeathers.GetValue(_fixedWeatherIndex));
	}

	private void ChangeWeather(int v)
	{
		_fixedWeatherIndex = (_fixedWeatherIndex + _fixedWeathers.Length + v) % _fixedWeathers.Length;
		BetterDayNightManager.instance.SetFixedWeather((BetterDayNightManager.WeatherType)_fixedWeathers.GetValue(_fixedWeatherIndex));
	}

	private void NextState(bool fwd)
	{
		PlayerGameEvents.OnPlayerMoved -= OnPlayerMoved;
		PlayerGameEvents.OnPlayerSwam -= OnPlayerSwam;
		logging.gameObject.SetActive(value: false);
		logging.pageToDisplay = 1;
		if (_currentState == State.TimeAdjust)
		{
			BetterDayNightManager.instance.ClearFixedWeather();
		}
		switch (_currentState)
		{
		case State.Inactive:
			_currentState = (fwd ? State.Active : State.TimeAdjust);
			break;
		case State.Active:
			_currentState = (fwd ? State.ShowLog : State.Inactive);
			break;
		case State.ShowLog:
			_currentState = ((!fwd) ? State.Active : State.ShowError);
			break;
		case State.ShowError:
			_currentState = (fwd ? State.ShowStats : State.ShowLog);
			break;
		case State.ShowStats:
			_currentState = (fwd ? State.ShowRBs : State.ShowError);
			break;
		case State.ShowRBs:
			_currentState = (fwd ? State.TitleDataMonitor : State.ShowStats);
			break;
		case State.TitleDataMonitor:
			_currentState = (fwd ? State.TimeAdjust : State.ShowRBs);
			break;
		case State.TimeAdjust:
			_currentState = ((!fwd) ? State.TitleDataMonitor : State.Inactive);
			break;
		case State.RecordingMode:
			_currentState = ((!fwd) ? State.TimeAdjust : State.Inactive);
			break;
		}
		if (_currentState == State.TimeAdjust)
		{
			BetterDayNightManager.instance.SetFixedWeather((BetterDayNightManager.WeatherType)_fixedWeathers.GetValue(_fixedWeatherIndex));
		}
		UpdateLog();
	}

	private void DisplayLog(List<string> log)
	{
		logging.gameObject.SetActive(value: true);
		_logBuilder.Clear();
		for (int num = log.Count - 1; num >= 0; num--)
		{
			_logBuilder.Append(log[num]);
			_logBuilder.Append('\n');
		}
		logging.SetText(_logBuilder);
		UpdateLogTitle();
	}

	private async void UpdateLogTitle()
	{
		await Task.Yield();
		_logTitleBuilder.Clear();
		_logTitleBuilder.Append(LogTitleFromState(_currentState));
		_logTitleBuilder.Append(" <<[B] turn page [A]>> (");
		_logTitleBuilder.Append(logging.pageToDisplay);
		_logTitleBuilder.Append('/');
		_logTitleBuilder.Append(logging.textInfo.pageCount);
		_logTitleBuilder.Append(')');
		logPage.SetText(_logTitleBuilder);
	}

	private string LogTitleFromState(State s)
	{
		return s switch
		{
			State.ShowLog => "Debug Log", 
			State.ShowError => "Error Log", 
			State.TitleDataMonitor => "Title Data Log", 
			_ => string.Empty, 
		};
	}

	private string ColorFromState(State s)
	{
		return s switch
		{
			State.ShowStats => "\"green\"", 
			State.ShowLog => "\"yellow\"", 
			State.ShowError => "\"orange\"", 
			State.ShowRBs => "\"red\"", 
			State.RecordingMode => "\"purple\"", 
			State.TitleDataMonitor => "#00ffff", 
			_ => "#ffffff", 
		};
	}

	private static string ColorFromFramerateHealth(GorillaTagger.FramerateHealthLevel health)
	{
		return health switch
		{
			GorillaTagger.FramerateHealthLevel.Good => "\"green\"", 
			GorillaTagger.FramerateHealthLevel.Fair => "\"yellow\"", 
			GorillaTagger.FramerateHealthLevel.Weak => "#ff8080", 
			GorillaTagger.FramerateHealthLevel.Poor => "#ff0000", 
			_ => "#ffffff", 
		};
	}

	private static string FramerateHealthName(GorillaTagger.FramerateHealthLevel health)
	{
		return health switch
		{
			GorillaTagger.FramerateHealthLevel.Good => "Good", 
			GorillaTagger.FramerateHealthLevel.Fair => "Fair", 
			GorillaTagger.FramerateHealthLevel.Weak => "Weak", 
			GorillaTagger.FramerateHealthLevel.Poor => "Poor", 
			_ => "Unknown", 
		};
	}

	private void OnPlayerSwam(float distance, float speed)
	{
		if (distance > 0.005f)
		{
			_distanceSwam += distance;
		}
	}

	private void OnPlayerMoved(float distance, float speed)
	{
		if (distance > 0.005f)
		{
			_distanceMoved += distance;
		}
	}

	private void OnEnable()
	{
		Application.logMessageReceived += LogMessageReceived;
		PlayFabTitleDataCache.OnValueRetieved = (Action<string, string>)Delegate.Combine(PlayFabTitleDataCache.OnValueRetieved, new Action<string, string>(TDValueRetrieved));
		PlayFabTitleDataCache.OnCachedValueRetieved = (Action<string, string>)Delegate.Combine(PlayFabTitleDataCache.OnCachedValueRetieved, new Action<string, string>(TDCachedValueRetrieved));
	}

	private void TDValueRetrieved(string arg1, string arg2)
	{
		_logTD.Add($" >{Time.realtimeSinceStartup:F2}> TitleData[ <color=#ffaaff>{arg1}</color> ] = {arg2}");
		if (_logTD.Count > 1000)
		{
			_logTD.RemoveAt(0);
		}
		UpdateLog();
	}

	private void TDCachedValueRetrieved(string arg1, string arg2)
	{
		_logTD.Add($" >{Time.realtimeSinceStartup:F2}> TitleData[ <color=#00ffff>{arg1}</color> ] = {arg2}");
		if (_logTD.Count > 1000)
		{
			_logTD.RemoveAt(0);
		}
		UpdateLog();
	}

	private void OnDisable()
	{
		PlayFabTitleDataCache.OnValueRetieved = (Action<string, string>)Delegate.Remove(PlayFabTitleDataCache.OnValueRetieved, new Action<string, string>(TDValueRetrieved));
		PlayFabTitleDataCache.OnCachedValueRetieved = (Action<string, string>)Delegate.Remove(PlayFabTitleDataCache.OnCachedValueRetieved, new Action<string, string>(TDCachedValueRetrieved));
		Application.logMessageReceived -= LogMessageReceived;
	}

	private void LogMessageReceived(string condition, string stackTrace, LogType type)
	{
		string text = $" >{Time.realtimeSinceStartup:F2}> {GetColorStringFromLogType(type)}{condition}</color>";
		if (_pLog != condition)
		{
			_logMessage.Add(text);
		}
		else
		{
			_logMessage[_logMessage.Count - 1] = text;
		}
		_pLog = condition;
		if (_logMessage.Count > 100)
		{
			_logMessage.RemoveAt(0);
		}
		if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
		{
			_logError.Add(text + "\n" + stackTrace);
			if (_logError.Count > 100)
			{
				_logError.RemoveAt(0);
			}
		}
		UpdateLog();
	}

	private void UpdateLog()
	{
		_logDirty = true;
	}

	private void RefreshLog()
	{
		switch (_currentState)
		{
		case State.ShowLog:
			DisplayLog(_logMessage);
			break;
		case State.ShowError:
			DisplayLog(_logError);
			break;
		case State.TitleDataMonitor:
			DisplayLog(_logTD);
			break;
		}
	}

	private static string GetColorStringFromLogType(LogType type)
	{
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			return "<color=\"red\">";
		case LogType.Warning:
			return "<color=\"yellow\">";
		default:
			return "<color=\"white\">";
		}
	}

	private void OnZoneChanged(ZoneData[] zoneData)
	{
		_zones = string.Empty;
		for (int i = 0; i < zoneData.Length; i++)
		{
			if (zoneData[i].active)
			{
				_zones = _zones + zoneData[i].zone.ToString().ToUpper() + "; ";
			}
		}
	}
}
