using System;
using System.Collections.Generic;
using System.Linq;
using Liv.Lck.Telemetry;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckVideoMixer : ILckVideoMixer, ILckVideoTextureProvider, ILckActiveCameraConfigurer, IDisposable
{
	private ILckCamera _activeCamera;

	private readonly ILckEventBus _eventBus;

	private readonly ILckTelemetryClient _telemetryClient;

	private bool _hasLoggedResolutionError;

	public RenderTexture CameraTrackTexture { get; private set; }

	[Preserve]
	public LckVideoMixer(ILckOutputConfigurer outputConfigurer, ILckEventBus eventBus, ILckTelemetryClient telemetryClient)
	{
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		_eventBus.AddListener<LckEvents.CameraResolutionChangedEvent>(OnResolutionChanged);
		LckMediator.CameraRegistered += OnCameraRegistered;
		LckMediator.CameraUnregistered += OnCameraUnregistered;
		UpdateTextureResolution(outputConfigurer.GetActiveCameraTrackDescriptor().Result.CameraResolutionDescriptor);
	}

	public LckResult<ILckCamera> GetActiveCamera()
	{
		return LckResult<ILckCamera>.NewSuccess(_activeCamera);
	}

	public LckResult ActivateCameraById(string cameraId, string monitorId = null)
	{
		ILckCamera cameraById = LckMediator.GetCameraById(cameraId);
		if (cameraById == null)
		{
			return LckResult.NewError(LckError.CameraIdNotFound, LckResultMessageBuilder.BuildCameraIdNotFoundMessage(cameraId, LckMediator.GetCameras().ToList()));
		}
		_activeCamera?.DeactivateCamera();
		_activeCamera = cameraById;
		_activeCamera.ActivateCamera(CameraTrackTexture);
		TriggerActiveCameraChangedEvent();
		if (!string.IsNullOrEmpty(monitorId))
		{
			LckResult lckResult = UpdateMonitorTexture(monitorId);
			if (!lckResult.Success)
			{
				return lckResult;
			}
		}
		return LckResult.NewSuccess();
	}

	public LckResult StopActiveCamera()
	{
		if (_activeCamera != null)
		{
			_activeCamera.DeactivateCamera();
			_activeCamera = null;
			TriggerActiveCameraChangedEvent();
		}
		return LckResult.NewSuccess();
	}

	public void Dispose()
	{
		ReleaseCameraTrackTextures();
		LckMediator.CameraRegistered -= OnCameraRegistered;
		LckMediator.CameraUnregistered -= OnCameraUnregistered;
	}

	private void TriggerActiveCameraChangedEvent()
	{
		TriggerActiveCameraChangedEvent(LckResult<ILckCamera>.NewSuccess(_activeCamera));
	}

	private void TriggerActiveCameraChangedEvent(LckResult<ILckCamera> result)
	{
		_eventBus.Trigger(new LckEvents.ActiveCameraChangedEvent(result));
	}

	private void ReleaseCameraTrackTextures()
	{
		if ((bool)CameraTrackTexture)
		{
			CameraTrackTexture.Release();
			UnityEngine.Object.Destroy(CameraTrackTexture);
			CameraTrackTexture = null;
			LckLog.Log("Released camera track texture", "ReleaseCameraTrackTextures", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckVideoMixer.cs", 109);
		}
	}

	private LckResult UpdateMonitorTexture(string monitorId)
	{
		ILckMonitor monitorById = LckMediator.GetMonitorById(monitorId);
		if (monitorById == null)
		{
			return LckResult.NewError(LckError.MonitorIdNotFound, LckResultMessageBuilder.BuildMonitorIdNotFoundMessage(monitorId, LckMediator.GetMonitors().ToList()));
		}
		monitorById.SetRenderTexture(CameraTrackTexture);
		return LckResult.NewSuccess();
	}

	private static RenderTexture InitializeTargetRenderTexture(CameraResolutionDescriptor cameraResolutionDescriptor)
	{
		int width = (int)cameraResolutionDescriptor.Width;
		int height = (int)cameraResolutionDescriptor.Height;
		RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D24_UNorm_S8_UInt);
		desc.memoryless = RenderTextureMemoryless.None;
		desc.useMipMap = false;
		desc.msaaSamples = 1;
		desc.sRGB = true;
		RenderTexture renderTexture = new RenderTexture(desc);
		renderTexture.antiAliasing = 1;
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.name = "LCK RenderTexture";
		renderTexture.Create();
		renderTexture.GetNativeTexturePtr();
		renderTexture.GetNativeDepthBufferPtr();
		return renderTexture;
	}

	private void InitCameraTexture(CameraResolutionDescriptor resolution)
	{
		if (!resolution.IsValid())
		{
			throw new ArgumentException($"Invalid resolution: {resolution.Width}x{resolution.Height}");
		}
		ReleaseCameraTrackTextures();
		CameraTrackTexture = InitializeTargetRenderTexture(resolution);
		IEnumerable<ILckCamera> cameras = LckMediator.GetCameras();
		if (!CameraTrackTexture)
		{
			return;
		}
		if (_activeCamera == null)
		{
			using IEnumerator<ILckCamera> enumerator = cameras.GetEnumerator();
			if (enumerator.MoveNext())
			{
				ILckCamera current = enumerator.Current;
				ActivateCameraById(current.CameraId);
			}
		}
		else
		{
			ActivateCameraById(_activeCamera.CameraId);
		}
		_eventBus.Trigger(new LckEvents.ActiveCameraTrackTextureChangedEvent(LckResult<RenderTexture>.NewSuccess(CameraTrackTexture)));
	}

	private void OnCameraRegistered(ILckCamera camera)
	{
	}

	private void OnCameraUnregistered(ILckCamera camera)
	{
		if (_activeCamera == camera)
		{
			StopActiveCamera();
		}
	}

	private void OnResolutionChanged(LckEvents.CameraResolutionChangedEvent cameraResolutionChangedEvent)
	{
		LckResult<CameraResolutionDescriptor> result = cameraResolutionChangedEvent.Result;
		if (!result.Success)
		{
			LckLog.LogWarning("LckVideoMixer ignoring failed camera resolution change (" + cameraResolutionChangedEvent.Result.Message + ")", "OnResolutionChanged", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckVideoMixer.cs", 214);
		}
		else
		{
			UpdateTextureResolution(result.Result);
		}
	}

	private void UpdateTextureResolution(CameraResolutionDescriptor resolution)
	{
		try
		{
			InitCameraTexture(resolution);
		}
		catch (Exception ex)
		{
			if (!_hasLoggedResolutionError)
			{
				_hasLoggedResolutionError = true;
				string arg = _activeCamera?.CameraId ?? "null";
				string arg2 = _activeCamera?.GetType().Name ?? "null";
				string text = ((CameraTrackTexture != null) ? $"{CameraTrackTexture.width}x{CameraTrackTexture.height}, created={CameraTrackTexture.IsCreated()}" : "null");
				int num = LckMediator.GetCameras()?.Count() ?? 0;
				string text2 = ((ex.InnerException != null) ? (", InnerException: " + ex.InnerException.GetType().Name + ": " + ex.InnerException.Message) : "");
				LckLog.LogError("SetTrackResolution failed (" + ex.GetType().Name + "): " + ex.Message + text2 + "\n" + $"Resolution: {resolution.Width}x{resolution.Height}, IsValid: {resolution.IsValid()}\n" + $"ActiveCamera: {arg} ({arg2}), CameraCount: {num}\n" + "CurrentTexture: " + text + "\nStackTrace: " + ex.StackTrace, "UpdateTextureResolution", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckVideoMixer.cs", 242);
			}
		}
	}
}
