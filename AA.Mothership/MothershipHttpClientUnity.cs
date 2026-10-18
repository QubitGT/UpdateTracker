using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MothershipHttpClientUnity : MothershipSendHTTPRequestDelegateWrapper
{
	private MothershipClientApiClient client;

	private bool isRequestLoggingEnabled;

	public MothershipHttpClientUnity(MothershipClientApiClient client, bool isRequestLoggingEnabled)
	{
		swigCMemOwn = false;
		this.client = client;
		this.isRequestLoggingEnabled = isRequestLoggingEnabled;
	}

	public override bool SendRequest(MothershipHTTPRequest request)
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(request.Path, request.Verb.ToString());
		byte[] data = null;
		if (request.Verb == MothershipHTTPVerbs.POST)
		{
			data = new UTF8Encoding().GetBytes(request.Body);
		}
		if (isRequestLoggingEnabled)
		{
			Debug.Log($"Mothership request body: {request.Body}");
		}
		unityWebRequest.uploadHandler = new UploadHandlerRaw(data);
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		foreach (MothershipHttpHeader requestHeader in request.RequestHeaders)
		{
			if (isRequestLoggingEnabled)
			{
				Debug.Log($"Mothership request header: {requestHeader.Name} {requestHeader.Value})");
			}
			unityWebRequest.SetRequestHeader(requestHeader.Name, requestHeader.Value);
		}
		unityWebRequest.timeout = 15;
		MothershipHttpRunner.instance.SendRequest(unityWebRequest, request, delegate(MothershipHTTPResponse Response)
		{
			if (isRequestLoggingEnabled)
			{
				Debug.Log($"Mothership: Request to {request.Path} status {Response.statusCode}");
			}
			if (!(MothershipApiClient.getCPtr(client).Handle == IntPtr.Zero))
			{
				client.ReceiveHttpResponse(Response);
			}
		});
		return true;
	}
}
