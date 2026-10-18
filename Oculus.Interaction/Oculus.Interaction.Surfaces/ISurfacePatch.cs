namespace Oculus.Interaction.Surfaces;

public interface ISurfacePatch : ISurface
{
	ISurface BackingSurface { get; }
}
