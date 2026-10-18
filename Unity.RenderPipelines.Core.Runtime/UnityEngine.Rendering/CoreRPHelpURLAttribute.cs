using System;
using System.Diagnostics;

namespace UnityEngine.Rendering;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = false)]
public class CoreRPHelpURLAttribute : HelpURLAttribute
{
	public CoreRPHelpURLAttribute(string pageName, string packageName = "com.unity.render-pipelines.core")
		: base(DocumentationInfo.GetPageLink(packageName, pageName, ""))
	{
	}

	public CoreRPHelpURLAttribute(string pageName, string pageHash, string packageName = "com.unity.render-pipelines.core")
		: base(DocumentationInfo.GetPageLink(packageName, pageName, pageHash))
	{
	}
}
