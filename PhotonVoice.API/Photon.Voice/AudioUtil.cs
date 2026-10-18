using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace Photon.Voice;

public static class AudioUtil
{
	public class ToneAudioReader<T> : IAudioReader<T>, IDataReader<T>, IDisposable, IAudioDesc
	{
		private double k;

		private long timeSamples;

		private Func<double> clockSec;

		private int samplingRate;

		private int channels;

		public int Channels => channels;

		public int SamplingRate => samplingRate;

		public string Error { get; private set; }

		public ToneAudioReader(Func<double> clockSec = null, double frequency = 440.0, int samplingRate = 48000, int channels = 2)
		{
			this.clockSec = ((clockSec == null) ? ((Func<double>)(() => (double)DateTime.Now.Ticks / 10000000.0)) : clockSec);
			this.samplingRate = samplingRate;
			this.channels = channels;
			k = Math.PI * 2.0 * frequency / (double)SamplingRate;
		}

		public void Dispose()
		{
		}

		public bool Read(T[] buf)
		{
			int num = buf.Length / Channels;
			long num2 = (long)(clockSec() * (double)SamplingRate);
			long num3 = num2 - timeSamples;
			if (Math.Abs(num3) > SamplingRate / 4)
			{
				num3 = num;
				timeSamples = num2 - num;
			}
			if (num3 < num)
			{
				return false;
			}
			int num4 = 0;
			if (buf is float[])
			{
				for (int i = 0; i < num; i++)
				{
					float[] array = buf as float[];
					float num5 = (float)(Math.Sin((double)timeSamples++ * this.k) * 0.20000000298023224);
					for (int j = 0; j < Channels; j++)
					{
						array[num4++] = num5;
					}
				}
			}
			else if (buf is short[])
			{
				short[] array2 = buf as short[];
				for (int k = 0; k < num; k++)
				{
					short num6 = (short)(Math.Sin((double)timeSamples++ * this.k) * 6553.39990234375);
					for (int l = 0; l < Channels; l++)
					{
						array2[num4++] = num6;
					}
				}
			}
			return true;
		}
	}

	public class ToneAudioPusher<T> : IAudioPusher<T>, IAudioDesc, IDisposable
	{
		private double k;

		private Timer timer;

		private Action<T[]> callback;

		private ObjectFactory<T[], int> bufferFactory;

		private int cntFrame;

		private int posSamples;

		private int bufSizeSamples;

		private int samplingRate;

		private int channels;

		public int Channels => channels;

		public int SamplingRate => samplingRate;

		public string Error { get; private set; }

		public ToneAudioPusher(int frequency = 440, int bufSizeMs = 100, int samplingRate = 48000, int channels = 2)
		{
			this.samplingRate = samplingRate;
			this.channels = channels;
			bufSizeSamples = bufSizeMs * SamplingRate / 1000;
			k = Math.PI * 2.0 * (double)frequency / (double)SamplingRate;
		}

		public void SetCallback(Action<T[]> callback, ObjectFactory<T[], int> bufferFactory)
		{
			if (timer != null)
			{
				Dispose();
			}
			this.callback = callback;
			this.bufferFactory = bufferFactory;
			timer = new Timer(1000.0 * (double)bufSizeSamples / (double)SamplingRate);
			timer.Elapsed += OnTimedEvent;
			timer.Enabled = true;
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			T[] array = bufferFactory.New(bufSizeSamples * Channels);
			int num = 0;
			if (array is float[])
			{
				float[] array2 = array as float[];
				for (int i = 0; i < bufSizeSamples; i++)
				{
					float num2 = (float)(Math.Sin((double)(posSamples + i) * this.k) / 2.0);
					for (int j = 0; j < Channels; j++)
					{
						array2[num++] = num2;
					}
				}
			}
			else if (array is short[])
			{
				short[] array3 = array as short[];
				for (int k = 0; k < bufSizeSamples; k++)
				{
					short num3 = (short)(Math.Sin((double)(posSamples + k) * this.k) * 32767.0 / 2.0);
					for (int l = 0; l < Channels; l++)
					{
						array3[num++] = num3;
					}
				}
			}
			cntFrame++;
			posSamples += bufSizeSamples;
			callback(array);
		}

		public void Dispose()
		{
			if (timer != null)
			{
				timer.Close();
			}
		}
	}

