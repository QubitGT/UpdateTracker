using System;
using Modio.Unity.UI.Components.Localization;
using Modio.Users;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyName : IUserProperty
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private ModioUILocalizedText _localisedText;

	[SerializeField]
	private string _userLoggedInFormat = "{0}";

	[SerializeField]
	private string _noUserLoggedIn;

	public void OnUserUpdate(UserProfile user)
	{
		if ((object)user != null && user.Username != null)
		{
			if (_localisedText != null)
			{
				_localisedText.SetFormatArgs(user.Username);
				return;
			}
			string text = user.Username;
			if (!string.IsNullOrEmpty(_userLoggedInFormat))
			{
				text = string.Format(_userLoggedInFormat, text);
			}
			_text.text = text;
		}
		else if (_localisedText != null)
		{
			_localisedText.SetFormatArgs("");
		}
		else if (_text != null)
		{
			_text.text = _noUserLoggedIn;
		}
	}
}
