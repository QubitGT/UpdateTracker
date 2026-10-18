using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class FieldEditorButtonAttribute : DecoratingPropertyAttribute
{
	public string Label;

	public bool AllowMultipleTargets;

	public string TargetMethod;

	public FieldEditorButtonAttribute(string label, string targetMethod)
	{
		Label = label;
		TargetMethod = targetMethod;
	}
}
