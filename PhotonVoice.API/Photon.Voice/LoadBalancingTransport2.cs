using System;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Voice;

public class LoadBalancingTransport2 : LoadBalancingTransport
{
	private const int DATA_OFFSET = 4;

	public LoadBalancingTransport2(ILogger logger = null, ConnectionProtocol connectionProtocol = ConnectionProtocol.Udp)
		: base(logger, connectionProtocol)
	{
		base.LoadBalancingPeer.UseByteArraySlicePoolForEvents = true;
		base.LoadBalancingPeer.ReuseEventInstance = true;
	}

	public override void SendFrame(ArraySegment<byte> data, FrameFlags flags, byte evNumber, byte voiceId, int channelId, int targetPlayerId, bool reliable, LocalVoice localVoice)
	{
		ByteArraySlice byteArraySlice = base.LoadBalancingPeer.ByteArraySlicePool.Acquire(data.Count + 4);
		byteArraySlice.Buffer[0] = 4;
		byteArraySlice.Buffer[1] = voiceId;
		byteArraySlice.Buffer[2] = evNumber;
		byteArraySlice.Buffer[3] = (byte)flags;
		Buffer.BlockCopy(data.Array, 0, byteArraySlice.Buffer, 4, data.Count);
		byteArraySlice.Count = data.Count + 4;
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = reliable;
		sendOptions.Channel = photonChannelForCodec(localVoice.Info.Codec);
		sendOptions.Encrypt = localVoice.Encrypt;
		SendOptions sendOptions2 = sendOptions;
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		switch (targetPlayerId)
		{
		case -1:
			raiseEventOptions.TargetActors = new int[1] { base.LocalPlayer.ActorNumber };
			break;
		default:
			raiseEventOptions.TargetActors = new int[1] { targetPlayerId };
			break;
		case 0:
			break;
		}
		if (localVoice.DebugEchoMode)
		{
			raiseEventOptions.Receivers = ReceiverGroup.All;
		}
		raiseEventOptions.InterestGroup = localVoice.InterestGroup;
		OpRaiseEvent(203, byteArraySlice, raiseEventOptions, sendOptions2);
		while (base.LoadBalancingPeer.SendOutgoingCommands())
		{
		}
	}

	protected override void onEventActionVoiceClient(EventData ev)
	{
		if (ev.Code == 203)
		{
			onVoiceFrameEvent(ev[245], 0, ev.Sender, base.LocalPlayer.ActorNumber);
		}
		else
		{
			base.onEventActionVoiceClient(ev);
		}
	}

	internal void onVoiceFrameEvent(object content0, int channelId, int playerId, int localPlayerId)
	{
		int num = 0;
		ByteArraySlice byteArraySlice = content0 as ByteArraySlice;
		byte[] array;
		int num2;
		if (byteArraySlice != null)
		{
			array = byteArraySlice.Buffer;
			num2 = byteArraySlice.Count;
			num = byteArraySlice.Offset;
		}
		else
		{
			array = content0 as byte[];
			num2 = array.Length;
		}
		if (array == null || num2 < 3)
		{
			LogError("[PV] onVoiceFrameEvent did not receive data (readable as byte[]) " + content0);
			return;
		}
		byte b = array[num];
		byte voiceId = array[num + 1];
		byte evNumber = array[num + 2];
		FrameFlags flags = (FrameFlags)0;
		if (b > 3)
		{
			flags = (FrameFlags)array[3];
		}
		FrameBuffer receivedBytes = ((byteArraySlice == null) ? new FrameBuffer(array, b, num2 - b, flags, null) : new FrameBuffer(byteArraySlice.Buffer, byteArraySlice.Offset + b, num2 - b, flags, byteArraySlice));
		voiceClient.onFrame(channelId, playerId, voiceId, evNumber, ref receivedBytes, playerId == localPlayerId);
		receivedBytes.Release();
	}
}
