using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public struct OccluderParameters
{
	public int viewInstanceID;

	public int subviewCount;

	public TextureHandle depthTexture;

	public Vector2Int depthSize;

	public bool depthIsArray;

	public OccluderParameters(int viewInstanceID)
	{
		this.viewInstanceID = viewInstanceID;
		subviewCount = 1;
		depthTexture = TextureHandle.nullHandle;
		depthSize = Vector2Int.zero;
		depthIsArray = false;
	}
}
