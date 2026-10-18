#define DEBUG
using UnityEngine;

namespace Fusion;

public ref struct NetworkBehaviourBuffer
{
	internal unsafe int* _ptr;

	internal int _length;

	internal Tick _tick;

	public Tick Tick => _tick;

	public int Length => _length;

	public unsafe bool Valid => _ptr != null && _length > 0;

	public unsafe int this[int index]
	{
		get
		{
			Assert.Check((uint)index < (uint)_length);
			return _ptr[index];
		}
	}

	public unsafe T ReinterpretState<T>(int offset = 0) where T : unmanaged
	{
		Assert.Check(Valid);
		Assert.Check((uint)offset < (uint)_length, offset, _length);
		Assert.Check((uint)(offset + Native.WordCount(sizeof(T), 4)) <= (uint)_length);
		return *(T*)(_ptr + offset);
	}

	internal unsafe NetworkBehaviourBuffer(Tick tick, int* ptr, int length)
	{
		_ptr = ptr;
		_tick = tick;
		_length = length;
	}

	public T Read<T>(NetworkBehaviour.BehaviourReader<T> reader) where T : NetworkBehaviour
	{
		Assert.Check(Valid);
		return reader.Read(this);
	}

	public unsafe T Read<T>(NetworkBehaviour.PropertyReader<T> reader) where T : unmanaged
	{
		Assert.Check(Valid);
		Assert.Check((uint)reader.Data.Offset < (uint)_length);
		Assert.Check((uint)(reader.Data.Offset + Native.WordCount(sizeof(T), 4)) <= (uint)_length);
		return *(T*)(_ptr + reader.Data.Offset);
	}

	public unsafe float Read(NetworkBehaviour.PropertyReader<float> reader)
	{
		Assert.Check(Valid);
		Assert.Check((uint)reader.Data.Offset < (uint)_length);
		Assert.Check((uint)(reader.Data.Offset + 1) <= (uint)_length);
		return ReadWriteUtils.ReadFloat(_ptr + reader.Data.Offset);
	}

	public unsafe Vector2 Read(NetworkBehaviour.PropertyReader<Vector2> reader)
	{
		Assert.Check(Valid);
		Assert.Check((uint)reader.Data.Offset < (uint)_length);
		Assert.Check((uint)(reader.Data.Offset + 2) <= (uint)_length);
		return ReadWriteUtils.ReadVector2(_ptr + reader.Data.Offset);
	}

	public unsafe Vector3 Read(NetworkBehaviour.PropertyReader<Vector3> reader)
	{
		Assert.Check(Valid);
		Assert.Check((uint)reader.Data.Offset < (uint)_length);
		Assert.Check((uint)(reader.Data.Offset + 3) <= (uint)_length);
		return ReadWriteUtils.ReadVector3(_ptr + reader.Data.Offset);
	}

	public unsafe Vector4 Read(NetworkBehaviour.PropertyReader<Vector4> reader)
	{
		Assert.Check(Valid);
		Assert.Check((uint)reader.Data.Offset < (uint)_length);
		Assert.Check((uint)(reader.Data.Offset + 4) <= (uint)_length);
		return ReadWriteUtils.ReadVector4(_ptr + reader.Data.Offset);
	}

	public unsafe Quaternion Read(NetworkBehaviour.PropertyReader<Quaternion> reader)
	{
		Assert.Check(Valid);
		Assert.Check((uint)reader.Data.Offset < (uint)_length);
		Assert.Check((uint)(reader.Data.Offset + 4) <= (uint)_length);
		return ReadWriteUtils.ReadQuaternion(_ptr + reader.Data.Offset);
	}

	public static implicit operator bool(NetworkBehaviourBuffer buffer)
	{
		return buffer.Valid;
	}
}
