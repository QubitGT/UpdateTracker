using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT;
using Liv.Lck.Collections;
using Liv.Lck.ErrorHandling;
using Liv.Lck.Telemetry;
using Liv.NGFX;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Liv.Lck.Encoding;

internal class LckEncoder : ILckEncoder, IDisposable
{
	private struct CaptureData
	{
		public NativeRenderBuffer nativeRenderBuffer;

		public uint trackIndex;
	}

	private readonly ILckOutputConfigurer _outputConfigurer;

	private readonly ILckVideoTextureProvider _videoTextureProvider;

	private readonly ILckEventBus _eventBus;

	private readonly ILckTelemetryClient _telemetryClient;

	private readonly ILckCaptureErrorDispatcher _captureErrorDispatcher;

	private readonly IList<LckEncodedPacketHandler> _registeredPacketHandlers = new List<LckEncodedPacketHandler>();

	private readonly HashSet<EncoderConsumer> _activeConsumers = new HashSet<EncoderConsumer>();

	private readonly Dictionary<EncoderConsumer, List<LckEncodedPacketHandler>> _consumerHandlers = new Dictionary<EncoderConsumer, List<LckEncodedPacketHandler>>();

	private readonly LckNativeEncodingApi.AudioTrack[] _audioTracks = new LckNativeEncodingApi.AudioTrack[1];

	private readonly bool[] _readyVideoTracks = new bool[1];

	private static readonly ProfilerMarker _allocateFrameSubmissionMarker = new ProfilerMarker("LckEncoder.AllocateFrameSubmission");

	private static readonly ProfilerMarker _commandBufferMarker = new ProfilerMarker("LckEncoder.CommandBuffer");

	private static readonly ProfilerMarker _releaseNativeRenderBufferMarker = new ProfilerMarker("LckEncoder.ReleaseNativeRenderBuffer");

	private Liv.NGFX.LogLevel _logLevel = Liv.NGFX.LogLevel.Error;

	private IntPtr _encoderContext;

	private Handle<LckNativeEncodingApi.FrameTexture[]> _textureIds;

	private List<LckNativeEncodingApi.FrameTexture> _texturesList;

	private Handle<LckNativeEncodingApi.ResourceData> _resourceInitData;

	private List<CaptureData> _cameraRenderData = new List<CaptureData>();

	private bool _isActive;

	private IntPtr _resourceContext = IntPtr.Zero;

	private bool _disposed;

	private EncoderSessionData _currentEncoderSessionData;

	private static ILckCaptureErrorDispatcher CaptureErrorDispatcher { get; set; }

