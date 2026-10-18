namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public interface IXRInputValueReader
{
}
public interface IXRInputValueReader<TValue> : IXRInputValueReader where TValue : struct
{
	TValue ReadValue();

	bool TryReadValue(out TValue value);
}
