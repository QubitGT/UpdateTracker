using System;

namespace SouthPointe.Serialization.MessagePack;

public sealed class Lazy<T>
{
	private readonly object padlock = new object();

	private readonly Func<T> createValue;

	private bool isValueCreated;

	private T value;

	public T Value
	{
		get
		{
			if (!isValueCreated)
			{
				lock (padlock)
				{
					if (!isValueCreated)
					{
						value = createValue();
						isValueCreated = true;
					}
				}
			}
			return value;
		}
	}

	public bool IsValueCreated
	{
		get
		{
			lock (padlock)
			{
				return isValueCreated;
			}
		}
	}

	public Lazy(Func<T> createValue)
	{
		if (createValue == null)
		{
			throw new ArgumentNullException("createValue");
		}
		this.createValue = createValue;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
