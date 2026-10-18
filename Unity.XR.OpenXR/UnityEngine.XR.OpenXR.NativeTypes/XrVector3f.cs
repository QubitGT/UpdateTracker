namespace UnityEngine.XR.OpenXR.NativeTypes;

public struct XrVector3f
{
	public float X;

	public float Y;

	public float Z;

	public XrVector3f(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = 0f - z;
	}

	public XrVector3f(Vector3 value)
	{
		X = value.x;
		Y = value.y;
		Z = 0f - value.z;
	}
}
