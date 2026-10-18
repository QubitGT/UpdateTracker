using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckPreviewer : ILckPreviewer, IDisposable
{
	private readonly ILckVideoTextureProvider _videoTextureProvider;

	private readonly ILckEventBus _eventBus;

	public bool IsPreviewActive { get; set; } = true;


	[Preserve]
	public LckPreviewer(ILckVideoTextureProvider videoTextureProvider, ILckEventBus eventBus)
	{
		_videoTextureProvider = videoTextureProvider;
		_eventBus = eventBus;
		_eventBus.AddListener<LckEvents.ActiveCameraTrackTextureChangedEvent>(OnCameraTrackTextureChanged);
		LckMediator.MonitorRegistered += OnMonitorRegistered;
		LckMediator.MonitorUnregistered += OnMonitorUnregistered;
	}

	private void SetMonitorRenderTexture(ILckMonitor monitor)
	{
		RenderTexture cameraTrackTexture = _videoTextureProvider.CameraTrackTexture;
		if (cameraTrackTexture == null)
		{
			LckLog.LogWarning("LCK Camera track texture not found.", "SetMonitorRenderTexture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckPreviewer.cs", 29);
		}
		else if (monitor == null)
		{
			LckLog.LogWarning("LCK Monitor not found.", "SetMonitorRenderTexture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckPreviewer.cs", 35);
		}
		else
		{
			monitor.SetRenderTexture(cameraTrackTexture);
		}
	}

	private void OnMonitorRegistered(ILckMonitor monitor)
	{
		SetMonitorRenderTexture(monitor);
	}

	private static void OnMonitorUnregistered(ILckMonitor monitor)
	{
		monitor?.SetRenderTexture(null);
	}

	private void SetMonitorTextureForAllMonitors()
	{
		foreach (ILckMonitor monitor in LckMediator.GetMonitors())
		{
			SetMonitorRenderTexture(monitor);
		}
	}

	private void OnCameraTrackTextureChanged(LckEvents.ActiveCameraTrackTextureChangedEvent activeCameraTrackTextureChangedEvent)
	{
		SetMonitorTextureForAllMonitors();
	}

	public void Dispose()
	{
		_eventBus?.RemoveListener<LckEvents.ActiveCameraTrackTextureChangedEvent>(OnCameraTrackTextureChanged);
		LckMediator.MonitorRegistered -= OnMonitorRegistered;
		LckMediator.MonitorUnregistered -= OnMonitorUnregistered;
	}
}
