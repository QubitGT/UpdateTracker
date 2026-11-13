using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Cysharp.Threading.Tasks;

public static class UnityBindingExtensions
{
	public static void BindTo(this IUniTaskAsyncEnumerable<string> source, Text text, bool rebindOnError = true)
	{
		BindToCore(source, text, text.GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	public static void BindTo(this IUniTaskAsyncEnumerable<string> source, Text text, CancellationToken cancellationToken, bool rebindOnError = true)
	{
		BindToCore(source, text, cancellationToken, rebindOnError).Forget();
	}

	private static async UniTaskVoid BindToCore(IUniTaskAsyncEnumerable<string> source, Text text, CancellationToken cancellationToken, bool rebindOnError)
	{
		bool repeat = false;
		object obj3;
		while (true)
		{
			IUniTaskAsyncEnumerator<string> e = source.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					bool flag;
					try
					{
						flag = await e.MoveNextAsync();
						repeat = false;
					}
					catch (Exception ex)
					{
						if (ex is OperationCanceledException)
						{
							goto IL_00fc;
						}
						if (rebindOnError && !repeat)
						{
							repeat = true;
							goto IL_00f3;
						}
						throw;
					}
					if (flag)
					{
						text.text = e.Current;
						continue;
					}
					goto IL_00fc;
					IL_00f3:
					num = 1;
					break;
					IL_00fc:
					num = 2;
					break;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			obj3 = obj;
			if (obj3 != null)
			{
				Exception obj4 = obj3 as Exception;
				if (obj4 == null)
				{
					break;
				}
				ExceptionDispatchInfo.Capture(obj4).Throw();
			}
			switch (num)
			{
			case 1:
				break;
			default:
				return;
			}
		}
		throw obj3;
	}

	public static void BindTo<T>(this IUniTaskAsyncEnumerable<T> source, Text text, bool rebindOnError = true)
	{
		BindToCore(source, text, text.GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	public static void BindTo<T>(this IUniTaskAsyncEnumerable<T> source, Text text, CancellationToken cancellationToken, bool rebindOnError = true)
	{
		BindToCore(source, text, cancellationToken, rebindOnError).Forget();
	}

	public static void BindTo<T>(this AsyncReactiveProperty<T> source, Text text, bool rebindOnError = true)
	{
		BindToCore(source, text, text.GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	private static async UniTaskVoid BindToCore<T>(IUniTaskAsyncEnumerable<T> source, Text text, CancellationToken cancellationToken, bool rebindOnError)
	{
		bool repeat = false;
		object obj3;
		while (true)
		{
			IUniTaskAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					bool flag;
					try
					{
						flag = await e.MoveNextAsync();
						repeat = false;
					}
					catch (Exception ex)
					{
						if (ex is OperationCanceledException)
						{
							goto IL_010b;
						}
						if (rebindOnError && !repeat)
						{
							repeat = true;
							goto IL_0102;
						}
						throw;
					}
					if (flag)
					{
						text.text = e.Current.ToString();
						continue;
					}
					goto IL_010b;
					IL_0102:
					num = 1;
					break;
					IL_010b:
					num = 2;
					break;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			obj3 = obj;
			if (obj3 != null)
			{
				Exception obj4 = obj3 as Exception;
				if (obj4 == null)
				{
					break;
				}
				ExceptionDispatchInfo.Capture(obj4).Throw();
			}
			switch (num)
			{
			case 1:
				break;
			default:
				return;
			}
		}
		throw obj3;
	}

	public static void BindTo(this IUniTaskAsyncEnumerable<bool> source, Selectable selectable, bool rebindOnError = true)
	{
		BindToCore(source, selectable, selectable.GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	public static void BindTo(this IUniTaskAsyncEnumerable<bool> source, Selectable selectable, CancellationToken cancellationToken, bool rebindOnError = true)
	{
		BindToCore(source, selectable, cancellationToken, rebindOnError).Forget();
	}

	private static async UniTaskVoid BindToCore(IUniTaskAsyncEnumerable<bool> source, Selectable selectable, CancellationToken cancellationToken, bool rebindOnError)
	{
		bool repeat = false;
		object obj3;
		while (true)
		{
			IUniTaskAsyncEnumerator<bool> e = source.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					bool flag;
					try
					{
						flag = await e.MoveNextAsync();
						repeat = false;
					}
					catch (Exception ex)
					{
						if (ex is OperationCanceledException)
						{
							goto IL_00fc;
						}
						if (rebindOnError && !repeat)
						{
							repeat = true;
							goto IL_00f3;
						}
						throw;
					}
					if (flag)
					{
						selectable.interactable = e.Current;
						continue;
					}
					goto IL_00fc;
					IL_00f3:
					num = 1;
					break;
					IL_00fc:
					num = 2;
					break;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			obj3 = obj;
			if (obj3 != null)
			{
				Exception obj4 = obj3 as Exception;
				if (obj4 == null)
				{
					break;
				}
				ExceptionDispatchInfo.Capture(obj4).Throw();
			}
			switch (num)
			{
			case 1:
				break;
			default:
				return;
			}
		}
		throw obj3;
	}

	public static void BindTo<TSource, TObject>(this IUniTaskAsyncEnumerable<TSource> source, TObject monoBehaviour, Action<TObject, TSource> bindAction, bool rebindOnError = true) where TObject : MonoBehaviour
	{
		BindToCore(source, monoBehaviour, bindAction, monoBehaviour.GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	public static void BindTo<TSource, TObject>(this IUniTaskAsyncEnumerable<TSource> source, TObject bindTarget, Action<TObject, TSource> bindAction, CancellationToken cancellationToken, bool rebindOnError = true)
	{
		BindToCore(source, bindTarget, bindAction, cancellationToken, rebindOnError).Forget();
	}

	private static async UniTaskVoid BindToCore<TSource, TObject>(IUniTaskAsyncEnumerable<TSource> source, TObject bindTarget, Action<TObject, TSource> bindAction, CancellationToken cancellationToken, bool rebindOnError)
	{
		bool repeat = false;
		object obj3;
		while (true)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			object obj = null;
			int num = 0;
			try
			{
				while (true)
				{
					bool flag;
					try
					{
						flag = await e.MoveNextAsync();
						repeat = false;
					}
					catch (Exception ex)
					{
						if (ex is OperationCanceledException)
						{
							goto IL_0102;
						}
						if (rebindOnError && !repeat)
						{
							repeat = true;
							goto IL_00f9;
						}
						throw;
					}
					if (flag)
					{
						bindAction(bindTarget, e.Current);
						continue;
					}
					goto IL_0102;
					IL_00f9:
					num = 1;
					break;
					IL_0102:
					num = 2;
					break;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if (e != null)
			{
				await e.DisposeAsync();
			}
			obj3 = obj;
			if (obj3 != null)
			{
				Exception obj4 = obj3 as Exception;
				if (obj4 == null)
				{
					break;
				}
				ExceptionDispatchInfo.Capture(obj4).Throw();
			}
			switch (num)
			{
			case 1:
				break;
			default:
				return;
			}
		}
		throw obj3;
	}
}
