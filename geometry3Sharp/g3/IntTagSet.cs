using System.Collections.Generic;

namespace g3;

public class IntTagSet<T>
{
	public const int InvalidTag = int.MaxValue;

	private Dictionary<T, int> tags;

	private void create()
	{
		if (tags == null)
		{
			tags = new Dictionary<T, int>();
		}
	}

	public void Add(T reference, int tag)
	{
		create();
		tags.Add(reference, tag);
	}

	public bool Has(T reference)
	{
		int value = 0;
		if (tags != null && tags.TryGetValue(reference, out value))
		{
			return true;
		}
		return false;
	}

	public int Get(T reference)
	{
		int value = 0;
		if (tags != null && tags.TryGetValue(reference, out value))
		{
			return value;
		}
		return int.MaxValue;
	}
}
