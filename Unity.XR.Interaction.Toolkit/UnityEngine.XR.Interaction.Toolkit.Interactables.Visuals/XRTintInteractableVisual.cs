using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[AddComponentMenu("XR/Visual/XR Tint Interactable Visual", 11)]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.Visuals.XRTintInteractableVisual.html")]
public class XRTintInteractableVisual : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct ShaderPropertyLookup
	{
		public static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
	}

	[SerializeField]
	[Tooltip("Tint color for interactable.")]
	private Color m_TintColor = Color.yellow;

	[SerializeField]
	[Tooltip("Tint on hover.")]
	private bool m_TintOnHover = true;

	[SerializeField]
	[Tooltip("Tint on selection.")]
	private bool m_TintOnSelection = true;

	[SerializeField]
	[Tooltip("Renderer(s) to use for tinting (will default to any Renderer on the GameObject if not specified).")]
	private List<Renderer> m_TintRenderers = new List<Renderer>();

	private IXRInteractable m_Interactable;

	private IXRHoverInteractable m_HoverInteractable;

	private IXRSelectInteractable m_SelectInteractable;

	private MaterialPropertyBlock m_TintPropertyBlock;

	private bool m_EmissionEnabled;

	private bool m_HasLoggedMaterialInstance;

	private static readonly List<Material> s_Materials = new List<Material>();

	public Color tintColor
	{
		get
		{
			return m_TintColor;
		}
		set
		{
			m_TintColor = value;
		}
	}

	public bool tintOnHover
	{
		get
		{
			return m_TintOnHover;
		}
		set
		{
			m_TintOnHover = value;
		}
	}

	public bool tintOnSelection
	{
		get
		{
			return m_TintOnSelection;
		}
		set
		{
			m_TintOnSelection = value;
		}
	}

	public List<Renderer> tintRenderers
	{
		get
		{
			return m_TintRenderers;
		}
		set
		{
			m_TintRenderers = value;
		}
	}

	protected void Awake()
	{
		m_Interactable = GetComponent<IXRInteractable>();
		if (m_Interactable is Object @object && @object != null)
		{
			m_HoverInteractable = m_Interactable as IXRHoverInteractable;
			m_SelectInteractable = m_Interactable as IXRSelectInteractable;
			if (m_HoverInteractable != null)
			{
				m_HoverInteractable.firstHoverEntered.AddListener(OnFirstHoverEntered);
				m_HoverInteractable.lastHoverExited.AddListener(OnLastHoverExited);
			}
			if (m_SelectInteractable != null)
			{
				m_SelectInteractable.firstSelectEntered.AddListener(OnFirstSelectEntered);
				m_SelectInteractable.lastSelectExited.AddListener(OnLastSelectExited);
			}
		}
		else
		{
			Debug.LogWarning($"Could not find required interactable component on {base.gameObject} for tint visual." + " Cannot respond to hover or selection.", this);
		}
		if (m_TintRenderers.Count == 0)
		{
			GetComponents(m_TintRenderers);
			if (m_TintRenderers.Count == 0)
			{
				Debug.LogWarning($"Could not find required Renderer component on {base.gameObject} for tint visual.", this);
			}
		}
		m_EmissionEnabled = GetEmissionEnabled();
		m_TintPropertyBlock = new MaterialPropertyBlock();
		if (m_TintOnHover)
		{
			IXRHoverInteractable hoverInteractable = m_HoverInteractable;
			if (hoverInteractable != null && hoverInteractable.isHovered)
			{
				goto IL_0179;
			}
		}
		if (m_TintOnSelection)
		{
			IXRSelectInteractable selectInteractable = m_SelectInteractable;
			if (selectInteractable == null || !selectInteractable.isSelected)
			{
				return;
			}
			goto IL_0179;
		}
		return;
		IL_0179:
		SetTint(on: true);
	}

	protected void OnDestroy()
	{
		if (m_Interactable is Object @object && @object != null)
		{
			if (m_HoverInteractable != null)
			{
				m_HoverInteractable.firstHoverEntered.RemoveListener(OnFirstHoverEntered);
				m_HoverInteractable.lastHoverExited.RemoveListener(OnLastHoverExited);
			}
			if (m_SelectInteractable != null)
			{
				m_SelectInteractable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
				m_SelectInteractable.lastSelectExited.RemoveListener(OnLastSelectExited);
			}
		}
	}

	protected virtual void SetTint(bool on)
	{
		Color value = (on ? (m_TintColor * Mathf.LinearToGammaSpace(1f)) : Color.black);
		if (!m_EmissionEnabled && !m_HasLoggedMaterialInstance)
		{
			Debug.LogWarning("Emission is not enabled on a Material used by a tint visual, a Material instance will need to be created.", this);
			m_HasLoggedMaterialInstance = true;
		}
		foreach (Renderer tintRenderer in m_TintRenderers)
		{
			if (tintRenderer == null)
			{
				continue;
			}
			if (!m_EmissionEnabled)
			{
				tintRenderer.GetMaterials(s_Materials);
				foreach (Material s_Material in s_Materials)
				{
					if (on)
					{
						s_Material.EnableKeyword("_EMISSION");
					}
					else
					{
						s_Material.DisableKeyword("_EMISSION");
					}
				}
				s_Materials.Clear();
			}
			tintRenderer.GetPropertyBlock(m_TintPropertyBlock);
			m_TintPropertyBlock.SetColor(ShaderPropertyLookup.emissionColor, value);
			tintRenderer.SetPropertyBlock(m_TintPropertyBlock);
		}
	}

	protected virtual bool GetEmissionEnabled()
	{
		foreach (Renderer tintRenderer in m_TintRenderers)
		{
			if (tintRenderer == null)
			{
				continue;
			}
			tintRenderer.GetSharedMaterials(s_Materials);
			foreach (Material s_Material in s_Materials)
			{
				if (!s_Material.IsKeywordEnabled("_EMISSION"))
				{
					s_Materials.Clear();
					return false;
				}
			}
		}
		s_Materials.Clear();
		return true;
	}

	private void OnFirstHoverEntered(HoverEnterEventArgs args)
	{
		if (m_TintOnHover)
		{
			SetTint(on: true);
		}
	}

	private void OnLastHoverExited(HoverExitEventArgs args)
	{
		if (m_TintOnHover)
		{
			SetTint(m_TintOnSelection && (m_SelectInteractable?.isSelected ?? false));
		}
	}

	private void OnFirstSelectEntered(SelectEnterEventArgs args)
	{
		if (m_TintOnSelection)
		{
			SetTint(on: true);
		}
	}

	private void OnLastSelectExited(SelectExitEventArgs args)
	{
		if (m_TintOnSelection)
		{
			SetTint(m_TintOnHover && (m_HoverInteractable?.isHovered ?? false));
		}
	}
}
