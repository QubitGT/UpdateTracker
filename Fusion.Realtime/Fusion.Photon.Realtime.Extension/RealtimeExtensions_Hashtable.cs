using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime.Extension;

internal static class RealtimeExtensions_Hashtable
{
	private static readonly StreamBuffer buffer = new StreamBuffer(1024);

	private static readonly Protocol18 protocol = new Protocol18();

	public static Dictionary<string, SessionProperty> ConvertToDictionaryProperty(this ExitGames.Client.Photon.Hashtable customProperties)
	{
		Dictionary<string, SessionProperty> dictionary = new Dictionary<string, SessionProperty>();
		foreach (DictionaryEntry customProperty in customProperties)
		{
			if (customProperty.Key is string key && SessionProperty.Support(customProperty.Value))
			{
				dictionary[key] = SessionProperty.Convert(customProperty.Value);
			}
		}
		return dictionary;
	}

	public static ExitGames.Client.Photon.Hashtable ConvertToHashtable(this Dictionary<string, SessionProperty> properties)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		foreach (KeyValuePair<string, SessionProperty> property in properties)
		{
			if (property.Key != null && property.Value != null)
			{
				hashtable[property.Key] = property.Value.PropertyValue;
			}
		}
		return hashtable;
	}

	public static int CalculateTotalSize(this ExitGames.Client.Photon.Hashtable hashtable)
	{
		buffer.Position = 0;
		protocol.Serialize(buffer, hashtable, setType: true);
		return buffer.Position;
	}
}
