using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace g3;

public class gParallel
{
	public static void ForEach_Sequential<T>(IEnumerable<T> source, Action<T> body)
	{
		foreach (T item in source)
		{
			body(item);
		}
	}

	public static void ForEach<T>(IEnumerable<T> source, Action<T> body)
	{
		Parallel.ForEach(source, body);
	}

	public static void Evaluate(params Action[] funcs)
	{
		ForEach(Interval1i.Range(funcs.Length), delegate(int i)
		{
			funcs[i]();
		});
	}

	public static void BlockStartEnd(int iStart, int iEnd, Action<int, int> blockF, int iBlockSize = -1, bool bDisableParallel = false)
	{
		if (iBlockSize == -1)
		{
			iBlockSize = 100;
		}
		int num = iEnd - iStart + 1;
		int num2 = num / iBlockSize;
		if (bDisableParallel)
		{
			ForEach_Sequential(Interval1i.Range(num2), delegate(int bi)
			{
				int num6 = iStart + iBlockSize * bi;
				blockF(num6, num6 + iBlockSize - 1);
			});
		}
		else
		{
			ForEach(Interval1i.Range(num2), delegate(int bi)
			{
				int num5 = iStart + iBlockSize * bi;
				blockF(num5, num5 + iBlockSize - 1);
			});
		}
		int num3 = num - num2 * iBlockSize;
		if (num3 > 0)
		{
			int num4 = iStart + num2 * iBlockSize;
			blockF(num4, num4 + num3 - 1);
		}
	}

	private static void for_each<T>(IEnumerable<T> source, Action<T> body)
	{
		int remainingWorkItems;
		int num = (remainingWorkItems = Environment.ProcessorCount);
		Exception last_exception = null;
		IEnumerator<T> enumerator = source.GetEnumerator();
		try
		{
			ManualResetEvent mre = new ManualResetEvent(initialState: false);
			try
			{
				for (int i = 0; i < num; i++)
				{
					ThreadPool.QueueUserWorkItem(delegate
					{
						while (true)
						{
							T current;
							lock (enumerator)
							{
								if (enumerator.MoveNext())
								{
									current = enumerator.Current;
									goto IL_0039;
								}
							}
							break;
							IL_0039:
							try
							{
								body(current);
							}
							catch (Exception ex)
							{
								last_exception = ex;
								break;
							}
						}
						if (Interlocked.Decrement(ref remainingWorkItems) == 0)
						{
							mre.Set();
						}
					});
				}
				mre.WaitOne();
			}
			finally
			{
				if (mre != null)
				{
					((IDisposable)mre).Dispose();
				}
			}
		}
		finally
		{
			if (enumerator != null)
			{
				enumerator.Dispose();
			}
		}
		if (last_exception != null)
		{
			throw last_exception;
		}
	}
}
