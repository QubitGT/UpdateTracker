using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

public sealed class LensFlareCommonSRP
{
	internal class LensFlareCompInfo
	{
		internal int index;

		internal LensFlareComponentSRP comp;

		internal LensFlareCompInfo(int idx, LensFlareComponentSRP cmp)
		{
			index = idx;
			comp = cmp;
		}
	}

	private static LensFlareCommonSRP m_Instance = null;

	private static readonly object m_Padlock = new object();

	private static List<LensFlareCompInfo> m_Data = new List<LensFlareCompInfo>();

	private static List<int> m_AvailableIndicies = new List<int>();

	public static int maxLensFlareWithOcclusion = 128;

	public static int maxLensFlareWithOcclusionTemporalSample = 8;

	public static int mergeNeeded = 1;

	public static RTHandle occlusionRT = null;

	private static int frameIdx = 0;

	internal static readonly int _FlareOcclusionPermutation = Shader.PropertyToID("_FlareOcclusionPermutation");

	internal static readonly int _FlareOcclusionRemapTex = Shader.PropertyToID("_FlareOcclusionRemapTex");

	internal static readonly int _FlareOcclusionTex = Shader.PropertyToID("_FlareOcclusionTex");

	internal static readonly int _FlareOcclusionIndex = Shader.PropertyToID("_FlareOcclusionIndex");

	internal static readonly int _FlareCloudOpacity = Shader.PropertyToID("_FlareCloudOpacity");

	internal static readonly int _FlareSunOcclusionTex = Shader.PropertyToID("_FlareSunOcclusionTex");

	internal static readonly int _FlareTex = Shader.PropertyToID("_FlareTex");

	internal static readonly int _FlareColorValue = Shader.PropertyToID("_FlareColorValue");

	internal static readonly int _FlareData0 = Shader.PropertyToID("_FlareData0");

	internal static readonly int _FlareData1 = Shader.PropertyToID("_FlareData1");

	internal static readonly int _FlareData2 = Shader.PropertyToID("_FlareData2");

	internal static readonly int _FlareData3 = Shader.PropertyToID("_FlareData3");

	internal static readonly int _FlareData4 = Shader.PropertyToID("_FlareData4");

	internal static readonly int _FlareData5 = Shader.PropertyToID("_FlareData5");

	internal static readonly int _FlareRadialTint = Shader.PropertyToID("_FlareRadialTint");

	internal static readonly int _ViewId = Shader.PropertyToID("_ViewId");

	internal static readonly int _LensFlareScreenSpaceBloomMipTexture = Shader.PropertyToID("_LensFlareScreenSpaceBloomMipTexture");

	internal static readonly int _LensFlareScreenSpaceResultTexture = Shader.PropertyToID("_LensFlareScreenSpaceResultTexture");

	internal static readonly int _LensFlareScreenSpaceSpectralLut = Shader.PropertyToID("_LensFlareScreenSpaceSpectralLut");

	internal static readonly int _LensFlareScreenSpaceStreakTex = Shader.PropertyToID("_LensFlareScreenSpaceStreakTex");

	internal static readonly int _LensFlareScreenSpaceMipLevel = Shader.PropertyToID("_LensFlareScreenSpaceMipLevel");

	internal static readonly int _LensFlareScreenSpaceTintColor = Shader.PropertyToID("_LensFlareScreenSpaceTintColor");

	internal static readonly int _LensFlareScreenSpaceParams1 = Shader.PropertyToID("_LensFlareScreenSpaceParams1");

	internal static readonly int _LensFlareScreenSpaceParams2 = Shader.PropertyToID("_LensFlareScreenSpaceParams2");

	internal static readonly int _LensFlareScreenSpaceParams3 = Shader.PropertyToID("_LensFlareScreenSpaceParams3");

	internal static readonly int _LensFlareScreenSpaceParams4 = Shader.PropertyToID("_LensFlareScreenSpaceParams4");

	internal static readonly int _LensFlareScreenSpaceParams5 = Shader.PropertyToID("_LensFlareScreenSpaceParams5");

