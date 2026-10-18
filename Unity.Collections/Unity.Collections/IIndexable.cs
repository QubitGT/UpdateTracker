namespace Unity.Collections;

public interface IIndexable<T> where T : unmanaged
{
	int Length { get; set; }

	ref T ElementAt(int index);
}
