using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/TextMeshPro - Text (UI)", 11)]
[ExecuteAlways]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html")]
public class TextMeshProUGUI : TMP_Text, ILayoutElement
{
	private bool m_isRebuildingLayout;

	private Coroutine m_DelayedGraphicRebuild;

	private Coroutine m_DelayedMaterialRebuild;

	private bool m_ShouldUpdateCulling;

	private Rect m_ClipRect;

	private bool m_ValidRect;

	[SerializeField]
	private bool m_hasFontAssetChanged;

	protected TMP_SubMeshUI[] m_subTextObjects = new TMP_SubMeshUI[8];

	private float m_previousLossyScaleY = -1f;

	private Vector3[] m_RectTransformCorners = new Vector3[4];

	private CanvasRenderer m_canvasRenderer;

	private Canvas m_canvas;

	private float m_CanvasScaleFactor;

	private bool m_isFirstAllocation;

	private int m_max_characters = 8;

	[SerializeField]
	private Material m_baseMaterial;

	private bool m_isScrollRegionSet;

	[SerializeField]
	private Vector4 m_maskOffset;

	private Matrix4x4 m_EnvMapMatrix;

	[NonSerialized]
	private bool m_isRegisteredForEvents;

	private static ProfilerMarker k_GenerateTextMarker = new ProfilerMarker("TMP.GenerateText");

	private static ProfilerMarker k_SetArraySizesMarker = new ProfilerMarker("TMP.SetArraySizes");

	private static ProfilerMarker k_GenerateTextPhaseIMarker = new ProfilerMarker("TMP GenerateText - Phase I");

	private static ProfilerMarker k_ParseMarkupTextMarker = new ProfilerMarker("TMP Parse Markup Text");

	private static ProfilerMarker k_CharacterLookupMarker = new ProfilerMarker("TMP Lookup Character & Glyph Data");

	private static ProfilerMarker k_HandleGPOSFeaturesMarker = new ProfilerMarker("TMP Handle GPOS Features");

	private static ProfilerMarker k_CalculateVerticesPositionMarker = new ProfilerMarker("TMP Calculate Vertices Position");

	private static ProfilerMarker k_ComputeTextMetricsMarker = new ProfilerMarker("TMP Compute Text Metrics");

	private static ProfilerMarker k_HandleVisibleCharacterMarker = new ProfilerMarker("TMP Handle Visible Character");

	private static ProfilerMarker k_HandleWhiteSpacesMarker = new ProfilerMarker("TMP Handle White Space & Control Character");

	private static ProfilerMarker k_HandleHorizontalLineBreakingMarker = new ProfilerMarker("TMP Handle Horizontal Line Breaking");

	private static ProfilerMarker k_HandleVerticalLineBreakingMarker = new ProfilerMarker("TMP Handle Vertical Line Breaking");

	private static ProfilerMarker k_SaveGlyphVertexDataMarker = new ProfilerMarker("TMP Save Glyph Vertex Data");

	private static ProfilerMarker k_ComputeCharacterAdvanceMarker = new ProfilerMarker("TMP Compute Character Advance");

	private static ProfilerMarker k_HandleCarriageReturnMarker = new ProfilerMarker("TMP Handle Carriage Return");

	private static ProfilerMarker k_HandleLineTerminationMarker = new ProfilerMarker("TMP Handle Line Termination");

	private static ProfilerMarker k_SavePageInfoMarker = new ProfilerMarker("TMP Save Page Info");

	private static ProfilerMarker k_SaveTextExtentMarker = new ProfilerMarker("TMP Save Text Extent");

	private static ProfilerMarker k_SaveProcessingStatesMarker = new ProfilerMarker("TMP Save Processing States");

	private static ProfilerMarker k_GenerateTextPhaseIIMarker = new ProfilerMarker("TMP GenerateText - Phase II");

	private static ProfilerMarker k_GenerateTextPhaseIIIMarker = new ProfilerMarker("TMP GenerateText - Phase III");

	private Dictionary<int, int> materialIndexPairs = new Dictionary<int, int>();

	public override Material materialForRendering => TMP_MaterialManager.GetMaterialForRendering(this, m_sharedMaterial);

	public override bool autoSizeTextContainer
	{
		get
		{
			return m_autoSizeTextContainer;
		}
		set
		{
			if (m_autoSizeTextContainer != value)
			{
				m_autoSizeTextContainer = value;
				if (m_autoSizeTextContainer)
				{
					CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
					SetLayoutDirty();
				}
			}
		}
	}

	public override Mesh mesh => m_mesh;

	public new CanvasRenderer canvasRenderer
	{
		get
		{
			if (m_canvasRenderer == null)
			{
				m_canvasRenderer = GetComponent<CanvasRenderer>();
			}
			return m_canvasRenderer;
		}
	}

	public Vector4 maskOffset
	{
		get
		{
			return m_maskOffset;
		}
		set
		{
			m_maskOffset = value;
			UpdateMask();
			m_havePropertiesChanged = true;
		}
	}

