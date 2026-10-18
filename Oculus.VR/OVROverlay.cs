using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OVR.OpenVR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

[ExecuteInEditMode]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-ovroverlay/")]
public class OVROverlay : MonoBehaviour
{
	public enum OverlayShape
	{
		Quad = 0,
		Cylinder = 1,
		Cubemap = 2,
		OffcenterCubemap = 4,
		Equirect = 5,
		ReconstructionPassthrough = 7,
		SurfaceProjectedPassthrough = 8,
		Fisheye = 9,
		KeyboardHandsPassthrough = 10,
		KeyboardMaskedHandsPassthrough = 11
	}

	public enum OverlayType
	{
		None,
		Underlay,
		Overlay
	}

	public delegate void ExternalSurfaceObjectCreated();

	protected struct LayerTexture
	{
		public Texture appTexture;

		public IntPtr appTexturePtr;

		public Texture[] swapChain;

		public IntPtr[] swapChainPtr;
	}

	[Tooltip("Specify overlay's type")]
	public OverlayType currentOverlayType = OverlayType.Overlay;

	[Tooltip("If true, the texture's content is copied to the compositor each frame.")]
	public bool isDynamic;

	[Tooltip("If true, the layer would be used to present protected content (e.g. HDCP), the content won't be shown in screenshots or recordings.")]
	public bool isProtectedContent;

	public Rect srcRectLeft = new Rect(0f, 0f, 1f, 1f);

	public Rect srcRectRight = new Rect(0f, 0f, 1f, 1f);

	public Rect destRectLeft = new Rect(0f, 0f, 1f, 1f);

	public Rect destRectRight = new Rect(0f, 0f, 1f, 1f);

	public bool invertTextureRects;

	private OVRPlugin.TextureRectMatrixf textureRectMatrix = OVRPlugin.TextureRectMatrixf.zero;

	public bool overrideTextureRectMatrix;

	public bool overridePerLayerColorScaleAndOffset;

	public Vector4 colorScale = Vector4.one;

	public Vector4 colorOffset = Vector4.zero;

	public bool useExpensiveSuperSample;

	public bool useExpensiveSharpen;

	public bool hidden;

	[Tooltip("If true, the layer will be created as an external surface. externalSurfaceObject contains the Surface object. It's effective only on Android.")]
	public bool isExternalSurface;

	[Tooltip("The width which will be used to create the external surface. It's effective only on Android.")]
	public int externalSurfaceWidth;

	[Tooltip("The height which will be used to create the external surface. It's effective only on Android.")]
	public int externalSurfaceHeight;

	[Tooltip("The compositionDepth defines the order of the OVROverlays in composition. The overlay/underlay with smaller compositionDepth would be composited in the front of the overlay/underlay with larger compositionDepth.")]
	public int compositionDepth;

	private int layerCompositionDepth;

	[Tooltip("The noDepthBufferTesting will stop layer's depth buffer compositing even if the engine has \"Shared Depth Buffer\" enabled. The layer's ordering will be used instead which is determined by it's composition depth and overlay/underlay type.")]
	public bool noDepthBufferTesting = true;

	public OVRPlugin.EyeTextureFormat layerTextureFormat;

	[Tooltip("Specify overlay's shape")]
	public OverlayShape currentOverlayShape;

	private OverlayShape prevOverlayShape;

	[Tooltip("The left- and right-eye Textures to show in the layer.")]
	public Texture[] textures = new Texture[2];

	[Tooltip("When checked, the texture is treated as if the alpha was already premultiplied")]
	public bool isAlphaPremultiplied;

	[Tooltip("When checked, the layer will use bicubic filtering")]
	public bool useBicubicFiltering;

	[Tooltip("When checked, the cubemap will retain the legacy rotation which was rotated 180 degrees around the Y axis comapred to Unity's definition of cubemaps. This setting will be deprecated in the near future, therefore it is recommended to fix the cubemap texture instead.")]
	public bool useLegacyCubemapRotation;

	[Tooltip("When checked, the layer will use efficient super sampling")]
	public bool useEfficientSupersample;

	[Tooltip("When checked, the layer will use efficient sharpen.")]
	public bool useEfficientSharpen;

	[Tooltip("When checked, The runtime automatically chooses the appropriate sharpening or super sampling filter")]
	public bool useAutomaticFiltering;

	[SerializeField]
	internal bool _previewInEditor;

	protected IntPtr[] texturePtrs = new IntPtr[2]
	{
		IntPtr.Zero,
		IntPtr.Zero
	};

	public IntPtr externalSurfaceObject;

	public ExternalSurfaceObjectCreated externalSurfaceObjectCreated;

	protected bool isOverridePending;

	public static List<OVROverlay> instances = new List<OVROverlay>();

	protected static Material tex2DMaterial;

	protected static readonly Material[] cubeMaterial = new Material[6];

