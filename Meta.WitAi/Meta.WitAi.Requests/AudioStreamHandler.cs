using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Audio.Decoding;
using Meta.Voice.Logging;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Meta.WitAi.Requests;

[Preserve]
[LogCategory(LogCategory.Audio, LogCategory.Output)]
internal class AudioStreamHandler : DownloadHandlerScript, IVRequestDownloadDecoder, ILogSource
{
	private const int BUFFER_LENGTH = 24000;

	private static readonly ArrayPool<byte> _bufferPool = new ArrayPool<byte>(24000);

	private readonly Queue<byte[]> _buffers;

	private byte[] _inBuffer;

	private int _inBufferOffset;

	private byte[] _decodeBuffer;

	private int _decodeBufferOffset;

	private ulong _expectedBytes;

	private ulong _receivedBytes;

	private ulong _decodedBytes;

	private bool _requestComplete;

	private Task _decoder;

	private bool _unloaded;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Output);


	public bool IsStarted { get; private set; }

	public float Progress { get; private set; }

	public bool IsComplete { get; private set; }

	public TaskCompletionSource<bool> Completion { get; } = new TaskCompletionSource<bool>();


	public bool IsError { get; private set; }

	public IAudioDecoder AudioDecoder { get; }

	public bool WillDecodeInBackground => AudioDecoder.WillDecodeInBackground;

	public AudioSampleDecodeDelegate OnSamplesDecoded { get; }

	private bool _decodeComplete => _decodedBytes == Max(_receivedBytes, _expectedBytes);

	public event VRequestResponseDelegate OnFirstResponse;

	public event VRequestResponseDelegate OnResponse;

	public event VRequestProgressDelegate OnProgress;

	private ulong Max(ulong var1, ulong var2)
	{
		if (var1 <= var2)
		{
			return var2;
		}
		return var1;
	}

	public AudioStreamHandler(IAudioDecoder audioDecoder, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		AudioDecoder = audioDecoder;
		OnSamplesDecoded = onSamplesDecoded;
		if (WillDecodeInBackground)
		{
			_buffers = new Queue<byte[]>();
		}
	}

	~AudioStreamHandler()
	{
		UnloadBuffers();
	}

	[Preserve]
	protected override void ReceiveContentLengthHeader(ulong contentLength)
	{
		if (contentLength != 0L && !IsComplete)
		{
			_expectedBytes = contentLength;
			IsError = _expectedBytes < 2400;
		}
	}

	[Preserve]
	protected override bool ReceiveData(byte[] bufferData, int length)
	{
		if (!base.ReceiveData(bufferData, length) || IsComplete)
		{
			return false;
		}
		if (!IsStarted)
		{
			IsStarted = true;
			this.OnFirstResponse?.Invoke();
		}
		this.OnResponse?.Invoke();
		if (WillDecodeInBackground)
		{
			EnqueueAndDecodeChunkAsync(bufferData, 0, length);
		}
		else
		{
			_receivedBytes += (uint)length;
			DecodeChunk(bufferData, 0, length);
			if (_decodeComplete)
			{
				TryToFinalize();
			}
		}
		return true;
	}

	private void EnqueueAndDecodeChunkAsync(byte[] chunk, int offset, int length)
	{
		while (length > 0)
		{
			if (_inBuffer == null)
			{
				_inBuffer = _bufferPool.Get();
				lock (_buffers)
				{
					_buffers.Enqueue(_inBuffer);
				}
			}
			int num = Mathf.Min(length, _inBuffer.Length - _inBufferOffset);
			Array.Copy(chunk, offset, _inBuffer, _inBufferOffset, num);
			offset += num;
			length -= num;
			_inBufferOffset += num;
			if (_inBufferOffset >= _inBuffer.Length)
			{
				_inBufferOffset = 0;
				_inBuffer = null;
			}
			_receivedBytes += (ulong)num;
		}
		if (_decoder == null)
		{
			_decoder = ThreadUtility.Background(Logger, DecodeAsync);
		}
	}

	private void DecodeAsync()
	{
		if (IsError)
		{
			_decodedBytes = _receivedBytes;
			ThreadUtility.CallOnMainThread(TryToFinalize).WrapErrors();
			return;
		}
		while (_decodedBytes < _receivedBytes)
		{
			if (_decodeBuffer == null)
			{
				lock (_buffers)
				{
					if (!_buffers.TryDequeue(out _decodeBuffer))
					{
						break;
					}
				}
			}
			int num = Mathf.Min((int)(_receivedBytes - _decodedBytes), _decodeBuffer.Length - _decodeBufferOffset);
			DecodeChunk(_decodeBuffer, _decodeBufferOffset, num);
			_decodeBufferOffset += num;
			if (_decodeBufferOffset >= _decodeBuffer.Length)
			{
				_decodeBufferOffset = 0;
				_bufferPool.Return(_decodeBuffer);
				_decodeBuffer = null;
			}
			RefreshProgress();
		}
		_decoder = null;
		if (_decodeComplete)
		{
			ThreadUtility.CallOnMainThread(TryToFinalize).WrapErrors();
		}
	}

	private void DecodeChunk(byte[] chunk, int offset, int length)
	{
		try
		{
			AudioDecoder.Decode(chunk, offset, length, OnSamplesDecoded);
		}
		catch (Exception ex)
		{
			Logger.Error("AudioStreamHandler Decode Failed\nException: {0}", ex);
		}
		finally
		{
			_decodedBytes += (ulong)length;
		}
	}

	[Preserve]
	protected override string GetText()
	{
		if (IsError && _inBuffer != null)
		{
			return Encoding.UTF8.GetString(_inBuffer, 0, _inBufferOffset);
		}
		return null;
	}

	private void RefreshProgress()
	{
		if (_expectedBytes != 0)
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
		if (_expectedBytes != 0)
		{
			return Mathf.Clamp01(_decodedBytes / _expectedBytes);
		}
		return 0f;
	}

	[Preserve]
	protected override void CompleteContent()
	{
		if (!_requestComplete)
		{
			_requestComplete = true;
			TryToFinalize();
		}
	}

	private void TryToFinalize()
	{
		if (!IsComplete && _requestComplete && _decodeComplete)
		{
			IsComplete = true;
			Dispose();
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		UnloadBuffers();
		IsComplete = true;
		Completion.TrySetResult(result: true);
	}

	private void UnloadBuffers()
	{
		if (_unloaded)
		{
			return;
		}
		_inBuffer = null;
		if (WillDecodeInBackground)
		{
			lock (_buffers)
			{
				byte[] result;
				while (_buffers.TryDequeue(out result))
				{
					_bufferPool.Return(result);
				}
			}
		}
		if (_decodeBuffer != null)
		{
			_bufferPool.Return(_decodeBuffer);
			_decodeBuffer = null;
		}
		_unloaded = true;
	}
}
