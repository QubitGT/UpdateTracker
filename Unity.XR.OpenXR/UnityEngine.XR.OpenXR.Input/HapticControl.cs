using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Input;

[Preserve]
public class HapticControl : InputControl<Haptic>
{
	public HapticControl()
	{
		m_StateBlock.sizeInBits = 1u;
		m_StateBlock.bitOffset = 0u;
		m_StateBlock.byteOffset = 0u;
	}

	public unsafe override Haptic ReadUnprocessedValueFromState(void* statePtr)
	{
		return default(Haptic);
	}
}
