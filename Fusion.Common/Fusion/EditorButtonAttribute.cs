using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method)]
public class EditorButtonAttribute : Attribute
{
	public string Label;

	public EditorButtonVisibility Visibility;

	public int Priority;

	public bool AllowMultipleTargets;

	public bool DirtyObject;

	public EditorButtonAttribute(string label, EditorButtonVisibility visibility = EditorButtonVisibility.Always, int priority = 0, bool dirtyObject = false)
	{
		Label = label;
		Visibility = visibility;
		Priority = priority;
		DirtyObject = dirtyObject;
	}

	public EditorButtonAttribute(EditorButtonVisibility visibility = EditorButtonVisibility.Always, int priority = 0, bool dirtyObject = false)
		: this(null, visibility, priority, dirtyObject)
	{
	}
}
