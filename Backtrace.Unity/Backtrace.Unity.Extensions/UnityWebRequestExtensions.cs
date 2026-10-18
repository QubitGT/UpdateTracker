using System.Text;
using Backtrace.Unity.Model;
using UnityEngine.Networking;

namespace Backtrace.Unity.Extensions;

public static class UnityWebRequestExtensions
{
	internal const string ContentTypeHeader = "Content-Type";

	internal static UnityWebRequest SetMultipartFormData(this UnityWebRequest source, byte[] boundaryId)
	{
		source.SetRequestHeader("Content-Type", string.Format("{0}{1}", "multipart/form-data; boundary=", Encoding.UTF8.GetString(boundaryId)));
		return source;
	}

	public static bool ReceivedNetworkError(this UnityWebRequest request)
	{
		if (request.result != UnityWebRequest.Result.ConnectionError)
		{
			return request.result == UnityWebRequest.Result.ProtocolError;
		}
		return true;
	}

	internal static UnityWebRequest SetJsonContentType(this UnityWebRequest source)
	{
		source.SetRequestHeader("Content-Type", "application/json");
		return source;
	}

	internal static UnityWebRequest IgnoreSsl(this UnityWebRequest source, bool shouldIgnore)
	{
		if (shouldIgnore)
		{
			source.certificateHandler = new BacktraceSelfSSLCertificateHandler();
		}
		return source;
	}
}
