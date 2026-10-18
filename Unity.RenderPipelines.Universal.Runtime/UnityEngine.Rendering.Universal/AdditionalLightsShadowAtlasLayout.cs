using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

internal struct AdditionalLightsShadowAtlasLayout
{
	internal struct ShadowResolutionRequest
	{
		[Flags]
		private enum SettingsOptions : ushort
		{
			None = 0,
			SoftShadow = 1,
			PointLightShadow = 2,
			All = ushort.MaxValue
		}

		public ushort visibleLightIndex;

		public ushort perLightShadowSliceIndex;

		public ushort requestedResolution;

		public ushort offsetX;

		public ushort offsetY;

		public ushort allocatedResolution;

		private SettingsOptions m_ShadowProperties;

		public bool softShadow
		{
			get
			{
				return m_ShadowProperties.HasFlag(SettingsOptions.SoftShadow);
			}
			set
			{
				if (value)
				{
					m_ShadowProperties |= SettingsOptions.SoftShadow;
				}
				else
				{
					m_ShadowProperties &= ~SettingsOptions.SoftShadow;
				}
			}
		}

		public bool pointLightShadow
		{
			get
			{
				return m_ShadowProperties.HasFlag(SettingsOptions.PointLightShadow);
			}
			set
			{
				if (value)
				{
					m_ShadowProperties |= SettingsOptions.PointLightShadow;
				}
				else
				{
					m_ShadowProperties &= ~SettingsOptions.PointLightShadow;
				}
			}
		}
	}

	private static List<RectInt> s_UnusedAtlasSquareAreas;

	private static List<ShadowResolutionRequest> s_ShadowResolutionRequests;

	private static float[] s_VisibleLightIndexToCameraSquareDistance;

	private static Func<ShadowResolutionRequest, ShadowResolutionRequest, int> s_CompareShadowResolutionRequest;

	private static ShadowResolutionRequest[] s_SortedShadowResolutionRequests;

	private NativeArray<ShadowResolutionRequest> m_SortedShadowResolutionRequests;

	private NativeArray<int> m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex;

	private int m_TotalShadowSlicesCount;

	private int m_TotalShadowResolutionRequestCount;

	private bool m_TooManyShadowMaps;

	private int m_ShadowSlicesScaleFactor;

	private int m_AtlasSize;

