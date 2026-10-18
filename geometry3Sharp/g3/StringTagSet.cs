using System.Collections.Generic;

namespace g3;

public class StringTagSet<T>
{
	public const string InvalidTag = "";

	private Dictionary<T, string> tags;

	private void create()
	{
		if (tags == null)
		{
			tags = new Dictionary<T, string>();
		}
	}

	public void Add(T reference, string tag)
	{
		create();
		tags.Add(reference, tag);
	}

	public bool Has(T reference)
	{
		string value = "";
		if (tags != null && tags.TryGetValue(reference, out value))
		{
			return true;
		}
		return false;
	}

	public string Get(T reference)
	{
		string value = "";
		if (tags != null && tags.TryGetValue(reference, out value))
		{
			return value;
		}
		return "";
	}
}
