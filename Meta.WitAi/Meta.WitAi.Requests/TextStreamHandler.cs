using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Meta.WitAi.Requests;

internal class TextStreamHandler : DownloadHandlerScript, IVRequestDownloadDecoder
{
	public delegate void TextStreamResponseDelegate(string rawText);

	private TextStreamResponseDelegate _partialResponseDelegate;

	private string _partialDelimiter = "\r\n";

	public const string DEFAULT_PARTIAL_DELIMITER = "\r\n";

	private string _finalDelimiter = "\n";

	public const string DEFAULT_FINAL_DELIMITER = "\n";

	private StringBuilder _partialBuilder = new StringBuilder();

	private StringBuilder _finalBuilder = new StringBuilder();

	private int _finalLength;

	public bool IsStarted { get; private set; }

	public float Progress { get; private set; }

	public bool IsComplete { get; private set; }

	public TaskCompletionSource<bool> Completion { get; } = new TaskCompletionSource<bool>();


	public event VRequestResponseDelegate OnFirstResponse;

	public event VRequestResponseDelegate OnResponse;

	public event VRequestProgressDelegate OnProgress;

	[Preserve]
	public TextStreamHandler(TextStreamResponseDelegate partialResponseDelegate, string partialDelimiter = "\r\n", string finalDelimiter = "\n")
	{
		_partialResponseDelegate = partialResponseDelegate;
		_partialDelimiter = partialDelimiter;
		_finalDelimiter = finalDelimiter;
	}

	[Preserve]
	protected override bool ReceiveData(byte[] receiveData, int dataLength)
	{
		if (!IsStarted)
		{
			IsStarted = true;
			this.OnFirstResponse?.Invoke();
		}
		this.OnResponse?.Invoke();
		string[] array = SplitText(DecodeBytes(receiveData, 0, dataLength), _partialDelimiter);
		for (int i = 0; i < array.Length; i++)
		{
			string value = array[i];
			if (!string.IsNullOrEmpty(value))
			{
				_partialBuilder.Append(value);
				if (i < array.Length - 1)
				{
					HandlePartial(_partialBuilder.ToString());
					_partialBuilder.Clear();
				}
			}
		}
		RefreshProgress();
		return true;
	}

	protected virtual void HandlePartial(string newPartial)
	{
		_partialResponseDelegate?.Invoke(newPartial);
		if (_finalBuilder.Length > 0)
		{
			_finalBuilder.Append(_finalDelimiter);
		}
		_finalBuilder.Append(newPartial);
	}

	[Preserve]
	protected override string GetText()
	{
		return _finalBuilder.ToString() + ((_finalBuilder.Length > 0 && _partialBuilder.Length > 0) ? _finalDelimiter : "") + _partialBuilder.ToString();
	}

	[Preserve]
	protected override void ReceiveContentLengthHeader(ulong contentLength)
	{
		base.ReceiveContentLengthHeader(contentLength);
		_finalLength = GetDecodedLength(contentLength);
	}

	private void RefreshProgress()
	{
		if (_finalLength > 0)
		{
			float progress = GetProgress();
			if (!Progress.Equals(progress))
			{
				Progress = progress;
				this.OnProgress?.Invoke(progress);
			}
		}
	}

	[Preserve]
	protected override float GetProgress()
	{
		if (IsComplete)
		{
			return 1f;
		}
		if (_finalLength > 0)
		{
			return (float)(_partialBuilder.Length + _finalBuilder.Length) / (float)_finalLength;
		}
		return 0f;
	}

	[Preserve]
	protected override byte[] GetData()
	{
		return Encoding.UTF8.GetBytes(_finalBuilder.ToString());
	}

	[Preserve]
	protected override void CompleteContent()
	{
		if (_partialBuilder.Length > 0)
		{
			HandlePartial(_partialBuilder.ToString());
			_partialBuilder.Clear();
		}
		IsComplete = true;
		Completion.TrySetResult(result: true);
	}

	public static string DecodeBytes(byte[] receiveData, int start, int length)
	{
		return Encoding.UTF8.GetString(receiveData, start, length);
	}

	public static int GetDecodedLength(ulong totalBits)
	{
		return (int)(totalBits / 8);
	}

	public static string[] SplitText(string source, string delimiter)
	{
		return source.Split(delimiter);
	}

	internal bool ReceiveData(byte[] receiveData)
	{
		return ReceiveData(receiveData, receiveData.Length);
	}

	internal void Complete()
	{
		CompleteContent();
	}
}
