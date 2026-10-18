namespace UnityEngine.ProBuilder.KdTree;

internal struct HyperRect<T>
{
	private T[] minPoint;

	private T[] maxPoint;

	public T[] MinPoint
	{
		get
		{
			return minPoint;
		}
		set
		{
			minPoint = new T[value.Length];
			value.CopyTo(minPoint, 0);
		}
	}

	public T[] MaxPoint
	{
		get
		{
			return maxPoint;
		}
		set
		{
			maxPoint = new T[value.Length];
			value.CopyTo(maxPoint, 0);
		}
	}

	public static HyperRect<T> Infinite(int dimensions, ITypeMath<T> math)
	{
		HyperRect<T> result = default(HyperRect<T>);
		result.MinPoint = new T[dimensions];
		result.MaxPoint = new T[dimensions];
		for (int i = 0; i < dimensions; i++)
		{
			result.MinPoint[i] = math.NegativeInfinity;
			result.MaxPoint[i] = math.PositiveInfinity;
		}
		return result;
	}

	public T[] GetClosestPoint(T[] toPoint, ITypeMath<T> math)
	{
		T[] array = new T[toPoint.Length];
		for (int i = 0; i < toPoint.Length; i++)
		{
			if (math.Compare(minPoint[i], toPoint[i]) > 0)
			{
				array[i] = minPoint[i];
			}
			else if (math.Compare(maxPoint[i], toPoint[i]) < 0)
			{
				array[i] = maxPoint[i];
			}
			else
			{
				array[i] = toPoint[i];
			}
		}
		return array;
	}

	public HyperRect<T> Clone()
	{
		HyperRect<T> result = default(HyperRect<T>);
		result.MinPoint = MinPoint;
		result.MaxPoint = MaxPoint;
		return result;
	}
}
