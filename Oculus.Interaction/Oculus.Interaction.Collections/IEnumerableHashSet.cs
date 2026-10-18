using System.Collections;
using System.Collections.Generic;

namespace Oculus.Interaction.Collections;

public interface IEnumerableHashSet<T> : IEnumerable<T>, IEnumerable
{
	int Count { get; }

	new HashSet<T>.Enumerator GetEnumerator();

	bool Contains(T item);

	bool IsProperSubsetOf(IEnumerable<T> other);

	bool IsProperSupersetOf(IEnumerable<T> other);

	bool IsSubsetOf(IEnumerable<T> other);

	bool IsSupersetOf(IEnumerable<T> other);

	bool Overlaps(IEnumerable<T> other);

	bool SetEquals(IEnumerable<T> other);
}