	public AdditionalLightsShadowAtlasLayout(UniversalLightData lightData, UniversalShadowData shadowData, UniversalCameraData cameraData)
	{
		bool useStructuredBuffer = RenderingUtils.useStructuredBuffer;
		NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
		int length = visibleLights.Length;
		if (s_UnusedAtlasSquareAreas == null)
		{
			s_UnusedAtlasSquareAreas = new List<RectInt>();
		}
		if (s_ShadowResolutionRequests == null)
		{
			s_ShadowResolutionRequests = new List<ShadowResolutionRequest>();
		}
		if (s_VisibleLightIndexToCameraSquareDistance == null || s_VisibleLightIndexToCameraSquareDistance.Length < length)
		{
			s_VisibleLightIndexToCameraSquareDistance = new float[length];
		}
		if (s_CompareShadowResolutionRequest == null)
		{
			s_CompareShadowResolutionRequest = CreateCompareShadowResolutionRequesPredicate();
		}
		if (!useStructuredBuffer)
		{
			int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
			if (s_UnusedAtlasSquareAreas.Capacity < maxVisibleAdditionalLights)
			{
				s_UnusedAtlasSquareAreas.Capacity = maxVisibleAdditionalLights;
			}
			if (s_ShadowResolutionRequests.Count < length)
			{
				s_ShadowResolutionRequests.Capacity = length;
				int num = length - s_ShadowResolutionRequests.Count + 1;
				for (int i = 0; i < num; i++)
				{
					s_ShadowResolutionRequests.Add(default(ShadowResolutionRequest));
				}
			}
		}
		s_UnusedAtlasSquareAreas.Clear();
		ushort num2 = 0;
		for (int j = 0; j < visibleLights.Length; j++)
		{
			if (j == lightData.mainLightIndex)
			{
				s_VisibleLightIndexToCameraSquareDistance[j] = float.MaxValue;
				continue;
			}
			ref VisibleLight reference = ref visibleLights.UnsafeElementAt(j);
			Light light = reference.light;
			LightType lightType = reference.lightType;
			LightShadows shadows = light.shadows;
			float shadowStrength = light.shadowStrength;
			if (!ShadowUtils.IsValidShadowCastingLight(lightData, j, lightType, shadows, shadowStrength))
			{
				s_VisibleLightIndexToCameraSquareDistance[j] = float.MaxValue;
				continue;
			}
			bool softShadow = shadows == LightShadows.Soft;
			bool pointLightShadow = lightType == LightType.Point;
			ushort visibleLightIndex = (ushort)j;
			ushort requestedResolution = (ushort)shadowData.resolution[j];
			int punctualLightShadowSlicesCount = ShadowUtils.GetPunctualLightShadowSlicesCount(in lightType);
			for (ushort num3 = 0; num3 < punctualLightShadowSlicesCount; num3++)
			{
				if (num2 >= s_ShadowResolutionRequests.Count)
				{
					s_ShadowResolutionRequests.Add(default(ShadowResolutionRequest));
				}
				ShadowResolutionRequest value = s_ShadowResolutionRequests[num2];
				value.visibleLightIndex = visibleLightIndex;
				value.perLightShadowSliceIndex = num3;
				value.requestedResolution = requestedResolution;
				value.softShadow = softShadow;
				value.pointLightShadow = pointLightShadow;
				s_ShadowResolutionRequests[num2] = value;
				num2++;
			}
			s_VisibleLightIndexToCameraSquareDistance[j] = (cameraData.worldSpaceCameraPos - light.transform.position).sqrMagnitude;
		}
		if (s_SortedShadowResolutionRequests == null || s_SortedShadowResolutionRequests.Length < num2)
		{
			s_SortedShadowResolutionRequests = new ShadowResolutionRequest[num2];
		}
		for (int k = 0; k < num2; k++)
		{
			s_SortedShadowResolutionRequests[k] = s_ShadowResolutionRequests[k];
		}
		using (new ProfilingScope(Sorting.s_QuickSortSampler))
		{
			Sorting.QuickSort(s_SortedShadowResolutionRequests, 0, num2 - 1, s_CompareShadowResolutionRequest);
		}
		m_SortedShadowResolutionRequests = new NativeArray<ShadowResolutionRequest>(s_SortedShadowResolutionRequests, Allocator.Temp);
		int num4 = (useStructuredBuffer ? num2 : Math.Min(num2, UniversalRenderPipeline.maxVisibleAdditionalLights));
		int additionalLightsShadowmapWidth = shadowData.additionalLightsShadowmapWidth;
		bool flag = false;
		int num5 = 1;
		while (!flag && num4 > 0)
		{
			ShadowResolutionRequest shadowResolutionRequest = m_SortedShadowResolutionRequests[num4 - 1];
			num5 = EstimateScaleFactorNeededToFitAllShadowsInAtlas(in m_SortedShadowResolutionRequests, num4, additionalLightsShadowmapWidth);
			if (shadowResolutionRequest.requestedResolution >= num5 * ShadowUtils.MinimalPunctualLightShadowResolution(shadowResolutionRequest.softShadow))
			{
				flag = true;
				continue;
			}
			int num6 = num4;
			LightType lightType2 = (shadowResolutionRequest.pointLightShadow ? LightType.Point : LightType.Spot);
			num4 = num6 - ShadowUtils.GetPunctualLightShadowSlicesCount(in lightType2);
		}
		for (int l = num4; l < m_SortedShadowResolutionRequests.Length; l++)
		{
			m_SortedShadowResolutionRequests[l] = default(ShadowResolutionRequest);
		}
		m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex = new NativeArray<int>(visibleLights.Length, Allocator.Temp);
		for (int m = 0; m < m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex.Length; m++)
		{
			m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[m] = -1;
		}
		for (int num7 = num4 - 1; num7 >= 0; num7--)
		{
			int visibleLightIndex2 = s_SortedShadowResolutionRequests[num7].visibleLightIndex;
			m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[visibleLightIndex2] = num7;
		}
		bool flag2 = false;
		bool flag3 = false;
		int num8 = num5;
		while (!flag2 && !flag3)
		{
			s_UnusedAtlasSquareAreas.Clear();
			s_UnusedAtlasSquareAreas.Add(new RectInt(0, 0, additionalLightsShadowmapWidth, additionalLightsShadowmapWidth));
			flag2 = true;
			for (int n = 0; n < num4; n++)
			{
				int num9 = m_SortedShadowResolutionRequests[n].requestedResolution / num8;
				if (num9 < ShadowUtils.MinimalPunctualLightShadowResolution(m_SortedShadowResolutionRequests[n].softShadow))
				{
					flag3 = true;
					break;
				}
				bool flag4 = false;
				for (int num10 = 0; num10 < s_UnusedAtlasSquareAreas.Count; num10++)
				{
					RectInt rectInt = s_UnusedAtlasSquareAreas[num10];
					int width = rectInt.width;
					if (width < num9)
					{
						continue;
					}
					int height = rectInt.height;
					int x = rectInt.x;
					int y = rectInt.y;
					ref ShadowResolutionRequest reference2 = ref m_SortedShadowResolutionRequests.UnsafeElementAtMutable(n);
					reference2.offsetX = (ushort)x;
					reference2.offsetY = (ushort)y;
					reference2.allocatedResolution = (ushort)num9;
					s_UnusedAtlasSquareAreas.RemoveAt(num10);
					int num11 = num4 - n - 1;
					int num12 = 0;
					int num13 = num9;
					int num14 = num9;
					int num15 = x;
					int num16 = y;
					for (; num12 < num11; num12++)
					{
						num15 += num13;
						if (num15 + num13 > x + width)
						{
							num15 = x;
							num16 += num14;
							if (num16 + num14 > y + height)
							{
								break;
							}
						}
						s_UnusedAtlasSquareAreas.Insert(num10 + num12, new RectInt(num15, num16, num13, num14));
					}
					flag4 = true;
					break;
				}
				if (!flag4)
				{
					flag2 = false;
					break;
				}
			}
			if (!flag2 && !flag3)
			{
				num8 *= 2;
			}
		}
		m_TooManyShadowMaps = flag3;
		m_ShadowSlicesScaleFactor = num8;
		m_TotalShadowSlicesCount = num4;
		m_TotalShadowResolutionRequestCount = num2;
		m_AtlasSize = additionalLightsShadowmapWidth;
	}

