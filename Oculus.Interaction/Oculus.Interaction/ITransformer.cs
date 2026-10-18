namespace Oculus.Interaction;

public interface ITransformer
{
	void Initialize(IGrabbable grabbable);

	void BeginTransform();

	void UpdateTransform();

	void EndTransform();
}
