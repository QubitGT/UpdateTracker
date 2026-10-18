using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Backtrace.Unity.Extensions;

internal static class StringHelper
{
	internal static string OnlyLetters(this string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}
		return new string(source.Where((char n) => char.IsLetter(n)).ToArray());
	}

	internal static string GetSha(this StringBuilder source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		return source.ToString().GetSha();
	}

	internal static string GetSha(this string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			return "0000000000000000000000000000000000000000000000000000000000000000";
		}
		using SHA256 sHA = SHA256.Create();
		byte[] array = sHA.ComputeHash(Encoding.ASCII.GetBytes(source));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
