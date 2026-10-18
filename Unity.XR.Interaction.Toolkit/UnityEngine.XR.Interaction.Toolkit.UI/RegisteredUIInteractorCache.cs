using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal class RegisteredUIInteractorCache
{
	private XRUIInputModule m_InputModule;

	private XRUIInputModule m_RegisteredInputModule;

	private readonly IUIInteractor m_UiInteractor;

	private readonly XRBaseInteractor m_BaseInteractor;

	public RegisteredUIInteractorCache(IUIInteractor uiInteractor)
	{
		m_UiInteractor = uiInteractor;
		m_BaseInteractor = uiInteractor as XRBaseInteractor;
	}

	public void RegisterOrUnregisterXRUIInputModule(bool enabled)
	{
		if (Application.isPlaying && (!(m_BaseInteractor != null) || m_BaseInteractor.isActiveAndEnabled))
		{
			if (enabled)
			{
				RegisterWithXRUIInputModule();
			}
			else
			{
				UnregisterFromXRUIInputModule();
			}
		}
	}

	public void RegisterWithXRUIInputModule()
	{
		if (m_InputModule == null)
		{
			FindOrCreateXRUIInputModule();
		}
		if (!(m_RegisteredInputModule == m_InputModule))
		{
			UnregisterFromXRUIInputModule();
			m_InputModule.RegisterInteractor(m_UiInteractor);
			m_RegisteredInputModule = m_InputModule;
		}
	}

	public void UnregisterFromXRUIInputModule()
	{
		if (m_RegisteredInputModule != null)
		{
			m_RegisteredInputModule.UnregisterInteractor(m_UiInteractor);
		}
		m_RegisteredInputModule = null;
	}

	private void FindOrCreateXRUIInputModule()
	{
		EventSystem component = EventSystem.current;
		if (component == null)
		{
			if (ComponentLocatorUtility<EventSystem>.TryFindComponent(out component))
			{
				if (component.TryGetComponent<StandaloneInputModule>(out var component2))
				{
					Object.Destroy(component2);
				}
			}
			else
			{
				component = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
			}
		}
		if (!component.TryGetComponent<XRUIInputModule>(out m_InputModule))
		{
			m_InputModule = component.gameObject.AddComponent<XRUIInputModule>();
		}
	}

	public bool TryGetUIModel(out TrackedDeviceModel model)
	{
		if (m_InputModule != null)
		{
			return m_InputModule.GetTrackedDeviceModel(m_UiInteractor, out model);
		}
		model = TrackedDeviceModel.invalid;
		return false;
	}

	public bool IsOverUIGameObject()
	{
		if (m_InputModule != null && TryGetUIModel(out var model))
		{
			return m_InputModule.IsPointerOverGameObject(model.pointerId);
		}
		return false;
	}

	public bool TryGetCurrentUIGameObject(bool useAnyPointerId, out GameObject currentGameObject)
	{
		if (m_InputModule != null)
		{
			TrackedDeviceModel model;
			if (useAnyPointerId)
			{
				currentGameObject = m_InputModule.GetCurrentGameObject(-1);
			}
			else if (TryGetUIModel(out model))
			{
				currentGameObject = m_InputModule.GetCurrentGameObject(model.pointerId);
			}
			else
			{
				currentGameObject = null;
			}
			return currentGameObject != null;
		}
		currentGameObject = null;
		return false;
	}
}
