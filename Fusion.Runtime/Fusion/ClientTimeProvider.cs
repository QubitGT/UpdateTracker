#define ENABLE_PROFILER
#define DEBUG
#define TRACE
using System;
using System.Collections.Generic;
using Fusion.Statistics;
using UnityEngine;

namespace Fusion;

internal class ClientTimeProvider : ITimeProvider
{
	private ClientTimeProviderSettings _settings;

	private IFeedbackController _clockFeedback;

	private double _latestInputOffset;

	private double _targetInputOffset;

	private double _clockTimeScaleOffset;

	private IFeedbackController _delayFeedback;

	private double _latestInputDelay;

	private double _targetInputDelay;

	private double _delayTimeScaleOffset;

	private IFeedbackController _interpFeedback;

	private double _latestInterpDelay;

	private double _targetInterpDelay;

	private double _interpTimeScaleOffset;

	private double _inputTime;

	private double _simulationTime;

	private double _interpTime;

	private TimeSeries _roundTripTime;

	private TimeSeries _inputOffset;

	private TimeSeries _interpDelay;

	private Histogram _frameTimeDeltaHist;

	private ExponentialDecay _frameTimeDeltaHistDecay;

	private bool _frameTimeDeltaChecked;

	private bool _frameTimeDeltaOutliersSeen;

	private bool _latestSnapshotIsOutlier;

	private Histogram _snapshotTimeDeltaHist;

	private ExponentialDecay _snapshotTimeDeltaHistDecay;

	private Timer _snapshotTimer;

	private TimeSeries _snapshotTimeDelta;

	private Tick _snapshot;

	private Timer _clockSyncTimer;

	private double _lastSeenServerTime;

	private double _lastSeenServerTimeScale;

	private Timer _sampleTimer;

	private Timer _resetInputTimer;

	private Timer _resetSimulationTimer;

	private Timer _resetInterpTimer;

	private int _inputTimeResetCount;

	private int _simulationTimeResetCount;

	private int _interpTimeResetCount;

	private bool _inputTimeReset;

	private bool _simulationTimeReset;

	private bool _interpTimeReset;

	private readonly List<TimeProviderCallback> _resetInputTimeCallbacks;

	private readonly List<TimeProviderCallback> _resetSimulationTimeCallbacks;

	private readonly List<TimeProviderCallback> _resetInterpTimeCallbacks;

	private bool _isRunning;

	private int _playerIndex;

	private int _snapshotExceededFrames;

	private double _snapshotExceededTime;

	private Simulation.TimeFeedback _serverFeedback;

	private RingBuffer<TimeAdjustment> _inputOffsetAdjust;

	private Tick _lastInputOffsetAdjustTick;

	private double _totalInputOffsetAdjust;

	private bool _trace;

	private ClientTimeTrace _timeTrace;

	private const double RttSmoothingFactor = 0.25;

	private const float FeedbackSmoothingFactor = 0.5f;

	private const double InputOffsetSmoothingFactor = 0.5;

	private const double InputDelaySmoothingFactor = 0.5;

	private const double InterpDelaySmoothingFactor = 0.5;

	private const double InterpDelayTempSmoothingFactor = 0.125;

	private const double TargetInputOffsetSmoothingFactor = 0.2;

	private const double TargetInputDelaySmoothingFactor = 0.2;

	private const double TargetInterpDelaySmoothingFactor = 0.2;

	private const double SnapshotTimeDeltaHistogramDecayFraction = 0.5;

	private const double SnapshotTimeDeltaHistogramDecayTime = 2.0;

	private const double SnapshotTimeDeltaHighQuantile = 0.95;

	private const double FrameTimeDeltaHistogramDecayFraction = 0.5;

	private const double FrameTimeDeltaHistogramDecayTime = 2.0;

	private const double FrameTimeDeltaHighQuantile = 0.95;

	private const double FrameTimeDeltaOutlierTest1Quantile = 0.95;

