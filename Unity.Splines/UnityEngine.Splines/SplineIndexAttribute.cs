namespace UnityEngine.Splines;

public class SplineIndexAttribute : PropertyAttribute
{
	public readonly string SplineContainerProperty;

	public SplineIndexAttribute(string splineContainerProperty)
	{
		SplineContainerProperty = splineContainerProperty;
	}
}
