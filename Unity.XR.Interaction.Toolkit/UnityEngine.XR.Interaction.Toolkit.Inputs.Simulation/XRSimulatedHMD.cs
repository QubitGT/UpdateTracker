using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[InputControlLayout(stateType = typeof(XRSimulatedHMDState), isGenericTypeOfDevice = false, displayName = "XR Simulated HMD", updateBeforeRender = true)]
[Preserve]
public class XRSimulatedHMD : XRHMD
{
	protected unsafe override long ExecuteCommand(InputDeviceCommand* commandPtr)
	{
		if (!XRSimulatorUtility.TryExecuteCommand(commandPtr, out var result))
		{
			return base.ExecuteCommand(commandPtr);
		}
		return result;
	}
}
