using System;
using System.Collections.Generic;

namespace Photon.Voice;

public interface IVoiceTransport
{
	bool IsChannelJoined(int channelId);

	void SendVoicesInfo(IEnumerable<LocalVoice> voices, int channelId, int targetPlayerId);

	void SendVoiceRemove(LocalVoice voice, int channelId, int targetPlayerId);

	void SendFrame(ArraySegment<byte> data, FrameFlags flags, byte evNumber, byte voiceId, int channelId, int targetPlayerId, bool reliable, LocalVoice localVoice);

	string ChannelIdStr(int channelId);

	string PlayerIdStr(int playerId);
}
