using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace UnityEngine.Rendering.Universal;

[ExecuteAlways]
[DisallowMultipleComponent]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal", "Unity.RenderPipelines.Universal.Runtime", null)]
[AddComponentMenu("Rendering/2D/Light 2D")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/2DLightProperties.html")]
public sealed class Light2D : Light2DBase, ISerializationCallbackReceiver
{
	public enum DeprecatedLightType
	{
		Parametric
	}

	public enum LightType
	{
		Parametric,
		Freeform,
		Sprite,
		Point,
		Global
	}

	public enum NormalMapQuality
	{
		Disabled = 2,
		Fast = 0,
		Accurate = 1
	}

	public enum OverlapOperation
	{
		Additive,
		AlphaBlend
	}

	private enum ComponentVersions
	{
		Version_Unserialized,
		Version_1,
		Version_2
	}

	private const ComponentVersions k_CurrentComponentVersion = ComponentVersions.Version_2;

	[SerializeField]
	private ComponentVersions m_ComponentVersion;

	[SerializeField]
	private LightType m_LightType = LightType.Point;

	[SerializeField]
	[FormerlySerializedAs("m_LightOperationIndex")]
	private int m_BlendStyleIndex;

	[SerializeField]
	private float m_FalloffIntensity = 0.5f;

	[ColorUsage(true)]
	[SerializeField]
	private Color m_Color = Color.white;

	[SerializeField]
	private float m_Intensity = 1f;

	[FormerlySerializedAs("m_LightVolumeOpacity")]
	[SerializeField]
	private float m_LightVolumeIntensity = 1f;

	[FormerlySerializedAs("m_LightVolumeIntensityEnabled")]
	[SerializeField]
	private bool m_LightVolumeEnabled;

	[SerializeField]
	private int[] m_ApplyToSortingLayers;

	[Reload("Textures/2D/Sparkle.png", ReloadAttribute.Package.Root)]
	[SerializeField]
	private Sprite m_LightCookieSprite;

	[FormerlySerializedAs("m_LightCookieSprite")]
	[SerializeField]
	private Sprite m_DeprecatedPointLightCookieSprite;

	[SerializeField]
	private int m_LightOrder;

	[SerializeField]
	private bool m_AlphaBlendOnOverlap;

	[SerializeField]
	private OverlapOperation m_OverlapOperation;

	[FormerlySerializedAs("m_PointLightDistance")]
	[SerializeField]
	private float m_NormalMapDistance = 3f;

	[FormerlySerializedAs("m_PointLightQuality")]
	[SerializeField]
	private NormalMapQuality m_NormalMapQuality = NormalMapQuality.Disabled;

	[SerializeField]
	private bool m_UseNormalMap;

	[FormerlySerializedAs("m_ShadowIntensityEnabled")]
	[SerializeField]
	private bool m_ShadowsEnabled = true;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ShadowIntensity = 0.75f;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ShadowSoftness = 0.3f;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ShadowSoftnessFalloffIntensity = 0.5f;

	[SerializeField]
	private bool m_ShadowVolumeIntensityEnabled;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ShadowVolumeIntensity = 0.75f;

	private Mesh m_Mesh;

	[NonSerialized]
	private LightUtility.LightMeshVertex[] m_Vertices = new LightUtility.LightMeshVertex[1];

	[NonSerialized]
	private ushort[] m_Triangles = new ushort[1];

	private int m_PreviousLightCookieSprite;

	internal Vector3 m_CachedPosition;

	private int m_BatchSlotIndex;

	internal RTHandle m_CookieSpriteTexture;

	internal TextureHandle m_CookieSpriteTextureHandle;

	[SerializeField]
	private Bounds m_LocalBounds;

	internal bool forceUpdate;

	[SerializeField]
	private float m_PointLightInnerAngle = 360f;

	[SerializeField]
	private float m_PointLightOuterAngle = 360f;

	[SerializeField]
	private float m_PointLightInnerRadius;

	[SerializeField]
	private float m_PointLightOuterRadius = 1f;

	[SerializeField]
	private int m_ShapeLightParametricSides = 5;

	[SerializeField]
	private float m_ShapeLightParametricAngleOffset;

	[SerializeField]
	private float m_ShapeLightParametricRadius = 1f;

	[SerializeField]
	private float m_ShapeLightFalloffSize = 0.5f;

	[SerializeField]
	private Vector2 m_ShapeLightFalloffOffset = Vector2.zero;

	[SerializeField]
	private Vector3[] m_ShapePath;

	private float m_PreviousShapeLightFalloffSize = -1f;

	private int m_PreviousShapeLightParametricSides = -1;

	private float m_PreviousShapeLightParametricAngleOffset = -1f;

	private float m_PreviousShapeLightParametricRadius = -1f;

	private int m_PreviousShapePathHash = -1;

	private LightType m_PreviousLightType;

	internal LightUtility.LightMeshVertex[] vertices
	{
		get
		{
			return m_Vertices;
		}
		set
		{
			m_Vertices = value;
		}
	}

	internal ushort[] indices
	{
		get
		{
			return m_Triangles;
		}
		set
		{
			m_Triangles = value;
		}
	}

	internal int batchSlotIndex
	{
		get
		{
			return m_BatchSlotIndex;
		}
		set
		{
			m_BatchSlotIndex = value;
		}
	}

	private int lightCookieSpriteInstanceID => lightCookieSprite?.GetInstanceID() ?? 0;

	internal bool useCookieSprite
	{
		get
		{
			if (lightType == LightType.Point || lightType == LightType.Sprite)
			{
				if (lightCookieSprite != null)
				{
					return lightCookieSprite.texture != null;
				}
				return false;
			}
			return false;
		}
	}

	internal BoundingSphere boundingSphere { get; private set; }

	internal Mesh lightMesh
	{
		get
		{
			if (null == m_Mesh)
			{
				m_Mesh = new Mesh();
			}
			return m_Mesh;
		}
	}

	internal bool hasCachedMesh
	{
		get
		{
			if (vertices.Length > 1)
			{
				return indices.Length > 1;
			}
			return false;
		}
	}

	public LightType lightType
	{
		get
		{
			return m_LightType;
		}
		set
		{
			if (m_LightType != value)
			{
				UpdateMesh();
			}
			m_LightType = value;
			Light2DManager.ErrorIfDuplicateGlobalLight(this);
		}
	}

	public int blendStyleIndex
	{
		get
		{
			return m_BlendStyleIndex;
		}
		set
		{
			m_BlendStyleIndex = value;
		}
	}

	public float shadowIntensity
	{
		get
		{
			return m_ShadowIntensity;
		}
		set
		{
			m_ShadowIntensity = Mathf.Clamp01(value);
		}
	}

	public float shadowSoftness
	{
		get
		{
			return m_ShadowSoftness;
		}
		set
		{
			m_ShadowSoftness = value;
		}
	}

	public bool shadowsEnabled
	{
		get
		{
			return m_ShadowsEnabled;
		}
		set
		{
			m_ShadowsEnabled = value;
		}
	}

	public float shadowVolumeIntensity
	{
		get
		{
			return m_ShadowVolumeIntensity;
		}
		set
		{
			m_ShadowVolumeIntensity = Mathf.Clamp01(value);
		}
	}

	public bool volumetricShadowsEnabled
	{
		get
		{
			return m_ShadowVolumeIntensityEnabled;
		}
		set
		{
			m_ShadowVolumeIntensityEnabled = value;
		}
	}

	public Color color
	{
		get
		{
			return m_Color;
		}
		set
		{
			m_Color = value;
		}
	}

	public float intensity
	{
		get
		{
			return m_Intensity;
		}
		set
		{
			m_Intensity = value;
		}
	}

	[Obsolete]
	public float volumeOpacity => m_LightVolumeIntensity;

	public float volumeIntensity
	{
		get
		{
			return m_LightVolumeIntensity;
		}
		set
		{
			m_LightVolumeIntensity = value;
		}
	}

	[Obsolete]
	public bool volumeIntensityEnabled
	{
		get
		{
			return m_LightVolumeEnabled;
		}
		set
		{
			m_LightVolumeEnabled = value;
		}
	}

	public bool volumetricEnabled
	{
		get
		{
			return m_LightVolumeEnabled;
		}
		set
		{
			m_LightVolumeEnabled = value;
		}
	}

	public Sprite lightCookieSprite
	{
		get
		{
			if (m_LightType == LightType.Point)
			{
				return m_DeprecatedPointLightCookieSprite;
			}
			return m_LightCookieSprite;
		}
		set
		{
			m_LightCookieSprite = value;
		}
	}

	public float falloffIntensity
	{
		get
		{
			return m_FalloffIntensity;
		}
		set
		{
			m_FalloffIntensity = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float shadowSoftnessFalloffIntensity
	{
		get
		{
			return m_ShadowSoftnessFalloffIntensity;
		}
		set
		{
			m_ShadowSoftnessFalloffIntensity = Mathf.Clamp(value, 0f, 1f);
		}
	}

	[Obsolete]
	public bool alphaBlendOnOverlap => m_OverlapOperation == OverlapOperation.AlphaBlend;

	public OverlapOperation overlapOperation
	{
		get
		{
			return m_OverlapOperation;
		}
		set
		{
			m_OverlapOperation = value;
		}
	}

	public int lightOrder
	{
		get
		{
			return m_LightOrder;
		}
		set
		{
			m_LightOrder = value;
		}
	}

	public float normalMapDistance => m_NormalMapDistance;

	public NormalMapQuality normalMapQuality => m_NormalMapQuality;

	public bool renderVolumetricShadows
	{
		get
		{
			if (volumetricShadowsEnabled)
			{
				return shadowVolumeIntensity > 0f;
			}
			return false;
		}
	}

	public int[] targetSortingLayers
	{
		get
		{
			return m_ApplyToSortingLayers;
		}
		set
		{
			List<int> list = new List<int>();
			foreach (int num in value)
			{
				if (SortingLayer.IsValid(num))
				{
					list.Add(num);
				}
			}
			m_ApplyToSortingLayers = list.ToArray();
		}
	}

	public float pointLightInnerAngle
	{
		get
		{
			return m_PointLightInnerAngle;
		}
		set
		{
			m_PointLightInnerAngle = value;
		}
	}

	public float pointLightOuterAngle
	{
		get
		{
			return m_PointLightOuterAngle;
		}
		set
		{
			m_PointLightOuterAngle = value;
		}
	}

	public float pointLightInnerRadius
	{
		get
		{
			return m_PointLightInnerRadius;
		}
		set
		{
			m_PointLightInnerRadius = value;
		}
	}

	public float pointLightOuterRadius
	{
		get
		{
			return m_PointLightOuterRadius;
		}
		set
		{
			m_PointLightOuterRadius = value;
		}
	}

	[Obsolete("pointLightDistance has been changed to normalMapDistance", true)]
	public float pointLightDistance => m_NormalMapDistance;

	[Obsolete("pointLightQuality has been changed to normalMapQuality", true)]
	public NormalMapQuality pointLightQuality => m_NormalMapQuality;

	internal bool isPointLight => m_LightType == LightType.Point;

	public int shapeLightParametricSides => m_ShapeLightParametricSides;

	public float shapeLightParametricAngleOffset => m_ShapeLightParametricAngleOffset;

	public float shapeLightParametricRadius
	{
		get
		{
			return m_ShapeLightParametricRadius;
		}
		internal set
		{
			m_ShapeLightParametricRadius = value;
		}
	}

	public float shapeLightFalloffSize
	{
		get
		{
			return m_ShapeLightFalloffSize;
		}
		set
		{
			m_ShapeLightFalloffSize = Mathf.Max(0f, value);
		}
	}

	public Vector3[] shapePath
	{
		get
		{
			return m_ShapePath;
		}
		internal set
		{
			m_ShapePath = value;
		}
	}

	private bool IsValidLayer(string name)
	{
		SortingLayer[] cachedSortingLayer = Light2DManager.GetCachedSortingLayer();
		foreach (SortingLayer sortingLayer in cachedSortingLayer)
		{
			if (sortingLayer.name == name)
			{
				return true;
			}
		}
		return false;
	}

	public bool AddTargetSortingLayer(string layerName)
	{
		List<int> list = new List<int>(m_ApplyToSortingLayers);
		int item = SortingLayer.NameToID(layerName);
		if (!IsValidLayer(layerName) || list.Contains(item))
		{
			return false;
		}
		list.Add(item);
		m_ApplyToSortingLayers = list.ToArray();
		return true;
	}

	public bool AddTargetSortingLayer(int layerID)
	{
		return AddTargetSortingLayer(SortingLayer.IDToName(layerID));
	}

	public bool RemoveTargetSortingLayer(string layerName)
	{
		List<int> list = new List<int>(m_ApplyToSortingLayers);
		int item = SortingLayer.NameToID(layerName);
		if (!IsValidLayer(layerName) || !list.Contains(item))
		{
			return false;
		}
		list.Remove(item);
		m_ApplyToSortingLayers = list.ToArray();
		return true;
	}

	public bool RemoveTargetSortingLayer(int layerID)
	{
		return RemoveTargetSortingLayer(SortingLayer.IDToName(layerID));
	}

	internal void MarkForUpdate()
	{
		forceUpdate = true;
	}

	internal void CacheValues()
	{
		m_CachedPosition = base.transform.position;
	}

	internal int GetTopMostLitLayer()
	{
		int result = int.MinValue;
		int num = 0;
		SortingLayer[] cachedSortingLayer = Light2DManager.GetCachedSortingLayer();
		for (int i = 0; i < m_ApplyToSortingLayers.Length; i++)
		{
			for (int num2 = cachedSortingLayer.Length - 1; num2 >= num; num2--)
			{
				if (cachedSortingLayer[num2].id == m_ApplyToSortingLayers[i])
				{
					result = cachedSortingLayer[num2].value;
					num = num2;
				}
			}
		}
		return result;
	}

	internal Bounds UpdateSpriteMesh()
	{
		if (m_LightCookieSprite == null && (m_Vertices.Length != 1 || m_Triangles.Length != 1))
		{
			m_Vertices = new LightUtility.LightMeshVertex[1];
			m_Triangles = new ushort[1];
		}
		return LightUtility.GenerateSpriteMesh(this, m_LightCookieSprite, LightBatch.GetBatchColor());
	}

	internal void UpdateBatchSlotIndex()
	{
		if ((bool)lightMesh && lightMesh.colors != null && lightMesh.colors.Length != 0)
		{
			m_BatchSlotIndex = LightBatch.GetBatchSlotIndex(lightMesh.colors[0].b);
		}
	}

	internal bool NeedsColorIndexBaking()
	{
		if ((bool)lightMesh && LightBatch.isBatchingSupported && lightMesh.colors.Length != 0)
		{
			return lightMesh.colors[0].b == 0f;
		}
		return false;
	}

	internal void UpdateCookieSpriteTexture()
	{
		m_CookieSpriteTexture?.Release();
		if (useCookieSprite)
		{
			m_CookieSpriteTexture = RTHandles.Alloc(lightCookieSprite.texture);
		}
	}

	internal void UpdateMesh(bool forceUpdate = false)
	{
		int shapePathHash = LightUtility.GetShapePathHash(shapePath);
		bool flag = LightUtility.CheckForChange(m_ShapeLightFalloffSize, ref m_PreviousShapeLightFalloffSize);
		bool flag2 = LightUtility.CheckForChange(m_ShapeLightParametricRadius, ref m_PreviousShapeLightParametricRadius);
		bool flag3 = LightUtility.CheckForChange(m_ShapeLightParametricSides, ref m_PreviousShapeLightParametricSides);
		bool flag4 = LightUtility.CheckForChange(m_ShapeLightParametricAngleOffset, ref m_PreviousShapeLightParametricAngleOffset);
		bool flag5 = LightUtility.CheckForChange(lightCookieSpriteInstanceID, ref m_PreviousLightCookieSprite);
		bool flag6 = LightUtility.CheckForChange(shapePathHash, ref m_PreviousShapePathHash);
		bool flag7 = LightUtility.CheckForChange(m_LightType, ref m_PreviousLightType);
		if (flag || flag2 || flag3 || flag4 || flag5 || flag6 || flag7 || NeedsColorIndexBaking() || forceUpdate)
		{
			float batchColor = LightBatch.GetBatchColor();
			switch (m_LightType)
			{
			case LightType.Freeform:
				m_LocalBounds = LightUtility.GenerateShapeMesh(this, m_ShapePath, m_ShapeLightFalloffSize, batchColor);
				break;
			case LightType.Parametric:
				m_LocalBounds = LightUtility.GenerateParametricMesh(this, m_ShapeLightParametricRadius, m_ShapeLightFalloffSize, m_ShapeLightParametricAngleOffset, m_ShapeLightParametricSides, batchColor);
				break;
			case LightType.Sprite:
				m_LocalBounds = UpdateSpriteMesh();
				break;
			case LightType.Point:
				m_LocalBounds = LightUtility.GenerateParametricMesh(this, 1.412135f, 0f, 0f, 4, batchColor);
				break;
			}
			UpdateCookieSpriteTexture();
			UpdateBatchSlotIndex();
		}
	}

	internal void UpdateBoundingSphere()
	{
		if (isPointLight)
		{
			boundingSphere = new BoundingSphere(base.transform.position, m_PointLightOuterRadius);
			return;
		}
		Vector3 vector = base.transform.TransformPoint(Vector3.Max(m_LocalBounds.max, m_LocalBounds.max + (Vector3)m_ShapeLightFalloffOffset));
		Vector3 vector2 = base.transform.TransformPoint(Vector3.Min(m_LocalBounds.min, m_LocalBounds.min + (Vector3)m_ShapeLightFalloffOffset));
		Vector3 vector3 = 0.5f * (vector + vector2);
		float rad = Vector3.Magnitude(vector - vector3);
		boundingSphere = new BoundingSphere(vector3, rad);
	}

	internal bool IsLitLayer(int layer)
	{
		if (m_ApplyToSortingLayers == null)
		{
			return false;
		}
		for (int i = 0; i < m_ApplyToSortingLayers.Length; i++)
		{
			if (m_ApplyToSortingLayers[i] == layer)
			{
				return true;
			}
		}
		return false;
	}

	internal Matrix4x4 GetMatrix()
	{
		Matrix4x4 result = base.transform.localToWorldMatrix;
		if (lightType == LightType.Point)
		{
			result = Matrix4x4.TRS(s: new Vector3(pointLightOuterRadius, pointLightOuterRadius, pointLightOuterRadius), pos: base.transform.position, q: base.transform.rotation);
		}
		return result;
	}

	private void Awake()
	{
		if (m_ApplyToSortingLayers == null)
		{
			m_ApplyToSortingLayers = new int[SortingLayer.layers.Length];
			for (int i = 0; i < m_ApplyToSortingLayers.Length; i++)
			{
				m_ApplyToSortingLayers[i] = SortingLayer.layers[i].id;
			}
		}
	}

	private void OnEnable()
	{
		m_PreviousLightCookieSprite = lightCookieSpriteInstanceID;
		Light2DManager.RegisterLight(this);
		UpdateCookieSpriteTexture();
	}

	private void OnDisable()
	{
		Light2DManager.DeregisterLight(this);
		m_CookieSpriteTexture?.Release();
	}

	private void LateUpdate()
	{
		if (m_LightType != LightType.Global)
		{
			UpdateMesh(forceUpdate);
			UpdateBoundingSphere();
			forceUpdate = false;
		}
	}

	public void OnBeforeSerialize()
	{
		m_ComponentVersion = ComponentVersions.Version_2;
	}

	public void OnAfterDeserialize()
	{
		if (m_ComponentVersion == ComponentVersions.Version_Unserialized)
		{
			m_ShadowVolumeIntensityEnabled = m_ShadowVolumeIntensity > 0f;
			m_ShadowsEnabled = m_ShadowIntensity > 0f;
			m_LightVolumeEnabled = m_LightVolumeIntensity > 0f;
			m_NormalMapQuality = ((!m_UseNormalMap) ? NormalMapQuality.Disabled : m_NormalMapQuality);
			m_OverlapOperation = (m_AlphaBlendOnOverlap ? OverlapOperation.AlphaBlend : m_OverlapOperation);
			m_ComponentVersion = ComponentVersions.Version_1;
		}
		if (m_ComponentVersion < ComponentVersions.Version_2)
		{
			m_ShadowSoftness = 0f;
		}
	}

	public void SetShapePath(Vector3[] path)
	{
		m_ShapePath = path;
	}
}