	protected LayerTexture[] layerTextures;

	protected OVRPlugin.LayerDesc layerDesc;

	protected int stageCount = -1;

	protected GCHandle layerIdHandle;

	protected IntPtr layerIdPtr = IntPtr.Zero;

	protected int frameIndex;

	protected int prevFrameIndex = -1;

	protected Renderer rend;

	private static readonly int _tempRenderTextureId = Shader.PropertyToID("_OVROverlayTempTexture");

	private CommandBuffer _commandBuffer;

	private Mesh _blitMesh;

	private ulong OpenVROverlayHandle;

	private Vector4 OpenVRUVOffsetAndScale = new Vector4(0f, 0f, 1f, 1f);

	private Vector2 OpenVRMouseScale = new Vector2(1f, 1f);

	private OVRManager.XRDevice constructedOverlayXRDevice;

	private bool xrDeviceConstructed;

	public bool previewInEditor
	{
		get
		{
			return _previewInEditor;
		}
		set
		{
			if (_previewInEditor != value)
			{
				_previewInEditor = value;
				SetupEditorPreview();
			}
		}
	}

	public int layerId { get; private set; }

	protected OVRPlugin.LayerLayout layout => OVRPlugin.LayerLayout.Mono;

	public int layerIndex { get; protected set; } = -1;


	public bool isOverlayVisible { get; private set; }

	protected int texturesPerStage
	{
		get
		{
			if (layout != 0)
			{
				return 1;
			}
			return 2;
		}
	}

	public static string OpenVROverlayKey => "unity:" + Application.companyName + "." + Application.productName;

	public void OverrideOverlayTextureInfo(Texture srcTexture, IntPtr nativePtr, XRNode node)
	{
		int num = ((node == XRNode.RightEye) ? 1 : 0);
		if (textures.Length > num)
		{
			textures[num] = srcTexture;
			texturePtrs[num] = nativePtr;
			isOverridePending = true;
		}
	}

	protected static bool NeedsTexturesForShape(OverlayShape shape)
	{
		return !IsPassthroughShape(shape);
	}

	protected bool CreateLayer(int mipLevels, int sampleCount, OVRPlugin.EyeTextureFormat etFormat, int flags, OVRPlugin.Sizei size, OVRPlugin.OverlayShape shape)
	{
		if (!layerIdHandle.IsAllocated || layerIdPtr == IntPtr.Zero)
		{
			layerIdHandle = GCHandle.Alloc(layerId, GCHandleType.Pinned);
			layerIdPtr = layerIdHandle.AddrOfPinnedObject();
		}
		if (layerIndex == -1)
		{
			layerIndex = instances.IndexOf(this);
			if (layerIndex == -1)
			{
				layerIndex = instances.IndexOf(null);
				if (layerIndex == -1)
				{
					layerIndex = instances.Count;
					instances.Add(this);
				}
				else
				{
					instances[layerIndex] = this;
				}
			}
		}
		if (!isOverridePending && layerDesc.MipLevels == mipLevels && layerDesc.SampleCount == sampleCount && layerDesc.Format == etFormat && layerDesc.Layout == layout && layerDesc.LayerFlags == flags && layerDesc.TextureSize.Equals(size) && layerDesc.Shape == shape && layerCompositionDepth == compositionDepth)
		{
			return false;
		}
		OVRPlugin.LayerDesc desc = OVRPlugin.CalculateLayerDesc(shape, layout, size, mipLevels, sampleCount, etFormat, flags);
		OVRPlugin.EnqueueSetupLayer(desc, compositionDepth, layerIdPtr);
		layerId = (int)layerIdHandle.Target;
		if (layerId > 0)
		{
			layerDesc = desc;
			layerCompositionDepth = compositionDepth;
			if (isExternalSurface)
			{
				stageCount = 1;
			}
			else
			{
				stageCount = OVRPlugin.GetLayerTextureStageCount(layerId);
			}
		}
		isOverridePending = false;
		return true;
	}

