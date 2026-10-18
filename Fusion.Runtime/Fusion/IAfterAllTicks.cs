namespace Fusion;

public interface IAfterAllTicks : IPublicFacingInterface
{
	void AfterAllTicks(bool resimulation, int tickCount);
}
