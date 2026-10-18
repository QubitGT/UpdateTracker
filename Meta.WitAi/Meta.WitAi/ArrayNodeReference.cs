using Meta.WitAi.Json;

namespace Meta.WitAi;

public class ArrayNodeReference : WitResponseReference
{
	public int index;

	public override string GetStringValue(WitResponseNode response)
	{
		if (child != null)
		{
			return child.GetStringValue(response[index]);
		}
		return response[index].Value;
	}

	public override int GetIntValue(WitResponseNode response)
	{
		if (child != null)
		{
			return child.GetIntValue(response[index]);
		}
		return response[index].AsInt;
	}

	public override float GetFloatValue(WitResponseNode response)
	{
		if (child != null)
		{
			return child.GetFloatValue(response[index]);
		}
		return response[index].AsInt;
	}
}
