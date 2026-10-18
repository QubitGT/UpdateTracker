using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XInput.LowLevel;

namespace UnityEngine.InputSystem.XInput;

[InputControlLayout(stateType = typeof(XInputControllerWindowsState), hideInUI = true)]
public class XInputControllerWindows : XInputController
{
}
