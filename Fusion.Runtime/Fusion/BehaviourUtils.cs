using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fusion;

internal static class BehaviourUtils
{
	public struct DeferredJoin
	{
		public IEnumerable _enumerable;

		public override string ToString()
		{
			return string.Join(", ", _enumerable.Cast<object>());
		}
	}

	internal struct NameDeferred
	{
		private Behaviour _behaviour;

		public NameDeferred(Behaviour behaviour)
		{
			_behaviour = behaviour;
		}

		public static explicit operator NameDeferred(Behaviour behaviour)
		{
			return new NameDeferred(behaviour);
		}

		public static implicit operator string(NameDeferred wrapper)
		{
			return wrapper.ToString();
		}

		public override string ToString()
		{
			if (IsNull(_behaviour))
			{
				return "(null)";
			}
			return _behaviour.DebugNameThreadSafe;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(Behaviour obj)
	{
		return (object)obj == null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(Behaviour obj)
	{
		return (object)obj != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlive(NetworkRunner obj)
	{
		return obj;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotAlive(NetworkRunner obj)
	{
		return !obj;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlive(SimulationBehaviour obj)
	{
		return (object)obj != null && (obj.Flags & SimulationBehaviourRuntimeFlags.IsUnityDestroyed) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotAlive(SimulationBehaviour obj)
	{
		return (object)obj == null || (obj.Flags & SimulationBehaviourRuntimeFlags.IsUnityDestroyed) == SimulationBehaviourRuntimeFlags.IsUnityDestroyed;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlive(NetworkObject obj)
	{
		return (object)obj != null && (obj.RuntimeFlags & NetworkObjectRuntimeFlags.IsDestroyed) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotAlive(NetworkObject obj)
	{
		return (object)obj == null || (obj.RuntimeFlags & NetworkObjectRuntimeFlags.IsDestroyed) == NetworkObjectRuntimeFlags.IsDestroyed;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSame(Behaviour a, Behaviour b)
	{
		return (object)a == b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSameNotNull(Behaviour a, Behaviour b)
	{
		return (object)a != null && (object)a == b;
	}

	public static NameDeferred GetName(Behaviour obj)
	{
		return new NameDeferred(obj);
	}

	public static DeferredJoin Join(IEnumerable objects)
	{
		DeferredJoin result = default(DeferredJoin);
		result._enumerable = objects;
		return result;
	}
}
