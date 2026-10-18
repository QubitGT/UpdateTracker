using System;
using System.Collections;
using System.Diagnostics;
using Liv.Lck.Encoding;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckVideoCapturer : ILckVideoCapturer, IDisposable
{
	private readonly ILckVideoTextureProvider _videoTextureProvider;

	private readonly ILckActiveCameraConfigurer _activeCameraConfigurer;

	private readonly ILckPreviewer _previewer;

	private readonly ILckEncoder _encoder;

	private readonly ILckEventBus _eventBus;

	private readonly Stopwatch _captureStopwatch = new Stopwatch();

	private bool _frameHasBeenRendered;

	private double _captureTimeOverflow;

	private double _targetSecondsPerCapture;

	private const string CaptureLoopCoroutineName = "LckCaptureLooper:CaptureLoopCoroutine";

	public bool ForceCaptureAllFrames { get; set; }

	public bool IsCapturing { get; private set; }

	[Preserve]
	public LckVideoCapturer(ILckVideoTextureProvider videoTextureProvider, ILckActiveCameraConfigurer activeCameraConfigurer, ILckPreviewer previewer, ILckEncoder encoder, ILckOutputConfigurer outputConfigurer, ILckEventBus eventBus)
	{
		_videoTextureProvider = videoTextureProvider;
		_activeCameraConfigurer = activeCameraConfigurer;
		_previewer = previewer;
		_encoder = encoder;
		_eventBus = eventBus;
		uint framerate = outputConfigurer.GetActiveCameraTrackDescriptor().Result.Framerate;
		SetTargetCaptureFramerate(framerate);
		_eventBus.AddListener<LckEvents.CameraFramerateChangedEvent>(OnCameraFramerateChanged);
	}

	private void OnCameraFramerateChanged(LckEvents.CameraFramerateChangedEvent cameraFramerateChangedEvent)
	{
		LckResult<uint> result = cameraFramerateChangedEvent.Result;
		if (result.Success)
		{
			SetTargetCaptureFramerate(result.Result);
		}
	}

	public void StartCapturing()
	{
		IsCapturing = true;
		LckMonoBehaviourMediator.StartCoroutine("LckCaptureLooper:CaptureLoopCoroutine", CaptureLoopCoroutine());
	}

	public void StopCapturing()
	{
		IsCapturing = false;
		LckMonoBehaviourMediator.StopCoroutineByName("LckCaptureLooper:CaptureLoopCoroutine");
	}

	public bool HasCurrentFrameBeenCaptured()
	{
		return _frameHasBeenRendered;
	}

	private void SetTargetCaptureFramerate(uint targetCaptureFramerate)
	{
		_targetSecondsPerCapture = 1.0 / (double)targetCaptureFramerate;
	}

	private IEnumerator CaptureLoopCoroutine()
	{
		_captureStopwatch.Start();
		_captureTimeOverflow = 0.0;
		while (IsCapturing)
		{
			HandleCameraFrame();
			yield return null;
		}
	}

	private void PrepareCameraForCapture(ILckCamera camera)
	{
		if (CaptureCanBeCulled())
		{
			_frameHasBeenRendered = false;
			camera.DeactivateCamera();
		}
		else
		{
			_frameHasBeenRendered = true;
			camera.ActivateCamera(_videoTextureProvider.CameraTrackTexture);
		}
	}

	private void HandleCameraFrame(ILckCamera activeCamera)
	{
		double totalSeconds = _captureStopwatch.Elapsed.TotalSeconds;
		bool flag = totalSeconds + _captureTimeOverflow >= _targetSecondsPerCapture;
		if (ForceCaptureAllFrames || flag)
		{
			double num = totalSeconds - _targetSecondsPerCapture;
			_captureTimeOverflow = (_captureTimeOverflow + num) % _targetSecondsPerCapture;
			_captureStopwatch.Restart();
			PrepareCameraForCapture(activeCamera);
		}
		else
		{
			_frameHasBeenRendered = false;
			activeCamera.DeactivateCamera();
		}
	}

	private void HandleCameraFrame()
	{
		LckResult<ILckCamera> activeCamera = _activeCameraConfigurer.GetActiveCamera();
		if (activeCamera.Success)
		{
			ILckCamera result = activeCamera.Result;
			if (result != null)
			{
				HandleCameraFrame(result);
			}
		}
	}

	private bool CaptureCanBeCulled()
	{
		if (_encoder.IsActive())
		{
			return false;
		}
		if (_previewer.IsPreviewActive)
		{
			return false;
		}
		return true;
	}

	public void Dispose()
	{
		if (IsCapturing)
		{
			StopCapturing();
		}
		_eventBus.RemoveListener<LckEvents.CameraFramerateChangedEvent>(OnCameraFramerateChanged);
	}
}
