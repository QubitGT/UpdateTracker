using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaNetworking;
using GorillaTagScripts;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;

public class BananaMail : MonoBehaviour
{
	[Serializable]
	public class BananaMailMessage : IComparable<BananaMailMessage>
	{
		[SerializeField]
		private string subject;

		[SerializeField]
		private string body;

		[SerializeField]
		private string sent;

		[SerializeField]
		private string expires;

		private DateTime? dtSent;

		private DateTime? dtExpires;

		public int Id => (subject + body + sent).GetHashCode();

		public string Subject
		{
			get
			{
				return subject;
			}
			set
			{
				subject = value;
			}
		}

		public string Body
		{
			get
			{
				return body;
			}
			set
			{
				body = value;
			}
		}

		public DateTime Sent
		{
			get
			{
				if (dtSent.HasValue)
				{
					return dtSent.Value;
				}
				try
				{
					dtSent = DateTime.Parse(sent);
				}
				catch
				{
					dtSent = DateTime.MinValue;
				}
				return dtSent.Value;
			}
			set
			{
				dtSent = value;
			}
		}

		public DateTime Expires
		{
			get
			{
				if (dtExpires.HasValue)
				{
					return dtExpires.Value;
				}
				try
				{
					dtExpires = DateTime.Parse(expires);
				}
				catch
				{
					dtExpires = DateTime.MaxValue;
				}
				return dtExpires.Value;
			}
			set
			{
				dtExpires = value;
			}
		}

		public int CompareTo(BananaMailMessage bmm)
		{
			return -Sent.CompareTo(bmm.Sent);
		}
	}

	public enum BananaMailStatus
	{
		none,
		loading,
		error,
		ready
	}

	[SerializeField]
	private string titleDataKey;

	[SerializeField]
	private BananaMailMessage[] messages;

	public static BananaMailMessage[] Messages;

	private static BananaMailStatus _status;

	public static int Unread;

	public static BananaMailStatus Status
	{
		get
		{
			return _status;
		}
		internal set
		{
			_status = value;
			BananaMail.BananaMailStatusChange?.Invoke();
		}
	}

	public static event Action BananaMailStatusChange;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		Unread = 0;
		Messages = null;
		_status = BananaMailStatus.none;
	}

	public static List<int> GetReadMessages()
	{
		List<int> list = new List<int>();
		string @string = PlayerPrefs.GetString("BananaMailRead", string.Empty);
		if (@string.Length > 0)
		{
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(int.Parse(array[i]));
			}
		}
		return list;
	}

	public static void MarkMessageRead(int id)
	{
		string @string = PlayerPrefs.GetString("BananaMailRead", string.Empty);
		@string = ((@string.Length != 0) ? $"{@string},{id}" : id.ToString());
		PlayerPrefs.SetString("BananaMailRead", @string);
	}

	public static (int, string) DisplayMessage(int index)
	{
		if (Status != BananaMailStatus.ready)
		{
			return (index, "NO BANANA\n\nTRY AGAIN LATER.\n\n");
		}
		if (index < Messages.Length)
		{
			MarkMessageRead(Messages[index].Id);
			return (index + 1, $"BANAN@MAIL ニ MSG {Messages.Length - index}/{Messages.Length}\nSENT: {Messages[index].Sent.ToShortDateString()}\nSUBJ: {Messages[index].Subject}\n\n{Messages[index].Body}");
		}
		return (0, "HAVE A BANANA!            \n\n                  *       \n                   #*     \n                   # #    \n                   #  *   \n                  #  #    \n                 *  #     \n                # #       \n                #*        \n");
	}

	private async void Start()
	{
		int tdCallsOut = 0;
		while (PlayFabTitleDataCache.Instance == null || GorillaComputer.instance == null || GorillaComputer.instance.GetServerTime().Year < 2000)
		{
			await Task.Delay(500);
		}
		List<BananaMailMessage> messages = new List<BananaMailMessage>();
		tdCallsOut++;
		PlayFabTitleDataCache.Instance.GetTitleData(titleDataKey, delegate(string data)
		{
			messages.AddRange(JsonConvert.DeserializeObject<BananaMailMessage[]>(data));
			tdCallsOut--;
		}, delegate(PlayFabError err)
		{
			Debug.Log($"BananaMail :: Start :: TitleData Error {err}");
			tdCallsOut--;
		});
		while (tdCallsOut > 0)
		{
			await Task.Yield();
		}
		SubscriptionManager.SubscriptionDetails subscriptionDetails = SubscriptionManager.LocalSubscriptionDetails();
		if (!subscriptionDetails.active && subscriptionDetails.daysAccrued > 0 && subscriptionDetails.subscriptionActiveUntilDate < GorillaComputer.instance.GetServerTime())
		{
			messages.Add(new BananaMailMessage
			{
				Subject = "WE'LL MISS YOU!!",
				Body = "Hello Very Important Monke!\n\nLooks like your Gorilla Tag Fan Club subscription has ended. I hope you had a good time and I'd love to see you come back soon! You can sign up any time at any GTFC Kiosk.\n\n   -Lemming",
				Sent = subscriptionDetails.subscriptionActiveUntilDate,
				Expires = subscriptionDetails.subscriptionActiveUntilDate.AddYears(1)
			});
		}
		Unread = 0;
		List<int> readMessages = GetReadMessages();
		List<int> list = new List<int>();
		for (int num = messages.Count - 1; num >= 0; num--)
		{
			if (!readMessages.Contains(messages[num].Id))
			{
				Unread++;
			}
			else
			{
				list.Add(messages[num].Id);
			}
		}
		string text = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			text = ((text.Length != 0) ? $"{text},{list[i]}" : list[i].ToString());
		}
		PlayerPrefs.SetString("BananaMailRead", text);
		messages.Sort();
		Messages = (this.messages = messages.ToArray());
		Status = BananaMailStatus.ready;
	}
}
