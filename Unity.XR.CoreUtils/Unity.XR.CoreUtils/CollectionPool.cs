using System.Collections.Generic;

namespace Unity.XR.CoreUtils;

public static class CollectionPool<TCollection, TValue> where TCollection : ICollection<TValue>, new()
{
	private static readonly Queue<TCollection> k_CollectionQueue = new Queue<TCollection>();

	public static TCollection GetCollection()
	{
		if (k_CollectionQueue.Count <= 0)
		{
			return new TCollection();
		}
		return k_CollectionQueue.Dequeue();
	}

	public static void RecycleCollection(TCollection collection)
	{
		collection.Clear();
		k_CollectionQueue.Enqueue(collection);
	}
}
