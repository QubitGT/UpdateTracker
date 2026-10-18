using System.Text;

namespace Unity.XR.CoreUtils;

public static class StringExtensions
{
	private static readonly StringBuilder k_StringBuilder = new StringBuilder();

	public static string FirstToUpper(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		if (str.Length == 1)
		{
			return char.ToUpper(str[0]).ToString();
		}
		return $"{char.ToUpper(str[0])}{str.Substring(1)}";
	}

	public static string InsertSpacesBetweenWords(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		k_StringBuilder.Length = 0;
		k_StringBuilder.Append(str[0]);
		int length = str.Length;
		for (int i = 0; i < length - 1; i++)
		{
			char c = str[i];
			char c2 = str[i + 1];
			bool flag = char.IsLower(c);
			bool flag2 = char.IsLower(c2);
			bool flag3 = flag && !flag2;
			if (i + 2 < length)
			{
				bool flag4 = char.IsLower(str[i + 2]);
				flag3 = flag3 || (!flag && !flag2 && flag4);
			}
			if (flag3)
			{
				k_StringBuilder.Append(' ');
			}
			k_StringBuilder.Append(c2);
		}
		return k_StringBuilder.ToString();
	}
}
