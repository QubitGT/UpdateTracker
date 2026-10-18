using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;
using UnityEngine.XR;

namespace UnityEngine.InputSystem.XR;

[Preserve]
[InputControlLayout(stateType = typeof(PoseState))]
public class PoseControl : InputControl<PoseState>
{
	public ButtonControl isTracked { get; set; }

	public IntegerControl trackingState { get; set; }

	public Vector3Control position { get; set; }

	public QuaternionControl rotation { get; set; }

	public Vector3Control velocity { get; set; }

	public Vector3Control angularVelocity { get; set; }

	public PoseControl()
	{
		m_StateBlock.format = PoseState.s_Format;
	}

	protected override void FinishSetup()
	{
		isTracked = GetChildControl<ButtonControl>("isTracked");
		trackingState = GetChildControl<IntegerControl>("trackingState");
		position = GetChildControl<Vector3Control>("position");
		rotation = GetChildControl<QuaternionControl>("rotation");
		velocity = GetChildControl<Vector3Control>("velocity");
		angularVelocity = GetChildControl<Vector3Control>("angularVelocity");
		base.FinishSetup();
	}

	public unsafe override PoseState ReadUnprocessedValueFromState(void* statePtr)
	{
		if ((int)m_OptimizedControlDataType == 1349481317)
		{
			return *(PoseState*)((byte*)statePtr + (int)m_StateBlock.byteOffset);
		}
		PoseState result = default(PoseState);
		result.isTracked = isTracked.ReadUnprocessedValueFromStateWithCaching(statePtr) > 0.5f;
		result.trackingState = (InputTrackingState)trackingState.ReadUnprocessedValueFromStateWithCaching(statePtr);
		result.position = position.ReadUnprocessedValueFromStateWithCaching(statePtr);
		result.rotation = rotation.ReadUnprocessedValueFromStateWithCaching(statePtr);
		result.velocity = velocity.ReadUnprocessedValueFromStateWithCaching(statePtr);
		result.angularVelocity = angularVelocity.ReadUnprocessedValueFromStateWithCaching(statePtr);
		return result;
	}

	public unsafe override void WriteValueIntoState(PoseState value, void* statePtr)
	{
		if ((int)m_OptimizedControlDataType == 1349481317)
		{
			*(PoseState*)((byte*)statePtr + (int)m_StateBlock.byteOffset) = value;
			return;
		}
		isTracked.WriteValueIntoState(value.isTracked, statePtr);
		trackingState.WriteValueIntoState((uint)value.trackingState, statePtr);
		position.WriteValueIntoState(value.position, statePtr);
		rotation.WriteValueIntoState(value.rotation, statePtr);
		velocity.WriteValueIntoState(value.velocity, statePtr);
		angularVelocity.WriteValueIntoState(value.angularVelocity, statePtr);
	}

	protected override FourCC CalculateOptimizedControlDataType()
	{
		if (m_StateBlock.sizeInBits == 480 && m_StateBlock.bitOffset == 0 && isTracked.optimizedControlDataType == 1113150533 && trackingState.optimizedControlDataType == 1229870112 && position.optimizedControlDataType == 1447379763 && rotation.optimizedControlDataType == 1364541780 && velocity.optimizedControlDataType == 1447379763 && angularVelocity.optimizedControlDataType == 1447379763 && trackingState.m_StateBlock.byteOffset == isTracked.m_StateBlock.byteOffset + 4 && position.m_StateBlock.byteOffset == isTracked.m_StateBlock.byteOffset + 8 && rotation.m_StateBlock.byteOffset == isTracked.m_StateBlock.byteOffset + 20 && velocity.m_StateBlock.byteOffset == isTracked.m_StateBlock.byteOffset + 36 && angularVelocity.m_StateBlock.byteOffset == isTracked.m_StateBlock.byteOffset + 48)
		{
			return 1349481317;
		}
		return 0;
	}
}
