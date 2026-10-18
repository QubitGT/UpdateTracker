using System;
using System.Collections;
using System.Collections.Generic;
using Meta.Voice;
using Meta.Voice.Logging;
using Meta.WitAi.Attributes;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Lib;
using UnityEngine;

namespace Meta.WitAi.Data;

[LogCategory(LogCategory.Audio, LogCategory.Input)]
public class AudioBuffer : MonoBehaviour
{
	private const string DEFAULT_OBJECT_NAME = "AudioBuffer";

	private static bool _isQuitting = false;

	public static bool instantiateMic = true;

	private static AudioBuffer _instance;

	public static IAudioBufferProvider AudioBufferProvider;

	[Tooltip("If set to true, the audio buffer will always be recording.")]
	[SerializeField]
	private bool alwaysRecording;

	[Tooltip("Configuration settings for the audio buffer.")]
	[SerializeField]
	private AudioBufferConfiguration audioBufferConfiguration = new AudioBufferConfiguration();

	[TooltipBox("Events triggered when AudioBuffer processes and receives audio data.")]
	[SerializeField]
	private AudioBufferEvents events = new AudioBufferEvents();

	[ObjectType(typeof(IAudioInputSource), new Type[] { })]
	[SerializeField]
	private UnityEngine.Object _micInput;

	private IAudioLevelRangeProvider _micLevelRange;

	private bool _active;

	private Mic _instantiatedMic;

	private int _totalSampleChunks;

	private RingBuffer<byte> _outputBuffer;

	private HashSet<Component> _recorders = new HashSet<Component>();

	private const float MIC_RESET = -1f;

	private Coroutine _volumeUpdate;

	private Coroutine _sampleReadyCoroutine;

	private RingBuffer<byte>.Marker _sampleReadyMarker;

	private float _sampleReadyMaxLevel;

	private long _lastSampleTime;

	private long _startSampleTime;

	private long _measureSampleTotal;

	private int _measuredSampleRateCount;

	private readonly double[] _measuredSampleRates = new double[20];

	private const int TIMEOUT_TICKS = 500000;

	private const int MEASURE_TICKS = 2500000;

	private const int MEASURE_AVERAGE_COUNT = 20;

	private static readonly int[] ALLOWED_SAMPLE_RATES = new int[11]
	{
		8000, 11025, 16000, 22050, 32000, 44100, 48000, 88200, 96000, 176400,
		192000
	};

	public static IVLogger _log { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Input);


	public static AudioBuffer Instance
	{
		get
		{
			if (!_instance)
			{
				_instance = UnityEngine.Object.FindAnyObjectByType<AudioBuffer>();
				if (!_instance && CanInstantiate())
				{
					if (AudioBufferProvider != null)
					{
						_log.Verbose("No {0} found, creating using provider {1}.", "AudioBuffer", AudioBufferProvider.GetType().Name, null, null, "Instance", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 70);
						_instance = AudioBufferProvider.InstantiateAudioBuffer();
					}
					if (!_instance)
					{
						_log.Verbose("No {0} found, creating using {0}.", "AudioBuffer", null, null, null, "Instance", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 75);
						_instance = new GameObject("AudioBuffer").AddComponent<AudioBuffer>();
					}
				}
			}
			return _instance;
		}
	}

	public AudioEncoding AudioEncoding => audioBufferConfiguration.encoding;

	public AudioBufferEvents Events => events;

	public IAudioInputSource MicInput
	{
		get
		{
			return _micInput as IAudioInputSource;
		}
		set
		{
			SetInputSource(value);
		}
	}

	public float MicMinAudioLevel
	{
		get
		{
			if (_micLevelRange != null)
			{
				return _micLevelRange.MinAudioLevel;
			}
			return 0.5f;
		}
	}

	public float MicMaxAudioLevel
	{
		get
		{
			if (_micLevelRange != null)
			{
				return _micLevelRange.MaxAudioLevel;
			}
			return 1f;
		}
	}

	public bool IsInputAvailable => MicInput != null;

	public VoiceAudioInputState AudioState { get; private set; }

	public float MicMaxLevel { get; private set; } = -1f;


