using System;
using System.Collections.Generic;

namespace g3;

public class CommandArgumentSet
{
	public Dictionary<string, int> Integers = new Dictionary<string, int>();

	public Dictionary<string, float> Floats = new Dictionary<string, float>();

	public Dictionary<string, string> Strings = new Dictionary<string, string>();

	public Dictionary<string, bool> Flags = new Dictionary<string, bool>();

	public HashSet<string> SawArguments = new HashSet<string>();

	public List<string> Filenames = new List<string>();

	public void Register(string argument, int defaultValue)
	{
		Integers.Add(argument, defaultValue);
	}

	public void Register(string argument, float defaultValue)
	{
		Floats.Add(argument, defaultValue);
	}

	public void Register(string argument, bool defaultValue)
	{
		Flags.Add(argument, defaultValue);
	}

	public void Register(string argument, string defaultValue)
	{
		Strings.Add(argument, defaultValue);
	}

	public bool Saw(string argument)
	{
		return SawArguments.Contains(argument);
	}

	public bool Validate(string sParam, params string[] values)
	{
		if (!Strings.ContainsKey(sParam))
		{
			throw new Exception("Argument set does not contain " + sParam);
		}
		string text = Strings[sParam];
		for (int i = 0; i < values.Length; i++)
		{
			if (text == values[i])
			{
				return true;
			}
		}
		return false;
	}

	public bool Parse(string[] arguments)
	{
		int num = arguments.Length;
		int num2 = 0;
		while (num2 < num)
		{
			string text = arguments[num2];
			num2++;
			if (Integers.ContainsKey(text))
			{
				SawArguments.Add(text);
				if (num2 == num)
				{
					error_missing_argument(text);
					return false;
				}
				string text2 = arguments[num2];
				if (!int.TryParse(text2, out var result))
				{
					error_invalid_value(text, text2);
					return false;
				}
				Integers[text] = result;
				num2++;
			}
			else if (Floats.ContainsKey(text))
			{
				SawArguments.Add(text);
				if (num2 == num)
				{
					error_missing_argument(text);
					return false;
				}
				string text3 = arguments[num2];
				if (!float.TryParse(text3, out var result2))
				{
					error_invalid_value(text, text3);
					return false;
				}
				Floats[text] = result2;
				num2++;
			}
			else if (Strings.ContainsKey(text))
			{
				SawArguments.Add(text);
				if (num2 == num)
				{
					error_missing_argument(text);
					return false;
				}
				string value = arguments[num2];
				Strings[text] = value;
				num2++;
			}
			else if (Flags.ContainsKey(text))
			{
				SawArguments.Add(text);
				Flags[text] = true;
			}
			else
			{
				Filenames.Add(text);
			}
		}
		return true;
	}

	protected virtual void error_missing_argument(string arg)
	{
		Console.WriteLine("argument {0} is missing value", arg);
	}

	protected virtual void error_invalid_value(string arg, string value)
	{
		Console.WriteLine("argument {0} has invalid value {1}", arg, value);
	}
}
