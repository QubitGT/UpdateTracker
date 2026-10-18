using System;
using UnityEngine.Categorization;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

public static class STP
{
	public struct PerViewConfig
	{
		public Matrix4x4 currentProj;

		public Matrix4x4 lastProj;

		public Matrix4x4 lastLastProj;

		public Matrix4x4 currentView;

		public Matrix4x4 lastView;

		public Matrix4x4 lastLastView;
	}

	public struct Config
	{
		public Texture2D noiseTexture;

		public TextureHandle inputColor;

		public TextureHandle inputDepth;

		public TextureHandle inputMotion;

		public TextureHandle inputStencil;

		public TextureHandle debugView;

		public TextureHandle destination;

		public HistoryContext historyContext;

		public bool enableHwDrs;

		public bool enableTexArray;

		public bool enableMotionScaling;

		public float nearPlane;

		public float farPlane;

		public int frameIndex;

		public bool hasValidHistory;

		public int stencilMask;

		public int debugViewIndex;

		public float deltaTime;

		public float lastDeltaTime;

		public Vector2Int currentImageSize;

		public Vector2Int priorImageSize;

		public Vector2Int outputImageSize;

		public int numActiveViews;

		public PerViewConfig[] perViewConfigs;
	}

	internal enum HistoryTextureType
	{
		DepthMotion,
		Luma,
		Convergence,
		Feedback,
		Count
	}

	public struct HistoryUpdateInfo
	{
		public Vector2Int preUpscaleSize;

		public Vector2Int postUpscaleSize;

		public bool useHwDrs;

		public bool useTexArray;
	}

	public sealed class HistoryContext : IDisposable
	{
		private RTHandle[] m_textures = new RTHandle[8];

		private Hash128 m_hash = Hash128.Compute(0);

		public bool Update(ref HistoryUpdateInfo info)
		{
			bool result = true;
			Hash128 hash = ComputeHistoryHash(ref info);
			if (hash != m_hash)
			{
				result = false;
				Dispose();
				m_hash = hash;
				Vector2Int historyTextureSize = (info.useHwDrs ? info.postUpscaleSize : info.preUpscaleSize);
				TextureDimension dimension = (info.useTexArray ? TextureDimension.Tex2DArray : TextureDimension.Tex2D);
				int slices = ((!info.useTexArray) ? 1 : TextureXR.slices);
				int num = 0;
				int num2 = 0;
				GraphicsFormat graphicsFormat = GraphicsFormat.None;
				bool useDynamicScaleExplicit = false;
				string text = "";
				for (int i = 0; i < 4; i++)
				{
					switch ((HistoryTextureType)i)
					{
					case HistoryTextureType.DepthMotion:
						num = historyTextureSize.x;
						num2 = historyTextureSize.y;
						graphicsFormat = GraphicsFormat.R32_UInt;
						useDynamicScaleExplicit = info.useHwDrs;
						text = "STP Depth & Motion";
						break;
					case HistoryTextureType.Luma:
						num = historyTextureSize.x;
						num2 = historyTextureSize.y;
						graphicsFormat = GraphicsFormat.R8G8_UNorm;
						useDynamicScaleExplicit = info.useHwDrs;
						text = "STP Luma";
						break;
					case HistoryTextureType.Convergence:
					{
						Vector2Int vector2Int = CalculateConvergenceTextureSize(historyTextureSize);
						num = vector2Int.x;
						num2 = vector2Int.y;
						graphicsFormat = GraphicsFormat.R8_UNorm;
						useDynamicScaleExplicit = info.useHwDrs;
						text = "STP Convergence";
						break;
					}
					case HistoryTextureType.Feedback:
						num = info.postUpscaleSize.x;
						num2 = info.postUpscaleSize.y;
						graphicsFormat = GraphicsFormat.A2B10G10R10_UNormPack32;
						useDynamicScaleExplicit = false;
						text = "STP Feedback";
						break;
					}
					for (int j = 0; j < 2; j++)
					{
						int num3 = j * 4 + i;
						RTHandle[] textures = m_textures;
						int width = num;
						int height = num2;
						GraphicsFormat format = graphicsFormat;
						string name = text;
						textures[num3] = RTHandles.Alloc(width, height, format, slices, FilterMode.Point, TextureWrapMode.Repeat, dimension, enableRandomWrite: true, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit, RenderTextureMemoryless.None, VRTextureUsage.None, name);
					}
				}
			}
			return result;
		}

		internal RTHandle GetCurrentHistoryTexture(HistoryTextureType historyType, int frameIndex)
		{
			return m_textures[(int)((frameIndex & 1) * 4 + historyType)];
		}

		internal RTHandle GetPreviousHistoryTexture(HistoryTextureType historyType, int frameIndex)
		{
			return m_textures[(int)(((frameIndex & 1) ^ 1) * 4 + historyType)];
		}

		public void Dispose()
		{
			for (int i = 0; i < m_textures.Length; i++)
			{
				if (m_textures[i] != null)
				{
					m_textures[i].Release();
					m_textures[i] = null;
				}
			}
			m_hash = Hash128.Compute(0);
		}
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\STP\\STP.cs")]
	private enum StpSetupPerViewConstants
	{
		Count = 8
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\STP\\STP.cs", needAccessors = false, generateCBuffer = true)]
	private struct StpConstantBufferData
	{
		public Vector4 _StpCommonConstant;

