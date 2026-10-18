using System.Collections.Generic;
using System.Text;

namespace Unity.XR.CoreUtils;

public static class CollectionExtensions
{
	private static readonly StringBuilder k_String = new StringBuilder();

	public static string Stringify<T>(this ICollection<T> collection)
	{
		k_String.Length = 0;
		int num = collection.Count - 1;
		int num2 = 0;
		foreach (T item in collection)
		{
			k_String.AppendFormat((num2++ == num) ? "{0}" : "{0}, ", item);
		}
		return k_String.ToString();
	}
}
