namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public interface IXRInputButtonReader : IXRInputValueReader<float>, IXRInputValueReader
{
	bool ReadIsPerformed();

	bool ReadWasPerformedThisFrame();

	bool ReadWasCompletedThisFrame();
}