	public override event Action<TMP_TextInfo> OnPreRenderText;

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
	}

	public override void SetVerticesDirty()
	{
		if (!(this == null) && IsActive() && !CanvasUpdateRegistry.IsRebuildingGraphics())
		{
			CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
			if (m_OnDirtyVertsCallback != null)
			{
				m_OnDirtyVertsCallback();
			}
		}
	}

	public override void SetLayoutDirty()
	{
		m_isPreferredWidthDirty = true;
		m_isPreferredHeightDirty = true;
		if (!(this == null) && IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
			m_isLayoutDirty = true;
			if (m_OnDirtyLayoutCallback != null)
			{
				m_OnDirtyLayoutCallback();
			}
		}
	}

	public override void SetMaterialDirty()
	{
		if (!(this == null) && IsActive() && !CanvasUpdateRegistry.IsRebuildingGraphics())
		{
			m_isMaterialDirty = true;
			CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
			if (m_OnDirtyMaterialCallback != null)
			{
				m_OnDirtyMaterialCallback();
			}
		}
	}

	public override void SetAllDirty()
	{
		SetLayoutDirty();
		SetVerticesDirty();
		SetMaterialDirty();
	}

	private IEnumerator DelayedGraphicRebuild()
	{
		yield return null;
		CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
		if (m_OnDirtyVertsCallback != null)
		{
			m_OnDirtyVertsCallback();
		}
		m_DelayedGraphicRebuild = null;
	}

	private IEnumerator DelayedMaterialRebuild()
	{
		yield return null;
		m_isMaterialDirty = true;
		CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
		if (m_OnDirtyMaterialCallback != null)
		{
			m_OnDirtyMaterialCallback();
		}
		m_DelayedMaterialRebuild = null;
	}

	public override void Rebuild(CanvasUpdate update)
	{
		if (this == null)
		{
			return;
		}
		switch (update)
		{
		case CanvasUpdate.Prelayout:
			if (m_autoSizeTextContainer)
			{
				m_rectTransform.sizeDelta = GetPreferredValues(float.PositiveInfinity, float.PositiveInfinity);
			}
			break;
		case CanvasUpdate.PreRender:
			OnPreRenderCanvas();
			if (m_isMaterialDirty)
			{
				UpdateMaterial();
				m_isMaterialDirty = false;
			}
			break;
		}
	}

	private void UpdateSubObjectPivot()
	{
		if (m_textInfo != null)
		{
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				m_subTextObjects[i].SetPivotDirty();
			}
		}
	}

	public override Material GetModifiedMaterial(Material baseMaterial)
	{
		Material material = baseMaterial;
		if (m_ShouldRecalculateStencil)
		{
			Transform stopAfter = MaskUtilities.FindRootSortOverrideCanvas(base.transform);
			m_StencilValue = (base.maskable ? MaskUtilities.GetStencilDepth(base.transform, stopAfter) : 0);
			m_ShouldRecalculateStencil = false;
		}
		if (m_StencilValue > 0)
		{
			Material maskMaterial = StencilMaterial.Add(material, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
			StencilMaterial.Remove(m_MaskMaterial);
			m_MaskMaterial = maskMaterial;
			material = m_MaskMaterial;
		}
		return material;
	}

	protected override void UpdateMaterial()
	{
		if (!(m_sharedMaterial == null) && !(canvasRenderer == null))
		{
			m_canvasRenderer.materialCount = 1;
			m_canvasRenderer.SetMaterial(materialForRendering, 0);
		}
	}

	public override void RecalculateClipping()
	{
		base.RecalculateClipping();
	}

	public override void Cull(Rect clipRect, bool validRect)
	{
		m_ShouldUpdateCulling = false;
		if (m_isLayoutDirty)
		{
			m_ShouldUpdateCulling = true;
			m_ClipRect = clipRect;
			m_ValidRect = validRect;
			return;
		}
		Rect canvasSpaceClippingRect = GetCanvasSpaceClippingRect();
		bool flag = !validRect || !clipRect.Overlaps(canvasSpaceClippingRect, allowInverse: true);
		if (m_canvasRenderer.cull != flag)
		{
			m_canvasRenderer.cull = flag;
			base.onCullStateChanged.Invoke(flag);
			OnCullingChanged();
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				m_subTextObjects[i].canvasRenderer.cull = flag;
			}
		}
	}

	internal override void UpdateCulling()
	{
		Rect canvasSpaceClippingRect = GetCanvasSpaceClippingRect();
		bool flag = !m_ValidRect || !m_ClipRect.Overlaps(canvasSpaceClippingRect, allowInverse: true);
		if (m_canvasRenderer.cull != flag)
		{
			m_canvasRenderer.cull = flag;
			base.onCullStateChanged.Invoke(flag);
			OnCullingChanged();
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				m_subTextObjects[i].canvasRenderer.cull = flag;
			}
		}
		m_ShouldUpdateCulling = false;
	}

	public override void UpdateMeshPadding()
	{
		m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
		m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
		m_havePropertiesChanged = true;
		checkPaddingRequired = false;
		if (m_textInfo != null)
		{
			for (int i = 1; i < m_textInfo.materialCount; i++)
			{
				m_subTextObjects[i].UpdateMeshPadding(m_enableExtraPadding, m_isUsingBold);
			}
		}
	}

	protected override void InternalCrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
	{
		if (m_textInfo != null)
		{
			int materialCount = m_textInfo.materialCount;
			for (int i = 1; i < materialCount; i++)
			{
				m_subTextObjects[i].CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			}
		}
	}

	protected override void InternalCrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
	{
		if (m_textInfo != null)
		{
			int materialCount = m_textInfo.materialCount;
			for (int i = 1; i < materialCount; i++)
			{
				m_subTextObjects[i].CrossFadeAlpha(alpha, duration, ignoreTimeScale);
			}
		}
	}

	public override void ForceMeshUpdate(bool ignoreActiveState = false, bool forceTextReparsing = false)
	{
		m_havePropertiesChanged = true;
		m_ignoreActiveState = ignoreActiveState;
		if (m_canvas == null)
		{
			m_canvas = GetComponentInParent<Canvas>();
		}
		OnPreRenderCanvas();
	}

	public override TMP_TextInfo GetTextInfo(string text)
	{
		SetText(text);
		SetArraySizes(m_TextProcessingArray);
		m_renderMode = TextRenderFlags.DontRender;
		ComputeMarginSize();
		if (m_canvas == null)
		{
			m_canvas = base.canvas;
		}
		GenerateTextMesh();
		m_renderMode = TextRenderFlags.Render;
		return base.textInfo;
	}

	public override void ClearMesh()
	{
		m_canvasRenderer.SetMesh(null);
		for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
		{
			m_subTextObjects[i].canvasRenderer.SetMesh(null);
		}
	}

	public override void UpdateGeometry(Mesh mesh, int index)
	{
		mesh.RecalculateBounds();
		if (index == 0)
		{
			m_canvasRenderer.SetMesh(mesh);
		}
		else
		{
			m_subTextObjects[index].canvasRenderer.SetMesh(mesh);
		}
	}

	public override void UpdateVertexData(TMP_VertexDataUpdateFlags flags)
	{
		int materialCount = m_textInfo.materialCount;
		for (int i = 0; i < materialCount; i++)
		{
			Mesh mesh = ((i != 0) ? m_subTextObjects[i].mesh : m_mesh);
			if ((flags & TMP_VertexDataUpdateFlags.Vertices) == TMP_VertexDataUpdateFlags.Vertices)
			{
				mesh.vertices = m_textInfo.meshInfo[i].vertices;
			}
			if ((flags & TMP_VertexDataUpdateFlags.Uv0) == TMP_VertexDataUpdateFlags.Uv0)
			{
				mesh.SetUVs(0, m_textInfo.meshInfo[i].uvs0);
			}
			if ((flags & TMP_VertexDataUpdateFlags.Uv2) == TMP_VertexDataUpdateFlags.Uv2)
			{
				mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
			}
			if ((flags & TMP_VertexDataUpdateFlags.Colors32) == TMP_VertexDataUpdateFlags.Colors32)
			{
				mesh.colors32 = m_textInfo.meshInfo[i].colors32;
			}
			mesh.RecalculateBounds();
			if (i == 0)
			{
				m_canvasRenderer.SetMesh(mesh);
			}
			else
			{
				m_subTextObjects[i].canvasRenderer.SetMesh(mesh);
			}
		}
	}

	public override void UpdateVertexData()
	{
		int materialCount = m_textInfo.materialCount;
		for (int i = 0; i < materialCount; i++)
		{
			Mesh mesh;
			if (i == 0)
			{
				mesh = m_mesh;
			}
			else
			{
				m_textInfo.meshInfo[i].ClearUnusedVertices();
				mesh = m_subTextObjects[i].mesh;
			}
			mesh.vertices = m_textInfo.meshInfo[i].vertices;
			mesh.SetUVs(0, m_textInfo.meshInfo[i].uvs0);
			mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
			mesh.colors32 = m_textInfo.meshInfo[i].colors32;
			mesh.RecalculateBounds();
			if (i == 0)
			{
				m_canvasRenderer.SetMesh(mesh);
			}
			else
			{
				m_subTextObjects[i].canvasRenderer.SetMesh(mesh);
			}
		}
	}

	public void UpdateFontAsset()
	{
		LoadFontAsset();
	}

	protected override void Awake()
	{
		m_canvas = base.canvas;
		m_isOrthographic = true;
		m_rectTransform = base.gameObject.GetComponent<RectTransform>();
		if (m_rectTransform == null)
		{
			m_rectTransform = base.gameObject.AddComponent<RectTransform>();
		}
		m_canvasRenderer = GetComponent<CanvasRenderer>();
		if (m_canvasRenderer == null)
		{
			m_canvasRenderer = base.gameObject.AddComponent<CanvasRenderer>();
		}
		if (m_mesh == null)
		{
			m_mesh = new Mesh();
			m_mesh.hideFlags = HideFlags.HideAndDontSave;
			m_textInfo = new TMP_TextInfo(this);
		}
		LoadDefaultSettings();
		LoadFontAsset();
		if (m_TextProcessingArray == null)
		{
			m_TextProcessingArray = new TextProcessingElement[m_max_characters];
		}
		m_cached_TextElement = new TMP_Character();
		m_isFirstAllocation = true;
		m_havePropertiesChanged = true;
		m_isAwake = true;
	}

	protected override void OnEnable()
	{
		if (m_isAwake)
		{
			if (!m_isRegisteredForEvents)
			{
				m_isRegisteredForEvents = true;
			}
			m_canvas = GetCanvas();
			SetActiveSubMeshes(state: true);
			GraphicRegistry.RegisterGraphicForCanvas(m_canvas, this);
			if (!m_IsTextObjectScaleStatic)
			{
				TMP_UpdateManager.RegisterTextObjectForUpdate(this);
			}
			ComputeMarginSize();
			SetAllDirty();
			RecalculateClipping();
			RecalculateMasking();
		}
	}

	protected override void OnDisable()
	{
		if (m_isAwake)
		{
			GraphicRegistry.UnregisterGraphicForCanvas(m_canvas, this);
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
			TMP_UpdateManager.UnRegisterTextObjectForUpdate(this);
			if (m_canvasRenderer != null)
			{
				m_canvasRenderer.Clear();
			}
			SetActiveSubMeshes(state: false);
			LayoutRebuilder.MarkLayoutForRebuild(m_rectTransform);
			RecalculateClipping();
			RecalculateMasking();
		}
	}

	protected override void OnDestroy()
	{
		GraphicRegistry.UnregisterGraphicForCanvas(m_canvas, this);
		TMP_UpdateManager.UnRegisterTextObjectForUpdate(this);
		if (m_mesh != null)
		{
			UnityEngine.Object.DestroyImmediate(m_mesh);
		}
		if (m_MaskMaterial != null)
		{
			TMP_MaterialManager.ReleaseStencilMaterial(m_MaskMaterial);
			m_MaskMaterial = null;
		}
		m_isRegisteredForEvents = false;
	}

	protected override void LoadFontAsset()
	{
		ShaderUtilities.GetShaderPropertyIDs();
		if (m_fontAsset == null)
		{
			if (TMP_Settings.defaultFontAsset != null)
			{
				m_fontAsset = TMP_Settings.defaultFontAsset;
			}
			if (m_fontAsset == null)
			{
				Debug.LogWarning("The LiberationSans SDF Font Asset was not found. There is no Font Asset assigned to " + base.gameObject.name + ".", this);
				return;
			}
			if (m_fontAsset.characterLookupTable == null)
			{
				Debug.Log("Dictionary is Null!");
			}
			m_sharedMaterial = m_fontAsset.material;
		}
		else
		{
			if (m_fontAsset.characterLookupTable == null)
			{
				m_fontAsset.ReadFontAssetDefinition();
			}
			if (m_sharedMaterial == null && m_baseMaterial != null)
			{
				m_sharedMaterial = m_baseMaterial;
				m_baseMaterial = null;
			}
			if (m_sharedMaterial == null || m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex) == null || m_fontAsset.atlasTexture.GetInstanceID() != m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
			{
				if (m_fontAsset.material == null)
				{
					Debug.LogWarning("The Font Atlas Texture of the Font Asset " + m_fontAsset.name + " assigned to " + base.gameObject.name + " is missing.", this);
				}
				else
				{
					m_sharedMaterial = m_fontAsset.material;
				}
			}
		}
		ValidateEnvMapProperty();
		GetSpecialCharacters(m_fontAsset);
		m_padding = GetPaddingForMaterial();
		SetMaterialDirty();
	}

	private Canvas GetCanvas()
	{
		Canvas result = null;
		List<Canvas> list = TMP_ListPool<Canvas>.Get();
		base.gameObject.GetComponentsInParent(includeInactive: false, list);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].isActiveAndEnabled)
				{
					result = list[i];
					break;
				}
			}
		}
		TMP_ListPool<Canvas>.Release(list);
		return result;
	}

	private void ValidateEnvMapProperty()
	{
		if (m_sharedMaterial != null)
		{
			m_hasEnvMapProperty = m_sharedMaterial.HasProperty(ShaderUtilities.ID_EnvMap) && m_sharedMaterial.GetTexture(ShaderUtilities.ID_EnvMap) != null;
		}
		else
		{
			m_hasEnvMapProperty = false;
		}
	}

	private void UpdateEnvMapMatrix()
	{
		if (m_hasEnvMapProperty)
		{
			Vector3 vector = m_sharedMaterial.GetVector(ShaderUtilities.ID_EnvMatrixRotation);
			if (!(m_currentEnvMapRotation == vector))
			{
				m_currentEnvMapRotation = vector;
				m_EnvMapMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(m_currentEnvMapRotation), Vector3.one);
				m_sharedMaterial.SetMatrix(ShaderUtilities.ID_EnvMatrix, m_EnvMapMatrix);
			}
		}
	}

	private void EnableMasking()
	{
		if (m_fontMaterial == null)
		{
			m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
			m_canvasRenderer.SetMaterial(m_fontMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
		}
		m_sharedMaterial = m_fontMaterial;
		if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
		{
			m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
			m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
			m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
			UpdateMask();
		}
		m_isMaskingEnabled = true;
	}

	private void DisableMasking()
	{
	}

	private void UpdateMask()
	{
		if (m_rectTransform != null)
		{
			if (!ShaderUtilities.isInitialized)
			{
				ShaderUtilities.GetShaderPropertyIDs();
			}
			m_isScrollRegionSet = true;
			float num = Mathf.Min(Mathf.Min(m_margin.x, m_margin.z), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX));
			float num2 = Mathf.Min(Mathf.Min(m_margin.y, m_margin.w), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessY));
			num = ((num > 0f) ? num : 0f);
			num2 = ((num2 > 0f) ? num2 : 0f);
			float z = (m_rectTransform.rect.width - Mathf.Max(m_margin.x, 0f) - Mathf.Max(m_margin.z, 0f)) / 2f + num;
			float w = (m_rectTransform.rect.height - Mathf.Max(m_margin.y, 0f) - Mathf.Max(m_margin.w, 0f)) / 2f + num2;
			Vector2 vector = m_rectTransform.localPosition + new Vector3((0.5f - m_rectTransform.pivot.x) * m_rectTransform.rect.width + (Mathf.Max(m_margin.x, 0f) - Mathf.Max(m_margin.z, 0f)) / 2f, (0.5f - m_rectTransform.pivot.y) * m_rectTransform.rect.height + (0f - Mathf.Max(m_margin.y, 0f) + Mathf.Max(m_margin.w, 0f)) / 2f);
			Vector4 value = new Vector4(vector.x, vector.y, z, w);
			m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, value);
		}
	}

	protected override Material GetMaterial(Material mat)
	{
		ShaderUtilities.GetShaderPropertyIDs();
		if (m_fontMaterial == null || m_fontMaterial.GetInstanceID() != mat.GetInstanceID())
		{
			m_fontMaterial = CreateMaterialInstance(mat);
		}
		m_sharedMaterial = m_fontMaterial;
		m_padding = GetPaddingForMaterial();
		m_ShouldRecalculateStencil = true;
		SetVerticesDirty();
		SetMaterialDirty();
		return m_sharedMaterial;
	}

	protected override Material[] GetMaterials(Material[] mats)
	{
		int materialCount = m_textInfo.materialCount;
		if (m_fontMaterials == null)
		{
			m_fontMaterials = new Material[materialCount];
		}
		else if (m_fontMaterials.Length != materialCount)
		{
			TMP_TextInfo.Resize(ref m_fontMaterials, materialCount, isBlockAllocated: false);
		}
		for (int i = 0; i < materialCount; i++)
		{
			if (i == 0)
			{
				m_fontMaterials[i] = base.fontMaterial;
			}
			else
			{
				m_fontMaterials[i] = m_subTextObjects[i].material;
			}
		}
		m_fontSharedMaterials = m_fontMaterials;
		return m_fontMaterials;
	}

	protected override void SetSharedMaterial(Material mat)
	{
		m_sharedMaterial = mat;
		m_padding = GetPaddingForMaterial();
		SetMaterialDirty();
	}

	protected override Material[] GetSharedMaterials()
	{
		int materialCount = m_textInfo.materialCount;
		if (m_fontSharedMaterials == null)
		{
			m_fontSharedMaterials = new Material[materialCount];
		}
		else if (m_fontSharedMaterials.Length != materialCount)
		{
			TMP_TextInfo.Resize(ref m_fontSharedMaterials, materialCount, isBlockAllocated: false);
		}
		for (int i = 0; i < materialCount; i++)
		{
			if (i == 0)
			{
				m_fontSharedMaterials[i] = m_sharedMaterial;
			}
			else
			{
				m_fontSharedMaterials[i] = m_subTextObjects[i].sharedMaterial;
			}
		}
		return m_fontSharedMaterials;
	}

	protected override void SetSharedMaterials(Material[] materials)
	{
		int materialCount = m_textInfo.materialCount;
		if (m_fontSharedMaterials == null)
		{
			m_fontSharedMaterials = new Material[materialCount];
		}
		else if (m_fontSharedMaterials.Length != materialCount)
		{
			TMP_TextInfo.Resize(ref m_fontSharedMaterials, materialCount, isBlockAllocated: false);
		}
		for (int i = 0; i < materialCount; i++)
		{
			if (i == 0)
			{
				if (!(materials[i].GetTexture(ShaderUtilities.ID_MainTex) == null) && materials[i].GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() == m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
				{
					m_sharedMaterial = (m_fontSharedMaterials[i] = materials[i]);
					m_padding = GetPaddingForMaterial(m_sharedMaterial);
				}
			}
			else if (!(materials[i].GetTexture(ShaderUtilities.ID_MainTex) == null) && materials[i].GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() == m_subTextObjects[i].sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() && m_subTextObjects[i].isDefaultMaterial)
			{
				m_subTextObjects[i].sharedMaterial = (m_fontSharedMaterials[i] = materials[i]);
			}
		}
	}

	protected override void SetOutlineThickness(float thickness)
	{
		if (m_fontMaterial != null && m_sharedMaterial.GetInstanceID() != m_fontMaterial.GetInstanceID())
		{
			m_sharedMaterial = m_fontMaterial;
			m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
		}
		else if (m_fontMaterial == null)
		{
			m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
			m_sharedMaterial = m_fontMaterial;
			m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
		}
		thickness = Mathf.Clamp01(thickness);
		m_sharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
		m_padding = GetPaddingForMaterial();
	}

	protected override void SetFaceColor(Color32 color)
	{
		if (m_fontMaterial == null)
		{
			m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
		}
		m_sharedMaterial = m_fontMaterial;
		m_padding = GetPaddingForMaterial();
		m_sharedMaterial.SetColor(ShaderUtilities.ID_FaceColor, color);
	}

	protected override void SetOutlineColor(Color32 color)
	{
		if (m_fontMaterial == null)
		{
			m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
		}
		m_sharedMaterial = m_fontMaterial;
		m_padding = GetPaddingForMaterial();
		m_sharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, color);
	}

	protected override void SetShaderDepth()
	{
		if (!(m_canvas == null) && !(m_sharedMaterial == null) && m_canvas.renderMode != 0)
		{
			_ = m_isOverlay;
		}
	}

	protected override void SetCulling()
	{
		if (m_isCullingEnabled)
		{
			Material material = materialForRendering;
			if (material != null)
			{
				material.SetFloat("_CullMode", 2f);
			}
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				material = m_subTextObjects[i].materialForRendering;
				if (material != null)
				{
					material.SetFloat(ShaderUtilities.ShaderTag_CullMode, 2f);
				}
			}
			return;
		}
		Material material2 = materialForRendering;
		if (material2 != null)
		{
			material2.SetFloat("_CullMode", 0f);
		}
		for (int j = 1; j < m_subTextObjects.Length && m_subTextObjects[j] != null; j++)
		{
			material2 = m_subTextObjects[j].materialForRendering;
			if (material2 != null)
			{
				material2.SetFloat(ShaderUtilities.ShaderTag_CullMode, 0f);
			}
		}
	}

	private void SetPerspectiveCorrection()
	{
		if (m_isOrthographic)
		{
			m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0f);
		}
		else
		{
			m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.875f);
		}
	}

	private void SetMeshArrays(int size)
	{
		m_textInfo.meshInfo[0].ResizeMeshInfo(size);
		m_canvasRenderer.SetMesh(m_textInfo.meshInfo[0].mesh);
	}

	internal override int SetArraySizes(TextProcessingElement[] textProcessingArray)
	{
		int num = 0;
		m_totalCharacterCount = 0;
		m_isUsingBold = false;
		m_isTextLayoutPhase = false;
		tag_NoParsing = false;
		m_FontStyleInternal = m_fontStyle;
		m_fontStyleStack.Clear();
		m_FontWeightInternal = (((m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold) ? FontWeight.Bold : m_fontWeight);
		m_FontWeightStack.SetDefault(m_FontWeightInternal);
		m_currentFontAsset = m_fontAsset;
		m_currentMaterial = m_sharedMaterial;
		m_currentMaterialIndex = 0;
		materialIndexPairs.Clear();
		TMP_Text.m_materialReferenceStack.SetDefault(new MaterialReference(m_currentMaterialIndex, m_currentFontAsset, null, m_currentMaterial, m_padding));
		TMP_Text.m_materialReferenceIndexLookup.Clear();
		MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
		if (m_textInfo == null)
		{
			m_textInfo = new TMP_TextInfo(m_InternalTextProcessingArraySize);
		}
		else if (m_textInfo.characterInfo.Length < m_InternalTextProcessingArraySize)
		{
			TMP_TextInfo.Resize(ref m_textInfo.characterInfo, m_InternalTextProcessingArraySize, isBlockAllocated: false);
		}
		m_textElementType = TMP_TextElementType.Character;
		if (m_overflowMode == TextOverflowModes.Ellipsis)
		{
			GetEllipsisSpecialCharacter(m_currentFontAsset);
			if (m_Ellipsis.character != null)
			{
				if (m_Ellipsis.fontAsset.GetInstanceID() != m_currentFontAsset.GetInstanceID())
				{
					if (TMP_Settings.matchMaterialPreset && m_currentMaterial.GetInstanceID() != m_Ellipsis.fontAsset.material.GetInstanceID())
					{
						m_Ellipsis.material = TMP_MaterialManager.GetFallbackMaterial(m_currentMaterial, m_Ellipsis.fontAsset.material);
					}
					else
					{
						m_Ellipsis.material = m_Ellipsis.fontAsset.material;
					}
					m_Ellipsis.materialIndex = MaterialReference.AddMaterialReference(m_Ellipsis.material, m_Ellipsis.fontAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
					TMP_Text.m_materialReferences[m_Ellipsis.materialIndex].referenceCount = 0;
				}
			}
			else
			{
				m_overflowMode = TextOverflowModes.Truncate;
				if (!TMP_Settings.warningsDisabled)
				{
					Debug.LogWarning("The character used for Ellipsis is not available in font asset [" + m_currentFontAsset.name + "] or any potential fallbacks. Switching Text Overflow mode to Truncate.", this);
				}
			}
		}
		bool flag = m_ActiveFontFeatures.Contains(OTL_FeatureTag.liga);
		if (m_overflowMode == TextOverflowModes.Linked && m_linkedTextComponent != null && !m_isCalculatingPreferredValues)
		{
			TMP_Text tMP_Text = m_linkedTextComponent;
			while (tMP_Text != null)
			{
				tMP_Text.text = string.Empty;
				tMP_Text.ClearMesh();
				tMP_Text.textInfo.Clear();
				tMP_Text = tMP_Text.linkedTextComponent;
			}
		}
		for (int i = 0; i < textProcessingArray.Length && textProcessingArray[i].unicode != 0; i++)
		{
			if (m_textInfo.characterInfo == null || m_totalCharacterCount >= m_textInfo.characterInfo.Length)
			{
				TMP_TextInfo.Resize(ref m_textInfo.characterInfo, m_totalCharacterCount + 1, isBlockAllocated: true);
			}
			uint num2 = textProcessingArray[i].unicode;
			if (m_isRichText && num2 == 60)
			{
				int currentMaterialIndex = m_currentMaterialIndex;
				if (ValidateHtmlTag(textProcessingArray, i + 1, out var endIndex))
				{
					int stringIndex = textProcessingArray[i].stringIndex;
					i = endIndex;
					if ((m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold)
					{
						m_isUsingBold = true;
					}
					if (m_textElementType == TMP_TextElementType.Sprite)
					{
						TMP_Text.m_materialReferences[m_currentMaterialIndex].referenceCount++;
						m_textInfo.characterInfo[m_totalCharacterCount].character = (char)(57344 + m_spriteIndex);
						m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
						m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
						m_textInfo.characterInfo[m_totalCharacterCount].textElement = m_currentSpriteAsset.spriteCharacterTable[m_spriteIndex];
						m_textInfo.characterInfo[m_totalCharacterCount].elementType = m_textElementType;
						m_textInfo.characterInfo[m_totalCharacterCount].index = stringIndex;
						m_textInfo.characterInfo[m_totalCharacterCount].stringLength = textProcessingArray[i].stringIndex - stringIndex + 1;
						m_textElementType = TMP_TextElementType.Character;
						m_currentMaterialIndex = currentMaterialIndex;
						num++;
						m_totalCharacterCount++;
					}
					continue;
				}
			}
			bool isAlternativeTypeface = false;
			bool flag2 = false;
			TMP_FontAsset currentFontAsset = m_currentFontAsset;
			Material currentMaterial = m_currentMaterial;
			int currentMaterialIndex2 = m_currentMaterialIndex;
			if (m_textElementType == TMP_TextElementType.Character)
			{
				if ((m_FontStyleInternal & FontStyles.UpperCase) == FontStyles.UpperCase)
				{
					if (char.IsLower((char)num2))
					{
						num2 = char.ToUpper((char)num2);
					}
				}
				else if ((m_FontStyleInternal & FontStyles.LowerCase) == FontStyles.LowerCase)
				{
					if (char.IsUpper((char)num2))
					{
						num2 = char.ToLower((char)num2);
					}
				}
				else if ((m_FontStyleInternal & FontStyles.SmallCaps) == FontStyles.SmallCaps && char.IsLower((char)num2))
				{
					num2 = char.ToUpper((char)num2);
				}
			}
			TMP_TextElement tMP_TextElement = null;
			uint num3 = ((i + 1 < textProcessingArray.Length) ? textProcessingArray[i + 1].unicode : 0u);
			if (base.emojiFallbackSupport && ((TMP_TextParsingUtilities.IsEmojiPresentationForm(num2) && num3 != 65038) || (TMP_TextParsingUtilities.IsEmoji(num2) && num3 == 65039)) && TMP_Settings.emojiFallbackTextAssets != null && TMP_Settings.emojiFallbackTextAssets.Count > 0)
			{
				tMP_TextElement = TMP_FontAssetUtilities.GetTextElementFromTextAssets(num2, m_currentFontAsset, TMP_Settings.emojiFallbackTextAssets, includeFallbacks: true, base.fontStyle, base.fontWeight, out isAlternativeTypeface);
			}
			if (tMP_TextElement == null)
			{
				tMP_TextElement = GetTextElement(num2, m_currentFontAsset, m_FontStyleInternal, m_FontWeightInternal, out isAlternativeTypeface);
			}
			if (tMP_TextElement == null)
			{
				DoMissingGlyphCallback((int)num2, textProcessingArray[i].stringIndex, m_currentFontAsset);
				uint num4 = num2;
				num2 = (textProcessingArray[i].unicode = ((TMP_Settings.missingGlyphCharacter == 0) ? 9633u : ((uint)TMP_Settings.missingGlyphCharacter)));
				tMP_TextElement = TMP_FontAssetUtilities.GetCharacterFromFontAsset(num2, m_currentFontAsset, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isAlternativeTypeface);
				if (tMP_TextElement == null && TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
				{
					tMP_TextElement = TMP_FontAssetUtilities.GetCharacterFromFontAssets(num2, m_currentFontAsset, TMP_Settings.fallbackFontAssets, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isAlternativeTypeface);
				}
				if (tMP_TextElement == null && TMP_Settings.defaultFontAsset != null)
				{
					tMP_TextElement = TMP_FontAssetUtilities.GetCharacterFromFontAsset(num2, TMP_Settings.defaultFontAsset, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isAlternativeTypeface);
				}
				if (tMP_TextElement == null)
				{
					num2 = (textProcessingArray[i].unicode = 32u);
					tMP_TextElement = TMP_FontAssetUtilities.GetCharacterFromFontAsset(num2, m_currentFontAsset, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isAlternativeTypeface);
				}
				if (tMP_TextElement == null)
				{
					num2 = (textProcessingArray[i].unicode = 3u);
					tMP_TextElement = TMP_FontAssetUtilities.GetCharacterFromFontAsset(num2, m_currentFontAsset, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isAlternativeTypeface);
				}
				if (!TMP_Settings.warningsDisabled)
				{
					Debug.LogWarning((num4 > 65535) ? $"The character with Unicode value \\U{num4:X8} was not found in the [{m_fontAsset.name}] font asset or any potential fallbacks. It was replaced by Unicode character \\u{tMP_TextElement.unicode:X4} in text object [{base.name}]." : $"The character with Unicode value \\u{num4:X4} was not found in the [{m_fontAsset.name}] font asset or any potential fallbacks. It was replaced by Unicode character \\u{tMP_TextElement.unicode:X4} in text object [{base.name}].", this);
				}
			}
			m_textInfo.characterInfo[m_totalCharacterCount].alternativeGlyph = null;
			if (tMP_TextElement.elementType == TextElementType.Character)
			{
				if (tMP_TextElement.textAsset.instanceID != m_currentFontAsset.instanceID)
				{
					flag2 = true;
					m_currentFontAsset = tMP_TextElement.textAsset as TMP_FontAsset;
				}
				if ((num3 >= 65024 && num3 <= 65039) || (num3 >= 917760 && num3 <= 917999))
				{
					uint glyphVariantIndex = m_currentFontAsset.GetGlyphVariantIndex(num2, num3);
					if (glyphVariantIndex != 0 && m_currentFontAsset.TryAddGlyphInternal(glyphVariantIndex, out var glyph))
					{
						m_textInfo.characterInfo[m_totalCharacterCount].alternativeGlyph = glyph;
					}
					textProcessingArray[i + 1].unicode = 26u;
					i++;
				}
				if (flag && m_currentFontAsset.fontFeatureTable.m_LigatureSubstitutionRecordLookup.TryGetValue(tMP_TextElement.glyphIndex, out var value))
				{
					if (value == null)
					{
						break;
					}
					for (int j = 0; j < value.Count; j++)
					{
						LigatureSubstitutionRecord ligatureSubstitutionRecord = value[j];
						uint[] componentGlyphIDs = ligatureSubstitutionRecord.componentGlyphIDs;
						int num5 = ligatureSubstitutionRecord.componentGlyphIDs.Length;
						uint num6 = ligatureSubstitutionRecord.ligatureGlyphID;
						int num7 = i + 1;
						int num8 = 1;
						while (num6 != 0 && num8 < num5)
						{
							if (num7 >= textProcessingArray.Length)
							{
								num6 = 0u;
								break;
							}
							uint unicode = textProcessingArray[num7].unicode;
							uint glyphIndex = m_currentFontAsset.GetGlyphIndex(unicode);
							uint num9 = componentGlyphIDs[num8];
							if (glyphIndex != num9 && TMP_TextParsingUtilities.IsIgnorableForLigature(unicode))
							{
								num7++;
								continue;
							}
							if (glyphIndex == num9)
							{
								num8++;
								num7++;
								continue;
							}
							num6 = 0u;
							break;
						}
						if (num6 == 0 || num8 != num5)
						{
							continue;
						}
						int num10 = num7 - i;
						if (m_currentFontAsset.TryAddGlyphInternal(num6, out var glyph2))
						{
							m_textInfo.characterInfo[m_totalCharacterCount].alternativeGlyph = glyph2;
							textProcessingArray[i].length = num10;
							for (int k = 1; k < num10; k++)
							{
								textProcessingArray[i + k].unicode = 26u;
							}
							i += num10 - 1;
							break;
						}
					}
				}
			}
			m_textInfo.characterInfo[m_totalCharacterCount].elementType = TMP_TextElementType.Character;
			m_textInfo.characterInfo[m_totalCharacterCount].textElement = tMP_TextElement;
			m_textInfo.characterInfo[m_totalCharacterCount].isUsingAlternateTypeface = isAlternativeTypeface;
			m_textInfo.characterInfo[m_totalCharacterCount].character = (char)num2;
			m_textInfo.characterInfo[m_totalCharacterCount].index = textProcessingArray[i].stringIndex;
			m_textInfo.characterInfo[m_totalCharacterCount].stringLength = textProcessingArray[i].length;
			m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
			if (tMP_TextElement.elementType == TextElementType.Sprite)
			{
				TMP_SpriteAsset tMP_SpriteAsset = tMP_TextElement.textAsset as TMP_SpriteAsset;
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(tMP_SpriteAsset.material, tMP_SpriteAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
				TMP_Text.m_materialReferences[m_currentMaterialIndex].referenceCount++;
				m_textInfo.characterInfo[m_totalCharacterCount].elementType = TMP_TextElementType.Sprite;
				m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
				m_textElementType = TMP_TextElementType.Character;
				m_currentMaterialIndex = currentMaterialIndex2;
				num++;
				m_totalCharacterCount++;
				continue;
			}
			if (flag2 && m_currentFontAsset.instanceID != m_fontAsset.instanceID)
			{
				if (TMP_Settings.matchMaterialPreset)
				{
					m_currentMaterial = TMP_MaterialManager.GetFallbackMaterial(m_currentMaterial, m_currentFontAsset.material);
				}
				else
				{
					m_currentMaterial = m_currentFontAsset.material;
				}
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
			}
			int num11 = ((m_textInfo.characterInfo[m_totalCharacterCount].alternativeGlyph != null) ? m_textInfo.characterInfo[m_totalCharacterCount].alternativeGlyph.atlasIndex : (tMP_TextElement?.glyph.atlasIndex ?? 0));
			if (tMP_TextElement != null && num11 > 0)
			{
				m_currentMaterial = TMP_MaterialManager.GetFallbackMaterial(m_currentFontAsset, m_currentMaterial, num11);
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
				flag2 = true;
			}
			if (!char.IsWhiteSpace((char)num2) && num2 != 8203)
			{
				if (TMP_Text.m_materialReferences[m_currentMaterialIndex].referenceCount < 16383)
				{
					TMP_Text.m_materialReferences[m_currentMaterialIndex].referenceCount++;
				}
				else if (flag2)
				{
					if (materialIndexPairs.TryGetValue(m_currentMaterialIndex, out var value2) && TMP_Text.m_materialReferences[value2].referenceCount < 16383)
					{
						m_currentMaterialIndex = value2;
					}
					else
					{
						int num12 = MaterialReference.AddMaterialReference(new Material(m_currentMaterial), m_currentFontAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
						materialIndexPairs[m_currentMaterialIndex] = num12;
						m_currentMaterialIndex = num12;
					}
					TMP_Text.m_materialReferences[m_currentMaterialIndex].referenceCount++;
				}
				else
				{
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(new Material(m_currentMaterial), m_currentFontAsset, ref TMP_Text.m_materialReferences, TMP_Text.m_materialReferenceIndexLookup);
					TMP_Text.m_materialReferences[m_currentMaterialIndex].referenceCount++;
				}
			}
			m_textInfo.characterInfo[m_totalCharacterCount].material = m_currentMaterial;
			m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
			TMP_Text.m_materialReferences[m_currentMaterialIndex].isFallbackMaterial = flag2;
			if (flag2)
			{
				TMP_Text.m_materialReferences[m_currentMaterialIndex].fallbackMaterial = currentMaterial;
				m_currentFontAsset = currentFontAsset;
				m_currentMaterial = currentMaterial;
				m_currentMaterialIndex = currentMaterialIndex2;
			}
			m_totalCharacterCount++;
		}
		if (m_isCalculatingPreferredValues)
		{
			m_isCalculatingPreferredValues = false;
			return m_totalCharacterCount;
		}
		m_textInfo.spriteCount = num;
		int num13 = (m_textInfo.materialCount = TMP_Text.m_materialReferenceIndexLookup.Count);
		if (num13 > m_textInfo.meshInfo.Length)
		{
			TMP_TextInfo.Resize(ref m_textInfo.meshInfo, num13, isBlockAllocated: false);
		}
		if (num13 > m_subTextObjects.Length)
		{
			TMP_TextInfo.Resize(ref m_subTextObjects, Mathf.NextPowerOfTwo(num13 + 1));
		}
		if (m_VertexBufferAutoSizeReduction && m_textInfo.characterInfo.Length - m_totalCharacterCount > 256)
		{
			TMP_TextInfo.Resize(ref m_textInfo.characterInfo, Mathf.Max(m_totalCharacterCount + 1, 256), isBlockAllocated: true);
		}
		for (int l = 0; l < num13; l++)
		{
			if (l > 0)
			{
				if (m_subTextObjects[l] == null)
				{
					m_subTextObjects[l] = TMP_SubMeshUI.AddSubTextObject(this, TMP_Text.m_materialReferences[l]);
					m_textInfo.meshInfo[l].vertices = null;
				}
				if (m_rectTransform.pivot != m_subTextObjects[l].rectTransform.pivot)
				{
					m_subTextObjects[l].rectTransform.pivot = m_rectTransform.pivot;
				}
				if (m_subTextObjects[l].sharedMaterial == null || m_subTextObjects[l].sharedMaterial.GetInstanceID() != TMP_Text.m_materialReferences[l].material.GetInstanceID())
				{
					m_subTextObjects[l].sharedMaterial = TMP_Text.m_materialReferences[l].material;
					m_subTextObjects[l].fontAsset = TMP_Text.m_materialReferences[l].fontAsset;
					m_subTextObjects[l].spriteAsset = TMP_Text.m_materialReferences[l].spriteAsset;
				}
				if (TMP_Text.m_materialReferences[l].isFallbackMaterial)
				{
					m_subTextObjects[l].fallbackMaterial = TMP_Text.m_materialReferences[l].material;
					m_subTextObjects[l].fallbackSourceMaterial = TMP_Text.m_materialReferences[l].fallbackMaterial;
				}
			}
			int referenceCount = TMP_Text.m_materialReferences[l].referenceCount;
			if (m_textInfo.meshInfo[l].vertices == null || m_textInfo.meshInfo[l].vertices.Length < referenceCount * 4)
			{
				if (m_textInfo.meshInfo[l].vertices == null)
				{
					if (l == 0)
					{
						m_textInfo.meshInfo[l] = new TMP_MeshInfo(m_mesh, referenceCount + 1);
					}
					else
					{
						m_textInfo.meshInfo[l] = new TMP_MeshInfo(m_subTextObjects[l].mesh, referenceCount + 1);
					}
				}
				else
				{
					m_textInfo.meshInfo[l].ResizeMeshInfo((referenceCount > 1024) ? (referenceCount + 256) : Mathf.NextPowerOfTwo(referenceCount + 1));
				}
			}
			else if (m_VertexBufferAutoSizeReduction && referenceCount > 0 && m_textInfo.meshInfo[l].vertices.Length / 4 - referenceCount > 256)
			{
				m_textInfo.meshInfo[l].ResizeMeshInfo((referenceCount > 1024) ? (referenceCount + 256) : Mathf.NextPowerOfTwo(referenceCount + 1));
			}
			m_textInfo.meshInfo[l].material = TMP_Text.m_materialReferences[l].material;
		}
		for (int m = num13; m < m_subTextObjects.Length && m_subTextObjects[m] != null; m++)
		{
			if (m < m_textInfo.meshInfo.Length)
			{
				m_subTextObjects[m].canvasRenderer.SetMesh(null);
			}
		}
		return m_totalCharacterCount;
	}

	public override void ComputeMarginSize()
	{
		if (base.rectTransform != null)
		{
			Rect rect = m_rectTransform.rect;
			m_marginWidth = rect.width - m_margin.x - m_margin.z;
			m_marginHeight = rect.height - m_margin.y - m_margin.w;
			m_PreviousRectTransformSize = rect.size;
			m_PreviousPivotPosition = m_rectTransform.pivot;
			m_RectTransformCorners = GetTextContainerLocalCorners();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		m_havePropertiesChanged = true;
		SetVerticesDirty();
		SetLayoutDirty();
	}

	protected override void OnCanvasHierarchyChanged()
	{
		base.OnCanvasHierarchyChanged();
		m_canvas = base.canvas;
		if (m_isAwake && base.isActiveAndEnabled)
		{
			if (m_canvas == null || !m_canvas.enabled)
			{
				TMP_UpdateManager.UnRegisterTextObjectForUpdate(this);
			}
			else if (!m_IsTextObjectScaleStatic)
			{
				TMP_UpdateManager.RegisterTextObjectForUpdate(this);
			}
		}
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		m_canvas = base.canvas;
		ComputeMarginSize();
		m_havePropertiesChanged = true;
	}

	protected override void OnRectTransformDimensionsChange()
	{
		if (base.gameObject.activeInHierarchy)
		{
			bool flag = false;
			if (m_canvas != null && !Mathf.Approximately(m_CanvasScaleFactor, m_canvas.scaleFactor))
			{
				m_CanvasScaleFactor = m_canvas.scaleFactor;
				flag = true;
			}
			if (flag || !(base.rectTransform != null) || !(Mathf.Abs(m_rectTransform.rect.width - m_PreviousRectTransformSize.x) < 0.0001f) || !(Mathf.Abs(m_rectTransform.rect.height - m_PreviousRectTransformSize.y) < 0.0001f) || !(Mathf.Abs(m_rectTransform.pivot.x - m_PreviousPivotPosition.x) < 0.0001f) || !(Mathf.Abs(m_rectTransform.pivot.y - m_PreviousPivotPosition.y) < 0.0001f))
			{
				ComputeMarginSize();
				UpdateSubObjectPivot();
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	internal override void InternalUpdate()
	{
		if (!m_havePropertiesChanged)
		{
			float y = m_rectTransform.lossyScale.y;
			if (y != m_previousLossyScaleY && m_TextProcessingArray[0].unicode != 0)
			{
				float num = y / m_previousLossyScaleY;
				if (num < 0.8f || num > 1.25f)
				{
					UpdateSDFScale(num);
					m_previousLossyScaleY = y;
				}
			}
		}
		if (m_isUsingLegacyAnimationComponent)
		{
			m_havePropertiesChanged = true;
			OnPreRenderCanvas();
		}
		UpdateEnvMapMatrix();
	}

	private void OnPreRenderCanvas()
	{
		if (!m_isAwake || (!IsActive() && !m_ignoreActiveState))
		{
			return;
		}
		if (m_canvas == null)
		{
			m_canvas = base.canvas;
			if (m_canvas == null)
			{
				return;
			}
		}
		if (!m_havePropertiesChanged && !m_isLayoutDirty)
		{
			return;
		}
		if (m_fontAsset == null)
		{
			Debug.LogWarning("Please assign a Font Asset to this " + base.transform.name + " gameobject.", this);
			return;
		}
		if (checkPaddingRequired)
		{
			UpdateMeshPadding();
		}
		ParseInputText();
		TMP_FontAsset.UpdateFontAssetsInUpdateQueue();
		if (m_enableAutoSizing)
		{
			m_fontSize = Mathf.Clamp(m_fontSizeBase, m_fontSizeMin, m_fontSizeMax);
		}
		m_maxFontSize = m_fontSizeMax;
		m_minFontSize = m_fontSizeMin;
		m_lineSpacingDelta = 0f;
		m_charWidthAdjDelta = 0f;
		m_isTextTruncated = false;
		m_havePropertiesChanged = false;
		m_isLayoutDirty = false;
		m_ignoreActiveState = false;
		m_IsAutoSizePointSizeSet = false;
		m_AutoSizeIterationCount = 0;
		while (!m_IsAutoSizePointSizeSet)
		{
			GenerateTextMesh();
			m_AutoSizeIterationCount++;
		}
	}

	protected virtual void GenerateTextMesh()
	{
		if (m_fontAsset == null || m_fontAsset.characterLookupTable == null)
		{
			Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + GetInstanceID());
			m_IsAutoSizePointSizeSet = true;
			return;
		}
		if (m_textInfo != null)
		{
			m_textInfo.Clear();
		}
		if (m_TextProcessingArray == null || m_TextProcessingArray.Length == 0 || m_TextProcessingArray[0].unicode == 0)
		{
			ClearMesh();
			m_preferredWidth = 0f;
			m_preferredHeight = 0f;
			TMPro_EventManager.ON_TEXT_CHANGED(this);
			m_IsAutoSizePointSizeSet = true;
			return;
		}
		m_currentFontAsset = m_fontAsset;
		m_currentMaterial = m_sharedMaterial;
		m_currentMaterialIndex = 0;
		TMP_Text.m_materialReferenceStack.SetDefault(new MaterialReference(m_currentMaterialIndex, m_currentFontAsset, null, m_currentMaterial, m_padding));
		m_currentSpriteAsset = m_spriteAsset;
		if (m_spriteAnimator != null)
		{
			m_spriteAnimator.StopAllAnimations();
		}
		int totalCharacterCount = m_totalCharacterCount;
		float num = (m_isOrthographic ? 1f : 0.1f);
		float num2 = m_fontSize / m_fontAsset.m_FaceInfo.pointSize * m_fontAsset.m_FaceInfo.scale * num;
		float num3 = num2;
		float num4 = m_fontSize * 0.01f * num;
		m_fontScaleMultiplier = 1f;
		m_currentFontSize = m_fontSize;
		m_sizeStack.SetDefault(m_currentFontSize);
		float num5 = 0f;
		uint num6 = 0u;
		m_FontStyleInternal = m_fontStyle;
		m_FontWeightInternal = (((m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold) ? FontWeight.Bold : m_fontWeight);
		m_FontWeightStack.SetDefault(m_FontWeightInternal);
		m_fontStyleStack.Clear();
		m_lineJustification = m_HorizontalAlignment;
		m_lineJustificationStack.SetDefault(m_lineJustification);
		float num7 = 0f;
		m_baselineOffset = 0f;
		m_baselineOffsetStack.Clear();
		bool flag = false;
		Vector3 start = Vector3.zero;
		Vector3 zero = Vector3.zero;
		bool flag2 = false;
		Vector3 start2 = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		bool flag3 = false;
		Vector3 start3 = Vector3.zero;
		Vector3 end = Vector3.zero;
		m_fontColor32 = m_fontColor;
		m_htmlColor = m_fontColor32;
		m_underlineColor = m_htmlColor;
		m_strikethroughColor = m_htmlColor;
		m_colorStack.SetDefault(m_htmlColor);
		m_underlineColorStack.SetDefault(m_htmlColor);
		m_strikethroughColorStack.SetDefault(m_htmlColor);
		m_HighlightStateStack.SetDefault(new HighlightState(m_htmlColor, TMP_Offset.zero));
		m_colorGradientPreset = null;
		m_colorGradientStack.SetDefault(null);
		m_ItalicAngle = m_currentFontAsset.italicStyle;
		m_ItalicAngleStack.SetDefault(m_ItalicAngle);
		m_actionStack.Clear();
		m_FXScale = Vector3.one;
		m_FXRotation = Quaternion.identity;
		m_lineOffset = 0f;
		m_lineHeight = -32767f;
		float num8 = m_currentFontAsset.m_FaceInfo.lineHeight - (m_currentFontAsset.m_FaceInfo.ascentLine - m_currentFontAsset.m_FaceInfo.descentLine);
		m_cSpacing = 0f;
		m_monoSpacing = 0f;
		m_xAdvance = 0f;
		tag_LineIndent = 0f;
		tag_Indent = 0f;
		m_indentStack.SetDefault(0f);
		tag_NoParsing = false;
		m_characterCount = 0;
		m_firstCharacterOfLine = m_firstVisibleCharacter;
		m_lastCharacterOfLine = 0;
		m_firstVisibleCharacterOfLine = 0;
		m_lastVisibleCharacterOfLine = 0;
		m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
		m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
		m_lineNumber = 0;
		m_startOfLineAscender = 0f;
		m_startOfLineDescender = 0f;
		m_lineVisibleCharacterCount = 0;
		m_lineVisibleSpaceCount = 0;
		bool flag4 = true;
		m_IsDrivenLineSpacing = false;
		m_firstOverflowCharacterIndex = -1;
		m_LastBaseGlyphIndex = int.MinValue;
		bool flag5 = m_ActiveFontFeatures.Contains(OTL_FeatureTag.kern);
		bool flag6 = m_ActiveFontFeatures.Contains(OTL_FeatureTag.mark);
		bool flag7 = m_ActiveFontFeatures.Contains(OTL_FeatureTag.mkmk);
		m_pageNumber = 0;
		int num9 = Mathf.Clamp(m_pageToDisplay - 1, 0, m_textInfo.pageInfo.Length - 1);
		m_textInfo.ClearPageInfo();
		Vector4 vector = m_margin;
		float num10 = ((m_marginWidth > 0f) ? m_marginWidth : 0f);
		float num11 = ((m_marginHeight > 0f) ? m_marginHeight : 0f);
		m_marginLeft = 0f;
		m_marginRight = 0f;
		m_width = -1f;
		float num12 = num10 + 0.0001f - m_marginLeft - m_marginRight;
		m_meshExtents.min = TMP_Text.k_LargePositiveVector2;
		m_meshExtents.max = TMP_Text.k_LargeNegativeVector2;
		m_textInfo.ClearLineInfo();
		m_maxCapHeight = 0f;
		m_maxTextAscender = 0f;
		m_ElementDescender = 0f;
		m_PageAscender = 0f;
		float maxVisibleDescender = 0f;
		bool isMaxVisibleDescenderSet = false;
		m_isNewPage = false;
		bool flag8 = true;
		m_isNonBreakingSpace = false;
		bool flag9 = false;
		int num13 = 0;
		CharacterSubstitution characterSubstitution = new CharacterSubstitution(-1, 0u);
		bool flag10 = false;
		SaveWordWrappingState(ref TMP_Text.m_SavedWordWrapState, -1, -1);
		SaveWordWrappingState(ref TMP_Text.m_SavedLineState, -1, -1);
		SaveWordWrappingState(ref TMP_Text.m_SavedEllipsisState, -1, -1);
		SaveWordWrappingState(ref TMP_Text.m_SavedLastValidState, -1, -1);
		SaveWordWrappingState(ref TMP_Text.m_SavedSoftLineBreakState, -1, -1);
		TMP_Text.m_EllipsisInsertionCandidateStack.Clear();
		int num14 = 0;
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		Vector3 vector5 = default(Vector3);
		for (int i = 0; i < m_TextProcessingArray.Length && m_TextProcessingArray[i].unicode != 0; i++)
		{
			num6 = m_TextProcessingArray[i].unicode;
			if (num14 > 5)
			{
				Debug.LogError("Line breaking recursion max threshold hit... Character [" + num6 + "] index: " + i);
				characterSubstitution.index = m_characterCount;
				characterSubstitution.unicode = 3u;
			}
			if (num6 == 26)
			{
				continue;
			}
			if (m_isRichText && num6 == 60)
			{
				m_isTextLayoutPhase = true;
				m_textElementType = TMP_TextElementType.Character;
				if (ValidateHtmlTag(m_TextProcessingArray, i + 1, out var endIndex))
				{
					i = endIndex;
					if (m_textElementType == TMP_TextElementType.Character)
					{
						continue;
					}
				}
			}
			else
			{
				m_textElementType = m_textInfo.characterInfo[m_characterCount].elementType;
				m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
				m_currentFontAsset = m_textInfo.characterInfo[m_characterCount].fontAsset;
			}
			int currentMaterialIndex = m_currentMaterialIndex;
			bool isUsingAlternateTypeface = m_textInfo.characterInfo[m_characterCount].isUsingAlternateTypeface;
			m_isTextLayoutPhase = false;
			bool flag11 = false;
			if (characterSubstitution.index == m_characterCount)
			{
				num6 = characterSubstitution.unicode;
				m_textElementType = TMP_TextElementType.Character;
				flag11 = true;
				switch (num6)
				{
				case 3u:
					m_textInfo.characterInfo[m_characterCount].textElement = m_currentFontAsset.characterLookupTable[3u];
					m_isTextTruncated = true;
					break;
				case 8230u:
					m_textInfo.characterInfo[m_characterCount].textElement = m_Ellipsis.character;
					m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
					m_textInfo.characterInfo[m_characterCount].fontAsset = m_Ellipsis.fontAsset;
					m_textInfo.characterInfo[m_characterCount].material = m_Ellipsis.material;
					m_textInfo.characterInfo[m_characterCount].materialReferenceIndex = m_Ellipsis.materialIndex;
					TMP_Text.m_materialReferences[m_Underline.materialIndex].referenceCount++;
					m_isTextTruncated = true;
					characterSubstitution.index = m_characterCount + 1;
					characterSubstitution.unicode = 3u;
					break;
				}
			}
			if (m_characterCount < m_firstVisibleCharacter && num6 != 3)
			{
				m_textInfo.characterInfo[m_characterCount].isVisible = false;
				m_textInfo.characterInfo[m_characterCount].character = '\u200b';
				m_textInfo.characterInfo[m_characterCount].lineNumber = 0;
				m_characterCount++;
				continue;
			}
			float num15 = 1f;
			if (m_textElementType == TMP_TextElementType.Character)
			{
				if ((m_FontStyleInternal & FontStyles.UpperCase) == FontStyles.UpperCase)
				{
					if (char.IsLower((char)num6))
					{
						num6 = char.ToUpper((char)num6);
					}
				}
				else if ((m_FontStyleInternal & FontStyles.LowerCase) == FontStyles.LowerCase)
				{
					if (char.IsUpper((char)num6))
					{
						num6 = char.ToLower((char)num6);
					}
				}
				else if ((m_FontStyleInternal & FontStyles.SmallCaps) == FontStyles.SmallCaps && char.IsLower((char)num6))
				{
					num15 = 0.8f;
					num6 = char.ToUpper((char)num6);
				}
			}
			float num16 = 0f;
			float num17 = 0f;
			float num18 = 0f;
			FaceInfo faceInfo = m_currentFontAsset.m_FaceInfo;
			if (m_textElementType == TMP_TextElementType.Sprite)
			{
				TMP_SpriteCharacter tMP_SpriteCharacter = (TMP_SpriteCharacter)base.textInfo.characterInfo[m_characterCount].textElement;
				if (tMP_SpriteCharacter == null)
				{
					continue;
				}
				m_currentSpriteAsset = tMP_SpriteCharacter.textAsset as TMP_SpriteAsset;
				m_spriteIndex = (int)tMP_SpriteCharacter.glyphIndex;
				if (num6 == 60)
				{
					num6 = (uint)(57344 + m_spriteIndex);
				}
				else
				{
					m_spriteColor = TMP_Text.s_colorWhite;
				}
				float num19 = m_currentFontSize / faceInfo.pointSize * faceInfo.scale * num;
				FaceInfo faceInfo2 = m_currentSpriteAsset.m_FaceInfo;
				if (faceInfo2.pointSize > 0f)
				{
					float num20 = m_currentFontSize / faceInfo2.pointSize * faceInfo2.scale * num;
					num3 = tMP_SpriteCharacter.m_Scale * tMP_SpriteCharacter.m_Glyph.scale * num20;
					num17 = faceInfo2.ascentLine;
					num16 = faceInfo2.baseline * num19 * m_fontScaleMultiplier * faceInfo2.scale;
					num18 = faceInfo2.descentLine;
				}
				else
				{
					float num21 = m_currentFontSize / faceInfo.pointSize * faceInfo.scale * num;
					num3 = faceInfo.ascentLine / tMP_SpriteCharacter.m_Glyph.metrics.height * tMP_SpriteCharacter.m_Scale * tMP_SpriteCharacter.m_Glyph.scale * num21;
					float num22 = ((num3 != 0f) ? (num21 / num3) : 0f);
					num17 = faceInfo.ascentLine * num22;
					num16 = faceInfo.baseline * num19 * m_fontScaleMultiplier * faceInfo.scale;
					num18 = faceInfo.descentLine * num22;
				}
				m_cached_TextElement = tMP_SpriteCharacter;
				m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;
				m_textInfo.characterInfo[m_characterCount].scale = num3;
				m_textInfo.characterInfo[m_characterCount].fontAsset = m_currentFontAsset;
				m_textInfo.characterInfo[m_characterCount].materialReferenceIndex = m_currentMaterialIndex;
				m_currentMaterialIndex = currentMaterialIndex;
				num7 = 0f;
			}
			else if (m_textElementType == TMP_TextElementType.Character)
			{
				m_cached_TextElement = m_textInfo.characterInfo[m_characterCount].textElement;
				if (m_cached_TextElement == null)
				{
					continue;
				}
				m_currentFontAsset = m_textInfo.characterInfo[m_characterCount].fontAsset;
				m_currentMaterial = m_textInfo.characterInfo[m_characterCount].material;
				m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
				float num23 = ((!flag11 || m_TextProcessingArray[i].unicode != 10 || m_characterCount == m_firstCharacterOfLine) ? (m_currentFontSize * num15 / faceInfo.pointSize * faceInfo.scale * num) : (m_textInfo.characterInfo[m_characterCount - 1].pointSize * num15 / faceInfo.pointSize * faceInfo.scale * num));
				if (flag11 && num6 == 8230)
				{
					num17 = 0f;
					num18 = 0f;
				}
				else
				{
					num17 = faceInfo.ascentLine;
					num18 = faceInfo.descentLine;
				}
				num3 = num23 * m_fontScaleMultiplier * m_cached_TextElement.m_Scale * m_cached_TextElement.m_Glyph.scale;
				num16 = faceInfo.baseline * num23 * m_fontScaleMultiplier * faceInfo.scale;
				m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
				m_textInfo.characterInfo[m_characterCount].scale = num3;
				num7 = ((m_currentMaterialIndex == 0) ? m_padding : m_subTextObjects[m_currentMaterialIndex].padding);
			}
			float num24 = num3;
			if (num6 == 173 || num6 == 3)
			{
				num3 = 0f;
			}
			m_textInfo.characterInfo[m_characterCount].character = (char)num6;
			m_textInfo.characterInfo[m_characterCount].pointSize = m_currentFontSize;
			m_textInfo.characterInfo[m_characterCount].color = m_htmlColor;
			m_textInfo.characterInfo[m_characterCount].underlineColor = m_underlineColor;
			m_textInfo.characterInfo[m_characterCount].strikethroughColor = m_strikethroughColor;
			m_textInfo.characterInfo[m_characterCount].highlightState = m_HighlightState;
			m_textInfo.characterInfo[m_characterCount].style = m_FontStyleInternal;
			GlyphMetrics glyphMetrics = m_textInfo.characterInfo[m_characterCount].alternativeGlyph?.metrics ?? m_cached_TextElement.m_Glyph.metrics;
			bool flag12 = num6 <= 65535 && char.IsWhiteSpace((char)num6);
			GlyphValueRecord glyphValueRecord = default(GlyphValueRecord);
			float num25 = m_characterSpacing;
			if (flag5 && m_textElementType == TMP_TextElementType.Character)
			{
				uint glyphIndex = m_cached_TextElement.m_GlyphIndex;
				GlyphPairAdjustmentRecord value;
				if (m_characterCount < totalCharacterCount - 1 && m_textInfo.characterInfo[m_characterCount + 1].elementType == TMP_TextElementType.Character)
				{
					uint key = (m_textInfo.characterInfo[m_characterCount + 1].textElement.m_GlyphIndex << 16) | glyphIndex;
					if (m_currentFontAsset.m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.TryGetValue(key, out value))
					{
						glyphValueRecord = value.firstAdjustmentRecord.glyphValueRecord;
						num25 = (((value.featureLookupFlags & UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) == UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) ? 0f : num25);
					}
				}
				if (m_characterCount >= 1)
				{
					uint glyphIndex2 = m_textInfo.characterInfo[m_characterCount - 1].textElement.m_GlyphIndex;
					uint key2 = (glyphIndex << 16) | glyphIndex2;
					if (base.textInfo.characterInfo[m_characterCount - 1].elementType == TMP_TextElementType.Character && m_currentFontAsset.m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.TryGetValue(key2, out value))
					{
						glyphValueRecord += value.secondAdjustmentRecord.glyphValueRecord;
						num25 = (((value.featureLookupFlags & UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) == UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) ? 0f : num25);
					}
				}
			}
			m_textInfo.characterInfo[m_characterCount].adjustedHorizontalAdvance = glyphValueRecord.xAdvance;
			bool flag13 = TMP_TextParsingUtilities.IsBaseGlyph(num6);
			if (flag13)
			{
				m_LastBaseGlyphIndex = m_characterCount;
			}
			if (m_characterCount > 0 && !flag13)
			{
				if (flag6 && m_LastBaseGlyphIndex != int.MinValue && m_LastBaseGlyphIndex == m_characterCount - 1)
				{
					uint index = m_textInfo.characterInfo[m_LastBaseGlyphIndex].textElement.glyph.index;
					uint key3 = (m_cached_TextElement.glyphIndex << 16) | index;
					if (m_currentFontAsset.fontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.TryGetValue(key3, out var value2))
					{
						float num26 = (m_textInfo.characterInfo[m_LastBaseGlyphIndex].origin - m_xAdvance) / num3;
						glyphValueRecord.xPlacement = num26 + value2.baseGlyphAnchorPoint.xCoordinate - value2.markPositionAdjustment.xPositionAdjustment;
						glyphValueRecord.yPlacement = value2.baseGlyphAnchorPoint.yCoordinate - value2.markPositionAdjustment.yPositionAdjustment;
						num25 = 0f;
					}
				}
				else
				{
					bool flag14 = false;
					if (flag7)
					{
						int num27 = m_characterCount - 1;
						while (num27 >= 0 && num27 != m_LastBaseGlyphIndex)
						{
							uint index2 = m_textInfo.characterInfo[num27].textElement.glyph.index;
							uint key4 = (m_cached_TextElement.glyphIndex << 16) | index2;
							if (m_currentFontAsset.fontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.TryGetValue(key4, out var value3))
							{
								float num28 = (m_textInfo.characterInfo[num27].origin - m_xAdvance) / num3;
								float num29 = num16 - m_lineOffset + m_baselineOffset;
								float num30 = (m_textInfo.characterInfo[num27].baseLine - num29) / num3;
								glyphValueRecord.xPlacement = num28 + value3.baseMarkGlyphAnchorPoint.xCoordinate - value3.combiningMarkPositionAdjustment.xPositionAdjustment;
								glyphValueRecord.yPlacement = num30 + value3.baseMarkGlyphAnchorPoint.yCoordinate - value3.combiningMarkPositionAdjustment.yPositionAdjustment;
								num25 = 0f;
								flag14 = true;
								break;
							}
							num27--;
						}
					}
					if (flag6 && m_LastBaseGlyphIndex != int.MinValue && !flag14)
					{
						uint index3 = m_textInfo.characterInfo[m_LastBaseGlyphIndex].textElement.glyph.index;
						uint key5 = (m_cached_TextElement.glyphIndex << 16) | index3;
						if (m_currentFontAsset.fontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.TryGetValue(key5, out var value4))
						{
							float num31 = (m_textInfo.characterInfo[m_LastBaseGlyphIndex].origin - m_xAdvance) / num3;
							glyphValueRecord.xPlacement = num31 + value4.baseGlyphAnchorPoint.xCoordinate - value4.markPositionAdjustment.xPositionAdjustment;
							glyphValueRecord.yPlacement = value4.baseGlyphAnchorPoint.yCoordinate - value4.markPositionAdjustment.yPositionAdjustment;
							num25 = 0f;
						}
					}
				}
			}
			num17 += glyphValueRecord.yPlacement;
			num18 += glyphValueRecord.yPlacement;
			if (m_isRightToLeft)
			{
				m_xAdvance -= glyphMetrics.horizontalAdvance * (1f - m_charWidthAdjDelta) * num3;
				if (flag12 || num6 == 8203)
				{
					m_xAdvance -= m_wordSpacing * num4;
				}
			}
			float num32 = 0f;
			if (m_monoSpacing != 0f)
			{
				num32 = ((!m_duoSpace || (num6 != 46 && num6 != 58 && num6 != 44)) ? ((m_monoSpacing / 2f - (glyphMetrics.width / 2f + glyphMetrics.horizontalBearingX) * num3) * (1f - m_charWidthAdjDelta)) : ((m_monoSpacing / 4f - (glyphMetrics.width / 2f + glyphMetrics.horizontalBearingX) * num3) * (1f - m_charWidthAdjDelta)));
				m_xAdvance += num32;
			}
			float num33;
			float num34;
			if (m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && (m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold)
			{
				if (m_currentMaterial != null && m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
				{
					float @float = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
					num33 = m_currentFontAsset.boldStyle / 4f * @float * m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
					if (num33 + num7 > @float)
					{
						num7 = @float - num33;
					}
				}
				else
				{
					num33 = 0f;
				}
				num34 = m_currentFontAsset.boldSpacing;
			}
			else
			{
				if (m_currentMaterial != null && m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale) && m_currentMaterial.HasProperty(ShaderUtilities.ID_ScaleRatio_A))
				{
					float float2 = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
					num33 = m_currentFontAsset.normalStyle / 4f * float2 * m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
					if (num33 + num7 > float2)
					{
						num7 = float2 - num33;
					}
				}
				else
				{
					num33 = 0f;
				}
				num34 = 0f;
			}
			vector2.x = m_xAdvance + (glyphMetrics.horizontalBearingX * m_FXScale.x - num7 - num33 + glyphValueRecord.xPlacement) * num3 * (1f - m_charWidthAdjDelta);
			vector2.y = num16 + (glyphMetrics.horizontalBearingY + num7 + glyphValueRecord.yPlacement) * num3 - m_lineOffset + m_baselineOffset;
			vector2.z = 0f;
			vector3.x = vector2.x;
			vector3.y = vector2.y - (glyphMetrics.height + num7 * 2f) * num3;
			vector3.z = 0f;
			vector4.x = vector3.x + (glyphMetrics.width * m_FXScale.x + num7 * 2f + num33 * 2f) * num3 * (1f - m_charWidthAdjDelta);
			vector4.y = vector2.y;
			vector4.z = 0f;
			vector5.x = vector4.x;
			vector5.y = vector3.y;
			vector5.z = 0f;
			if (m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && (m_FontStyleInternal & FontStyles.Italic) == FontStyles.Italic)
			{
				float num35 = (float)m_ItalicAngle * 0.01f;
				float num36 = (m_currentFontAsset.m_FaceInfo.capLine - (m_currentFontAsset.m_FaceInfo.baseline + m_baselineOffset)) / 2f * m_fontScaleMultiplier * m_currentFontAsset.m_FaceInfo.scale;
				Vector3 vector6 = new Vector3(num35 * ((glyphMetrics.horizontalBearingY + num7 + num33 - num36) * num3), 0f, 0f);
				Vector3 vector7 = new Vector3(num35 * ((glyphMetrics.horizontalBearingY - glyphMetrics.height - num7 - num33 - num36) * num3), 0f, 0f);
				vector2 += vector6;
				vector3 += vector7;
				vector4 += vector6;
				vector5 += vector7;
			}
			if (m_FXRotation != Quaternion.identity)
			{
				Matrix4x4 matrix4x = Matrix4x4.Rotate(m_FXRotation);
				Vector3 vector8 = (vector4 + vector3) / 2f;
				vector2 = matrix4x.MultiplyPoint3x4(vector2 - vector8) + vector8;
				vector3 = matrix4x.MultiplyPoint3x4(vector3 - vector8) + vector8;
				vector4 = matrix4x.MultiplyPoint3x4(vector4 - vector8) + vector8;
				vector5 = matrix4x.MultiplyPoint3x4(vector5 - vector8) + vector8;
			}
			m_textInfo.characterInfo[m_characterCount].bottomLeft = vector3;
			m_textInfo.characterInfo[m_characterCount].topLeft = vector2;
			m_textInfo.characterInfo[m_characterCount].topRight = vector4;
			m_textInfo.characterInfo[m_characterCount].bottomRight = vector5;
			m_textInfo.characterInfo[m_characterCount].origin = m_xAdvance + glyphValueRecord.xPlacement * num3;
			m_textInfo.characterInfo[m_characterCount].baseLine = num16 - m_lineOffset + m_baselineOffset + glyphValueRecord.yPlacement * num3;
			m_textInfo.characterInfo[m_characterCount].aspectRatio = (vector4.x - vector3.x) / (vector2.y - vector3.y);
			float num37 = ((m_textElementType == TMP_TextElementType.Character) ? (num17 * num3 / num15 + m_baselineOffset) : (num17 * num3 + m_baselineOffset));
			float num38 = ((m_textElementType == TMP_TextElementType.Character) ? (num18 * num3 / num15 + m_baselineOffset) : (num18 * num3 + m_baselineOffset));
			float num39 = num37;
			float num40 = num38;
			bool flag15 = m_characterCount == m_firstCharacterOfLine;
			if (flag15 || !flag12)
			{
				if (m_baselineOffset != 0f)
				{
					num39 = Mathf.Max((num37 - m_baselineOffset) / m_fontScaleMultiplier, num39);
					num40 = Mathf.Min((num38 - m_baselineOffset) / m_fontScaleMultiplier, num40);
				}
				m_maxLineAscender = Mathf.Max(num39, m_maxLineAscender);
				m_maxLineDescender = Mathf.Min(num40, m_maxLineDescender);
			}
			if (flag15 || !flag12)
			{
				m_textInfo.characterInfo[m_characterCount].adjustedAscender = num39;
				m_textInfo.characterInfo[m_characterCount].adjustedDescender = num40;
				m_ElementAscender = (m_textInfo.characterInfo[m_characterCount].ascender = num37 - m_lineOffset);
				m_ElementDescender = (m_textInfo.characterInfo[m_characterCount].descender = num38 - m_lineOffset);
			}
			else
			{
				m_textInfo.characterInfo[m_characterCount].adjustedAscender = m_maxLineAscender;
				m_textInfo.characterInfo[m_characterCount].adjustedDescender = m_maxLineDescender;
				m_ElementAscender = (m_textInfo.characterInfo[m_characterCount].ascender = m_maxLineAscender - m_lineOffset);
				m_ElementDescender = (m_textInfo.characterInfo[m_characterCount].descender = m_maxLineDescender - m_lineOffset);
			}
			if ((m_lineNumber == 0 || m_isNewPage) && (flag15 || !flag12))
			{
				m_maxTextAscender = m_maxLineAscender;
				m_maxCapHeight = Mathf.Max(m_maxCapHeight, m_currentFontAsset.m_FaceInfo.capLine * num3 / num15);
			}
			if (m_lineOffset == 0f && (flag15 || !flag12))
			{
				m_PageAscender = ((m_PageAscender > num37) ? m_PageAscender : num37);
			}
			m_textInfo.characterInfo[m_characterCount].isVisible = false;
			bool flag16 = (m_lineJustification & HorizontalAlignmentOptions.Flush) == HorizontalAlignmentOptions.Flush || (m_lineJustification & HorizontalAlignmentOptions.Justified) == HorizontalAlignmentOptions.Justified;
			if (num6 == 9 || ((m_TextWrappingMode == TextWrappingModes.PreserveWhitespace || m_TextWrappingMode == TextWrappingModes.PreserveWhitespaceNoWrap) && (flag12 || num6 == 8203)) || (!flag12 && num6 != 8203 && num6 != 173 && num6 != 3) || (num6 == 173 && !flag10) || m_textElementType == TMP_TextElementType.Sprite)
			{
				m_textInfo.characterInfo[m_characterCount].isVisible = true;
				float marginLeft = m_marginLeft;
				float marginRight = m_marginRight;
				if (flag11)
				{
					marginLeft = m_textInfo.lineInfo[m_lineNumber].marginLeft;
					marginRight = m_textInfo.lineInfo[m_lineNumber].marginRight;
				}
				num12 = ((m_width != -1f) ? Mathf.Min(num10 + 0.0001f - marginLeft - marginRight, m_width) : (num10 + 0.0001f - marginLeft - marginRight));
				float num41 = Mathf.Abs(m_xAdvance) + ((!m_isRightToLeft) ? glyphMetrics.horizontalAdvance : 0f) * (1f - m_charWidthAdjDelta) * ((num6 == 173) ? num24 : num3);
				float num42 = m_maxTextAscender - (m_maxLineDescender - m_lineOffset) + ((m_lineOffset > 0f && !m_IsDrivenLineSpacing) ? (m_maxLineAscender - m_startOfLineAscender) : 0f);
				int characterCount = m_characterCount;
				if (num42 > num11 + 0.0001f)
				{
					if (m_firstOverflowCharacterIndex == -1)
					{
						m_firstOverflowCharacterIndex = m_characterCount;
					}
					if (m_enableAutoSizing)
					{
						if (m_lineSpacingDelta > m_lineSpacingMax && m_lineOffset > 0f && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
						{
							float num43 = (num11 - num42) / (float)m_lineNumber;
							m_lineSpacingDelta = Mathf.Max(m_lineSpacingDelta + num43 / num2, m_lineSpacingMax);
							return;
						}
						if (m_fontSize > m_fontSizeMin && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
						{
							m_maxFontSize = m_fontSize;
							float num44 = Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
							m_fontSize -= num44;
							m_fontSize = Mathf.Max((float)(int)(m_fontSize * 20f + 0.5f) / 20f, m_fontSizeMin);
							return;
						}
					}
					switch (m_overflowMode)
					{
					case TextOverflowModes.Truncate:
						i = RestoreWordWrappingState(ref TMP_Text.m_SavedLastValidState);
						characterSubstitution.index = characterCount;
						characterSubstitution.unicode = 3u;
						continue;
					case TextOverflowModes.Ellipsis:
						if (TMP_Text.m_EllipsisInsertionCandidateStack.Count == 0)
						{
							i = -1;
							m_characterCount = 0;
							characterSubstitution.index = 0;
							characterSubstitution.unicode = 3u;
							m_firstCharacterOfLine = 0;
						}
						else
						{
							WordWrapState state = TMP_Text.m_EllipsisInsertionCandidateStack.Pop();
							i = RestoreWordWrappingState(ref state);
							i--;
							m_characterCount--;
							characterSubstitution.index = m_characterCount;
							characterSubstitution.unicode = 8230u;
							num14++;
						}
						continue;
					case TextOverflowModes.Linked:
						i = RestoreWordWrappingState(ref TMP_Text.m_SavedLastValidState);
						if (m_linkedTextComponent != null)
						{
							m_linkedTextComponent.text = text;
							m_linkedTextComponent.m_inputSource = m_inputSource;
							m_linkedTextComponent.firstVisibleCharacter = m_characterCount;
							m_linkedTextComponent.ForceMeshUpdate();
							m_isTextTruncated = true;
						}
						characterSubstitution.index = characterCount;
						characterSubstitution.unicode = 3u;
						continue;
					case TextOverflowModes.Page:
						if (i < 0 || characterCount == 0)
						{
							i = -1;
							m_characterCount = 0;
							characterSubstitution.index = 0;
							characterSubstitution.unicode = 3u;
							continue;
						}
						if (m_maxLineAscender - m_maxLineDescender > num11 + 0.0001f)
						{
							i = RestoreWordWrappingState(ref TMP_Text.m_SavedLineState);
							characterSubstitution.index = characterCount;
							characterSubstitution.unicode = 3u;
							continue;
						}
						i = RestoreWordWrappingState(ref TMP_Text.m_SavedLineState);
						m_isNewPage = true;
						m_firstCharacterOfLine = m_characterCount;
						m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
						m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
						m_startOfLineAscender = 0f;
						m_xAdvance = 0f + tag_Indent;
						m_lineOffset = 0f;
						m_maxTextAscender = 0f;
						m_PageAscender = 0f;
						m_lineNumber++;
						m_pageNumber++;
						continue;
					}
				}
				if (flag13 && num41 > num12 * (flag16 ? 1.05f : 1f))
				{
					if (m_TextWrappingMode != 0 && m_TextWrappingMode != TextWrappingModes.PreserveWhitespaceNoWrap && m_characterCount != m_firstCharacterOfLine)
					{
						i = RestoreWordWrappingState(ref TMP_Text.m_SavedWordWrapState);
						float num45 = 0f;
						if (m_lineHeight == -32767f)
						{
							float adjustedAscender = m_textInfo.characterInfo[m_characterCount].adjustedAscender;
							num45 = ((m_lineOffset > 0f && !m_IsDrivenLineSpacing) ? (m_maxLineAscender - m_startOfLineAscender) : 0f) - m_maxLineDescender + adjustedAscender + (num8 + m_lineSpacingDelta) * num2 + m_lineSpacing * num4;
						}
						else
						{
							num45 = m_lineHeight + m_lineSpacing * num4;
							m_IsDrivenLineSpacing = true;
						}
						float num46 = m_maxTextAscender + num45 + m_lineOffset - m_textInfo.characterInfo[m_characterCount].adjustedDescender;
						if (m_textInfo.characterInfo[m_characterCount - 1].character == '\u00ad' && !flag10 && (m_overflowMode == TextOverflowModes.Overflow || num46 < num11 + 0.0001f))
						{
							characterSubstitution.index = m_characterCount - 1;
							characterSubstitution.unicode = 45u;
							i--;
							m_characterCount--;
							continue;
						}
						flag10 = false;
						if (m_textInfo.characterInfo[m_characterCount].character == '\u00ad')
						{
							flag10 = true;
							continue;
						}
						if (m_enableAutoSizing && flag8)
						{
							if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
							{
								float num47 = num41;
								if (m_charWidthAdjDelta > 0f)
								{
									num47 /= 1f - m_charWidthAdjDelta;
								}
								float num48 = num41 - (num12 - 0.0001f) * (flag16 ? 1.05f : 1f);
								m_charWidthAdjDelta += num48 / num47;
								m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100f);
								return;
							}
							if (m_fontSize > m_fontSizeMin && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
							{
								m_maxFontSize = m_fontSize;
								float num49 = Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
								m_fontSize -= num49;
								m_fontSize = Mathf.Max((float)(int)(m_fontSize * 20f + 0.5f) / 20f, m_fontSizeMin);
								return;
							}
						}
						int previous_WordBreak = TMP_Text.m_SavedSoftLineBreakState.previous_WordBreak;
						if (flag8 && previous_WordBreak != -1 && previous_WordBreak != num13)
						{
							i = RestoreWordWrappingState(ref TMP_Text.m_SavedSoftLineBreakState);
							num13 = previous_WordBreak;
							if (m_textInfo.characterInfo[m_characterCount - 1].character == '\u00ad')
							{
								characterSubstitution.index = m_characterCount - 1;
								characterSubstitution.unicode = 45u;
								i--;
								m_characterCount--;
								continue;
							}
						}
						if (!(num46 > num11 + 0.0001f))
						{
							InsertNewLine(i, num2, num3, num4, num34, num25, num12, num8, ref isMaxVisibleDescenderSet, ref maxVisibleDescender);
							flag4 = true;
							flag8 = true;
							continue;
						}
						if (m_firstOverflowCharacterIndex == -1)
						{
							m_firstOverflowCharacterIndex = m_characterCount;
						}
						if (m_enableAutoSizing)
						{
							if (m_lineSpacingDelta > m_lineSpacingMax && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
							{
								float num50 = (num11 - num46) / (float)(m_lineNumber + 1);
								m_lineSpacingDelta = Mathf.Max(m_lineSpacingDelta + num50 / num2, m_lineSpacingMax);
								return;
							}
							if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
							{
								float num51 = num41;
								if (m_charWidthAdjDelta > 0f)
								{
									num51 /= 1f - m_charWidthAdjDelta;
								}
								float num52 = num41 - (num12 - 0.0001f) * (flag16 ? 1.05f : 1f);
								m_charWidthAdjDelta += num52 / num51;
								m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100f);
								return;
							}
							if (m_fontSize > m_fontSizeMin && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
							{
								m_maxFontSize = m_fontSize;
								float num53 = Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
								m_fontSize -= num53;
								m_fontSize = Mathf.Max((float)(int)(m_fontSize * 20f + 0.5f) / 20f, m_fontSizeMin);
								return;
							}
						}
						switch (m_overflowMode)
						{
						case TextOverflowModes.Overflow:
						case TextOverflowModes.Masking:
						case TextOverflowModes.ScrollRect:
							InsertNewLine(i, num2, num3, num4, num34, num25, num12, num8, ref isMaxVisibleDescenderSet, ref maxVisibleDescender);
							flag4 = true;
							flag8 = true;
							continue;
						case TextOverflowModes.Truncate:
							i = RestoreWordWrappingState(ref TMP_Text.m_SavedLastValidState);
							characterSubstitution.index = characterCount;
							characterSubstitution.unicode = 3u;
							continue;
						case TextOverflowModes.Ellipsis:
							if (TMP_Text.m_EllipsisInsertionCandidateStack.Count == 0)
							{
								i = -1;
								m_characterCount = 0;
								characterSubstitution.index = 0;
								characterSubstitution.unicode = 3u;
								m_firstCharacterOfLine = 0;
							}
							else
							{
								WordWrapState state2 = TMP_Text.m_EllipsisInsertionCandidateStack.Pop();
								i = RestoreWordWrappingState(ref state2);
								i--;
								m_characterCount--;
								characterSubstitution.index = m_characterCount;
								characterSubstitution.unicode = 8230u;
								num14++;
							}
							continue;
						case TextOverflowModes.Linked:
							if (m_linkedTextComponent != null)
							{
								m_linkedTextComponent.text = text;
								m_linkedTextComponent.m_inputSource = m_inputSource;
								m_linkedTextComponent.firstVisibleCharacter = m_characterCount;
								m_linkedTextComponent.ForceMeshUpdate();
								m_isTextTruncated = true;
							}
							characterSubstitution.index = m_characterCount;
							characterSubstitution.unicode = 3u;
							continue;
						case TextOverflowModes.Page:
							m_isNewPage = true;
							InsertNewLine(i, num2, num3, num4, num34, num25, num12, num8, ref isMaxVisibleDescenderSet, ref maxVisibleDescender);
							m_startOfLineAscender = 0f;
							m_lineOffset = 0f;
							m_maxTextAscender = 0f;
							m_PageAscender = 0f;
							m_pageNumber++;
							flag4 = true;
							flag8 = true;
							continue;
						}
					}
					else
					{
						if (m_enableAutoSizing && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
						{
							if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f)
							{
								float num54 = num41;
								if (m_charWidthAdjDelta > 0f)
								{
									num54 /= 1f - m_charWidthAdjDelta;
								}
								float num55 = num41 - (num12 - 0.0001f) * (flag16 ? 1.05f : 1f);
								m_charWidthAdjDelta += num55 / num54;
								m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100f);
								return;
							}
							if (m_fontSize > m_fontSizeMin)
							{
								m_maxFontSize = m_fontSize;
								float num56 = Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
								m_fontSize -= num56;
								m_fontSize = Mathf.Max((float)(int)(m_fontSize * 20f + 0.5f) / 20f, m_fontSizeMin);
								return;
							}
						}
						switch (m_overflowMode)
						{
						case TextOverflowModes.Truncate:
							i = RestoreWordWrappingState(ref TMP_Text.m_SavedWordWrapState);
							characterSubstitution.index = characterCount;
							characterSubstitution.unicode = 3u;
							continue;
						case TextOverflowModes.Ellipsis:
							if (TMP_Text.m_EllipsisInsertionCandidateStack.Count == 0)
							{
								i = -1;
								m_characterCount = 0;
								characterSubstitution.index = 0;
								characterSubstitution.unicode = 3u;
								m_firstCharacterOfLine = 0;
							}
							else
							{
								WordWrapState state3 = TMP_Text.m_EllipsisInsertionCandidateStack.Pop();
								i = RestoreWordWrappingState(ref state3);
								i--;
								m_characterCount--;
								characterSubstitution.index = m_characterCount;
								characterSubstitution.unicode = 8230u;
								num14++;
							}
							continue;
						case TextOverflowModes.Linked:
							i = RestoreWordWrappingState(ref TMP_Text.m_SavedWordWrapState);
							if (m_linkedTextComponent != null)
							{
								m_linkedTextComponent.text = text;
								m_linkedTextComponent.m_inputSource = m_inputSource;
								m_linkedTextComponent.firstVisibleCharacter = m_characterCount;
								m_linkedTextComponent.ForceMeshUpdate();
								m_isTextTruncated = true;
							}
							characterSubstitution.index = m_characterCount;
							characterSubstitution.unicode = 3u;
							continue;
						}
					}
				}
				if (flag12)
				{
					m_textInfo.characterInfo[m_characterCount].isVisible = false;
					m_lastVisibleCharacterOfLine = m_characterCount;
					m_lineVisibleSpaceCount = ++m_textInfo.lineInfo[m_lineNumber].spaceCount;
					m_textInfo.lineInfo[m_lineNumber].marginLeft = marginLeft;
					m_textInfo.lineInfo[m_lineNumber].marginRight = marginRight;
					m_textInfo.spaceCount++;
					if (num6 == 160)
					{
						m_textInfo.lineInfo[m_lineNumber].controlCharacterCount++;
					}
				}
				else if (num6 == 173)
				{
					m_textInfo.characterInfo[m_characterCount].isVisible = false;
				}
				else
				{
					Color32 vertexColor = ((!m_overrideHtmlColors) ? m_htmlColor : m_fontColor32);
					if (m_textElementType == TMP_TextElementType.Character)
					{
						SaveGlyphVertexInfo(num7, num33, vertexColor);
					}
					else if (m_textElementType == TMP_TextElementType.Sprite)
					{
						SaveSpriteVertexInfo(vertexColor);
					}
					if (flag4)
					{
						flag4 = false;
						m_firstVisibleCharacterOfLine = m_characterCount;
					}
					m_lineVisibleCharacterCount++;
					m_lastVisibleCharacterOfLine = m_characterCount;
					m_textInfo.lineInfo[m_lineNumber].marginLeft = marginLeft;
					m_textInfo.lineInfo[m_lineNumber].marginRight = marginRight;
				}
			}
			else
			{
				if (m_overflowMode == TextOverflowModes.Linked && (num6 == 10 || num6 == 11))
				{
					float num57 = m_maxTextAscender - (m_maxLineDescender - m_lineOffset) + ((m_lineOffset > 0f && !m_IsDrivenLineSpacing) ? (m_maxLineAscender - m_startOfLineAscender) : 0f);
					int characterCount2 = m_characterCount;
					if (num57 > num11 + 0.0001f)
					{
						if (m_firstOverflowCharacterIndex == -1)
						{
							m_firstOverflowCharacterIndex = m_characterCount;
						}
						i = RestoreWordWrappingState(ref TMP_Text.m_SavedLastValidState);
						if (m_linkedTextComponent != null)
						{
							m_linkedTextComponent.text = text;
							m_linkedTextComponent.m_inputSource = m_inputSource;
							m_linkedTextComponent.firstVisibleCharacter = m_characterCount;
							m_linkedTextComponent.ForceMeshUpdate();
							m_isTextTruncated = true;
						}
						characterSubstitution.index = characterCount2;
						characterSubstitution.unicode = 3u;
						continue;
					}
				}
				if ((num6 == 10 || num6 == 11 || num6 == 160 || num6 == 8199 || num6 == 8232 || num6 == 8233 || char.IsSeparator((char)num6)) && num6 != 173 && num6 != 8203 && num6 != 8288)
				{
					m_textInfo.lineInfo[m_lineNumber].spaceCount++;
					m_textInfo.spaceCount++;
				}
				if (num6 == 160)
				{
					m_textInfo.lineInfo[m_lineNumber].controlCharacterCount++;
				}
			}
			if (m_overflowMode == TextOverflowModes.Ellipsis && (!flag11 || num6 == 45))
			{
				float num58 = m_currentFontSize / m_Ellipsis.fontAsset.m_FaceInfo.pointSize * m_Ellipsis.fontAsset.m_FaceInfo.scale * num * m_fontScaleMultiplier * m_Ellipsis.character.m_Scale * m_Ellipsis.character.m_Glyph.scale;
				float marginLeft2 = m_marginLeft;
				float marginRight2 = m_marginRight;
				if (num6 == 10 && m_characterCount != m_firstCharacterOfLine)
				{
					num58 = m_textInfo.characterInfo[m_characterCount - 1].pointSize / m_Ellipsis.fontAsset.m_FaceInfo.pointSize * m_Ellipsis.fontAsset.m_FaceInfo.scale * num * m_fontScaleMultiplier * m_Ellipsis.character.m_Scale * m_Ellipsis.character.m_Glyph.scale;
					marginLeft2 = m_textInfo.lineInfo[m_lineNumber].marginLeft;
					marginRight2 = m_textInfo.lineInfo[m_lineNumber].marginRight;
				}
				float num59 = m_maxTextAscender - (m_maxLineDescender - m_lineOffset) + ((m_lineOffset > 0f && !m_IsDrivenLineSpacing) ? (m_maxLineAscender - m_startOfLineAscender) : 0f);
				float num60 = Mathf.Abs(m_xAdvance) + ((!m_isRightToLeft) ? m_Ellipsis.character.m_Glyph.metrics.horizontalAdvance : 0f) * (1f - m_charWidthAdjDelta) * num58;
				float num61 = ((m_width != -1f) ? Mathf.Min(num10 + 0.0001f - marginLeft2 - marginRight2, m_width) : (num10 + 0.0001f - marginLeft2 - marginRight2));
				if (num60 < num61 * (flag16 ? 1.05f : 1f) && num59 < num11 + 0.0001f)
				{
					SaveWordWrappingState(ref TMP_Text.m_SavedEllipsisState, i, m_characterCount);
					TMP_Text.m_EllipsisInsertionCandidateStack.Push(TMP_Text.m_SavedEllipsisState);
				}
			}
			m_textInfo.characterInfo[m_characterCount].lineNumber = m_lineNumber;
			m_textInfo.characterInfo[m_characterCount].pageNumber = m_pageNumber;
			if ((num6 != 10 && num6 != 11 && num6 != 13 && !flag11) || m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
			{
				m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;
			}
			if (num6 == 9)
			{
				float num62 = m_currentFontAsset.m_FaceInfo.tabWidth * (float)(int)m_currentFontAsset.tabSize * num3;
				if (m_isRightToLeft)
				{
					float num63 = Mathf.Floor(m_xAdvance / num62) * num62;
					m_xAdvance = ((num63 < m_xAdvance) ? num63 : (m_xAdvance - num62));
				}
				else
				{
					float num64 = Mathf.Ceil(m_xAdvance / num62) * num62;
					m_xAdvance = ((num64 > m_xAdvance) ? num64 : (m_xAdvance + num62));
				}
			}
			else if (m_monoSpacing != 0f)
			{
				float num65 = ((!m_duoSpace || (num6 != 46 && num6 != 58 && num6 != 44)) ? (m_monoSpacing - num32) : (m_monoSpacing / 2f - num32));
				m_xAdvance += (num65 + (m_currentFontAsset.normalSpacingOffset + num25) * num4 + m_cSpacing) * (1f - m_charWidthAdjDelta);
				if (flag12 || num6 == 8203)
				{
					m_xAdvance += m_wordSpacing * num4;
				}
			}
			else if (m_isRightToLeft)
			{
				m_xAdvance -= (glyphValueRecord.xAdvance * num3 + (m_currentFontAsset.normalSpacingOffset + num25 + num34) * num4 + m_cSpacing) * (1f - m_charWidthAdjDelta);
				if (flag12 || num6 == 8203)
				{
					m_xAdvance -= m_wordSpacing * num4;
				}
			}
			else
			{
				m_xAdvance += ((glyphMetrics.horizontalAdvance * m_FXScale.x + glyphValueRecord.xAdvance) * num3 + (m_currentFontAsset.normalSpacingOffset + num25 + num34) * num4 + m_cSpacing) * (1f - m_charWidthAdjDelta);
				if (flag12 || num6 == 8203)
				{
					m_xAdvance += m_wordSpacing * num4;
				}
			}
			m_textInfo.characterInfo[m_characterCount].xAdvance = m_xAdvance;
			if (num6 == 13)
			{
				m_xAdvance = 0f + tag_Indent;
			}
			if (m_overflowMode == TextOverflowModes.Page && num6 != 10 && num6 != 11 && num6 != 13 && num6 != 8232 && num6 != 8233)
			{
				if (m_pageNumber + 1 > m_textInfo.pageInfo.Length)
				{
					TMP_TextInfo.Resize(ref m_textInfo.pageInfo, m_pageNumber + 1, isBlockAllocated: true);
				}
				m_textInfo.pageInfo[m_pageNumber].ascender = m_PageAscender;
				m_textInfo.pageInfo[m_pageNumber].descender = ((m_ElementDescender < m_textInfo.pageInfo[m_pageNumber].descender) ? m_ElementDescender : m_textInfo.pageInfo[m_pageNumber].descender);
				if (m_isNewPage)
				{
					m_isNewPage = false;
					m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
				}
				m_textInfo.pageInfo[m_pageNumber].lastCharacterIndex = m_characterCount;
			}
			if (num6 == 10 || num6 == 11 || num6 == 3 || num6 == 8232 || num6 == 8233 || (num6 == 45 && flag11) || m_characterCount == totalCharacterCount - 1)
			{
				float num66 = m_maxLineAscender - m_startOfLineAscender;
				if (m_lineOffset > 0f && Math.Abs(num66) > 0.01f && !m_IsDrivenLineSpacing && !m_isNewPage)
				{
					AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, num66);
					m_ElementDescender -= num66;
					m_lineOffset += num66;
					if (TMP_Text.m_SavedEllipsisState.lineNumber == m_lineNumber)
					{
						TMP_Text.m_SavedEllipsisState = TMP_Text.m_EllipsisInsertionCandidateStack.Pop();
						TMP_Text.m_SavedEllipsisState.startOfLineAscender += num66;
						TMP_Text.m_SavedEllipsisState.lineOffset += num66;
						TMP_Text.m_EllipsisInsertionCandidateStack.Push(TMP_Text.m_SavedEllipsisState);
					}
				}
				m_isNewPage = false;
				float num67 = m_maxLineAscender - m_lineOffset;
				float num68 = m_maxLineDescender - m_lineOffset;
				m_ElementDescender = ((m_ElementDescender < num68) ? m_ElementDescender : num68);
				if (!isMaxVisibleDescenderSet)
				{
					maxVisibleDescender = m_ElementDescender;
				}
				if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
				{
					isMaxVisibleDescenderSet = true;
				}
				m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
				m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = (m_firstVisibleCharacterOfLine = ((m_firstCharacterOfLine > m_firstVisibleCharacterOfLine) ? m_firstCharacterOfLine : m_firstVisibleCharacterOfLine));
				m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = (m_lastCharacterOfLine = m_characterCount);
				m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = (m_lastVisibleCharacterOfLine = ((m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine) ? m_firstVisibleCharacterOfLine : m_lastVisibleCharacterOfLine));
				m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;
				m_textInfo.lineInfo[m_lineNumber].visibleCharacterCount = m_lineVisibleCharacterCount;
				m_textInfo.lineInfo[m_lineNumber].visibleSpaceCount = m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex + 1 - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex - m_lineVisibleCharacterCount;
				m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, num68);
				m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, num67);
				m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x - num7 * num3;
				m_textInfo.lineInfo[m_lineNumber].width = num12;
				if (m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
				{
					m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;
				}
				float num69 = ((m_currentFontAsset.normalSpacingOffset + num25 + num34) * num4 + m_cSpacing) * (1f - m_charWidthAdjDelta);
				if (m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].isVisible)
				{
					m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance + (m_isRightToLeft ? num69 : (0f - num69));
				}
				else
				{
					m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastCharacterOfLine].xAdvance + (m_isRightToLeft ? num69 : (0f - num69));
				}
				m_textInfo.lineInfo[m_lineNumber].baseline = 0f - m_lineOffset;
				m_textInfo.lineInfo[m_lineNumber].ascender = num67;
				m_textInfo.lineInfo[m_lineNumber].descender = num68;
				m_textInfo.lineInfo[m_lineNumber].lineHeight = num67 - num68 + num8 * num2;
				if (num6 == 10 || num6 == 11 || (num6 == 45 && flag11) || num6 == 8232 || num6 == 8233)
				{
					SaveWordWrappingState(ref TMP_Text.m_SavedLineState, i, m_characterCount);
					m_lineNumber++;
					flag4 = true;
					flag9 = false;
					flag8 = true;
					m_firstCharacterOfLine = m_characterCount + 1;
					m_lineVisibleCharacterCount = 0;
					m_lineVisibleSpaceCount = 0;
					if (m_lineNumber >= m_textInfo.lineInfo.Length)
					{
						ResizeLineExtents(m_lineNumber);
					}
					float adjustedAscender2 = m_textInfo.characterInfo[m_characterCount].adjustedAscender;
					if (m_lineHeight == -32767f)
					{
						float num70 = 0f - m_maxLineDescender + adjustedAscender2 + (num8 + m_lineSpacingDelta) * num2 + (m_lineSpacing + ((num6 == 10 || num6 == 8233) ? m_paragraphSpacing : 0f)) * num4;
						m_lineOffset += num70;
						m_IsDrivenLineSpacing = false;
					}
					else
					{
						m_lineOffset += m_lineHeight + (m_lineSpacing + ((num6 == 10 || num6 == 8233) ? m_paragraphSpacing : 0f)) * num4;
						m_IsDrivenLineSpacing = true;
					}
					m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
					m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
					m_startOfLineAscender = adjustedAscender2;
					m_xAdvance = 0f + tag_LineIndent + tag_Indent;
					SaveWordWrappingState(ref TMP_Text.m_SavedWordWrapState, i, m_characterCount);
					SaveWordWrappingState(ref TMP_Text.m_SavedLastValidState, i, m_characterCount);
					m_characterCount++;
					continue;
				}
				if (num6 == 3)
				{
					i = m_TextProcessingArray.Length;
				}
			}
			if (m_textInfo.characterInfo[m_characterCount].isVisible)
			{
				m_meshExtents.min.x = Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[m_characterCount].bottomLeft.x);
				m_meshExtents.min.y = Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[m_characterCount].bottomLeft.y);
				m_meshExtents.max.x = Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[m_characterCount].topRight.x);
				m_meshExtents.max.y = Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[m_characterCount].topRight.y);
			}
			if ((m_TextWrappingMode != 0 && m_TextWrappingMode != TextWrappingModes.PreserveWhitespaceNoWrap) || m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis || m_overflowMode == TextOverflowModes.Linked)
			{
				bool flag17 = false;
				bool flag18 = false;
				uint num71 = ((m_characterCount + 1 < totalCharacterCount) ? m_textInfo.characterInfo[m_characterCount + 1].character : '\0');
				if ((flag12 || num6 == 8203 || num6 == 45 || num6 == 173) && (!m_isNonBreakingSpace || flag9) && num6 != 160 && num6 != 8199 && num6 != 8209 && num6 != 8239 && num6 != 8288)
				{
					if (num6 != 45 || m_characterCount <= 0 || !char.IsWhiteSpace(m_textInfo.characterInfo[m_characterCount - 1].character) || m_textInfo.characterInfo[m_characterCount - 1].lineNumber != m_lineNumber)
					{
						flag8 = false;
						flag17 = true;
						TMP_Text.m_SavedSoftLineBreakState.previous_WordBreak = -1;
					}
				}
				else if (!m_isNonBreakingSpace && ((TMP_TextParsingUtilities.IsHangul(num6) && !TMP_Settings.useModernHangulLineBreakingRules) || TMP_TextParsingUtilities.IsCJK(num6)))
				{
					bool num72 = TMP_Settings.linebreakingRules.leadingCharacters.Contains(num6);
					bool flag19 = m_characterCount < totalCharacterCount - 1 && TMP_Settings.linebreakingRules.followingCharacters.Contains(num71);
					if (!num72)
					{
						if (!flag19)
						{
							flag8 = false;
							flag17 = true;
						}
						if (flag8)
						{
							if (flag12)
							{
								flag18 = true;
							}
							flag17 = true;
						}
					}
					else if (flag8 && flag15)
					{
						if (flag12)
						{
							flag18 = true;
						}
						flag17 = true;
					}
				}
				else if (!m_isNonBreakingSpace && TMP_TextParsingUtilities.IsCJK(num71) && !TMP_Settings.linebreakingRules.followingCharacters.Contains(num71))
				{
					flag17 = true;
				}
				else if (flag8)
				{
					if ((flag12 && num6 != 160) || (num6 == 173 && !flag10))
					{
						flag18 = true;
					}
					flag17 = true;
				}
				if (flag17)
				{
					SaveWordWrappingState(ref TMP_Text.m_SavedWordWrapState, i, m_characterCount);
				}
				if (flag18)
				{
					SaveWordWrappingState(ref TMP_Text.m_SavedSoftLineBreakState, i, m_characterCount);
				}
			}
			SaveWordWrappingState(ref TMP_Text.m_SavedLastValidState, i, m_characterCount);
			m_characterCount++;
		}
		num5 = m_maxFontSize - m_minFontSize;
		if (m_enableAutoSizing && num5 > 0.051f && m_fontSize < m_fontSizeMax && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
		{
			if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f)
			{
				m_charWidthAdjDelta = 0f;
			}
			m_minFontSize = m_fontSize;
			float num73 = Mathf.Max((m_maxFontSize - m_fontSize) / 2f, 0.05f);
			m_fontSize += num73;
			m_fontSize = Mathf.Min((float)(int)(m_fontSize * 20f + 0.5f) / 20f, m_fontSizeMax);
			return;
		}
		m_IsAutoSizePointSizeSet = true;
		if (m_AutoSizeIterationCount >= m_AutoSizeMaxIterationCount)
		{
			Debug.Log("Auto Size Iteration Count: " + m_AutoSizeIterationCount + ". Final Point Size: " + m_fontSize);
		}
		if (m_characterCount == 0 || (m_characterCount == 1 && num6 == 3))
		{
			ClearMesh();
			TMPro_EventManager.ON_TEXT_CHANGED(this);
			return;
		}
		int index4 = TMP_Text.m_materialReferences[m_Underline.materialIndex].referenceCount * 4;
		m_textInfo.meshInfo[0].Clear(uploadChanges: false);
		Vector3 vector9 = Vector3.zero;
		Vector3[] rectTransformCorners = m_RectTransformCorners;
		switch (m_VerticalAlignment)
		{
		case VerticalAlignmentOptions.Top:
			vector9 = ((m_overflowMode == TextOverflowModes.Page) ? (rectTransformCorners[1] + new Vector3(0f + vector.x, 0f - m_textInfo.pageInfo[num9].ascender - vector.y, 0f)) : (rectTransformCorners[1] + new Vector3(0f + vector.x, 0f - m_maxTextAscender - vector.y, 0f)));
			break;
		case VerticalAlignmentOptions.Middle:
			vector9 = ((m_overflowMode == TextOverflowModes.Page) ? ((rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + vector.x, 0f - (m_textInfo.pageInfo[num9].ascender + vector.y + m_textInfo.pageInfo[num9].descender - vector.w) / 2f, 0f)) : ((rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + vector.x, 0f - (m_maxTextAscender + vector.y + maxVisibleDescender - vector.w) / 2f, 0f)));
			break;
		case VerticalAlignmentOptions.Bottom:
			vector9 = ((m_overflowMode == TextOverflowModes.Page) ? (rectTransformCorners[0] + new Vector3(0f + vector.x, 0f - m_textInfo.pageInfo[num9].descender + vector.w, 0f)) : (rectTransformCorners[0] + new Vector3(0f + vector.x, 0f - maxVisibleDescender + vector.w, 0f)));
			break;
		case VerticalAlignmentOptions.Baseline:
			vector9 = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + vector.x, 0f, 0f);
			break;
		case VerticalAlignmentOptions.Geometry:
			vector9 = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + vector.x, 0f - (m_meshExtents.max.y + vector.y + m_meshExtents.min.y - vector.w) / 2f, 0f);
			break;
		case VerticalAlignmentOptions.Capline:
			vector9 = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + vector.x, 0f - (m_maxCapHeight - vector.y - vector.w) / 2f, 0f);
			break;
		}
		Vector3 vector10 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		int num74 = 0;
		int lineCount = 0;
		int num75 = 0;
		bool flag20 = false;
		bool flag21 = false;
		int num76 = 0;
		int num77 = 0;
		bool flag22 = !(m_canvas.worldCamera == null);
		float f = (m_previousLossyScaleY = base.transform.lossyScale.y);
		RenderMode renderMode = m_canvas.renderMode;
		float scaleFactor = m_canvas.scaleFactor;
		Color32 color = Color.white;
		Color32 underlineColor = Color.white;
		HighlightState highlightState = new HighlightState(new Color32(byte.MaxValue, byte.MaxValue, 0, 64), TMP_Offset.zero);
		float num78 = 0f;
		float num79 = 0f;
		float num80 = 0f;
		float num81 = 0f;
		float num82 = 0f;
		float num83 = TMP_Text.k_LargePositiveFloat;
		int num84 = 0;
		float num85 = 0f;
		float num86 = 0f;
		float b = 0f;
		TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
		for (int j = 0; j < m_characterCount; j++)
		{
			TMP_FontAsset fontAsset = characterInfo[j].fontAsset;
			char character = characterInfo[j].character;
			bool flag23 = char.IsWhiteSpace(character);
			int lineNumber = characterInfo[j].lineNumber;
			TMP_LineInfo tMP_LineInfo = m_textInfo.lineInfo[lineNumber];
			lineCount = lineNumber + 1;
			HorizontalAlignmentOptions horizontalAlignmentOptions = tMP_LineInfo.alignment;
			switch (horizontalAlignmentOptions)
			{
			case HorizontalAlignmentOptions.Left:
				vector10 = (m_isRightToLeft ? new Vector3(0f - tMP_LineInfo.maxAdvance, 0f, 0f) : new Vector3(0f + tMP_LineInfo.marginLeft, 0f, 0f));
				break;
			case HorizontalAlignmentOptions.Center:
				vector10 = new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width / 2f - tMP_LineInfo.maxAdvance / 2f, 0f, 0f);
				break;
			case HorizontalAlignmentOptions.Geometry:
				vector10 = new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width / 2f - (tMP_LineInfo.lineExtents.min.x + tMP_LineInfo.lineExtents.max.x) / 2f, 0f, 0f);
				break;
			case HorizontalAlignmentOptions.Right:
				vector10 = (m_isRightToLeft ? new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width, 0f, 0f) : new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width - tMP_LineInfo.maxAdvance, 0f, 0f));
				break;
			case HorizontalAlignmentOptions.Justified:
			case HorizontalAlignmentOptions.Flush:
			{
				if (j > tMP_LineInfo.lastVisibleCharacterIndex || character == '\n' || character == '\u00ad' || character == '\u200b' || character == '\u2060' || character == '\u0003')
				{
					break;
				}
				char character2 = characterInfo[tMP_LineInfo.lastCharacterIndex].character;
				bool flag24 = (horizontalAlignmentOptions & HorizontalAlignmentOptions.Flush) == HorizontalAlignmentOptions.Flush;
				if ((!char.IsControl(character2) && lineNumber < m_lineNumber) || flag24 || tMP_LineInfo.maxAdvance > tMP_LineInfo.width)
				{
					if (lineNumber != num75 || j == 0 || j == m_firstVisibleCharacter)
					{
						vector10 = (m_isRightToLeft ? new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width, 0f, 0f) : new Vector3(tMP_LineInfo.marginLeft, 0f, 0f));
						flag20 = (char.IsSeparator(character) ? true : false);
						break;
					}
					float num87 = ((!m_isRightToLeft) ? (tMP_LineInfo.width - tMP_LineInfo.maxAdvance) : (tMP_LineInfo.width + tMP_LineInfo.maxAdvance));
					int num88 = tMP_LineInfo.visibleCharacterCount - 1 + tMP_LineInfo.controlCharacterCount;
					int num89 = tMP_LineInfo.visibleSpaceCount - tMP_LineInfo.controlCharacterCount;
					if (flag20)
					{
						num89--;
						num88++;
					}
					float num90 = ((num89 > 0) ? m_wordWrappingRatios : 1f);
					if (num89 < 1)
					{
						num89 = 1;
					}
					if (character != '\u00a0' && (character == '\t' || char.IsSeparator(character)))
					{
						if (!m_isRightToLeft)
						{
							vector10 += new Vector3(num87 * (1f - num90) / (float)num89, 0f, 0f);
						}
						else
						{
							vector10 -= new Vector3(num87 * (1f - num90) / (float)num89, 0f, 0f);
						}
					}
					else if (!m_isRightToLeft)
					{
						vector10 += new Vector3(num87 * num90 / (float)num88, 0f, 0f);
					}
					else
					{
						vector10 -= new Vector3(num87 * num90 / (float)num88, 0f, 0f);
					}
				}
				else
				{
					vector10 = (m_isRightToLeft ? new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width, 0f, 0f) : new Vector3(tMP_LineInfo.marginLeft, 0f, 0f));
				}
				break;
			}
			}
			zero3 = vector9 + vector10;
			if (characterInfo[j].isVisible)
			{
				TMP_TextElementType elementType = characterInfo[j].elementType;
				switch (elementType)
				{
				case TMP_TextElementType.Character:
				{
					Extents lineExtents = tMP_LineInfo.lineExtents;
					float num91 = m_uvLineOffset * (float)lineNumber % 1f;
					switch (m_horizontalMapping)
					{
					case TextureMappingOptions.Character:
						characterInfo[j].vertex_BL.uv2.x = 0f;
						characterInfo[j].vertex_TL.uv2.x = 0f;
						characterInfo[j].vertex_TR.uv2.x = 1f;
						characterInfo[j].vertex_BR.uv2.x = 1f;
						break;
					case TextureMappingOptions.Line:
						if (m_textAlignment != TextAlignmentOptions.Justified)
						{
							characterInfo[j].vertex_BL.uv2.x = (characterInfo[j].vertex_BL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num91;
							characterInfo[j].vertex_TL.uv2.x = (characterInfo[j].vertex_TL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num91;
							characterInfo[j].vertex_TR.uv2.x = (characterInfo[j].vertex_TR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num91;
							characterInfo[j].vertex_BR.uv2.x = (characterInfo[j].vertex_BR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num91;
						}
						else
						{
							characterInfo[j].vertex_BL.uv2.x = (characterInfo[j].vertex_BL.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
							characterInfo[j].vertex_TL.uv2.x = (characterInfo[j].vertex_TL.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
							characterInfo[j].vertex_TR.uv2.x = (characterInfo[j].vertex_TR.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
							characterInfo[j].vertex_BR.uv2.x = (characterInfo[j].vertex_BR.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
						}
						break;
					case TextureMappingOptions.Paragraph:
						characterInfo[j].vertex_BL.uv2.x = (characterInfo[j].vertex_BL.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
						characterInfo[j].vertex_TL.uv2.x = (characterInfo[j].vertex_TL.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
						characterInfo[j].vertex_TR.uv2.x = (characterInfo[j].vertex_TR.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
						characterInfo[j].vertex_BR.uv2.x = (characterInfo[j].vertex_BR.position.x + vector10.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num91;
						break;
					case TextureMappingOptions.MatchAspect:
					{
						switch (m_verticalMapping)
						{
						case TextureMappingOptions.Character:
							characterInfo[j].vertex_BL.uv2.y = 0f;
							characterInfo[j].vertex_TL.uv2.y = 1f;
							characterInfo[j].vertex_TR.uv2.y = 0f;
							characterInfo[j].vertex_BR.uv2.y = 1f;
							break;
						case TextureMappingOptions.Line:
							characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + num91;
							characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + num91;
							characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
							characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
							break;
						case TextureMappingOptions.Paragraph:
							characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + num91;
							characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + num91;
							characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
							characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
							break;
						case TextureMappingOptions.MatchAspect:
							Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
							break;
						}
						float num92 = (1f - (characterInfo[j].vertex_BL.uv2.y + characterInfo[j].vertex_TL.uv2.y) * characterInfo[j].aspectRatio) / 2f;
						characterInfo[j].vertex_BL.uv2.x = characterInfo[j].vertex_BL.uv2.y * characterInfo[j].aspectRatio + num92 + num91;
						characterInfo[j].vertex_TL.uv2.x = characterInfo[j].vertex_BL.uv2.x;
						characterInfo[j].vertex_TR.uv2.x = characterInfo[j].vertex_TL.uv2.y * characterInfo[j].aspectRatio + num92 + num91;
						characterInfo[j].vertex_BR.uv2.x = characterInfo[j].vertex_TR.uv2.x;
						break;
					}
					}
					switch (m_verticalMapping)
					{
					case TextureMappingOptions.Character:
						characterInfo[j].vertex_BL.uv2.y = 0f;
						characterInfo[j].vertex_TL.uv2.y = 1f;
						characterInfo[j].vertex_TR.uv2.y = 1f;
						characterInfo[j].vertex_BR.uv2.y = 0f;
						break;
					case TextureMappingOptions.Line:
						characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - tMP_LineInfo.descender) / (tMP_LineInfo.ascender - tMP_LineInfo.descender);
						characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - tMP_LineInfo.descender) / (tMP_LineInfo.ascender - tMP_LineInfo.descender);
						characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
						characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
						break;
					case TextureMappingOptions.Paragraph:
						characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y);
						characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y);
						characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
						characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
						break;
					case TextureMappingOptions.MatchAspect:
					{
						float num93 = (1f - (characterInfo[j].vertex_BL.uv2.x + characterInfo[j].vertex_TR.uv2.x) / characterInfo[j].aspectRatio) / 2f;
						characterInfo[j].vertex_BL.uv2.y = num93 + characterInfo[j].vertex_BL.uv2.x / characterInfo[j].aspectRatio;
						characterInfo[j].vertex_TL.uv2.y = num93 + characterInfo[j].vertex_TR.uv2.x / characterInfo[j].aspectRatio;
						characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
						characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
						break;
					}
					}
					num78 = characterInfo[j].scale * (1f - m_charWidthAdjDelta);
					if (!characterInfo[j].isUsingAlternateTypeface && (characterInfo[j].style & FontStyles.Bold) == FontStyles.Bold)
					{
						num78 *= -1f;
					}
					switch (renderMode)
					{
					case RenderMode.ScreenSpaceOverlay:
						num78 *= Mathf.Abs(f) / scaleFactor;
						break;
					case RenderMode.ScreenSpaceCamera:
						num78 *= (flag22 ? Mathf.Abs(f) : 1f);
						break;
					case RenderMode.WorldSpace:
						num78 *= Mathf.Abs(f);
						break;
					}
					characterInfo[j].vertex_BL.uv.w = num78;
					characterInfo[j].vertex_TL.uv.w = num78;
					characterInfo[j].vertex_TR.uv.w = num78;
					characterInfo[j].vertex_BR.uv.w = num78;
					break;
				}
				}
				if (j < m_maxVisibleCharacters && num74 < m_maxVisibleWords && lineNumber < m_maxVisibleLines && m_overflowMode != TextOverflowModes.Page)
				{
					characterInfo[j].vertex_BL.position += zero3;
					characterInfo[j].vertex_TL.position += zero3;
					characterInfo[j].vertex_TR.position += zero3;
					characterInfo[j].vertex_BR.position += zero3;
				}
				else if (j < m_maxVisibleCharacters && num74 < m_maxVisibleWords && lineNumber < m_maxVisibleLines && m_overflowMode == TextOverflowModes.Page && characterInfo[j].pageNumber == num9)
				{
					characterInfo[j].vertex_BL.position += zero3;
					characterInfo[j].vertex_TL.position += zero3;
					characterInfo[j].vertex_TR.position += zero3;
					characterInfo[j].vertex_BR.position += zero3;
				}
				else
				{
					characterInfo[j].vertex_BL.position = Vector3.zero;
					characterInfo[j].vertex_TL.position = Vector3.zero;
					characterInfo[j].vertex_TR.position = Vector3.zero;
					characterInfo[j].vertex_BR.position = Vector3.zero;
					characterInfo[j].isVisible = false;
				}
				switch (elementType)
				{
				case TMP_TextElementType.Character:
					FillCharacterVertexBuffers(j);
					break;
				case TMP_TextElementType.Sprite:
					FillSpriteVertexBuffers(j);
					break;
				}
			}
			m_textInfo.characterInfo[j].bottomLeft += zero3;
			m_textInfo.characterInfo[j].topLeft += zero3;
			m_textInfo.characterInfo[j].topRight += zero3;
			m_textInfo.characterInfo[j].bottomRight += zero3;
			m_textInfo.characterInfo[j].origin += zero3.x;
			m_textInfo.characterInfo[j].xAdvance += zero3.x;
			m_textInfo.characterInfo[j].ascender += zero3.y;
			m_textInfo.characterInfo[j].descender += zero3.y;
			m_textInfo.characterInfo[j].baseLine += zero3.y;
			if (lineNumber != num75 || j == m_characterCount - 1)
			{
				if (lineNumber != num75)
				{
					m_textInfo.lineInfo[num75].baseline += zero3.y;
					m_textInfo.lineInfo[num75].ascender += zero3.y;
					m_textInfo.lineInfo[num75].descender += zero3.y;
					m_textInfo.lineInfo[num75].maxAdvance += zero3.x;
					m_textInfo.lineInfo[num75].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[num75].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[num75].descender);
					m_textInfo.lineInfo[num75].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[num75].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[num75].ascender);
				}
				if (j == m_characterCount - 1)
				{
					m_textInfo.lineInfo[lineNumber].baseline += zero3.y;
					m_textInfo.lineInfo[lineNumber].ascender += zero3.y;
					m_textInfo.lineInfo[lineNumber].descender += zero3.y;
					m_textInfo.lineInfo[lineNumber].maxAdvance += zero3.x;
					m_textInfo.lineInfo[lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lineNumber].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[lineNumber].descender);
					m_textInfo.lineInfo[lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lineNumber].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[lineNumber].ascender);
				}
			}
			if (char.IsLetterOrDigit(character) || character == '-' || character == '\u00ad' || character == '‐' || character == '‑')
			{
				if (!flag21)
				{
					flag21 = true;
					num76 = j;
				}
				if (flag21 && j == m_characterCount - 1)
				{
					int num94 = m_textInfo.wordInfo.Length;
					int wordCount = m_textInfo.wordCount;
					if (m_textInfo.wordCount + 1 > num94)
					{
						TMP_TextInfo.Resize(ref m_textInfo.wordInfo, num94 + 1);
					}
					num77 = j;
					m_textInfo.wordInfo[wordCount].firstCharacterIndex = num76;
					m_textInfo.wordInfo[wordCount].lastCharacterIndex = num77;
					m_textInfo.wordInfo[wordCount].characterCount = num77 - num76 + 1;
					m_textInfo.wordInfo[wordCount].textComponent = this;
					num74++;
					m_textInfo.wordCount++;
					m_textInfo.lineInfo[lineNumber].wordCount++;
				}
			}
			else if ((flag21 || (j == 0 && (!char.IsPunctuation(character) || flag23 || character == '\u200b' || j == m_characterCount - 1))) && (j <= 0 || j >= characterInfo.Length - 1 || j >= m_characterCount || (character != '\'' && character != '’') || !char.IsLetterOrDigit(characterInfo[j - 1].character) || !char.IsLetterOrDigit(characterInfo[j + 1].character)))
			{
				num77 = ((j == m_characterCount - 1 && char.IsLetterOrDigit(character)) ? j : (j - 1));
				flag21 = false;
				int num95 = m_textInfo.wordInfo.Length;
				int wordCount2 = m_textInfo.wordCount;
				if (m_textInfo.wordCount + 1 > num95)
				{
					TMP_TextInfo.Resize(ref m_textInfo.wordInfo, num95 + 1);
				}
				m_textInfo.wordInfo[wordCount2].firstCharacterIndex = num76;
				m_textInfo.wordInfo[wordCount2].lastCharacterIndex = num77;
				m_textInfo.wordInfo[wordCount2].characterCount = num77 - num76 + 1;
				m_textInfo.wordInfo[wordCount2].textComponent = this;
				num74++;
				m_textInfo.wordCount++;
				m_textInfo.lineInfo[lineNumber].wordCount++;
			}
			if ((m_textInfo.characterInfo[j].style & FontStyles.Underline) == FontStyles.Underline)
			{
				bool flag25 = true;
				int pageNumber = m_textInfo.characterInfo[j].pageNumber;
				m_textInfo.characterInfo[j].underlineVertexIndex = index4;
				if (j > m_maxVisibleCharacters || lineNumber > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && pageNumber + 1 != m_pageToDisplay))
				{
					flag25 = false;
				}
				if (!flag23 && character != '\u200b')
				{
					num82 = Mathf.Max(num82, m_textInfo.characterInfo[j].scale);
					num79 = Mathf.Max(num79, Mathf.Abs(num78));
					num83 = Mathf.Min((pageNumber == num84) ? num83 : TMP_Text.k_LargePositiveFloat, m_textInfo.characterInfo[j].baseLine + base.font.m_FaceInfo.underlineOffset * num82);
					num84 = pageNumber;
				}
				if (!flag && flag25 && j <= tMP_LineInfo.lastVisibleCharacterIndex && character != '\n' && character != '\v' && character != '\r' && (j != tMP_LineInfo.lastVisibleCharacterIndex || !char.IsSeparator(character)))
				{
					flag = true;
					num80 = m_textInfo.characterInfo[j].scale;
					if (num82 == 0f)
					{
						num82 = num80;
						num79 = num78;
					}
					start = new Vector3(m_textInfo.characterInfo[j].bottomLeft.x, num83, 0f);
					color = m_textInfo.characterInfo[j].underlineColor;
				}
				if (flag && m_characterCount == 1)
				{
					flag = false;
					zero = new Vector3(m_textInfo.characterInfo[j].topRight.x, num83, 0f);
					num81 = m_textInfo.characterInfo[j].scale;
					DrawUnderlineMesh(start, zero, ref index4, num80, num81, num82, num79, color);
					num82 = 0f;
					num79 = 0f;
					num83 = TMP_Text.k_LargePositiveFloat;
				}
				else if (flag && (j == tMP_LineInfo.lastCharacterIndex || j >= tMP_LineInfo.lastVisibleCharacterIndex))
				{
					if (flag23 || character == '\u200b')
					{
						int lastVisibleCharacterIndex = tMP_LineInfo.lastVisibleCharacterIndex;
						zero = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, num83, 0f);
						num81 = m_textInfo.characterInfo[lastVisibleCharacterIndex].scale;
					}
					else
					{
						zero = new Vector3(m_textInfo.characterInfo[j].topRight.x, num83, 0f);
						num81 = m_textInfo.characterInfo[j].scale;
					}
					flag = false;
					DrawUnderlineMesh(start, zero, ref index4, num80, num81, num82, num79, color);
					num82 = 0f;
					num79 = 0f;
					num83 = TMP_Text.k_LargePositiveFloat;
				}
				else if (flag && !flag25)
				{
					flag = false;
					zero = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, num83, 0f);
					num81 = m_textInfo.characterInfo[j - 1].scale;
					DrawUnderlineMesh(start, zero, ref index4, num80, num81, num82, num79, color);
					num82 = 0f;
					num79 = 0f;
					num83 = TMP_Text.k_LargePositiveFloat;
				}
				else if (flag && j < m_characterCount - 1 && !color.Compare(m_textInfo.characterInfo[j + 1].underlineColor))
				{
					flag = false;
					zero = new Vector3(m_textInfo.characterInfo[j].topRight.x, num83, 0f);
					num81 = m_textInfo.characterInfo[j].scale;
					DrawUnderlineMesh(start, zero, ref index4, num80, num81, num82, num79, color);
					num82 = 0f;
					num79 = 0f;
					num83 = TMP_Text.k_LargePositiveFloat;
				}
			}
			else if (flag)
			{
				flag = false;
				zero = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, num83, 0f);
				num81 = m_textInfo.characterInfo[j - 1].scale;
				DrawUnderlineMesh(start, zero, ref index4, num80, num81, num82, num79, color);
				num82 = 0f;
				num79 = 0f;
				num83 = TMP_Text.k_LargePositiveFloat;
			}
			bool num96 = (m_textInfo.characterInfo[j].style & FontStyles.Strikethrough) == FontStyles.Strikethrough;
			float strikethroughOffset = fontAsset.m_FaceInfo.strikethroughOffset;
			if (num96)
			{
				bool flag26 = true;
				m_textInfo.characterInfo[j].strikethroughVertexIndex = index4;
				if (j > m_maxVisibleCharacters || lineNumber > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && m_textInfo.characterInfo[j].pageNumber + 1 != m_pageToDisplay))
				{
					flag26 = false;
				}
				if (!flag2 && flag26 && j <= tMP_LineInfo.lastVisibleCharacterIndex && character != '\n' && character != '\v' && character != '\r' && (j != tMP_LineInfo.lastVisibleCharacterIndex || !char.IsSeparator(character)))
				{
					flag2 = true;
					num85 = m_textInfo.characterInfo[j].pointSize;
					num86 = m_textInfo.characterInfo[j].scale;
					start2 = new Vector3(m_textInfo.characterInfo[j].bottomLeft.x, m_textInfo.characterInfo[j].baseLine + strikethroughOffset * num86, 0f);
					underlineColor = m_textInfo.characterInfo[j].strikethroughColor;
					b = m_textInfo.characterInfo[j].baseLine;
				}
				if (flag2 && m_characterCount == 1)
				{
					flag2 = false;
					zero2 = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethroughOffset * num86, 0f);
					DrawUnderlineMesh(start2, zero2, ref index4, num86, num86, num86, num78, underlineColor);
				}
				else if (flag2 && j == tMP_LineInfo.lastCharacterIndex)
				{
					if (flag23 || character == '\u200b')
					{
						int lastVisibleCharacterIndex2 = tMP_LineInfo.lastVisibleCharacterIndex;
						zero2 = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex2].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex2].baseLine + strikethroughOffset * num86, 0f);
					}
					else
					{
						zero2 = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethroughOffset * num86, 0f);
					}
					flag2 = false;
					DrawUnderlineMesh(start2, zero2, ref index4, num86, num86, num86, num78, underlineColor);
				}
				else if (flag2 && j < m_characterCount && (m_textInfo.characterInfo[j + 1].pointSize != num85 || !TMP_Math.Approximately(m_textInfo.characterInfo[j + 1].baseLine + zero3.y, b)))
				{
					flag2 = false;
					int lastVisibleCharacterIndex3 = tMP_LineInfo.lastVisibleCharacterIndex;
					zero2 = ((j <= lastVisibleCharacterIndex3) ? new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethroughOffset * num86, 0f) : new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex3].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex3].baseLine + strikethroughOffset * num86, 0f));
					DrawUnderlineMesh(start2, zero2, ref index4, num86, num86, num86, num78, underlineColor);
				}
				else if (flag2 && j < m_characterCount && fontAsset.GetInstanceID() != characterInfo[j + 1].fontAsset.GetInstanceID())
				{
					flag2 = false;
					zero2 = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethroughOffset * num86, 0f);
					DrawUnderlineMesh(start2, zero2, ref index4, num86, num86, num86, num78, underlineColor);
				}
				else if (flag2 && !flag26)
				{
					flag2 = false;
					zero2 = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, m_textInfo.characterInfo[j - 1].baseLine + strikethroughOffset * num86, 0f);
					DrawUnderlineMesh(start2, zero2, ref index4, num86, num86, num86, num78, underlineColor);
				}
			}
			else if (flag2)
			{
				flag2 = false;
				zero2 = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, m_textInfo.characterInfo[j - 1].baseLine + strikethroughOffset * num86, 0f);
				DrawUnderlineMesh(start2, zero2, ref index4, num86, num86, num86, num78, underlineColor);
			}
			if ((m_textInfo.characterInfo[j].style & FontStyles.Highlight) == FontStyles.Highlight)
			{
				bool flag27 = true;
				int pageNumber2 = m_textInfo.characterInfo[j].pageNumber;
				if (j > m_maxVisibleCharacters || lineNumber > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && pageNumber2 + 1 != m_pageToDisplay))
				{
					flag27 = false;
				}
				if (!flag3 && flag27 && j <= tMP_LineInfo.lastVisibleCharacterIndex && character != '\n' && character != '\v' && character != '\r' && (j != tMP_LineInfo.lastVisibleCharacterIndex || !char.IsSeparator(character)))
				{
					flag3 = true;
					start3 = TMP_Text.k_LargePositiveVector2;
					end = TMP_Text.k_LargeNegativeVector2;
					highlightState = m_textInfo.characterInfo[j].highlightState;
				}
				if (flag3)
				{
					TMP_CharacterInfo tMP_CharacterInfo = m_textInfo.characterInfo[j];
					HighlightState highlightState2 = tMP_CharacterInfo.highlightState;
					bool flag28 = false;
					if (highlightState != highlightState2)
					{
						if (flag23)
						{
							end.x = (end.x - highlightState.padding.right + tMP_CharacterInfo.origin) / 2f;
						}
						else
						{
							end.x = (end.x - highlightState.padding.right + tMP_CharacterInfo.bottomLeft.x) / 2f;
						}
						start3.y = Mathf.Min(start3.y, tMP_CharacterInfo.descender);
						end.y = Mathf.Max(end.y, tMP_CharacterInfo.ascender);
						DrawTextHighlight(start3, end, ref index4, highlightState.color);
						flag3 = true;
						start3 = new Vector2(end.x, tMP_CharacterInfo.descender - highlightState2.padding.bottom);
						end = ((!flag23) ? ((Vector3)new Vector2(tMP_CharacterInfo.topRight.x + highlightState2.padding.right, tMP_CharacterInfo.ascender + highlightState2.padding.top)) : ((Vector3)new Vector2(tMP_CharacterInfo.xAdvance + highlightState2.padding.right, tMP_CharacterInfo.ascender + highlightState2.padding.top)));
						highlightState = highlightState2;
						flag28 = true;
					}
					if (!flag28)
					{
						if (flag23)
						{
							start3.x = Mathf.Min(start3.x, tMP_CharacterInfo.origin - highlightState.padding.left);
							end.x = Mathf.Max(end.x, tMP_CharacterInfo.xAdvance + highlightState.padding.right);
						}
						else
						{
							start3.x = Mathf.Min(start3.x, tMP_CharacterInfo.bottomLeft.x - highlightState.padding.left);
							end.x = Mathf.Max(end.x, tMP_CharacterInfo.topRight.x + highlightState.padding.right);
						}
						start3.y = Mathf.Min(start3.y, tMP_CharacterInfo.descender - highlightState.padding.bottom);
						end.y = Mathf.Max(end.y, tMP_CharacterInfo.ascender + highlightState.padding.top);
					}
				}
				if (flag3 && m_characterCount == 1)
				{
					flag3 = false;
					DrawTextHighlight(start3, end, ref index4, highlightState.color);
				}
				else if (flag3 && (j == tMP_LineInfo.lastCharacterIndex || j >= tMP_LineInfo.lastVisibleCharacterIndex))
				{
					flag3 = false;
					DrawTextHighlight(start3, end, ref index4, highlightState.color);
				}
				else if (flag3 && !flag27)
				{
					flag3 = false;
					DrawTextHighlight(start3, end, ref index4, highlightState.color);
				}
			}
			else if (flag3)
			{
				flag3 = false;
				DrawTextHighlight(start3, end, ref index4, highlightState.color);
			}
			num75 = lineNumber;
		}
		m_textInfo.meshInfo[m_Underline.materialIndex].vertexCount = index4;
		m_textInfo.characterCount = m_characterCount;
		m_textInfo.spriteCount = m_spriteCount;
		m_textInfo.lineCount = lineCount;
		m_textInfo.wordCount = ((num74 == 0 || m_characterCount <= 0) ? 1 : num74);
		m_textInfo.pageCount = m_pageNumber + 1;
		if (m_renderMode == TextRenderFlags.Render && IsActive())
		{
			OnPreRenderText?.Invoke(m_textInfo);
			if (m_canvas.additionalShaderChannels != (AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent))
			{
				m_canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
			}
			if (m_geometrySortingOrder != 0)
			{
				m_textInfo.meshInfo[0].SortGeometry(VertexSortingOrder.Reverse);
			}
			m_mesh.MarkDynamic();
			m_mesh.vertices = m_textInfo.meshInfo[0].vertices;
			m_mesh.SetUVs(0, m_textInfo.meshInfo[0].uvs0);
			m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
			m_mesh.colors32 = m_textInfo.meshInfo[0].colors32;
			m_mesh.RecalculateBounds();
			m_canvasRenderer.SetMesh(m_mesh);
			Color color2 = m_canvasRenderer.GetColor();
			bool cullTransparentMesh = m_canvasRenderer.cullTransparentMesh;
			for (int k = 1; k < m_textInfo.materialCount; k++)
			{
				m_textInfo.meshInfo[k].ClearUnusedVertices();
				if (!(m_subTextObjects[k] == null))
				{
					if (m_geometrySortingOrder != 0)
					{
						m_textInfo.meshInfo[k].SortGeometry(VertexSortingOrder.Reverse);
					}
					m_subTextObjects[k].mesh.vertices = m_textInfo.meshInfo[k].vertices;
					m_subTextObjects[k].mesh.SetUVs(0, m_textInfo.meshInfo[k].uvs0);
					m_subTextObjects[k].mesh.uv2 = m_textInfo.meshInfo[k].uvs2;
					m_subTextObjects[k].mesh.colors32 = m_textInfo.meshInfo[k].colors32;
					m_subTextObjects[k].mesh.RecalculateBounds();
					m_subTextObjects[k].canvasRenderer.SetMesh(m_subTextObjects[k].mesh);
					m_subTextObjects[k].canvasRenderer.SetColor(color2);
					m_subTextObjects[k].canvasRenderer.cullTransparentMesh = cullTransparentMesh;
					m_subTextObjects[k].raycastTarget = raycastTarget;
				}
			}
		}
		if (m_ShouldUpdateCulling)
		{
			UpdateCulling();
		}
		TMPro_EventManager.ON_TEXT_CHANGED(this);
	}

	protected override Vector3[] GetTextContainerLocalCorners()
	{
		if (m_rectTransform == null)
		{
			m_rectTransform = base.rectTransform;
		}
		m_rectTransform.GetLocalCorners(m_RectTransformCorners);
		return m_RectTransformCorners;
	}

	protected override void SetActiveSubMeshes(bool state)
	{
		for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
		{
			if (m_subTextObjects[i].enabled != state)
			{
				m_subTextObjects[i].enabled = state;
			}
		}
	}

	protected override void DestroySubMeshObjects()
	{
		for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
		{
			UnityEngine.Object.DestroyImmediate(m_subTextObjects[i]);
		}
	}

	protected override Bounds GetCompoundBounds()
	{
		Bounds bounds = m_mesh.bounds;
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
		{
			Bounds bounds2 = m_subTextObjects[i].mesh.bounds;
			min.x = ((min.x < bounds2.min.x) ? min.x : bounds2.min.x);
			min.y = ((min.y < bounds2.min.y) ? min.y : bounds2.min.y);
			max.x = ((max.x > bounds2.max.x) ? max.x : bounds2.max.x);
			max.y = ((max.y > bounds2.max.y) ? max.y : bounds2.max.y);
		}
		Vector3 center = (min + max) / 2f;
		Vector2 vector = max - min;
		return new Bounds(center, vector);
	}

	internal override Rect GetCanvasSpaceClippingRect()
	{
		if (m_canvas == null || m_canvas.rootCanvas == null || m_mesh == null)
		{
			return Rect.zero;
		}
		Transform obj = m_canvas.rootCanvas.transform;
		Bounds compoundBounds = GetCompoundBounds();
		Vector2 vector = obj.InverseTransformPoint(m_rectTransform.position);
		Vector2 vector2 = obj.lossyScale;
		Vector2 vector3 = m_rectTransform.lossyScale / vector2;
		return new Rect(vector + compoundBounds.min * vector3, compoundBounds.size * vector3);
	}

	private void UpdateSDFScale(float scaleDelta)
	{
		if (scaleDelta == 0f || scaleDelta == float.PositiveInfinity || scaleDelta == float.NegativeInfinity)
		{
			m_havePropertiesChanged = true;
			OnPreRenderCanvas();
			return;
		}
		for (int i = 0; i < m_textInfo.materialCount; i++)
		{
			TMP_MeshInfo tMP_MeshInfo = m_textInfo.meshInfo[i];
			for (int j = 0; j < tMP_MeshInfo.uvs0.Length; j++)
			{
				tMP_MeshInfo.uvs0[j].w *= Mathf.Abs(scaleDelta);
			}
		}
		for (int k = 0; k < m_textInfo.materialCount; k++)
		{
			if (k == 0)
			{
				m_mesh.SetUVs(0, m_textInfo.meshInfo[0].uvs0);
				m_canvasRenderer.SetMesh(m_mesh);
			}
			else
			{
				m_subTextObjects[k].mesh.SetUVs(0, m_textInfo.meshInfo[k].uvs0);
				m_subTextObjects[k].canvasRenderer.SetMesh(m_subTextObjects[k].mesh);
			}
		}
	}
}
