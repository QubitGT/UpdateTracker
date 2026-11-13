using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq;

internal static class SequenceEqual
{
	internal static async UniTask<bool> SequenceEqualAsync<TSource>(IUniTaskAsyncEnumerable<TSource> first, IUniTaskAsyncEnumerable<TSource> second, IEqualityComparer<TSource> comparer, CancellationToken cancellationToken)
	{
		IUniTaskAsyncEnumerator<TSource> e1 = first.GetAsyncEnumerator(cancellationToken);
		object obj = null;
		int num = 0;
		bool result = default(bool);
		object obj4;
		try
		{
			IUniTaskAsyncEnumerator<TSource> e2 = second.GetAsyncEnumerator(cancellationToken);
			object obj2 = null;
			int num2 = 0;
			bool flag = default(bool);
			try
			{
				while (true)
				{
					if (await e1.MoveNextAsync())
					{
						if (await e2.MoveNextAsync())
						{
							if (!comparer.Equals(e1.Current, e2.Current))
							{
								flag = false;
								break;
							}
							continue;
						}
						flag = false;
						break;
					}
					flag = !(await e2.MoveNextAsync());
					break;
				}
				num2 = 1;
			}
			catch (object obj3)
			{
				obj2 = obj3;
			}
			if (e2 != null)
			{
				await e2.DisposeAsync();
			}
			obj4 = obj2;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num2 == 1)
			{
				result = flag;
				num = 1;
			}
		}
		catch (object obj3)
		{
			obj = obj3;
		}
		if (e1 != null)
		{
			await e1.DisposeAsync();
		}
		obj4 = obj;
		if (obj4 != null)
		{
			ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		bool result2 = default(bool);
		return result2;
	}
}
