using System;
using System.Diagnostics;

namespace UnityEngine.Rendering;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = false)]
public class CurrentPipelineHelpURLAttribute : HelpURLAttribute
{
	private string pageName { get; }

	private string pageHash { get; }

	public override string URL => string.Empty;

	public CurrentPipelineHelpURLAttribute(string pageName, string pageHash = "")
		: base(null)
	{
		this.pageName = pageName;
		this.pageHash = pageHash;
	}
}
