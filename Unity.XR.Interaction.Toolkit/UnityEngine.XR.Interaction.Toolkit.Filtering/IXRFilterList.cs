using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRFilterList<T>
{
	int count { get; }

	void Add(T item);

	bool Remove(T item);

	void MoveTo(T item, int newIndex);

	void Clear();

	void GetAll(List<T> results);

	T GetAt(int index);
}
