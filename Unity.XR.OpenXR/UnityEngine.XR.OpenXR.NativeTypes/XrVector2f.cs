namespace UnityEngine.XR.OpenXR.NativeTypes;

public struct XrVector2f
{
	public float X;

	public float Y;

	public XrVector2f(float x, float y)
	{
		X = x;
		Y = y;
	}

	public XrVector2f(Vector2 value)
	{
		X = value.x;
		Y = value.y;
	}
}
