using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public static class AffordanceStateShortcuts
{
	public const byte disabled = 0;

	public const byte idle = 1;

	public const byte hovered = 2;

	public const byte hoveredPriority = 3;

	public const byte selected = 4;

	public const byte activated = 5;

	public const byte focused = 6;

	private static readonly Dictionary<byte, string> k_StateNames = new Dictionary<byte, string>
	{
		{ 0, "disabled" },
		{ 1, "idle" },
		{ 2, "hovered" },
		{ 3, "hoveredPriority" },
		{ 4, "selected" },
		{ 5, "activated" },
		{ 6, "focused" }
	};

	public static AffordanceStateData disabledState { get; } = new AffordanceStateData(0, 1f);


	public static AffordanceStateData idleState { get; } = new AffordanceStateData(1, 1f);


	public static AffordanceStateData hoveredState { get; } = new AffordanceStateData(2, 0f);


	public static AffordanceStateData hoveredPriorityState { get; } = new AffordanceStateData(3, 0f);


	public static AffordanceStateData selectedState { get; } = new AffordanceStateData(4, 1f);


	public static AffordanceStateData activatedState { get; } = new AffordanceStateData(5, 1f);


	public static AffordanceStateData focusedState { get; } = new AffordanceStateData(6, 1f);


	internal static byte stateCount { get; } = (byte)k_StateNames.Count;


	internal static string GetNameForIndex(byte index)
	{
		if (!k_StateNames.TryGetValue(index, out var value))
		{
			return null;
		}
		return value;
	}
}
