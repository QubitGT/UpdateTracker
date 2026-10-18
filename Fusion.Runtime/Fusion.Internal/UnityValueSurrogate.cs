using System;

namespace Fusion.Internal;

[Serializable]
public abstract class UnityValueSurrogate<T, TReaderWriter> : UnitySurrogateBase, IUnityValueSurrogate<T>, IUnitySurrogate where TReaderWriter : unmanaged, IElementReaderWriter<T>
{
	public abstract T DataProperty { get; set; }

	public unsafe override void Read(int* data, int capacity)
	{
		DataProperty = default(TReaderWriter).Read((byte*)data, 0);
	}

	public unsafe override void Write(int* data, int capacity)
	{
		default(TReaderWriter).Write((byte*)data, 0, DataProperty);
	}

	public override void Init(int capacity)
	{
	}
}
