using System;

namespace UnityEngine.Localization.SmartFormat.Core.Formatting;

internal class DataNotReadyException : Exception
{
	public string Text { get; private set; }

	public DataNotReadyException()
	{
	}

	public DataNotReadyException(string text)
	{
		Text = text;
	}
}
