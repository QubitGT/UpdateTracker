using System;
using System.Collections.Generic;
using System.Threading;

namespace Photon.Voice;

public class WebRTCAudioProcessor : WebRTCAudioLib, IProcessor<short>, IDisposable
{
	private const int REVERSE_BUFFER_POOL_CAPACITY = 50;

	private int reverseStreamDelayMs;

	private bool aec;

	private bool aecHighPass = true;

	private bool aecm;

	private bool highPass;

	private bool ns;

	private bool agc = true;

	private int agcCompressionGain = 9;

	private int agcTargetLevel = 3;

	private bool agc2;

	private bool vad;

	private bool reverseStreamThreadRunning;

	private Queue<short[]> reverseStreamQueue = new Queue<short[]>();

	private AutoResetEvent reverseStreamQueueReady = new AutoResetEvent(initialState: false);

	private FactoryPrimitiveArrayPool<short> reverseBufferFactory;

	private bool bypass;

	private int inFrameSize;

	private int processFrameSize;

	private int samplingRate;

	private int channels;

	private IntPtr proc;

	private bool disposed;

	private Framer<float> reverseFramer;

	private int reverseSamplingRate;

	private int reverseChannels;

	private ILogger logger;

	private const int supportedFrameLenMs = 10;

	public static readonly int[] SupportedSamplingRates = new int[4] { 8000, 16000, 32000, 48000 };

	private bool aecInited;

	private int lastProcessErr;

	private int lastProcessReverseErr;

