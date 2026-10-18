using System;
using System.Diagnostics;
using UnityEngine.Networking;

namespace UnityEngine.ResourceManagement.Util;

public class UnityWebRequestUtilities
{
	private const string k_AddressablesLogConditional = "ADDRESSABLES_LOG_ALL";

	public static bool RequestHasErrors(UnityWebRequest webReq, out UnityWebRequestResult result)
	{
		result = null;
		if (webReq == null || !webReq.isDone)
		{
			return false;
		}
		switch (webReq.result)
		{
		case UnityWebRequest.Result.InProgress:
		case UnityWebRequest.Result.Success:
			return false;
		case UnityWebRequest.Result.ConnectionError:
		case UnityWebRequest.Result.ProtocolError:
		case UnityWebRequest.Result.DataProcessingError:
			result = new UnityWebRequestResult(webReq);
			return true;
		default:
			throw new NotImplementedException($"Cannot determine whether UnityWebRequest succeeded or not from result : {webReq.result}");
		}
	}

	public static bool IsAssetBundleDownloaded(UnityWebRequestAsyncOperation op)
	{
		DownloadHandlerAssetBundle downloadHandlerAssetBundle = (DownloadHandlerAssetBundle)op.webRequest.downloadHandler;
		if (downloadHandlerAssetBundle != null && downloadHandlerAssetBundle.autoLoadAssetBundle)
		{
			return downloadHandlerAssetBundle.isDownloadComplete;
		}
		return op.isDone;
	}

	internal static void LogOperationResult(AsyncOperation op)
	{
		if (op is UnityWebRequestAsyncOperation unityWebRequestAsyncOperation)
		{
			UnityWebRequestResult unityWebRequestResult = new UnityWebRequestResult(unityWebRequestAsyncOperation.webRequest);
			if (unityWebRequestResult.Result != UnityWebRequest.Result.Success)
			{
				LogError(unityWebRequestResult.ToString());
			}
		}
	}

	[Conditional("ADDRESSABLES_LOG_ALL")]
	internal static void Log(string msg)
	{
		Debug.Log(msg);
	}

	internal static void LogError(string msg)
	{
		Debug.LogError(msg);
	}
}