	public int GetTotalShadowSlicesCount()
	{
		return m_TotalShadowSlicesCount;
	}

	public int GetTotalShadowResolutionRequestCount()
	{
		return m_TotalShadowResolutionRequestCount;
	}

	public bool HasTooManyShadowMaps()
	{
		return m_TooManyShadowMaps;
	}

	public int GetShadowSlicesScaleFactor()
	{
		return m_ShadowSlicesScaleFactor;
	}

	public int GetAtlasSize()
	{
		return m_AtlasSize;
	}

	public bool HasSpaceForLight(int originalVisibleLightIndex)
	{
		return m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[originalVisibleLightIndex] != -1;
	}

	public ShadowResolutionRequest GetSortedShadowResolutionRequest(int sortedShadowResolutionRequestIndex)
	{
		return m_SortedShadowResolutionRequests[sortedShadowResolutionRequestIndex];
	}

	public ShadowResolutionRequest GetSliceShadowResolutionRequest(int originalVisibleLightIndex, int sliceIndex)
	{
		int num = m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[originalVisibleLightIndex];
		return m_SortedShadowResolutionRequests[num + sliceIndex];
	}

	public static void ClearStaticCaches()
	{
		s_UnusedAtlasSquareAreas = null;
		s_ShadowResolutionRequests = null;
		s_VisibleLightIndexToCameraSquareDistance = null;
		s_CompareShadowResolutionRequest = null;
		s_SortedShadowResolutionRequests = null;
	}

	private static int EstimateScaleFactorNeededToFitAllShadowsInAtlas(in NativeArray<ShadowResolutionRequest> shadowResolutionRequests, int endIndex, int atlasSize)
	{
		long num = atlasSize * atlasSize;
		long num2 = 0L;
		for (int i = 0; i < endIndex; i++)
		{
			num2 += shadowResolutionRequests[i].requestedResolution * shadowResolutionRequests[i].requestedResolution;
		}
		int num3 = 1;
		while (num2 > num * num3 * num3)
		{
			num3 *= 2;
		}
		return num3;
	}

	private static Func<ShadowResolutionRequest, ShadowResolutionRequest, int> CreateCompareShadowResolutionRequesPredicate()
	{
		return (ShadowResolutionRequest curr, ShadowResolutionRequest other) => (curr.requestedResolution <= other.requestedResolution && (curr.requestedResolution != other.requestedResolution || curr.softShadow || !other.softShadow) && (curr.requestedResolution != other.requestedResolution || curr.softShadow != other.softShadow || !(s_VisibleLightIndexToCameraSquareDistance[curr.visibleLightIndex] < s_VisibleLightIndexToCameraSquareDistance[other.visibleLightIndex])) && (curr.requestedResolution != other.requestedResolution || curr.softShadow != other.softShadow || s_VisibleLightIndexToCameraSquareDistance[curr.visibleLightIndex] != s_VisibleLightIndexToCameraSquareDistance[other.visibleLightIndex] || curr.visibleLightIndex >= other.visibleLightIndex) && (curr.requestedResolution != other.requestedResolution || curr.softShadow != other.softShadow || s_VisibleLightIndexToCameraSquareDistance[curr.visibleLightIndex] != s_VisibleLightIndexToCameraSquareDistance[other.visibleLightIndex] || curr.visibleLightIndex != other.visibleLightIndex || curr.perLightShadowSliceIndex >= other.perLightShadowSliceIndex)) ? 1 : (-1);
	}
}
