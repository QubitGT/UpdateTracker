using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Photon.Voice;

public class AudioOutDelayControl
{
	public class PlayDelayConfig
	{
		public int Low { get; set; }

		public int High { get; set; }

		public int Max { get; set; }

		public int SpeedUpPerc { get; set; }

		public PlayDelayConfig()
		{
			Low = 200;
			High = 400;
			Max = 1000;
			SpeedUpPerc = 5;
		}

		public PlayDelayConfig Clone()
		{
			return new PlayDelayConfig
			{
				Low = Low,
				High = High,
				Max = Max,
				SpeedUpPerc = SpeedUpPerc
			};
		}
	}
}
public abstract class AudioOutDelayControl<T> : AudioOutDelayControl, IAudioOut<T>
{
	private readonly int sizeofT = Marshal.SizeOf(default(T));

	private const int TEMPO_UP_SKIP_GROUP = 6;

	private int frameSamples;

	private int frameSize;

	protected int bufferSamples;

	protected int frequency;

	private int clipWriteSamplePos;

	private int playSamplePosPrev;

	private int sourceTimeSamplesPrev;

	private int playLoopCount;

	private PlayDelayConfig playDelayConfig;

	protected int channels;

	private bool started;

	private bool flushed = true;

	private int targetDelaySamples;

	private int upperTargetDelaySamples;

	private int maxDelaySamples;

	private const int NO_PUSH_TIMEOUT_MS = 100;

	private int lastPushTime = Environment.TickCount - 100;

	protected readonly ILogger logger;

	protected readonly string logPrefix;

	private readonly bool debugInfo;

	private readonly bool processInService;

	private T[] zeroFrame;

	private T[] resampledFrame;

	private AudioUtil.TempoUp<T> tempoUp;

	private bool tempoChangeHQ;

	private Queue<T[]> frameQueue = new Queue<T[]>();

	public const int FRAME_POOL_CAPACITY = 50;

	private PrimitiveArrayPool<T> framePool = new PrimitiveArrayPool<T>(50, "AudioOutDelayControl");

	private bool catchingUp;

	public abstract int OutPos { get; }

	public int Lag => (int)(((float)clipWriteSamplePos - (started ? ((float)playLoopCount * (float)bufferSamples + (float)OutPos) : 0f)) * 1000f / (float)frequency);

	public bool IsFlushed
	{
		get
		{
			if (started)
			{
				return flushed;
			}
			return true;
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (!IsFlushed)
			{
				return Environment.TickCount - lastPushTime < 100;
			}
			return false;
		}
	}

	public abstract void OutCreate(int frequency, int channels, int bufferSamples);

	public abstract void OutStart();

	public abstract void OutWrite(T[] data, int offsetSamples);

	public AudioOutDelayControl(bool processInService, PlayDelayConfig playDelayConfig, ILogger logger, string logPrefix, bool debugInfo)
	{
		this.processInService = processInService;
		this.playDelayConfig = playDelayConfig.Clone();
		this.logger = logger;
		this.logPrefix = logPrefix;
		this.debugInfo = debugInfo;
	}

	public void Start(int frequency, int channels, int frameSamples)
	{
		this.frequency = frequency;
		this.channels = channels;
		targetDelaySamples = playDelayConfig.Low * frequency / 1000 + frameSamples;
		upperTargetDelaySamples = playDelayConfig.High * frequency / 1000 + frameSamples;
		if (upperTargetDelaySamples < targetDelaySamples + 2 * frameSamples)
		{
			upperTargetDelaySamples = targetDelaySamples + 2 * frameSamples;
		}
		_ = playDelayConfig.Max;
		maxDelaySamples = playDelayConfig.Max * frequency / 1000;
		if (maxDelaySamples < upperTargetDelaySamples)
		{
			maxDelaySamples = upperTargetDelaySamples;
		}
		bufferSamples = 3 * maxDelaySamples;
		this.frameSamples = frameSamples;
		frameSize = frameSamples * channels;
		clipWriteSamplePos = targetDelaySamples;
		if (framePool.Info != frameSize)
		{
			framePool.Init(frameSize);
		}
		zeroFrame = new T[frameSize];
		resampledFrame = new T[frameSize];
		tempoChangeHQ = false;
		if (!tempoChangeHQ)
		{
			tempoUp = new AudioUtil.TempoUp<T>();
		}
		OutCreate(frequency, channels, bufferSamples);
		OutStart();
		started = true;
		logger.LogInfo("{0} Start: {1} bs={2} ch={3} f={4} tds={5} utds={6} mds={7} speed={8} tempo={9}", logPrefix, (sizeofT == 2) ? "short" : "float", bufferSamples, channels, frequency, targetDelaySamples, upperTargetDelaySamples, maxDelaySamples, playDelayConfig.SpeedUpPerc, tempoChangeHQ ? "HQ" : "LQ");
	}

