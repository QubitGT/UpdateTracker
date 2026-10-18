using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

[AddComponentMenu("Input/Input Action Manager")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager.html")]
public class InputActionManager : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Input action assets to affect when inputs are enabled or disabled.")]
	private List<InputActionAsset> m_ActionAssets;

	public List<InputActionAsset> actionAssets
	{
		get
		{
			return m_ActionAssets;
		}
		set
		{
			m_ActionAssets = value ?? throw new ArgumentNullException("value");
		}
	}

	protected void OnEnable()
	{
		EnableInput();
	}

	protected void OnDisable()
	{
		DisableInput();
	}

	public void EnableInput()
	{
		if (m_ActionAssets == null)
		{
			return;
		}
		foreach (InputActionAsset actionAsset in m_ActionAssets)
		{
			if (actionAsset != null)
			{
				actionAsset.Enable();
			}
		}
	}

	public void DisableInput()
	{
		if (m_ActionAssets == null)
		{
			return;
		}
		foreach (InputActionAsset actionAsset in m_ActionAssets)
		{
			if (actionAsset != null)
			{
				actionAsset.Disable();
			}
		}
	}
}
