using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Modio.API;

public class ModioAPIRequestOptions : IDisposable
{
	internal Dictionary<string, string> QueryParameters { get; } = new Dictionary<string, string>();


	internal Dictionary<string, string> HeaderParameters { get; } = new Dictionary<string, string>();


	internal bool RequiresAuthentication { get; private set; }

	internal Dictionary<string, string> FormParameters { get; } = new Dictionary<string, string>();


	internal Dictionary<string, ModioAPIFileParameter> FileParameters { get; } = new Dictionary<string, ModioAPIFileParameter>();


	public byte[] BodyDataBytes { get; private set; }

	public void Dispose()
	{
		RequiresAuthentication = false;
		HeaderParameters.Clear();
		QueryParameters.Clear();
		FormParameters.Clear();
		FileParameters.Clear();
	}

	internal void AddQueryParameter(string key, object value)
	{
		if (value != null)
		{
			QueryParameters.Add(key, ParameterToString(value));
		}
	}

	internal void AddHeaderParameter(string key, object value)
	{
		if (value != null)
		{
			HeaderParameters.Add(key, ParameterToString(value));
		}
	}

	internal void AddFilterParameters(SearchFilter filter)
	{
		if (filter == null)
		{
			return;
		}
		AddQueryParameter("_offset", filter.PageIndex * filter.PageSize);
		AddQueryParameter("_limit", filter.PageSize);
		foreach (KeyValuePair<string, object> parameter in filter.Parameters)
		{
			AddQueryParameter(parameter.Key, parameter.Value);
		}
	}

	public void RequireAuthentication()
	{
		RequiresAuthentication = true;
	}

	private static string ParameterToString(object value)
	{
		if (value == null)
		{
			return string.Empty;
		}
		if (value is IEnumerable<string> values)
		{
			return string.Join(",", values);
		}
		if (value is ICollection collection)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (object item in collection)
			{
				if (num++ > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(item);
			}
			return stringBuilder.ToString();
		}
		return $"{value}";
	}

	public void AddBody(byte[] data)
	{
		BodyDataBytes = data;
	}

	public void AddBody(IApiRequest request)
	{
		foreach (KeyValuePair<string, object> bodyParameter in request.GetBodyParameters())
		{
			if (bodyParameter.Value is ModioAPIFileParameter value)
			{
				FileParameters.Add(bodyParameter.Key, value);
			}
			else if (bodyParameter.Value != null)
			{
				FormParameters.Add(bodyParameter.Key, ParameterToString(bodyParameter.Value));
			}
		}
	}

	public void AddBody(IApiRequest request, string hint)
	{
		if (hint == "application/json")
		{
			string s = JsonConvert.SerializeObject(from param in request.GetBodyParameters()
				where param.Value != null
				select param);
			AddBody(Encoding.UTF8.GetBytes(s));
		}
		else
		{
			AddBody(request);
		}
	}
}
