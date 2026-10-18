using System;

namespace UnityEngine.Rendering.Universal;

[ExecuteAlways]
[AddComponentMenu("Rendering/URP Decal Projector")]
public class DecalProjector : MonoBehaviour, ISerializationCallbackReceiver
{
	internal delegate void DecalProjectorAction(DecalProjector decalProjector);

	private enum Version
	{
		Initial,
		RenderingLayerMask,
		Count
	}

	[SerializeField]
	private Material m_Material;

	[SerializeField]
	private float m_DrawDistance = 1000f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_FadeScale = 0.9f;

	[SerializeField]
	[Range(0f, 180f)]
	private float m_StartAngleFade = 180f;

	[SerializeField]
	[Range(0f, 180f)]
	private float m_EndAngleFade = 180f;

	[SerializeField]
	private Vector2 m_UVScale = new Vector2(1f, 1f);

	[SerializeField]
	private Vector2 m_UVBias = new Vector2(0f, 0f);

	[SerializeField]
	private RenderingLayerMask m_RenderingLayerMask = RenderingLayerMask.defaultRenderingLayerMask;

	[SerializeField]
	private DecalScaleMode m_ScaleMode;

	[SerializeField]
	internal Vector3 m_Offset = new Vector3(0f, 0f, 0.5f);

