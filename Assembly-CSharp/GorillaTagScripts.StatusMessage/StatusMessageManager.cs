using System;
using System.Collections;
using System.Text;
using GorillaLocomotion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTagScripts.StatusMessage;

public sealed class StatusMessageManager : MonoBehaviour
{
	private const string StaticConfigUrl = "https://static.aa-mothership.com/index.json";

	private const string StatusMessageKey = "f3e9fb19_status_message";

	private const int RequestTimeoutSeconds = 10;

	private const float DisplayDuration = 10f;

	[SerializeField]
	private float textSizePercent = 25f;

	[SerializeField]
	private int lineWidth = 40;

	[TextArea(1, 10)]
	[SerializeField]
	private string debugText;

	private IEnumerator Start()
	{
		string message = null;
		if (!string.IsNullOrWhiteSpace(debugText))
		{
			message = debugText;
		}
		else
		{
			yield return FetchStatusMessage(delegate(string m)
			{
				message = m;
			});
		}
		if (!string.IsNullOrWhiteSpace(message))
		{
			yield return ShowStatusMessage(message);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private static IEnumerator FetchStatusMessage(Action<string> onComplete)
	{
		using UnityWebRequest request = UnityWebRequest.Get("https://static.aa-mothership.com/index.json");
		request.timeout = 10;
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError("[StatusMessageManager] Failed to fetch static config: " + request.error);
			yield break;
		}
		try
		{
			if (JObject.Parse(request.downloadHandler.text)["f3e9fb19_status_message"] is JValue { Type: JTokenType.String } jValue)
			{
				onComplete((string?)(JToken?)jValue);
			}
		}
		catch (JsonException ex)
		{
			Debug.LogError("[StatusMessageManager] Failed to parse static config: " + ex.Message);
		}
	}

	private IEnumerator ShowStatusMessage(string message)
	{
		while (!GTPlayer.hasInstance)
		{
			yield return null;
		}
		while (!GorillaTagger.hasInstance || !GorillaTagger.Instance.mainCamera)
		{
			yield return null;
		}
		while (PrivateUIRoom.GetInOverlay())
		{
			yield return null;
		}
		PersistLog.Log("[StatusMessageManager] Showing status message: " + message);
		PrivateUIRoom.ForceStartOverlay(PrivateUIRoom.OverlaySource.StatusMessage, FormattableString.Invariant($"<size={textSizePercent}%>{WordWrap(message.ToUpperInvariant(), lineWidth)}</size>"));
		yield return new WaitForSeconds(10f);
		PrivateUIRoom.StopForcedOverlay(PrivateUIRoom.OverlaySource.StatusMessage);
	}

	private static string WordWrap(string text, int maxLineLength)
	{
		if (maxLineLength <= 0)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length + text.Length / maxLineLength);
		string[] array = text.Replace("\r\n", "\n").Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append('\n');
			}
			int num = 0;
			string[] array2 = array[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < array2.Length; j++)
			{
				string text2 = array2[j];
				if (num > 0 && num + 1 + text2.Length > maxLineLength)
				{
					stringBuilder.Append('\n');
					num = 0;
				}
				else if (num > 0)
				{
					stringBuilder.Append(' ');
					num++;
				}
				while (text2.Length > maxLineLength - num)
				{
					int num2 = maxLineLength - num;
					stringBuilder.Append(text2, 0, num2).Append('\n');
					text2 = text2.Substring(num2);
					num = 0;
				}
				stringBuilder.Append(text2);
				num += text2.Length;
			}
		}
		return stringBuilder.ToString();
	}
}
