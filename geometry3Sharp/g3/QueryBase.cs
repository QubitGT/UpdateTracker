namespace g3;

public class QueryBase
{
	public bool Sort(ref int v0, ref int v1)
	{
		int key;
		int key2;
		bool result;
		if (v0 < v1)
		{
			key = 0;
			key2 = 1;
			result = true;
		}
		else
		{
			key = 1;
			key2 = 0;
			result = false;
		}
		Index2i index2i = new Index2i(v0, v1);
		v0 = index2i[key];
		v1 = index2i[key2];
		return result;
	}

	public bool Sort(ref int v0, ref int v1, ref int v2)
	{
		int key;
		int key2;
		int key3;
		bool result;
		if (v0 < v1)
		{
			if (v2 < v0)
			{
				key = 2;
				key2 = 0;
				key3 = 1;
				result = true;
			}
			else if (v2 < v1)
			{
				key = 0;
				key2 = 2;
				key3 = 1;
				result = false;
			}
			else
			{
				key = 0;
				key2 = 1;
				key3 = 2;
				result = true;
			}
		}
		else if (v2 < v1)
		{
			key = 2;
			key2 = 1;
			key3 = 0;
			result = false;
		}
		else if (v2 < v0)
		{
			key = 1;
			key2 = 2;
			key3 = 0;
			result = true;
		}
		else
		{
			key = 1;
			key2 = 0;
			key3 = 2;
			result = false;
		}
		Index3i index3i = new Index3i(v0, v1, v2);
		v0 = index3i[key];
		v1 = index3i[key2];
		v2 = index3i[key3];
		return result;
	}

	public bool Sort(ref int v0, ref int v1, ref int v2, ref int v3)
	{
		int key;
		int key2;
		int key3;
		int key4;
		bool result;
		if (v0 < v1)
		{
			if (v2 < v3)
			{
				if (v1 < v2)
				{
					key = 0;
					key2 = 1;
					key3 = 2;
					key4 = 3;
					result = true;
				}
				else if (v3 < v0)
				{
					key = 2;
					key2 = 3;
					key3 = 0;
					key4 = 1;
					result = true;
				}
				else if (v2 < v0)
				{
					if (v3 < v1)
					{
						key = 2;
						key2 = 0;
						key3 = 3;
						key4 = 1;
						result = false;
					}
					else
					{
						key = 2;
						key2 = 0;
						key3 = 1;
						key4 = 3;
						result = true;
					}
				}
				else if (v3 < v1)
				{
					key = 0;
					key2 = 2;
					key3 = 3;
					key4 = 1;
					result = true;
				}
				else
				{
					key = 0;
					key2 = 2;
					key3 = 1;
					key4 = 3;
					result = false;
				}
			}
			else if (v1 < v3)
			{
				key = 0;
				key2 = 1;
				key3 = 3;
				key4 = 2;
				result = false;
			}
			else if (v2 < v0)
			{
				key = 3;
				key2 = 2;
				key3 = 0;
				key4 = 1;
				result = false;
			}
			else if (v3 < v0)
			{
				if (v2 < v1)
				{
					key = 3;
					key2 = 0;
					key3 = 2;
					key4 = 1;
					result = true;
				}
				else
				{
					key = 3;
					key2 = 0;
					key3 = 1;
					key4 = 2;
					result = false;
				}
			}
			else if (v2 < v1)
			{
				key = 0;
				key2 = 3;
				key3 = 2;
				key4 = 1;
				result = false;
			}
			else
			{
				key = 0;
				key2 = 3;
				key3 = 1;
				key4 = 2;
				result = true;
			}
		}
		else if (v2 < v3)
		{
			if (v0 < v2)
			{
				key = 1;
				key2 = 0;
				key3 = 2;
				key4 = 3;
				result = false;
			}
			else if (v3 < v1)
			{
				key = 2;
				key2 = 3;
				key3 = 1;
				key4 = 0;
				result = false;
			}
			else if (v2 < v1)
			{
				if (v3 < v0)
				{
					key = 2;
					key2 = 1;
					key3 = 3;
					key4 = 0;
					result = true;
				}
				else
				{
					key = 2;
					key2 = 1;
					key3 = 0;
					key4 = 3;
					result = false;
				}
			}
			else if (v3 < v0)
			{
				key = 1;
				key2 = 2;
				key3 = 3;
				key4 = 0;
				result = false;
			}
			else
			{
				key = 1;
				key2 = 2;
				key3 = 0;
				key4 = 3;
				result = true;
			}
		}
		else if (v0 < v3)
		{
			key = 1;
			key2 = 0;
			key3 = 3;
			key4 = 2;
			result = true;
		}
		else if (v2 < v1)
		{
			key = 3;
			key2 = 2;
			key3 = 1;
			key4 = 0;
			result = true;
		}
		else if (v3 < v1)
		{
			if (v2 < v0)
			{
				key = 3;
				key2 = 1;
				key3 = 2;
				key4 = 0;
				result = false;
			}
			else
			{
				key = 3;
				key2 = 1;
				key3 = 0;
				key4 = 2;
				result = true;
			}
		}
		else if (v2 < v0)
		{
			key = 1;
			key2 = 3;
			key3 = 2;
			key4 = 0;
			result = true;
		}
		else
		{
			key = 1;
			key2 = 3;
			key3 = 0;
			key4 = 2;
			result = false;
		}
		Index4i index4i = new Index4i(v0, v1, v2, v3);
		v0 = index4i[key];
		v1 = index4i[key2];
		v2 = index4i[key3];
		v3 = index4i[key4];
		return result;
	}
}
