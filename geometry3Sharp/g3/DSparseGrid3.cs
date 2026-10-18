using System.Collections.Generic;
using System.Linq;

namespace g3;

public class DSparseGrid3<ElemType> : IGrid3 where ElemType : class, IGridElement3
{
	private ElemType exemplar;

	private Dictionary<Vector3i, ElemType> elements;

	private AxisAlignedBox3i bounds;

	public int Count => elements.Count;

	public double Density => (double)elements.Count / (double)bounds.Volume;

	public AxisAlignedBox3i BoundsInclusive => bounds;

	public Vector3i Dimensions => bounds.Diagonal + Vector3i.One;

	public DSparseGrid3(ElemType toDuplicate)
	{
		exemplar = toDuplicate;
		elements = new Dictionary<Vector3i, ElemType>();
		bounds = AxisAlignedBox3i.Empty;
	}

	public bool Has(Vector3i index)
	{
		return elements.ContainsKey(index);
	}

	public ElemType Get(Vector3i index, bool allocateIfMissing = true)
	{
		if (elements.TryGetValue(index, out var value))
		{
			return value;
		}
		if (allocateIfMissing)
		{
			return allocate(index);
		}
		return null;
	}

	public bool Free(Vector3i index)
	{
		if (elements.ContainsKey(index))
		{
			elements.Remove(index);
			return true;
		}
		return false;
	}

	public void FreeAll()
	{
		while (elements.Count > 0)
		{
			elements.Remove(elements.First().Key);
		}
	}

	public IEnumerable<Vector3i> AllocatedIndices()
	{
		foreach (KeyValuePair<Vector3i, ElemType> element in elements)
		{
			yield return element.Key;
		}
	}

	public IEnumerable<KeyValuePair<Vector3i, ElemType>> Allocated()
	{
		return elements;
	}

	private ElemType allocate(Vector3i index)
	{
		ElemType val = exemplar.CreateNewGridElement(bCopy: false) as ElemType;
		elements.Add(index, val);
		bounds.Contain(index);
		return val;
	}
}