		public Vector4 _StpSetupConstants0;

		public Vector4 _StpSetupConstants1;

		public Vector4 _StpSetupConstants2;

		public Vector4 _StpSetupConstants3;

		public Vector4 _StpSetupConstants4;

		public Vector4 _StpSetupConstants5;

		[HLSLArray(16, typeof(Vector4))]
		public unsafe fixed float _StpSetupPerViewConstants[64];

		public Vector4 _StpDilConstants0;

		public Vector4 _StpTaaConstants0;

		public Vector4 _StpTaaConstants1;

		public Vector4 _StpTaaConstants2;

		public Vector4 _StpTaaConstants3;
	}

	private static class ShaderResources
	{
		public static readonly int _StpConstantBufferData = Shader.PropertyToID("StpConstantBufferData");

		public static readonly int _StpBlueNoiseIn = Shader.PropertyToID("_StpBlueNoiseIn");

		public static readonly int _StpDebugOut = Shader.PropertyToID("_StpDebugOut");

		public static readonly int _StpInputColor = Shader.PropertyToID("_StpInputColor");

		public static readonly int _StpInputDepth = Shader.PropertyToID("_StpInputDepth");

		public static readonly int _StpInputMotion = Shader.PropertyToID("_StpInputMotion");

		public static readonly int _StpInputStencil = Shader.PropertyToID("_StpInputStencil");

		public static readonly int _StpIntermediateColor = Shader.PropertyToID("_StpIntermediateColor");

		public static readonly int _StpIntermediateConvergence = Shader.PropertyToID("_StpIntermediateConvergence");

		public static readonly int _StpIntermediateWeights = Shader.PropertyToID("_StpIntermediateWeights");

		public static readonly int _StpPriorLuma = Shader.PropertyToID("_StpPriorLuma");

		public static readonly int _StpLuma = Shader.PropertyToID("_StpLuma");

		public static readonly int _StpPriorDepthMotion = Shader.PropertyToID("_StpPriorDepthMotion");

		public static readonly int _StpDepthMotion = Shader.PropertyToID("_StpDepthMotion");

		public static readonly int _StpPriorFeedback = Shader.PropertyToID("_StpPriorFeedback");

		public static readonly int _StpFeedback = Shader.PropertyToID("_StpFeedback");

		public static readonly int _StpPriorConvergence = Shader.PropertyToID("_StpPriorConvergence");

		public static readonly int _StpConvergence = Shader.PropertyToID("_StpConvergence");

		public static readonly int _StpOutput = Shader.PropertyToID("_StpOutput");
	}

	private static class ShaderKeywords
	{
		public static readonly string EnableDebugMode = "ENABLE_DEBUG_MODE";

		public static readonly string EnableLargeKernel = "ENABLE_LARGE_KERNEL";

		public static readonly string EnableStencilResponsive = "ENABLE_STENCIL_RESPONSIVE";

		public static readonly string DisableTexture2DXArray = "DISABLE_TEXTURE2D_X_ARRAY";
	}

	[Serializable]
	[SupportedOnRenderPipeline(new Type[] { })]
	[CategoryInfo(Name = "R: STP", Order = 1000)]
	[ElementInfo(Order = 0)]
	[HideInInspector]
	internal class RuntimeResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
	{
		[SerializeField]
		[ResourcePath("Runtime/STP/StpSetup.compute", SearchType.ProjectPath)]
		private ComputeShader m_setupCS;

		[SerializeField]
		[ResourcePath("Runtime/STP/StpPreTaa.compute", SearchType.ProjectPath)]
		private ComputeShader m_preTaaCS;

		[SerializeField]
		[ResourcePath("Runtime/STP/StpTaa.compute", SearchType.ProjectPath)]
		private ComputeShader m_taaCS;

		public int version => 0;

		public ComputeShader setupCS
		{
			get
			{
				return m_setupCS;
			}
			set
			{
				this.SetValueAndNotify(ref m_setupCS, value, "setupCS");
			}
		}

		public ComputeShader preTaaCS
		{
			get
			{
				return m_preTaaCS;
			}
			set
			{
				this.SetValueAndNotify(ref m_preTaaCS, value, "preTaaCS");
			}
		}

		public ComputeShader taaCS
		{
			get
			{
				return m_taaCS;
			}
			set
			{
				this.SetValueAndNotify(ref m_taaCS, value, "taaCS");
			}
		}
	}

	private enum ProfileId
	{
		StpSetup,
		StpPreTaa,
		StpTaa
	}

	private class SetupData
	{
		public ComputeShader cs;

		public int kernelIndex;

		public int viewCount;

		public Vector2Int dispatchSize;

		public StpConstantBufferData constantBufferData;

		public TextureHandle noiseTexture;

		public TextureHandle debugView;

		public TextureHandle inputColor;

		public TextureHandle inputDepth;

		public TextureHandle inputMotion;

		public TextureHandle inputStencil;

		public TextureHandle intermediateColor;

		public TextureHandle intermediateConvergence;