	private const double FrameTimeDeltaOutlierTest1Threshold = 0.1;

	private const double FrameTimeDeltaOutlierTest2Threshold = 0.25;

	private const double InitialServerFeedbackJitter = 0.025;

	private const double PositiveBumpThreshold = 0.05;

	private const double BumpFraction = 0.125;

	private const double NegativeResetThreshold = -0.5;

	private const double PositiveResetThreshold = 1.0;

	private const double ResetCooldownSeconds = 1.0;

	private const double Kp = 0.3;

	private const double Ki = 0.0;

	private const double Kd = 0.0;

	internal ClientTimeProvider()
		: this(ClientTimeProviderSettings.Default())
	{
	}

	internal ClientTimeProvider(ClientTimeProviderSettings settings)
	{
		_settings = settings;
		_resetInputTimeCallbacks = new List<TimeProviderCallback>();
		_resetSimulationTimeCallbacks = new List<TimeProviderCallback>();
		_resetInterpTimeCallbacks = new List<TimeProviderCallback>();
		Initialize();
	}

	internal void OnReset(Clock clock, TimeProviderCallback callback)
	{
		switch (clock)
		{
		case Clock.Input:
			_resetInputTimeCallbacks.Add(callback);
			break;
		case Clock.Local:
			_resetSimulationTimeCallbacks.Add(callback);
			break;
		case Clock.Remote:
			_resetInterpTimeCallbacks.Add(callback);
			break;
		}
	}

	private void Configure(TimeSyncConfiguration tsc)
	{
		_settings.SampleWindowSeconds = tsc.SampleWindowSecondsNormalized;
		_settings.TimeScaleOffsetMax = tsc.MaxSimSpeedAdjustNormalized;
		_settings.OutgoingQuantile = 1.0 - tsc.MaxLateInputsNormalized;
		_settings.IncomingQuantile = 1.0 - tsc.MaxLateSnapshotsNormalized;
		_settings.OutgoingRedundancy = tsc.RedundantInputsNormalized;
		_settings.IncomingRedundancy = tsc.RedundantSnapshotsNormalized;
		Initialize();
	}

	private void Configure(SimulationRuntimeConfig src)
	{
		_settings.OutgoingSendRate = src.TickRate.ClientSend;
		_settings.IncomingSendRate = src.TickRate.ServerSend;
		_settings.OutgoingSendDelta = src.TickRate.ClientSendDelta;
		_settings.IncomingSendDelta = src.TickRate.ServerSendDelta;
		_settings.ClientTickRate = src.TickRate.Client;
		_settings.ClientSimDeltaTime = src.TickRate.ClientTickDelta;
		_settings.ServerTickRate = src.TickRate.Server;
		_settings.ServerSimDeltaTime = src.TickRate.ServerTickDelta;
		_settings.PredictionMax = double.MaxValue;
		_settings.InputDelayMin = 0.0;
		_settings.InputDelayMax = 0.0;
		Initialize();
	}

	private void Initialize()
	{
		double timeScaleOffsetMax = _settings.TimeScaleOffsetMax;
		_clockFeedback = new VariableFeedback(0.3, 0.0, 0.0, 0.0 - timeScaleOffsetMax, timeScaleOffsetMax);
		_delayFeedback = new VariableFeedback(0.3, 0.0, 0.0, 0.0 - timeScaleOffsetMax, timeScaleOffsetMax);
		_interpFeedback = new VariableFeedback(0.3, 0.0, 0.0, 0.0 - timeScaleOffsetMax, timeScaleOffsetMax);
		_clockSyncTimer = default(Timer);
		_snapshotTimer = default(Timer);
		_resetInputTimer = default(Timer);
		_resetSimulationTimer = default(Timer);
		_resetInterpTimer = default(Timer);
		_sampleTimer = default(Timer);
		_roundTripTime = new TimeSeries((int)((double)_settings.IncomingSendRate * _settings.SampleWindowSeconds));
		_inputOffset = new TimeSeries((int)((double)_settings.IncomingSendRate * _settings.SampleWindowSeconds));
		_snapshotTimeDelta = new TimeSeries((int)((double)_settings.IncomingSendRate * _settings.SampleWindowSeconds));
		_interpDelay = new TimeSeries((int)((double)_settings.IncomingSendRate * _settings.SampleWindowSeconds));
		_inputOffsetAdjust = new RingBuffer<TimeAdjustment>(_settings.ClientTickRate);
		_frameTimeDeltaHist = new Histogram();
		_frameTimeDeltaHistDecay = new ExponentialDecay(0.5, 2.0);
		_snapshotTimeDeltaHist = new Histogram();
		_snapshotTimeDeltaHistDecay = new ExponentialDecay(0.5, 2.0);
	}

