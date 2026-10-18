using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[DebuggerDisplay("{reason} : {passId} / MSAA {msaaReason} : {msaaPassId}")]
internal readonly struct StoreAudit
{
	public static readonly string[] StoreReasonMessages = new string[7] { "Invalid reason", "The resource is imported in the graph. The data is stored so results are available outside the graph.", "The resource is read by pass {pass} executed later in the graph. The data is stored.", "The resource is imported but the import was with the 'discard on last use' option enabled. The data is discarded.", "The resource is written by this pass but no later passes are using the results. The data is discarded.", "The resource was created as MSAA only resource, the data can never be resolved.", "The resource is a single sample resource, there is no multi-sample data to handle." };

	public readonly StoreReason reason;

	public readonly int passId;

	public readonly StoreReason msaaReason;

	public readonly int msaaPassId;

	public StoreAudit(StoreReason setReason, int setPassId = -1, StoreReason setMsaaReason = StoreReason.NoMSAABuffer, int setMsaaPassId = -1)
	{
		reason = setReason;
		passId = setPassId;
		msaaReason = setMsaaReason;
		msaaPassId = setMsaaPassId;
	}
}