	public class TempoUp<T>
	{
		private readonly int sizeofT = Marshal.SizeOf(default(T));

		private int channels;

		private int skipGroup;

		private int skipFactor;

		private int sign;

		private int waveCnt;

		private bool skipping;

		public void Begin(int channels, int changePerc, int skipGroup)
		{
			this.channels = channels;
			skipFactor = 100 / changePerc;
			this.skipGroup = skipGroup;
			sign = 0;
			skipping = false;
			waveCnt = 0;
		}

		public int Process(T[] s, T[] d)
		{
			if (sizeofT == 2)
			{
				return processShort(s as short[], d as short[]);
			}
			return processFloat(s as float[], d as float[]);
		}

		public int End(T[] s)
		{
			if (!skipping)
			{
				return 0;
			}
			if (sizeofT == 2)
			{
				return endShort(s as short[]);
			}
			return endFloat(s as float[]);
		}

		private int processFloat(float[] s, float[] d)
		{
			int num = 0;
			if (channels == 1)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] < 0f)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						sign = 0;
					}
					if (!skipping)
					{
						d[num++] = s[i];
					}
				}
			}
			else if (channels == 2)
			{
				for (int j = 0; j < s.Length; j += 2)
				{
					if (s[j] + s[j + 1] < 0f)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						sign = 0;
					}
					if (!skipping)
					{
						d[num++] = s[j];
						d[num++] = s[j + 1];
					}
				}
			}
			else
			{
				for (int k = 0; k < s.Length; k += channels)
				{
					float num2 = s[k] + s[k + 1];
					int num3 = 2;
					while (k < channels)
					{
						num2 += s[k + num3];
						num3++;
					}
					if (num2 < 0f)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						sign = 0;
					}
					if (!skipping)
					{
						d[num++] = s[k];
						d[num++] = s[k + 1];
						int num4 = 2;
						while (k < channels)
						{
							d[num++] += s[k + num4];
							num4++;
						}
					}
				}
			}
			return num / channels;
		}

		public int endFloat(float[] s)
		{
			if (channels == 1)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] < 0f)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						if (!skipping)
						{
							return i;
						}
						sign = 0;
					}
				}
			}
			else if (channels == 2)
			{
				for (int j = 0; j < s.Length; j += 2)
				{
					if (s[j] + s[j + 1] < 0f)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						if (!skipping)
						{
							return j / 2;
						}
						sign = 0;
					}
				}
			}
			else
			{
				for (int k = 0; k < s.Length; k += channels)
				{
					float num = s[k] + s[k + 1];
					int num2 = 2;
					while (k < channels)
					{
						num += s[k + num2];
						num2++;
					}
					if (num < 0f)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						if (!skipping)
						{
							return k / channels;
						}
						sign = 0;
					}
				}
			}
			return 0;
		}

		private int processShort(short[] s, short[] d)
		{
			int num = 0;
			if (channels == 1)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] < 0)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						sign = 0;
					}
					if (!skipping)
					{
						d[num++] = s[i];
					}
				}
			}
			else if (channels == 2)
			{
				for (int j = 0; j < s.Length; j += 2)
				{
					if (s[j] + s[j + 1] < 0)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						sign = 0;
					}
					if (!skipping)
					{
						d[num++] = s[j];
						d[num++] = s[j + 1];
					}
				}
			}
			else
			{
				for (int k = 0; k < s.Length; k += channels)
				{
					int num2 = s[k] + s[k + 1];
					int num3 = 2;
					while (k < channels)
					{
						num2 += s[k + num3];
						num3++;
					}
					if (num2 < 0)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						sign = 0;
					}
					if (!skipping)
					{
						d[num++] = s[k];
						d[num++] = s[k + 1];
						int num4 = 2;
						while (k < channels)
						{
							d[num++] += s[k + num4];
							num4++;
						}
					}
				}
			}
			return num / channels;
		}

		public int endShort(short[] s)
		{
			if (channels == 1)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] < 0)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						if (!skipping)
						{
							return i;
						}
						sign = 0;
					}
				}
			}
			else if (channels == 2)
			{
				for (int j = 0; j < s.Length; j += 2)
				{
					if (s[j] + s[j + 1] < 0)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						if (!skipping)
						{
							return j / 2;
						}
						sign = 0;
					}
				}
			}
			else
			{
				for (int k = 0; k < s.Length; k += channels)
				{
					int num = s[k] + s[k + 1];
					int num2 = 2;
					while (k < channels)
					{
						num += s[k + num2];
						num2++;
					}
					if (num < 0)
					{
						sign = -1;
					}
					else if (sign < 0)
					{
						waveCnt++;
						skipping = waveCnt % (skipGroup * skipFactor) < skipGroup;
						if (!skipping)
						{
							return k / channels;
						}
						sign = 0;
					}
				}
			}
			return 0;
		}
	}

	public class Resampler<T> : IProcessor<T>, IDisposable
	{
		protected T[] frameResampled;

		private int channels;

		public Resampler(int dstSize, int channels)
		{
			frameResampled = new T[dstSize];
			this.channels = channels;
		}

		public T[] Process(T[] buf)
		{
			Resample(buf, frameResampled, frameResampled.Length, channels);
			return frameResampled;
		}

		public void Dispose()
		{
		}
	}

	public interface ILevelMeter
	{
		float CurrentAvgAmp { get; }

		float CurrentPeakAmp { get; }

		float AccumAvgPeakAmp { get; }

		void ResetAccumAvgPeakAmp();
	}

	public class LevelMeterDummy : ILevelMeter
	{
		public float CurrentAvgAmp => 0f;

		public float CurrentPeakAmp => 0f;

		public float AccumAvgPeakAmp => 0f;

		public void ResetAccumAvgPeakAmp()
		{
		}
	}

	public abstract class LevelMeter<T> : IProcessor<T>, IDisposable, ILevelMeter
	{
		protected float ampSum;

		protected float ampPeak;

		protected int bufferSize;

		protected float[] prevValues;

		protected int prevValuesHead;

		protected float accumAvgPeakAmpSum;

		protected int accumAvgPeakAmpCount;

		protected float currentPeakAmp;

		protected float norm;

		public float CurrentAvgAmp => ampSum / (float)bufferSize * norm;

		public float CurrentPeakAmp
		{
			get
			{
				return currentPeakAmp * norm;
			}
			protected set
			{
				currentPeakAmp = value / norm;
			}
		}

		public float AccumAvgPeakAmp
		{
			get
			{
				if (accumAvgPeakAmpCount != 0)
				{
					return accumAvgPeakAmpSum / (float)accumAvgPeakAmpCount * norm;
				}
				return 0f;
			}
		}

		internal LevelMeter(int samplingRate, int numChannels)
		{
			bufferSize = samplingRate * numChannels / 2;
			prevValues = new float[bufferSize];
		}

		public void ResetAccumAvgPeakAmp()
		{
			accumAvgPeakAmpSum = 0f;
			accumAvgPeakAmpCount = 0;
			ampPeak = 0f;
		}

		public abstract T[] Process(T[] buf);

		public void Dispose()
		{
		}
	}

	public class LevelMeterFloat : LevelMeter<float>
	{
		public LevelMeterFloat(int samplingRate, int numChannels)
			: base(samplingRate, numChannels)
		{
			norm = 1f;
		}

		public override float[] Process(float[] buf)
		{
			for (int i = 0; i < buf.Length; i++)
			{
				float num = buf[i];
				if (num < 0f)
				{
					num = 0f - num;
				}
				ampSum = ampSum + num - prevValues[prevValuesHead];
				prevValues[prevValuesHead] = num;
				if (ampPeak < num)
				{
					ampPeak = num;
				}
				if (prevValuesHead == 0)
				{
					currentPeakAmp = ampPeak;
					ampPeak = 0f;
					accumAvgPeakAmpSum += currentPeakAmp;
					accumAvgPeakAmpCount++;
				}
				prevValuesHead = (prevValuesHead + 1) % bufferSize;
			}
			return buf;
		}
	}

	public class LevelMeterShort : LevelMeter<short>
	{
		public LevelMeterShort(int samplingRate, int numChannels)
			: base(samplingRate, numChannels)
		{
			norm = 3.051851E-05f;
		}

		public override short[] Process(short[] buf)
		{
			for (int i = 0; i < buf.Length; i++)
			{
				short num = buf[i];
				if (num < 0)
				{
					num = (short)(-num);
				}
				ampSum = ampSum + (float)num - prevValues[prevValuesHead];
				prevValues[prevValuesHead] = num;
				if (ampPeak < (float)num)
				{
					ampPeak = num;
				}
				if (prevValuesHead == 0)
				{
					currentPeakAmp = ampPeak;
					ampPeak = 0f;
					accumAvgPeakAmpSum += currentPeakAmp;
					accumAvgPeakAmpCount++;
				}
				prevValuesHead = (prevValuesHead + 1) % bufferSize;
			}
			return buf;
		}
	}

	public interface IVoiceDetector
	{
		bool On { get; set; }

		float Threshold { get; set; }

		bool Detected { get; }

		DateTime DetectedTime { get; }

		int ActivityDelayMs { get; set; }

		event Action OnDetected;
	}

	public class VoiceDetectorCalibration<T> : IProcessor<T>, IDisposable
	{
		private IVoiceDetector voiceDetector;

		private ILevelMeter levelMeter;

		private int valuesPerSec;

		protected int calibrateCount;

		private Action<float> onCalibrated;

		public bool IsCalibrating => calibrateCount > 0;

		public VoiceDetectorCalibration(IVoiceDetector voiceDetector, ILevelMeter levelMeter, int samplingRate, int channels)
		{
			valuesPerSec = samplingRate * channels;
			this.voiceDetector = voiceDetector;
			this.levelMeter = levelMeter;
		}

		public void Calibrate(int durationMs, Action<float> onCalibrated = null)
		{
			calibrateCount = valuesPerSec * durationMs / 1000;
			this.onCalibrated = onCalibrated;
			levelMeter.ResetAccumAvgPeakAmp();
		}

		public T[] Process(T[] buf)
		{
			if (calibrateCount != 0)
			{
				calibrateCount -= buf.Length;
				if (calibrateCount <= 0)
				{
					calibrateCount = 0;
					voiceDetector.Threshold = levelMeter.AccumAvgPeakAmp * 2f;
					if (onCalibrated != null)
					{
						onCalibrated(voiceDetector.Threshold);
					}
				}
			}
			return buf;
		}

		public void Dispose()
		{
		}
	}

	public class VoiceDetectorDummy : IVoiceDetector
	{
		public bool On
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float Threshold
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool Detected => false;

		public int ActivityDelayMs
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public DateTime DetectedTime { get; private set; }

		public event Action OnDetected
		{
			add
			{
			}
			remove
			{
			}
		}
	}

	public abstract class VoiceDetector<T> : IProcessor<T>, IDisposable, IVoiceDetector
	{
		protected float norm;

		protected float threshold;

		private bool detected;

		protected int activityDelay;

		protected int autoSilenceCounter;

		protected int valuesCountPerSec;

		protected int activityDelayValuesCount;

		public bool On { get; set; }

		public float Threshold
		{
			get
			{
				return threshold * norm;
			}
			set
			{
				threshold = value / norm;
			}
		}

		public bool Detected
		{
			get
			{
				return detected;
			}
			protected set
			{
				if (detected != value)
				{
					detected = value;
					DetectedTime = DateTime.Now;
					if (detected && this.OnDetected != null)
					{
						this.OnDetected();
					}
				}
			}
		}

		public DateTime DetectedTime { get; private set; }

		public int ActivityDelayMs
		{
			get
			{
				return activityDelay;
			}
			set
			{
				activityDelay = value;
				activityDelayValuesCount = value * valuesCountPerSec / 1000;
			}
		}

		public event Action OnDetected;

		internal VoiceDetector(int samplingRate, int numChannels)
		{
			valuesCountPerSec = samplingRate * numChannels;
			ActivityDelayMs = 500;
			On = true;
		}

		public abstract T[] Process(T[] buf);

		public void Dispose()
		{
		}
	}

	public class VoiceDetectorFloat : VoiceDetector<float>
	{
		public VoiceDetectorFloat(int samplingRate, int numChannels)
			: base(samplingRate, numChannels)
		{
			norm = 1f;
		}

		public override float[] Process(float[] buffer)
		{
			if (base.On)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					if (buffer[i] > threshold)
					{
						base.Detected = true;
						autoSilenceCounter = 0;
					}
					else
					{
						autoSilenceCounter++;
					}
				}
				if (autoSilenceCounter > activityDelayValuesCount)
				{
					base.Detected = false;
				}
				if (!base.Detected)
				{
					return null;
				}
				return buffer;
			}
			return buffer;
		}
	}

	public class VoiceDetectorShort : VoiceDetector<short>
	{
		public VoiceDetectorShort(int samplingRate, int numChannels)
			: base(samplingRate, numChannels)
		{
			norm = 3.051851E-05f;
		}

		public override short[] Process(short[] buffer)
		{
			if (base.On)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					if ((float)buffer[i] > threshold)
					{
						base.Detected = true;
						autoSilenceCounter = 0;
					}
					else
					{
						autoSilenceCounter++;
					}
				}
				if (autoSilenceCounter > activityDelayValuesCount)
				{
					base.Detected = false;
				}
				if (!base.Detected)
				{
					return null;
				}
				return buffer;
			}
			return buffer;
		}
	}

	public class VoiceLevelDetectCalibrate<T> : IProcessor<T>, IDisposable
	{
		private VoiceDetectorCalibration<T> calibration;

		public ILevelMeter LevelMeter { get; private set; }

		public IVoiceDetector VoiceDetector { get; private set; }

		public bool IsCalibrating => calibration.IsCalibrating;

		public VoiceLevelDetectCalibrate(int samplingRate, int channels)
		{
			T[] array = new T[1];
			if (array[0] is float)
			{
				LevelMeter = new LevelMeterFloat(samplingRate, channels);
				VoiceDetector = new VoiceDetectorFloat(samplingRate, channels);
			}
			else
			{
				if (!(array[0] is short))
				{
					throw new Exception("VoiceLevelDetectCalibrate: type not supported: " + array[0].GetType());
				}
				LevelMeter = new LevelMeterShort(samplingRate, channels);
				VoiceDetector = new VoiceDetectorShort(samplingRate, channels);
			}
			calibration = new VoiceDetectorCalibration<T>(VoiceDetector, LevelMeter, samplingRate, channels);
		}

		public void Calibrate(int durationMs, Action<float> onCalibrated = null)
		{
			calibration.Calibrate(durationMs, onCalibrated);
		}

		public T[] Process(T[] buf)
		{
			buf = (LevelMeter as IProcessor<T>).Process(buf);
			buf = ((IProcessor<T>)calibration).Process(buf);
			buf = (VoiceDetector as IProcessor<T>).Process(buf);
			return buf;
		}

		public void Dispose()
		{
			(LevelMeter as IProcessor<T>).Dispose();
			(VoiceDetector as IProcessor<T>).Dispose();
			calibration.Dispose();
		}
	}

	public static void Resample<T>(T[] src, T[] dst, int dstCount, int channels)
	{
		switch (channels)
		{
		case 1:
		{
			for (int i = 0; i < dstCount; i++)
			{
				dst[i] = src[i * src.Length / dstCount];
			}
			return;
		}
		case 2:
		{
			for (int j = 0; j < dstCount / 2; j++)
			{
				int num = j * src.Length / dstCount;
				int num2 = j * 2;
				int num3 = num * 2;
				dst[num2++] = src[num3++];
				dst[num2] = src[num3];
			}
			return;
		}
		}
		for (int k = 0; k < dstCount / channels; k++)
		{
			int num4 = k * src.Length / dstCount;
			int num5 = k * channels;
			int num6 = num4 * channels;
			for (int l = 0; l < channels; l++)
			{
				dst[num5++] = src[num6++];
			}
		}
	}

	public static void Resample<T>(T[] src, int srcOffset, int srcCount, T[] dst, int dstOffset, int dstCount, int channels)
	{
		switch (channels)
		{
		case 1:
		{
			for (int i = 0; i < dstCount; i++)
			{
				dst[dstOffset + i] = src[srcOffset + i * srcCount / dstCount];
			}
			return;
		}
		case 2:
		{
			for (int j = 0; j < dstCount / 2; j++)
			{
				int num = j * srcCount / dstCount;
				int num2 = j * 2;
				int num3 = num * 2;
				dst[dstOffset + num2++] = src[srcOffset + num3++];
				dst[dstOffset + num2] = src[srcOffset + num3];
			}
			return;
		}
		}
		for (int k = 0; k < dstCount / channels; k++)
		{
			int num4 = k * srcCount / dstCount;
			int num5 = k * channels;
			int num6 = num4 * channels;
			for (int l = 0; l < channels; l++)
			{
				dst[dstOffset + num5++] = src[srcOffset + num6++];
			}
		}
	}

	public static void Resample<T>(T[] src, int srcOffset, int srcCount, int srcChannels, T[] dst, int dstOffset, int dstCount, int dstChannels)
	{
		if (srcChannels == dstChannels)
		{
			Resample(src, srcOffset, srcCount, dst, dstOffset, dstCount, dstChannels);
			return;
		}
		if (srcChannels == 1 && dstChannels == 2)
		{
			int i = 0;
			int num = 0;
			for (; i < dstCount / 2; i++)
			{
				T val = src[srcOffset + i * srcCount * 2 / dstCount];
				dst[dstOffset + num++] = val;
				dst[dstOffset + num++] = val;
			}
			return;
		}
		if (srcChannels == 2 && dstChannels == 1)
		{
			for (int j = 0; j < dstCount; j++)
			{
				dst[dstOffset + j] = src[srcOffset + j * srcCount / dstCount / 2 * 2];
			}
			return;
		}
		int k = 0;
		int num2 = 0;
		for (; k < dstCount / dstChannels; k++)
		{
			int num3 = srcOffset + k * srcCount * dstChannels / dstCount / srcChannels * srcChannels;
			if (srcChannels >= dstChannels)
			{
				for (int l = 0; l < dstChannels; l++)
				{
					dst[dstOffset + num2++] = src[num3 + l];
				}
				continue;
			}
			for (int m = 0; m < srcChannels; m++)
			{
				dst[dstOffset + num2++] = src[num3 + m];
			}
			num2 += dstChannels - srcChannels;
		}
	}

	public static void ResampleAndConvert(short[] src, float[] dst, int dstCount, int channels)
	{
		switch (channels)
		{
		case 1:
		{
			for (int i = 0; i < dstCount; i++)
			{
				dst[i] = (float)src[i * src.Length / dstCount] / 32767f;
			}
			return;
		}
		case 2:
		{
			for (int j = 0; j < dstCount / 2; j++)
			{
				int num = j * src.Length / dstCount;
				int num2 = j * 2;
				int num3 = num * 2;
				dst[num2++] = (float)src[num3++] / 32767f;
				dst[num2] = (float)src[num3] / 32767f;
			}
			return;
		}
		}
		for (int k = 0; k < dstCount / channels; k++)
		{
			int num4 = k * src.Length / dstCount;
			int num5 = k * channels;
			int num6 = num4 * channels;
			for (int l = 0; l < channels; l++)
			{
				dst[num5++] = (float)src[num6++] / 32767f;
			}
		}
	}

	public static void ResampleAndConvert(float[] src, short[] dst, int dstCount, int channels)
	{
		switch (channels)
		{
		case 1:
		{
			for (int i = 0; i < dstCount; i++)
			{
				dst[i] = (short)(src[i * src.Length / dstCount] * 32767f);
			}
			return;
		}
		case 2:
		{
			for (int j = 0; j < dstCount / 2; j++)
			{
				int num = j * src.Length / dstCount;
				int num2 = j * 2;
				int num3 = num * 2;
				dst[num2++] = (short)(src[num3++] * 32767f);
				dst[num2] = (short)(src[num3] * 32767f);
			}
			return;
		}
		}
		for (int k = 0; k < dstCount / channels; k++)
		{
			int num4 = k * src.Length / dstCount;
			int num5 = k * channels;
			int num6 = num4 * channels;
			for (int l = 0; l < channels; l++)
			{
				dst[num5++] = (short)(src[num6++] * 32767f);
			}
		}
	}

	public static void Convert(float[] src, short[] dst, int dstCount)
	{
		for (int i = 0; i < dstCount; i++)
		{
			dst[i] = (short)(src[i] * 32767f);
		}
	}

	public static void Convert(short[] src, float[] dst, int dstCount)
	{
		for (int i = 0; i < dstCount; i++)
		{
			dst[i] = (float)src[i] / 32767f;
		}
	}

	public static void ForceToStereo<T>(T[] src, T[] dst, int srcChannels)
	{
		int num = 0;
		for (int i = 0; i < dst.Length - 1; i += 2)
		{
			dst[i] = src[num];
			dst[i + 1] = ((srcChannels > 1) ? src[num + 1] : src[num]);
			num += srcChannels;
		}
	}

	internal static string tostr<T>(T[] x, int lim = 10)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ((x.Length < lim) ? x.Length : lim); i++)
		{
			stringBuilder.Append("-");
			stringBuilder.Append(x[i]);
		}
		return stringBuilder.ToString();
	}
}
