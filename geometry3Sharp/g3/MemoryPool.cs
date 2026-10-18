namespace g3;

public class MemoryPool<T> where T : class, new()
{
	private DVector<T> Allocated;

	private DVector<T> Free;

	public MemoryPool()
	{
		Allocated = new DVector<T>();
		Free = new DVector<T>();
	}

	public T Allocate()
	{
		if (Free.size > 0)
		{
			T result = Free[Free.size - 1];
			Free.pop_back();
			return result;
		}
		T val = new T();
		Allocated.Add(val);
		return val;
	}

	public void Return(T obj)
	{
		Free.Add(obj);
	}

	public void ReturnAll()
	{
		Free = new DVector<T>(Allocated);
	}

	public void FreeAll()
	{
		Allocated = new DVector<T>();
		Free = new DVector<T>();
	}
}
