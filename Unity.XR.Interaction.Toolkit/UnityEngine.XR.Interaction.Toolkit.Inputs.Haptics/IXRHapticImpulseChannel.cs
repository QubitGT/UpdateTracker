namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public interface IXRHapticImpulseChannel
{
	bool SendHapticImpulse(float amplitude, float duration, float frequency = 0f);
}
