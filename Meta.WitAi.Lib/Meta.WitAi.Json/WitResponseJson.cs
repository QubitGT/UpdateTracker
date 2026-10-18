using System;

namespace Meta.WitAi.Json;

public static class WitResponseJson
{
	[Obsolete("Instead use WitResponseNode.Parse")]
	public static WitResponseNode Parse(string aJSON)
	{
		return WitResponseNode.Parse(aJSON);
	}
}
