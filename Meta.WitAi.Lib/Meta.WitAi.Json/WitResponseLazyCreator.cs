namespace Meta.WitAi.Json;

internal class WitResponseLazyCreator : WitResponseNode
{
	private WitResponseNode m_Node;

	private string m_Key;

	public override WitResponseNode this[int aIndex]
	{
		get
		{
			return new WitResponseLazyCreator(this);
		}
		set
		{
			WitResponseArray aVal = new WitResponseArray { value };
			Set(aVal);
		}
	}

	public override WitResponseNode this[string aKey]
	{
		get
		{
			return new WitResponseLazyCreator(this, aKey);
		}
		set
		{
			WitResponseClass aVal = new WitResponseClass { { aKey, value } };
			Set(aVal);
		}
	}

	public override int AsInt
	{
		get
		{
			WitResponseData aVal = new WitResponseData(0);
			Set(aVal);
			return 0;
		}
		set
		{
			WitResponseData aVal = new WitResponseData(value);
			Set(aVal);
		}
	}

	public override float AsFloat
	{
		get
		{
			WitResponseData aVal = new WitResponseData(0f);
			Set(aVal);
			return 0f;
		}
		set
		{
			WitResponseData aVal = new WitResponseData(value);
			Set(aVal);
		}
	}

	public override double AsDouble
	{
		get
		{
			WitResponseData aVal = new WitResponseData(0.0);
			Set(aVal);
			return 0.0;
		}
		set
		{
			WitResponseData aVal = new WitResponseData(value);
			Set(aVal);
		}
	}

	public override bool AsBool
	{
		get
		{
			WitResponseData aVal = new WitResponseData(aData: false);
			Set(aVal);
			return false;
		}
		set
		{
			WitResponseData aVal = new WitResponseData(value);
			Set(aVal);
		}
	}

	public override WitResponseArray AsArray
	{
		get
		{
			WitResponseArray witResponseArray = new WitResponseArray();
			Set(witResponseArray);
			return witResponseArray;
		}
	}

	public override WitResponseClass AsObject
	{
		get
		{
			WitResponseClass witResponseClass = new WitResponseClass();
			Set(witResponseClass);
			return witResponseClass;
		}
	}

	public WitResponseLazyCreator(WitResponseNode aNode)
	{
		m_Node = aNode;
		m_Key = null;
	}

	public WitResponseLazyCreator(WitResponseNode aNode, string aKey)
	{
		m_Node = aNode;
		m_Key = aKey;
	}

	private void Set(WitResponseNode aVal)
	{
		if (m_Key == null)
		{
			m_Node.Add(aVal);
		}
		else
		{
			m_Node.Add(m_Key, aVal);
		}
		m_Node = null;
	}

	public override void Add(WitResponseNode aItem)
	{
		WitResponseArray aVal = new WitResponseArray { aItem };
		Set(aVal);
	}

	public override void Add(string aKey, WitResponseNode aItem)
	{
		WitResponseClass aVal = new WitResponseClass { { aKey, aItem } };
		Set(aVal);
	}

	public static bool operator ==(WitResponseLazyCreator a, object b)
	{
		if (b == null)
		{
			return true;
		}
		return (object)a == b;
	}

	public static bool operator !=(WitResponseLazyCreator a, object b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return true;
		}
		return (object)this == obj;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return "";
	}

	public override string ToString(string aPrefix)
	{
		return "";
	}
}