	private void Reset(double roundTripTime, Tick snapshot, double time, double timeScale)
	{
		if (_trace)
		{
			_timeTrace.OnPacket(snapshot, _snapshotTimer.ElapsedInSeconds, roundTripTime);
		}
		_clockFeedback.Reset();
		_delayFeedback.Reset();
		_interpFeedback.Reset();
		_roundTripTime.Clear();
		_inputOffset.Clear();
		_interpDelay.Clear();
		_interpDelay.Fill(_settings.IncomingSendDelta);
		_snapshotTimeDelta.Clear();
		_snapshotTimeDelta.Fill(_settings.IncomingSendDelta);
		_snapshotTimeDeltaHist.Clear();
		_snapshotTimeDeltaHist.Record(_settings.IncomingSendDelta, _settings.IncomingSendRate);
		_frameTimeDeltaHist.Clear();
		_frameTimeDeltaHist.Record(0.025, 50.0);
		_frameTimeDeltaChecked = false;
		_frameTimeDeltaOutliersSeen = false;
		_clockSyncTimer.Reset();
		_snapshotTimer.Reset();
		_resetInputTimer.Reset();
		_resetSimulationTimer.Reset();
		_resetInterpTimer.Reset();
		_sampleTimer.Restart();
		_inputTimeResetCount = 0;
		_simulationTimeResetCount = 0;
		_interpTimeResetCount = 0;
		_serverFeedback = new Simulation.TimeFeedback(0.0, 0.025, 0.0, 0.025);
		UpdateSnapshot(snapshot);
		UpdateServerStats(roundTripTime, time, timeScale);
		UpdateIncomingTargets(snap: true);
		UpdateOutgoingTargets(snap: true);
		Snap();
		_isRunning = true;
	}

	private void Snap()
	{
		ResetInputTime();
		ResetSimulationTime();
		ResetInterpolationTime();
	}

	private void ResetInputTime()
	{
		_inputOffsetAdjust.Clear();
		_lastInputOffsetAdjustTick = _snapshot;
		_totalInputOffsetAdjust = 0.0;
		_clockFeedback.Reset();
		_clockTimeScaleOffset = 0.0;
		double inputTime = _inputTime;
		double inputTime2 = GetServerTime() + _targetInputOffset;
		_inputTime = inputTime2;
		_latestInputOffset = _targetInputOffset;
		_inputTimeResetCount++;
		_inputTimeReset = true;
		_resetInputTimer.Restart();
		foreach (TimeProviderCallback resetInputTimeCallback in _resetInputTimeCallbacks)
		{
			resetInputTimeCallback();
		}
	}

	private void ResetSimulationTime()
	{
		_delayFeedback.Reset();
		_delayTimeScaleOffset = 0.0;
		double simulationTime = _simulationTime;
		double num = _inputTime - _targetInputDelay;
		InternalLogStreams.LogTraceTime?.Log($"[P{_playerIndex}] (re)setting client local time from {simulationTime:f3} to {num:f3}");
		_simulationTime = num;
		_latestInputDelay = _targetInputDelay;
		_simulationTimeResetCount++;
		_simulationTimeReset = true;
		_resetSimulationTimer.Restart();
		foreach (TimeProviderCallback resetSimulationTimeCallback in _resetSimulationTimeCallbacks)
		{
			resetSimulationTimeCallback();
		}
	}

