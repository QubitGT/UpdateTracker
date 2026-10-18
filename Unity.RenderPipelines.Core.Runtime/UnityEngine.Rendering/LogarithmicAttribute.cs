namespace UnityEngine.Rendering;

internal class LogarithmicAttribute : PropertyAttribute
{
	public int min;

	public int max;

	public LogarithmicAttribute(int min, int max)
	{
		this.min = min;
		this.max = max;
	}
}
