using System;
using Modio.Users;
using UnityEngine;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyEnableOnLogin : IUserProperty
{
	[SerializeField]
	private GameObject[] _activeWhenLoggedOut;

	[SerializeField]
	private GameObject[] _activeWhenLoggedIn;

	public void OnUserUpdate(UserProfile user)
	{
		bool flag = user != null;
		GameObject[] activeWhenLoggedOut = _activeWhenLoggedOut;
		for (int i = 0; i < activeWhenLoggedOut.Length; i++)
		{
			activeWhenLoggedOut[i].SetActive(!flag);
		}
		activeWhenLoggedOut = _activeWhenLoggedIn;
		for (int i = 0; i < activeWhenLoggedOut.Length; i++)
		{
			activeWhenLoggedOut[i].SetActive(flag);
		}
	}
}
