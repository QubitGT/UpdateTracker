using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MothershipHttpRunner : MonoBehaviour
{
	private static MothershipHttpRunner _instance;

	private readonly HashSet<UnityWebRequest> pendingRequests = new HashSet<UnityWebRequest>();

	public static MothershipHttpRunner instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	private static void CreateInstance()
	{
		if (_instance == null)
		{
			_instance = new GameObject(typeof(MothershipHttpRunner).Name).AddComponent<MothershipHttpRunner>();
		}
	}

	public virtual void Awake()
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		if (_instance != null)
		{
			_instance = null;
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	public void SendRequest(UnityWebRequest uwr, MothershipHTTPRequest request, Action<MothershipHTTPResponse> responseCallback)
	{
		StartCoroutine(SendRequestInternal(uwr, request, responseCallback));
	}

	public static void CancelAllRequests()
	{
		if (_instance == null)
		{
			return;
		}
		_instance.StopAllCoroutines();
		foreach (UnityWebRequest pendingRequest in _instance.pendingRequests)
		{
			pendingRequest.Abort();
			pendingRequest.Dispose();
		}
		_instance.pendingRequests.Clear();
	}

	private IEnumerator SendRequestInternal(UnityWebRequest uwr, MothershipHTTPRequest request, Action<MothershipHTTPResponse> responseCallback)
	{
		SWIGTYPE_p_void cbData = request.cbData;
		pendingRequests.Add(uwr);
		yield return uwr.SendWebRequest();
		pendingRequests.Remove(uwr);
		MothershipHTTPResponse mothershipHTTPResponse = new MothershipHTTPResponse();
		mothershipHTTPResponse.statusCode = (int)uwr.responseCode;
		mothershipHTTPResponse.Body = uwr.downloadHandler.text;
		mothershipHTTPResponse.cbData = cbData;
		uwr.Dispose();
		responseCallback(mothershipHTTPResponse);
	}
}