	private void ResetInterpolationTime(bool resetMayBeCausedByFalseOutlier = false)
	{
		double snapshotTime = GetSnapshotTime();
		double interpTime = _interpTime;
		double num = snapshotTime - _targetInterpDelay;
		if (resetMayBeCausedByFalseOutlier && num < interpTime && interpTime < snapshotTime)
		{
			InternalLogStreams.LogTraceTime?.Log($"[P{_playerIndex}] canceled client remote time reset because time would have jumped backwards from {interpTime:f3} to {num:f3}, even though {interpTime:f3} was still behind the latest snapshot ({snapshotTime:f3})");
			_snapshotTimeDelta.Clear();
			_snapshotTimeDelta.Fill(_settings.IncomingSendDelta);
			_snapshotTimeDeltaHist.Clear();
			_snapshotTimeDeltaHist.Record(_settings.IncomingSendDelta, _settings.IncomingSendRate);
			UpdateIncomingTargets(snap: true);
			return;
		}
		InternalLogStreams.LogTraceTime?.Log($"[P{_playerIndex}] (re)setting client remote time from {interpTime:f3} to {num:f3}");
		_interpFeedback.Reset();
		_interpTimeScaleOffset = 0.0;
		_interpTime = num;
		_latestInterpDelay = _targetInterpDelay;
		_interpTimeResetCount++;
		_interpTimeReset = true;
		_resetInterpTimer.Restart();
		foreach (TimeProviderCallback resetInterpTimeCallback in _resetInterpTimeCallbacks)
		{
			resetInterpTimeCallback();
		}
	}

	private double GetInputOffsetLegacy()
	{
		double inputTime = _inputTime;
		double serverTime = GetServerTime();
		return inputTime - serverTime;
	}

	private double GetInputOffset()
	{
		return (double)_serverFeedback.OffsetAvg + _totalInputOffsetAdjust;
	}

	private void AddInputOffsetAdjustment(double amount)
	{
		Tick tick = (int)(_inputTime * (double)_settings.ClientTickRate);
		if (_inputOffsetAdjust.IsEmpty || tick != _lastInputOffsetAdjustTick)
		{
			_inputOffsetAdjust.PushBack(new TimeAdjustment(tick, 0.0));
		}
		try
		{
			_inputOffsetAdjust.BackMut().Total += amount;
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
			_totalInputOffsetAdjust = 0.0;
			_inputOffsetAdjust = new RingBuffer<TimeAdjustment>(_settings.ClientTickRate);
			_inputOffsetAdjust.PushBack(new TimeAdjustment(tick, amount));
		}
		_totalInputOffsetAdjust += amount;
		_lastInputOffsetAdjustTick = tick;
	}

	private void RemoveInputOffsetAdjustmentsOlderThan(Tick snapshot)
	{
		_totalInputOffsetAdjust = 0.0;
		foreach (TimeAdjustment item in _inputOffsetAdjust)
		{
			if (item.Tick >= snapshot)
			{
				_totalInputOffsetAdjust += item.Total;
			}
		}
	}

	private double GetServerTime()
	{
		double num = _roundTripTime.Smoothed(0.25);
		double num2 = _clockSyncTimer.ElapsedInSeconds + num;
		num2 *= _lastSeenServerTimeScale;
		return _lastSeenServerTime + num2;
	}

	private double GetSnapshotTime()
	{
		double elapsedInSeconds = _snapshotTimer.ElapsedInSeconds;
		elapsedInSeconds *= _lastSeenServerTimeScale;
		return (double)(int)_snapshot * _settings.ClientSimDeltaTime + elapsedInSeconds;
	}

