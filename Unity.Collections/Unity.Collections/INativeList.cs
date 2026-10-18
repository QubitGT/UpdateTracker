namespace Unity.Collections;

public interface INativeList<T> : IIndexable<T> where T : unmanaged
{
	int Capacity { get; set; }

	bool IsEmpty { get; }

	T this[int index] { get; set; }

	void Clear();
}
