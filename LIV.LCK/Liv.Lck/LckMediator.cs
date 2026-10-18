using System;
using System.Collections.Generic;

namespace Liv.Lck;

public static class LckMediator
{
	private static readonly Dictionary<string, ILckCamera> _cameras = new Dictionary<string, ILckCamera>();

	private static readonly Dictionary<string, ILckMonitor> _monitors = new Dictionary<string, ILckMonitor>();

	public static event Action<ILckCamera> CameraRegistered;

	public static event Action<ILckCamera> CameraUnregistered;

	public static event Action<ILckMonitor> MonitorRegistered;

	public static event Action<ILckMonitor> MonitorUnregistered;

	public static event Action<string, string> MonitorToCameraAssignment;

	public static void RegisterCamera(ILckCamera camera)
	{
		if (!_cameras.ContainsKey(camera.CameraId))
		{
			_cameras.Add(camera.CameraId, camera);
			LckMediator.CameraRegistered?.Invoke(camera);
			LckLog.Log("ILckCamera registered (id=\"" + camera.CameraId + "\")", "RegisterCamera", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 22);
		}
		else
		{
			LckLog.LogWarning("RegisterCamera called with already registered camera id: \"" + camera.CameraId + "\"", "RegisterCamera", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 26);
		}
	}

	public static void UnregisterCamera(ILckCamera camera)
	{
		if (_cameras.ContainsKey(camera.CameraId))
		{
			_cameras.Remove(camera.CameraId);
			LckMediator.CameraUnregistered?.Invoke(camera);
			LckLog.Log("ILckCamera unregistered (id=\"" + camera.CameraId + "\")", "UnregisterCamera", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 36);
		}
		else
		{
			LckLog.LogWarning("UnregisterCamera called with unknown camera id: \"" + camera.CameraId + "\"", "UnregisterCamera", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 40);
		}
	}

	public static void RegisterMonitor(ILckMonitor monitor)
	{
		if (!_monitors.ContainsKey(monitor.MonitorId))
		{
			_monitors.Add(monitor.MonitorId, monitor);
			LckMediator.MonitorRegistered?.Invoke(monitor);
			LckLog.Log("ILckMonitor registered (id=\"" + monitor.MonitorId + "\")", "RegisterMonitor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 50);
		}
		else
		{
			LckLog.LogWarning("RegisterMonitor called with already registered monitor id: \"" + monitor.MonitorId + "\"", "RegisterMonitor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 54);
		}
	}

	public static void UnregisterMonitor(ILckMonitor monitor)
	{
		if (_monitors.ContainsKey(monitor.MonitorId))
		{
			_monitors.Remove(monitor.MonitorId);
			LckMediator.MonitorUnregistered?.Invoke(monitor);
			LckLog.Log("ILckMonitor unregistered (id=\"" + monitor.MonitorId + "\")", "UnregisterMonitor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 64);
		}
		else
		{
			LckLog.LogWarning("UnregisterMonitor called with unknown monitor id: \"" + monitor.MonitorId + "\"", "UnregisterMonitor", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMediator.cs", 68);
		}
	}

	public static ILckCamera GetCameraById(string id)
	{
		_cameras.TryGetValue(id, out var value);
		return value;
	}

	public static ILckMonitor GetMonitorById(string id)
	{
		_monitors.TryGetValue(id, out var value);
		return value;
	}

	public static IEnumerable<ILckCamera> GetCameras()
	{
		return _cameras.Values;
	}

	public static IEnumerable<ILckMonitor> GetMonitors()
	{
		return _monitors.Values;
	}

	public static void NotifyMixerAboutMonitorForCamera(string monitorId, string cameraId)
	{
		LckMediator.MonitorToCameraAssignment?.Invoke(monitorId, cameraId);
	}
}