	private void UpdateServerStats(double roundTripTime, double time, double timeScale)
	{
		Assert.Check(roundTripTime >= 0.0);
		Assert.Check(time >= 0.0);
		Assert.Check(timeScale >= 0.0);
		double num = time - _lastSeenServerTime;
		_lastSeenServerTime = time;
		_lastSeenServerTimeScale = timeScale;
		if (!(num <= 0.0))
		{
			_clockSyncTimer.Restart();
			_roundTripTime.Add(roundTripTime);
			double inputOffset = GetInputOffset();
			_inputOffset.Add(inputOffset);
			UpdateOutgoingTargets();
		}
	}

	private void FrameTimeDeltaCheck(double dt)
	{
		if (!_frameTimeDeltaChecked)
		{
			if (FrameTimeDeltaSeemsLikeAnExtremeOutlier(dt))
			{
				_frameTimeDeltaOutliersSeen = true;
			}
			_frameTimeDeltaHist.Rescale(_frameTimeDeltaHistDecay.Calculate(dt));
			_frameTimeDeltaHist.Record(dt);
			_frameTimeDeltaChecked = true;
		}
	}

	private void FrameTimeDeltaCheckReset()
	{
		_frameTimeDeltaChecked = false;
	}

	private bool FrameTimeDeltaSeemsLikeAnExtremeOutlier(double dt)
	{
		if (_frameTimeDeltaHist.Count <= 1.0)
		{
			return false;
		}
		return (dt > 0.1 && dt > _frameTimeDeltaHist.Quantile(0.95)) || dt > 0.25;
	}

	private void UpdateSnapshot(Tick snapshot)
	{
		Assert.Check(snapshot >= 0);
		if (!(_snapshot == snapshot))
		{
			_latestSnapshotIsOutlier = _frameTimeDeltaOutliersSeen;
			if (!_latestSnapshotIsOutlier && _snapshotExceededFrames > 0)
			{
				InternalLogStreams.LogTraceTime?.Log($"[P{_playerIndex}] before receiving snapshot {snapshot}, remote time went past snapshot {_snapshot} for {_snapshotExceededFrames} frame(s) by up to {_snapshotExceededTime:f3} seconds");
			}
			_snapshotExceededFrames = 0;
			_snapshotExceededTime = 0.0;
			RemoveInputOffsetAdjustmentsOlderThan(snapshot);
			_snapshot = snapshot;
			double elapsedInSeconds = _snapshotTimer.ElapsedInSeconds;
			_snapshotTimer.Restart();
			double value = elapsedInSeconds;
			if (_frameTimeDeltaOutliersSeen)
			{
				_frameTimeDeltaOutliersSeen = false;
				value = Math.Min(elapsedInSeconds, _snapshotTimeDeltaHist.Quantile(0.95));
				InternalLogStreams.LogTraceTime?.Log($"[P{_playerIndex}] rejecting sample of {elapsedInSeconds:f3} seconds elapsed since receiving last snapshot because it was influenced by unusual frame lag");
			}
			_snapshotTimeDelta.Add(value);
			_snapshotTimeDeltaHist.Rescale(_snapshotTimeDeltaHistDecay.Calculate(elapsedInSeconds));
			_snapshotTimeDeltaHist.Record(value);
			UpdateIncomingTargets();
		}
	}

	private void SaveInterpDelaySample(double interpDelay)
	{
		if (!(_sampleTimer.ElapsedInSeconds < _settings.IncomingSendDelta) && !_frameTimeDeltaOutliersSeen && !_latestSnapshotIsOutlier)
		{
			_interpDelay.Add(interpDelay);
			_sampleTimer.Restart();
		}
	}

	private double RoundToNearestMultiple(double x, double round, bool minimumOne = false)
	{
		double num = Math.Round(x / round);
		num = ((minimumOne && num < 1.0) ? 1.0 : num);
		return num * round;
	}