	protected bool CreateLayerTextures(bool useMipmaps, OVRPlugin.Sizei size, bool isHdr)
	{
		if (isExternalSurface)
		{
			if (externalSurfaceObject == IntPtr.Zero)
			{
				externalSurfaceObject = OVRPlugin.GetLayerAndroidSurfaceObject(layerId);
				if (externalSurfaceObject != IntPtr.Zero)
				{
					Debug.LogFormat("GetLayerAndroidSurfaceObject returns {0}", externalSurfaceObject);
					if (externalSurfaceObjectCreated != null)
					{
						externalSurfaceObjectCreated();
					}
				}
			}
			return false;
		}
		bool result = false;
		if (stageCount <= 0)
		{
			return false;
		}
		if (layerTextures == null)
		{
			layerTextures = new LayerTexture[texturesPerStage];
		}
		for (int i = 0; i < texturesPerStage; i++)
		{
			if (layerTextures[i].swapChain == null)
			{
				layerTextures[i].swapChain = new Texture[stageCount];
			}
			if (layerTextures[i].swapChainPtr == null)
			{
				layerTextures[i].swapChainPtr = new IntPtr[stageCount];
			}
			for (int j = 0; j < stageCount; j++)
			{
				Texture texture = layerTextures[i].swapChain[j];
				IntPtr intPtr = layerTextures[i].swapChainPtr[j];
				if (!(texture != null) || !(intPtr != IntPtr.Zero) || size.w != texture.width || size.h != texture.height)
				{
					if (intPtr == IntPtr.Zero)
					{
						intPtr = OVRPlugin.GetLayerTexture(layerId, j, (OVRPlugin.Eye)i);
					}
					if (!(intPtr == IntPtr.Zero))
					{
						TextureFormat format = (isHdr ? TextureFormat.RGBAHalf : TextureFormat.RGBA32);
						texture = ((currentOverlayShape == OverlayShape.Cubemap || currentOverlayShape == OverlayShape.OffcenterCubemap) ? ((Texture)Cubemap.CreateExternalTexture(size.w, format, useMipmaps, intPtr)) : ((Texture)Texture2D.CreateExternalTexture(size.w, size.h, format, useMipmaps, linear: false, intPtr)));
						layerTextures[i].swapChain[j] = texture;
						layerTextures[i].swapChainPtr[j] = intPtr;
						result = true;
					}
				}
			}
		}
		return result;
	}

	protected void DestroyLayerTextures()
	{
		if (isExternalSurface)
		{
			return;
		}
		int num = 0;
		while (layerTextures != null && num < texturesPerStage)
		{
			if (layerTextures[num].swapChain != null)
			{
				for (int i = 0; i < stageCount; i++)
				{
					UnityEngine.Object.Destroy(layerTextures[num].swapChain[i]);
				}
			}
			num++;
		}
		layerTextures = null;
	}

	protected void DestroyLayer()
	{
		if (layerIndex != -1)
		{
			OVRPlugin.EnqueueSubmitLayer(onTop: true, headLocked: false, noDepthBufferTesting: false, IntPtr.Zero, IntPtr.Zero, -1, 0, OVRPose.identity.ToPosef_Legacy(), Vector3.one.ToVector3f(), layerIndex, (OVRPlugin.OverlayShape)prevOverlayShape);
			instances[layerIndex] = null;
			layerIndex = -1;
		}
		if (layerIdPtr != IntPtr.Zero)
		{
			OVRPlugin.EnqueueDestroyLayer(layerIdPtr);
			layerIdPtr = IntPtr.Zero;
			layerIdHandle.Free();
			layerId = 0;
		}
		layerDesc = default(OVRPlugin.LayerDesc);
		frameIndex = 0;
		prevFrameIndex = -1;
	}

	public void SetSrcDestRects(Rect srcLeft, Rect srcRight, Rect destLeft, Rect destRight)
	{
		srcRectLeft = srcLeft;
		srcRectRight = srcRight;
		destRectLeft = destLeft;
		destRectRight = destRight;
	}

	public void UpdateTextureRectMatrix()
	{
		Rect leftRect = new Rect(srcRectLeft.x, (isExternalSurface ^ invertTextureRects) ? (1f - srcRectLeft.y - srcRectLeft.height) : srcRectLeft.y, srcRectLeft.width, srcRectLeft.height);
		Rect rightRect = new Rect(srcRectRight.x, (isExternalSurface ^ invertTextureRects) ? (1f - srcRectRight.y - srcRectRight.height) : srcRectRight.y, srcRectRight.width, srcRectRight.height);
		Rect rect = new Rect(destRectLeft.x, (isExternalSurface ^ invertTextureRects) ? (1f - destRectLeft.y - destRectLeft.height) : destRectLeft.y, destRectLeft.width, destRectLeft.height);
		Rect rect2 = new Rect(destRectRight.x, (isExternalSurface ^ invertTextureRects) ? (1f - destRectRight.y - destRectRight.height) : destRectRight.y, destRectRight.width, destRectRight.height);
		textureRectMatrix.leftRect = leftRect;
		textureRectMatrix.rightRect = rightRect;
		if (currentOverlayShape == OverlayShape.Fisheye)
		{
			rect.x -= 0.5f;
			rect.y -= 0.5f;
			rect2.x -= 0.5f;
			rect2.y -= 0.5f;
		}
		float num = srcRectLeft.width / destRectLeft.width;
		float num2 = srcRectLeft.height / destRectLeft.height;
		textureRectMatrix.leftScaleBias = new Vector4(num, num2, leftRect.x - rect.x * num, leftRect.y - rect.y * num2);
		float num3 = srcRectRight.width / destRectRight.width;
		float num4 = srcRectRight.height / destRectRight.height;
		textureRectMatrix.rightScaleBias = new Vector4(num3, num4, rightRect.x - rect2.x * num3, rightRect.y - rect2.y * num4);
	}

