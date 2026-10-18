using System;
using System.Collections.Generic;

namespace Modio;

public static class Version
{
	private static readonly System.Version Current = new System.Version(2025, 6);

	private static readonly List<string> EnvironmentDetails = new List<string>();

	public static void AddEnvironmentDetails(string details)
	{
		EnvironmentDetails.Add(details);
	}

	public static string GetCurrent()
	{
		if (EnvironmentDetails.Count != 0)
		{
			return string.Format("modio.cs/{0} ({1})", Current, string.Join("; ", EnvironmentDetails));
		}
		return $"modio.cs/{Current}";
	}
}