	private void UpdateOutgoingTargets(bool snap = false)
	{
		double num = _roundTripTime.Smoothed(0.25);
		double predictionMax = _settings.PredictionMax;
		double num2 = Math.Max(0.0, num - predictionMax - _settings.InputDelayMin);
		double num3 = _settings.InputDelayMin + num2;
		num3 *= _lastSeenServerTimeScale;
		if (snap)
		{
			_targetInputDelay = num3;
		}
		else
		{
			_targetInputDelay = Maths.Lerp(_targetInputDelay, num3, 0.2);
		}
		double num4 = TimeSeries.InverseCdfNormal(_settings.OutgoingQuantile);
		double num5 = RoundToNearestMultiple(_serverFeedback.RecvDeltaAvg, _settings.OutgoingSendDelta, minimumOne: true);
		double num6 = Math.Max(_serverFeedback.RecvDeltaDev, _roundTripTime.MedianAbsDev);
		double num7 = num5 * _settings.OutgoingRedundancy + num4 * num6;
		num7 *= _lastSeenServerTimeScale;
		if (snap)
		{
			_targetInputOffset = num7;
		}
		else
		{
			_targetInputOffset = Maths.Lerp(_targetInputOffset, num7, 0.2);
		}
	}

	private void UpdateIncomingTargets(bool snap = false)
	{
		double num = _snapshotTimeDeltaHist.Quantile(_settings.IncomingQuantile);
		double num2 = (1.0 + _settings.IncomingRedundancy) * num;
		num2 *= _lastSeenServerTimeScale;
		if (snap)
		{
			_targetInterpDelay = num2;
		}
		else
		{
			_targetInterpDelay = Maths.Lerp(_targetInterpDelay, num2, 0.2);
		}
	}