	public void SetPerLayerColorScaleAndOffset(Vector4 scale, Vector4 offset)
	{
		colorScale = scale;
		colorOffset = offset;
	}

	protected bool LatchLayerTextures()
	{
		if (isExternalSurface)
		{
			return true;
		}
		for (int i = 0; i < texturesPerStage; i++)
		{
			if ((textures[i] != layerTextures[i].appTexture || layerTextures[i].appTexturePtr == IntPtr.Zero) && textures[i] != null)
			{
				RenderTexture renderTexture = textures[i] as RenderTexture;
				if ((bool)renderTexture && !renderTexture.IsCreated())
				{
					renderTexture.Create();
				}
				layerTextures[i].appTexturePtr = ((texturePtrs[i] != IntPtr.Zero) ? texturePtrs[i] : textures[i].GetNativeTexturePtr());
				if (layerTextures[i].appTexturePtr != IntPtr.Zero)
				{
					layerTextures[i].appTexture = textures[i];
				}
			}
			if (currentOverlayShape == OverlayShape.Cubemap && textures[i] as Cubemap == null)
			{
				Debug.LogError("Need Cubemap texture for cube map overlay");
				return false;
			}
		}
		if (currentOverlayShape == OverlayShape.OffcenterCubemap)
		{
			Debug.LogWarning("Overlay shape " + currentOverlayShape.ToString() + " is not supported on current platform");
			return false;
		}
		if (layerTextures[0].appTexture == null || layerTextures[0].appTexturePtr == IntPtr.Zero)
		{
			return false;
		}
		return true;
	}

	protected OVRPlugin.LayerDesc GetCurrentLayerDesc()
	{
		OVRPlugin.Sizei sizei = default(OVRPlugin.Sizei);
		sizei.w = 0;
		sizei.h = 0;
		OVRPlugin.Sizei textureSize = sizei;
		if (isExternalSurface)
		{
			textureSize.w = externalSurfaceWidth;
			textureSize.h = externalSurfaceHeight;
		}
		else if (NeedsTexturesForShape(currentOverlayShape))
		{
			if (textures[0] == null)
			{
				Debug.LogWarning("textures[0] hasn't been set");
			}
			textureSize.w = (textures[0] ? textures[0].width : 0);
			textureSize.h = (textures[0] ? textures[0].height : 0);
		}
		OVRPlugin.LayerDesc layerDesc = default(OVRPlugin.LayerDesc);
		layerDesc.Format = layerTextureFormat;
		layerDesc.LayerFlags = ((!isExternalSurface) ? 8 : 0);
		layerDesc.Layout = layout;
		layerDesc.MipLevels = 1;
		layerDesc.SampleCount = 1;
		layerDesc.Shape = (OVRPlugin.OverlayShape)currentOverlayShape;
		layerDesc.TextureSize = textureSize;
		OVRPlugin.LayerDesc result = layerDesc;
		Texture2D texture2D = textures[0] as Texture2D;
		if (texture2D != null)
		{
			if (texture2D.format == TextureFormat.RGBAHalf || texture2D.format == TextureFormat.RGBAFloat)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
			result.MipLevels = texture2D.mipmapCount;
		}
		Cubemap cubemap = textures[0] as Cubemap;
		if (cubemap != null)
		{
			if (cubemap.format == TextureFormat.RGBAHalf || cubemap.format == TextureFormat.RGBAFloat)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
			result.MipLevels = cubemap.mipmapCount;
		}
		RenderTexture renderTexture = textures[0] as RenderTexture;
		if (renderTexture != null)
		{
			result.SampleCount = renderTexture.antiAliasing;
			if (renderTexture.format == RenderTextureFormat.ARGBHalf || renderTexture.format == RenderTextureFormat.ARGBFloat || renderTexture.format == RenderTextureFormat.RGB111110Float)
			{
				result.Format = OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
			}
		}
		if (isProtectedContent)
		{
			result.LayerFlags |= 64;
		}
		if (isExternalSurface)
		{
			result.LayerFlags |= 128;
		}
		if (useBicubicFiltering)
		{
			result.LayerFlags |= 16384;
		}
		return result;
	}

