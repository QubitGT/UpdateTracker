using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Backtrace.Unity.Json;

public class BacktraceJObject
{
	internal readonly Dictionary<string, string> PrimitiveValues = new Dictionary<string, string>();

	internal readonly IDictionary<string, string> UserPrimitives;

	internal readonly Dictionary<string, BacktraceJObject> InnerObjects = new Dictionary<string, BacktraceJObject>();

	internal readonly Dictionary<string, object> ComplexObjects = new Dictionary<string, object>();

	public BacktraceJObject()
		: this(null)
	{
	}

	public BacktraceJObject(IDictionary<string, string> source)
	{
		IDictionary<string, string> userPrimitives;
		if (source != null)
		{
			userPrimitives = source;
		}
		else
		{
			IDictionary<string, string> dictionary = new Dictionary<string, string>();
			userPrimitives = dictionary;
		}
		UserPrimitives = userPrimitives;
	}

	public void Add(string key, bool value)
	{
		PrimitiveValues.Add(key, value.ToString(CultureInfo.InvariantCulture).ToLower());
	}

	public void Add(string key, float value, string format = "G")
	{
		PrimitiveValues.Add(key, value.ToString(format, CultureInfo.InvariantCulture));
	}

	public void Add(string key, double value, string format = "G")
	{
		PrimitiveValues.Add(key, value.ToString(format, CultureInfo.InvariantCulture));
	}

	public void Add(string key, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			value = string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("\"");
		EscapeString(value, stringBuilder);
		stringBuilder.Append("\"");
		PrimitiveValues.Add(key, stringBuilder.ToString());
	}

	public void Add(string key, long value)
	{
		PrimitiveValues.Add(key, value.ToString(CultureInfo.InvariantCulture));
	}

	public void Add(string key, BacktraceJObject value)
	{
		if (value != null)
		{
			InnerObjects.Add(key, value);
		}
		else
		{
			ComplexObjects.Add(key, null);
		}
	}

	public void Add(string key, IEnumerable value)
	{
		ComplexObjects.Add(key, value);
	}

