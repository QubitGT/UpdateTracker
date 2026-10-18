using System.Collections.Generic;

namespace g3;

public class IndexHashSet : HashSet<int>
{
	public bool this[int key]
	{
		get
		{
			return Contains(key);
		}
		set
		{
			if (value)
			{
				Add(key);
			}
			else if (!value && Contains(key))
			{
				Remove(key);
			}
		}
	}
}
