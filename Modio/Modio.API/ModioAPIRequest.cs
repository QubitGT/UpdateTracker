using System;
using System.Collections.Generic;
using System.Linq;

namespace Modio.API;

public class ModioAPIRequest : IDisposable
{
	private static readonly List<ModioAPIRequest> Pool = new List<ModioAPIRequest>();

	private string Uri { get; set; }

	public ModioAPIRequestOptions Options { get; } = new ModioAPIRequestOptions();


	public ModioAPIRequestMethod Method { get; private set; }

	public ModioAPIRequestContentType ContentType { get; private set; }

	public string ContentTypeHint { get; private set; } = "";


	private ModioAPIRequest()
	{
	}

	internal static ModioAPIRequest New(string uri, ModioAPIRequestMethod method = ModioAPIRequestMethod.Get, ModioAPIRequestContentType contentType = ModioAPIRequestContentType.None, string contentTypeHint = "")
	{
		ModioAPIRequest modioAPIRequest;
		lock (Pool)
		{
			if (Pool.Count == 0)
			{
				modioAPIRequest = new ModioAPIRequest();
			}
			else
			{
				int index = Pool.Count - 1;
				modioAPIRequest = Pool[index];
				Pool.RemoveAt(index);
			}
		}
		modioAPIRequest.Uri = uri;
		modioAPIRequest.Method = method;
		modioAPIRequest.ContentType = contentType;
		modioAPIRequest.ContentTypeHint = contentTypeHint;
		return modioAPIRequest;
	}

	public string GetUri(List<string> defaultParameters)
	{
		string[] array = new string[defaultParameters.Count + Options.QueryParameters.Count];
		if (array.Length == 0)
		{
			return Uri;
		}
		Options.QueryParameters.Select((KeyValuePair<string, string> key) => key.Key + "=" + key.Value).ToArray().CopyTo(array, 0);
		defaultParameters.CopyTo(array, Options.QueryParameters.Count);
		if (array.Length != 0)
		{
			return Uri + ((Uri.LastIndexOf('?') == -1) ? "?" : "&") + string.Join("&", array);
		}
		return Uri;
	}

	public void Dispose()
	{
		Options.Dispose();
		Method = ModioAPIRequestMethod.Get;
		ContentType = ModioAPIRequestContentType.None;
		lock (Pool)
		{
			Pool.Add(this);
		}
	}
}