	public string ToJson()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToJson(stringBuilder);
		return stringBuilder.ToString();
	}

	internal void ToJson(StringBuilder stringBuilder)
	{
		stringBuilder.Append("{");
		AppendPrimitives(stringBuilder);
		AddUserPrimitives(stringBuilder);
		AppendJObjects(stringBuilder);
		AppendComplexValues(stringBuilder);
		stringBuilder.Append("}");
	}

	private void AddUserPrimitives(StringBuilder stringBuilder)
	{
		if (UserPrimitives.Count == 0)
		{
			return;
		}
		int num = 0;
		if (ShouldContinueAddingJSONProperties(stringBuilder))
		{
			stringBuilder.Append(',');
		}
		using IEnumerator<KeyValuePair<string, string>> enumerator = UserPrimitives.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
			KeyValuePair<string, string> current = enumerator.Current;
			AppendKey(current.Key, stringBuilder);
			if (string.IsNullOrEmpty(current.Value))
			{
				stringBuilder.Append("\"\"");
			}
			else
			{
				stringBuilder.Append("\"");
				EscapeString(current.Value, stringBuilder);
				stringBuilder.Append("\"");
			}
			if (num != UserPrimitives.Count)
			{
				stringBuilder.Append(",");
			}
		}
	}

	private void AppendPrimitives(StringBuilder stringBuilder)
	{
		int num = 0;
		using Dictionary<string, string>.Enumerator enumerator = PrimitiveValues.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
			KeyValuePair<string, string> current = enumerator.Current;
			AppendKey(current.Key, stringBuilder);
			stringBuilder.Append(string.IsNullOrEmpty(current.Value) ? "\"\"" : current.Value);
			if (num != PrimitiveValues.Count)
			{
				stringBuilder.Append(",");
			}
		}
	}

	private void AppendJObjects(StringBuilder stringBuilder)
	{
		if (InnerObjects.Count == 0)
		{
			return;
		}
		int num = 0;
		using Dictionary<string, BacktraceJObject>.Enumerator enumerator = InnerObjects.GetEnumerator();
		if (ShouldContinueAddingJSONProperties(stringBuilder))
		{
			stringBuilder.Append(',');
		}
		while (enumerator.MoveNext())
		{
			num++;
			KeyValuePair<string, BacktraceJObject> current = enumerator.Current;
			AppendKey(current.Key, stringBuilder);
			current.Value.ToJson(stringBuilder);
			if (num != InnerObjects.Count)
			{
				stringBuilder.Append(",");
			}
		}
	}

	private void AppendComplexValues(StringBuilder stringBuilder)
	{
		if (ComplexObjects.Count == 0)
		{
			return;
		}
		int num = 0;
		using Dictionary<string, object>.Enumerator enumerator = ComplexObjects.GetEnumerator();
		if (ShouldContinueAddingJSONProperties(stringBuilder))
		{
			stringBuilder.Append(',');
		}
		while (enumerator.MoveNext())
		{
			num++;
			KeyValuePair<string, object> current = enumerator.Current;
			AppendKey(current.Key, stringBuilder);
			if (current.Value == null)
			{
				stringBuilder.Append("null");
			}
			else if (current.Value is IEnumerable && !(current.Value is IDictionary))
			{
				stringBuilder.Append('[');
				int num2 = 0;
				foreach (object item in current.Value as IEnumerable)
				{
					if (num2 != 0)
					{
						stringBuilder.Append(',');
					}
					if (item == null)
					{
						stringBuilder.Append("\"\"");
					}
					else if (item is BacktraceJObject)
					{
						(item as BacktraceJObject).ToJson(stringBuilder);
					}
					else
					{
						stringBuilder.Append("\"");
						EscapeString(item.ToString(), stringBuilder);
						stringBuilder.Append("\"");
					}
					num2++;
				}
				stringBuilder.Append(']');
			}
			if (num != ComplexObjects.Count)
			{
				stringBuilder.Append(",");
			}
		}
	}

	private bool ShouldContinueAddingJSONProperties(StringBuilder stringBuilder)
	{
		if (stringBuilder[stringBuilder.Length - 1] != ',')
		{
			return stringBuilder[stringBuilder.Length - 1] != '{';
		}
		return false;
	}

	private void AppendKey(string value, StringBuilder builder)
	{
		builder.Append("\"");
		if (string.IsNullOrEmpty(value))
		{
			builder.Append("\"\"");
		}
		else
		{
			EscapeString(value, builder);
		}
		builder.Append("\":");
	}

	private void EscapeString(string value, StringBuilder output)
	{
		foreach (char c in value)
		{
			switch (c)
			{
			case '\\':
				output.Append("\\\\");
				continue;
			case '"':
				output.Append("\\\"");
				continue;
			case '\b':
				output.Append("\\b");
				continue;
			case '\t':
				output.Append("\\t");
				continue;
			case '\n':
				output.Append("\\n");
				continue;
			case '\f':
				output.Append("\\f");
				continue;
			case '\r':
				output.Append("\\r");
				continue;
			}
			if (char.GetUnicodeCategory(c) == UnicodeCategory.Control)
			{
				ToCharAsUnicodeToStringBuilder(c, output);
			}
			else
			{
				output.Append(c);
			}
		}
	}

	private char IntToHex(int n)
	{
		if (n <= 9)
		{
			return (char)(n + 48);
		}
		return (char)(n - 10 + 97);
	}

	private void ToCharAsUnicodeToStringBuilder(char c, StringBuilder output)
	{
		output.AppendFormat("\\u{0}{1}{2}{3}", IntToHex(((int)c >> 12) & 0xF), IntToHex(((int)c >> 8) & 0xF), IntToHex(((int)c >> 4) & 0xF), IntToHex(c & 0xF));
	}
}
