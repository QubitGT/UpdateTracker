namespace Unity.XR.CoreUtils.Bindings;

public interface IEventBinding
{
	bool IsBound { get; }

	void Bind();

	void Unbind();

	void ClearBinding();
}
