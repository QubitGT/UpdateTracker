using System;
using System.Collections.Generic;
using System.Threading;

namespace Photon.Voice;

public class LocalVoiceFramed<T> : LocalVoiceFramedBase
{
	private Framer<T> framer;

	private int preProcessorsCnt;

	private List<IProcessor<T>> processors = new List<IProcessor<T>>();

	private bool dataEncodeThreadStarted;

	private Queue<T[]> pushDataQueue = new Queue<T[]>();

	private AutoResetEvent pushDataQueueReady = new AutoResetEvent(initialState: false);

	private FactoryPrimitiveArrayPool<T> bufferFactory;

	private int framesSkippedNextLog;

	private int framesSkipped;

	private bool exitThread;

	private int processNullFramesCnt;

	public FactoryPrimitiveArrayPool<T> BufferFactory => bufferFactory;

	public bool PushDataAsyncReady
	{
		get
		{
			lock (pushDataQueue)
			{
				return pushDataQueue.Count < 49;
			}
		}
	}

	protected T[] processFrame(T[] buf)
	{
		lock (processors)
		{
			foreach (IProcessor<T> processor in processors)
			{
				buf = processor.Process(buf);
				if (buf == null)
				{
					break;
				}
			}
		}
		return buf;
	}

	public void AddPostProcessor(params IProcessor<T>[] processors)
	{
		lock (this.processors)
		{
			foreach (IProcessor<T> item in processors)
			{
				this.processors.Add(item);
			}
		}
	}

	public void AddPreProcessor(params IProcessor<T>[] processors)
	{
		lock (this.processors)
		{
			foreach (IProcessor<T> item in processors)
			{
				this.processors.Insert(preProcessorsCnt++, item);
			}
		}
	}

	public void ClearProcessors()
	{
		lock (processors)
		{
			processors.Clear();
			preProcessorsCnt = 0;
		}
	}

	internal LocalVoiceFramed(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, int channelId, int frameSize)
		: base(voiceClient, encoder, id, voiceInfo, channelId, frameSize)
	{
		if (frameSize == 0)
		{
			throw new Exception(base.LogPrefix + ": non 0 frame size required for framed stream");
		}
		framer = new Framer<T>(base.FrameSize);
		bufferFactory = new FactoryPrimitiveArrayPool<T>(50, base.Name + " Data", base.FrameSize);
	}

	public void PushDataAsync(T[] buf)
	{
		if (disposed)
		{
			return;
		}
		if (!dataEncodeThreadStarted)
		{
			voiceClient.logger.LogInfo(base.LogPrefix + ": Starting data encode thread");
			Thread thread = new Thread(PushDataAsyncThread);
			thread.Start();
			Util.SetThreadName(thread, "[PV] EncData " + base.shortName);
			dataEncodeThreadStarted = true;
		}
		if (PushDataAsyncReady)
		{
			lock (pushDataQueue)
			{
				pushDataQueue.Enqueue(buf);
			}
			pushDataQueueReady.Set();
			return;
		}
		bufferFactory.Free(buf, buf.Length);
		if (framesSkipped == framesSkippedNextLog)
		{
			voiceClient.logger.LogWarning(base.LogPrefix + ": PushData queue overflow. Frames skipped: " + (framesSkipped + 1));
			framesSkippedNextLog = framesSkipped + 10;
		}
		framesSkipped++;
	}

	private void PushDataAsyncThread()
	{
		try
		{
			while (!exitThread)
			{
				pushDataQueueReady.WaitOne();
				while (!exitThread)
				{
					T[] array = null;
					lock (pushDataQueue)
					{
						if (pushDataQueue.Count > 0)
						{
							array = pushDataQueue.Dequeue();
						}
					}
					if (array == null)
					{
						break;
					}
					PushData(array);
					bufferFactory.Free(array, array.Length);
				}
			}
		}
		catch (Exception ex)
		{
			voiceClient.logger.LogError(base.LogPrefix + ": Exception in encode thread: " + ex);
			throw ex;
		}
		finally
		{
			Dispose();
			bufferFactory.Dispose();
			pushDataQueueReady.Close();
			voiceClient.logger.LogInfo(base.LogPrefix + ": Exiting data encode thread");
		}
	}

	public void PushData(T[] buf)
	{
		if (!voiceClient.transport.IsChannelJoined(channelId) || !base.TransmitEnabled)
		{
			return;
		}
		if (encoder is IEncoderDirect<T[]>)
		{
			lock (disposeLock)
			{
				if (!disposed)
				{
					foreach (T[] item in framer.Frame(buf))
					{
						T[] array = processFrame(item);
						if (array != null)
						{
							processNullFramesCnt = 0;
							((IEncoderDirect<T[]>)encoder).Input(array);
						}
						else
						{
							processNullFramesCnt++;
							if (processNullFramesCnt == 1)
							{
								encoder.EndOfStream();
							}
						}
					}
					return;
				}
				return;
			}
		}
		throw new Exception(base.LogPrefix + ": PushData(T[]) called on encoder of unsupported type " + ((encoder == null) ? "null" : encoder.GetType().ToString()));
	}

	public override void Dispose()
	{
		exitThread = true;
		lock (disposeLock)
		{
			if (!disposed)
			{
				lock (processors)
				{
					foreach (IProcessor<T> processor in processors)
					{
						processor.Dispose();
					}
				}
				base.Dispose();
				pushDataQueueReady.Set();
			}
		}
		base.Dispose();
	}
}
