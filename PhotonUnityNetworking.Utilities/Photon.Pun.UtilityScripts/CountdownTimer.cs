using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts;

public class CountdownTimer : MonoBehaviourPunCallbacks
{
	public delegate void CountdownTimerHasExpired();

	public const string CountdownStartTime = "StartTime";

	[Header("Countdown time in seconds")]
	public float Countdown = 5f;

	private bool isTimerRunning;

	private int startTime;

	[Header("Reference to a Text component for visualizing the countdown")]
	public Text Text;

	public static event CountdownTimerHasExpired OnCountdownTimerHasExpired;

	public void Start()
	{
		if (Text == null)
		{
			Debug.LogError("Reference to 'Text' is not set. Please set a valid reference.", this);
		}
	}

	public override void OnEnable()
	{
		Debug.Log("OnEnable CountdownTimer");
		base.OnEnable();
		Initialize();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Debug.Log("OnDisable CountdownTimer");
	}

	public void Update()
	{
		if (isTimerRunning)
		{
			float num = TimeRemaining();
			Text.text = string.Format("Game starts in {0} seconds", num.ToString("n0"));
			if (!(num > 0f))
			{
				OnTimerEnds();
			}
		}
	}

	private void OnTimerRuns()
	{
		isTimerRunning = true;
		base.enabled = true;
	}

	private void OnTimerEnds()
	{
		isTimerRunning = false;
		base.enabled = false;
		Debug.Log("Emptying info text.", Text);
		Text.text = string.Empty;
		if (CountdownTimer.OnCountdownTimerHasExpired != null)
		{
			CountdownTimer.OnCountdownTimerHasExpired();
		}
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		Debug.Log("CountdownTimer.OnRoomPropertiesUpdate " + propertiesThatChanged.ToStringFull());
		Initialize();
	}

	private void Initialize()
	{
		if (TryGetStartTime(out var startTimestamp))
		{
			startTime = startTimestamp;
			Debug.Log("Initialize sets StartTime " + startTime + " server time now: " + PhotonNetwork.ServerTimestamp + " remain: " + TimeRemaining());
			isTimerRunning = TimeRemaining() > 0f;
			if (isTimerRunning)
			{
				OnTimerRuns();
			}
			else
			{
				OnTimerEnds();
			}
		}
	}

	private float TimeRemaining()
	{
		int num = PhotonNetwork.ServerTimestamp - startTime;
		return Countdown - (float)num / 1000f;
	}

	public static bool TryGetStartTime(out int startTimestamp)
	{
		startTimestamp = PhotonNetwork.ServerTimestamp;
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out var value))
		{
			startTimestamp = (int)value;
			return true;
		}
		return false;
	}

	public static void SetStartTime()
	{
		int startTimestamp = 0;
		bool flag = TryGetStartTime(out startTimestamp);
		Hashtable hashtable = new Hashtable { 
		{
			"StartTime",
			PhotonNetwork.ServerTimestamp
		} };
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
		Debug.Log("Set Custom Props for Time: " + hashtable.ToStringFull() + " wasSet: " + flag);
	}
}
