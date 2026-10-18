using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Settings;
using Liv.Lck.Telemetry;
using Liv.Lck.Utilities;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckPhotoCapture : ILckPhotoCapture, IDisposable
{
	private readonly ILckVideoTextureProvider _videoTextureProvider;

	private readonly ILckEventBus _eventBus;

	private readonly ILckTelemetryClient _telemetryClient;

	private static readonly string[] ImageFileFormatStrings = new string[4] { "exr", "jpg", "tga", "png" };

	private RenderTexture _renderTexture;

	private StringBuilder _imageFilePathBuilder = new StringBuilder(256);

	private Queue<Action> _captureQueue = new Queue<Action>();

	private bool _isCapturing;

	private static readonly ProfilerMarker _copyOutputFileToNativeGalleryProfileMarker = new ProfilerMarker("LckPhotoCapture.CopyOutputFileToPhotoGallery");

	private static readonly ProfilerMarker _captureProfileMarker = new ProfilerMarker("LckPhotoCapture.Capture");

	private static readonly ProfilerMarker _asyncCallbackProfileMarker = new ProfilerMarker("LckPhotoCapture.AsyncCallback");

	private WaitForSecondsRealtime _copyPhotoSpinWait = new WaitForSecondsRealtime(0.1f);

	[Preserve]
	public LckPhotoCapture(ILckVideoTextureProvider videoTextureProvider, ILckEventBus eventBus, ILckTelemetryClient telemetryClient)
	{
		_videoTextureProvider = videoTextureProvider;
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		_renderTexture = _videoTextureProvider.CameraTrackTexture;
		_eventBus.AddListener<LckEvents.ActiveCameraTrackTextureChangedEvent>(OnCameraTrackTextureChanged);
	}

	private void OnCameraTrackTextureChanged(LckEvents.ActiveCameraTrackTextureChangedEvent activeCameraTrackTextureChangedEvent)
	{
		_renderTexture = activeCameraTrackTextureChangedEvent.CameraTrackTextureResult.Result;
	}

	public LckResult Capture()
	{
		if (_renderTexture == null)
		{
			return LckResult.NewError(LckError.PhotoCaptureError, "Failed to capture photo - No render texture set on LckPhotoCapture");
		}
		Dictionary<string, object> context = new Dictionary<string, object>
		{
			{ "photo.targetResolutionX", _renderTexture.width },
			{ "photo.targetResolutionY", _renderTexture.height },
			{
				"photo.format",
				LckSettings.Instance.ImageCaptureFileFormat.ToString()
			}
		};
		_telemetryClient.SendTelemetry(new LckTelemetryEvent(LckTelemetryEventType.PhotoCaptured, context));
		_captureQueue.Enqueue(delegate
		{
			using (_captureProfileMarker.Auto())
			{
				LckSettings.ImageFileFormat imageCaptureFileFormat = LckSettings.Instance.ImageCaptureFileFormat;
				_imageFilePathBuilder.Clear();
				_imageFilePathBuilder.Append(Path.Combine(Application.temporaryCachePath, FileUtility.GenerateFilename(ImageFileFormatStrings[(int)imageCaptureFileFormat])));
				SaveRenderTextureToFile(_imageFilePathBuilder.ToString(), LckSettings.Instance.ImageCaptureFileFormat, OnCaptureComplete);
			}
		});
		if (!_isCapturing)
		{
			ProcessQueue();
		}
		return LckResult.NewSuccess();
	}

	private void ProcessQueue()
	{
		if (_captureQueue.Count > 0 && !_isCapturing)
		{
			_isCapturing = true;
			_captureQueue.Dequeue()();
		}
	}

	private void OnCaptureComplete(LckResult result)
	{
		if (result.Success)
		{
			LckMonoBehaviourMediator.StartCoroutine("CopyImageToGalleryWhenReady", CopyImageToGalleryWhenReady());
			return;
		}
		_eventBus.Trigger(new LckEvents.PhotoCaptureSavedEvent(result));
		_isCapturing = false;
		ProcessQueue();
	}

	private IEnumerator CopyImageToGalleryWhenReady()
	{
		while (FileUtility.IsFileLocked(_imageFilePathBuilder.ToString()) && File.Exists(_imageFilePathBuilder.ToString()))
		{
			yield return _copyPhotoSpinWait;
		}
		using (_copyOutputFileToNativeGalleryProfileMarker.Auto())
		{
			Task task = FileUtility.CopyToGallery(_imageFilePathBuilder.ToString(), LckSettings.Instance.RecordingAlbumName, delegate(bool success, string path)
			{
				LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
				{
					if (success)
					{
						LckLog.Log("LCK Photo saved to gallery: " + path, "CopyImageToGalleryWhenReady", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckPhotoCapture.cs", 133);
						_eventBus.Trigger(new LckEvents.PhotoCaptureSavedEvent(LckResult.NewSuccess()));
					}
					else
					{
						_eventBus.Trigger(new LckEvents.PhotoCaptureSavedEvent(LckResult.NewError(LckError.FailedToCopyPhotoToGallery, "Failed to copy photo to Gallery")));
						LckLog.LogError("LCK Failed to save photo to gallery", "CopyImageToGalleryWhenReady", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckPhotoCapture.cs", 141);
					}
					_isCapturing = false;
					ProcessQueue();
				});
			});
			yield return new WaitUntil(() => task.IsCompleted);
		}
	}

	public void SetRenderTexture(RenderTexture renderTexture)
	{
		_renderTexture = renderTexture;
	}

	private void SaveRenderTextureToFile(string filePath, LckSettings.ImageFileFormat fileFormat, Action<LckResult> onCaptureComplete)
	{
		if (_renderTexture == null)
		{
			onCaptureComplete?.Invoke(LckResult.NewError(LckError.PhotoCaptureError, "RenderTexture is null"));
			return;
		}
		int width = _renderTexture.width;
		int height = _renderTexture.height;
		GraphicsFormat renderTextureGraphicsFormat = _renderTexture.graphicsFormat;
		NativeArray<byte> narray = new NativeArray<byte>(width * height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		AsyncGPUReadback.RequestIntoNativeArray(ref narray, _renderTexture, 0, delegate(AsyncGPUReadbackRequest request)
		{
			using (_asyncCallbackProfileMarker.Auto())
			{
				if (!request.hasError)
				{
					Task.Run(delegate
					{
						NativeArray<byte> nativeArray = default(NativeArray<byte>);
						FillAlphaChannel(narray);
						try
						{
							nativeArray = fileFormat switch
							{
								LckSettings.ImageFileFormat.EXR => ImageConversion.EncodeNativeArrayToEXR(narray, renderTextureGraphicsFormat, (uint)width, (uint)height), 
								LckSettings.ImageFileFormat.JPG => ImageConversion.EncodeNativeArrayToJPG(narray, renderTextureGraphicsFormat, (uint)width, (uint)height, 0u, 95), 
								LckSettings.ImageFileFormat.TGA => ImageConversion.EncodeNativeArrayToTGA(narray, renderTextureGraphicsFormat, (uint)width, (uint)height), 
								_ => ImageConversion.EncodeNativeArrayToPNG(narray, renderTextureGraphicsFormat, (uint)width, (uint)height), 
							};
							File.WriteAllBytes(filePath, nativeArray.ToArray());
						}
						catch
						{
							LckLog.LogError("LCK Failed to encode image during Photo Capture", "SaveRenderTextureToFile", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckPhotoCapture.cs", 212);
							LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
							{
								onCaptureComplete?.Invoke(LckResult.NewError(LckError.PhotoCaptureError, "Failed to save photo to gallery"));
							});
						}
						finally
						{
							if (nativeArray.IsCreated)
							{
								nativeArray.Dispose();
							}
							narray.Dispose();
							LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
							{
								onCaptureComplete?.Invoke(LckResult.NewSuccess());
							});
						}
					});
				}
				else
				{
					narray.Dispose();
					LckMonoBehaviourMediator.Instance.EnqueueMainThreadAction(delegate
					{
						onCaptureComplete?.Invoke(LckResult.NewError(LckError.PhotoCaptureError, "AsyncGPUReadback.RequestIntoNativeArray Failed"));
					});
				}
			}
		});
	}

	private static void FillAlphaChannel(NativeArray<byte> narray)
	{
		for (int i = 0; i < narray.Length; i += 4)
		{
			narray[i + 3] = byte.MaxValue;
		}
	}

	public void Dispose()
	{
		_eventBus?.RemoveListener<LckEvents.ActiveCameraTrackTextureChangedEvent>(OnCameraTrackTextureChanged);
	}
}
