using System;

namespace VYaml.Parser;

public class Tag : ITokenContent
{
	public string Handle { get; }

	public string Suffix { get; }

	public Tag(string handle, string suffix)
	{
		Handle = handle;
		Suffix = suffix;
	}

	public override string ToString()
	{
		return Handle + Suffix;
	}

	public bool Equals(string tagString)
	{
		if (tagString.Length != Handle.Length + Suffix.Length)
		{
			return false;
		}
		int num = tagString.IndexOf(Handle, StringComparison.Ordinal);
		if (num < 0)
		{
			return false;
		}
		return tagString.IndexOf(Suffix, num, StringComparison.Ordinal) > 0;
	}
}
