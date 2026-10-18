using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[Obsolete("ForwardRendererData has been deprecated (UnityUpgradable) -> UniversalRendererData", true)]
[ReloadGroup]
[ExcludeFromPreset]
public class ForwardRendererData : ScriptableRendererData
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[Reload("Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader blitPS;

		[Reload("Shaders/Utils/CopyDepth.shader", ReloadAttribute.Package.Root)]
		public Shader copyDepthPS;

		[Obsolete("Obsolete, this feature will be supported by new 'ScreenSpaceShadows' renderer feature", true)]
		public Shader screenSpaceShadowPS;

		[Reload("Shaders/Utils/Sampling.shader", ReloadAttribute.Package.Root)]
		public Shader samplingPS;

		[Reload("Shaders/Utils/StencilDeferred.shader", ReloadAttribute.Package.Root)]
		public Shader stencilDeferredPS;

		[Reload("Shaders/Utils/FallbackError.shader", ReloadAttribute.Package.Root)]
		public Shader fallbackErrorPS;

		[Reload("Shaders/Utils/FallbackLoading.shader", ReloadAttribute.Package.Root)]
		public Shader fallbackLoadingPS;

		[Obsolete("Use fallbackErrorPS instead", true)]
		[Reload("Shaders/Utils/MaterialError.shader", ReloadAttribute.Package.Root)]
		public Shader materialErrorPS;

		[Reload("Shaders/Utils/CoreBlit.shader", ReloadAttribute.Package.Root)]
		[SerializeField]
		internal Shader coreBlitPS;

		[Reload("Shaders/Utils/CoreBlitColorAndDepth.shader", ReloadAttribute.Package.Root)]
		[SerializeField]
		internal Shader coreBlitColorAndDepthPS;

		[Reload("Shaders/CameraMotionVectors.shader", ReloadAttribute.Package.Root)]
		public Shader cameraMotionVector;

		[Reload("Shaders/ObjectMotionVectors.shader", ReloadAttribute.Package.Root)]
		public Shader objectMotionVector;
	}

	private const string k_ErrorMessage = "ForwardRendererData has been deprecated. Use UniversalRendererData instead";

	public ShaderResources shaders;

	public PostProcessData postProcessData;

	public XRSystemData xrSystemData;

	[SerializeField]
	private LayerMask m_OpaqueLayerMask;

	[SerializeField]
	private LayerMask m_TransparentLayerMask;

	[SerializeField]
	private StencilStateData m_DefaultStencilState;

	[SerializeField]
	private bool m_ShadowTransparentReceive;

	[SerializeField]
	private RenderingMode m_RenderingMode;

	[SerializeField]
	private DepthPrimingMode m_DepthPrimingMode;

	[SerializeField]
	private bool m_AccurateGbufferNormals;

	[SerializeField]
	private bool m_ClusteredRendering;

	[SerializeField]
	private TileSize m_TileSize;

	public LayerMask opaqueLayerMask
	{
		get
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
		set
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
	}

	public LayerMask transparentLayerMask
	{
		get
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
		set
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
	}

	public StencilStateData defaultStencilState
	{
		get
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
		set
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
	}

	public bool shadowTransparentReceive
	{
		get
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
		set
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
	}

	public RenderingMode renderingMode
	{
		get
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
		set
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
	}

	public bool accurateGbufferNormals
	{
		get
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
		set
		{
			throw new NotSupportedException("ForwardRendererData has been deprecated. Use UniversalRendererData instead");
		}
	}

	protected override ScriptableRenderer Create()
	{
		Debug.LogWarning("Forward Renderer Data has been deprecated, " + base.name + " will be upgraded to a UniversalRendererData.");
		return null;
	}
}
