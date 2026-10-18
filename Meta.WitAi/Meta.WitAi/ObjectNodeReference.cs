using Meta.WitAi.Json;

namespace Meta.WitAi;

public class ObjectNodeReference : WitResponseReference
{
	public string key;

	public override string GetStringValue(WitResponseNode response)
	{
		if (child != null && null != response?[key])
		{
			return child.GetStringValue(response[key]);
		}
		return response?[key]?.Value;
	}

	public override int GetIntValue(WitResponseNode response)
	{
		if (child != null)
		{
			return child.GetIntValue(response[key]);
		}
		return response[key].AsInt;
	}

	public override float GetFloatValue(WitResponseNode response)
	{
		if (child != null)
		{
			return child.GetFloatValue(response[key]);
		}
		return response[key].AsFloat;
	}
}
