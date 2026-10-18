using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal class ExposedRegistrationList<T> : SmallRegistrationList<T>, IXRFilterList<T> where T : class
{
	public int count => base.flushedCount;

	public void Add(T item)
	{
		if (item == null || (item is Object @object && @object == null))
		{
			throw new ArgumentNullException("item");
		}
		Register(item);
	}

	public bool Remove(T item)
	{
		return Unregister(item);
	}

	public void MoveTo(T item, int newIndex)
	{
		MoveItemImmediately(item, newIndex);
	}

	public void Clear()
	{
		UnregisterAll();
	}

	public void GetAll(List<T> results)
	{
		GetRegisteredItems(results);
	}

	public T GetAt(int index)
	{
		return GetRegisteredItemAt(index);
	}

	public void RegisterReferences<TObject>(List<TObject> references, Object context = null) where TObject : Object
	{
		foreach (TObject reference in references)
		{
			if (reference != null && reference is T item)
			{
				Add(item);
			}
			else if (context != null)
			{
				Debug.LogError($"Trying to add the invalid item {reference} into {typeof(IXRFilterList<T>).Name}, in {context}. {reference} does not implement {typeof(T).Name}.", context);
			}
			else
			{
				Debug.LogError($"Trying to add the invalid item {reference} into {typeof(IXRFilterList<T>).Name}. {reference} does not implement {typeof(T).Name}.");
			}
		}
	}
}
