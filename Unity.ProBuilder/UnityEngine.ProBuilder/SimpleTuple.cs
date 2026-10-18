namespace UnityEngine.ProBuilder;

public struct SimpleTuple<T1, T2>
{
	private T1 m_Item1;

	private T2 m_Item2;

	public T1 item1
	{
		get
		{
			return m_Item1;
		}
		set
		{
			m_Item1 = value;
		}
	}

	public T2 item2
	{
		get
		{
			return m_Item2;
		}
		set
		{
			m_Item2 = value;
		}
	}

	public SimpleTuple(T1 item1, T2 item2)
	{
		m_Item1 = item1;
		m_Item2 = item2;
	}

	public override string ToString()
	{
		return $"{item1.ToString()}, {item2.ToString()}";
	}
}
internal struct SimpleTuple<T1, T2, T3>
{
	private T1 m_Item1;

	private T2 m_Item2;

	private T3 m_Item3;

	public T1 item1
	{
		get
		{
			return m_Item1;
		}
		set
		{
			m_Item1 = value;
		}
	}

	public T2 item2
	{
		get
		{
			return m_Item2;
		}
		set
		{
			m_Item2 = value;
		}
	}

	public T3 item3
	{
		get
		{
			return m_Item3;
		}
		set
		{
			m_Item3 = value;
		}
	}

	public SimpleTuple(T1 item1, T2 item2, T3 item3)
	{
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
	}

	public override string ToString()
	{
		return $"{item1.ToString()}, {item2.ToString()}, {item3.ToString()}";
	}
}