	[Preserve]
	public LckEncoder(ILckOutputConfigurer outputConfigurer, ILckVideoTextureProvider videoTextureProvider, ILckEventBus eventBus, ILckTelemetryClient telemetryClient, ILckCaptureErrorDispatcher captureErrorDispatcher)
	{
		_outputConfigurer = outputConfigurer;
		_videoTextureProvider = videoTextureProvider;
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		CaptureErrorDispatcher = captureErrorDispatcher;
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public bool IsPaused()
	{
		return _registeredPacketHandlers.All((LckEncodedPacketHandler encodedPacketHandler) => encodedPacketHandler.CaptureStateProvider.IsPaused().Result);
	}

	public LckResult AcquireEncoder(EncoderConsumer consumer, CameraTrackDescriptor descriptor, IEnumerable<LckEncodedPacketHandler> handlers)
	{
		if (_activeConsumers.Contains(consumer))
		{
			return LckResult.NewError(LckError.CaptureAlreadyStarted, $"Consumer '{consumer}' has already acquired the encoder");
		}
		List<LckEncodedPacketHandler> list = new List<LckEncodedPacketHandler>(handlers);
		if (!_isActive)
		{
			LckResult lckResult = StartEncoderInternal(descriptor, list);
			if (!lckResult.Success)
			{
				return lckResult;
			}
		}
		else
		{
			AddEncodedPacketHandlers(list);
		}
		_consumerHandlers[consumer] = list;
		_activeConsumers.Add(consumer);
		LckLog.Log($"Encoder acquired by consumer '{consumer}' (total consumers: {_activeConsumers.Count})", "AcquireEncoder", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 107);
		return LckResult.NewSuccess();
	}

	public async Task<LckResult> ReleaseEncoderAsync(EncoderConsumer consumer, IEnumerable<LckEncodedPacketHandler> handlers)
	{
		if (!_activeConsumers.Remove(consumer))
		{
			return LckResult.NewError(LckError.EncodingError, $"Consumer '{consumer}' has not acquired the encoder");
		}
		foreach (LckEncodedPacketHandler handler in handlers)
		{
			RemoveEncodedPacketHandler(handler);
		}
		_consumerHandlers.Remove(consumer);
		LckLog.Log($"Encoder released by consumer '{consumer}' (remaining consumers: {_activeConsumers.Count})", "ReleaseEncoderAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 126);
		if (_activeConsumers.Count == 0)
		{
			return await StopEncoderInternal();
		}
		return LckResult.NewSuccess();
	}

	private LckResult StartEncoderInternal(CameraTrackDescriptor cameraTrackDescriptor, IEnumerable<LckEncodedPacketHandler> initialHandlers = null)
	{
		if (!CreateEncoderInstance())
		{
			return LckResult.NewError(LckError.EncodingError, "Failed to create encoder instance");
		}
		if (initialHandlers != null)
		{
			AddEncodedPacketHandlers(initialHandlers);
		}
		_resourceContext = LckNativeEncodingApi.GetResourceContext(_encoderContext);
		if (_resourceContext == IntPtr.Zero)
		{
			return LckResult.NewError(LckError.EncodingError, "Resource context pointer is not set");
		}
		_resourceInitData = new Handle<LckNativeEncodingApi.ResourceData>(new LckNativeEncodingApi.ResourceData
		{
			encoderContext = _encoderContext
		});
		LckNativeEncodingApi.TrackInfo[] array = CreateTrackInfoInteropData(cameraTrackDescriptor, _outputConfigurer.GetAudioSampleRate().Result, _outputConfigurer.GetNumberOfAudioChannels().Result);
		LckResult lckResult = InitCameraRenderData(array);
		if (!lckResult.Success)
		{
			return lckResult;
		}
		if (!LckNativeEncodingApi.StartEncoder(_encoderContext, array, (uint)array.Length))
		{
			return LckResult.NewError(LckError.EncodingError, "Failed to start native encoder");
		}
		ExecuteNativeInitResourcesFunction();
		InitTextureHandles();
		_isActive = true;
		_currentEncoderSessionData = default(EncoderSessionData);
		LckLog.Log("Encoder started successfully", "StartEncoderInternal", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 181);
		LckResult result = LckResult.NewSuccess();
		_eventBus.Trigger(new LckEvents.EncoderStartedEvent(result));
		return result;
	}

	private LckResult StopNativeEncoder()
	{
		_isActive = false;
		try
		{
			LckNativeEncodingApi.StopEncoder(_encoderContext);
			return LckResult.NewSuccess();
		}
		catch (Exception ex)
		{
			LckLog.LogError($"An exception occurred while stopping the encoder: {ex}", "StopNativeEncoder", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 198);
			return LckResult.NewError(LckError.EncodingError, ex.Message);
		}
	}

	private LckResult FinalizeEncoderStop()
	{
		UnregisterEncodedPacketHandlers();
		_activeConsumers.Clear();
		_consumerHandlers.Clear();
		ReleaseResources();
		_encoderContext = IntPtr.Zero;
		LckLog.Log("Encoding stopped successfully", "FinalizeEncoderStop", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 216);
		LckResult result = LckResult.NewSuccess();
		_eventBus.Trigger(new LckEvents.EncoderStoppedEvent(result));
		return result;
	}

	private async Task<LckResult> StopEncoderInternal()
	{
		LckResult lckResult = await Task.Run((Func<LckResult>)StopNativeEncoder);
		if (!lckResult.Success)
		{
			return lckResult;
		}
		return FinalizeEncoderStop();
	}

	public bool EncodeFrame(float videoTimeSeconds, AudioBuffer audioData, bool encodeVideo)
	{
		if (!IsActive())
		{
			LckLog.LogError("Cannot encode frame - encoder is not open", "EncodeFrame", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 236);
			return false;
		}
		try
		{
			ProvideDataToEncoder(videoTimeSeconds, audioData, encodeVideo);
		}
		catch (Exception ex)
		{
			HandleEncodeFrameError("LCK EncodeFrame failed: " + ex.Message);
			return false;
		}
		return true;
	}

	public void SetLogLevel(Liv.NGFX.LogLevel logLevel)
	{
		_logLevel = logLevel;
		if (_encoderContext != IntPtr.Zero)
		{
			LckNativeEncodingApi.SetEncoderLogLevel(_encoderContext, (uint)_logLevel);
		}
	}

	public EncoderSessionData GetCurrentSessionData()
	{
		return _currentEncoderSessionData;
	}

	private void ProvideDataToEncoder(float videoTime, AudioBuffer audioData, bool encodeVideo)
	{
		using Handle<float[]> handle = new Handle<float[]>(audioData.Buffer);
		_audioTracks[0].data = handle.ptr();
		_audioTracks[0].dataSize = (uint)audioData.Count;
		_audioTracks[0].timestampSamples = _currentEncoderSessionData.EncodedAudioSamplesPerChannel;
		_audioTracks[0].trackIndex = 0u;
		_readyVideoTracks[0] = encodeVideo;
		EncodeFrameData(AllocateFrameSubmission(videoTime, _readyVideoTracks, _audioTracks));
		if (_readyVideoTracks[0])
		{
			_currentEncoderSessionData.EncodedVideoFrames++;
		}
		_currentEncoderSessionData.EncodedAudioSamplesPerChannel += _audioTracks[0].dataSize / _outputConfigurer.GetNumberOfAudioChannels().Result;
		_currentEncoderSessionData.CaptureTimeSeconds = videoTime;
	}

	private void AddEncodedPacketHandler(LckEncodedPacketHandler handler)
	{
		if (_encoderContext == IntPtr.Zero)
		{
			LckLog.LogError("Cannot add encoded packet handler - invalid encoder context", "AddEncodedPacketHandler", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 294);
			return;
		}
		if (!handler.EncodedPacketCallback.IsValid)
		{
			LckLog.LogError("Cannot add encoded packet handler - missing callback object or function pointer", "AddEncodedPacketHandler", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 300);
			return;
		}
		if (_registeredPacketHandlers.Contains(handler))
		{
			LckLog.LogError("Cannot add encoded packet handler - it is already registered", "AddEncodedPacketHandler", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 306);
			return;
		}
		_registeredPacketHandlers.Add(handler);
		LckEncodedPacketCallback encodedPacketCallback = handler.EncodedPacketCallback;
		LckNativeEncodingApi.AddEncoderPacketCallback(_encoderContext, encodedPacketCallback.CallbackObjectPtr, encodedPacketCallback.CallbackFunctionPtr);
		LckLog.Log("Encoder packet handler added", "AddEncodedPacketHandler", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 316);
	}

	private void AddEncodedPacketHandlers(IEnumerable<LckEncodedPacketHandler> encodedPacketHandlers)
	{
		foreach (LckEncodedPacketHandler encodedPacketHandler in encodedPacketHandlers)
		{
			AddEncodedPacketHandler(encodedPacketHandler);
		}
	}

	private void RemoveEncodedPacketHandler(LckEncodedPacketHandler handler)
	{
		if (!_registeredPacketHandlers.Remove(handler))
		{
			LckLog.LogError("Cannot remove encoded packet handler - it is not registered", "RemoveEncodedPacketHandler", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 331);
			return;
		}
		LckEncodedPacketCallback encodedPacketCallback = handler.EncodedPacketCallback;
		LckNativeEncodingApi.RemoveEncoderPacketCallback(_encoderContext, encodedPacketCallback.CallbackObjectPtr, encodedPacketCallback.CallbackFunctionPtr);
		LckLog.Log("Removed encoded packet handler", "RemoveEncodedPacketHandler", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 340);
	}

	private bool CreateEncoderInstance()
	{
		if (_encoderContext != IntPtr.Zero)
		{
			LckLog.LogWarning("Encoder context is already set", "CreateEncoderInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 347);
			return false;
		}
		_encoderContext = LckNativeEncodingApi.CreateEncoder();
		if (_encoderContext == IntPtr.Zero)
		{
			LckLog.LogError("Failed to create native encoder", "CreateEncoderInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 354);
			return false;
		}
		LckNativeEncodingApi.SetEncoderLogLevel(_encoderContext, (uint)_logLevel);
		if (!LckNativeEncodingApi.SetCaptureErrorCallback(_encoderContext, OnNativeCaptureError))
		{
			LckLog.LogError("Failed to set encoder error callback", "CreateEncoderInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 362);
			return false;
		}
		LckLog.Log("Encoder created successfully", "CreateEncoderInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 366);
		return true;
	}

	private IntPtr AllocateFrameSubmission(float frameTime, bool[] readyTracks, LckNativeEncodingApi.AudioTrack[] audioTracks)
	{
		LckNativeEncodingApi.FrameSubmission frame = default(LckNativeEncodingApi.FrameSubmission);
		frame.encoderContext = _encoderContext;
		frame.textureIDs = _textureIds.ptr();
		frame.textureIDsSize = (uint)_textureIds.data().Length;
		frame.videoTimestampMilli = (ulong)(frameTime * 1000f);
		frame.audioTracksSize = 1u;
		frame.readyFramesSize = 1u;
		return LckNativeEncodingApi.AllocateFrameSubmission(frame, audioTracks, readyTracks);
	}

	private static void EncodeFrameData(IntPtr framePtr)
	{
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(LckNativeEncodingApi.GetPluginUpdateFunction(), 1, framePtr);
		commandBuffer.name = "qck Encoder";
		Graphics.ExecuteCommandBuffer(commandBuffer);
	}

	public void ReleaseNativeRenderBuffers()
	{
		using (_releaseNativeRenderBufferMarker.Auto())
		{
			if (IsActive())
			{
				LckLog.LogWarning("LCK can't release native render buffers while encoder is active", "ReleaseNativeRenderBuffers", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 406);
				return;
			}
			foreach (CaptureData cameraRenderDatum in _cameraRenderData)
			{
				cameraRenderDatum.nativeRenderBuffer.Dispose();
			}
		}
	}

	public int GetAudioFrameSize()
	{
		return (int)LckNativeEncodingApi.GetAudioTrackFrameSize(_encoderContext, 0u);
	}

	private void ReleaseResources()
	{
		LckLog.Log("Releasing encoder resources", "ReleaseResources", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 425);
		ReleaseNativeRenderBuffers();
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(LckNativeEncodingApi.GetReleaseResourcesFunction(), 1, _resourceInitData.ptr());
		commandBuffer.name = "qck ReleaseResource";
		Graphics.ExecuteCommandBuffer(commandBuffer);
	}

	private LckResult InitCameraRenderData(LckNativeEncodingApi.TrackInfo[] trackInfo)
	{
		_cameraRenderData = new List<CaptureData>();
		(LckNativeEncodingApi.TrackInfo, int)[] array = trackInfo.Select((LckNativeEncodingApi.TrackInfo track, int trackIndex) => (track: track, trackIndex: trackIndex)).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			var (trackInfo2, trackIndex2) = array[i];
			if (trackInfo2.type == LckNativeEncodingApi.TrackType.Video)
			{
				_cameraRenderData.Add(InitCameraRenderData(trackIndex2));
			}
		}
		if (!_cameraRenderData.Any())
		{
			return LckResult.NewError(LckError.EncodingError, "No video tracks found");
		}
		return LckResult.NewSuccess();
	}

	private CaptureData InitCameraRenderData(int trackIndex)
	{
		bool flag = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3;
		RenderTexture cameraTrackTexture = _videoTextureProvider.CameraTrackTexture;
		CaptureData result = default(CaptureData);
		result.nativeRenderBuffer = (flag ? new NativeRenderBuffer(_resourceContext, cameraTrackTexture.colorBuffer, cameraTrackTexture.GetNativeTexturePtr(), cameraTrackTexture.width, cameraTrackTexture.height, 1, GraphicsFormat.R8G8B8A8_UNorm) : new NativeRenderBuffer(_resourceContext, cameraTrackTexture.colorBuffer, cameraTrackTexture.width, cameraTrackTexture.height, 1, GraphicsFormat.R8G8B8A8_UNorm));
		result.trackIndex = (uint)trackIndex;
		return result;
	}

	private void InitTextureHandles()
	{
		_texturesList = new List<LckNativeEncodingApi.FrameTexture>();
		foreach (CaptureData cameraRenderDatum in _cameraRenderData)
		{
			_texturesList.Add(new LckNativeEncodingApi.FrameTexture
			{
				id = cameraRenderDatum.nativeRenderBuffer.id,
				trackIndex = cameraRenderDatum.trackIndex
			});
		}
		_textureIds = new Handle<LckNativeEncodingApi.FrameTexture[]>(_texturesList.ToArray());
	}

	private void ExecuteNativeInitResourcesFunction()
	{
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.IssuePluginEventAndData(LckNativeEncodingApi.GetInitResourcesFunction(), 1, _resourceInitData.ptr());
		commandBuffer.name = "qck InitResource";
		Graphics.ExecuteCommandBuffer(commandBuffer);
	}

	private void UnregisterEncodedPacketHandlers()
	{
		LckEncodedPacketHandler[] array = _registeredPacketHandlers.ToArray();
		foreach (LckEncodedPacketHandler handler in array)
		{
			RemoveEncodedPacketHandler(handler);
		}
	}

	private void HandleEncodeFrameError(string errorMessage)
	{
		LckLog.LogError(errorMessage, "HandleEncodeFrameError", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 502);
		StopEncoderInternal();
	}

	private static LckNativeEncodingApi.TrackInfo[] CreateTrackInfoInteropData(CameraTrackDescriptor cameraTrackDescriptor, uint audioSampleRate, uint numberOfAudioChannels)
	{
		return new LckNativeEncodingApi.TrackInfo[2]
		{
			new LckNativeEncodingApi.TrackInfo
			{
				type = LckNativeEncodingApi.TrackType.Audio,
				bitrate = cameraTrackDescriptor.AudioBitrate,
				samplerate = audioSampleRate,
				channels = numberOfAudioChannels
			},
			new LckNativeEncodingApi.TrackInfo
			{
				type = LckNativeEncodingApi.TrackType.Video,
				bitrate = cameraTrackDescriptor.Bitrate,
				width = cameraTrackDescriptor.CameraResolutionDescriptor.Width,
				height = cameraTrackDescriptor.CameraResolutionDescriptor.Height,
				framerate = cameraTrackDescriptor.Framerate
			}
		};
	}

	[MonoPInvokeCallback(typeof(LckNativeEncodingApi.CaptureErrorCallback))]
	private static void OnNativeCaptureError(CaptureErrorType errorType, string errorMessage)
	{
		if (CaptureErrorDispatcher != null)
		{
			CaptureErrorDispatcher.PushError(new LckCaptureError(errorType, errorMessage));
		}
		else
		{
			LckLog.LogError("The CaptureErrorDispatcher reference is null while error occurred - Error will not be handled: " + errorMessage, "OnNativeCaptureError", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 539);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		if (IsActive())
		{
			LckResult lckResult = StopNativeEncoder();
			if (lckResult.Success)
			{
				FinalizeEncoderStop();
			}
			else
			{
				LckLog.LogError("LckEncoder was disposed while active, but failed to stop encoding: " + lckResult.Message, "Dispose", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Encoding\\LckEncoder.cs", 557);
			}
		}
		CaptureErrorDispatcher = null;
		_disposed = true;
	}
}
