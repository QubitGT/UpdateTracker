using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal class Light2DCullResult : ILight2DCullResult
{
	private List<Light2D> m_VisibleLights = new List<Light2D>();

	private HashSet<ShadowCasterGroup2D> m_VisibleShadows = new HashSet<ShadowCasterGroup2D>();

	public List<Light2D> visibleLights => m_VisibleLights;

	public HashSet<ShadowCasterGroup2D> visibleShadows => m_VisibleShadows;

	public bool IsSceneLit()
	{
		return Light2DManager.lights.Count > 0;
	}

	public LightStats GetLightStatsByLayer(int layerID, ref LayerBatch layer)
	{
		layer.lights.Clear();
		layer.shadowIndices.Clear();
		layer.shadowCasters.Clear();
		LightStats result = default(LightStats);
		foreach (Light2D visibleLight in visibleLights)
		{
			if (!visibleLight.IsLitLayer(layerID))
			{
				continue;
			}
			if (visibleLight.normalMapQuality != Light2D.NormalMapQuality.Disabled)
			{
				result.totalNormalMapUsage++;
			}
			if (visibleLight.volumeIntensity > 0f && visibleLight.volumetricEnabled)
			{
				result.totalVolumetricUsage++;
			}
			if (visibleLight.volumeIntensity > 0f && visibleLight.volumetricEnabled && RendererLighting.CanCastShadows(visibleLight, layerID))
			{
				result.totalVolumetricShadowUsage++;
			}
			result.blendStylesUsed |= (uint)(1 << visibleLight.blendStyleIndex);
			if (visibleLight.lightType != Light2D.LightType.Global)
			{
				result.blendStylesWithLights |= (uint)(1 << visibleLight.blendStyleIndex);
			}
			bool flag = false;
			if (RendererLighting.CanCastShadows(visibleLight, layerID))
			{
				foreach (ShadowCasterGroup2D visibleShadow in visibleShadows)
				{
					List<ShadowCaster2D> shadowCasters = visibleShadow.GetShadowCasters();
					if (shadowCasters == null)
					{
						continue;
					}
					foreach (ShadowCaster2D item in shadowCasters)
					{
						if (item.IsLit(visibleLight) && item.IsShadowedLayer(layerID))
						{
							flag = true;
							result.totalShadows++;
							if (!layer.shadowCasters.Contains(visibleShadow))
							{
								layer.shadowCasters.Add(visibleShadow);
							}
						}
					}
				}
			}
			if (flag)
			{
				result.totalShadowLights++;
				layer.shadowIndices.Add(layer.lights.Count);
			}
			result.totalLights++;
			layer.lights.Add(visibleLight);
		}
		return result;
	}

	public void SetupCulling(ref ScriptableCullingParameters cullingParameters, Camera camera)
	{
		m_VisibleLights.Clear();
		foreach (Light2D light in Light2DManager.lights)
		{
			if ((camera.cullingMask & (1 << light.gameObject.layer)) == 0)
			{
				continue;
			}
			if (light.lightType == Light2D.LightType.Global)
			{
				m_VisibleLights.Add(light);
				continue;
			}
			Vector3 position = light.boundingSphere.position;
			bool flag = false;
			for (int i = 0; i < cullingParameters.cullingPlaneCount; i++)
			{
				Plane cullingPlane = cullingParameters.GetCullingPlane(i);
				if (math.dot(position, cullingPlane.normal) + cullingPlane.distance < 0f - light.boundingSphere.radius)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_VisibleLights.Add(light);
			}
		}
		m_VisibleLights.Sort((Light2D l1, Light2D l2) => l1.lightOrder - l2.lightOrder);
		m_VisibleShadows.Clear();
		if (ShadowCasterGroup2DManager.shadowCasterGroups == null)
		{
			return;
		}
		foreach (ShadowCasterGroup2D shadowCasterGroup in ShadowCasterGroup2DManager.shadowCasterGroups)
		{
			List<ShadowCaster2D> shadowCasters = shadowCasterGroup.GetShadowCasters();
			if (shadowCasters == null)
			{
				continue;
			}
			foreach (ShadowCaster2D item in shadowCasters)
			{
				foreach (Light2D visibleLight in m_VisibleLights)
				{
					if (item.IsLit(visibleLight) && !m_VisibleShadows.Contains(shadowCasterGroup))
					{
						m_VisibleShadows.Add(shadowCasterGroup);
						break;
					}
				}
				if (m_VisibleShadows.Contains(shadowCasterGroup))
				{
					break;
				}
			}
		}
	}
}
