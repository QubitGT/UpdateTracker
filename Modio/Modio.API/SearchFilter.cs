using System.Collections.Generic;

namespace Modio.API;

public abstract class SearchFilter<T> : SearchFilter where T : SearchFilter<T>
{
	protected SearchFilter(int pageIndex, int pageSize)
		: base(pageIndex, pageSize)
	{
	}

	public T SetPagination(int pageIndex, int pageSize = 100)
	{
		PageIndex = pageIndex;
		PageSize = pageSize;
		return this as T;
	}
}
public abstract class SearchFilter
{
	internal int PageIndex;

	internal int PageSize;

	internal readonly Dictionary<string, object> Parameters;

	protected SearchFilter(int pageIndex, int pageSize)
	{
		Parameters = new Dictionary<string, object>();
		PageIndex = pageIndex;
		PageSize = pageSize;
	}
}