	private bool processFrame(T[] frame, int playSamplePos)
	{
		int num = clipWriteSamplePos - playSamplePos;
		if (!flushed)
		{
			if (num > maxDelaySamples)
			{
				if (debugInfo)
				{
					logger.LogDebug("{0} overrun {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
				}
				clipWriteSamplePos = playSamplePos + maxDelaySamples;
				num = maxDelaySamples;
			}
			else if (num < 0)
			{
				if (debugInfo)
				{
					logger.LogDebug("{0} underrun {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
				}
				clipWriteSamplePos = playSamplePos + targetDelaySamples;
				num = targetDelaySamples;
			}
		}
		if (frame == null)
		{
			flushed = true;
			if (debugInfo)
			{
				logger.LogDebug("{0} stream flush pause {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
			}
			if (catchingUp)
			{
				catchingUp = false;
				if (debugInfo)
				{
					logger.LogDebug("{0} stream sync reset {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
				}
			}
			return true;
		}
		if (flushed)
		{
			clipWriteSamplePos = playSamplePos + targetDelaySamples;
			num = targetDelaySamples;
			flushed = false;
			if (debugInfo)
			{
				logger.LogDebug("{0} stream unpause {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
			}
		}
		if (num > upperTargetDelaySamples && !catchingUp)
		{
			if (!tempoChangeHQ)
			{
				tempoUp.Begin(channels, playDelayConfig.SpeedUpPerc, 6);
			}
			catchingUp = true;
			if (debugInfo)
			{
				logger.LogDebug("{0} stream sync started {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
			}
		}
		bool flag = false;
		if (num <= targetDelaySamples && catchingUp)
		{
			if (!tempoChangeHQ)
			{
				int num2 = tempoUp.End(frame);
				int num3 = frame.Length / channels - num2;
				Buffer.BlockCopy(frame, num2 * channels * sizeofT, resampledFrame, 0, num3 * channels * sizeofT);
				writeResampled(resampledFrame, num3);
				flag = true;
			}
			catchingUp = false;
			if (debugInfo)
			{
				logger.LogDebug("{0} stream sync finished {1} {2} {3} {4} {5}", logPrefix, upperTargetDelaySamples, num, playSamplePos, clipWriteSamplePos, playSamplePos + targetDelaySamples);
			}
		}
		if (flag)
		{
			return false;
		}
		if (catchingUp)
		{
			if (!tempoChangeHQ)
			{
				int resampledLenSamples = tempoUp.Process(frame, resampledFrame);
				writeResampled(resampledFrame, resampledLenSamples);
			}
		}
		else
		{
			OutWrite(frame, clipWriteSamplePos % bufferSamples);
			clipWriteSamplePos += frame.Length / channels;
		}
		return false;
	}

	public void Service()
	{
		if (!started)
		{
			return;
		}
		int outPos = OutPos;
		if (outPos < sourceTimeSamplesPrev)
		{
			playLoopCount++;
		}
		sourceTimeSamplesPrev = outPos;
		int num = playLoopCount * bufferSamples + outPos;
		if (processInService)
		{
			lock (frameQueue)
			{
				while (frameQueue.Count > 0)
				{
					T[] array = frameQueue.Dequeue();
					if (processFrame(array, num))
					{
						return;
					}
					framePool.Release(array, array.Length);
				}
			}
		}
		int num2 = playSamplePosPrev;
		int num3 = num - bufferSamples;
		if (num2 < num3)
		{
			num2 = num3;
		}
		int num4 = (num - num2 - 1) / frameSamples + 1;
		for (int i = num - num4 * frameSamples; i < num; i += frameSamples)
		{
			int num5 = i % bufferSamples;
			if (num5 < 0)
			{
				num5 += bufferSamples;
			}
			OutWrite(zeroFrame, num5);
		}
		playSamplePosPrev = num;
	}

	private int writeResampled(T[] f, int resampledLenSamples)
	{
		int num = (f.Length - resampledLenSamples * channels) * sizeofT;
		if (num > 0)
		{
			Buffer.BlockCopy(zeroFrame, 0, f, resampledLenSamples * channels * sizeofT, num);
		}
		OutWrite(f, clipWriteSamplePos % bufferSamples);
		clipWriteSamplePos += resampledLenSamples;
		return resampledLenSamples;
	}

	public void Push(T[] frame)
	{
		if (!started || frame.Length == 0)
		{
			return;
		}
		if (frame.Length != frameSize)
		{
			logger.LogError("{0} audio frames are not of size: {1} != {2}", logPrefix, frame.Length, frameSize);
			return;
		}
		if (processInService)
		{
			T[] array = framePool.AcquireOrCreate();
			Buffer.BlockCopy(frame, 0, array, 0, frame.Length * sizeofT);
			lock (frameQueue)
			{
				frameQueue.Enqueue(array);
			}
		}
		else
		{
			processFrame(frame, playLoopCount * bufferSamples + OutPos);
		}
		lastPushTime = Environment.TickCount;
	}

	public void Flush()
	{
		if (processInService)
		{
			lock (frameQueue)
			{
				frameQueue.Enqueue(null);
				return;
			}
		}
		processFrame(null, playLoopCount * bufferSamples + OutPos);
	}

	public virtual void Stop()
	{
		started = false;
	}

	public virtual void ToggleAudioSource(bool toggle)
	{
	}
}