	private void Update(double unscaledDeltaTime)
	{
		if (!_isRunning)
		{
			return;
		}
		if (_trace && _timeTrace != null)
		{
			_timeTrace.OnFrame(unscaledDeltaTime);
		}
		if (unscaledDeltaTime > 1.0)
		{
			InternalLogStreams.LogTraceTime?.Log($"[P{_playerIndex}] time will not advance this frame because its dt={unscaledDeltaTime:f3} is too big");
			FrameTimeDeltaCheckReset();
			return;
		}
		FrameTimeDeltaCheck(unscaledDeltaTime);
		double lastSeenServerTimeScale = _lastSeenServerTimeScale;
		double num = lastSeenServerTimeScale + _clockTimeScaleOffset;
		double num2 = lastSeenServerTimeScale + _clockTimeScaleOffset + _delayTimeScaleOffset;
		double num3 = lastSeenServerTimeScale + _interpTimeScaleOffset;
		_inputTime += Math.Max(0.0, num * unscaledDeltaTime);
		_simulationTime += Math.Max(0.0, num2 * unscaledDeltaTime);
		_interpTime += Math.Max(0.0, num3 * unscaledDeltaTime);
		double inputOffset = GetInputOffset();
		double b = _inputTime - _simulationTime;
		double num4 = GetSnapshotTime() - _interpTime;
		SaveInterpDelaySample(num4);
		_latestInputOffset = Maths.Lerp(_latestInputOffset, inputOffset, 0.5);
		_latestInputDelay = Maths.Lerp(_latestInputDelay, b, 0.5);
		_latestInterpDelay = Maths.Lerp(_latestInterpDelay, num4, _latestSnapshotIsOutlier ? 0.125 : 0.5);
		double num5 = _targetInputOffset - _latestInputOffset;
		double num6 = 0.0 - (_targetInputDelay - _latestInputDelay);
		double num7 = 0.0 - (_targetInterpDelay - _latestInterpDelay);
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		_inputTimeReset = false;
		_simulationTimeReset = false;
		_interpTimeReset = false;
		if (_resetInputTimer.IsRunning && _resetInputTimer.ElapsedInSeconds > 1.0)
		{
			_resetInputTimer.Reset();
			_resetSimulationTimer.Reset();
		}
		if (!_resetInputTimer.IsRunning && (num5 < -0.5 || num5 > 1.0))
		{
			ResetInputTime();
			ResetSimulationTime();
		}
		else if (num5 > 0.05)
		{
			num8 = num5 * 0.125;
		}
		if (_resetSimulationTimer.IsRunning && _resetSimulationTimer.ElapsedInSeconds > 1.0)
		{
			_resetSimulationTimer.Reset();
		}
		if (!_resetSimulationTimer.IsRunning && (num6 < -0.5 || num6 > 1.0))
		{
			ResetSimulationTime();
		}
		else if (num6 > 0.05)
		{
			num9 = num6 * 0.125;
		}
		if (_resetInterpTimer.IsRunning && _resetInterpTimer.ElapsedInSeconds > 1.0)
		{
			_resetInterpTimer.Reset();
		}
		if (!_resetInterpTimer.IsRunning && (num7 < -0.5 || num7 > 1.0))
		{
			ResetInterpolationTime(resetMayBeCausedByFalseOutlier: true);
		}
		else if (num7 > 0.05)
		{
			num10 = num7 * 0.125;
		}
		_inputTime += Math.Max(0.0, num8);
		_simulationTime += Math.Max(0.0, num8 + num9);
		_interpTime += Math.Max(0.0, num10);
		_latestInputOffset += num8;
		_latestInputDelay -= num9;
		_latestInterpDelay -= num10;
		double amount = _clockTimeScaleOffset * unscaledDeltaTime + num8;
		AddInputOffsetAdjustment(amount);
		_clockFeedback.Update(_latestInputOffset, _targetInputOffset, unscaledDeltaTime);
		_delayFeedback.Update(_latestInputDelay, _targetInputDelay, unscaledDeltaTime);
		_interpFeedback.Update(_latestInterpDelay, _targetInterpDelay, unscaledDeltaTime);
		_clockTimeScaleOffset = _clockFeedback.Output();
		_delayTimeScaleOffset = 0.0 - _delayFeedback.Output();
		_interpTimeScaleOffset = 0.0 - _interpFeedback.Output();
		double num11 = (double)(int)_snapshot * _settings.ClientSimDeltaTime;
		if (_interpTime > num11)
		{
			_snapshotExceededFrames++;
			_snapshotExceededTime = Math.Max(_snapshotExceededTime, _interpTime - num11);
		}
		FrameTimeDeltaCheckReset();
	}

	bool ITimeProvider.IsRunning()
	{
		return _isRunning;
	}

	void ITimeProvider.Configure(SimulationRuntimeConfig src)
	{
		Configure(src);
	}

	void ITimeProvider.Configure(TimeSyncConfiguration tsc)
	{
		Configure(tsc);
	}

	void ITimeProvider.Reset(double roundTripTime, Tick snapshot)
	{
		double time = (double)(int)snapshot * _settings.ClientSimDeltaTime;
		double timeScale = 1.0;
		Reset(roundTripTime, snapshot, time, timeScale);
	}

	void ITimeProvider.Snap()
	{
		Snap();
	}

	void ITimeProvider.Update(double unscaledDeltaTime)
	{
		Update(unscaledDeltaTime);
	}

	void ITimeProvider.OnSnapshotReceived(double roundTripTime, Tick snapshot)
	{
		if (_trace && _timeTrace != null)
		{
			_timeTrace.OnPacket(snapshot, _snapshotTimer.ElapsedInSeconds, roundTripTime);
		}
		double time = (double)(int)snapshot * _settings.ClientSimDeltaTime;
		double timeScale = 1.0;
		FrameTimeDeltaCheck(Time.unscaledDeltaTime);
		UpdateSnapshot(snapshot);
		UpdateServerStats(roundTripTime, time, timeScale);
	}

