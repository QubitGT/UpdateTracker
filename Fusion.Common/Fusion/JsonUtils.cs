using System.Text.RegularExpressions;

namespace Fusion;

internal class JsonUtils
{
	private static Regex ReferencesRegex = new Regex(",\"references\":", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public static string RemoveExtraReferences(string baseJson)
	{
		Match match = ReferencesRegex.Match(baseJson);
		if (match.Success)
		{
			int num = match.Index + match.Length;
			int num2 = num;
			int num3 = 0;
			bool flag = false;
			do
			{
				if (baseJson[num2] == '{')
				{
					num3++;
					flag = true;
				}
				if (baseJson[num2] == '}')
				{
					num3--;
				}
				num2++;
			}
			while (num2 < baseJson.Length && num3 > 0);
			if (num3 == 0 && flag)
			{
				baseJson = baseJson.Remove(match.Index, num2 - match.Index);
			}
		}
		return baseJson;
	}
}