	protected Rect GetBlitRect(int eyeId, int width, int height, bool invertRect)
	{
		Rect rect;
		if (texturesPerStage == 2)
		{
			rect = ((eyeId == 0) ? srcRectLeft : srcRectRight);
		}
		else
		{
			float num = Mathf.Min(srcRectLeft.x, srcRectRight.x);
			float num2 = Mathf.Min(srcRectLeft.y, srcRectRight.y);
			float num3 = Mathf.Max(srcRectLeft.x + srcRectLeft.width, srcRectRight.x + srcRectRight.width);
			float num4 = Mathf.Max(srcRectLeft.y + srcRectLeft.height, srcRectRight.y + srcRectRight.height);
			rect = new Rect(num, num2, num3 - num, num4 - num2);
		}
		if (invertRect)
		{
			rect.y = 1f - rect.y - rect.height;
		}
		return new Rect(Mathf.Max(0f, Mathf.Floor((float)width * rect.x) - 2f), Mathf.Max(0f, Mathf.Floor((float)height * rect.y) - 2f), Mathf.Min(width, Mathf.Ceil((float)width * rect.xMax) - Mathf.Floor((float)width * rect.x) + 4f), Mathf.Min(height, Mathf.Ceil((float)height * rect.yMax) - Mathf.Floor((float)height * rect.y) + 4f));
	}

