using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Fusion;

[ScriptHelp]
public abstract class Behaviour : MonoBehaviour, ILogSource, ILogDumpable
{
	private const string NameUnavailable = "(unavailable)";

	private const string NameDestroyed = "(destroyed)";

	internal string DebugNameThreadSafe
	{
		get
		{
			if ((bool)this)
			{
				try
				{
					return base.name;
				}
				catch
				{
					return "(unavailable)";
				}
			}
			return "(destroyed)";
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T AddBehaviour<T>() where T : Behaviour
	{
		return base.gameObject.AddComponent<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetBehaviour<T>(out T behaviour) where T : Behaviour
	{
		return base.gameObject.TryGetComponent<T>(out behaviour);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetBehaviour<T>() where T : Behaviour
	{
		return base.gameObject.GetComponentInChildren<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DestroyBehaviour(Behaviour behaviour)
	{
		Object.Destroy(behaviour);
	}

	void ILogDumpable.Dump(StringBuilder builder)
	{
		GetDumpString(builder);
	}

	protected internal virtual void GetDumpString(StringBuilder builder)
	{
		builder.Append("[");
		builder.Append(DebugNameThreadSafe);
		builder.Append("]");
	}
}
