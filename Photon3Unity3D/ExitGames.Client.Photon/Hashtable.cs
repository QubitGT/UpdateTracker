using System.Collections.Generic;

namespace ExitGames.Client.Photon;

public class Hashtable : Dictionary<object, object>
{
	internal static readonly object[] boxedByte;

	public new object this[object key]
	{
		get
		{
			object value = null;
			TryGetValue(key, out value);
			return value;
		}
		set
		{
			base[key] = value;
		}
	}

	public object this[byte key]
	{
		get
		{
			object value = null;
			TryGetValue(boxedByte[key], out value);
			return value;
		}
		set
		{
			base[boxedByte[key]] = value;
		}
	}

	public static object GetBoxedByte(byte value)
	{
		return boxedByte[value];
	}

	static Hashtable()
	{
		int num = 256;
		boxedByte = new object[num];
		for (int i = 0; i < num; i++)
		{
			boxedByte[i] = (byte)i;
		}
	}

	public Hashtable()
	{
	}

	public Hashtable(int x)
		: base(x)
	{
	}

	public void Add(byte k, object v)
	{
		Add(boxedByte[k], v);
	}

	public void Remove(byte k)
	{
		Remove(boxedByte[k]);
	}

	public bool ContainsKey(byte key)
	{
		return ContainsKey(boxedByte[key]);
	}

	public new DictionaryEntryEnumerator GetEnumerator()
	{
		return new DictionaryEntryEnumerator(base.GetEnumerator());
	}

	public override string ToString()
	{
		List<string> list = new List<string>();
		foreach (object key in base.Keys)
		{
			if (key == null || this[key] == null)
			{
				list.Add(key?.ToString() + "=" + this[key]);
				continue;
			}
			list.Add("(" + key.GetType()?.ToString() + ")" + key?.ToString() + "=(" + this[key].GetType()?.ToString() + ")" + this[key]);
		}
		return string.Join(", ", list.ToArray());
	}

	public object Clone()
	{
		return new Dictionary<object, object>(this);
	}
}
