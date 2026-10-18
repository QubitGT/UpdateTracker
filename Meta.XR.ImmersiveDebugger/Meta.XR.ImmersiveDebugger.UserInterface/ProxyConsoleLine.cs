using Meta.XR.ImmersiveDebugger.UserInterface.Generic;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class ProxyConsoleLine : ProxyController<ConsoleLine>
{
	public LogEntry Entry { get; set; }

	protected override void Fill()
	{
		base.Target.Entry = Entry;
	}
}
