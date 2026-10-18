using Meta.WitAi.Json;

namespace Meta.WitAi.Data;

public class WitStringValue : WitValue
{
	public override object GetValue(WitResponseNode response)
	{
		return GetStringValue(response);
	}

	public override bool Equals(WitResponseNode response, object value)
	{
		if (value is string text)
		{
			return GetStringValue(response) == text;
		}
		return (value?.ToString() ?? "") == GetStringValue(response);
	}

	public string GetStringValue(WitResponseNode response)
	{
		return base.Reference.GetStringValue(response);
	}
}
