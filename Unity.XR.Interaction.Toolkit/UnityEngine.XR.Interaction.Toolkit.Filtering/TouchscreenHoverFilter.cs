using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[AddComponentMenu("XR/AR/Touchscreen Hover Filter", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.TouchscreenHoverFilter.html")]
public class TouchscreenHoverFilter : MonoBehaviour, IXRHoverFilter
{
	[SerializeField]
	private XRInputValueReader<int> m_ScreenTouchCountInput = new XRInputValueReader<int>("Screen Touch Count");

	public XRInputValueReader<int> screenTouchCountInput
	{
		get
		{
			return m_ScreenTouchCountInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ScreenTouchCountInput, value, this);
		}
	}

	public bool canProcess => base.isActiveAndEnabled;

	protected void OnEnable()
	{
		m_ScreenTouchCountInput.EnableDirectActionIfModeUsed();
	}

	protected void OnDisable()
	{
		m_ScreenTouchCountInput.DisableDirectActionIfModeUsed();
	}

	public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		if (interactor is XRBaseInputInteractor xRBaseInputInteractor)
		{
			if (xRBaseInputInteractor.selectInput.ReadIsPerformed())
			{
				return m_ScreenTouchCountInput.ReadValue() <= 1;
			}
			return false;
		}
		return m_ScreenTouchCountInput.ReadValue() > 0;
	}
}
