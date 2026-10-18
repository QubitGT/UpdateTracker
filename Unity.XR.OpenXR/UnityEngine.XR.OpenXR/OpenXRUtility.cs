using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR;

public static class OpenXRUtility
{
	private const string LibraryName = "UnityOpenXR";

	public static bool IsSessionFocused => Internal_IsSessionFocused();

	public static bool IsUserPresent => Internal_GetUserPresence();

	private static Pose Inverse(Pose p)
	{
		Pose result = default(Pose);
		result.rotation = Quaternion.Inverse(p.rotation);
		result.position = result.rotation * -p.position;
		return result;
	}

	public static Pose ComputePoseToWorldSpace(Transform t, Camera camera)
	{
		if (camera == null)
		{
			return default(Pose);
		}
		Transform transform = camera.transform;
		Pose lhs = new Pose(transform.localPosition, transform.localRotation);
		Pose p = new Pose(transform.position, transform.rotation);
		return new Pose(t.position, t.rotation).GetTransformedBy(Inverse(p)).GetTransformedBy(lhs);
	}

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_IsSessionFocused")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_IsSessionFocused();

	[DllImport("UnityOpenXR", EntryPoint = "NativeConfig_GetUserPresence")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetUserPresence();
}