	public int AECStreamDelayMs
	{
		set
		{
			if (reverseStreamDelayMs != value)
			{
				reverseStreamDelayMs = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.REVERSE_STREAM_DELAY_MS, value);
				}
			}
		}
	}

	public bool AEC
	{
		set
		{
			if (aec != value)
			{
				aec = value;
				InitReverseStream();
				if (proc != IntPtr.Zero)
				{
					setParam(Param.AEC, aec ? 1 : 0);
				}
				aecm = !aec && aecm;
			}
		}
	}

	public bool AECHighPass
	{
		set
		{
			if (aecHighPass != value)
			{
				aecHighPass = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.AEC_HIGH_PASS_FILTER, value ? 1 : 0);
				}
			}
		}
	}

	public bool AECMobile
	{
		set
		{
			if (aecm != value)
			{
				aecm = value;
				InitReverseStream();
				if (proc != IntPtr.Zero)
				{
					setParam(Param.AECM, aecm ? 1 : 0);
				}
				aec = !aecm && aec;
			}
		}
	}

	public bool HighPass
	{
		set
		{
			if (highPass != value)
			{
				highPass = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.HIGH_PASS_FILTER, value ? 1 : 0);
				}
			}
		}
	}

	public bool NoiseSuppression
	{
		set
		{
			if (ns != value)
			{
				ns = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.NS, value ? 1 : 0);
				}
			}
		}
	}

	public bool AGC
	{
		set
		{
			if (agc != value)
			{
				agc = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.AGC, value ? 1 : 0);
				}
			}
		}
	}

	public int AGCCompressionGain
	{
		set
		{
			if (agcCompressionGain == value)
			{
				return;
			}
			if (value < 0 || value > 90)
			{
				logger.LogError("[PV] WebRTCAudioProcessor: new AGCCompressionGain value {0} not in range [0..90]", value);
				return;
			}
			agcCompressionGain = value;
			if (proc != IntPtr.Zero)
			{
				setParam(Param.AGC_COMPRESSION_GAIN, value);
			}
		}
	}

	public int AGCTargetLevel
	{
		set
		{
			if (agcTargetLevel == value)
			{
				return;
			}
			if (value > 31 || value < 0)
			{
				logger.LogError("[PV] WebRTCAudioProcessor: new AGCTargetLevel value {0} not in range [0..31]", value);
				return;
			}
			agcTargetLevel = value;
			if (proc != IntPtr.Zero)
			{
				setParam(Param.AGC_TARGET_LEVEL_DBFS, value);
			}
		}
	}

	public bool AGC2
	{
		set
		{
			if (agc2 != value)
			{
				agc2 = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.AGC2, value ? 1 : 0);
				}
			}
		}
	}

	public bool VAD
	{
		set
		{
			if (vad != value)
			{
				vad = value;
				if (proc != IntPtr.Zero)
				{
					setParam(Param.VAD, value ? 1 : 0);
				}
			}
		}
	}

	public bool Bypass
	{
		private get
		{
			return bypass;
		}
		set
		{
			if (bypass != value)
			{
				logger.LogInfo("[PV] WebRTCAudioProcessor: setting bypass=" + value);
			}
			bypass = value;
		}
	}

	public WebRTCAudioProcessor(ILogger logger, int frameSize, int samplingRate, int channels, int reverseSamplingRate, int reverseChannels)
	{
		bool flag = false;
		int[] supportedSamplingRates = SupportedSamplingRates;
		foreach (int num in supportedSamplingRates)
		{
			if (samplingRate == num)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			logger.LogError("[PV] WebRTCAudioProcessor: input sampling rate ({0}) must be 8000, 16000, 32000 or 48000", samplingRate);
			disposed = true;
			return;
		}
		this.logger = logger;
		inFrameSize = frameSize;
		processFrameSize = samplingRate * 10 / 1000;
		if (inFrameSize / processFrameSize * processFrameSize != inFrameSize)
		{
			logger.LogError("[PV] WebRTCAudioProcessor: input frame size ({0} samples / {1} ms) must be equal to or N times more than webrtc processing frame size ({2} samples / 10 ms)", inFrameSize, 1000f * (float)inFrameSize / (float)samplingRate, processFrameSize);
			disposed = true;
			return;
		}
		this.samplingRate = samplingRate;
		this.channels = channels;
		this.reverseSamplingRate = reverseSamplingRate;
		this.reverseChannels = reverseChannels;
		proc = WebRTCAudioLib.webrtc_audio_processor_create(samplingRate, channels, processFrameSize, samplingRate, reverseChannels);
		WebRTCAudioLib.webrtc_audio_processor_init(proc);
		logger.LogInfo("[PV] WebRTCAudioProcessor create sampling rate {0}, channels{1}, frame size {2}, frame samples {3}, reverseChannels {4}", samplingRate, channels, processFrameSize, inFrameSize / this.channels, this.reverseChannels);
	}

	private void InitReverseStream()
	{
		lock (this)
		{
			if (!aecInited && !disposed)
			{
				int num = processFrameSize * reverseSamplingRate / samplingRate * reverseChannels;
				reverseFramer = new Framer<float>(num);
				reverseBufferFactory = new FactoryPrimitiveArrayPool<short>(50, "WebRTCAudioProcessor Reverse Buffers", inFrameSize);
				logger.LogInfo("[PV] WebRTCAudioProcessor Init reverse stream: frame size {0}, reverseSamplingRate {1}, reverseChannels {2}", num, reverseSamplingRate, reverseChannels);
				if (!reverseStreamThreadRunning)
				{
					Thread thread = new Thread(ReverseStreamThread);
					thread.Start();
					Util.SetThreadName(thread, "[PV] WebRTCProcRevStream");
				}
				if (reverseSamplingRate != samplingRate)
				{
					logger.LogWarning("[PV] WebRTCAudioProcessor AEC: output sampling rate {0} != {1} capture sampling rate. For better AEC, set audio source (microphone) and audio output samping rates to the same value.", reverseSamplingRate, samplingRate);
				}
				aecInited = true;
			}
		}
	}

	public short[] Process(short[] buf)
	{
		if (Bypass)
		{
			return buf;
		}
		if (disposed)
		{
			return buf;
		}
		if (proc == IntPtr.Zero)
		{
			return buf;
		}
		if (buf.Length != inFrameSize)
		{
			logger.LogError("[PV] WebRTCAudioProcessor Process: frame size expected: {0}, passed: {1}", inFrameSize, buf);
			return buf;
		}
		bool flag = false;
		for (int i = 0; i < inFrameSize; i += processFrameSize)
		{
			bool voiceDetected = true;
			int num = WebRTCAudioLib.webrtc_audio_processor_process(proc, buf, i, out voiceDetected);
			if (voiceDetected)
			{
				flag = true;
			}
			if (lastProcessErr != num)
			{
				lastProcessErr = num;
				logger.LogError("[PV] WebRTCAudioProcessor Process: webrtc_audio_processor_process() error {0}", num);
				return buf;
			}
		}
		if (vad && !flag)
		{
			return null;
		}
		return buf;
	}

	public void OnAudioOutFrameFloat(float[] data)
	{
		if (disposed || !aecInited || proc == IntPtr.Zero)
		{
			return;
		}
		foreach (float[] item in reverseFramer.Frame(data))
		{
			short[] array = reverseBufferFactory.New();
			if (item.Length != array.Length)
			{
				AudioUtil.ResampleAndConvert(item, array, array.Length, reverseChannels);
			}
			else
			{
				AudioUtil.Convert(item, array, array.Length);
			}
			lock (reverseStreamQueue)
			{
				if (reverseStreamQueue.Count < 49)
				{
					reverseStreamQueue.Enqueue(array);
					reverseStreamQueueReady.Set();
				}
				else
				{
					logger.LogError("[PV] WebRTCAudioProcessor Reverse stream queue overflow");
					reverseBufferFactory.Free(array);
				}
			}
		}
	}

	private void ReverseStreamThread()
	{
		logger.LogInfo("[PV] WebRTCAudioProcessor: Starting reverse stream thread");
		reverseStreamThreadRunning = true;
		try
		{
			while (!disposed)
			{
				reverseStreamQueueReady.WaitOne();
				while (true)
				{
					short[] array = null;
					lock (reverseStreamQueue)
					{
						if (reverseStreamQueue.Count > 0)
						{
							array = reverseStreamQueue.Dequeue();
						}
					}
					if (array == null)
					{
						break;
					}
					int num = WebRTCAudioLib.webrtc_audio_processor_process_reverse(proc, array, array.Length);
					reverseBufferFactory.Free(array);
					if (lastProcessReverseErr != num)
					{
						lastProcessReverseErr = num;
						logger.LogError("[PV] WebRTCAudioProcessor: OnAudioOutFrameFloat: webrtc_audio_processor_process_reverse() error {0}", num);
					}
				}
			}
		}
		catch (Exception ex)
		{
			logger.LogError("[PV] WebRTCAudioProcessor: ReverseStreamThread Exceptions: " + ex);
		}
		finally
		{
			logger.LogInfo("[PV] WebRTCAudioProcessor: Exiting reverse stream thread");
			reverseStreamThreadRunning = false;
		}
	}

	private int setParam(Param param, int v)
	{
		if (disposed)
		{
			return 0;
		}
		logger.LogInfo("[PV] WebRTCAudioProcessor: setting param " + param.ToString() + "=" + v);
		return WebRTCAudioLib.webrtc_audio_processor_set_param(proc, (int)param, v);
	}

	public void Dispose()
	{
		lock (this)
		{
			if (disposed)
			{
				return;
			}
			disposed = true;
			logger.LogInfo("[PV] WebRTCAudioProcessor: destroying...");
			reverseStreamQueueReady.Set();
			if (proc != IntPtr.Zero)
			{
				while (reverseStreamThreadRunning)
				{
					Thread.Sleep(1);
				}
				WebRTCAudioLib.webrtc_audio_processor_destroy(proc);
				logger.LogInfo("[PV] WebRTCAudioProcessor: destroyed");
			}
		}
	}
}
