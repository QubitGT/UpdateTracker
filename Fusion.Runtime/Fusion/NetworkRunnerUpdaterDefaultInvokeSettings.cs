using System;

namespace Fusion;

public struct NetworkRunnerUpdaterDefaultInvokeSettings : IEquatable<NetworkRunnerUpdaterDefaultInvokeSettings>
{
	public Type ReferencePlayerLoopSystem;

	public UnityPlayerLoopSystemAddMode AddMode;

	public bool Equals(NetworkRunnerUpdaterDefaultInvokeSettings other)
	{
		return ReferencePlayerLoopSystem == other.ReferencePlayerLoopSystem && AddMode == other.AddMode;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkRunnerUpdaterDefaultInvokeSettings other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (((ReferencePlayerLoopSystem != null) ? ReferencePlayerLoopSystem.GetHashCode() : 0) * 397) ^ (int)AddMode;
	}

	public override string ToString()
	{
		return $"[{ReferencePlayerLoopSystem?.FullName}, {AddMode}]";
	}

	public static bool operator ==(NetworkRunnerUpdaterDefaultInvokeSettings left, NetworkRunnerUpdaterDefaultInvokeSettings right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NetworkRunnerUpdaterDefaultInvokeSettings left, NetworkRunnerUpdaterDefaultInvokeSettings right)
	{
		return !left.Equals(right);
	}
}
