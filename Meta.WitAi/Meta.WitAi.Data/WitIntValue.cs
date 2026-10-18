using Meta.WitAi.Json;

namespace Meta.WitAi.Data;

public class WitIntValue : WitValue
{
	public override object GetValue(WitResponseNode response)
	{
		return GetIntValue(response);
	}

	public override bool Equals(WitResponseNode response, object value)
	{
		int result = 0;
		if (value is int num)
		{
			result = num;
		}
		else if (value != null && !int.TryParse(value?.ToString() ?? "", out result))
		{
			return false;
		}
		return GetIntValue(response) == result;
	}

	public int GetIntValue(WitResponseNode response)
	{
		return base.Reference.GetIntValue(response);
	}
}