	[SerializeField]
	internal Vector3 m_Size = new Vector3(1f, 1f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float m_FadeFactor = 1f;

	private Material m_OldMaterial;

	private float m_OldDrawDistance = 1000f;

	private float m_OldFadeScale = 0.9f;

	private float m_OldStartAngleFade = 180f;

	private float m_OldEndAngleFade = 180f;

	private Vector2 m_OldUVScale = new Vector2(1f, 1f);

	private Vector2 m_OldUVBias = new Vector2(0f, 0f);

	private DecalScaleMode m_OldScaleMode;

	private Vector3 m_OldOffset = new Vector3(0f, 0f, 0.5f);

	private Vector3 m_OldSize = new Vector3(1f, 1f, 1f);

	private float m_OldFadeFactor = 1f;

	[SerializeField]
	private Version version = Version.Count;

	[SerializeField]
	[Obsolete("This field is only kept for migration purpose. Use m_RenderingLayersMask instead. #from(6000.2)", false)]
	private uint m_DecalLayerMask = 1u;

	internal static Material defaultMaterial { get; set; }

	internal static bool isSupported => DecalProjector.onDecalAdd != null;

	internal DecalEntity decalEntity { get; set; }

	public Material material
	{
		get
		{
			return m_Material;
		}
		set
		{
			m_Material = value;
			OnValidate();
		}
	}

	public float drawDistance
	{
		get
		{
			return m_DrawDistance;
		}
		set
		{
			m_DrawDistance = Mathf.Max(0f, value);
			OnValidate();
		}
	}

	public float fadeScale
	{
		get
		{
			return m_FadeScale;
		}
		set
		{
			m_FadeScale = Mathf.Clamp01(value);
			OnValidate();
		}
	}

	public float startAngleFade
	{
		get
		{
			return m_StartAngleFade;
		}
		set
		{
			m_StartAngleFade = Mathf.Clamp(value, 0f, 180f);
			OnValidate();
		}
	}

	public float endAngleFade
	{
		get
		{
			return m_EndAngleFade;
		}
		set
		{
			m_EndAngleFade = Mathf.Clamp(value, m_StartAngleFade, 180f);
			OnValidate();
		}
	}

	public Vector2 uvScale
	{
		get
		{
			return m_UVScale;
		}
		set
		{
			m_UVScale = value;
			OnValidate();
		}
	}

	public Vector2 uvBias
	{
		get
		{
			return m_UVBias;
		}
		set
		{
			m_UVBias = value;
			OnValidate();
		}
	}

	public RenderingLayerMask renderingLayerMask
	{
		get
		{
			return m_RenderingLayerMask;
		}
		set
		{
			m_RenderingLayerMask = value;
		}
	}

	public DecalScaleMode scaleMode
	{
		get
		{
			return m_ScaleMode;
		}
		set
		{
			m_ScaleMode = value;
			OnValidate();
		}
	}

	public Vector3 pivot
	{
		get
		{
			return m_Offset;
		}
		set
		{
			m_Offset = value;
			OnValidate();
		}
	}

	public Vector3 size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size = value;
			OnValidate();
		}
	}

	public float fadeFactor
	{
		get
		{
			return m_FadeFactor;
		}
		set
		{
			m_FadeFactor = Mathf.Clamp01(value);
			OnValidate();
		}
	}

	internal Vector3 effectiveScale
	{
		get
		{
			if (m_ScaleMode != DecalScaleMode.InheritFromHierarchy)
			{
				return Vector3.one;
			}
			return base.transform.lossyScale;
		}
	}

	internal Vector3 decalSize => new Vector3(m_Size.x, m_Size.z, m_Size.y);

	internal Vector3 decalOffset => new Vector3(m_Offset.x, 0f - m_Offset.z, m_Offset.y);

	internal Vector4 uvScaleBias => new Vector4(m_UVScale.x, m_UVScale.y, m_UVBias.x, m_UVBias.y);

	internal static event DecalProjectorAction onDecalAdd;

	internal static event DecalProjectorAction onDecalRemove;

	internal static event DecalProjectorAction onDecalPropertyChange;

	internal static event Action onAllDecalPropertyChange;

	internal static event DecalProjectorAction onDecalMaterialChange;

	private void InitMaterial()
	{
		_ = m_Material == null;
	}

	private void OnEnable()
	{
		InitMaterial();
		m_OldMaterial = m_Material;
		DecalProjector.onDecalAdd?.Invoke(this);
	}

	private void OnDisable()
	{
		DecalProjector.onDecalRemove?.Invoke(this);
	}

	internal void OnValidate()
	{
		if (base.isActiveAndEnabled)
		{
			if (m_Material != m_OldMaterial)
			{
				DecalProjector.onDecalMaterialChange?.Invoke(this);
				m_OldMaterial = m_Material;
			}
			else
			{
				DecalProjector.onDecalPropertyChange?.Invoke(this);
			}
			m_OldDrawDistance = m_DrawDistance;
			m_OldFadeScale = m_FadeScale;
			m_OldStartAngleFade = m_StartAngleFade;
			m_OldEndAngleFade = m_EndAngleFade;
			m_OldUVScale = m_UVScale;
			m_OldUVBias = m_UVBias;
			m_OldScaleMode = m_ScaleMode;
			m_OldOffset = m_Offset;
			m_OldSize = m_Size;
			m_OldFadeFactor = m_FadeFactor;
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		if (m_OldMaterial != m_Material || Mathf.Abs(m_OldDrawDistance - m_DrawDistance) > Mathf.Epsilon || Mathf.Abs(m_OldFadeScale - m_FadeScale) > Mathf.Epsilon || Mathf.Abs(m_OldStartAngleFade - m_StartAngleFade) > Mathf.Epsilon || Mathf.Abs(m_OldEndAngleFade - m_EndAngleFade) > Mathf.Epsilon || m_OldUVScale != m_UVScale || m_OldUVBias != m_UVBias || m_OldScaleMode != m_ScaleMode || m_OldOffset != m_Offset || m_OldSize != m_Size || Mathf.Abs(m_OldFadeFactor - m_FadeFactor) > Mathf.Epsilon)
		{
			OnValidate();
		}
	}

	public bool IsValid()
	{
		if (material == null)
		{
			return false;
		}
		if (material.FindPass("DBufferProjector") != -1)
		{
			return true;
		}
		if (material.FindPass("DecalProjectorForwardEmissive") != -1)
		{
			return true;
		}
		if (material.FindPass("DecalScreenSpaceProjector") != -1)
		{
			return true;
		}
		if (material.FindPass("DecalGBufferProjector") != -1)
		{
			return true;
		}
		return false;
	}

	internal static void UpdateAllDecalProperties()
	{
		DecalProjector.onAllDecalPropertyChange?.Invoke();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (version == Version.Count)
		{
			version = Version.RenderingLayerMask;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (version == Version.Count)
		{
			version = Version.Initial;
		}
		if (version < Version.RenderingLayerMask)
		{
			m_RenderingLayerMask = m_DecalLayerMask;
			version = Version.RenderingLayerMask;
		}
	}
}