		public TextureHandle priorDepthMotion;

		public TextureHandle depthMotion;

		public TextureHandle priorLuma;

		public TextureHandle luma;

		public TextureHandle priorFeedback;

		public TextureHandle priorConvergence;
	}

	private class PreTaaData
	{
		public ComputeShader cs;

		public int kernelIndex;

		public int viewCount;

		public Vector2Int dispatchSize;

		public TextureHandle noiseTexture;

		public TextureHandle debugView;

		public TextureHandle intermediateConvergence;

		public TextureHandle intermediateWeights;

		public TextureHandle luma;

		public TextureHandle convergence;
	}

	private class TaaData
	{
		public ComputeShader cs;

		public int kernelIndex;

		public int viewCount;

		public Vector2Int dispatchSize;

		public TextureHandle noiseTexture;

		public TextureHandle debugView;

		public TextureHandle intermediateColor;

		public TextureHandle intermediateWeights;

		public TextureHandle priorFeedback;

		public TextureHandle depthMotion;

		public TextureHandle convergence;

		public TextureHandle feedback;

		public TextureHandle output;
	}

	private const int kNumDebugViews = 6;

	private static readonly GUIContent[] s_DebugViewDescriptions = new GUIContent[6]
	{
		new GUIContent("Clipped Input Color", "Shows input color clipped to {0 to 1}"),
		new GUIContent("Log Input Depth", "Shows input depth in log scale"),
		new GUIContent("Reversible Tonemapped Input Color", "Shows input color after conversion to reversible tonemaped space"),
		new GUIContent("Shaped Absolute Input Motion", "Visualizes input motion vectors"),
		new GUIContent("Motion Reprojection {R=Prior G=This Sqrt Luma Feedback Diff, B=Offscreen}", "Visualizes reprojected frame difference"),
		new GUIContent("Sensitivity {G=No motion match, R=Responsive, B=Luma}", "Visualize pixel sensitivities")
	};

	private static readonly int[] s_DebugViewIndices = new int[6] { 0, 1, 2, 3, 4, 5 };

	private const int kMaxPerViewConfigs = 2;

	private static PerViewConfig[] s_PerViewConfigs = new PerViewConfig[2];

	private const int kNumHistoryTextureTypes = 4;

	private const int kTotalSetupViewConstantsCount = 16;

	private static readonly int kQualcommVendorId = 20803;

	public static GUIContent[] debugViewDescriptions => s_DebugViewDescriptions;

	public static int[] debugViewIndices => s_DebugViewIndices;

	public static PerViewConfig[] perViewConfigs
	{
		get
		{
			return s_PerViewConfigs;
		}
		set
		{
			s_PerViewConfigs = value;
		}
	}

	public static bool IsSupported()
	{
		return true & SystemInfo.supportsComputeShaders & (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3);
	}

	public static Vector2 Jit16(int frameIndex)
	{
		Vector2 result = default(Vector2);
		result.x = HaltonSequence.Get(frameIndex, 2) - 0.5f;
		result.y = HaltonSequence.Get(frameIndex, 3) - 0.5f;
		return result;
	}

	private static Hash128 ComputeHistoryHash(ref HistoryUpdateInfo info)
	{
		Hash128 result = default(Hash128);
		result.Append(ref info.useHwDrs);
		result.Append(ref info.useTexArray);
		result.Append(ref info.postUpscaleSize);
		if (!info.useHwDrs)
		{
			result.Append(ref info.preUpscaleSize);
		}
		return result;
	}

	private static Vector2Int CalculateConvergenceTextureSize(Vector2Int historyTextureSize)
	{
		return new Vector2Int(CoreUtils.DivRoundUp(historyTextureSize.x, 4), CoreUtils.DivRoundUp(historyTextureSize.y, 4));
	}

	private static float CalculateMotionScale(float deltaTime, float lastDeltaTime)
	{
		float result = 1f;
		if (!Mathf.Approximately(lastDeltaTime, 0f))
		{
			result = deltaTime / lastDeltaTime;
		}
		return result;
	}

	private static Matrix4x4 ExtractRotation(Matrix4x4 input)
	{
		Matrix4x4 result = input;
		result[0, 3] = 0f;
		result[1, 3] = 0f;
		result[2, 3] = 0f;
		result[3, 3] = 1f;
		return result;
	}

	private static int PackVector2ToInt(Vector2 value)
	{
		ushort num = Mathf.FloatToHalf(value.x);
		uint num2 = Mathf.FloatToHalf(value.y);
		return (int)(num | (num2 << 16));
	}

