namespace UnityEngine.XR.OpenXR.NativeTypes;

public struct XrPosef
{
	public XrQuaternionf Orientation;

	public XrVector3f Position;

	public XrPosef(Vector3 vec3, Quaternion quaternion)
	{
		Position = new XrVector3f(vec3);
		Orientation = new XrQuaternionf(quaternion);
	}
}
