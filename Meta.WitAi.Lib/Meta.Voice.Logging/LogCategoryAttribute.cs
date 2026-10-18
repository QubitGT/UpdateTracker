using System;

namespace Meta.Voice.Logging;

[AttributeUsage(AttributeTargets.Class)]
public class LogCategoryAttribute : Attribute
{
	public string CategoryName { get; }

	public string ParentCategoryName { get; }

	public LogCategoryAttribute(string categoryName)
	{
		CategoryName = categoryName;
	}

	public LogCategoryAttribute(LogCategory categoryName)
	{
		CategoryName = categoryName.ToString();
	}

	public LogCategoryAttribute(string parentCategoryName, string categoryName)
	{
		ParentCategoryName = parentCategoryName;
		CategoryName = categoryName;
	}

	public LogCategoryAttribute(LogCategory parentCategoryName, LogCategory categoryName)
		: this(parentCategoryName.ToString(), categoryName.ToString())
	{
	}
}
