using System;
using System.Security.Principal;
using System.Text;
using WebSocketSharp.Net.WebSockets;

namespace WebSocketSharp.Net;

public sealed class HttpListenerContext
{
	private HttpConnection _connection;

	private string _errorMessage;

	private int _errorStatusCode;

	private HttpListener _listener;

	private HttpListenerRequest _request;

	private HttpListenerResponse _response;

	private IPrincipal _user;

	private HttpListenerWebSocketContext _websocketContext;

	internal HttpConnection Connection => _connection;

	internal string ErrorMessage
	{
		get
		{
			return _errorMessage;
		}
		set
		{
			_errorMessage = value;
		}
	}

	internal int ErrorStatusCode
	{
		get
		{
			return _errorStatusCode;
		}
		set
		{
			_errorStatusCode = value;
		}
	}

	internal bool HasErrorMessage => _errorMessage != null;

	internal HttpListener Listener
	{
		get
		{
			return _listener;
		}
		set
		{
			_listener = value;
		}
	}

	public HttpListenerRequest Request => _request;

	public HttpListenerResponse Response => _response;

	public IPrincipal User
	{
		get
		{
			return _user;
		}
		internal set
		{
			_user = value;
		}
	}

	internal HttpListenerContext(HttpConnection connection)
	{
		_connection = connection;
		_errorStatusCode = 400;
		_request = new HttpListenerRequest(this);
		_response = new HttpListenerResponse(this);
	}

	private static string createErrorContent(int statusCode, string statusDescription, string message)
	{
		return (message != null && message.Length > 0) ? $"<html><body><h1>{statusCode} {statusDescription} ({message})</h1></body></html>" : $"<html><body><h1>{statusCode} {statusDescription}</h1></body></html>";
	}

	internal HttpListenerWebSocketContext GetWebSocketContext(string protocol)
	{
		_websocketContext = new HttpListenerWebSocketContext(this, protocol);
		return _websocketContext;
	}

	internal void SendAuthenticationChallenge(AuthenticationSchemes scheme, string realm)
	{
		_response.StatusCode = 401;
		string value = new AuthenticationChallenge(scheme, realm).ToString();
		_response.Headers.InternalSet("WWW-Authenticate", value, response: true);
		_response.Close();
	}

	internal void SendError()
	{
		try
		{
			_response.StatusCode = _errorStatusCode;
			_response.ContentType = "text/html";
			string s = createErrorContent(_errorStatusCode, _response.StatusDescription, _errorMessage);
			Encoding uTF = Encoding.UTF8;
			byte[] bytes = uTF.GetBytes(s);
			_response.ContentEncoding = uTF;
			_response.ContentLength64 = bytes.LongLength;
			_response.Close(bytes, willBlock: true);
		}
		catch
		{
			_connection.Close(force: true);
		}
	}

	internal void SendError(int statusCode)
	{
		_errorStatusCode = statusCode;
		SendError();
	}

	internal void SendError(int statusCode, string message)
	{
		_errorStatusCode = statusCode;
		_errorMessage = message;
		SendError();
	}

	internal void Unregister()
	{
		if (_listener != null)
		{
			_listener.UnregisterContext(this);
		}
	}

	public HttpListenerWebSocketContext AcceptWebSocket(string protocol)
	{
		if (_websocketContext != null)
		{
			string message = "The accepting is already in progress.";
			throw new InvalidOperationException(message);
		}
		if (protocol != null)
		{
			if (protocol.Length == 0)
			{
				string message2 = "An empty string.";
				throw new ArgumentException(message2, "protocol");
			}
			if (!protocol.IsToken())
			{
				string message3 = "It contains an invalid character.";
				throw new ArgumentException(message3, "protocol");
			}
		}
		return GetWebSocketContext(protocol);
	}
}