	private unsafe static void PopulateConstantData(ref Config config, ref StpConstantBufferData constants)
	{
		int num = (config.noiseTexture.width - 1) & 0xFF;
		int num2 = (config.hasValidHistory ? 1 : 0) << 8;
		int num3 = (config.stencilMask & 0xFF) << 16;
		int num4 = (config.debugViewIndex & 0xFF) << 24;
		int value = num3 | num2 | num | num4;
		float y = (config.farPlane - config.nearPlane) / (config.nearPlane * config.farPlane);
		float z = 1f / config.farPlane;
		constants._StpCommonConstant = new Vector4(BitConverter.Int32BitsToSingle(value), y, z, 0f);
		constants._StpSetupConstants0.x = 1f / (float)config.currentImageSize.x;
		constants._StpSetupConstants0.y = 1f / (float)config.currentImageSize.y;
		constants._StpSetupConstants0.z = 0.5f / (float)config.currentImageSize.x;
		constants._StpSetupConstants0.w = 0.5f / (float)config.currentImageSize.y;
		Vector2 vector = Jit16(config.frameIndex - 1);
		Vector2 vector2 = Jit16(config.frameIndex);
		constants._StpSetupConstants1.x = vector2.x / (float)config.currentImageSize.x - vector.x / (float)config.priorImageSize.x;
		constants._StpSetupConstants1.y = vector2.y / (float)config.currentImageSize.y - vector.y / (float)config.priorImageSize.y;
		constants._StpSetupConstants1.z = vector2.x / (float)config.currentImageSize.x;
		constants._StpSetupConstants1.w = vector2.y / (float)config.currentImageSize.y;
		constants._StpSetupConstants2.x = config.outputImageSize.x;
		constants._StpSetupConstants2.y = config.outputImageSize.y;
		float num5 = 1f / config.nearPlane;
		float w = 1f / Mathf.Log(num5 * config.farPlane, 2f);
		constants._StpSetupConstants2.z = num5;
		constants._StpSetupConstants2.w = w;
		Vector2 vector3 = default(Vector2);
		vector3.x = 2f;
		vector3.y = 2f;
		vector3.x *= (float)config.priorImageSize.x / ((float)config.priorImageSize.x + 4f);
		vector3.y *= (float)config.priorImageSize.y / ((float)config.priorImageSize.y + 4f);
		constants._StpSetupConstants3.x = vector3[0];
		constants._StpSetupConstants3.y = vector3[1];
		constants._StpSetupConstants3.z = -0.5f * vector3[0];
		constants._StpSetupConstants3.w = -0.5f * vector3[1];
		constants._StpSetupConstants4.x = Mathf.Log(config.farPlane / config.nearPlane, 2f);
		constants._StpSetupConstants4.y = config.nearPlane;
		constants._StpSetupConstants4.z = (config.enableMotionScaling ? CalculateMotionScale(config.deltaTime, config.lastDeltaTime) : 1f);
		constants._StpSetupConstants4.w = 0f;
		constants._StpSetupConstants5.x = config.currentImageSize.x;
		constants._StpSetupConstants5.y = config.currentImageSize.y;
		constants._StpSetupConstants5.z = (float)config.outputImageSize.x / (Mathf.Ceil((float)config.outputImageSize.x / 4f) * 4f);
		constants._StpSetupConstants5.w = (float)config.outputImageSize.y / (Mathf.Ceil((float)config.outputImageSize.y / 4f) * 4f);
		Vector4 vector4 = default(Vector4);
		Vector4 vector5 = default(Vector4);
		Vector4 vector6 = default(Vector4);
		Vector4 vector7 = default(Vector4);
		Vector4 vector8 = default(Vector4);
		Vector4 vector9 = default(Vector4);
		for (uint num6 = 0u; num6 < config.numActiveViews; num6++)
		{
			uint num7 = num6 * 8 * 4;
			PerViewConfig perViewConfig = config.perViewConfigs[num6];
			vector4.x = perViewConfig.lastProj[0, 0];
			vector4.y = Mathf.Abs(perViewConfig.lastProj[1, 1]);
			vector4.z = 0f - perViewConfig.lastProj[0, 2];
			vector4.w = 0f - perViewConfig.lastProj[1, 2];
			vector5.x = perViewConfig.lastProj[2, 2];
			vector5.y = perViewConfig.lastProj[2, 3];
			vector5.z = perViewConfig.lastProj[3, 2];
			vector5.w = perViewConfig.lastProj[3, 3];
			vector6.x = perViewConfig.currentProj[0, 0];
			vector6.y = Mathf.Abs(perViewConfig.currentProj[1, 1]);
			vector6.z = perViewConfig.currentProj[0, 2];
			vector6.w = perViewConfig.currentProj[1, 2];
			vector7.x = perViewConfig.currentProj[2, 2];
			vector7.y = perViewConfig.currentProj[2, 3];
			vector7.z = perViewConfig.currentProj[3, 2];
			vector7.w = perViewConfig.currentProj[3, 3];
			Matrix4x4 matrix4x = ExtractRotation(perViewConfig.currentView) * Matrix4x4.Translate(-perViewConfig.currentView.GetColumn(3)) * Matrix4x4.Translate(perViewConfig.lastView.GetColumn(3)) * ExtractRotation(perViewConfig.lastView).transpose;
			Vector4 row = matrix4x.GetRow(0);
			Vector4 row2 = matrix4x.GetRow(1);
			Vector4 row3 = matrix4x.GetRow(2);
			vector8.x = perViewConfig.lastLastProj[0, 0];
			vector8.y = Mathf.Abs(perViewConfig.lastLastProj[1, 1]);
			vector8.z = perViewConfig.lastLastProj[0, 2];
			vector8.w = perViewConfig.lastLastProj[1, 2];
			vector9.x = perViewConfig.lastLastProj[2, 2];
			vector9.y = perViewConfig.lastLastProj[2, 3];
			vector9.z = perViewConfig.lastLastProj[3, 2];
			vector9.w = perViewConfig.lastLastProj[3, 3];
			Matrix4x4 matrix4x2 = ExtractRotation(perViewConfig.lastLastView) * Matrix4x4.Translate(-perViewConfig.lastLastView.GetColumn(3)) * Matrix4x4.Translate(perViewConfig.lastView.GetColumn(3)) * ExtractRotation(perViewConfig.lastView).transpose;
			Vector4 row4 = matrix4x2.GetRow(0);
			Vector4 row5 = matrix4x2.GetRow(1);
			Vector4 row6 = matrix4x2.GetRow(2);
			constants._StpSetupPerViewConstants[num7] = vector5.z / vector4.x;
			constants._StpSetupPerViewConstants[num7 + 1] = vector5.w / vector4.x;
			constants._StpSetupPerViewConstants[num7 + 2] = vector4.z / vector4.x;
			constants._StpSetupPerViewConstants[num7 + 3] = vector5.z / vector4.y;
			constants._StpSetupPerViewConstants[num7 + 4] = vector5.w / vector4.y;
			constants._StpSetupPerViewConstants[num7 + 5] = vector4.w / vector4.y;
			constants._StpSetupPerViewConstants[num7 + 6] = row.x * vector6.x + row3.x * vector6.z;
			constants._StpSetupPerViewConstants[num7 + 7] = row.y * vector6.x + row3.y * vector6.z;
			constants._StpSetupPerViewConstants[num7 + 8] = row.z * vector6.x + row3.z * vector6.z;
			constants._StpSetupPerViewConstants[num7 + 9] = row.w * vector6.x + row3.w * vector6.z;
			constants._StpSetupPerViewConstants[num7 + 10] = row2.x * vector6.y + row3.x * vector6.w;
			constants._StpSetupPerViewConstants[num7 + 11] = row2.y * vector6.y + row3.y * vector6.w;
			constants._StpSetupPerViewConstants[num7 + 12] = row2.z * vector6.y + row3.z * vector6.w;
			constants._StpSetupPerViewConstants[num7 + 13] = row2.w * vector6.y + row3.w * vector6.w;
			constants._StpSetupPerViewConstants[num7 + 14] = row3.x * vector7.z;
			constants._StpSetupPerViewConstants[num7 + 15] = row3.y * vector7.z;
			constants._StpSetupPerViewConstants[num7 + 16] = row3.z * vector7.z;
			constants._StpSetupPerViewConstants[num7 + 17] = row3.w * vector7.z + vector7.w;
			constants._StpSetupPerViewConstants[num7 + 18] = row4.x * vector8.x + row6.x * vector8.z;
			constants._StpSetupPerViewConstants[num7 + 19] = row4.y * vector8.x + row6.y * vector8.z;
			constants._StpSetupPerViewConstants[num7 + 20] = row4.z * vector8.x + row6.z * vector8.z;
			constants._StpSetupPerViewConstants[num7 + 21] = row4.w * vector8.x + row6.w * vector8.z;
			constants._StpSetupPerViewConstants[num7 + 22] = row5.x * vector8.y + row6.x * vector8.w;
			constants._StpSetupPerViewConstants[num7 + 23] = row5.y * vector8.y + row6.y * vector8.w;
			constants._StpSetupPerViewConstants[num7 + 24] = row5.z * vector8.y + row6.z * vector8.w;
			constants._StpSetupPerViewConstants[num7 + 25] = row5.w * vector8.y + row6.w * vector8.w;
			constants._StpSetupPerViewConstants[num7 + 26] = row6.x * vector9.z;
			constants._StpSetupPerViewConstants[num7 + 27] = row6.y * vector9.z;
			constants._StpSetupPerViewConstants[num7 + 28] = row6.z * vector9.z;
			constants._StpSetupPerViewConstants[num7 + 29] = row6.w * vector9.z + vector9.w;
			constants._StpSetupPerViewConstants[num7 + 30] = 0f;
			constants._StpSetupPerViewConstants[num7 + 31] = 0f;
		}
		constants._StpDilConstants0.x = 4f / (float)config.currentImageSize.x;
		constants._StpDilConstants0.y = 4f / (float)config.currentImageSize.y;
		constants._StpDilConstants0.z = BitConverter.Int32BitsToSingle(config.currentImageSize.x >> 2);
		constants._StpDilConstants0.w = BitConverter.Int32BitsToSingle(config.currentImageSize.y >> 2);
		constants._StpTaaConstants0.x = (float)config.currentImageSize.x / (float)config.outputImageSize.x;
		constants._StpTaaConstants0.y = (float)config.currentImageSize.y / (float)config.outputImageSize.y;
		constants._StpTaaConstants0.z = 0.5f * (float)config.currentImageSize.x / (float)config.outputImageSize.x - vector2.x;
		constants._StpTaaConstants0.w = 0.5f * (float)config.currentImageSize.y / (float)config.outputImageSize.y - vector2.y;
		constants._StpTaaConstants1.x = 1f / (float)config.currentImageSize.x;
		constants._StpTaaConstants1.y = 1f / (float)config.currentImageSize.y;
		constants._StpTaaConstants1.z = 1f / (float)config.outputImageSize.x;
		constants._StpTaaConstants1.w = 1f / (float)config.outputImageSize.y;
		constants._StpTaaConstants2.x = 0.5f / (float)config.outputImageSize.x;
		constants._StpTaaConstants2.y = 0.5f / (float)config.outputImageSize.y;
		constants._StpTaaConstants2.z = vector2.x / (float)config.currentImageSize.x - 0.5f / (float)config.currentImageSize.x;
		constants._StpTaaConstants2.w = vector2.y / (float)config.currentImageSize.y + 0.5f / (float)config.currentImageSize.y;
		constants._StpTaaConstants3.x = 0.5f / (float)config.currentImageSize.x;
		constants._StpTaaConstants3.y = 0.5f / (float)config.currentImageSize.y;
		constants._StpTaaConstants3.z = config.outputImageSize.x;
		constants._StpTaaConstants3.w = config.outputImageSize.y;
	}

