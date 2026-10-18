using Modio.Users;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.UserProperties;

public abstract class UserPropertyNumberBase : IUserProperty
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	[Tooltip("None: \"10500\".\r\nComma: \"10,500\".\r\nKilo: \"10.5k\".")]
	private StringFormatKilo _format = StringFormatKilo.Kilo;

	[SerializeField]
	[ShowIf("IsCustomFormat")]
	private string _customFormat;

	public void OnUserUpdate(UserProfile user)
	{
		_text.text = StringFormat.Kilo(_format, GetValue(user), _customFormat);
	}

	protected abstract long GetValue(UserProfile user);

	private bool IsCustomFormat()
	{
		return _format == StringFormatKilo.Custom;
	}
}
