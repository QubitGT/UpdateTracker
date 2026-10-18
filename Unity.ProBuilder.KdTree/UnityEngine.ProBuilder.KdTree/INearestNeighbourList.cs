namespace UnityEngine.ProBuilder.KdTree;

internal interface INearestNeighbourList<TItem, TDistance>
{
	int MaxCapacity { get; }

	int Count { get; }

	bool Add(TItem item, TDistance distance);

	TItem GetFurtherest();

	TItem RemoveFurtherest();
}
