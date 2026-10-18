using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal struct PostProcessParams
{
	public Material blitMaterial;

	public GraphicsFormat requestColorFormat;

	public static PostProcessParams Create()
	{
		PostProcessParams result = default(PostProcessParams);
		result.blitMaterial = null;
		result.requestColorFormat = GraphicsFormat.None;
		return result;
	}
}
