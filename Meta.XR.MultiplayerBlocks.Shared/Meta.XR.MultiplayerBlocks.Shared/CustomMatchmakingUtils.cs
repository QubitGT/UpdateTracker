using System;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Shared;

internal static class CustomMatchmakingUtils
{
	internal static MatchInfo DecodeMatchInfoWithStruct(string matchInfoString)
	{
		if (string.IsNullOrEmpty(matchInfoString))
		{
			throw new InvalidOperationException("matchInfoString can not be null or empty");
		}
		try
		{
			return SerializationUtils.DeserializeFromString<MatchInfo>(matchInfoString);
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"Failed to decode the matchInfo from string {matchInfoString}, {arg}");
			return default(MatchInfo);
		}
	}

	internal static string EncodeMatchInfoWithStruct(string roomId, string roomPassword = null, string extra = null)
	{
		if (string.IsNullOrEmpty(roomId))
		{
			throw new InvalidOperationException("roomId can not be null or empty");
		}
		MatchInfo obj = default(MatchInfo);
		obj.RoomId = roomId;
		obj.RoomPassword = roomPassword;
		obj.Extra = extra;
		return SerializationUtils.SerializeToString(obj);
	}

	public static (string, string) ExtractMatchInfoFromSessionId(string matchSessionId)
	{
		if (string.IsNullOrEmpty(matchSessionId))
		{
			throw new InvalidOperationException("matchSessionId can not be null or empty");
		}
		if (!matchSessionId.Contains(":"))
		{
			return (matchSessionId, null);
		}
		string[] array = matchSessionId.Split(':');
		(string, string) result = array.Length switch
		{
			0 => (null, null), 
			1 => (array[0], null), 
			_ => (array[0], array[1]), 
		};
		if (result.Item1 == string.Empty)
		{
			result.Item1 = null;
		}
		if (result.Item2 == string.Empty)
		{
			result.Item2 = null;
		}
		return result;
	}

	public static string EncodeMatchInfoToSessionId(string roomId, string roomPassword = null)
	{
		if (string.IsNullOrEmpty(roomId))
		{
			throw new InvalidOperationException("roomId can not be null or empty");
		}
		if (!string.IsNullOrEmpty(roomPassword))
		{
			return roomId + ":" + roomPassword;
		}
		return roomId;
	}
}
