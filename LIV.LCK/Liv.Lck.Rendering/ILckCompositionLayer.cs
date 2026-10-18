using UnityEngine;

namespace Liv.Lck.Rendering;

public interface ILckCompositionLayer
{
	string Name { get; set; }

	Material BlendMaterial { get; set; }

	bool IsActive { get; set; }

	Texture CurrentTexture { get; }
}
