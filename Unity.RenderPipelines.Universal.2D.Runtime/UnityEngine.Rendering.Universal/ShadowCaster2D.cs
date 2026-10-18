using System;
using Unity.Collections;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/2D/Shadow Caster 2D")]
[MovedFrom(false, "UnityEngine.Experimental.Rendering.Universal", "com.unity.render-pipelines.universal", null)]
public class ShadowCaster2D : ShadowCasterGroup2D, ISerializationCallbackReceiver
{
	internal enum ComponentVersions
	{
		Version_Unserialized,
		Version_1,
		Version_2,
		Version_3,
		Version_4,
		Version_5
	}

	internal enum ShadowCastingSources
	{
		None,
		ShapeEditor,
		ShapeProvider
	}

	public enum ShadowCastingOptions
	{
		SelfShadow,
		CastShadow,
		CastAndSelfShadow,
		NoShadow
	}

	internal enum EdgeProcessing
	{
		None,
		Clipping
	}

	private const ComponentVersions k_CurrentComponentVersion = ComponentVersions.Version_5;

	[SerializeField]
	private ComponentVersions m_ComponentVersion;

	[SerializeField]
	private bool m_HasRenderer;

	[SerializeField]
	private bool m_UseRendererSilhouette = true;

	[SerializeField]
	private bool m_CastsShadows = true;

	[SerializeField]
	private bool m_SelfShadows;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_AlphaCutoff = 0.1f;

	[SerializeField]
	private int[] m_ApplyToSortingLayers;

	[SerializeField]
	private Vector3[] m_ShapePath;

	[SerializeField]
	private int m_ShapePathHash;

	[SerializeField]
	private int m_InstanceId;

	[SerializeField]
	private Component m_ShadowShape2DComponent;

	[SerializeReference]
	private ShadowShape2DProvider m_ShadowShape2DProvider;

	[SerializeField]
	private ShadowCastingSources m_ShadowCastingSource = (ShadowCastingSources)(-1);

	[SerializeField]
	internal ShadowMesh2D m_ShadowMesh;

	[SerializeField]
	private ShadowCastingOptions m_CastingOption = ShadowCastingOptions.CastShadow;

	[SerializeField]
	internal float m_PreviousTrimEdge;

	[SerializeField]
	internal int m_PreviousEdgeProcessing;

	[SerializeField]
	internal int m_PreviousShadowCastingSource;

	[SerializeField]
	internal Component m_PreviousShadowShape2DSource;

	internal ShadowCasterGroup2D m_ShadowCasterGroup;

	internal ShadowCasterGroup2D m_PreviousShadowCasterGroup;

	internal bool m_ForceShadowMeshRebuild;

	private int m_PreviousShadowGroup;

	private bool m_PreviousCastsShadows = true;

	private int m_PreviousPathHash;

	private int m_SpriteMaterialCount;

	internal Vector3 m_CachedPosition;

	internal Vector3 m_CachedLossyScale;

	internal Quaternion m_CachedRotation;

	internal Matrix4x4 m_CachedShadowMatrix;

	internal Matrix4x4 m_CachedInverseShadowMatrix;

	internal Matrix4x4 m_CachedLocalToWorldMatrix;

	internal EdgeProcessing edgeProcessing
	{
		get
		{
			return (EdgeProcessing)m_ShadowMesh.edgeProcessing;
		}
		set
		{
			m_ShadowMesh.edgeProcessing = (ShadowMesh2D.EdgeProcessing)value;
		}
	}

	public Mesh mesh => m_ShadowMesh.mesh;

	public BoundingSphere boundingSphere => m_ShadowMesh.boundingSphere;

	public float trimEdge
	{
		get
		{
			return m_ShadowMesh.trimEdge;
		}
		set
		{
			m_ShadowMesh.trimEdge = value;
		}
	}

	public float alphaCutoff
	{
		get
		{
			return m_AlphaCutoff;
		}
		set
		{
			m_AlphaCutoff = value;
		}
	}

	public Vector3[] shapePath => m_ShapePath;

	internal int shapePathHash
	{
		get
		{
			return m_ShapePathHash;
		}
		set
		{
			m_ShapePathHash = value;
		}
	}

	internal ShadowCastingSources shadowCastingSource
	{
		get
		{
			return m_ShadowCastingSource;
		}
		set
		{
			m_ShadowCastingSource = value;
		}
	}

	internal Component shadowShape2DComponent
	{
		get
		{
			return m_ShadowShape2DComponent;
		}
		set
		{
			m_ShadowShape2DComponent = value;
		}
	}

	internal ShadowShape2DProvider shadowShape2DProvider
	{
		get
		{
			return m_ShadowShape2DProvider;
		}
		set
		{
			m_ShadowShape2DProvider = value;
		}
	}

	internal int spriteMaterialCount => m_SpriteMaterialCount;

	public ShadowCastingOptions castingOption
	{
		get
		{
			return m_CastingOption;
		}
		set
		{
			m_CastingOption = value;
		}
	}

