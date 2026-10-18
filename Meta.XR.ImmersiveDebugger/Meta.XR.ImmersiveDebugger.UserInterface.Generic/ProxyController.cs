using System.Collections.Generic;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

internal abstract class ProxyController<ControllerType> where ControllerType : Controller
{
	public ControllerType Target { get; private set; }

	public void Fill(ControllerType target, Dictionary<ControllerType, ProxyController<ControllerType>> targets)
	{
		if (!targets.TryGetValue(target, out var value) || value != this)
		{
			targets[target] = this;
			Target = target;
			Fill();
			Target.RefreshLayout();
		}
	}

	protected abstract void Fill();
}
