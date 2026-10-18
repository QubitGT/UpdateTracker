using System;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Interactions;

[Preserve]
public class SectorInteraction : IInputInteraction<Vector2>, IInputInteraction
{
	[Flags]
	public enum Directions
	{
		None = 0,
		North = 1,
		South = 2,
		East = 4,
		West = 8
	}

	public enum SweepBehavior
	{
		Locked,
		[InspectorName("Allow Reentry")]
		AllowReentry,
		[InspectorName("Disallow Reentry")]
		DisallowReentry,
		[InspectorName("History Independent")]
		HistoryIndependent
	}

	private enum State
	{
		Centered,
		StartedValidDirection,
		StartedInvalidDirection
	}

	public Directions directions;

	public SweepBehavior sweepBehavior;

	public float pressPoint = -1f;

	private State m_State;

	private bool m_WasValidDirection;

	internal float pressPointOrDefault
	{
		get
		{
			if (!(pressPoint >= 0f))
			{
				return defaultPressPoint;
			}
			return pressPoint;
		}
	}

	public static float defaultPressPoint { get; set; }

	public void Process(ref InputInteractionContext context)
	{
		if (!context.ControlIsActuated(pressPointOrDefault))
		{
			State state = m_State;
			if (state != 0 && (uint)(state - 1) <= 1u)
			{
				m_State = State.Centered;
				context.Canceled();
			}
			return;
		}
		bool flag = IsValidDirection(ref context);
		if (m_State == State.Centered)
		{
			m_State = (flag ? State.StartedValidDirection : State.StartedInvalidDirection);
			if (flag)
			{
				context.PerformedAndStayPerformed();
			}
			m_WasValidDirection = flag;
			return;
		}
		switch (sweepBehavior)
		{
		case SweepBehavior.AllowReentry:
			if (m_WasValidDirection && !flag && m_State == State.StartedValidDirection)
			{
				context.Canceled();
			}
			else if (!m_WasValidDirection && flag && m_State == State.StartedValidDirection)
			{
				context.PerformedAndStayPerformed();
			}
			break;
		case SweepBehavior.DisallowReentry:
			if (m_WasValidDirection && !flag && m_State == State.StartedValidDirection)
			{
				context.Canceled();
			}
			break;
		case SweepBehavior.HistoryIndependent:
			if (m_WasValidDirection && !flag)
			{
				context.Canceled();
			}
			else if (!m_WasValidDirection && flag)
			{
				context.PerformedAndStayPerformed();
			}
			break;
		}
		m_WasValidDirection = flag;
	}

	private bool IsValidDirection(ref InputInteractionContext context)
	{
		return (GetNearestDirection(CardinalUtility.GetNearestCardinal(context.ReadValue<Vector2>())) & directions) != 0;
	}

	private static Directions GetNearestDirection(Cardinal value)
	{
		return value switch
		{
			Cardinal.North => Directions.North, 
			Cardinal.South => Directions.South, 
			Cardinal.East => Directions.East, 
			Cardinal.West => Directions.West, 
			_ => Directions.None, 
		};
	}

	public void Reset()
	{
	}

	[Preserve]
	static SectorInteraction()
	{
		defaultPressPoint = 0.5f;
		UnityEngine.InputSystem.InputSystem.RegisterInteraction<SectorInteraction>();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	private static void Initialize()
	{
	}
}
