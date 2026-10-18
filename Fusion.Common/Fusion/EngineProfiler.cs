#define ENABLE_PROFILER
using System;
using System.Diagnostics;
using UnityEngine.Profiling;

namespace Fusion;

public static class EngineProfiler
{
	public static Action<float> RoundTripTimeCallback;

	public static Action<int> ResimulationsCallback;

	public static Action<int> WorldSnapshotSizeCallback;

	public static Action<int> InputSizeCallback;

	public static Action<int> InputQueueCallback;

	public static Action<int> RpcInCallback;

	public static Action<int> RpcOutCallback;

	public static Action<float> StateRecvDeltaCallback;

	public static Action<float> StateRecvDeltaDeviationCallback;

	public static Action<float> InterpolationSpeedCallback;

	public static Action<float> InterpolationOffsetCallback;

	public static Action<float> InterpolationOffsetDeviationCallback;

	public static Action<float> InputRecvDeltaCallback;

	public static Action<float> SimulationSpeedCallback;

	public static Action<float> InputRecvDeltaDeviationCallback;

	public static Action<float> SimulationOffsetCallback;

	public static Action<float> SimulationOffsetDeviationCallback;

	[Conditional("ENABLE_PROFILER")]
	public static void Begin(string sample)
	{
		Profiler.BeginSample(sample);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void End()
	{
		Profiler.EndSample();
	}

	[Conditional("ENABLE_PROFILER")]
	public static void RoundTripTime(float value)
	{
		RoundTripTimeCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void Resimulations(int value)
	{
		ResimulationsCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void WorldSnapshotSize(int value)
	{
		WorldSnapshotSizeCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InputSize(int value)
	{
		InputSizeCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InputQueue(int value)
	{
		InputQueueCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void RpcIn(int value)
	{
		RpcInCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void RpcOut(int value)
	{
		RpcOutCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void StateRecvDelta(float value)
	{
		StateRecvDeltaCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void StateRecvDeltaDeviation(float value)
	{
		StateRecvDeltaDeviationCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InterpolationSpeed(float value)
	{
		InterpolationSpeedCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InterpolationOffset(float value)
	{
		InterpolationOffsetCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InterpolationOffsetDeviation(float value)
	{
		InterpolationOffsetDeviationCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InputRecvDelta(float value)
	{
		InputRecvDeltaCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void InputRecvDeltaDeviation(float value)
	{
		InputRecvDeltaDeviationCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void SimulationSpeed(float value)
	{
		SimulationSpeedCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void SimulationOffset(float value)
	{
		SimulationOffsetCallback?.Invoke(value);
	}

	[Conditional("ENABLE_PROFILER")]
	public static void SimulationOffsetDeviation(float value)
	{
		SimulationOffsetDeviationCallback?.Invoke(value);
	}
}
