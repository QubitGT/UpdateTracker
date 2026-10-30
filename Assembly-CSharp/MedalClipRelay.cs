using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MedalClipRelay : MonoBehaviour
{
	[Serializable]
	public class MedalClip
	{
		public enum ClipType
		{
			Misc,
			Tag
		}

		[SerializeField]
		private ClipType clipType;

		[SerializeField]
		private int duration = 15;

		[SerializeField]
		private int captureDelayMs = 10000;

		public string Payload => $"{{\"eventId\":\"1\",\"eventName\":\"{clipType}\",\"triggerActions\":[\"SaveClip\"],\"clipOptions\":{{\"duration\":{duration},\"captureDelayMs\":{captureDelayMs}}}}}";

		public ClipType MyClipType => clipType;
	}

	[SerializeField]
	private string url = "http://localhost:12665/api/v1/event/invoke";

	[SerializeField]
	private string APIKey = "pub_WsOlLdOhswUDwW1hwF3qxj0VwqVTO3U7";

	[SerializeField]
	private MedalClip[] clips;

	private static string _url;

	private static string _APIKey;

	private static Dictionary<MedalClip.ClipType, MedalClip> _clips;

	[RuntimeInitializeOnLoadMethod]
	private static void RuntimeInitializeOnLoad()
	{
		_url = string.Empty;
		_APIKey = string.Empty;
		_clips = null;
	}

	private void Start()
	{
		_url = url;
		_APIKey = APIKey;
		_clips = new Dictionary<MedalClip.ClipType, MedalClip>();
		for (int i = 0; i < clips.Length; i++)
		{
			_clips.Add(clips[i].MyClipType, clips[i]);
		}
	}

	public static void Clip(MedalClip.ClipType ct)
	{
		if (_clips != null)
		{
			MedalClip medalClip = _clips[ct];
			if (medalClip != null)
			{
				UnityWebRequest unityWebRequest = UnityWebRequest.Post(_url, medalClip.Payload, "application/json; charset=utf-8");
				unityWebRequest.SetRequestHeader("publicKey", _APIKey);
				unityWebRequest.SendWebRequest();
			}
		}
	}

	public void RequestClip(MedalClip.ClipType ct)
	{
		Clip(ct);
	}
}
