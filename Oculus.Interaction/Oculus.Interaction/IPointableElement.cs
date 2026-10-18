namespace Oculus.Interaction;

public interface IPointableElement : IPointable
{
	void ProcessPointerEvent(PointerEvent evt);
}
