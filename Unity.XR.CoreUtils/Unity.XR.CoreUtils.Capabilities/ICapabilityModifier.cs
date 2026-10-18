namespace Unity.XR.CoreUtils.Capabilities;

public interface ICapabilityModifier
{
	bool TryGetCapabilityValue(string capabilityKey, out bool capabilityValue);
}
