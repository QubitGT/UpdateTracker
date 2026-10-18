using System;

namespace Modio.Unity.UI.Search;

[Serializable]
public enum SpecialSearchType
{
	Nothing = 8,
	Installed = 5,
	Subscribed = 6,
	InstalledOrSubscribed = 7,
	UserCreations = 9,
	Purchased = 10,
	SearchForTag = 11,
	SearchForUser = 12
}
