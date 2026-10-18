namespace Oculus.Interaction.Input;

public interface IHandSkeletonProvider
{
	HandSkeleton this[Handedness handedness] { get; }
}
