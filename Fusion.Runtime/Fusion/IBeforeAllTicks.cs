namespace Fusion;

public interface IBeforeAllTicks : IPublicFacingInterface
{
	void BeforeAllTicks(bool resimulation, int tickCount);
}
