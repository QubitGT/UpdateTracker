using System;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.UI;

[AddComponentMenu("Affordance System/Receiver/UI/Image Color Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.UI.ImageColorAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class ImageColorAffordanceReceiver : ColorAffordanceReceiver
{
	[Tooltip("Image to apply the color to.")]
	[SerializeField]
	private Image m_Image;

	[Tooltip("If set, alpha changes will be applied to the CanvasGroup rather than the Image.")]
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[Tooltip("Ignore alpha changes in color theme.")]
	[SerializeField]
	private bool m_IgnoreAlpha;

	private bool m_HasImage;

	private bool m_HasCanvasGroup;

	public Image image
	{
		get
		{
			return m_Image;
		}
		set
		{
			m_Image = value;
		}
	}

	public CanvasGroup canvasGroup
	{
		get
		{
			return m_CanvasGroup;
		}
		set
		{
			m_CanvasGroup = value;
		}
	}

	public bool ignoreAlpha
	{
		get
		{
			return m_IgnoreAlpha;
		}
		set
		{
			m_IgnoreAlpha = value;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_HasImage = m_Image != null;
		m_HasCanvasGroup = m_CanvasGroup != null;
	}

	protected override void OnAffordanceValueUpdated(Color newValue)
	{
		if (m_HasImage)
		{
			if (m_HasCanvasGroup)
			{
				m_Image.color = new Color(newValue.r, newValue.g, newValue.b, 1f);
				if (!m_IgnoreAlpha)
				{
					m_CanvasGroup.alpha = newValue.a;
				}
			}
			else
			{
				m_Image.color = (m_IgnoreAlpha ? new Color(newValue.r, newValue.g, newValue.b, 1f) : newValue);
			}
		}
		base.OnAffordanceValueUpdated(newValue);
	}

	protected override Color GetCurrentValueForCapture()
	{
		if (!m_HasImage)
		{
			return base.GetCurrentValueForCapture();
		}
		return m_Image.color;
	}
}
