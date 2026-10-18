using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public static class OVRPermissionsRequester
{
	public enum Permission
	{
		FaceTracking,
		BodyTracking,
		EyeTracking,
		Scene,
		RecordAudio
	}

	public const string FaceTrackingPermission = "com.oculus.permission.FACE_TRACKING";

	public const string EyeTrackingPermission = "com.oculus.permission.EYE_TRACKING";

	public const string BodyTrackingPermission = "com.oculus.permission.BODY_TRACKING";

	public const string ScenePermission = "com.oculus.permission.USE_SCENE";

	public const string RecordAudioPermission = "android.permission.RECORD_AUDIO";

	public static event Action<string> PermissionGranted;

	public static string GetPermissionId(Permission permission)
	{
		return permission switch
		{
			Permission.FaceTracking => "com.oculus.permission.FACE_TRACKING", 
			Permission.BodyTracking => "com.oculus.permission.BODY_TRACKING", 
			Permission.EyeTracking => "com.oculus.permission.EYE_TRACKING", 
			Permission.Scene => "com.oculus.permission.USE_SCENE", 
			Permission.RecordAudio => "android.permission.RECORD_AUDIO", 
			_ => throw new ArgumentOutOfRangeException("permission", permission, null), 
		};
	}

	private static bool IsPermissionSupportedByPlatform(Permission permission)
	{
		return permission switch
		{
			Permission.FaceTracking => OVRPlugin.faceTrackingSupported || OVRPlugin.faceTracking2Supported, 
			Permission.BodyTracking => OVRPlugin.bodyTrackingSupported, 
			Permission.EyeTracking => OVRPlugin.eyeTrackingSupported, 
			Permission.Scene => true, 
			Permission.RecordAudio => true, 
			_ => throw new ArgumentOutOfRangeException("permission", permission, null), 
		};
	}

	public static bool IsPermissionGranted(Permission permission)
	{
		return true;
	}

	public static void Request(IEnumerable<Permission> permissions)
	{
	}

	private static void RequestPermissions(IEnumerable<Permission> permissions)
	{
		List<string> list = new List<string>();
		foreach (Permission permission in permissions)
		{
			if (ShouldRequestPermission(permission))
			{
				list.Add(GetPermissionId(permission));
			}
		}
		if (list.Count > 0)
		{
			UnityEngine.Android.Permission.RequestUserPermissions(list.ToArray(), BuildPermissionCallbacks());
		}
	}

	private static bool ShouldRequestPermission(Permission permission)
	{
		if (!IsPermissionSupportedByPlatform(permission))
		{
			Debug.LogWarning(string.Format("[[{0}] Permission {1} is not supported by the platform and can't be requested.", "OVRPermissionsRequester", permission));
			return false;
		}
		return !IsPermissionGranted(permission);
	}

	private static PermissionCallbacks BuildPermissionCallbacks()
	{
		PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
		permissionCallbacks.PermissionDenied += delegate(string permissionId)
		{
			Debug.LogWarning("[OVRPermissionsRequester] Permission " + permissionId + " was denied.");
		};
		permissionCallbacks.PermissionGranted += delegate(string permissionId)
		{
			Debug.Log("[OVRPermissionsRequester] Permission " + permissionId + " was granted.");
			OVRPermissionsRequester.PermissionGranted?.Invoke(permissionId);
		};
		return permissionCallbacks;
	}
}
