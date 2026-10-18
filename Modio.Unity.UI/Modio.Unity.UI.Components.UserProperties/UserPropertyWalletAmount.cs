using System;
using Modio.Users;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyWalletAmount : IUserProperty, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private TMP_Text _text;

	private bool hasSetText;

	public void OnUserUpdate(UserProfile user)
	{
		Wallet wallet = User.Current?.Wallet;
		_text.text = ((wallet != null) ? wallet.Balance.ToString() : "");
		hasSetText = true;
	}

	public void Start()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnEnable()
	{
		if (!hasSetText)
		{
			_text.text = "";
		}
	}

	public void OnDisable()
	{
	}
}
