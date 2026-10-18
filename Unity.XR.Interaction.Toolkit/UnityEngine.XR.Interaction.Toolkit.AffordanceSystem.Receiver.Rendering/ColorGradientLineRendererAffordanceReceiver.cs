using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

[AddComponentMenu("Affordance System/Receiver/Rendering/Color Gradient Line Renderer Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.ColorGradientLineRendererAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class ColorGradientLineRendererAffordanceReceiver : ColorAffordanceReceiver
{
	public enum LineColorProperty
	{
		StartColor,
		EndColor
	}

	[SerializeField]
	[Tooltip("Line Renderer on which to animate colors.")]
	private LineRenderer m_LineRenderer;

	[SerializeField]
	[Tooltip("Mode determining how color is applied to the associated Line Renderer.")]
	private LineColorProperty m_LineColorProperty;

	[SerializeField]
	[Tooltip("Prevent XR Interactor Line Visual from controlling line rendering color if present.")]
	private bool m_DisableXRInteractorLineVisualColorControlIfPresent = true;

	private Color m_InitialStartColor;

	private Color m_InitialEndColor;

	public LineRenderer lineRenderer
	{
		get
		{
			return m_LineRenderer;
		}
		set
		{
			m_LineRenderer = value;
		}
	}

	public LineColorProperty lineColorProperty
	{
		get
		{
			return m_LineColorProperty;
		}
		set
		{
			m_LineColorProperty = value;
			CaptureInitialValue();
		}
	}

	public bool disableXRInteractorLineVisualColorControlIfPresent
	{
		get
		{
			return m_DisableXRInteractorLineVisualColorControlIfPresent;
		}
		set
		{
			m_DisableXRInteractorLineVisualColorControlIfPresent = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_LineRenderer == null)
		{
			m_LineRenderer = GetComponentInParent<LineRenderer>();
			if (m_LineRenderer == null)
			{
				XRLoggingUtils.LogError("Missing Line Renderer on " + this, this);
				base.enabled = false;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		if (m_DisableXRInteractorLineVisualColorControlIfPresent)
		{
			XRInteractorLineVisual componentInParent = GetComponentInParent<XRInteractorLineVisual>();
			if (componentInParent != null)
			{
				componentInParent.setLineColorGradient = false;
			}
		}
	}

	protected override void OnAffordanceValueUpdated(Color newValue)
	{
		base.OnAffordanceValueUpdated(newValue);
		switch (m_LineColorProperty)
		{
		case LineColorProperty.StartColor:
			m_LineRenderer.startColor = newValue;
			break;
		case LineColorProperty.EndColor:
			m_LineRenderer.endColor = newValue;
			break;
		}
	}

	protected override void CaptureInitialValue()
	{
		if (!base.initialValueCaptured)
		{
			m_InitialStartColor = m_LineRenderer.startColor;
			m_InitialEndColor = m_LineRenderer.endColor;
			base.CaptureInitialValue();
		}
	}

	protected override Color GetCurrentValueForCapture()
	{
		return m_LineColorProperty switch
		{
			LineColorProperty.StartColor => m_InitialStartColor, 
			LineColorProperty.EndColor => m_InitialEndColor, 
			_ => base.GetCurrentValueForCapture(), 
		};
	}
}
