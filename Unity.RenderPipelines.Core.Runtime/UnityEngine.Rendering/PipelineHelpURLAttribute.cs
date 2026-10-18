using System;
using System.Diagnostics;

namespace UnityEngine.Rendering;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = true)]
public class PipelineHelpURLAttribute : HelpURLAttribute
{
	private string pipelineName { get; }

	private string pageName { get; }

	private string pageHash { get; }

	public override string URL => string.Empty;

	public PipelineHelpURLAttribute(string pipelineName, string pageName, string pageHash = "")
		: base(null)
	{
		this.pipelineName = pipelineName;
		this.pageName = pageName;
		this.pageHash = pageHash;
	}
}
