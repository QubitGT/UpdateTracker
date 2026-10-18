using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[DebuggerDisplay("{reason} : {passId}")]
internal readonly struct LoadAudit
{
	public static readonly string[] LoadReasonMessages = new string[6] { "Invalid reason", "The resource is imported in the graph and loaded to retrieve the existing buffer contents.", "The resource is written by {pass} executed previously in the graph. The data is loaded.", "The resource is imported in the graph but was imported with the 'clear on first use' option enabled. The data is cleared.", "The resource is created in this pass and cleared on first use.", "The pass indicated it will rewrite the full resource contents. Existing contents are not loaded or cleared." };

	public readonly LoadReason reason;

	public readonly int passId;

	public LoadAudit(LoadReason setReason, int setPassId = -1)
	{
		reason = setReason;
		passId = setPassId;
	}
}