	private static readonly bool s_SupportsLensFlare16bitsFormat = SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, GraphicsFormatUsage.Render);

	private static readonly bool s_SupportsLensFlare32bitsFormat = SystemInfo.IsFormatSupported(GraphicsFormat.R32_SFloat, GraphicsFormatUsage.Render);

	private static readonly bool s_SupportsLensFlare16bitsFormatWithLoadStore = SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, GraphicsFormatUsage.LoadStore);

	private static readonly bool s_SupportsLensFlare32bitsFormatWithLoadStore = SystemInfo.IsFormatSupported(GraphicsFormat.R32_SFloat, GraphicsFormatUsage.LoadStore);

	private static bool requireOcclusionRTRandomWrite => mergeNeeded > 0;

	public static LensFlareCommonSRP Instance
	{
		get
		{
			if (m_Instance == null)
			{
				lock (m_Padlock)
				{
					if (m_Instance == null)
					{
						m_Instance = new LensFlareCommonSRP();
					}
				}
			}
			return m_Instance;
		}
	}

	private List<LensFlareCompInfo> Data => m_Data;

	private LensFlareCommonSRP()
	{
	}

	private static bool CheckOcclusionBasedOnDeviceType()
	{
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
		{
			return SystemInfo.graphicsDeviceType != GraphicsDeviceType.WebGPU;
		}
		return false;
	}

	public static bool IsOcclusionRTCompatible()
	{
		if (requireOcclusionRTRandomWrite)
		{
			if (CheckOcclusionBasedOnDeviceType())
			{
				if (!s_SupportsLensFlare16bitsFormatWithLoadStore)
				{
					return s_SupportsLensFlare32bitsFormatWithLoadStore;
				}
				return true;
			}
			return false;
		}
		if (CheckOcclusionBasedOnDeviceType())
		{
			if (!s_SupportsLensFlare16bitsFormat)
			{
				return s_SupportsLensFlare32bitsFormat;
			}
			return true;
		}
		return false;
	}

	private static GraphicsFormat GetOcclusionRTFormat()
	{
		if (requireOcclusionRTRandomWrite ? s_SupportsLensFlare16bitsFormatWithLoadStore : s_SupportsLensFlare16bitsFormat)
		{
			return GraphicsFormat.R16_SFloat;
		}
		return GraphicsFormat.R32_SFloat;
	}

	public static void Initialize()
	{
		frameIdx = 0;
		if (IsOcclusionRTCompatible() && occlusionRT == null)
		{
			occlusionRT = RTHandles.Alloc(maxLensFlareWithOcclusion, Mathf.Max(mergeNeeded * (maxLensFlareWithOcclusionTemporalSample + 1), 1), GetOcclusionRTFormat(), TextureXR.slices, FilterMode.Point, TextureWrapMode.Repeat, TextureDimension.Tex2DArray, requireOcclusionRTRandomWrite);
		}
	}

	public static void Dispose()
	{
		if (IsOcclusionRTCompatible() && occlusionRT != null)
		{
			RTHandles.Release(occlusionRT);
			occlusionRT = null;
		}
	}

	public bool IsEmpty()
	{
		return Data.Count == 0;
	}

	private int GetNextAvailableIndex()
	{
		if (m_AvailableIndicies.Count == 0)
		{
			return m_Data.Count;
		}
		int result = m_AvailableIndicies[m_AvailableIndicies.Count - 1];
		m_AvailableIndicies.RemoveAt(m_AvailableIndicies.Count - 1);
		return result;
	}

	public void AddData(LensFlareComponentSRP newData)
	{
		if (!m_Data.Exists((LensFlareCompInfo x) => x.comp == newData))
		{
			m_Data.Add(new LensFlareCompInfo(GetNextAvailableIndex(), newData));
		}
	}

	public void RemoveData(LensFlareComponentSRP data)
	{
		LensFlareCompInfo lensFlareCompInfo = m_Data.Find((LensFlareCompInfo x) => x.comp == data);
		if (lensFlareCompInfo != null)
		{
			int index = lensFlareCompInfo.index;
			m_Data.Remove(lensFlareCompInfo);
			m_AvailableIndicies.Add(index);
			if (m_Data.Count == 0)
			{
				m_AvailableIndicies.Clear();
			}
		}
	}

	public static float ShapeAttenuationPointLight()
	{
		return 1f;
	}

	public static float ShapeAttenuationDirLight(Vector3 forward, Vector3 wo)
	{
		return Mathf.Max(Vector3.Dot(-forward, wo), 0f);
	}

	public static float ShapeAttenuationSpotConeLight(Vector3 forward, Vector3 wo, float spotAngle, float innerSpotPercent01)
	{
		float num = Mathf.Max(Mathf.Cos(0.5f * spotAngle * (MathF.PI / 180f)), 0f);
		float num2 = Mathf.Max(Mathf.Cos(0.5f * spotAngle * (MathF.PI / 180f) * innerSpotPercent01), 0f);
		return Mathf.Clamp01((Mathf.Max(Vector3.Dot(forward, wo), 0f) - num) / (num2 - num));
	}

	public static float ShapeAttenuationSpotBoxLight(Vector3 forward, Vector3 wo)
	{
		return Mathf.Max(Mathf.Sign(Vector3.Dot(forward, wo)), 0f);
	}

	public static float ShapeAttenuationSpotPyramidLight(Vector3 forward, Vector3 wo)
	{
		return ShapeAttenuationSpotBoxLight(forward, wo);
	}

	public static float ShapeAttenuationAreaTubeLight(Vector3 lightPositionWS, Vector3 lightSide, float lightWidth, Camera cam)
	{
		Transform transform = cam.transform;
		Vector3 position = lightPositionWS + 0.5f * lightWidth * lightSide;
		Vector3 position2 = lightPositionWS - 0.5f * lightWidth * lightSide;
		Vector3 position3 = lightPositionWS + 0.5f * lightWidth * transform.right;
		Vector3 position4 = lightPositionWS - 0.5f * lightWidth * transform.right;
		Vector3 p3 = transform.InverseTransformPoint(position);
		Vector3 p4 = transform.InverseTransformPoint(position2);
		Vector3 p5 = transform.InverseTransformPoint(position3);
		Vector3 p6 = transform.InverseTransformPoint(position4);
		float num = DiffLineIntegral(p5, p6);
		float num2 = DiffLineIntegral(p3, p4);
		if (!(num > 0f))
		{
			return 1f;
		}
		return num2 / num;
		static float DiffLineIntegral(Vector3 p1, Vector3 p2)
		{
			Vector3 normalized = (p2 - p1).normalized;
			if ((double)p1.z <= 0.0 && (double)p2.z <= 0.0)
			{
				return 0f;
			}
			if ((double)p1.z < 0.0)
			{
				p1 = (p1 * p2.z - p2 * p1.z) / (p2.z - p1.z);
			}
			if ((double)p2.z < 0.0)
			{
				p2 = (-p1 * p2.z + p2 * p1.z) / (0f - p2.z + p1.z);
			}
			float num3 = Vector3.Dot(p1, normalized);
			float l2 = Vector3.Dot(p2, normalized);
			Vector3 vector = p1 - num3 * normalized;
			float magnitude = vector.magnitude;
			return ((Fpo(magnitude, l2) - Fpo(magnitude, num3)) * vector.z + (Fwt(magnitude, l2) - Fwt(magnitude, num3)) * normalized.z) / MathF.PI;
		}
		static float Fpo(float d, float l)
		{
			return l / (d * (d * d + l * l)) + Mathf.Atan(l / d) / (d * d);
		}
		static float Fwt(float d, float l)
		{
			return l * l / (d * (d * d + l * l));
		}
	}

	private static float ShapeAttenuateForwardLight(Vector3 forward, Vector3 wo)
	{
		return Mathf.Max(Vector3.Dot(forward, wo), 0f);
	}

	public static float ShapeAttenuationAreaRectangleLight(Vector3 forward, Vector3 wo)
	{
		return ShapeAttenuateForwardLight(forward, wo);
	}

	public static float ShapeAttenuationAreaDiscLight(Vector3 forward, Vector3 wo)
	{
		return ShapeAttenuateForwardLight(forward, wo);
	}

	private static bool IsLensFlareSRPHidden(Camera cam, LensFlareComponentSRP comp, LensFlareDataSRP data)
	{
		if (!comp.enabled || !comp.gameObject.activeSelf || !comp.gameObject.activeInHierarchy || data == null || data.elements == null || data.elements.Length == 0 || comp.intensity <= 0f || (cam.cullingMask & (1 << comp.gameObject.layer)) == 0)
		{
			return true;
		}
		return false;
	}

	public static Vector4 GetFlareData0(Vector2 screenPos, Vector2 translationScale, Vector2 rayOff0, Vector2 vLocalScreenRatio, float angleDeg, float position, float angularOffset, Vector2 positionOffset, bool autoRotate)
	{
		if (!SystemInfo.graphicsUVStartsAtTop)
		{
			angleDeg *= -1f;
			positionOffset.y *= -1f;
		}
		float num = Mathf.Cos((0f - angularOffset) * (MathF.PI / 180f));
		float num2 = Mathf.Sin((0f - angularOffset) * (MathF.PI / 180f));
		Vector2 vector = -translationScale * (screenPos + screenPos * (position - 1f));
		vector = new Vector2(num * vector.x - num2 * vector.y, num2 * vector.x + num * vector.y);
		float num3 = angleDeg;
		num3 += 180f;
		if (autoRotate)
		{
			Vector2 vector2 = vector.normalized * vLocalScreenRatio * translationScale;
			num3 += -57.29578f * Mathf.Atan2(vector2.y, vector2.x);
		}
		num3 *= MathF.PI / 180f;
		float x = Mathf.Cos(0f - num3);
		float y = Mathf.Sin(0f - num3);
		return new Vector4(x, y, positionOffset.x + rayOff0.x * translationScale.x, 0f - positionOffset.y + rayOff0.y * translationScale.y);
	}

	private static Vector2 GetLensFlareRayOffset(Vector2 screenPos, float position, float globalCos0, float globalSin0, Vector2 vAspectRatio)
	{
		Vector2 vector = -(screenPos + screenPos * (position - 1f));
		return new Vector2(globalCos0 * vector.x - globalSin0 * vector.y, globalSin0 * vector.x + globalCos0 * vector.y);
	}

	private static Vector3 WorldToViewport(Camera camera, bool isLocalLight, bool isCameraRelative, Matrix4x4 viewProjMatrix, Vector3 positionWS)
	{
		if (isLocalLight)
		{
			return WorldToViewportLocal(isCameraRelative, viewProjMatrix, camera.transform.position, positionWS);
		}
		return WorldToViewportDistance(camera, positionWS);
	}

	private static Vector3 WorldToViewportLocal(bool isCameraRelative, Matrix4x4 viewProjMatrix, Vector3 cameraPosWS, Vector3 positionWS)
	{
		Vector3 vector = positionWS;
		if (isCameraRelative)
		{
			vector -= cameraPosWS;
		}
		Vector4 vector2 = viewProjMatrix * vector;
		Vector3 result = new Vector3(vector2.x, vector2.y, 0f);
		result /= vector2.w;
		result.x = result.x * 0.5f + 0.5f;
		result.y = result.y * 0.5f + 0.5f;
		result.y = 1f - result.y;
		result.z = vector2.w;
		return result;
	}

	private static Vector3 WorldToViewportDistance(Camera cam, Vector3 positionWS)
	{
		Vector4 vector = cam.worldToCameraMatrix * positionWS;
		Vector4 vector2 = cam.projectionMatrix * vector;
		Vector3 result = new Vector3(vector2.x, vector2.y, 0f);
		result /= vector2.w;
		result.x = result.x * 0.5f + 0.5f;
		result.y = result.y * 0.5f + 0.5f;
		result.z = vector2.w;
		return result;
	}

	public static bool IsCloudLayerOpacityNeeded(Camera cam)
	{
		if (Instance.IsEmpty())
		{
			return false;
		}
		foreach (LensFlareCompInfo datum in Instance.Data)
		{
			if (datum != null && !(datum.comp == null))
			{
				LensFlareComponentSRP comp = datum.comp;
				LensFlareDataSRP lensFlareData = comp.lensFlareData;
				if (!IsLensFlareSRPHidden(cam, comp, lensFlareData) && comp.useOcclusion && (!comp.useOcclusion || comp.sampleCount != 0) && comp.environmentOcclusion)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void SetOcclusionPermutation(CommandBuffer cmd, bool useFogOpacityOcclusion, int _FlareSunOcclusionTex, Texture sunOcclusionTexture)
	{
		uint num = 1u;
		if (useFogOpacityOcclusion && sunOcclusionTexture != null)
		{
			num |= 4u;
			cmd.SetGlobalTexture(_FlareSunOcclusionTex, sunOcclusionTexture);
		}
		int value = (int)num;
		cmd.SetGlobalInt(_FlareOcclusionPermutation, value);
	}

	[Obsolete("Use ComputeOcclusion without _FlareOcclusionTex.._FlareData4 parameters.")]
	public static void ComputeOcclusion(Material lensFlareShader, Camera cam, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, UnsafeCommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture, int _FlareOcclusionTex, int _FlareCloudOpacity, int _FlareOcclusionIndex, int _FlareTex, int _FlareColorValue, int _FlareSunOcclusionTex, int _FlareData0, int _FlareData1, int _FlareData2, int _FlareData3, int _FlareData4)
	{
		ComputeOcclusion(lensFlareShader, cam, xr, xrIndex, actualWidth, actualHeight, usePanini, paniniDistance, paniniCropToFit, isCameraRelative, cameraPositionWS, viewProjMatrix, cmd.m_WrappedCommandBuffer, taaEnabled, hasCloudLayer, cloudOpacityTexture, sunOcclusionTexture, _FlareOcclusionTex, _FlareCloudOpacity, _FlareOcclusionIndex, _FlareTex, _FlareColorValue, _FlareSunOcclusionTex, _FlareData0, _FlareData1, _FlareData2, _FlareData3, _FlareData4);
	}

	public static void ComputeOcclusion(Material lensFlareShader, Camera cam, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, UnsafeCommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture)
	{
		ComputeOcclusion(lensFlareShader, cam, xr, xrIndex, actualWidth, actualHeight, usePanini, paniniDistance, paniniCropToFit, isCameraRelative, cameraPositionWS, viewProjMatrix, cmd.m_WrappedCommandBuffer, taaEnabled, hasCloudLayer, cloudOpacityTexture, sunOcclusionTexture);
	}

	[Obsolete("Use ComputeOcclusion without _FlareOcclusionTex.._FlareData4 parameters.")]
	public static void ComputeOcclusion(Material lensFlareShader, Camera cam, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, CommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture, int _FlareOcclusionTex, int _FlareCloudOpacity, int _FlareOcclusionIndex, int _FlareTex, int _FlareColorValue, int _FlareSunOcclusionTex, int _FlareData0, int _FlareData1, int _FlareData2, int _FlareData3, int _FlareData4)
	{
		ComputeOcclusion(lensFlareShader, cam, xr, xrIndex, actualWidth, actualHeight, usePanini, paniniDistance, paniniCropToFit, isCameraRelative, cameraPositionWS, viewProjMatrix, cmd, taaEnabled, hasCloudLayer, cloudOpacityTexture, sunOcclusionTexture);
	}

	private static bool ForceSingleElement(LensFlareDataElementSRP element)
	{
		if (element.allowMultipleElement && element.count != 1)
		{
			return element.flareType == SRPLensFlareType.Ring;
		}
		return true;
	}

	public static void ComputeOcclusion(Material lensFlareShader, Camera cam, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, CommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture)
	{
		if (!IsOcclusionRTCompatible())
		{
			return;
		}
		xr.StopSinglePass(cmd);
		if (Instance.IsEmpty())
		{
			return;
		}
		Vector2 vector = new Vector2(actualWidth, actualHeight);
		float x = vector.x / vector.y;
		Vector2 vector2 = new Vector2(x, 1f);
		if (xr.enabled && xr.singlePassEnabled)
		{
			CoreUtils.SetRenderTarget(cmd, occlusionRT, ClearFlag.None, 0, CubemapFace.Unknown, xrIndex);
			cmd.SetGlobalInt(_ViewId, xrIndex);
		}
		else
		{
			CoreUtils.SetRenderTarget(cmd, occlusionRT);
			if (xr.enabled)
			{
				cmd.SetGlobalInt(_ViewId, xr.multipassId);
			}
			else
			{
				cmd.SetGlobalInt(_ViewId, -1);
			}
		}
		if (!taaEnabled)
		{
			cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		}
		_ = 1f / (float)maxLensFlareWithOcclusion;
		_ = 1f / (float)(maxLensFlareWithOcclusionTemporalSample + mergeNeeded);
		_ = 0.5f / (float)maxLensFlareWithOcclusion;
		_ = 0.5f / (float)(maxLensFlareWithOcclusionTemporalSample + mergeNeeded);
		foreach (LensFlareCompInfo datum in m_Data)
		{
			if (datum == null || datum.comp == null)
			{
				continue;
			}
			LensFlareComponentSRP comp = datum.comp;
			LensFlareDataSRP lensFlareData = comp.lensFlareData;
			if (IsLensFlareSRPHidden(cam, comp, lensFlareData) || !comp.useOcclusion || (comp.useOcclusion && comp.sampleCount == 0))
			{
				continue;
			}
			Light component = null;
			if (!comp.TryGetComponent<Light>(out component))
			{
				component = null;
			}
			bool flag = false;
			Vector3 vector3;
			if (component != null && component.type == LightType.Directional)
			{
				vector3 = -component.transform.forward * cam.farClipPlane;
				flag = true;
			}
			else
			{
				vector3 = comp.transform.position;
			}
			Vector3 vector4 = WorldToViewport(cam, !flag, isCameraRelative, viewProjMatrix, vector3);
			if (usePanini && cam == Camera.main)
			{
				vector4 = DoPaniniProjection(vector4, actualWidth, actualHeight, cam.fieldOfView, paniniCropToFit, paniniDistance);
			}
			if (vector4.z < 0f || (!comp.allowOffScreen && (vector4.x < 0f || vector4.x > 1f || vector4.y < 0f || vector4.y > 1f)))
			{
				continue;
			}
			Vector3 rhs = vector3 - cameraPositionWS;
			if (Vector3.Dot(cam.transform.forward, rhs) < 0f)
			{
				continue;
			}
			float magnitude = rhs.magnitude;
			float time = magnitude / comp.maxAttenuationDistance;
			float time2 = magnitude / comp.maxAttenuationScale;
			float num = ((!flag && comp.distanceAttenuationCurve.length > 0) ? comp.distanceAttenuationCurve.Evaluate(time) : 1f);
			if (!flag && comp.scaleByDistanceCurve.length >= 1)
			{
				comp.scaleByDistanceCurve.Evaluate(time2);
			}
			Vector3 vector5 = ((!flag) ? (cam.transform.position - comp.transform.position).normalized : comp.transform.forward);
			Vector3 vector6 = WorldToViewport(cam, !flag, isCameraRelative, viewProjMatrix, vector3 + vector5 * comp.occlusionOffset);
			float num2 = (flag ? comp.celestialProjectedOcclusionRadius(cam) : comp.occlusionRadius);
			Vector2 vector7 = vector4;
			float magnitude2 = ((Vector2)WorldToViewport(cam, !flag, isCameraRelative, viewProjMatrix, vector3 + cam.transform.up * num2) - vector7).magnitude;
			cmd.SetGlobalVector(_FlareData1, new Vector4(magnitude2, comp.sampleCount, vector6.z, actualHeight / actualWidth));
			SetOcclusionPermutation(cmd, comp.environmentOcclusion, _FlareSunOcclusionTex, sunOcclusionTexture);
			cmd.EnableShaderKeyword("FLARE_COMPUTE_OCCLUSION");
			Vector2 screenPos = new Vector2(2f * vector4.x - 1f, 0f - (2f * vector4.y - 1f));
			if (!SystemInfo.graphicsUVStartsAtTop && flag)
			{
				screenPos.y = 0f - screenPos.y;
			}
			Vector2 vector8 = new Vector2(Mathf.Abs(screenPos.x), Mathf.Abs(screenPos.y));
			float time3 = Mathf.Max(vector8.x, vector8.y);
			float num3 = ((comp.radialScreenAttenuationCurve.length > 0) ? comp.radialScreenAttenuationCurve.Evaluate(time3) : 1f);
			if (!(comp.intensity * num3 * num <= 0f))
			{
				float globalCos = Mathf.Cos(0f);
				float globalSin = Mathf.Sin(0f);
				float position = 0f;
				float y = Mathf.Clamp01(0.999999f);
				cmd.SetGlobalVector(_FlareData3, new Vector4(comp.allowOffScreen ? 1f : (-1f), y, Mathf.Exp(Mathf.Lerp(0f, 4f, 1f)), 1f / 3f));
				Vector2 lensFlareRayOffset = GetLensFlareRayOffset(screenPos, position, globalCos, globalSin, vector2);
				Vector4 flareData = GetFlareData0(screenPos, Vector2.one, lensFlareRayOffset, vector2, 0f, position, 0f, Vector2.zero, autoRotate: false);
				cmd.SetGlobalVector(_FlareData0, flareData);
				cmd.SetGlobalVector(_FlareData2, new Vector4(screenPos.x, screenPos.y, 0f, 0f));
				Rect viewport;
				if (taaEnabled)
				{
					Rect rect = default(Rect);
					rect.x = datum.index;
					rect.y = frameIdx + mergeNeeded;
					rect.width = 1f;
					rect.height = 1f;
					viewport = rect;
				}
				else
				{
					Rect rect = default(Rect);
					rect.x = datum.index;
					rect.y = 0f;
					rect.width = 1f;
					rect.height = 1f;
					viewport = rect;
				}
				cmd.SetViewport(viewport);
				Blitter.DrawQuad(cmd, lensFlareShader, lensFlareShader.FindPass("LensFlareOcclusion"));
			}
		}
		if (taaEnabled)
		{
			CoreUtils.SetRenderTarget(cmd, occlusionRT, ClearFlag.None, 0, CubemapFace.Unknown, xrIndex);
			cmd.SetViewport(new Rect
			{
				x = m_Data.Count,
				y = 0f,
				width = maxLensFlareWithOcclusion - m_Data.Count,
				height = maxLensFlareWithOcclusionTemporalSample + mergeNeeded
			});
			cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		}
		frameIdx++;
		frameIdx %= maxLensFlareWithOcclusionTemporalSample;
		xr.StartSinglePass(cmd);
	}

	public static void ProcessLensFlareSRPElementsSingle(LensFlareDataElementSRP element, CommandBuffer cmd, Color globalColorModulation, Light light, float compIntensity, float scale, Material lensFlareShader, Vector2 screenPos, bool compAllowOffScreen, Vector2 vScreenRatio, Vector4 flareData1, bool preview, int depth)
	{
		if (element == null || !element.visible || (element.lensFlareTexture == null && element.flareType == SRPLensFlareType.Image) || element.localIntensity <= 0f || element.count <= 0 || (element.flareType == SRPLensFlareType.LensFlareDataSRP && element.lensFlareDataSRP == null))
		{
			return;
		}
		if (element.flareType == SRPLensFlareType.LensFlareDataSRP && element.lensFlareDataSRP != null)
		{
			ProcessLensFlareSRPElements(ref element.lensFlareDataSRP.elements, cmd, globalColorModulation, light, compIntensity, scale, lensFlareShader, screenPos, compAllowOffScreen, vScreenRatio, flareData1, preview, depth + 1);
			return;
		}
		Color color = globalColorModulation;
		if (light != null && element.modulateByLightColor)
		{
			if (light.useColorTemperature)
			{
				color *= light.color * Mathf.CorrelatedColorTemperatureToRGB(light.colorTemperature);
			}
			else
			{
				color *= light.color;
			}
		}
		Color color2 = color;
		float num = element.localIntensity * compIntensity;
		if (num <= 0f)
		{
			return;
		}
		Texture lensFlareTexture = element.lensFlareTexture;
		float usedAspectRatio;
		if (element.flareType == SRPLensFlareType.Image)
		{
			usedAspectRatio = (element.preserveAspectRatio ? ((float)lensFlareTexture.height / (float)lensFlareTexture.width) : 1f);
		}
		else
		{
			usedAspectRatio = 1f;
		}
		float rotation = element.rotation;
		Vector2 vector = ((!element.preserveAspectRatio) ? new Vector2(element.sizeXY.x, element.sizeXY.y) : ((!(usedAspectRatio >= 1f)) ? new Vector2(element.sizeXY.x, element.sizeXY.y * usedAspectRatio) : new Vector2(element.sizeXY.x / usedAspectRatio, element.sizeXY.y)));
		float num2 = 0.1f;
		Vector2 vector2 = new Vector2(vector.x, vector.y);
		float combinedScale = num2 * element.uniformScale * scale;
		vector2 *= combinedScale;
		color2 *= element.tint;
		float num3 = (SystemInfo.graphicsUVStartsAtTop ? element.angularOffset : (0f - element.angularOffset));
		float globalCos0 = Mathf.Cos((0f - num3) * (MathF.PI / 180f));
		float globalSin0 = Mathf.Sin((0f - num3) * (MathF.PI / 180f));
		float position = 2f * element.position;
		int shaderPass = element.blendMode switch
		{
			SRPLensFlareBlendMode.Additive => lensFlareShader.FindPass("LensFlareAdditive"), 
			SRPLensFlareBlendMode.Screen => lensFlareShader.FindPass("LensFlareScreen"), 
			SRPLensFlareBlendMode.Premultiply => lensFlareShader.FindPass("LensFlarePremultiply"), 
			SRPLensFlareBlendMode.Lerp => lensFlareShader.FindPass("LensFlareLerp"), 
			_ => lensFlareShader.FindPass("LensFlareOcclusion"), 
		};
		flareData1.x = (float)element.flareType;
		if (ForceSingleElement(element))
		{
			cmd.SetGlobalVector(_FlareData1, flareData1);
		}
		if (element.flareType == SRPLensFlareType.Circle || element.flareType == SRPLensFlareType.Polygon || element.flareType == SRPLensFlareType.Ring)
		{
			if (element.inverseSDF)
			{
				cmd.EnableShaderKeyword("FLARE_INVERSE_SDF");
			}
			else
			{
				cmd.DisableShaderKeyword("FLARE_INVERSE_SDF");
			}
		}
		else
		{
			cmd.DisableShaderKeyword("FLARE_INVERSE_SDF");
		}
		if (element.lensFlareTexture != null)
		{
			cmd.SetGlobalTexture(_FlareTex, element.lensFlareTexture);
		}
		if (element.tintColorType != 0)
		{
			cmd.SetGlobalTexture(_FlareRadialTint, element.tintGradient.GetTexture());
		}
		float num4 = Mathf.Clamp01(1f - element.edgeOffset - 1E-06f);
		if (element.flareType == SRPLensFlareType.Polygon)
		{
			num4 = Mathf.Pow(num4 + 1f, 5f);
		}
		float sdfRoundness = element.sdfRoundness;
		Vector4 value = new Vector4(compAllowOffScreen ? 1f : (-1f), num4, Mathf.Exp(Mathf.Lerp(0f, 4f, Mathf.Clamp01(1f - element.fallOff))), (element.flareType == SRPLensFlareType.Ring) ? element.ringThickness : (1f / (float)element.sideCount));
		cmd.SetGlobalVector(_FlareData3, value);
		if (element.flareType == SRPLensFlareType.Polygon)
		{
			float num5 = 1f / (float)element.sideCount;
			float num6 = Mathf.Cos(MathF.PI * num5);
			float num7 = num6 * sdfRoundness;
			float num8 = num6 - num7;
			float num9 = MathF.PI * 2f * num5;
			float w = num8 * Mathf.Tan(0.5f * num9);
			cmd.SetGlobalVector(_FlareData4, new Vector4(sdfRoundness, num8, num9, w));
		}
		else if (element.flareType == SRPLensFlareType.Ring)
		{
			cmd.SetGlobalVector(_FlareData4, new Vector4(element.noiseAmplitude, element.noiseFrequency, element.noiseSpeed, 0f));
		}
		else
		{
			cmd.SetGlobalVector(_FlareData4, new Vector4(sdfRoundness, 0f, 0f, 0f));
		}
		cmd.SetGlobalVector(_FlareData5, new Vector4((float)element.tintColorType, num, element.shapeCutOffSpeed, element.shapeCutOffRadius));
		if (ForceSingleElement(element))
		{
			Vector2 curSize2 = vector2;
			Vector2 lensFlareRayOffset = GetLensFlareRayOffset(screenPos, position, globalCos0, globalSin0, vScreenRatio);
			if (element.enableRadialDistortion)
			{
				Vector2 lensFlareRayOffset2 = GetLensFlareRayOffset(screenPos, 0f, globalCos0, globalSin0, vScreenRatio);
				curSize2 = ComputeLocalSize(lensFlareRayOffset, lensFlareRayOffset2, curSize2, element.distortionCurve);
			}
			Vector4 flareData2 = GetFlareData0(screenPos, element.translationScale, lensFlareRayOffset, vScreenRatio, rotation, position, num3, element.positionOffset, element.autoRotate);
			cmd.SetGlobalVector(_FlareData0, flareData2);
			cmd.SetGlobalVector(_FlareData2, new Vector4(screenPos.x, screenPos.y, curSize2.x, curSize2.y));
			cmd.SetGlobalVector(_FlareColorValue, color2);
			Blitter.DrawQuad(cmd, lensFlareShader, shaderPass);
			return;
		}
		float num10 = 2f * element.lengthSpread / (float)(element.count - 1);
		if (element.distribution == SRPLensFlareDistribution.Uniform)
		{
			float num11 = 0f;
			for (int i = 0; i < element.count; i++)
			{
				Vector2 curSize3 = vector2;
				Vector2 lensFlareRayOffset3 = GetLensFlareRayOffset(screenPos, position, globalCos0, globalSin0, vScreenRatio);
				if (element.enableRadialDistortion)
				{
					Vector2 lensFlareRayOffset4 = GetLensFlareRayOffset(screenPos, 0f, globalCos0, globalSin0, vScreenRatio);
					curSize3 = ComputeLocalSize(lensFlareRayOffset3, lensFlareRayOffset4, curSize3, element.distortionCurve);
				}
				float time = ((element.count >= 2) ? ((float)i / (float)(element.count - 1)) : 0.5f);
				Color color3 = element.colorGradient.Evaluate(time);
				Vector4 flareData3 = GetFlareData0(screenPos, element.translationScale, lensFlareRayOffset3, vScreenRatio, rotation + num11, position, num3, element.positionOffset, element.autoRotate);
				cmd.SetGlobalVector(_FlareData0, flareData3);
				flareData1.y = i;
				cmd.SetGlobalVector(_FlareData1, flareData1);
				cmd.SetGlobalVector(_FlareData2, new Vector4(screenPos.x, screenPos.y, curSize3.x, curSize3.y));
				cmd.SetGlobalVector(_FlareColorValue, color2 * color3);
				Blitter.DrawQuad(cmd, lensFlareShader, shaderPass);
				position += num10;
				num11 += element.uniformAngle;
			}
		}
		else if (element.distribution == SRPLensFlareDistribution.Random)
		{
			Random.State state = Random.state;
			Random.InitState(element.seed);
			Vector2 vector3 = new Vector2(globalSin0, globalCos0);
			vector3 *= element.positionVariation.y;
			for (int j = 0; j < element.count; j++)
			{
				float num12 = RandomRange(-1f, 1f) * element.intensityVariation + 1f;
				Vector2 lensFlareRayOffset5 = GetLensFlareRayOffset(screenPos, position, globalCos0, globalSin0, vScreenRatio);
				Vector2 vector4 = vector2;
				if (element.enableRadialDistortion)
				{
					Vector2 lensFlareRayOffset6 = GetLensFlareRayOffset(screenPos, 0f, globalCos0, globalSin0, vScreenRatio);
					vector4 = ComputeLocalSize(lensFlareRayOffset5, lensFlareRayOffset6, vector4, element.distortionCurve);
				}
				vector4 += vector4 * (element.scaleVariation * RandomRange(-1f, 1f));
				Color color4 = element.colorGradient.Evaluate(RandomRange(0f, 1f));
				Vector2 positionOffset = element.positionOffset + RandomRange(-1f, 1f) * vector3;
				float angleDeg = rotation + RandomRange(-MathF.PI, MathF.PI) * element.rotationVariation;
				if (num12 > 0f)
				{
					Vector4 flareData4 = GetFlareData0(screenPos, element.translationScale, lensFlareRayOffset5, vScreenRatio, angleDeg, position, num3, positionOffset, element.autoRotate);
					cmd.SetGlobalVector(_FlareData0, flareData4);
					flareData1.y = j;
					cmd.SetGlobalVector(_FlareData1, flareData1);
					cmd.SetGlobalVector(_FlareData2, new Vector4(screenPos.x, screenPos.y, vector4.x, vector4.y));
					cmd.SetGlobalVector(_FlareColorValue, color2 * color4 * num12);
					Blitter.DrawQuad(cmd, lensFlareShader, shaderPass);
				}
				position += num10;
				position += 0.5f * num10 * RandomRange(-1f, 1f) * element.positionVariation.x;
			}
			Random.state = state;
		}
		else
		{
			if (element.distribution != SRPLensFlareDistribution.Curve)
			{
				return;
			}
			for (int k = 0; k < element.count; k++)
			{
				float time2 = ((element.count >= 2) ? ((float)k / (float)(element.count - 1)) : 0.5f);
				Color color5 = element.colorGradient.Evaluate(time2);
				float num13 = ((element.positionCurve.length > 0) ? element.positionCurve.Evaluate(time2) : 1f);
				float position2 = position + 2f * element.lengthSpread * num13;
				Vector2 lensFlareRayOffset7 = GetLensFlareRayOffset(screenPos, position2, globalCos0, globalSin0, vScreenRatio);
				Vector2 curSize4 = vector2;
				if (element.enableRadialDistortion)
				{
					Vector2 lensFlareRayOffset8 = GetLensFlareRayOffset(screenPos, 0f, globalCos0, globalSin0, vScreenRatio);
					curSize4 = ComputeLocalSize(lensFlareRayOffset7, lensFlareRayOffset8, curSize4, element.distortionCurve);
				}
				float num14 = ((element.scaleCurve.length > 0) ? element.scaleCurve.Evaluate(time2) : 1f);
				curSize4 *= num14;
				float num15 = element.uniformAngleCurve.Evaluate(time2) * (180f - 180f / (float)element.count);
				Vector4 flareData5 = GetFlareData0(screenPos, element.translationScale, lensFlareRayOffset7, vScreenRatio, rotation + num15, position2, num3, element.positionOffset, element.autoRotate);
				cmd.SetGlobalVector(_FlareData0, flareData5);
				flareData1.y = k;
				cmd.SetGlobalVector(_FlareData1, flareData1);
				cmd.SetGlobalVector(_FlareData2, new Vector4(screenPos.x, screenPos.y, curSize4.x, curSize4.y));
				cmd.SetGlobalVector(_FlareColorValue, color2 * color5);
				Blitter.DrawQuad(cmd, lensFlareShader, shaderPass);
			}
		}
		Vector2 ComputeLocalSize(Vector2 rayOff, Vector2 rayOff0, Vector2 curSize, AnimationCurve distortionCurve)
		{
			GetLensFlareRayOffset(screenPos, position, globalCos0, globalSin0, vScreenRatio);
			float time3;
			if (!element.distortionRelativeToCenter)
			{
				Vector2 vector5 = (rayOff - rayOff0) * 0.5f;
				time3 = Mathf.Clamp01(Mathf.Max(Mathf.Abs(vector5.x), Mathf.Abs(vector5.y)));
			}
			else
			{
				time3 = Mathf.Clamp01((screenPos + (rayOff + new Vector2(element.positionOffset.x, 0f - element.positionOffset.y)) * element.translationScale).magnitude);
			}
			float t = Mathf.Clamp01(distortionCurve.Evaluate(time3));
			return new Vector2(Mathf.Lerp(curSize.x, element.targetSizeDistortion.x * combinedScale / usedAspectRatio, t), Mathf.Lerp(curSize.y, element.targetSizeDistortion.y * combinedScale, t));
		}
		static float RandomRange(float min, float max)
		{
			return Random.Range(min, max);
		}
	}

	private static void ProcessLensFlareSRPElements(ref LensFlareDataElementSRP[] elements, CommandBuffer cmd, Color globalColorModulation, Light light, float compIntensity, float scale, Material lensFlareShader, Vector2 screenPos, bool compAllowOffScreen, Vector2 vScreenRatio, Vector4 flareData1, bool preview, int depth)
	{
		if (depth > 16)
		{
			Debug.LogWarning("LensFlareSRPAsset contains too deep recursive asset (> 16). Be careful to not have recursive aggregation, A contains B, B contains A, ... which will produce an infinite loop.");
			return;
		}
		LensFlareDataElementSRP[] array = elements;
		for (int i = 0; i < array.Length; i++)
		{
			ProcessLensFlareSRPElementsSingle(array[i], cmd, globalColorModulation, light, compIntensity, scale, lensFlareShader, screenPos, compAllowOffScreen, vScreenRatio, flareData1, preview, depth);
		}
	}

	[Obsolete("Use DoLensFlareDataDrivenCommon without _FlareOcclusionRemapTex.._FlareData4 parameters.")]
	public static void DoLensFlareDataDrivenCommon(Material lensFlareShader, Camera cam, Rect viewport, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, UnsafeCommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture, RenderTargetIdentifier colorBuffer, Func<Light, Camera, Vector3, float> GetLensFlareLightAttenuation, int _FlareOcclusionRemapTex, int _FlareOcclusionTex, int _FlareOcclusionIndex, int _FlareCloudOpacity, int _FlareSunOcclusionTex, int _FlareTex, int _FlareColorValue, int _FlareData0, int _FlareData1, int _FlareData2, int _FlareData3, int _FlareData4, bool debugView)
	{
		DoLensFlareDataDrivenCommon(lensFlareShader, cam, viewport, xr, xrIndex, actualWidth, actualHeight, usePanini, paniniDistance, paniniCropToFit, isCameraRelative, cameraPositionWS, viewProjMatrix, cmd, taaEnabled, hasCloudLayer, cloudOpacityTexture, sunOcclusionTexture, colorBuffer, GetLensFlareLightAttenuation, debugView);
	}

	public static void DoLensFlareDataDrivenCommon(Material lensFlareShader, Camera cam, Rect viewport, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, UnsafeCommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture, RenderTargetIdentifier colorBuffer, Func<Light, Camera, Vector3, float> GetLensFlareLightAttenuation, bool debugView)
	{
		DoLensFlareDataDrivenCommon(lensFlareShader, cam, viewport, xr, xrIndex, actualWidth, actualHeight, usePanini, paniniDistance, paniniCropToFit, isCameraRelative, cameraPositionWS, viewProjMatrix, cmd.m_WrappedCommandBuffer, taaEnabled, hasCloudLayer, cloudOpacityTexture, sunOcclusionTexture, colorBuffer, GetLensFlareLightAttenuation, debugView);
	}

	[Obsolete("Use DoLensFlareDataDrivenCommon without _FlareOcclusionRemapTex.._FlareData4 parameters.")]
	public static void DoLensFlareDataDrivenCommon(Material lensFlareShader, Camera cam, Rect viewport, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, CommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture, RenderTargetIdentifier colorBuffer, Func<Light, Camera, Vector3, float> GetLensFlareLightAttenuation, int _FlareOcclusionRemapTex, int _FlareOcclusionTex, int _FlareOcclusionIndex, int _FlareCloudOpacity, int _FlareSunOcclusionTex, int _FlareTex, int _FlareColorValue, int _FlareData0, int _FlareData1, int _FlareData2, int _FlareData3, int _FlareData4, bool debugView)
	{
		DoLensFlareDataDrivenCommon(lensFlareShader, cam, viewport, xr, xrIndex, actualWidth, actualHeight, usePanini, paniniDistance, paniniCropToFit, isCameraRelative, cameraPositionWS, viewProjMatrix, cmd, taaEnabled, hasCloudLayer, cloudOpacityTexture, sunOcclusionTexture, colorBuffer, GetLensFlareLightAttenuation, debugView);
	}

	public static void DoLensFlareDataDrivenCommon(Material lensFlareShader, Camera cam, Rect viewport, XRPass xr, int xrIndex, float actualWidth, float actualHeight, bool usePanini, float paniniDistance, float paniniCropToFit, bool isCameraRelative, Vector3 cameraPositionWS, Matrix4x4 viewProjMatrix, CommandBuffer cmd, bool taaEnabled, bool hasCloudLayer, Texture cloudOpacityTexture, Texture sunOcclusionTexture, RenderTargetIdentifier colorBuffer, Func<Light, Camera, Vector3, float> GetLensFlareLightAttenuation, bool debugView)
	{
		xr.StopSinglePass(cmd);
		if (Instance.IsEmpty())
		{
			return;
		}
		Vector2 vector = new Vector2(actualWidth, actualHeight);
		float x = vector.x / vector.y;
		Vector2 vScreenRatio = new Vector2(x, 1f);
		if (xr.enabled && xr.singlePassEnabled)
		{
			CoreUtils.SetRenderTarget(cmd, colorBuffer, ClearFlag.None, 0, CubemapFace.Unknown, xrIndex);
			cmd.SetGlobalInt(_ViewId, xrIndex);
		}
		else
		{
			CoreUtils.SetRenderTarget(cmd, colorBuffer);
			if (xr.enabled)
			{
				cmd.SetGlobalInt(_ViewId, xr.multipassId);
			}
			else
			{
				cmd.SetGlobalInt(_ViewId, 0);
			}
		}
		cmd.SetViewport(viewport);
		if (debugView)
		{
			cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		}
		foreach (LensFlareCompInfo datum in m_Data)
		{
			if (datum == null || datum.comp == null)
			{
				continue;
			}
			LensFlareComponentSRP comp = datum.comp;
			LensFlareDataSRP lensFlareData = comp.lensFlareData;
			if (IsLensFlareSRPHidden(cam, comp, lensFlareData))
			{
				continue;
			}
			Light component = null;
			if (!comp.TryGetComponent<Light>(out component))
			{
				component = null;
			}
			bool flag = false;
			Vector3 vector2;
			if (component != null && component.type == LightType.Directional)
			{
				vector2 = -component.transform.forward * cam.farClipPlane;
				flag = true;
			}
			else
			{
				vector2 = comp.transform.position;
			}
			if (comp.lightOverride != null)
			{
				component = comp.lightOverride;
			}
			Vector3 vector3 = WorldToViewport(cam, !flag, isCameraRelative, viewProjMatrix, vector2);
			if (usePanini && cam == Camera.main)
			{
				vector3 = DoPaniniProjection(vector3, actualWidth, actualHeight, cam.fieldOfView, paniniCropToFit, paniniDistance);
			}
			if (vector3.z < 0f || (!comp.allowOffScreen && (vector3.x < 0f || vector3.x > 1f || vector3.y < 0f || vector3.y > 1f)))
			{
				continue;
			}
			Vector3 rhs = vector2 - cameraPositionWS;
			if (Vector3.Dot(cam.transform.forward, rhs) < 0f)
			{
				continue;
			}
			float magnitude = rhs.magnitude;
			float time = magnitude / comp.maxAttenuationDistance;
			float time2 = magnitude / comp.maxAttenuationScale;
			float num = ((!flag && comp.distanceAttenuationCurve.length > 0) ? comp.distanceAttenuationCurve.Evaluate(time) : 1f);
			float num2 = ((!flag && comp.scaleByDistanceCurve.length >= 1) ? comp.scaleByDistanceCurve.Evaluate(time2) : 1f);
			Color white = Color.white;
			if (component != null && comp.attenuationByLightShape)
			{
				white *= GetLensFlareLightAttenuation(component, cam, -rhs.normalized);
			}
			Vector2 screenPos = new Vector2(2f * vector3.x - 1f, 0f - (2f * vector3.y - 1f));
			if (!SystemInfo.graphicsUVStartsAtTop && flag)
			{
				screenPos.y = 0f - screenPos.y;
			}
			Vector2 vector4 = new Vector2(Mathf.Abs(screenPos.x), Mathf.Abs(screenPos.y));
			float time3 = Mathf.Max(vector4.x, vector4.y);
			float num3 = ((comp.radialScreenAttenuationCurve.length > 0) ? comp.radialScreenAttenuationCurve.Evaluate(time3) : 1f);
			float num4 = comp.intensity * num3 * num;
			if (!(num4 <= 0f))
			{
				white *= num;
				Vector3 normalized = (cam.transform.position - comp.transform.position).normalized;
				Vector3 vector5 = WorldToViewport(cam, !flag, isCameraRelative, viewProjMatrix, vector2 + normalized * comp.occlusionOffset);
				float num5 = (flag ? comp.celestialProjectedOcclusionRadius(cam) : comp.occlusionRadius);
				Vector2 vector6 = vector3;
				_ = ((Vector2)WorldToViewport(cam, !flag, isCameraRelative, viewProjMatrix, vector2 + cam.transform.up * num5) - vector6).magnitude;
				if (comp.useOcclusion && occlusionRT != null)
				{
					cmd.SetGlobalTexture(_FlareOcclusionTex, occlusionRT);
					cmd.EnableShaderKeyword("FLARE_HAS_OCCLUSION");
				}
				else
				{
					cmd.DisableShaderKeyword("FLARE_HAS_OCCLUSION");
				}
				if (IsOcclusionRTCompatible())
				{
					cmd.DisableShaderKeyword("FLARE_OPENGL3_OR_OPENGLCORE");
				}
				else
				{
					cmd.EnableShaderKeyword("FLARE_OPENGL3_OR_OPENGLCORE");
				}
				cmd.SetGlobalVector(_FlareOcclusionIndex, new Vector4(datum.index, 0f, 0f, 0f));
				cmd.SetGlobalTexture(_FlareOcclusionRemapTex, comp.occlusionRemapCurve.GetTexture());
				ProcessLensFlareSRPElements(flareData1: new Vector4(0f, comp.sampleCount, vector5.z, actualHeight / actualWidth), elements: ref lensFlareData.elements, cmd: cmd, globalColorModulation: white, light: component, compIntensity: num4, scale: num2 * comp.scale, lensFlareShader: lensFlareShader, screenPos: screenPos, compAllowOffScreen: comp.allowOffScreen, vScreenRatio: vScreenRatio, preview: false, depth: 0);
			}
		}
		xr.StartSinglePass(cmd);
	}

	public static void DoLensFlareScreenSpaceCommon(Material lensFlareShader, Camera cam, float actualWidth, float actualHeight, Color tintColor, Texture originalBloomTexture, Texture bloomMipTexture, Texture spectralLut, Texture streakTextureTmp, Texture streakTextureTmp2, Vector4 parameters1, Vector4 parameters2, Vector4 parameters3, Vector4 parameters4, Vector4 parameters5, UnsafeCommandBuffer cmd, RTHandle result, bool debugView)
	{
		DoLensFlareScreenSpaceCommon(lensFlareShader, cam, actualWidth, actualHeight, tintColor, originalBloomTexture, bloomMipTexture, spectralLut, streakTextureTmp, streakTextureTmp2, parameters1, parameters2, parameters3, parameters4, parameters5, cmd.m_WrappedCommandBuffer, result, debugView);
	}

	[Obsolete("Use DoLensFlareScreenSpaceCommon without _Shader IDs parameters.")]
	public static void DoLensFlareScreenSpaceCommon(Material lensFlareShader, Camera cam, float actualWidth, float actualHeight, Color tintColor, Texture originalBloomTexture, Texture bloomMipTexture, Texture spectralLut, Texture streakTextureTmp, Texture streakTextureTmp2, Vector4 parameters1, Vector4 parameters2, Vector4 parameters3, Vector4 parameters4, Vector4 parameters5, CommandBuffer cmd, RTHandle result, int _LensFlareScreenSpaceBloomMipTexture, int _LensFlareScreenSpaceResultTexture, int _LensFlareScreenSpaceSpectralLut, int _LensFlareScreenSpaceStreakTex, int _LensFlareScreenSpaceMipLevel, int _LensFlareScreenSpaceTintColor, int _LensFlareScreenSpaceParams1, int _LensFlareScreenSpaceParams2, int _LensFlareScreenSpaceParams3, int _LensFlareScreenSpaceParams4, int _LensFlareScreenSpaceParams5, bool debugView)
	{
		DoLensFlareScreenSpaceCommon(lensFlareShader, cam, actualWidth, actualHeight, tintColor, originalBloomTexture, bloomMipTexture, spectralLut, streakTextureTmp, streakTextureTmp2, parameters1, parameters2, parameters3, parameters4, parameters5, cmd, result, debugView);
	}

	public static void DoLensFlareScreenSpaceCommon(Material lensFlareShader, Camera cam, float actualWidth, float actualHeight, Color tintColor, Texture originalBloomTexture, Texture bloomMipTexture, Texture spectralLut, Texture streakTextureTmp, Texture streakTextureTmp2, Vector4 parameters1, Vector4 parameters2, Vector4 parameters3, Vector4 parameters4, Vector4 parameters5, CommandBuffer cmd, RTHandle result, bool debugView)
	{
		parameters2.x = Mathf.Pow(parameters2.x, 0.25f);
		parameters3.z /= 20f;
		parameters4.y *= 10f;
		parameters4.z /= 90f;
		parameters5.y = 1f / parameters5.y;
		parameters5.z = 1f / parameters5.z;
		cmd.SetViewport(new Rect
		{
			width = actualWidth,
			height = actualHeight
		});
		if (debugView)
		{
			cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
		}
		float y = parameters5.y;
		y *= actualWidth / actualHeight;
		parameters5.y = y;
		float y2 = parameters4.y;
		y2 *= actualWidth * 0.0005f;
		parameters4.y = y2;
		int shaderPass = lensFlareShader.FindPass("LensFlareScreenSpac Prefilter");
		int shaderPass2 = lensFlareShader.FindPass("LensFlareScreenSpace Downsample");
		int shaderPass3 = lensFlareShader.FindPass("LensFlareScreenSpace Upsample");
		int shaderPass4 = lensFlareShader.FindPass("LensFlareScreenSpace Composition");
		int shaderPass5 = lensFlareShader.FindPass("LensFlareScreenSpace Write to BloomTexture");
		cmd.SetGlobalTexture(_LensFlareScreenSpaceBloomMipTexture, bloomMipTexture);
		cmd.SetGlobalTexture(_LensFlareScreenSpaceSpectralLut, spectralLut);
		cmd.SetGlobalVector(_LensFlareScreenSpaceParams1, parameters1);
		cmd.SetGlobalVector(_LensFlareScreenSpaceParams2, parameters2);
		cmd.SetGlobalVector(_LensFlareScreenSpaceParams3, parameters3);
		cmd.SetGlobalVector(_LensFlareScreenSpaceParams4, parameters4);
		cmd.SetGlobalVector(_LensFlareScreenSpaceParams5, parameters5);
		cmd.SetGlobalColor(_LensFlareScreenSpaceTintColor, tintColor);
		if (parameters4.x > 0f)
		{
			CoreUtils.SetRenderTarget(cmd, streakTextureTmp);
			Blitter.DrawQuad(cmd, lensFlareShader, shaderPass);
			int b = Mathf.FloorToInt(Mathf.Log(Mathf.Max(actualHeight, actualWidth), 2f));
			int num = Mathf.Max(1, b);
			int num2 = 2;
			int num3 = 0;
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				flag = i % 2 == 0;
				cmd.SetGlobalInt(_LensFlareScreenSpaceMipLevel, i);
				cmd.SetGlobalTexture(_LensFlareScreenSpaceStreakTex, flag ? streakTextureTmp : streakTextureTmp2);
				CoreUtils.SetRenderTarget(cmd, flag ? streakTextureTmp2 : streakTextureTmp);
				Blitter.DrawQuad(cmd, lensFlareShader, shaderPass2);
			}
			if (flag)
			{
				num3 = 1;
			}
			for (int j = num3; j < num3 + num2; j++)
			{
				flag = j % 2 == 0;
				cmd.SetGlobalInt(_LensFlareScreenSpaceMipLevel, j - num3);
				cmd.SetGlobalTexture(_LensFlareScreenSpaceStreakTex, flag ? streakTextureTmp : streakTextureTmp2);
				CoreUtils.SetRenderTarget(cmd, flag ? streakTextureTmp2 : streakTextureTmp);
				Blitter.DrawQuad(cmd, lensFlareShader, shaderPass3);
			}
			cmd.SetGlobalTexture(_LensFlareScreenSpaceStreakTex, flag ? streakTextureTmp2 : streakTextureTmp);
		}
		CoreUtils.SetRenderTarget(cmd, result);
		Blitter.DrawQuad(cmd, lensFlareShader, shaderPass4);
		cmd.SetGlobalTexture(_LensFlareScreenSpaceResultTexture, result);
		CoreUtils.SetRenderTarget(cmd, originalBloomTexture);
		Blitter.DrawQuad(cmd, lensFlareShader, shaderPass5);
	}

	private static Vector2 DoPaniniProjection(Vector2 screenPos, float actualWidth, float actualHeight, float fieldOfView, float paniniProjectionCropToFit, float paniniProjectionDistance)
	{
		Vector2 vector = CalcViewExtents(actualWidth, actualHeight, fieldOfView);
		Vector2 vector2 = CalcCropExtents(actualWidth, actualHeight, fieldOfView, paniniProjectionDistance);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value = Mathf.Min(a, b);
		float num = Mathf.Lerp(1f, Mathf.Clamp01(value), paniniProjectionCropToFit);
		Vector2 vector3 = Panini_Generic_Inv(new Vector2(2f * screenPos.x - 1f, 2f * screenPos.y - 1f) * vector, paniniProjectionDistance) / (vector * num);
		return new Vector2(0.5f * vector3.x + 0.5f, 0.5f * vector3.y + 0.5f);
	}

	private static Vector2 CalcViewExtents(float actualWidth, float actualHeight, float fieldOfView)
	{
		float num = fieldOfView * (MathF.PI / 180f);
		float num2 = actualWidth / actualHeight;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private static Vector2 CalcCropExtents(float actualWidth, float actualHeight, float fieldOfView, float d)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(actualWidth, actualHeight, fieldOfView);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}

	private static Vector2 Panini_Generic_Inv(Vector2 projPos, float d)
	{
		float num = 1f + d;
		float num2 = Mathf.Sqrt(projPos.x * projPos.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return projPos * num3 * (num / num4);
	}
}
