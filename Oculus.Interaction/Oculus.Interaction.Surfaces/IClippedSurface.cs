using System.Collections.Generic;

namespace Oculus.Interaction.Surfaces;

public interface IClippedSurface<TClipper> : ISurfacePatch, ISurface
{
	IReadOnlyList<TClipper> GetClippers();
}
