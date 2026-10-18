using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

public class ExampleCustomIntegrationConfig : CustomIntegrationConfigBase
{
	public override Camera GetCamera()
	{
		return GameObject.Find("MainCamera").GetComponent<Camera>();
	}
}