	[Obsolete("useRendererSilhoutte is deprecated. Use selfShadows instead")]
	public bool useRendererSilhouette
	{
		get
		{
			if (m_UseRendererSilhouette)
			{
				return m_HasRenderer;
			}
			return false;
		}
		set
		{
			m_UseRendererSilhouette = value;
		}
	}

	public bool selfShadows
	{
		get
		{
			if (castingOption != ShadowCastingOptions.CastAndSelfShadow)
			{
				return castingOption == ShadowCastingOptions.SelfShadow;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (castingOption == ShadowCastingOptions.CastShadow)
				{
					castingOption = ShadowCastingOptions.CastAndSelfShadow;
				}
				else if (castingOption == ShadowCastingOptions.NoShadow)
				{
					castingOption = ShadowCastingOptions.SelfShadow;
				}
			}
			else if (castingOption == ShadowCastingOptions.CastAndSelfShadow)
			{
				castingOption = ShadowCastingOptions.CastShadow;
			}
			else if (castingOption == ShadowCastingOptions.SelfShadow)
			{
				castingOption = ShadowCastingOptions.NoShadow;
			}
		}
	}

	public bool castsShadows
	{
		get
		{
			if (castingOption != ShadowCastingOptions.CastShadow)
			{
				return castingOption == ShadowCastingOptions.CastAndSelfShadow;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (castingOption == ShadowCastingOptions.SelfShadow)
				{
					castingOption = ShadowCastingOptions.CastAndSelfShadow;
				}
				else if (castingOption == ShadowCastingOptions.NoShadow)
				{
					castingOption = ShadowCastingOptions.CastShadow;
				}
			}
			else if (castingOption == ShadowCastingOptions.CastAndSelfShadow)
			{
				castingOption = ShadowCastingOptions.SelfShadow;
			}
			else if (castingOption == ShadowCastingOptions.CastShadow)
			{
				castingOption = ShadowCastingOptions.NoShadow;
			}
		}
	}

	internal override void CacheValues()
	{
		m_CachedPosition = base.transform.position;
		m_CachedLossyScale = base.transform.lossyScale;
		m_CachedRotation = base.transform.rotation;
		m_ShadowMesh.GetFlip(out var flipX, out var flipY);
		Vector3 s = new Vector3((!flipX) ? 1 : (-1), (!flipY) ? 1 : (-1), 1f);
		m_CachedShadowMatrix = Matrix4x4.TRS(m_CachedPosition, m_CachedRotation, s);
		m_CachedInverseShadowMatrix = m_CachedShadowMatrix.inverse;
		m_CachedLocalToWorldMatrix = base.transform.localToWorldMatrix;
	}

