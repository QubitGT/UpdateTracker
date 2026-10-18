using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Photon.Voice;

public class AudioSyncBuffer<T> : IAudioOut<T>
{
	private int curPlayingFrameSamplePos;

	private int sampleRate;

	private int channels;

	private int frameSamples;

	private int frameSize;

	private bool started;

	private int maxDevPlayDelaySamples;

	private int targetPlayDelaySamples;

	private int playDelayMs;

	private readonly ILogger logger;

	private readonly string logPrefix;

	private readonly bool debugInfo;

	private readonly int elementSize = Marshal.SizeOf(typeof(T));

	private T[] emptyFrame;

	private Queue<T[]> frameQueue = new Queue<T[]>();

	public const int FRAME_POOL_CAPACITY = 50;

	private PrimitiveArrayPool<T> framePool = new PrimitiveArrayPool<T>(50, "AudioSyncBuffer");

	public int Lag
	{
		get
		{
			lock (this)
			{
				return (int)((float)frameQueue.Count * (float)frameSamples * 1000f / (float)sampleRate);
			}
		}
	}

	public bool IsPlaying
	{
		get
		{
			lock (this)
			{
				return started;
			}
		}
	}

	public AudioSyncBuffer(int playDelayMs, ILogger logger, string logPrefix, bool debugInfo)
	{
		this.playDelayMs = playDelayMs;
		this.logger = logger;
		this.logPrefix = logPrefix;
		this.debugInfo = debugInfo;
	}

	public void Start(int sampleRate, int channels, int frameSamples)
	{
		lock (this)
		{
			started = false;
			this.sampleRate = sampleRate;
			this.channels = channels;
			this.frameSamples = frameSamples;
			frameSize = frameSamples * channels;
			int num = playDelayMs * sampleRate / 1000 + frameSamples;
			maxDevPlayDelaySamples = num / 2;
			targetPlayDelaySamples = num + maxDevPlayDelaySamples;
			if (framePool.Info != frameSize)
			{
				framePool.Init(frameSize);
			}
			while (frameQueue.Count > 0)
			{
				dequeueFrameQueue();
			}
			emptyFrame = new T[frameSize];
			int num2 = targetPlayDelaySamples / this.frameSamples;
			curPlayingFrameSamplePos = targetPlayDelaySamples % this.frameSamples;
			while (frameQueue.Count < num2)
			{
				frameQueue.Enqueue(emptyFrame);
			}
			started = true;
		}
	}

	public void Service()
	{
	}

	public void Read(T[] outBuf, int outChannels, int outSampleRate)
	{
		lock (this)
		{
			if (!started)
			{
				return;
			}
			int num = 0;
			while ((frameQueue.Count * frameSamples - curPlayingFrameSamplePos) * channels * outSampleRate >= (outBuf.Length - num) * sampleRate)
			{
				int num2 = curPlayingFrameSamplePos * channels;
				T[] array = frameQueue.Peek();
				int num3 = outBuf.Length - num;
				int num4 = array.Length - num2;
				if (num4 * outChannels * outSampleRate > num3 * channels * sampleRate)
				{
					int num5 = num3 * channels * sampleRate / (outChannels * outSampleRate);
					if (sampleRate == outSampleRate && channels == outChannels)
					{
						Buffer.BlockCopy(array, num2 * elementSize, outBuf, num * elementSize, num3 * elementSize);
					}
					else
					{
						AudioUtil.Resample(array, num2, num5, channels, outBuf, num, num3, outChannels);
					}
					curPlayingFrameSamplePos += num5 / channels;
					break;
				}
				int num6 = num4 * outChannels * outSampleRate / (channels * sampleRate);
				if (sampleRate == outSampleRate && channels == outChannels)
				{
					Buffer.BlockCopy(array, num2 * elementSize, outBuf, num * elementSize, num4 * elementSize);
				}
				else
				{
					AudioUtil.Resample(array, num2, num4, channels, outBuf, num, num6, outChannels);
				}
				num += num6;
				curPlayingFrameSamplePos = 0;
				dequeueFrameQueue();
				if (num6 == num3)
				{
					break;
				}
			}
		}
	}

	public void Push(T[] frame)
	{
		lock (this)
		{
			if (!started || frame.Length == 0)
			{
				return;
			}
			if (frame.Length != frameSize)
			{
				logger.LogError("{0} AudioSyncBuffer audio frames are not of size: {1} != {2}", logPrefix, frame.Length, frameSize);
				return;
			}
			if (framePool.Info != frame.Length)
			{
				framePool.Init(frame.Length);
			}
			T[] array = framePool.AcquireOrCreate();
			Buffer.BlockCopy(frame, 0, array, 0, Buffer.ByteLength(frame));
			lock (this)
			{
				frameQueue.Enqueue(array);
				syncFrameQueue();
			}
		}
	}

	public void Flush()
	{
	}

	public void Stop()
	{
		lock (this)
		{
			started = false;
		}
	}

	private void dequeueFrameQueue()
	{
		T[] array = frameQueue.Dequeue();
		if (array != emptyFrame)
		{
			framePool.Release(array, array.Length);
		}
	}

	private void syncFrameQueue()
	{
		int num = frameQueue.Count * frameSamples - curPlayingFrameSamplePos;
		if (num > targetPlayDelaySamples + maxDevPlayDelaySamples)
		{
			int num2 = targetPlayDelaySamples / frameSamples;
			curPlayingFrameSamplePos = targetPlayDelaySamples % frameSamples;
			while (frameQueue.Count > num2)
			{
				dequeueFrameQueue();
			}
			if (debugInfo)
			{
				logger.LogWarning("{0} AudioSynctBuffer overrun {1} {2} {3} {4}", logPrefix, targetPlayDelaySamples - maxDevPlayDelaySamples, targetPlayDelaySamples + maxDevPlayDelaySamples, num, num2, curPlayingFrameSamplePos);
			}
		}
		else if (num < targetPlayDelaySamples - maxDevPlayDelaySamples)
		{
			int num3 = targetPlayDelaySamples / frameSamples;
			curPlayingFrameSamplePos = targetPlayDelaySamples % frameSamples;
			while (frameQueue.Count < num3)
			{
				frameQueue.Enqueue(emptyFrame);
			}
			if (debugInfo)
			{
				logger.LogWarning("{0} AudioSyncBuffer underrun {1} {2} {3} {4}", logPrefix, targetPlayDelaySamples - maxDevPlayDelaySamples, targetPlayDelaySamples + maxDevPlayDelaySamples, num, num3, curPlayingFrameSamplePos);
			}
		}
	}

	public virtual void ToggleAudioSource(bool toggle)
	{
	}
}
