using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Modio;

public static class ModioCommandLine
{
	private const string PREFIX = "-modio-";

	private static ReadOnlyDictionary<string, string> _argumentCache;

	public static bool TryGet(string argument, out string value)
	{
		if (_argumentCache == null)
		{
			GetArguments();
		}
		value = null;
		if (_argumentCache != null)
		{
			return _argumentCache.TryGetValue(argument, out value);
		}
		return false;
	}

	private static void GetArguments()
	{
		if (_argumentCache != null)
		{
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text = commandLineArgs[i];
			if (!text.StartsWith("-modio-"))
			{
				continue;
			}
			string[] array = text.Split('=');
			string key;
			string value;
			if (array.Length == 2)
			{
				key = array[0].Substring("-modio-".Length);
				value = array[1];
			}
			else
			{
				if (i + 1 >= commandLineArgs.Length)
				{
					continue;
				}
				key = text.Substring("-modio-".Length);
				value = commandLineArgs[i + 1];
			}
			dictionary[key] = value;
		}
		_argumentCache = new ReadOnlyDictionary<string, string>(dictionary);
	}
}
