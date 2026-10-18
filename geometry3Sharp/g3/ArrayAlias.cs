namespace g3;

public struct ArrayAlias<T>
{
	public T[] Source;

	public int Index;

	public T this[int i]
	{
		get
		{
			return Source[Index + i];
		}
		set
		{
			Source[Index + i] = value;
		}
	}

	public ArrayAlias(T[] source, int i)
	{
		Source = source;
		Index = i;
	}
}
