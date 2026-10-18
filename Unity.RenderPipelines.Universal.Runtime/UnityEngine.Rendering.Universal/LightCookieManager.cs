using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal class LightCookieManager : IDisposable
{
	private static class ShaderProperty
	{
		public static readonly int mainLightTexture = Shader.PropertyToID("_MainLightCookieTexture");

		public static readonly int mainLightWorldToLight = Shader.PropertyToID("_MainLightWorldToLight");

		public static readonly int mainLightCookieTextureFormat = Shader.PropertyToID("_MainLightCookieTextureFormat");

		public static readonly int additionalLightsCookieAtlasTexture = Shader.PropertyToID("_AdditionalLightsCookieAtlasTexture");

		public static readonly int additionalLightsCookieAtlasTextureFormat = Shader.PropertyToID("_AdditionalLightsCookieAtlasTextureFormat");

		public static readonly int additionalLightsCookieEnableBits = Shader.PropertyToID("_AdditionalLightsCookieEnableBits");

		public static readonly int additionalLightsCookieAtlasUVRectBuffer = Shader.PropertyToID("_AdditionalLightsCookieAtlasUVRectBuffer");

		public static readonly int additionalLightsCookieAtlasUVRects = Shader.PropertyToID("_AdditionalLightsCookieAtlasUVRects");

		public static readonly int additionalLightsWorldToLightBuffer = Shader.PropertyToID("_AdditionalLightsWorldToLightBuffer");

		public static readonly int additionalLightsLightTypeBuffer = Shader.PropertyToID("_AdditionalLightsLightTypeBuffer");

		public static readonly int additionalLightsWorldToLights = Shader.PropertyToID("_AdditionalLightsWorldToLights");

		public static readonly int additionalLightsLightTypes = Shader.PropertyToID("_AdditionalLightsLightTypes");
	}

	private enum LightCookieShaderFormat
	{
		None = -1,
		RGB,
		Alpha,
		Red
	}

	public struct Settings
	{
		public struct AtlasSettings
		{
			public Vector2Int resolution;

			public GraphicsFormat format;

			public bool isPow2
			{
				get
				{
					if (Mathf.IsPowerOfTwo(resolution.x))
					{
						return Mathf.IsPowerOfTwo(resolution.y);
					}
					return false;
				}
			}

			public bool isSquare => resolution.x == resolution.y;
		}

		public AtlasSettings atlas;

		public int maxAdditionalLights;

		public float cubeOctahedralSizeScale;

		public bool useStructuredBuffer;

		public static Settings Create()
		{
			Settings result = default(Settings);
			result.atlas.resolution = new Vector2Int(1024, 1024);
			result.atlas.format = GraphicsFormat.R8G8B8A8_SRGB;
			result.maxAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
			result.cubeOctahedralSizeScale = 2.5f;
			result.useStructuredBuffer = RenderingUtils.useStructuredBuffer;
			return result;
		}
	}

	private struct LightCookieMapping
	{
		public ushort visibleLightIndex;

		public ushort lightBufferIndex;

		public Light light;

		public static Func<LightCookieMapping, LightCookieMapping, int> s_CompareByCookieSize = delegate(LightCookieMapping a, LightCookieMapping b)
		{
			Texture cookie = a.light.cookie;
			Texture cookie2 = b.light.cookie;
			int num = cookie.width * cookie.height;
			int num2 = cookie2.width * cookie2.height - num;
			if (num2 == 0)
			{
				int instanceID = cookie.GetInstanceID();
				int instanceID2 = cookie2.GetInstanceID();
				return instanceID - instanceID2;
			}
			return num2;
		};

		public static Func<LightCookieMapping, LightCookieMapping, int> s_CompareByBufferIndex = (LightCookieMapping a, LightCookieMapping b) => a.lightBufferIndex - b.lightBufferIndex;
	}

	private readonly struct WorkSlice<T>
	{
		private readonly T[] m_Data;

		private readonly int m_Start;

		private readonly int m_Length;

		public T this[int index]
		{
			get
			{
				return m_Data[m_Start + index];
			}
			set
			{
				m_Data[m_Start + index] = value;
			}
		}

		public int length => m_Length;

		public int capacity => m_Data.Length;

		public WorkSlice(T[] src, int srcLen = -1)
			: this(src, 0, srcLen)
		{
		}

		public WorkSlice(T[] src, int srcStart, int srcLen = -1)
		{
			m_Data = src;
			m_Start = srcStart;
			m_Length = ((srcLen < 0) ? src.Length : Math.Min(srcLen, src.Length));
		}

		public void Sort(Func<T, T, int> compare)
		{
			if (m_Length > 1)
			{
				Sorting.QuickSort(m_Data, m_Start, m_Start + m_Length - 1, compare);
			}
		}
	}

	private class WorkMemory
	{
		public LightCookieMapping[] lightMappings;

		public Vector4[] uvRects;

		public void Resize(int size)
		{
			if (!(size <= lightMappings?.Length))
			{
				size = Math.Max(size, (size + 15) / 16 * 16);
				lightMappings = new LightCookieMapping[size];
				uvRects = new Vector4[size];
			}
		}
	}

	private class LightCookieShaderData : IDisposable
	{
		private int m_Size;

		private bool m_UseStructuredBuffer;

		private Matrix4x4[] m_WorldToLightCpuData;

		private Vector4[] m_AtlasUVRectCpuData;

		private float[] m_LightTypeCpuData;

		private ShaderBitArray m_CookieEnableBitsCpuData;

		private ComputeBuffer m_WorldToLightBuffer;

		private ComputeBuffer m_AtlasUVRectBuffer;

		private ComputeBuffer m_LightTypeBuffer;

		public Matrix4x4[] worldToLights => m_WorldToLightCpuData;

		public ShaderBitArray cookieEnableBits => m_CookieEnableBitsCpuData;

		public Vector4[] atlasUVRects => m_AtlasUVRectCpuData;

		public float[] lightTypes => m_LightTypeCpuData;

		public bool isUploaded { get; set; }

		public LightCookieShaderData(int size, bool useStructuredBuffer)
		{
			m_UseStructuredBuffer = useStructuredBuffer;
			Resize(size);
		}

		public void Dispose()
		{
			if (m_UseStructuredBuffer)
			{
				m_WorldToLightBuffer?.Dispose();
				m_AtlasUVRectBuffer?.Dispose();
				m_LightTypeBuffer?.Dispose();
			}
		}

		public void Resize(int size)
		{
			if (size > m_Size)
			{
				if (m_Size > 0)
				{
					Dispose();
				}
				m_WorldToLightCpuData = new Matrix4x4[size];
				m_AtlasUVRectCpuData = new Vector4[size];
				m_LightTypeCpuData = new float[size];
				m_CookieEnableBitsCpuData.Resize(size);
				if (m_UseStructuredBuffer)
				{
					m_WorldToLightBuffer = new ComputeBuffer(size, Marshal.SizeOf<Matrix4x4>());
					m_AtlasUVRectBuffer = new ComputeBuffer(size, Marshal.SizeOf<Vector4>());
					m_LightTypeBuffer = new ComputeBuffer(size, Marshal.SizeOf<float>());
				}
				m_Size = size;
			}
		}

		public void Upload(CommandBuffer cmd)
		{
			if (m_UseStructuredBuffer)
			{
				m_WorldToLightBuffer.SetData(m_WorldToLightCpuData);
				m_AtlasUVRectBuffer.SetData(m_AtlasUVRectCpuData);
				m_LightTypeBuffer.SetData(m_LightTypeCpuData);
				cmd.SetGlobalBuffer(ShaderProperty.additionalLightsWorldToLightBuffer, m_WorldToLightBuffer);
				cmd.SetGlobalBuffer(ShaderProperty.additionalLightsCookieAtlasUVRectBuffer, m_AtlasUVRectBuffer);
				cmd.SetGlobalBuffer(ShaderProperty.additionalLightsLightTypeBuffer, m_LightTypeBuffer);
			}
			else
			{
				cmd.SetGlobalMatrixArray(ShaderProperty.additionalLightsWorldToLights, m_WorldToLightCpuData);
				cmd.SetGlobalVectorArray(ShaderProperty.additionalLightsCookieAtlasUVRects, m_AtlasUVRectCpuData);
				cmd.SetGlobalFloatArray(ShaderProperty.additionalLightsLightTypes, m_LightTypeCpuData);
			}
			cmd.SetGlobalFloatArray(ShaderProperty.additionalLightsCookieEnableBits, m_CookieEnableBitsCpuData.data);
			isUploaded = true;
		}

		public void Clear(CommandBuffer cmd)
		{
			if (isUploaded)
			{
				m_CookieEnableBitsCpuData.Clear();
				cmd.SetGlobalFloatArray(ShaderProperty.additionalLightsCookieEnableBits, m_CookieEnableBitsCpuData.data);
				isUploaded = false;
			}
		}
	}

	private static readonly Matrix4x4 s_DirLightProj = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, -0.5f, 0.5f);

	private Texture2DAtlas m_AdditionalLightsCookieAtlas;

	private LightCookieShaderData m_AdditionalLightsCookieShaderData;

	private readonly Settings m_Settings;

	private WorkMemory m_WorkMem;

	private int[] m_VisibleLightIndexToShaderDataIndex;

	private const int k_MaxCookieSizeDivisor = 16;

	private int m_CookieSizeDivisor = 1;

	private uint m_PrevCookieRequestPixelCount = uint.MaxValue;

	private int m_PrevWarnFrame = -1;

	internal bool IsKeywordLightCookieEnabled { get; private set; }

	internal RTHandle AdditionalLightsCookieAtlasTexture => m_AdditionalLightsCookieAtlas?.AtlasTexture;

	public LightCookieManager(ref Settings settings)
	{
		m_Settings = settings;
		m_WorkMem = new WorkMemory();
	}

	private void InitAdditionalLights(int size)
	{
		m_AdditionalLightsCookieAtlas = new Texture2DAtlas(m_Settings.atlas.resolution.x, m_Settings.atlas.resolution.y, m_Settings.atlas.format, FilterMode.Bilinear, powerOfTwoPadding: false, "Universal Light Cookie Atlas", useMipMap: false);
		m_AdditionalLightsCookieShaderData = new LightCookieShaderData(size, m_Settings.useStructuredBuffer);
		m_VisibleLightIndexToShaderDataIndex = new int[m_Settings.maxAdditionalLights + 1];
		m_CookieSizeDivisor = 1;
		m_PrevCookieRequestPixelCount = uint.MaxValue;
	}

	public bool isInitialized()
	{
		if (m_AdditionalLightsCookieAtlas != null)
		{
			return m_AdditionalLightsCookieShaderData != null;
		}
		return false;
	}

	public void Dispose()
	{
		m_AdditionalLightsCookieAtlas?.Release();
		m_AdditionalLightsCookieShaderData?.Dispose();
	}

	public int GetLightCookieShaderDataIndex(int visibleLightIndex)
	{
		if (!isInitialized())
		{
			return -1;
		}
		return m_VisibleLightIndexToShaderDataIndex[visibleLightIndex];
	}

	public void Setup(CommandBuffer cmd, UniversalLightData lightData)
	{
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.LightCookies)))
		{
			bool flag = lightData.mainLightIndex >= 0;
			if (flag)
			{
				VisibleLight visibleMainLight = lightData.visibleLights[lightData.mainLightIndex];
				flag = SetupMainLight(cmd, ref visibleMainLight);
			}
			bool flag2 = lightData.additionalLightsCount > 0;
			if (flag2)
			{
				flag2 = SetupAdditionalLights(cmd, lightData);
			}
			if (!flag2)
			{
				if (m_VisibleLightIndexToShaderDataIndex != null && m_AdditionalLightsCookieShaderData.isUploaded)
				{
					int num = m_VisibleLightIndexToShaderDataIndex.Length;
					for (int i = 0; i < num; i++)
					{
						m_VisibleLightIndexToShaderDataIndex[i] = -1;
					}
				}
				m_AdditionalLightsCookieShaderData?.Clear(cmd);
			}
			IsKeywordLightCookieEnabled = flag || flag2;
			cmd.SetKeyword(in ShaderGlobalKeywords.LightCookies, IsKeywordLightCookieEnabled);
		}
	}

	private bool SetupMainLight(CommandBuffer cmd, ref VisibleLight visibleMainLight)
	{
		Light light = visibleMainLight.light;
		Texture cookie = light.cookie;
		bool num = cookie != null;
		if (num)
		{
			Matrix4x4 uvTransform = Matrix4x4.identity;
			float value = (float)GetLightCookieShaderFormat(cookie.graphicsFormat);
			if (light.TryGetComponent<UniversalAdditionalLightData>(out var component))
			{
				GetLightUVScaleOffset(ref component, ref uvTransform);
			}
			Matrix4x4 value2 = s_DirLightProj * uvTransform * visibleMainLight.localToWorldMatrix.inverse;
			cmd.SetGlobalTexture(ShaderProperty.mainLightTexture, cookie);
			cmd.SetGlobalMatrix(ShaderProperty.mainLightWorldToLight, value2);
			cmd.SetGlobalFloat(ShaderProperty.mainLightCookieTextureFormat, value);
			return num;
		}
		cmd.SetGlobalTexture(ShaderProperty.mainLightTexture, Texture2D.whiteTexture);
		cmd.SetGlobalMatrix(ShaderProperty.mainLightWorldToLight, Matrix4x4.identity);
		cmd.SetGlobalFloat(ShaderProperty.mainLightCookieTextureFormat, -1f);
		return num;
	}

	private LightCookieShaderFormat GetLightCookieShaderFormat(GraphicsFormat cookieFormat)
	{
		switch (cookieFormat)
		{
		default:
			return LightCookieShaderFormat.RGB;
		case (GraphicsFormat)54:
		case (GraphicsFormat)55:
			return LightCookieShaderFormat.Alpha;
		case GraphicsFormat.R8_SRGB:
		case GraphicsFormat.R8_UNorm:
		case GraphicsFormat.R8_SNorm:
		case GraphicsFormat.R8_UInt:
		case GraphicsFormat.R8_SInt:
		case GraphicsFormat.R16_UNorm:
		case GraphicsFormat.R16_SNorm:
		case GraphicsFormat.R16_UInt:
		case GraphicsFormat.R16_SInt:
		case GraphicsFormat.R32_UInt:
		case GraphicsFormat.R32_SInt:
		case GraphicsFormat.R16_SFloat:
		case GraphicsFormat.R32_SFloat:
		case GraphicsFormat.R_BC4_UNorm:
		case GraphicsFormat.R_BC4_SNorm:
		case GraphicsFormat.R_EAC_UNorm:
		case GraphicsFormat.R_EAC_SNorm:
			return LightCookieShaderFormat.Red;
		}
	}

	private void GetLightUVScaleOffset(ref UniversalAdditionalLightData additionalLightData, ref Matrix4x4 uvTransform)
	{
		Vector2 vector = Vector2.one / additionalLightData.lightCookieSize;
		Vector2 lightCookieOffset = additionalLightData.lightCookieOffset;
		if (Mathf.Abs(vector.x) < half.MinValue)
		{
			vector.x = Mathf.Sign(vector.x) * half.MinValue;
		}
		if (Mathf.Abs(vector.y) < half.MinValue)
		{
			vector.y = Mathf.Sign(vector.y) * half.MinValue;
		}
		uvTransform = Matrix4x4.Scale(new Vector3(vector.x, vector.y, 1f));
		uvTransform.SetColumn(3, new Vector4((0f - lightCookieOffset.x) * vector.x, (0f - lightCookieOffset.y) * vector.y, 0f, 1f));
	}

	private bool SetupAdditionalLights(CommandBuffer cmd, UniversalLightData lightData)
	{
		int size = Math.Min(m_Settings.maxAdditionalLights, lightData.visibleLights.Length);
		m_WorkMem.Resize(size);
		int num = FilterAndValidateAdditionalLights(lightData, m_WorkMem.lightMappings);
		if (num <= 0)
		{
			return false;
		}
		if (!isInitialized())
		{
			InitAdditionalLights(num);
		}
		WorkSlice<LightCookieMapping> validLightMappings = new WorkSlice<LightCookieMapping>(m_WorkMem.lightMappings, num);
		int srcLen = UpdateAdditionalLightsAtlas(cmd, ref validLightMappings, m_WorkMem.uvRects);
		WorkSlice<Vector4> validUvRects = new WorkSlice<Vector4>(m_WorkMem.uvRects, srcLen);
		UploadAdditionalLights(cmd, lightData, ref validLightMappings, ref validUvRects);
		return validUvRects.length > 0;
	}

	private int FilterAndValidateAdditionalLights(UniversalLightData lightData, LightCookieMapping[] validLightMappings)
	{
		int mainLightIndex = lightData.mainLightIndex;
		int num = 0;
		int num2 = 0;
		int length = lightData.visibleLights.Length;
		LightCookieMapping lightCookieMapping = default(LightCookieMapping);
		for (int i = 0; i < length; i++)
		{
			if (i == mainLightIndex)
			{
				num--;
				continue;
			}
			ref VisibleLight reference = ref lightData.visibleLights.UnsafeElementAtMutable(i);
			Light light = reference.light;
			if (light.cookie == null)
			{
				continue;
			}
			LightType lightType = reference.lightType;
			if (lightType != 0 && lightType != LightType.Point && lightType != LightType.Directional)
			{
				Debug.LogWarning("Additional " + lightType.ToString() + " light called '" + light.name + "' has a light cookie which will not be visible.", light);
				continue;
			}
			lightCookieMapping.visibleLightIndex = (ushort)i;
			lightCookieMapping.lightBufferIndex = (ushort)(i + num);
			lightCookieMapping.light = light;
			if (lightCookieMapping.lightBufferIndex >= validLightMappings.Length || num2 + 1 >= validLightMappings.Length)
			{
				if (length > m_Settings.maxAdditionalLights && Time.frameCount - m_PrevWarnFrame > 3600)
				{
					m_PrevWarnFrame = Time.frameCount;
					Debug.LogWarning("Max light cookies (" + validLightMappings.Length + ") reached. Some visible lights (" + (length - i - 1) + ") might skip light cookie rendering.");
				}
				break;
			}
			validLightMappings[num2++] = lightCookieMapping;
		}
		return num2;
	}

	private int UpdateAdditionalLightsAtlas(CommandBuffer cmd, ref WorkSlice<LightCookieMapping> validLightMappings, Vector4[] textureAtlasUVRects)
	{
		validLightMappings.Sort(LightCookieMapping.s_CompareByCookieSize);
		uint num = ComputeCookieRequestPixelCount(ref validLightMappings);
		Vector2Int referenceSize = m_AdditionalLightsCookieAtlas.AtlasTexture.referenceSize;
		float requestAtlasRatio = (float)num / (float)(referenceSize.x * referenceSize.y);
		int num2 = ApproximateCookieSizeDivisor(requestAtlasRatio);
		if (num2 < m_CookieSizeDivisor && num < m_PrevCookieRequestPixelCount)
		{
			m_AdditionalLightsCookieAtlas.ResetAllocator();
			m_CookieSizeDivisor = num2;
		}
		int num3 = 0;
		while (num3 <= 0)
		{
			num3 = FetchUVRects(cmd, ref validLightMappings, textureAtlasUVRects, m_CookieSizeDivisor);
			if (num3 <= 0)
			{
				m_AdditionalLightsCookieAtlas.ResetAllocator();
				m_CookieSizeDivisor = Mathf.Max(m_CookieSizeDivisor + 1, num2);
				m_PrevCookieRequestPixelCount = num;
			}
		}
		return num3;
	}

	private int FetchUVRects(CommandBuffer cmd, ref WorkSlice<LightCookieMapping> validLightMappings, Vector4[] textureAtlasUVRects, int cookieSizeDivisor)
	{
		int result = 0;
		for (int i = 0; i < validLightMappings.length; i++)
		{
			Texture cookie = validLightMappings[i].light.cookie;
			Vector4 zero = Vector4.zero;
			zero = ((cookie.dimension != TextureDimension.Cube) ? Fetch2D(cmd, cookie, cookieSizeDivisor) : FetchCube(cmd, cookie, cookieSizeDivisor));
			if (!(zero != Vector4.zero))
			{
				if (cookieSizeDivisor > 16)
				{
					Debug.LogWarning("Light cookies atlas is extremely full! Some of the light cookies were discarded. Increase light cookie atlas space or reduce the amount of unique light cookies.");
					return result;
				}
				return 0;
			}
			if (!SystemInfo.graphicsUVStartsAtTop)
			{
				zero.w = 1f - zero.w - zero.y;
			}
			textureAtlasUVRects[result++] = zero;
		}
		return result;
	}

	private uint ComputeCookieRequestPixelCount(ref WorkSlice<LightCookieMapping> validLightMappings)
	{
		uint num = 0u;
		int num2 = 0;
		for (int i = 0; i < validLightMappings.length; i++)
		{
			Texture cookie = validLightMappings[i].light.cookie;
			int instanceID = cookie.GetInstanceID();
			if (instanceID != num2)
			{
				num2 = instanceID;
				int num3 = cookie.width * cookie.height;
				num += (uint)num3;
			}
		}
		return num;
	}

	private int ApproximateCookieSizeDivisor(float requestAtlasRatio)
	{
		return (int)Mathf.Max(Mathf.Ceil(Mathf.Sqrt(requestAtlasRatio)), 1f);
	}

	private Vector4 Fetch2D(CommandBuffer cmd, Texture cookie, int cookieSizeDivisor = 1)
	{
		Vector4 scaleOffset = Vector4.zero;
		int num = Mathf.Max(cookie.width / cookieSizeDivisor, 4);
		int num2 = Mathf.Max(cookie.height / cookieSizeDivisor, 4);
		Vector2 cookieSize = new Vector2(num, num2);
		if (m_AdditionalLightsCookieAtlas.IsCached(out scaleOffset, cookie))
		{
			m_AdditionalLightsCookieAtlas.UpdateTexture(cmd, cookie, ref scaleOffset);
		}
		else
		{
			m_AdditionalLightsCookieAtlas.AllocateTexture(cmd, ref scaleOffset, cookie, num, num2);
		}
		AdjustUVRect(ref scaleOffset, cookie, ref cookieSize);
		return scaleOffset;
	}

	private Vector4 FetchCube(CommandBuffer cmd, Texture cookie, int cookieSizeDivisor = 1)
	{
		Vector4 scaleOffset = Vector4.zero;
		int num = Mathf.Max(ComputeOctahedralCookieSize(cookie) / cookieSizeDivisor, 4);
		if (m_AdditionalLightsCookieAtlas.IsCached(out scaleOffset, cookie))
		{
			m_AdditionalLightsCookieAtlas.UpdateTexture(cmd, cookie, ref scaleOffset);
		}
		else
		{
			m_AdditionalLightsCookieAtlas.AllocateTexture(cmd, ref scaleOffset, cookie, num, num);
		}
		Vector2 cookieSize = Vector2.one * num;
		AdjustUVRect(ref scaleOffset, cookie, ref cookieSize);
		return scaleOffset;
	}

	private int ComputeOctahedralCookieSize(Texture cookie)
	{
		int num = Math.Max(cookie.width, cookie.height);
		if (m_Settings.atlas.isPow2)
		{
			return num * Mathf.NextPowerOfTwo((int)m_Settings.cubeOctahedralSizeScale);
		}
		return (int)((float)num * m_Settings.cubeOctahedralSizeScale + 0.5f);
	}

	private void AdjustUVRect(ref Vector4 uvScaleOffset, Texture cookie, ref Vector2 cookieSize)
	{
		if (uvScaleOffset != Vector4.zero)
		{
			ShrinkUVRect(ref uvScaleOffset, 0.5f, ref cookieSize);
		}
	}

	private void ShrinkUVRect(ref Vector4 uvScaleOffset, float amountPixels, ref Vector2 cookieSize)
	{
		Vector2 vector = Vector2.one * amountPixels / cookieSize;
		Vector2 vector2 = (cookieSize - Vector2.one * (amountPixels * 2f)) / cookieSize;
		uvScaleOffset.z += uvScaleOffset.x * vector.x;
		uvScaleOffset.w += uvScaleOffset.y * vector.y;
		uvScaleOffset.x *= vector2.x;
		uvScaleOffset.y *= vector2.y;
	}

	private void UploadAdditionalLights(CommandBuffer cmd, UniversalLightData lightData, ref WorkSlice<LightCookieMapping> validLightMappings, ref WorkSlice<Vector4> validUvRects)
	{
		cmd.SetGlobalTexture(ShaderProperty.additionalLightsCookieAtlasTexture, m_AdditionalLightsCookieAtlas.AtlasTexture);
		cmd.SetGlobalFloat(ShaderProperty.additionalLightsCookieAtlasTextureFormat, (float)GetLightCookieShaderFormat(m_AdditionalLightsCookieAtlas.AtlasTexture.rt.graphicsFormat));
		if (m_VisibleLightIndexToShaderDataIndex.Length < lightData.visibleLights.Length)
		{
			m_VisibleLightIndexToShaderDataIndex = new int[lightData.visibleLights.Length];
		}
		int num = Math.Min(m_VisibleLightIndexToShaderDataIndex.Length, lightData.visibleLights.Length);
		for (int i = 0; i < num; i++)
		{
			m_VisibleLightIndexToShaderDataIndex[i] = -1;
		}
		m_AdditionalLightsCookieShaderData.Resize(m_Settings.maxAdditionalLights);
		Matrix4x4[] worldToLights = m_AdditionalLightsCookieShaderData.worldToLights;
		ShaderBitArray cookieEnableBits = m_AdditionalLightsCookieShaderData.cookieEnableBits;
		Vector4[] atlasUVRects = m_AdditionalLightsCookieShaderData.atlasUVRects;
		float[] lightTypes = m_AdditionalLightsCookieShaderData.lightTypes;
		Array.Clear(atlasUVRects, 0, atlasUVRects.Length);
		cookieEnableBits.Clear();
		for (int j = 0; j < validUvRects.length; j++)
		{
			int visibleLightIndex = validLightMappings[j].visibleLightIndex;
			int lightBufferIndex = validLightMappings[j].lightBufferIndex;
			m_VisibleLightIndexToShaderDataIndex[visibleLightIndex] = lightBufferIndex;
			ref VisibleLight reference = ref lightData.visibleLights.UnsafeElementAtMutable(visibleLightIndex);
			lightTypes[lightBufferIndex] = (float)reference.lightType;
			worldToLights[lightBufferIndex] = reference.localToWorldMatrix.inverse;
			atlasUVRects[lightBufferIndex] = validUvRects[j];
			cookieEnableBits[lightBufferIndex] = true;
			if (reference.lightType == LightType.Spot)
			{
				float spotAngle = reference.spotAngle;
				float range = reference.range;
				Matrix4x4 matrix4x = Matrix4x4.Perspective(spotAngle, 1f, 0.001f, range);
				matrix4x.SetColumn(2, matrix4x.GetColumn(2) * -1f);
				worldToLights[lightBufferIndex] = matrix4x * worldToLights[lightBufferIndex];
			}
			else if (reference.lightType == LightType.Directional)
			{
				reference.light.TryGetComponent<UniversalAdditionalLightData>(out var component);
				Matrix4x4 uvTransform = Matrix4x4.identity;
				GetLightUVScaleOffset(ref component, ref uvTransform);
				Matrix4x4 matrix4x2 = s_DirLightProj * uvTransform * reference.localToWorldMatrix.inverse;
				worldToLights[lightBufferIndex] = matrix4x2;
			}
		}
		m_AdditionalLightsCookieShaderData.Upload(cmd);
	}
}
