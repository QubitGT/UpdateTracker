using System;
using Modio.API;
using Modio.Users;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyPlatformName : IUserProperty
{
	[Serializable]
	private class PlatformIcon
	{
		public ModioAPI.Portal Portal;

		public Sprite Icon;
	}

	[SerializeField]
	private GameObject[] _enableIsUsernameDefinedByPortal;

	[SerializeField]
	private Image _platformImage;

	[SerializeField]
	private PlatformIcon[] _platformIcons;

	public void OnUserUpdate(UserProfile user)
	{
		bool flag = user != null && !string.IsNullOrEmpty(user.PortalUsername);
		GameObject[] enableIsUsernameDefinedByPortal = _enableIsUsernameDefinedByPortal;
		for (int i = 0; i < enableIsUsernameDefinedByPortal.Length; i++)
		{
			enableIsUsernameDefinedByPortal[i].SetActive(flag);
		}
		if (!(_platformImage != null && flag))
		{
			return;
		}
		Sprite sprite = null;
		ModioAPI.Portal currentPortal = ModioAPI.CurrentPortal;
		PlatformIcon[] platformIcons = _platformIcons;
		foreach (PlatformIcon platformIcon in platformIcons)
		{
			if (platformIcon.Portal == currentPortal)
			{
				sprite = platformIcon.Icon;
			}
		}
		_platformImage.enabled = sprite != null;
		_platformImage.sprite = sprite;
	}
}