	public void OnApplicationQuit()
	{
		_isQuitting = true;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void SingletonInit()
	{
		_isQuitting = false;
	}

	private static bool CanInstantiate()
	{
		if (!_isQuitting)
		{
			return Application.isPlaying;
		}
		return false;
	}

	private IAudioInputSource FindOrCreateInputSource()
	{
		IAudioInputSource audioInputSource = base.gameObject.GetComponentInChildren<IAudioInputSource>(includeInactive: true);
		if (audioInputSource != null)
		{
			return audioInputSource;
		}
		GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			audioInputSource = rootGameObjects[i].GetComponentInChildren<IAudioInputSource>(includeInactive: true);
			if (audioInputSource != null)
			{
				return audioInputSource;
			}
		}
		if (instantiateMic && CanInstantiate())
		{
			_log.Verbose("No input assigned or found, {0} will use Unity Mic Input.", "AudioBuffer", null, null, null, "FindOrCreateInputSource", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 161);
			_instantiatedMic = base.gameObject.AddComponent<Mic>();
			audioInputSource = _instantiatedMic;
		}
		return audioInputSource;
	}

	private void SetInputSource(IAudioInputSource newInput)
	{
		if (MicInput != newInput)
		{
			if ((bool)_instantiatedMic && !object.Equals(_instantiatedMic, newInput))
			{
				_log.Verbose("Replacing default mic.", null, null, null, null, "SetInputSource", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 180);
				UnityEngine.Object.Destroy(_instantiatedMic);
				_instantiatedMic = null;
			}
			bool flag = _recorders.Contains(this);
			if (flag)
			{
				StopRecording(this);
			}
			if (_active)
			{
				SetInputDelegates(add: false);
			}
			if (newInput is UnityEngine.Object micInput)
			{
				_micInput = micInput;
				_log.Verbose("{0} set input of type: {1}", "AudioBuffer", newInput.GetType().Name, null, null, "SetInputSource", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 196);
			}
			else if (newInput == null)
			{
				_log.Warning("{0} setting MicInput to null instead of {1}", "AudioBuffer", "IAudioInputSource");
			}
			else
			{
				_log.Error("{0} cannot set MicInput of type '{1}' since it does not inherit from {2}", "AudioBuffer", newInput.GetType().Name, "Object");
			}
			if (_micInput is IAudioLevelRangeProvider micLevelRange)
			{
				_micLevelRange = micLevelRange;
			}
			if (_active)
			{
				SetInputDelegates(add: true);
			}
			if (flag)
			{
				StartRecording(this);
			}
		}
	}

	private void SetInputDelegates(bool add)
	{
		IAudioInputSource micInput = MicInput;
		if (micInput != null)
		{
			if (add)
			{
				micInput.OnStartRecording += OnMicRecordSuccess;
				micInput.OnStartRecordingFailed += OnMicRecordFailed;
				micInput.OnStopRecording += OnMicRecordStop;
				micInput.OnSampleReady += OnMicSampleReady;
			}
			else
			{
				micInput.OnStartRecording -= OnMicRecordSuccess;
				micInput.OnStartRecordingFailed -= OnMicRecordFailed;
				micInput.OnStopRecording -= OnMicRecordStop;
				micInput.OnSampleReady -= OnMicSampleReady;
			}
		}
	}

	public bool IsRecording(Component component)
	{
		return _recorders.Contains(component);
	}

