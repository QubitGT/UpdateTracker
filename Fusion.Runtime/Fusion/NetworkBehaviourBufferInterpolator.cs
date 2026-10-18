#define DEBUG
using UnityEngine;

namespace Fusion;

public readonly ref struct NetworkBehaviourBufferInterpolator
{
	public readonly NetworkBehaviour Behaviour;

	public readonly NetworkBehaviourBuffer From;

	public readonly NetworkBehaviourBuffer To;

	public readonly float Alpha;

	public readonly bool Valid;

	public NetworkBehaviourBufferInterpolator(NetworkBehaviour nb)
	{
		Behaviour = nb;
		Valid = nb.TryGetSnapshotsBuffers(out From, out To, out Alpha);
	}

	public float Angle(string property)
	{
		return Angle(Behaviour.GetPropertyReader<Angle>(property));
	}

	public float Angle(NetworkBehaviour.PropertyReader<Angle> property)
	{
		Assert.Check(Valid);
		var (angle, angle2) = property.Read(From, To);
		return (float)angle + ((float)angle2 - (float)angle) * Alpha;
	}

	public float Float(string property)
	{
		return Float(Behaviour.GetPropertyReader<float>(property));
	}

	public float Float(NetworkBehaviour.PropertyReader<float> property)
	{
		Assert.Check(Valid);
		var (num, num2) = property.Read(From, To);
		return num + (num2 - num) * Alpha;
	}

	public int Int(string property)
	{
		return Select<int>(property);
	}

	public int Int(NetworkBehaviour.PropertyReader<int> property)
	{
		return Select(property);
	}

	public bool Bool(NetworkBehaviour.PropertyReader<bool> property)
	{
		return Select(property);
	}

	public bool Bool(string property)
	{
		return Select<bool>(property);
	}

	public T Select<T>(string property) where T : unmanaged
	{
		return Select(Behaviour.GetPropertyReader<T>(property));
	}

	public T Select<T>(NetworkBehaviour.PropertyReader<T> property) where T : unmanaged
	{
		Assert.Check(Valid);
		var (val, val2) = property.Read(From, To);
		return ((double)Alpha < 0.5) ? val : val2;
	}

	public Vector3 Vector3(string property)
	{
		return Vector3(Behaviour.GetPropertyReader<Vector3>(property));
	}

	public Vector3 Vector3(NetworkBehaviour.PropertyReader<Vector3> property)
	{
		Assert.Check(Valid);
		var (a, b) = property.Read(From, To);
		return UnityEngine.Vector3.Lerp(a, b, Alpha);
	}

	public Vector2 Vector2(string property)
	{
		return Vector2(Behaviour.GetPropertyReader<Vector2>(property));
	}

	public Vector2 Vector2(NetworkBehaviour.PropertyReader<Vector2> property)
	{
		Assert.Check(Valid);
		var (a, b) = property.Read(From, To);
		return UnityEngine.Vector2.Lerp(a, b, Alpha);
	}

	public Vector4 Vector4(string property)
	{
		return Vector4(Behaviour.GetPropertyReader<Vector4>(property));
	}

	public Vector4 Vector4(NetworkBehaviour.PropertyReader<Vector4> property)
	{
		Assert.Check(Valid);
		var (a, b) = property.Read(From, To);
		return UnityEngine.Vector4.Lerp(a, b, Alpha);
	}

	public Quaternion Quaternion(string property)
	{
		return Quaternion(Behaviour.GetPropertyReader<Quaternion>(property));
	}

	public Quaternion Quaternion(NetworkBehaviour.PropertyReader<Quaternion> property)
	{
		Assert.Check(Valid);
		var (a, b) = property.Read(From, To);
		return UnityEngine.Quaternion.Slerp(a, b, Alpha);
	}

	public static implicit operator bool(NetworkBehaviourBufferInterpolator i)
	{
		return i.Valid;
	}
}
