namespace UnityEngine.InputSystem.XR.Haptics;

public struct HapticCapabilities
{
	public uint numChannels { get; }

	public bool supportsImpulse { get; }

	public bool supportsBuffer { get; }

	public uint frequencyHz { get; }

	public uint maxBufferSize { get; }

	public uint optimalBufferSize { get; }

	public HapticCapabilities(uint numChannels, bool supportsImpulse, bool supportsBuffer, uint frequencyHz, uint maxBufferSize, uint optimalBufferSize)
	{
		this.numChannels = numChannels;
		this.supportsImpulse = supportsImpulse;
		this.supportsBuffer = supportsBuffer;
		this.frequencyHz = frequencyHz;
		this.maxBufferSize = maxBufferSize;
		this.optimalBufferSize = optimalBufferSize;
	}

	public HapticCapabilities(uint numChannels, uint frequencyHz, uint maxBufferSize)
		: this(numChannels, supportsImpulse: false, supportsBuffer: false, frequencyHz, maxBufferSize, 0u)
	{
	}
}
