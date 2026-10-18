using System;
using System.Collections.Generic;

namespace Fusion.Photon.Realtime;

public class AuthenticationValues
{
	private CustomAuthenticationType authType = CustomAuthenticationType.None;

	public CustomAuthenticationType AuthType
	{
		get
		{
			return authType;
		}
		set
		{
			authType = value;
		}
	}

	public string AuthGetParameters { get; set; }

	public object AuthPostData { get; private set; }

	public object Token { get; protected internal set; }

	public string UserId { get; set; }

	public AuthenticationValues()
	{
	}

	public AuthenticationValues(string userId)
	{
		UserId = userId;
	}

	public virtual void SetAuthPostData(string stringData)
	{
		AuthPostData = (string.IsNullOrEmpty(stringData) ? null : stringData);
	}

	public virtual void SetAuthPostData(byte[] byteData)
	{
		AuthPostData = byteData;
	}

	public virtual void SetAuthPostData(Dictionary<string, object> dictData)
	{
		AuthPostData = dictData;
	}

	public virtual void AddAuthParameter(string key, string value)
	{
		string text = (string.IsNullOrEmpty(AuthGetParameters) ? "" : "&");
		AuthGetParameters = $"{AuthGetParameters}{text}{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
	}

	public override string ToString()
	{
		return string.Format("AuthenticationValues = AuthType: {0} UserId: {1}{2}{3}{4}", AuthType, UserId, string.IsNullOrEmpty(AuthGetParameters) ? " GetParameters: yes" : "", (AuthPostData == null) ? "" : " PostData: yes", (Token == null) ? "" : " Token: yes");
	}

	public AuthenticationValues CopyTo(AuthenticationValues copy)
	{
		copy.AuthType = AuthType;
		copy.AuthGetParameters = AuthGetParameters;
		copy.AuthPostData = AuthPostData;
		copy.UserId = UserId;
		return copy;
	}
}
