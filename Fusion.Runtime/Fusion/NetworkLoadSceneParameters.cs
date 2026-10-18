using System;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

namespace Fusion;

[StructLayout(LayoutKind.Explicit, Size = 2)]
public readonly struct NetworkLoadSceneParameters : IEquatable<NetworkLoadSceneParameters>
{
	[FieldOffset(0)]
	public readonly NetworkSceneLoadId LoadId;

	[FieldOffset(1)]
	private readonly NetworkLoadSceneParametersFlags _flags;

	public LoadSceneMode LoadSceneMode => ((_flags & NetworkLoadSceneParametersFlags.Single) == 0) ? LoadSceneMode.Additive : LoadSceneMode.Single;

	public LocalPhysicsMode LocalPhysicsMode => (((_flags & NetworkLoadSceneParametersFlags.LocalPhysics3D) != 0) ? LocalPhysicsMode.Physics3D : LocalPhysicsMode.None) | (((_flags & NetworkLoadSceneParametersFlags.LocalPhysics2D) != 0) ? LocalPhysicsMode.Physics2D : LocalPhysicsMode.None);

	public LoadSceneParameters LoadSceneParameters => new LoadSceneParameters(LoadSceneMode, LocalPhysicsMode);

	public bool IsActiveOnLoad => (_flags & NetworkLoadSceneParametersFlags.ActiveOnLoad) != 0;

	public bool IsSingleLoad => (_flags & NetworkLoadSceneParametersFlags.Single) != 0;

	public bool IsLocalPhysics2D => (_flags & NetworkLoadSceneParametersFlags.LocalPhysics2D) != 0;

	public bool IsLocalPhysics3D => (_flags & NetworkLoadSceneParametersFlags.LocalPhysics3D) != 0;

	internal NetworkLoadSceneParameters(NetworkSceneLoadId loadId, NetworkLoadSceneParametersFlags flags)
	{
		LoadId = loadId;
		_flags = flags;
	}

	public bool Equals(NetworkLoadSceneParameters other)
	{
		return _flags == other._flags && LoadId.Equals(other.LoadId);
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkLoadSceneParameters other && Equals(other);
	}

	public override int GetHashCode()
	{
		return ((int)_flags * 397) ^ LoadId.GetHashCode();
	}

	public static bool operator ==(NetworkLoadSceneParameters left, NetworkLoadSceneParameters right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NetworkLoadSceneParameters left, NetworkLoadSceneParameters right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"[Flags: {_flags}, LoadId: {LoadId}]";
	}
}
