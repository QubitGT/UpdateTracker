namespace Fusion;

public interface IBeforeSimulation : IPublicFacingInterface
{
	void BeforeSimulation(int forwardTickCount);
}
