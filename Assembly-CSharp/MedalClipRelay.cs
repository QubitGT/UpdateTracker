using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MedalClipRelay : MonoBehaviour, IGorillaSliceableSimple
{
	[Serializable]
	public class MedalClip
	{
		public enum ClipType
		{
			Tag,
			TagStreak
		}

		[SerializeField]
		private ClipType clipType;

		[SerializeField]
		private string eventId;

		[SerializeField]
		private string eventName;

		[SerializeField]
		private int duration = 15;

		[SerializeField]
		private int captureDelayMs = 10000;

		[SerializeField]
		private int reqOccurances = 1;

		[SerializeField]
		private float reqTimeWindow;

		public string Payload => GetPayload(EventId, eventName, Duration, captureDelayMs);

		public ClipType MyClipType => clipType;

		public string EventId => eventId;

		public string EventName => eventName;

		public int ReqOccurances => reqOccurances;

		public float ReqTimeWindow => reqTimeWindow;

		public int Duration
		{
			get
			{
				return duration;
			}
			set
			{
				duration = value;
			}
		}

		public int CaptureDelayMs
		{
			get
			{
				return captureDelayMs;
			}
			set
			{
				captureDelayMs = value;
			}
		}

		public static string GetPayload(string eventId, string eventName, int duration, int captureDelayMs)
		{
			return $"{{\"eventId\":\"{eventId}\",\"eventName\":\"{eventName}\",\"triggerActions\":[\"SaveClip\"],\"clipOptions\":{{\"duration\":{duration},\"captureDelayMs\":{captureDelayMs}}}}}";
		}
	}

	public class MedalClipStreak
	{
		public float firstOccuranceTime;

		public float lastOccuranceTime;

		public int occurances;

		public int reqOccurances;

		public float reqTimeWindow;
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

	private static Dictionary<MedalClip.ClipType, MedalClipStreak> _clipStreaks;

	private static List<MedalClip.ClipType> _clipsTypes;

	[RuntimeInitializeOnLoadMethod]
	private static void RuntimeInitializeOnLoad()
	{
		_url = string.Empty;
		_APIKey = string.Empty;
		_clips = null;
		_clipStreaks = null;
		_clipsTypes = null;
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
		_clipStreaks = new Dictionary<MedalClip.ClipType, MedalClipStreak>();
		_clipsTypes = new List<MedalClip.ClipType>();
	}

	private void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	private void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public static void Clip(MedalClip.ClipType ct)
	{
		if (_clips == null)
		{
			return;
		}
		MedalClip medalClip = _clips[ct];
		if (medalClip == null)
		{
			return;
		}
		if (medalClip.ReqOccurances > 1 && medalClip.ReqTimeWindow > 0f)
		{
			if (!_clipStreaks.ContainsKey(medalClip.MyClipType))
			{
				_clipStreaks.Add(medalClip.MyClipType, new MedalClipStreak
				{
					reqOccurances = medalClip.ReqOccurances,
					reqTimeWindow = medalClip.ReqTimeWindow,
					firstOccuranceTime = Time.time
				});
			}
			_clipStreaks[medalClip.MyClipType].lastOccuranceTime = Time.time;
			_clipStreaks[medalClip.MyClipType].occurances++;
		}
		else
		{
			UnityWebRequest unityWebRequest = UnityWebRequest.Post(_url, medalClip.Payload, "application/json; charset=utf-8");
			unityWebRequest.SetRequestHeader("publicKey", _APIKey);
			unityWebRequest.SendWebRequest();
		}
	}

	public void RequestClip(MedalClip.ClipType ct)
	{
		Clip(ct);
	}

	void IGorillaSliceableSimple.SliceUpdate()
	{
		if (_clips == null)
		{
			return;
		}
		_clipsTypes.Clear();
		foreach (KeyValuePair<MedalClip.ClipType, MedalClipStreak> clipStreak in _clipStreaks)
		{
			if (clipStreak.Value.lastOccuranceTime + clipStreak.Value.reqTimeWindow < Time.time)
			{
				if (_clips.ContainsKey(clipStreak.Key) && clipStreak.Value.occurances >= clipStreak.Value.reqOccurances)
				{
					int duration = Mathf.CeilToInt(clipStreak.Value.lastOccuranceTime - clipStreak.Value.firstOccuranceTime + (float)_clips[clipStreak.Key].CaptureDelayMs / 1000f - clipStreak.Value.reqTimeWindow);
					UnityWebRequest unityWebRequest = UnityWebRequest.Post(_url, MedalClip.GetPayload(_clips[clipStreak.Key].EventId, _clips[clipStreak.Key].EventName, duration, _clips[clipStreak.Key].CaptureDelayMs), "application/json; charset=utf-8");
					unityWebRequest.SetRequestHeader("publicKey", _APIKey);
					unityWebRequest.SendWebRequest();
				}
				_clipsTypes.Add(clipStreak.Key);
			}
		}
		foreach (MedalClip.ClipType clipsType in _clipsTypes)
		{
			_clipStreaks.Remove(clipsType);
		}
	}
}
