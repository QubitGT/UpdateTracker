using System.Diagnostics;

namespace UnityEngine.Rendering.Universal;

[Conditional("UNITY_EDITOR")]
internal class URPHelpURLAttribute : CoreRPHelpURLAttribute
{
	public URPHelpURLAttribute(string pageName, string pageHash = "")
		: base(pageName, pageHash, "com.unity.render-pipelines.universal")
	{
	}
}