	private static TextureHandle UseTexture(IBaseRenderGraphBuilder builder, TextureHandle texture, AccessFlags flags = AccessFlags.Read)
	{
		builder.UseTexture(in texture, flags);
		return texture;
	}

	public static TextureHandle Execute(RenderGraph renderGraph, ref Config config)
	{
		RuntimeResources renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<RuntimeResources>();
		Texture2D noiseTexture = config.noiseTexture;
		RTHandleStaticHelpers.SetRTHandleStaticWrapper(noiseTexture);
		RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
		RenderTargetInfo info = default(RenderTargetInfo);
		info.width = noiseTexture.width;
		info.height = noiseTexture.height;
		info.volumeDepth = 1;
		info.msaaSamples = 1;
		info.format = noiseTexture.graphicsFormat;
		info.bindMS = false;
		TextureHandle texture = renderGraph.ImportTexture(s_RTHandleWrapper, info);
		RTHandle previousHistoryTexture = config.historyContext.GetPreviousHistoryTexture(HistoryTextureType.DepthMotion, config.frameIndex);
		RTHandle previousHistoryTexture2 = config.historyContext.GetPreviousHistoryTexture(HistoryTextureType.Luma, config.frameIndex);
		RTHandle previousHistoryTexture3 = config.historyContext.GetPreviousHistoryTexture(HistoryTextureType.Convergence, config.frameIndex);
		RTHandle previousHistoryTexture4 = config.historyContext.GetPreviousHistoryTexture(HistoryTextureType.Feedback, config.frameIndex);
		RTHandle currentHistoryTexture = config.historyContext.GetCurrentHistoryTexture(HistoryTextureType.DepthMotion, config.frameIndex);
		RTHandle currentHistoryTexture2 = config.historyContext.GetCurrentHistoryTexture(HistoryTextureType.Luma, config.frameIndex);
		RTHandle currentHistoryTexture3 = config.historyContext.GetCurrentHistoryTexture(HistoryTextureType.Convergence, config.frameIndex);
		RTHandle currentHistoryTexture4 = config.historyContext.GetCurrentHistoryTexture(HistoryTextureType.Feedback, config.frameIndex);
		if (config.enableHwDrs)
		{
			currentHistoryTexture.rt.ApplyDynamicScale();
			currentHistoryTexture2.rt.ApplyDynamicScale();
			currentHistoryTexture3.rt.ApplyDynamicScale();
		}
		Vector2Int historyTextureSize = (config.enableHwDrs ? config.outputImageSize : config.currentImageSize);
		bool flag = SystemInfo.graphicsDeviceVendorID == kQualcommVendorId;
		Vector2Int vector2Int = new Vector2Int(8, flag ? 16 : 8);
		SetupData passData;
		SetupData setupData3;
		using (IComputeRenderGraphBuilder computeRenderGraphBuilder = renderGraph.AddComputePass<SetupData>("STP Setup", out passData, ProfilingSampler.Get(ProfileId.StpSetup), ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\STP\\STP.cs", 1109))
		{
			passData.cs = renderPipelineSettings.setupCS;
			passData.cs.shaderKeywords = null;
			if (flag)
			{
				passData.cs.EnableKeyword(ShaderKeywords.EnableLargeKernel);
			}
			if (!config.enableTexArray)
			{
				passData.cs.EnableKeyword(ShaderKeywords.DisableTexture2DXArray);
			}
			PopulateConstantData(ref config, ref passData.constantBufferData);
			passData.noiseTexture = UseTexture(computeRenderGraphBuilder, texture);
			if (config.debugView.IsValid())
			{
				passData.cs.EnableKeyword(ShaderKeywords.EnableDebugMode);
				passData.debugView = UseTexture(computeRenderGraphBuilder, config.debugView, AccessFlags.WriteAll);
			}
			passData.kernelIndex = passData.cs.FindKernel("StpSetup");
			passData.viewCount = config.numActiveViews;
			passData.dispatchSize = new Vector2Int(CoreUtils.DivRoundUp(config.currentImageSize.x, vector2Int.x), CoreUtils.DivRoundUp(config.currentImageSize.y, vector2Int.y));
			passData.inputColor = UseTexture(computeRenderGraphBuilder, config.inputColor);
			passData.inputDepth = UseTexture(computeRenderGraphBuilder, config.inputDepth);
			passData.inputMotion = UseTexture(computeRenderGraphBuilder, config.inputMotion);
			if (config.inputStencil.IsValid())
			{
				passData.cs.EnableKeyword(ShaderKeywords.EnableStencilResponsive);
				passData.inputStencil = UseTexture(computeRenderGraphBuilder, config.inputStencil);
			}
			SetupData setupData = passData;
			TextureDesc desc = new TextureDesc(historyTextureSize.x, historyTextureSize.y, config.enableHwDrs, config.enableTexArray)
			{
				name = "STP Intermediate Color",
				format = GraphicsFormat.A2B10G10R10_UNormPack32,
				enableRandomWrite = true
			};
			setupData.intermediateColor = UseTexture(computeRenderGraphBuilder, renderGraph.CreateTexture(in desc), AccessFlags.WriteAll);
			Vector2Int vector2Int2 = CalculateConvergenceTextureSize(historyTextureSize);
			SetupData setupData2 = passData;
			desc = new TextureDesc(vector2Int2.x, vector2Int2.y, config.enableHwDrs, config.enableTexArray)
			{
				name = "STP Intermediate Convergence",
				format = GraphicsFormat.R8_UNorm,
				enableRandomWrite = true
			};
			setupData2.intermediateConvergence = UseTexture(computeRenderGraphBuilder, renderGraph.CreateTexture(in desc), AccessFlags.WriteAll);
			passData.priorDepthMotion = UseTexture(computeRenderGraphBuilder, renderGraph.ImportTexture(previousHistoryTexture));
			passData.depthMotion = UseTexture(computeRenderGraphBuilder, renderGraph.ImportTexture(currentHistoryTexture), AccessFlags.WriteAll);
			passData.priorLuma = UseTexture(computeRenderGraphBuilder, renderGraph.ImportTexture(previousHistoryTexture2));
			passData.luma = UseTexture(computeRenderGraphBuilder, renderGraph.ImportTexture(currentHistoryTexture2), AccessFlags.WriteAll);
			passData.priorFeedback = UseTexture(computeRenderGraphBuilder, renderGraph.ImportTexture(previousHistoryTexture4));
			passData.priorConvergence = UseTexture(computeRenderGraphBuilder, renderGraph.ImportTexture(previousHistoryTexture3));
			computeRenderGraphBuilder.SetRenderFunc(delegate(SetupData data, ComputeGraphContext ctx)
			{
				ConstantBuffer.UpdateData(ctx.cmd.m_WrappedCommandBuffer, in data.constantBufferData);
				ConstantBuffer.Set<StpConstantBufferData>(data.cs, ShaderResources._StpConstantBufferData);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpBlueNoiseIn, data.noiseTexture);
				if (data.debugView.IsValid())
				{
					ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpDebugOut, data.debugView);
				}
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpInputColor, data.inputColor);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpInputDepth, data.inputDepth);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpInputMotion, data.inputMotion);
				if (data.inputStencil.IsValid())
				{
					ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpInputStencil, data.inputStencil, 0, RenderTextureSubElement.Stencil);
				}
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpIntermediateColor, data.intermediateColor);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpIntermediateConvergence, data.intermediateConvergence);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpPriorDepthMotion, data.priorDepthMotion);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpDepthMotion, data.depthMotion);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpPriorLuma, data.priorLuma);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpLuma, data.luma);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpPriorFeedback, data.priorFeedback);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpPriorConvergence, data.priorConvergence);
				ctx.cmd.DispatchCompute(data.cs, data.kernelIndex, data.dispatchSize.x, data.dispatchSize.y, data.viewCount);
			});
			setupData3 = passData;
		}
		PreTaaData passData2;
		PreTaaData preTaaData2;
		using (IComputeRenderGraphBuilder computeRenderGraphBuilder2 = renderGraph.AddComputePass<PreTaaData>("STP Pre-TAA", out passData2, ProfilingSampler.Get(ProfileId.StpPreTaa), ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\STP\\STP.cs", 1212))
		{
			passData2.cs = renderPipelineSettings.preTaaCS;
			passData2.cs.shaderKeywords = null;
			if (flag)
			{
				passData2.cs.EnableKeyword(ShaderKeywords.EnableLargeKernel);
			}
			if (!config.enableTexArray)
			{
				passData2.cs.EnableKeyword(ShaderKeywords.DisableTexture2DXArray);
			}
			passData2.noiseTexture = UseTexture(computeRenderGraphBuilder2, texture);
			if (config.debugView.IsValid())
			{
				passData2.cs.EnableKeyword(ShaderKeywords.EnableDebugMode);
				passData2.debugView = UseTexture(computeRenderGraphBuilder2, config.debugView, AccessFlags.ReadWrite);
			}
			passData2.kernelIndex = passData2.cs.FindKernel("StpPreTaa");
			passData2.viewCount = config.numActiveViews;
			passData2.dispatchSize = new Vector2Int(CoreUtils.DivRoundUp(config.currentImageSize.x, vector2Int.x), CoreUtils.DivRoundUp(config.currentImageSize.y, vector2Int.y));
			passData2.intermediateConvergence = UseTexture(computeRenderGraphBuilder2, setupData3.intermediateConvergence);
			PreTaaData preTaaData = passData2;
			TextureDesc desc = new TextureDesc(historyTextureSize.x, historyTextureSize.y, config.enableHwDrs, config.enableTexArray)
			{
				name = "STP Intermediate Weights",
				format = GraphicsFormat.R8_UNorm,
				enableRandomWrite = true
			};
			preTaaData.intermediateWeights = UseTexture(computeRenderGraphBuilder2, renderGraph.CreateTexture(in desc), AccessFlags.WriteAll);
			passData2.luma = UseTexture(computeRenderGraphBuilder2, renderGraph.ImportTexture(currentHistoryTexture2));
			passData2.convergence = UseTexture(computeRenderGraphBuilder2, renderGraph.ImportTexture(currentHistoryTexture3), AccessFlags.WriteAll);
			computeRenderGraphBuilder2.SetRenderFunc(delegate(PreTaaData data, ComputeGraphContext ctx)
			{
				ConstantBuffer.Set<StpConstantBufferData>(data.cs, ShaderResources._StpConstantBufferData);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpBlueNoiseIn, data.noiseTexture);
				if (data.debugView.IsValid())
				{
					ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpDebugOut, data.debugView);
				}
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpIntermediateConvergence, data.intermediateConvergence);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpIntermediateWeights, data.intermediateWeights);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpLuma, data.luma);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpConvergence, data.convergence);
				ctx.cmd.DispatchCompute(data.cs, data.kernelIndex, data.dispatchSize.x, data.dispatchSize.y, data.viewCount);
			});
			preTaaData2 = passData2;
		}
		TaaData passData3;
		TaaData taaData;
		using (IComputeRenderGraphBuilder computeRenderGraphBuilder3 = renderGraph.AddComputePass<TaaData>("STP TAA", out passData3, ProfilingSampler.Get(ProfileId.StpTaa), ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\STP\\STP.cs", 1275))
		{
			passData3.cs = renderPipelineSettings.taaCS;
			passData3.cs.shaderKeywords = null;
			if (flag)
			{
				passData3.cs.EnableKeyword(ShaderKeywords.EnableLargeKernel);
			}
			if (!config.enableTexArray)
			{
				passData3.cs.EnableKeyword(ShaderKeywords.DisableTexture2DXArray);
			}
			passData3.noiseTexture = UseTexture(computeRenderGraphBuilder3, texture);
			if (config.debugView.IsValid())
			{
				passData3.cs.EnableKeyword(ShaderKeywords.EnableDebugMode);
				passData3.debugView = UseTexture(computeRenderGraphBuilder3, config.debugView, AccessFlags.ReadWrite);
			}
			passData3.kernelIndex = passData3.cs.FindKernel("StpTaa");
			passData3.viewCount = config.numActiveViews;
			passData3.dispatchSize = new Vector2Int(CoreUtils.DivRoundUp(config.outputImageSize.x, vector2Int.x), CoreUtils.DivRoundUp(config.outputImageSize.y, vector2Int.y));
			passData3.intermediateColor = UseTexture(computeRenderGraphBuilder3, setupData3.intermediateColor);
			passData3.intermediateWeights = UseTexture(computeRenderGraphBuilder3, preTaaData2.intermediateWeights);
			passData3.priorFeedback = UseTexture(computeRenderGraphBuilder3, renderGraph.ImportTexture(previousHistoryTexture4));
			passData3.depthMotion = UseTexture(computeRenderGraphBuilder3, renderGraph.ImportTexture(currentHistoryTexture));
			passData3.convergence = UseTexture(computeRenderGraphBuilder3, renderGraph.ImportTexture(currentHistoryTexture3));
			passData3.feedback = UseTexture(computeRenderGraphBuilder3, renderGraph.ImportTexture(currentHistoryTexture4), AccessFlags.WriteAll);
			passData3.output = UseTexture(computeRenderGraphBuilder3, config.destination, AccessFlags.WriteAll);
			computeRenderGraphBuilder3.SetRenderFunc(delegate(TaaData data, ComputeGraphContext ctx)
			{
				ConstantBuffer.Set<StpConstantBufferData>(data.cs, ShaderResources._StpConstantBufferData);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpBlueNoiseIn, data.noiseTexture);
				if (data.debugView.IsValid())
				{
					ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpDebugOut, data.debugView);
				}
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpIntermediateColor, data.intermediateColor);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpIntermediateWeights, data.intermediateWeights);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpPriorFeedback, data.priorFeedback);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpDepthMotion, data.depthMotion);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpConvergence, data.convergence);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpFeedback, data.feedback);
				ctx.cmd.SetComputeTextureParam(data.cs, data.kernelIndex, ShaderResources._StpOutput, data.output);
				ctx.cmd.DispatchCompute(data.cs, data.kernelIndex, data.dispatchSize.x, data.dispatchSize.y, data.viewCount);
			});
			taaData = passData3;
		}
		return taaData.output;
	}
}
