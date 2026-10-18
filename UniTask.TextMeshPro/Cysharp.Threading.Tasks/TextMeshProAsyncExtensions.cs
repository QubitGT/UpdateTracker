using System;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Cysharp.Threading.Tasks;

public static class TextMeshProAsyncExtensions
{
	public static void BindTo(this IUniTaskAsyncEnumerable<string> source, TMP_Text text, bool rebindOnError = true)
	{
		BindToCore(source, text, ((MonoBehaviour)(object)text).GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	public static void BindTo(this IUniTaskAsyncEnumerable<string> source, TMP_Text text, CancellationToken cancellationToken, bool rebindOnError = true)
	{
		BindToCore(source, text, cancellationToken, rebindOnError).Forget();
	}

	private static async UniTaskVoid BindToCore(IUniTaskAsyncEnumerable<string> source, TMP_Text text, CancellationToken cancellationToken, bool rebindOnError)
	{
		bool repeat = false;
		while (true)
		{
			IL_0018:
			IUniTaskAsyncEnumerator<string> e = source.GetAsyncEnumerator(cancellationToken);
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
							break;
						}
						if (rebindOnError && !repeat)
						{
							repeat = true;
							goto IL_0018;
						}
						throw;
					}
					if (flag)
					{
						text.text = e.Current;
						continue;
					}
					break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			break;
		}
		int num;
		if (num != 2)
		{
		}
	}

	public static void BindTo<T>(this IUniTaskAsyncEnumerable<T> source, TMP_Text text, bool rebindOnError = true)
	{
		BindToCore(source, text, ((MonoBehaviour)(object)text).GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	public static void BindTo<T>(this IUniTaskAsyncEnumerable<T> source, TMP_Text text, CancellationToken cancellationToken, bool rebindOnError = true)
	{
		BindToCore(source, text, cancellationToken, rebindOnError).Forget();
	}

	public static void BindTo<T>(this AsyncReactiveProperty<T> source, TMP_Text text, bool rebindOnError = true)
	{
		BindToCore(source, text, ((MonoBehaviour)(object)text).GetCancellationTokenOnDestroy(), rebindOnError).Forget();
	}

	private static async UniTaskVoid BindToCore<T>(IUniTaskAsyncEnumerable<T> source, TMP_Text text, CancellationToken cancellationToken, bool rebindOnError)
	{
		bool repeat = false;
		while (true)
		{
			IL_0018:
			IUniTaskAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken);
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
							break;
						}
						if (rebindOnError && !repeat)
						{
							repeat = true;
							goto IL_0018;
						}
						throw;
					}
					if (flag)
					{
						text.text = e.Current.ToString();
						continue;
					}
					break;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			break;
		}
		int num;
		if (num != 2)
		{
		}
	}

	public static IAsyncValueChangedEventHandler<string> GetAsyncValueChangedEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onValueChanged, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncValueChangedEventHandler<string> GetAsyncValueChangedEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onValueChanged, cancellationToken, callOnce: false);
	}

	public static UniTask<string> OnValueChangedAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onValueChanged, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<string> OnValueChangedAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onValueChanged, cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<string> OnValueChangedAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onValueChanged, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<string> OnValueChangedAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onValueChanged, cancellationToken);
	}

	public static IAsyncEndEditEventHandler<string> GetAsyncEndEditEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onEndEdit, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncEndEditEventHandler<string> GetAsyncEndEditEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onEndEdit, cancellationToken, callOnce: false);
	}

	public static UniTask<string> OnEndEditAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onEndEdit, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<string> OnEndEditAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onEndEdit, cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<string> OnEndEditAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onEndEdit, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<string> OnEndEditAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onEndEdit, cancellationToken);
	}

	public static IAsyncEndTextSelectionEventHandler<(string, int, int)> GetAsyncEndTextSelectionEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onEndTextSelection), ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncEndTextSelectionEventHandler<(string, int, int)> GetAsyncEndTextSelectionEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onEndTextSelection), cancellationToken, callOnce: false);
	}

	public static UniTask<(string, int, int)> OnEndTextSelectionAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onEndTextSelection), ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<(string, int, int)> OnEndTextSelectionAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onEndTextSelection), cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<(string, int, int)> OnEndTextSelectionAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<(string, int, int)>(new TextSelectionEventConverter(inputField.onEndTextSelection), ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<(string, int, int)> OnEndTextSelectionAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<(string, int, int)>(new TextSelectionEventConverter(inputField.onEndTextSelection), cancellationToken);
	}

	public static IAsyncTextSelectionEventHandler<(string, int, int)> GetAsyncTextSelectionEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onTextSelection), ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncTextSelectionEventHandler<(string, int, int)> GetAsyncTextSelectionEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onTextSelection), cancellationToken, callOnce: false);
	}

	public static UniTask<(string, int, int)> OnTextSelectionAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onTextSelection), ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<(string, int, int)> OnTextSelectionAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<(string, int, int)>(new TextSelectionEventConverter(inputField.onTextSelection), cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<(string, int, int)> OnTextSelectionAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<(string, int, int)>(new TextSelectionEventConverter(inputField.onTextSelection), ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<(string, int, int)> OnTextSelectionAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<(string, int, int)>(new TextSelectionEventConverter(inputField.onTextSelection), cancellationToken);
	}

	public static IAsyncDeselectEventHandler<string> GetAsyncDeselectEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onDeselect, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncDeselectEventHandler<string> GetAsyncDeselectEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onDeselect, cancellationToken, callOnce: false);
	}

	public static UniTask<string> OnDeselectAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onDeselect, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<string> OnDeselectAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onDeselect, cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<string> OnDeselectAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onDeselect, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<string> OnDeselectAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onDeselect, cancellationToken);
	}

	public static IAsyncSelectEventHandler<string> GetAsyncSelectEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSelect, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncSelectEventHandler<string> GetAsyncSelectEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSelect, cancellationToken, callOnce: false);
	}

	public static UniTask<string> OnSelectAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSelect, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<string> OnSelectAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSelect, cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<string> OnSelectAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onSelect, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<string> OnSelectAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onSelect, cancellationToken);
	}

	public static IAsyncSubmitEventHandler<string> GetAsyncSubmitEventHandler(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSubmit, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: false);
	}

	public static IAsyncSubmitEventHandler<string> GetAsyncSubmitEventHandler(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSubmit, cancellationToken, callOnce: false);
	}

	public static UniTask<string> OnSubmitAsync(this TMP_InputField inputField)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSubmit, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy(), callOnce: true).OnInvokeAsync();
	}

	public static UniTask<string> OnSubmitAsync(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new AsyncUnityEventHandler<string>(inputField.onSubmit, cancellationToken, callOnce: true).OnInvokeAsync();
	}

	public static IUniTaskAsyncEnumerable<string> OnSubmitAsAsyncEnumerable(this TMP_InputField inputField)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onSubmit, ((MonoBehaviour)(object)inputField).GetCancellationTokenOnDestroy());
	}

	public static IUniTaskAsyncEnumerable<string> OnSubmitAsAsyncEnumerable(this TMP_InputField inputField, CancellationToken cancellationToken)
	{
		return new UnityEventHandlerAsyncEnumerable<string>(inputField.onSubmit, cancellationToken);
	}
}
