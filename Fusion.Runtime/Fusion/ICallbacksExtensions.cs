using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

internal static class ICallbacksExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_UNITY")]
	public static void InvokeOnInput(this Simulation.ICallbacks callbacks, SimulationInput input)
	{
		callbacks.OnInput(input);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("FUSION_UNITY")]
	public static void InvokeOnInputMissing(this Simulation.ICallbacks callbacks, SimulationInput input)
	{
		callbacks.OnInputMissing(input);
	}
}
