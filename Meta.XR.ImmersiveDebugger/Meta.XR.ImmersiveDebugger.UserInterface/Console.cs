using System;
using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

[DefaultExecutionOrder(1)]
public class Console : DebugPanel
{
	private const int NumberOfLines = 14;

	private const int FullLogPanelBottomMargin = 40;

	private const int ContractedLogPanelBottomMargin = 140;

	private ScrollView _scrollView;

	private ScrollView _scrollViewLogDetails;

	private ProxyFlex<ConsoleLine, ProxyConsoleLine> _proxyFlex;

	private Flex _flex;

	private Flex _buttonsAnchor;

	private List<SeverityEntry> _severities = new List<SeverityEntry>();

	private Dictionary<LogType, SeverityEntry> _severitiesPerType = new Dictionary<LogType, SeverityEntry>();

	private readonly List<LogEntry> _entries = new List<LogEntry>();

	private readonly List<LogEntry> _allEntries = new List<LogEntry>();

	private readonly Dictionary<int, LogEntry> _entryMap = new Dictionary<int, LogEntry>();

	private Label _logDetailLabel;

	private Toggle _collapseBtn;

	private Texture2D _collapseActiveIcon;

	private Texture2D _collapseInactiveIcon;

	private ButtonWithIcon _logDetailPaneCloseBtn;

	private Vector3 _currentPosition;

	private Vector3 _targetPosition;

	private readonly float _lerpSpeed = 10f;

	private bool _lerpCompleted = true;

	private Background _logDetailPaneBackground;

	private ImageStyle _logDetailPaneBackgroundImageStyle;

	internal bool Dirty { get; set; }

	internal bool LogCollapseMode { get; private set; }

	internal int MaximumNumberOfLogEntries { get; private set; }

