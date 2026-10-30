using System;
using UnityEngine;

public static class BackendApiUrl
{
	public static string Build(string baseUrl, string path)
	{
		if (string.IsNullOrWhiteSpace(baseUrl))
		{
			throw new ArgumentException("The base URL is missing.", "baseUrl");
		}
		if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var result))
		{
			throw new ArgumentException("'" + baseUrl + "' is not an absolute URL.", "baseUrl");
		}
		string text = baseUrl + path;
		if (!Uri.TryCreate(text, UriKind.Absolute, out var result2) || !string.Equals(result.Host, result2.Host, StringComparison.OrdinalIgnoreCase) || !string.Equals(result.Scheme, result2.Scheme, StringComparison.OrdinalIgnoreCase))
		{
			Debug.LogError("[BackendApiUrl] Refusing request: built URL '" + text + "' has a different host or scheme than base URL '" + baseUrl + "'.");
			throw new InvalidOperationException("Refusing to build a backend API URL whose host or scheme does not match its base URL.");
		}
		return text;
	}
}