	void ITimeProvider.OnFeedbackReceived(Simulation.TimeFeedback feedback)
	{
		if (_trace && _timeTrace != null)
		{
			_timeTrace.OnFeedback(feedback);
		}
		_serverFeedback.OffsetAvg = Maths.Lerp(_serverFeedback.OffsetAvg, feedback.OffsetAvg, 0.5f);
		_serverFeedback.OffsetDev = Maths.Lerp(_serverFeedback.OffsetDev, feedback.OffsetDev, 0.5f);
		_serverFeedback.RecvDeltaAvg = Maths.Lerp(_serverFeedback.RecvDeltaAvg, feedback.RecvDeltaAvg, 0.5f);
		_serverFeedback.RecvDeltaDev = Maths.Lerp(_serverFeedback.RecvDeltaDev, feedback.RecvDeltaDev, 0.5f);
	}

	void ITimeProvider.ResetFeedback()
	{
		_serverFeedback = new Simulation.TimeFeedback(0.0, 0.025, 0.0, 0.025);
	}

	Instant ITimeProvider.Now()
	{
		Instant result = default(Instant);
		result.Input = _inputTime;
		result.Local = _simulationTime;
		result.Remote = _interpTime;
		return result;
	}

	void ITimeProvider.Log(FusionStatisticsManager stats)
	{
		EngineProfiler.InputRecvDelta(_serverFeedback.RecvDeltaAvg);
		EngineProfiler.InputRecvDeltaDeviation(_serverFeedback.RecvDeltaDev);
		EngineProfiler.StateRecvDelta((float)_snapshotTimeDelta.Avg);
		EngineProfiler.StateRecvDeltaDeviation((float)_snapshotTimeDelta.Dev);
		EngineProfiler.SimulationOffset(_serverFeedback.OffsetAvg);
		EngineProfiler.SimulationOffsetDeviation(_serverFeedback.OffsetDev);
		double lastSeenServerTimeScale = _lastSeenServerTimeScale;
		double num = lastSeenServerTimeScale + _clockTimeScaleOffset + _delayTimeScaleOffset;
		EngineProfiler.SimulationSpeed((float)num);
		EngineProfiler.InterpolationOffset((float)_interpDelay.Avg);
		EngineProfiler.InterpolationOffsetDeviation((float)_interpDelay.Dev);
		double num2 = lastSeenServerTimeScale + _interpTimeScaleOffset;
		EngineProfiler.InterpolationSpeed((float)num2);
		bool flag = _inputTimeReset | _simulationTimeReset | _interpTimeReset;
		stats.PendingSnapshot.AddToInputReceiveDeltaStat(_serverFeedback.RecvDeltaAvg, overrideValue: true);
		stats.PendingSnapshot.AddToTimeResetsStat(flag ? 1 : 0);
		stats.PendingSnapshot.AddToStateReceiveDeltaStat((float)_snapshotTimeDelta.Avg, overrideValue: true);
		stats.PendingSnapshot.AddToSimulationTimeOffsetStat(_serverFeedback.OffsetAvg, overrideValue: true);
		stats.PendingSnapshot.AddToSimulationSpeedStat((float)num, overrideValue: true);
		stats.PendingSnapshot.AddToInterpolationOffsetStat((float)_interpDelay.Avg, overrideValue: true);
		stats.PendingSnapshot.AddToInterpolationSpeedStat((float)num2, overrideValue: true);
	}

	void ITimeProvider.SetPlayerIndex(int index)
	{
		_playerIndex = index;
	}

	void ITimeProvider.StartTrace()
	{
		_timeTrace = new ClientTimeTrace(tickRate: new TickRate.Resolved(_settings.ClientTickRate, _settings.OutgoingSendRate, _settings.ServerTickRate, _settings.IncomingSendRate), player: _playerIndex);
		_trace = true;
	}

	void ITimeProvider.StopTrace()
	{
		_trace = false;
	}
}
