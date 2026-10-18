namespace UnityEngine.XR.OpenXR.NativeTypes;

public struct XrQuaternionf
{
	public float X;

	public float Y;

	public float Z;

	public float W;

	public XrQuaternionf(float x, float y, float z, float w)
	{
		X = 0f - x;
		Y = 0f - y;
		Z = z;
		W = w;
	}

	public XrQuaternionf(Quaternion quaternion)
	{
		X = 0f - quaternion.x;
		Y = 0f - quaternion.y;
		Z = quaternion.z;
		W = quaternion.w;
	}
}
