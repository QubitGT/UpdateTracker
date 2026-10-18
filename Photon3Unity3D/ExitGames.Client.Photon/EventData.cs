namespace ExitGames.Client.Photon;

public class EventData
{
	public byte Code;

	public readonly ParameterDictionary Parameters;

	public byte SenderKey = 254;

	private int sender = -1;

	public byte CustomDataKey = 245;

	private object customData;

	public object this[byte key]
	{
		get
		{
			Parameters.TryGetValue(key, out var value);
			return value;
		}
		internal set
		{
			Parameters.Add(key, value);
		}
	}

	public int Sender
	{
		get
		{
			if (sender == -1)
			{
				int value;
				bool flag = Parameters.TryGetValue(SenderKey, out value);
				sender = (flag ? value : (-1));
			}
			return sender;
		}
		internal set
		{
			sender = value;
		}
	}

	public object CustomData
	{
		get
		{
			if (customData == null)
			{
				Parameters.TryGetValue(CustomDataKey, out customData);
			}
			return customData;
		}
		internal set
		{
			customData = value;
		}
	}

	public EventData()
	{
		Parameters = new ParameterDictionary();
	}

	internal void Reset()
	{
		Code = 0;
		Parameters.Clear();
		sender = -1;
		customData = null;
	}

	public override string ToString()
	{
		return $"Event {Code.ToString()}.";
	}

	public string ToStringFull()
	{
		return $"Event {Code}: {SupportClass.DictionaryToString(Parameters)}";
	}
}
