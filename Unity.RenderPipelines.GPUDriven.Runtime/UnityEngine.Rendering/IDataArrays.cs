namespace UnityEngine.Rendering;

internal interface IDataArrays
{
	void Initialize(int initCapacity);

	void Dispose();

	void Grow(int newCapacity);

	void Remove(int index, int lastIndex);

	void SetDefault(int index);
}