	private void Awake()
	{
		_instance = this;
		InitializeMicDataBuffer();
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	private void OnEnable()
	{
		if ((bool)_instance && _instance != this)
		{
			_log.Error("Multiple {0} detected. This can lead to extra memory use and unexpected results. Duplicate was found on {1}", "AudioBuffer", base.name);
		}
		if (base.name != "AudioBuffer")
		{
			_log.Verbose("{0} active on {1}", "AudioBuffer", base.name, null, null, "OnEnable", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 332);
		}
		if (MicInput == null)
		{
			MicInput = FindOrCreateInputSource();
		}
		_active = true;
		SetInputDelegates(add: true);
		if (alwaysRecording)
		{
			StartRecording(this);
		}
	}

	private void OnDisable()
	{
		if (alwaysRecording)
		{
			StopRecording(this);
		}
		_active = false;
		SetInputDelegates(add: false);
	}

	private void SetAudioState(VoiceAudioInputState newAudioState)
	{
		AudioState = newAudioState;
		if (AudioState == VoiceAudioInputState.On)
		{
			StopUpdateVolume();
			MicMaxLevel = -1f;
			_volumeUpdate = StartCoroutine(UpdateVolume());
		}
		else if (AudioState == VoiceAudioInputState.Off)
		{
			StopUpdateVolume();
		}
		Events.OnAudioStateChange?.Invoke(AudioState);
	}

	public void StartRecording(Component component)
	{
		if (_recorders.Contains(component))
		{
			return;
		}
		_recorders.Add(component);
		if (AudioState == VoiceAudioInputState.Off || AudioState == VoiceAudioInputState.Deactivating)
		{
			_totalSampleChunks = 0;
			SetAudioState(VoiceAudioInputState.Activating);
			if (!MicInput.IsRecording)
			{
				MicInput.StartRecording(audioBufferConfiguration.sampleLengthInMs);
			}
			else
			{
				OnMicRecordSuccess();
			}
		}
		else if (AudioState == VoiceAudioInputState.On)
		{
			OnMicRecordStarted(component);
		}
	}

	private void OnMicRecordSuccess()
	{
		SetAudioState(VoiceAudioInputState.On);
		foreach (Component recorder in _recorders)
		{
			OnMicRecordStarted(recorder);
		}
	}

	private void OnMicRecordStarted(Component component)
	{
		if (component is IVoiceEventProvider voiceEventProvider)
		{
			voiceEventProvider.VoiceEvents.OnMicStartedListening?.Invoke();
		}
	}

	private void OnMicRecordFailed()
	{
		OnMicRecordStop();
	}

	public void StopRecording(Component component)
	{
		if (!_recorders.Contains(component))
		{
			return;
		}
		if (AudioState == VoiceAudioInputState.On || AudioState == VoiceAudioInputState.Activating)
		{
			SetAudioState(VoiceAudioInputState.Deactivating);
			if (MicInput.IsRecording)
			{
				MicInput.StopRecording();
			}
			else
			{
				OnMicRecordStop();
			}
		}
		else if (AudioState == VoiceAudioInputState.Off)
		{
			OnMicRecordStopped(component);
			_recorders.Remove(component);
		}
	}

	private void OnMicRecordStop()
	{
		HashSet<Component> recorders = _recorders;
		_recorders = new HashSet<Component>();
		foreach (Component item in recorders)
		{
			OnMicRecordStopped(item);
		}
		SetAudioState(VoiceAudioInputState.Off);
	}

	private void OnMicRecordStopped(Component component)
	{
		if (component is IVoiceEventProvider voiceEventProvider)
		{
			voiceEventProvider.VoiceEvents.OnMicStoppedListening?.Invoke();
		}
	}

	private void InitializeMicDataBuffer()
	{
		if (AudioEncoding.numChannels != 1)
		{
			VLog.E(GetType().Name, $"{AudioEncoding.numChannels} audio channels are not currently supported");
			AudioEncoding.numChannels = 1;
		}
		if (!string.Equals(AudioEncoding.encoding, "signed-integer") && !string.Equals(AudioEncoding.encoding, "unsigned-integer"))
		{
			VLog.E(GetType().Name, AudioEncoding.encoding + " encoding is not currently supported");
			AudioEncoding.encoding = "signed-integer";
		}
		if (AudioEncoding.bits != 8 && AudioEncoding.bits != 16 && AudioEncoding.bits != 32 && AudioEncoding.bits != 64)
		{
			VLog.E(GetType().Name, $"{AudioEncoding.bits} bit audio encoding is not currently supported");
			AudioEncoding.bits = 16;
		}
		float num = Mathf.Max(10f, audioBufferConfiguration.micBufferLengthInSeconds * 1000f);
		if (_outputBuffer == null)
		{
			int capacity = AudioEncoding.numChannels * AudioEncoding.samplerate * Mathf.CeilToInt((float)AudioEncoding.bits / 8f * num);
			_outputBuffer = new RingBuffer<byte>(capacity);
		}
	}

	private void OnMicSampleReady(int sampleCount, float[] samples, float levelMax)
	{
		OnAudioSampleReady(samples, 0, samples.Length);
	}

	private void OnAudioSampleReady(float[] samples, int offset, int length)
	{
		if (_sampleReadyCoroutine == null)
		{
			_sampleReadyCoroutine = StartCoroutine(WaitForSampleReady());
		}
		if (_sampleReadyMarker == null)
		{
			_sampleReadyMarker = CreateMarker();
			_sampleReadyMaxLevel = float.MinValue;
		}
		float num = EncodeAndPush(samples, offset, length);
		MicMaxLevel = Mathf.Max(num, MicMaxLevel);
		events.OnSampleReceived?.Invoke(samples, _totalSampleChunks, num);
		_totalSampleChunks++;
		if (num > _sampleReadyMaxLevel)
		{
			_sampleReadyMaxLevel = num;
		}
	}

	private IEnumerator WaitForSampleReady()
	{
		while (AudioState == VoiceAudioInputState.On)
		{
			if (Application.isPlaying && !Application.isBatchMode)
			{
				yield return new WaitForEndOfFrame();
			}
			else
			{
				yield return null;
			}
			if (_sampleReadyMarker != null)
			{
				RingBuffer<byte>.Marker sampleReadyMarker = _sampleReadyMarker;
				_sampleReadyMarker = null;
				CallSampleReady(sampleReadyMarker);
			}
		}
		_sampleReadyCoroutine = null;
	}

	private void CallSampleReady(RingBuffer<byte>.Marker marker)
	{
		if (events.OnByteDataReady != null)
		{
			marker.Clone().ReadIntoWriters(events.OnByteDataReady.Invoke);
		}
		events.OnSampleReady?.Invoke(marker, _sampleReadyMaxLevel);
	}

	private IEnumerator UpdateVolume()
	{
		float volume = -1f;
		while (true)
		{
			if (Application.isBatchMode)
			{
				yield return null;
			}
			else
			{
				yield return new WaitForEndOfFrame();
			}
			if (!volume.Equals(MicMaxLevel) && !MicMaxLevel.Equals(-1f))
			{
				volume = MicMaxLevel;
				events.OnMicLevelChanged?.Invoke(volume);
				MicMaxLevel = -1f;
			}
		}
	}

	private void StopUpdateVolume()
	{
		MicMaxLevel = -1f;
		if (_volumeUpdate != null)
		{
			StopCoroutine(_volumeUpdate);
			_volumeUpdate = null;
		}
	}

	private float EncodeAndPush(float[] samples, int offset, int length)
	{
		if (MicInput.AudioEncoding.samplerate <= 0 || MicInput is IAudioVariableSampleRate { NeedsSampleRateCalculation: not false })
		{
			UpdateSampleRate(length);
			if (MicInput.AudioEncoding.samplerate <= 0)
			{
				return 0f;
			}
		}
		AudioEncoding audioEncoding = MicInput.AudioEncoding;
		int numChannels = audioEncoding.numChannels;
		int samplerate = audioEncoding.samplerate;
		bool flag = string.Equals(audioEncoding.encoding, "signed-integer");
		AudioEncoding audioEncoding2 = AudioEncoding;
		int samplerate2 = audioEncoding2.samplerate;
		int num = Mathf.CeilToInt((float)audioEncoding2.bits / 8f);
		GetEncodingMinMax(audioEncoding2.bits, string.Equals(audioEncoding2.encoding, "signed-integer"), out var encodingMin, out var encodingMax);
		long num2 = encodingMax - encodingMin;
		float num3 = ((samplerate == samplerate2) ? 1f : ((float)samplerate / (float)samplerate2));
		num3 *= (float)numChannels;
		int num4 = (int)((float)length / num3);
		float num5 = 0f;
		for (int i = 0; i < num4; i++)
		{
			int num6 = offset + (int)((float)i * num3);
			float num7 = samples[num6];
			if (flag)
			{
				num7 = num7 / 2f + 0.5f;
			}
			if (num7 > num5)
			{
				num5 = num7;
			}
			long num8 = (long)((float)encodingMin + num7 * (float)num2);
			for (int j = 0; j < num; j++)
			{
				byte data = (byte)(num8 >> j * 8);
				_outputBuffer.Push(data);
			}
		}
		float micMinAudioLevel = MicMinAudioLevel;
		float micMaxAudioLevel = MicMaxAudioLevel;
		if ((!micMinAudioLevel.Equals(0f) || !micMaxAudioLevel.Equals(1f)) && micMaxAudioLevel > micMinAudioLevel)
		{
			num5 = (num5 - micMinAudioLevel) / (micMaxAudioLevel - micMinAudioLevel);
		}
		return Mathf.Clamp01(num5);
	}

	private void GetEncodingMinMax(int bits, bool signed, out long encodingMin, out long encodingMax)
	{
		switch (bits)
		{
		case 8:
			encodingMin = 0L;
			encodingMax = 255L;
			break;
		case 64:
			encodingMin = long.MinValue;
			encodingMax = long.MaxValue;
			break;
		case 32:
			encodingMin = (signed ? int.MinValue : 0);
			encodingMax = (uint)(signed ? int.MaxValue : (-1));
			break;
		default:
			encodingMin = (signed ? (-32768) : 0);
			encodingMax = (signed ? 32767 : 65535);
			break;
		}
	}

	public RingBuffer<byte>.Marker CreateMarker()
	{
		return _outputBuffer.CreateMarker();
	}

	public RingBuffer<byte>.Marker CreateMarker(float offset)
	{
		int offset2 = (int)((float)(AudioEncoding.numChannels * AudioEncoding.samplerate) * offset);
		return _outputBuffer.CreateMarker(offset2);
	}

	private void UpdateSampleRate(int sampleLength)
	{
		if (sampleLength <= 0)
		{
			return;
		}
		long ticks = DateTimeOffset.Now.Ticks;
		long num = ticks - _lastSampleTime;
		_lastSampleTime = ticks;
		if (num > 500000 || _startSampleTime == 0L)
		{
			_startSampleTime = ticks;
			_measureSampleTotal = 0L;
			return;
		}
		int numChannels = MicInput.AudioEncoding.numChannels;
		_measureSampleTotal += Mathf.FloorToInt((float)sampleLength / (float)numChannels);
		long num2 = ticks - _startSampleTime;
		if (num2 >= 2500000)
		{
			double num3 = (double)num2 / 10000000.0;
			double num4 = (double)_measureSampleTotal / num3;
			int num5 = _measuredSampleRateCount % 20;
			_measuredSampleRates[num5] = num4;
			_measuredSampleRateCount++;
			if (_measuredSampleRateCount == 40)
			{
				_measuredSampleRateCount -= 20;
			}
			double averageSampleRate = GetAverageSampleRate(_measuredSampleRates, _measuredSampleRateCount);
			int closestSampleRate = GetClosestSampleRate(averageSampleRate);
			if (MicInput.AudioEncoding.samplerate != closestSampleRate)
			{
				MicInput.AudioEncoding.samplerate = closestSampleRate;
				_log.Info("Input SampleRate Set: {0}\nElapsed: {1:0.000} seconds\nAverage Samples per Second: {2}", closestSampleRate, num3, averageSampleRate, null, "UpdateSampleRate", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Data\\AudioBuffer.cs", 932);
			}
			_startSampleTime = ticks;
			_measureSampleTotal = 0L;
		}
	}

	private static double GetAverageSampleRate(double[] sampleRates, int sampleRateCount)
	{
		int num = Mathf.Min(sampleRateCount, sampleRates.Length);
		if (num <= 0)
		{
			return 0.0;
		}
		double num2 = 0.0;
		for (int i = 0; i < num; i++)
		{
			num2 += sampleRates[i];
		}
		return num2 / (double)num;
	}

	private static int GetClosestSampleRate(double samplesPerSecond)
	{
		int result = 0;
		int num = int.MaxValue;
		int num2 = (int)Math.Round(samplesPerSecond);
		for (int i = 0; i < ALLOWED_SAMPLE_RATES.Length; i++)
		{
			int num3 = ALLOWED_SAMPLE_RATES[i];
			int num4 = Mathf.Abs(num3 - num2);
			if (num4 < num)
			{
				result = num3;
				num = num4;
				continue;
			}
			return result;
		}
		return result;
	}
}
