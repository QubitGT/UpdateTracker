namespace Assets.OVR.Scripts;

internal class RangedRecord : Record
{
	public float value;

	public float min;

	public float max;

	public RangedRecord(int order, string cat, string msg, float val, float minVal, float maxVal)
		: base(order, cat, msg)
	{
		value = val;
		min = minVal;
		max = maxVal;
	}
}
