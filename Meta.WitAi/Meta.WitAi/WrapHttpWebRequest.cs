using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace Meta.WitAi;

public class WrapHttpWebRequest : IRequest
{
	private HttpWebRequest _httpWebRequest;

	public WebHeaderCollection Headers
	{
		get
		{
			return _httpWebRequest.Headers;
		}
		set
		{
			_httpWebRequest.Headers = value;
		}
	}

	public string Method
	{
		get
		{
			return _httpWebRequest.Method;
		}
		set
		{
			_httpWebRequest.Method = value;
		}
	}

	public string ContentType
	{
		get
		{
			return _httpWebRequest.ContentType;
		}
		set
		{
			_httpWebRequest.ContentType = value;
		}
	}

	public long ContentLength
	{
		get
		{
			return _httpWebRequest.ContentLength;
		}
		set
		{
			_httpWebRequest.ContentLength = value;
		}
	}

	public bool SendChunked
	{
		get
		{
			return _httpWebRequest.SendChunked;
		}
		set
		{
			_httpWebRequest.SendChunked = value;
		}
	}

	public string UserAgent
	{
		get
		{
			return _httpWebRequest.UserAgent;
		}
		set
		{
			_httpWebRequest.UserAgent = value;
		}
	}

	public int Timeout
	{
		get
		{
			return _httpWebRequest.Timeout;
		}
		set
		{
			_httpWebRequest.Timeout = value;
		}
	}

	public WrapHttpWebRequest(HttpWebRequest httpWebRequest)
	{
		if (Application.isBatchMode)
		{
			httpWebRequest.KeepAlive = false;
		}
		_httpWebRequest = httpWebRequest;
	}

	public void Abort()
	{
		_httpWebRequest.Abort();
	}

	public void Dispose()
	{
		_httpWebRequest.Abort();
		_httpWebRequest = null;
	}

	public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
	{
		return _httpWebRequest.BeginGetRequestStream(callback, state);
	}

	public IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
	{
		return _httpWebRequest.BeginGetResponse(callback, state);
	}

	public Stream EndGetRequestStream(IAsyncResult asyncResult)
	{
		return _httpWebRequest.EndGetRequestStream(asyncResult);
	}

	public WebResponse EndGetResponse(IAsyncResult asyncResult)
	{
		return _httpWebRequest.EndGetResponse(asyncResult);
	}
}
