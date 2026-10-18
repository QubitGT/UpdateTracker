using System;
using System.Collections.Generic;
using Modio.Unity.UI.Search;
using Modio.Users;
using UnityEngine;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyUser : ISearchProperty
{
	[SerializeField]
	private ModioUIUser _user;

	public void OnSearchUpdate(ModioUISearch search)
	{
		IReadOnlyList<UserProfile> users = search.LastSearchFilter.GetUsers();
		_user.SetUser((users.Count > 0) ? users[0] : null);
	}
}
