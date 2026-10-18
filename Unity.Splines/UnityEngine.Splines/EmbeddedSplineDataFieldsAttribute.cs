namespace UnityEngine.Splines;

public class EmbeddedSplineDataFieldsAttribute : PropertyAttribute
{
	public readonly EmbeddedSplineDataField Fields;

	public EmbeddedSplineDataFieldsAttribute(EmbeddedSplineDataField fields)
	{
		Fields = fields;
	}
}
