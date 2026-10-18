using System.Collections.Generic;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model;

public class BacktraceSourceCode
{
	internal static string SOURCE_CODE_PROPERTY = "main";

	public readonly string Type = "Text";

	public readonly string Title = "Log File";

	public string Text { get; set; }

	internal BacktraceJObject ToJson()
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		BacktraceJObject backtraceJObject2 = new BacktraceJObject(new Dictionary<string, string>
		{
			{ "id", SOURCE_CODE_PROPERTY },
			{ "type", Type },
			{ "title", Title },
			{ "text", Text }
		});
		backtraceJObject2.Add("highlightLine", value: false);
		backtraceJObject.Add(SOURCE_CODE_PROPERTY, backtraceJObject2);
		return backtraceJObject;
	}
}