	protected void BlitSubImage(Texture src, int width, int height, Material mat, Rect rect)
	{
		_commandBuffer.SetRenderTarget(_tempRenderTextureId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		_commandBuffer.SetProjectionMatrix(Matrix4x4.Ortho(-1f, 1f, -1f, 1f, -1f, 1f));
		_commandBuffer.SetViewMatrix(Matrix4x4.identity);
		_commandBuffer.EnableScissorRect(new Rect(0f, 0f, rect.width, rect.height));
		_commandBuffer.SetViewport(new Rect(0f - rect.x, 0f - rect.y, width, height));
		mat.mainTexture = src;
		mat.SetPass(0);
		if (_blitMesh == null)
		{
			_blitMesh = new Mesh
			{
				name = "OVROverlay Blit Mesh"
			};
			_blitMesh.SetVertices(new Vector3[3]
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f)
			});
			_blitMesh.SetUVs(0, new Vector2[3]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 2f),
				new Vector2(2f, 0f)
			});
			_blitMesh.SetIndices(new ushort[3] { 0, 1, 2 }, MeshTopology.Triangles, 0);
			_blitMesh.UploadMeshData(markNoLongerReadable: true);
		}
		_commandBuffer.DrawMesh(_blitMesh, Matrix4x4.identity, mat);
	}

	protected bool PopulateLayer(int mipLevels, bool isHdr, OVRPlugin.Sizei size, int sampleCount, int stage)
	{
		if (isExternalSurface)
		{
			return true;
		}
		bool flag = false;
		RenderTextureFormat colorFormat = (isHdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
		if (_commandBuffer == null)
		{
			_commandBuffer = new CommandBuffer();
		}
		_commandBuffer.Clear();
		_commandBuffer.name = ToString();
		for (int i = 0; i < texturesPerStage; i++)
		{
			Texture texture = layerTextures[i].swapChain[stage];
			if (texture == null)
			{
				continue;
			}
			flag = true;
			bool flag2 = !isAlphaPremultiplied && !OVRPlugin.unpremultipliedAlphaLayersSupported;
			bool flag3 = isAlphaPremultiplied && !OVRPlugin.premultipliedAlphaLayersSupported;
			bool flag4 = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2;
			bool flag5 = texture.width == textures[i].width && texture.height == textures[i].height;
			bool flag6 = textures[i].mipmapCount == texture.mipmapCount;
			bool flag7 = currentOverlayShape == OverlayShape.Cubemap || currentOverlayShape == OverlayShape.OffcenterCubemap;
			if (Application.isMobilePlatform && !flag4 && flag5 && flag6 && !flag3)
			{
				_commandBuffer.CopyTexture(textures[i], texture);
				continue;
			}
			for (int j = 0; j < mipLevels; j++)
			{
				int num = size.w >> j;
				if (num < 1)
				{
					num = 1;
				}
				int num2 = size.h >> j;
				if (num2 < 1)
				{
					num2 = 1;
				}
				int width = num;
				int height = num2;
				if (overrideTextureRectMatrix && isDynamic)
				{
					Rect blitRect = GetBlitRect(i, num, num2, invertTextureRects);
					width = (int)blitRect.width;
					height = (int)blitRect.height;
				}
				RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, colorFormat, 0);
				desc.msaaSamples = sampleCount;
				desc.useMipMap = false;
				desc.autoGenerateMips = false;
				desc.sRGB = true;
				_commandBuffer.GetTemporaryRT(_tempRenderTextureId, desc, FilterMode.Point);
				int num3 = ((!flag7) ? 1 : 6);
				for (int k = 0; k < num3; k++)
				{
					Material material = (flag7 ? cubeMaterial[k] : tex2DMaterial);
					material.SetInt("_premultiply", flag2 ? 1 : 0);
					material.SetInt("_unmultiply", flag3 ? 1 : 0);
					if (!flag7)
					{
						material.SetInt("_flip", (OVRPlugin.nativeXrApi == OVRPlugin.XrApi.OpenXR) ? 1 : 0);
					}
					if (!flag7 && overrideTextureRectMatrix && isDynamic)
					{
						Rect blitRect2 = GetBlitRect(i, num, num2, invertTextureRects);
						BlitSubImage(textures[i], num, num2, material, blitRect2);
						_commandBuffer.CopyTexture(_tempRenderTextureId, 0, 0, 0, 0, (int)blitRect2.width, (int)blitRect2.height, texture, k, j, (int)blitRect2.x, (int)blitRect2.y);
					}
					else
					{
						_commandBuffer.Blit(textures[i], _tempRenderTextureId, material);
						_commandBuffer.CopyTexture(_tempRenderTextureId, 0, 0, texture, k, j);
					}
				}
				_commandBuffer.ReleaseTemporaryRT(_tempRenderTextureId);
			}
		}
		if (flag)
		{
			Graphics.ExecuteCommandBuffer(_commandBuffer);
		}
		return flag;
	}

	protected bool SubmitLayer(bool overlay, bool headLocked, bool noDepthBufferTesting, OVRPose pose, Vector3 scale, int frameIndex)
	{
		int num = ((texturesPerStage >= 2) ? 1 : 0);
		if (overrideTextureRectMatrix)
		{
			UpdateTextureRectMatrix();
		}
		bool efficientSharpen = useEfficientSharpen;
		bool efficientSuperSample = useEfficientSupersample;
		bool premultipledAlpha = isAlphaPremultiplied && OVRPlugin.premultipliedAlphaLayersSupported;
		if (useAutomaticFiltering && !useEfficientSharpen && !useEfficientSupersample && !useExpensiveSharpen && !useExpensiveSuperSample)
		{
			efficientSharpen = true;
			efficientSuperSample = true;
		}
		if (!useAutomaticFiltering && ((useEfficientSharpen && useEfficientSupersample) || (useExpensiveSharpen && useExpensiveSuperSample) || (useEfficientSharpen && useExpensiveSuperSample) || (useExpensiveSharpen && useEfficientSupersample)))
		{
			Debug.LogError("Warning-XR sharpening and supersampling cannot be enabled simultaneously, either enable autofiltering or disable one of the options");
			return false;
		}
		bool flag = isExternalSurface || !NeedsTexturesForShape(currentOverlayShape);
		bool result = OVRPlugin.EnqueueSubmitLayer(overlay, headLocked, noDepthBufferTesting, flag ? IntPtr.Zero : layerTextures[0].appTexturePtr, flag ? IntPtr.Zero : layerTextures[num].appTexturePtr, layerId, frameIndex, pose.flipZ().ToPosef_Legacy(), scale.ToVector3f(), layerIndex, (OVRPlugin.OverlayShape)currentOverlayShape, overrideTextureRectMatrix, textureRectMatrix, overridePerLayerColorScaleAndOffset, colorScale, colorOffset, useExpensiveSuperSample, useBicubicFiltering, efficientSuperSample, efficientSharpen, useExpensiveSharpen, hidden, isProtectedContent, useAutomaticFiltering, premultipledAlpha);
		prevOverlayShape = currentOverlayShape;
		return result;
	}

	protected void SetupEditorPreview()
	{
	}

	public void ResetEditorPreview()
	{
		previewInEditor = false;
		previewInEditor = true;
	}

	public static bool IsPassthroughShape(OverlayShape shape)
	{
		return OVRPlugin.IsPassthroughShape((OVRPlugin.OverlayShape)shape);
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			if (tex2DMaterial == null)
			{
				tex2DMaterial = new Material(Shader.Find("Oculus/Texture2D Blit"));
			}
			Shader shader = null;
			for (int i = 0; i < 6; i++)
			{
				if (cubeMaterial[i] == null)
				{
					if (shader == null)
					{
						shader = Shader.Find("Oculus/Cubemap Blit");
					}
					cubeMaterial[i] = new Material(shader);
				}
				cubeMaterial[i].SetInt("_face", i);
			}
		}
		rend = GetComponent<Renderer>();
		if (textures.Length == 0)
		{
			textures = new Texture[1];
		}
		if (rend != null && textures[0] == null)
		{
			textures[0] = rend.sharedMaterial.mainTexture;
		}
		SetupEditorPreview();
	}

	private void OnEnable()
	{
		if (OVRManager.OVRManagerinitialized)
		{
			InitOVROverlay();
		}
		if (base.enabled)
		{
			SetupEditorPreview();
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(HandlePreRender));
			RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
		}
	}

	private void InitOVROverlay()
	{
		if (!OVRPlugin.UnityOpenXR.Enabled && !OVRManager.isHmdPresent)
		{
			base.enabled = false;
			return;
		}
		constructedOverlayXRDevice = OVRManager.XRDevice.Unknown;
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
		{
			CVROverlay overlay = OpenVR.Overlay;
			if (overlay == null)
			{
				base.enabled = false;
				return;
			}
			if (overlay.CreateOverlay(OpenVROverlayKey + base.transform.name, base.gameObject.name, ref OpenVROverlayHandle) != 0)
			{
				base.enabled = false;
				return;
			}
		}
		constructedOverlayXRDevice = OVRManager.loadedXRDevice;
		xrDeviceConstructed = true;
	}

	private void OnDisable()
	{
		if (!(base.gameObject.scene.name == "DontDestroyOnLoad"))
		{
			DisableImmediately();
		}
	}

	private void DisableImmediately()
	{
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(HandlePreRender));
		RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
		if (OVRManager.OVRManagerinitialized && OVRManager.loadedXRDevice == constructedOverlayXRDevice)
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				DestroyLayerTextures();
				DestroyLayer();
			}
			else if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR && OpenVROverlayHandle != 0L)
			{
				OpenVR.Overlay?.DestroyOverlay(OpenVROverlayHandle);
				OpenVROverlayHandle = 0uL;
			}
			constructedOverlayXRDevice = OVRManager.XRDevice.Unknown;
			xrDeviceConstructed = false;
		}
	}

	private void OnDestroy()
	{
		DisableImmediately();
		DestroyLayerTextures();
		DestroyLayer();
		if (_commandBuffer != null)
		{
			_commandBuffer.Dispose();
		}
		if (_blitMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(_blitMesh);
		}
	}

	private void ComputePoseAndScale(out OVRPose pose, out Vector3 scale, out bool overlay, out bool headLocked)
	{
		Camera camera = OVRManager.FindMainCamera();
		overlay = currentOverlayType == OverlayType.Overlay;
		headLocked = false;
		Transform parent = base.transform;
		while (parent != null && !headLocked)
		{
			headLocked |= parent == camera.transform;
			parent = parent.parent;
		}
		pose = (headLocked ? base.transform.ToHeadSpacePose(camera) : base.transform.ToTrackingSpacePose(camera));
		scale = base.transform.lossyScale;
		for (int i = 0; i < 3; i++)
		{
			scale[i] /= camera.transform.lossyScale[i];
		}
		if (currentOverlayShape == OverlayShape.Cubemap)
		{
			if (!useLegacyCubemapRotation)
			{
				pose.orientation *= Quaternion.AngleAxis(180f, Vector3.up);
			}
			pose.position = camera.transform.position;
		}
	}

	private bool ComputeSubmit(out OVRPose pose, out Vector3 scale, out bool overlay, out bool headLocked)
	{
		ComputePoseAndScale(out pose, out scale, out overlay, out headLocked);
		if (currentOverlayShape == OverlayShape.OffcenterCubemap)
		{
			pose.position = base.transform.position;
			if (pose.position.magnitude > 1f)
			{
				Debug.LogWarning("Your cube map center offset's magnitude is greater than 1, which will cause some cube map pixel always invisible .");
				return false;
			}
		}
		if (OVRPlugin.nativeXrApi != OVRPlugin.XrApi.OpenXR && currentOverlayShape == OverlayShape.Cylinder)
		{
			float num = scale.x / scale.z / MathF.PI * 180f;
			if (num > 180f)
			{
				Debug.LogWarning("Cylinder overlay's arc angle has to be below 180 degree, current arc angle is " + num + " degree.");
				return false;
			}
		}
		if (OVRPlugin.nativeXrApi == OVRPlugin.XrApi.OpenXR && currentOverlayShape == OverlayShape.Fisheye)
		{
			Debug.LogWarning("Fisheye overlay shape is not support on OpenXR");
			return false;
		}
		return true;
	}

	private bool OpenVROverlayUpdate(Vector3 scale, OVRPose pose)
	{
		CVROverlay overlay = OpenVR.Overlay;
		if (overlay == null)
		{
			return false;
		}
		Texture texture = textures[0];
		if (texture == null)
		{
			return false;
		}
		EVROverlayError eVROverlayError = overlay.ShowOverlay(OpenVROverlayHandle);
		if ((eVROverlayError == EVROverlayError.InvalidHandle || eVROverlayError == EVROverlayError.UnknownOverlay) && overlay.FindOverlay(OpenVROverlayKey + base.transform.name, ref OpenVROverlayHandle) != 0)
		{
			return false;
		}
		Texture_t pTexture = default(Texture_t);
		pTexture.handle = texture.GetNativeTexturePtr();
		pTexture.eType = (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL") ? ETextureType.OpenGL : ETextureType.DirectX);
		pTexture.eColorSpace = EColorSpace.Auto;
		overlay.SetOverlayTexture(OpenVROverlayHandle, ref pTexture);
		VRTextureBounds_t pOverlayTextureBounds = default(VRTextureBounds_t);
		pOverlayTextureBounds.uMin = (0f + OpenVRUVOffsetAndScale.x) * OpenVRUVOffsetAndScale.z;
		pOverlayTextureBounds.vMin = (1f + OpenVRUVOffsetAndScale.y) * OpenVRUVOffsetAndScale.w;
		pOverlayTextureBounds.uMax = (1f + OpenVRUVOffsetAndScale.x) * OpenVRUVOffsetAndScale.z;
		pOverlayTextureBounds.vMax = (0f + OpenVRUVOffsetAndScale.y) * OpenVRUVOffsetAndScale.w;
		overlay.SetOverlayTextureBounds(OpenVROverlayHandle, ref pOverlayTextureBounds);
		HmdVector2_t pvecMouseScale = default(HmdVector2_t);
		pvecMouseScale.v0 = OpenVRMouseScale.x;
		pvecMouseScale.v1 = OpenVRMouseScale.y;
		overlay.SetOverlayMouseScale(OpenVROverlayHandle, ref pvecMouseScale);
		overlay.SetOverlayWidthInMeters(OpenVROverlayHandle, scale.x);
		HmdMatrix34_t pmatTrackingOriginToOverlayTransform = Matrix4x4.TRS(pose.position, pose.orientation, Vector3.one).ConvertToHMDMatrix34();
		overlay.SetOverlayTransformAbsolute(OpenVROverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref pmatTrackingOriginToOverlayTransform);
		return true;
	}

	private void HandlePreRender(Camera camera)
	{
		if (camera == OVRManager.FindMainCamera())
		{
			isOverlayVisible = TrySubmitLayer();
		}
	}

	private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (camera == OVRManager.FindMainCamera())
		{
			isOverlayVisible = TrySubmitLayer();
		}
	}

	private bool TrySubmitLayer()
	{
		if (!base.enabled)
		{
			DisableImmediately();
			return false;
		}
		if (!OVRManager.OVRManagerinitialized || !OVRPlugin.userPresent)
		{
			return false;
		}
		if (!xrDeviceConstructed)
		{
			InitOVROverlay();
		}
		if (OVRManager.loadedXRDevice != constructedOverlayXRDevice)
		{
			Debug.LogError("Warning-XR Device was switched during runtime with overlays still enabled. When doing so, all overlays constructed with the previous XR device must first be disabled.");
			return false;
		}
		bool flag = !isExternalSurface && NeedsTexturesForShape(currentOverlayShape);
		if (currentOverlayType == OverlayType.None || (flag && (textures.Length < texturesPerStage || textures[0] == null)))
		{
			return false;
		}
		if (!ComputeSubmit(out var pose, out var scale, out var overlay, out var headLocked))
		{
			return false;
		}
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
		{
			if (currentOverlayShape == OverlayShape.Quad)
			{
				return OpenVROverlayUpdate(scale, pose);
			}
			return false;
		}
		OVRPlugin.LayerDesc currentLayerDesc = GetCurrentLayerDesc();
		bool isHdr = currentLayerDesc.Format == OVRPlugin.EyeTextureFormat.R16G16B16A16_FP;
		bool num = !layerDesc.TextureSize.Equals(currentLayerDesc.TextureSize) && layerId > 0;
		bool flag2 = NeedsTexturesForShape(currentOverlayShape);
		bool flag3 = NeedsTexturesForShape(prevOverlayShape) != flag2;
		if (num || flag3)
		{
			DestroyLayerTextures();
			DestroyLayer();
		}
		bool flag4 = CreateLayer(currentLayerDesc.MipLevels, currentLayerDesc.SampleCount, currentLayerDesc.Format, currentLayerDesc.LayerFlags, currentLayerDesc.TextureSize, currentLayerDesc.Shape);
		if (layerIndex == -1 || layerId <= 0)
		{
			if (flag4)
			{
				prevOverlayShape = currentOverlayShape;
			}
			return false;
		}
		if (flag2)
		{
			bool useMipmaps = currentLayerDesc.MipLevels > 1;
			flag4 |= CreateLayerTextures(useMipmaps, currentLayerDesc.TextureSize, isHdr);
			if (!isExternalSurface && layerTextures[0].appTexture as RenderTexture != null)
			{
				isDynamic = true;
			}
			if (!LatchLayerTextures())
			{
				return false;
			}
			if (frameIndex > prevFrameIndex)
			{
				int stage = frameIndex % stageCount;
				if (!PopulateLayer(currentLayerDesc.MipLevels, isHdr, currentLayerDesc.TextureSize, currentLayerDesc.SampleCount, stage))
				{
					return false;
				}
			}
		}
		bool flag5 = SubmitLayer(overlay, headLocked, noDepthBufferTesting, pose, scale, frameIndex);
		prevFrameIndex = frameIndex;
		if (isDynamic)
		{
			frameIndex++;
		}
		if ((bool)rend)
		{
			rend.enabled = !flag5;
		}
		return flag5;
	}
}
