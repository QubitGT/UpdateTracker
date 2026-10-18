using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Voice;

internal class PhotonTransportProtocol
{
	private enum EventSubcode : byte
	{
		VoiceInfo = 1,
		VoiceRemove,
		Frame
	}

	private enum EventParam : byte
	{
		VoiceId = 1,
		SamplingRate,
		Channels,
		FrameDurationUs,
		Bitrate,
		Width,
		Height,
		FPS,
		KeyFrameInt,
		UserData,
		EventNumber,
		Codec
	}

	private VoiceClient voiceClient;

	private ILogger logger;

	public PhotonTransportProtocol(VoiceClient voiceClient, ILogger logger)
	{
		this.voiceClient = voiceClient;
		this.logger = logger;
	}

	internal object[] buildVoicesInfo(IEnumerable<LocalVoice> voicesToSend, bool logInfo)
	{
		object[] array = new object[voicesToSend.Count()];
		object[] result = new object[3]
		{
			(byte)0,
			EventSubcode.VoiceInfo,
			array
		};
		int num = 0;
		foreach (LocalVoice item in voicesToSend)
		{
			array[num] = new Dictionary<byte, object>
			{
				{ 1, item.ID },
				{
					12,
					item.Info.Codec
				},
				{
					2,
					item.Info.SamplingRate
				},
				{
					3,
					item.Info.Channels
				},
				{
					4,
					item.Info.FrameDurationUs
				},
				{
					5,
					item.Info.Bitrate
				},
				{
					6,
					item.Info.Width
				},
				{
					7,
					item.Info.Height
				},
				{
					8,
					item.Info.FPS
				},
				{
					9,
					item.Info.KeyFrameInt
				},
				{
					10,
					item.Info.UserData
				},
				{ 11, item.EvNumber }
			};
			num++;
			if (logInfo)
			{
				logger.LogInfo(item.LogPrefix + " Sending info: " + item.Info.ToString() + " ev=" + item.EvNumber);
			}
		}
		return result;
	}

	internal object[] buildVoiceRemoveMessage(LocalVoice v)
	{
		byte[] array = new byte[1] { v.ID };
		object[] result = new object[3]
		{
			(byte)0,
			EventSubcode.VoiceRemove,
			array
		};
		logger.LogInfo(v.LogPrefix + " remove sent");
		return result;
	}

	internal object[] buildFrameMessage(byte voiceId, byte evNumber, ArraySegment<byte> data, FrameFlags flags)
	{
		return new object[4]
		{
			voiceId,
			evNumber,
			data,
			(byte)flags
		};
	}

	internal void onVoiceEvent(object content0, int channelId, int playerId, bool isLocalPlayer)
	{
		object[] array = (object[])content0;
		if ((byte)array[0] == 0)
		{
			switch ((byte)array[1])
			{
			case 1:
				onVoiceInfo(channelId, playerId, array[2]);
				break;
			case 2:
				onVoiceRemove(channelId, playerId, array[2]);
				break;
			default:
				logger.LogError("[PV] Unknown sevent subcode " + array[1]);
				break;
			}
			return;
		}
		byte voiceId = (byte)array[0];
		byte evNumber = (byte)array[1];
		byte[] array2 = (byte[])array[2];
		FrameFlags flags = (FrameFlags)0;
		if (array.Length > 3)
		{
			flags = (FrameFlags)array[3];
		}
		FrameBuffer receivedBytes = new FrameBuffer(array2, flags);
		voiceClient.onFrame(channelId, playerId, voiceId, evNumber, ref receivedBytes, isLocalPlayer);
		receivedBytes.Release();
	}

	private void onVoiceInfo(int channelId, int playerId, object payload)
	{
		object[] array = (object[])payload;
		for (int i = 0; i < array.Length; i++)
		{
			Dictionary<byte, object> dictionary = (Dictionary<byte, object>)array[i];
			byte voiceId = (byte)dictionary[1];
			byte eventNumber = (byte)dictionary[11];
			VoiceInfo info = createVoiceInfoFromEventPayload(dictionary);
			voiceClient.onVoiceInfo(channelId, playerId, voiceId, eventNumber, info);
		}
	}

	private void onVoiceRemove(int channelId, int playerId, object payload)
	{
		byte[] voiceIds = (byte[])payload;
		voiceClient.onVoiceRemove(channelId, playerId, voiceIds);
	}

	private VoiceInfo createVoiceInfoFromEventPayload(Dictionary<byte, object> h)
	{
		VoiceInfo result = default(VoiceInfo);
		result.Codec = (Codec)h[12];
		result.SamplingRate = (int)h[2];
		result.Channels = (int)h[3];
		result.FrameDurationUs = (int)h[4];
		result.Bitrate = (int)h[5];
		if (h.ContainsKey(6))
		{
			result.Width = (int)h[6];
		}
		if (h.ContainsKey(7))
		{
			result.Height = (int)h[7];
		}
		if (h.ContainsKey(8))
		{
			result.FPS = (int)h[8];
		}
		if (h.ContainsKey(9))
		{
			result.KeyFrameInt = (int)h[9];
		}
		result.UserData = h[10];
		return result;
	}
}