	public ImageStyle LogDetailBackgroundStyle
	{
		set
		{
			_logDetailPaneBackground.Sprite = value.sprite;
			_logDetailPaneBackground.Color = value.color;
			_logDetailPaneBackground.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	private SeverityEntry GetSeverity(LogType logType)
	{
		if (!_severitiesPerType.TryGetValue(logType, out var value))
		{
			return null;
		}
		return value;
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_flex = Append<Flex>("main");
		_flex.LayoutStyle = Style.Load<LayoutStyle>("ConsoleFlex");
		_buttonsAnchor = _flex.Append<Flex>("buttons");
		_buttonsAnchor.LayoutStyle = Style.Load<LayoutStyle>("ConsoleButtons");
		LogCollapseMode = RuntimeSettings.Instance.CollapsedIdenticalLogEntries;
		_collapseActiveIcon = Resources.Load<Texture2D>("Textures/compress_icon");
		_collapseInactiveIcon = Resources.Load<Texture2D>("Textures/expand_icon");
		_collapseBtn = RegisterControl("LogCollapse", LogCollapseMode ? _collapseInactiveIcon : _collapseActiveIcon, Style.Load<ImageStyle>("LogCollapseIcon"), ToggleCollapseMode);
		_collapseBtn.State = LogCollapseMode;
		RegisterControl("Clear", Resources.Load<Texture2D>("Textures/bin_icon"), Style.Load<ImageStyle>("BinIcon"), Clear);
		SeverityEntry severityEntry = new SeverityEntry(this, "Error", Resources.Load<Texture2D>("Textures/error_icon"), Style.Load<ImageStyle>("ErrorIcon"), Style.Load<ImageStyle>("PillError"));
		SeverityEntry severityEntry2 = new SeverityEntry(this, "Warning", Resources.Load<Texture2D>("Textures/warning_icon"), Style.Load<ImageStyle>("WarningIcon"), Style.Load<ImageStyle>("PillWarning"));
		SeverityEntry severityEntry3 = new SeverityEntry(this, "Log", Resources.Load<Texture2D>("Textures/notice_icon"), Style.Load<ImageStyle>("NoticeIcon"), Style.Load<ImageStyle>("PillInfo"));
		_severities.Add(severityEntry3);
		_severities.Add(severityEntry2);
		_severities.Add(severityEntry);
		_severitiesPerType.Add(LogType.Assert, severityEntry);
		_severitiesPerType.Add(LogType.Error, severityEntry);
		_severitiesPerType.Add(LogType.Exception, severityEntry);
		_severitiesPerType.Add(LogType.Warning, severityEntry2);
		_severitiesPerType.Add(LogType.Log, severityEntry3);
		RuntimeSettings instance = RuntimeSettings.Instance;
		severityEntry.ShouldShow = instance.ShowErrorLog;
		severityEntry2.ShouldShow = instance.ShowWarningLog;
		severityEntry3.ShouldShow = instance.ShowInfoLog;
		_scrollView = Append<ScrollView>("logs");
		_scrollView.LayoutStyle = Style.Instantiate<LayoutStyle>("LogsScrollView");
		_scrollView.Flex.LayoutStyle = Style.Load<LayoutStyle>("ConsoleLogs");
		MaximumNumberOfLogEntries = instance.MaximumNumberOfLogEntries;
		_proxyFlex = new ProxyFlex<ConsoleLine, ProxyConsoleLine>(14, MaximumNumberOfLogEntries, Style.Load<LayoutStyle>("ConsoleLine"), _scrollView);
		_logDetailPaneBackground = Append<Background>("background");
		_logDetailPaneBackground.LayoutStyle = Style.Load<LayoutStyle>("LogDetailsPaneBackground");
		_logDetailPaneBackgroundImageStyle = Style.Load<ImageStyle>("LogDetailPaneBackground");
		LogDetailBackgroundStyle = _logDetailPaneBackgroundImageStyle;
		_scrollViewLogDetails = Append<ScrollView>("details");
		_scrollViewLogDetails.LayoutStyle = Style.Load<LayoutStyle>("LogDetailsScrollView");
		_scrollViewLogDetails.Flex.LayoutStyle = Style.Load<LayoutStyle>("ConsoleLogDetails");
		_logDetailLabel = _scrollViewLogDetails.Flex.Append<Label>("entry");
		_logDetailLabel.LayoutStyle = Style.Instantiate<LayoutStyle>("ConsoleLineLogDetailsLabel");
		_logDetailLabel.TextStyle = Style.Load<TextStyle>("ConsoleLogDetailsLabel");
		_logDetailLabel.Text.horizontalOverflow = HorizontalWrapMode.Wrap;
		_logDetailPaneCloseBtn = Append<ButtonWithIcon>("close");
		_logDetailPaneCloseBtn.LayoutStyle = Style.Load<LayoutStyle>("LogDetailPaneCloseButton");
		_logDetailPaneCloseBtn.Icon = Resources.Load<Texture2D>("Textures/close_icon");
		_logDetailPaneCloseBtn.IconStyle = Style.Load<ImageStyle>("LogDetailPaneCloseButton");
		_logDetailPaneCloseBtn.Callback = HideLogDetailsPanel;
		HideLogDetailsPanel();
		LogCollapseMode = RuntimeSettings.Instance.CollapsedIdenticalLogEntries;
		LogEntry.OnDisplayDetails = OnConsoleLineClicked;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ConsoleLogsCache.OnLogReceived = (Action<string, string, LogType>)Delegate.Remove(ConsoleLogsCache.OnLogReceived, new Action<string, string, LogType>(EnqueueLogEntry));
		ConsoleLogsCache.OnLogReceived = (Action<string, string, LogType>)Delegate.Combine(ConsoleLogsCache.OnLogReceived, new Action<string, string, LogType>(EnqueueLogEntry));
		ConsoleLogsCache.ConsumeStartupLogs(EnqueueLogEntry);
	}

	protected override void OnDisable()
	{
		ConsoleLogsCache.OnLogReceived = (Action<string, string, LogType>)Delegate.Remove(ConsoleLogsCache.OnLogReceived, new Action<string, string, LogType>(EnqueueLogEntry));
		base.OnDisable();
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_logDetailPaneBackground.Color = (base.Transparent ? _logDetailPaneBackgroundImageStyle.colorOff : _logDetailPaneBackgroundImageStyle.color);
	}

	internal Label RegisterCount()
	{
		Label label = _buttonsAnchor.Append<Label>("");
		label.LayoutStyle = Style.Load<LayoutStyle>("ConsoleButtonCount");
		label.TextStyle = Style.Load<TextStyle>("ConsoleButtonCount");
		return label;
	}

	internal Toggle RegisterControl(string buttonName, Texture2D icon, ImageStyle style, Action callback)
	{
		if (buttonName == null)
		{
			throw new ArgumentNullException("buttonName");
		}
		if (icon == null)
		{
			throw new ArgumentNullException("icon");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		Toggle toggle = _buttonsAnchor.Append<Toggle>(buttonName);
		toggle.LayoutStyle = Style.Load<LayoutStyle>("ConsoleButton");
		toggle.Icon = icon;
		toggle.IconStyle = (style ? style : Style.Default<ImageStyle>());
		toggle.Callback = callback;
		return toggle;
	}

	private void ToggleCollapseMode()
	{
		LogCollapseMode = !LogCollapseMode;
		_collapseBtn.Icon = (LogCollapseMode ? _collapseInactiveIcon : _collapseActiveIcon);
		if (LogCollapseMode)
		{
			MergeEntries();
		}
		else
		{
			FlattenEntries();
		}
	}

	private void EnqueueLogEntry(string logString, string stackTrace, LogType type)
	{
		SeverityEntry severity = GetSeverity(type);
		if (severity == null)
		{
			return;
		}
		int key = ComputeLogHash(logString, stackTrace);
		if (_entryMap.TryGetValue(key, out var value) && LogCollapseMode)
		{
			_entries.Remove(value);
			_proxyFlex.RemoveProxy(value.Line);
			value.Count++;
		}
		else
		{
			if (_entries.Count >= MaximumNumberOfLogEntries)
			{
				RemoveLogEntry(_entries[0]);
			}
			value = OVRObjectPool.Get<LogEntry>();
			value.Setup(logString, stackTrace, severity);
			_entryMap[key] = value;
		}
		_entries.Add(value);
		LogEntry logEntry = OVRObjectPool.Get<LogEntry>();
		logEntry.Setup(logString, stackTrace, severity);
		_allEntries.Add(logEntry);
		severity.Count++;
		AppendToProxyFlex(value);
	}

	private void RemoveLogEntry(LogEntry logEntry)
	{
		logEntry.Severity.Count -= logEntry.Count;
		_entries.Remove(logEntry);
		_allEntries.RemoveAll(delegate(LogEntry entry)
		{
			bool num = entry == logEntry;
			if (num)
			{
				OVRObjectPool.Return(entry);
			}
			return num;
		});
		OVRObjectPool.Return(logEntry);
	}

	private void Update()
	{
		if (Dirty)
		{
			RefreshAllEntries();
			Dirty = false;
		}
		_proxyFlex.Update();
		if (!_lerpCompleted)
		{
			_currentPosition = Utils.LerpPosition(_currentPosition, _targetPosition, _lerpSpeed);
			_lerpCompleted = _currentPosition == _targetPosition;
			base.SphericalCoordinates = _currentPosition;
		}
	}

	private void Clear()
	{
		_entries.Clear();
		foreach (LogEntry allEntry in _allEntries)
		{
			OVRObjectPool.Return(allEntry);
		}
		_allEntries.Clear();
		_entryMap.Clear();
		_proxyFlex.Clear();
		foreach (SeverityEntry severity in _severities)
		{
			severity.Reset();
		}
		HideLogDetailsPanel();
		Dirty = true;
	}

	private void RefreshAllEntries()
	{
		foreach (LogEntry entry in _entries)
		{
			if (!entry.Severity.ShouldShow)
			{
				if (entry.Shown)
				{
					_proxyFlex.RemoveProxy(entry.Line);
					entry.Line = null;
				}
			}
			else if (!entry.Shown)
			{
				ProxyConsoleLine proxyConsoleLine = _proxyFlex.AppendProxy();
				proxyConsoleLine.Entry = entry;
				entry.Line = proxyConsoleLine;
			}
		}
	}

	private void MergeEntries()
	{
		_entries.Clear();
		_proxyFlex.Clear();
		ResetLogCount();
		foreach (LogEntry allEntry in _allEntries)
		{
			int key = ComputeLogHash(allEntry.Label, allEntry.Callstack);
			if (_entryMap.TryGetValue(key, out var value))
			{
				_entries.Remove(value);
				_proxyFlex.RemoveProxy(value.Line);
				value.Count++;
			}
			_entries.Add(value);
			AppendToProxyFlex(value);
		}
		Dirty = true;
	}

	private void ResetLogCount()
	{
		foreach (LogEntry allEntry in _allEntries)
		{
			allEntry.Count = 0;
		}
	}

	private void FlattenEntries()
	{
		_entries.Clear();
		_proxyFlex.Clear();
		foreach (LogEntry allEntry in _allEntries)
		{
			_entries.Add(allEntry);
			AppendToProxyFlex(allEntry);
		}
		Dirty = true;
	}

	private void AppendToProxyFlex(LogEntry entry)
	{
		if (entry.Severity.ShouldShow)
		{
			ProxyConsoleLine proxyConsoleLine = _proxyFlex.AppendProxy();
			proxyConsoleLine.Entry = entry;
			entry.Line = proxyConsoleLine;
		}
	}

	private void OnConsoleLineClicked(LogEntry entry)
	{
		ShowLogDetailsPanel();
		_logDetailLabel.Content = entry.Label + "\n" + entry.Callstack;
		_logDetailLabel.SetHeight(_logDetailLabel.Text.preferredHeight + 20f);
		_logDetailLabel.RefreshLayout();
		_scrollViewLogDetails.Progress = 1f;
	}

	private void ShowLogDetailsPanel()
	{
		if (!_scrollViewLogDetails.Visibility)
		{
			_scrollViewLogDetails.Show();
			_logDetailPaneCloseBtn.Show();
			_logDetailPaneBackground.Show();
			_scrollView.LayoutStyle.bottomRightMargin.y = 140f;
			_scrollView.RefreshLayout();
		}
	}

	private void HideLogDetailsPanel()
	{
		if (_scrollViewLogDetails.Visibility)
		{
			_scrollViewLogDetails.Hide();
			_logDetailPaneCloseBtn.Hide();
			_logDetailPaneBackground.Hide();
			_scrollView.LayoutStyle.bottomRightMargin.y = 40f;
			_scrollView.RefreshLayout();
		}
	}

	private static int ComputeLogHash(string content, string stackTrace)
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(content.GetHashCode());
		hashCode.Add(stackTrace.GetHashCode());
		return hashCode.ToHashCode();
	}

	internal void SetPanelPosition(RuntimeSettings.DistanceOption distanceOption, bool skipAnimation = false)
	{
		ValueContainer<Vector3> valueContainer = ValueContainer<Vector3>.Load("ConsolePanelPositions");
		_targetPosition = distanceOption switch
		{
			RuntimeSettings.DistanceOption.Close => valueContainer["Close"], 
			RuntimeSettings.DistanceOption.Far => valueContainer["Far"], 
			_ => valueContainer["Default"], 
		};
		if (skipAnimation)
		{
			base.SphericalCoordinates = _targetPosition;
			_currentPosition = _targetPosition;
		}
		else
		{
			_lerpCompleted = false;
		}
	}
}