	private static int[] SetDefaultSortingLayers()
	{
		int num = SortingLayer.layers.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = SortingLayer.layers[i].id;
		}
		return array;
	}

	internal bool IsLit(Light2D light)
	{
		Vector3 vector = default(Vector3);
		vector.x = light.m_CachedPosition.x - boundingSphere.position.x;
		vector.y = light.m_CachedPosition.y - boundingSphere.position.y;
		vector.z = light.m_CachedPosition.z - boundingSphere.position.z;
		float num = Vector3.SqrMagnitude(vector);
		float num2 = light.boundingSphere.radius + boundingSphere.radius;
		return num <= num2 * num2;
	}

	internal bool IsShadowedLayer(int layer)
	{
		if (m_ApplyToSortingLayers == null)
		{
			return false;
		}
		return Array.IndexOf(m_ApplyToSortingLayers, layer) >= 0;
	}

	private void SetShadowShape(ShadowMesh2D shadowMesh)
	{
		m_ForceShadowMeshRebuild = false;
		if (m_ShadowCastingSource == ShadowCastingSources.ShapeEditor)
		{
			NativeArray<Vector3> vertices = new NativeArray<Vector3>(m_ShapePath, Allocator.Temp);
			NativeArray<int> indices = new NativeArray<int>(2 * m_ShapePath.Length, Allocator.Temp);
			int value = m_ShapePath.Length - 1;
			for (int i = 0; i < m_ShapePath.Length; i++)
			{
				int num = i << 1;
				indices[num] = value;
				indices[num + 1] = i;
				value = i;
			}
			shadowMesh.SetShapeWithLines(vertices, indices, allowTrimming: false);
			vertices.Dispose();
			indices.Dispose();
		}
		if (m_ShadowCastingSource == ShadowCastingSources.ShapeProvider)
		{
			ShapeProviderUtility.PersistantDataCreated(m_ShadowShape2DProvider, m_ShadowShape2DComponent, shadowMesh);
		}
	}

	private void Awake()
	{
		if (m_ShadowCastingSource < ShadowCastingSources.None)
		{
			m_ShadowCastingSource = ShadowCastingSources.ShapeEditor;
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = base.transform.position;
		if (base.transform.lossyScale.x != 0f && base.transform.lossyScale.y != 0f)
		{
			vector = new Vector3(1f / base.transform.lossyScale.x, 1f / base.transform.lossyScale.y);
			vector2 = new Vector3(vector.x * (0f - base.transform.position.x), vector.y * (0f - base.transform.position.y));
		}
		if (m_ApplyToSortingLayers == null)
		{
			m_ApplyToSortingLayers = SetDefaultSortingLayers();
		}
		Bounds bounds = new Bounds(base.transform.position, Vector3.one);
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			bounds = component.bounds;
			m_SpriteMaterialCount = component.sharedMaterials.Length;
		}
		if (m_ShapePath == null || m_ShapePath.Length == 0)
		{
			m_ShapePath = new Vector3[4]
			{
				vector2 + new Vector3(vector.x * bounds.min.x, vector.y * bounds.min.y),
				vector2 + new Vector3(vector.x * bounds.min.x, vector.y * bounds.max.y),
				vector2 + new Vector3(vector.x * bounds.max.x, vector.y * bounds.max.y),
				vector2 + new Vector3(vector.x * bounds.max.x, vector.y * bounds.min.y)
			};
		}
		if (m_ShadowMesh == null)
		{
			ShadowMesh2D shadowMesh2D = new ShadowMesh2D();
			SetShadowShape(shadowMesh2D);
			m_ShadowMesh = shadowMesh2D;
		}
	}

	protected void OnEnable()
	{
		if (m_ShadowShape2DProvider != null)
		{
			m_ShadowShape2DProvider.Enabled(m_ShadowShape2DComponent, m_ShadowMesh);
		}
		m_ShadowCasterGroup = null;
	}

	protected void OnDisable()
	{
		ShadowCasterGroup2DManager.RemoveFromShadowCasterGroup(this, m_ShadowCasterGroup);
		if (m_ShadowShape2DProvider != null)
		{
			m_ShadowShape2DProvider.Disabled(m_ShadowShape2DComponent, m_ShadowMesh);
		}
	}

	public void Update()
	{
		m_HasRenderer = TryGetComponent<Renderer>(out var _);
		bool flag = LightUtility.CheckForChange((int)m_ShadowCastingSource, ref m_PreviousShadowCastingSource);
		flag |= LightUtility.CheckForChange((int)edgeProcessing, ref m_PreviousEdgeProcessing);
		flag |= edgeProcessing != 0 && LightUtility.CheckForChange(trimEdge, ref m_PreviousTrimEdge);
		flag |= m_ForceShadowMeshRebuild;
		if (m_ShadowCastingSource == ShadowCastingSources.ShapeEditor)
		{
			if (flag | LightUtility.CheckForChange(m_ShapePathHash, ref m_PreviousPathHash))
			{
				SetShadowShape(m_ShadowMesh);
			}
		}
		else if ((flag || LightUtility.CheckForChange(m_ShadowShape2DComponent, ref m_PreviousShadowShape2DSource)) && m_ShadowShape2DComponent != null)
		{
			SetShadowShape(m_ShadowMesh);
		}
		m_PreviousShadowCasterGroup = m_ShadowCasterGroup;
		if (ShadowCasterGroup2DManager.AddToShadowCasterGroup(this, ref m_ShadowCasterGroup, ref m_Priority) && m_ShadowCasterGroup != null)
		{
			if (m_PreviousShadowCasterGroup == this)
			{
				ShadowCasterGroup2DManager.RemoveGroup(this);
			}
			ShadowCasterGroup2DManager.RemoveFromShadowCasterGroup(this, m_PreviousShadowCasterGroup);
			if (m_ShadowCasterGroup == this)
			{
				ShadowCasterGroup2DManager.AddGroup(this);
			}
		}
		if (LightUtility.CheckForChange(m_ShadowGroup, ref m_PreviousShadowGroup))
		{
			ShadowCasterGroup2DManager.RemoveGroup(this);
			ShadowCasterGroup2DManager.AddGroup(this);
		}
		if (LightUtility.CheckForChange(m_CastsShadows, ref m_PreviousCastsShadows))
		{
			ShadowCasterGroup2DManager.AddGroup(this);
		}
		if (m_ShadowMesh != null)
		{
			m_ShadowMesh.UpdateBoundingSphere(base.transform);
		}
	}

	public void OnBeforeSerialize()
	{
		m_ComponentVersion = ComponentVersions.Version_5;
	}

	public void OnAfterDeserialize()
	{
		if (m_ComponentVersion < ComponentVersions.Version_2)
		{
			if (m_SelfShadows && m_CastsShadows)
			{
				m_CastingOption = ShadowCastingOptions.CastAndSelfShadow;
			}
			else if (m_SelfShadows)
			{
				m_CastingOption = ShadowCastingOptions.SelfShadow;
			}
			else if (m_CastsShadows)
			{
				m_CastingOption = ShadowCastingOptions.CastShadow;
			}
			else
			{
				m_CastingOption = ShadowCastingOptions.NoShadow;
			}
		}
		if (m_ComponentVersion < ComponentVersions.Version_3)
		{
			m_ShadowMesh = null;
			m_ForceShadowMeshRebuild = true;
		}
	}
}
