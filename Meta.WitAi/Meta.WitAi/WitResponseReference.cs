using Meta.WitAi.Json;

namespace Meta.WitAi;

public class WitResponseReference
{
	public WitResponseReference child;

	public string path;

	public virtual string GetStringValue(WitResponseNode response)
	{
		return child.GetStringValue(response);
	}

	public virtual int GetIntValue(WitResponseNode response)
	{
		return child.GetIntValue(response);
	}

	public virtual float GetFloatValue(WitResponseNode response)
	{
		return child.GetFloatValue(response);
	}
}
