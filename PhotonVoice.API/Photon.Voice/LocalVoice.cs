using System;
using System.Collections.Generic;

namespace Photon.Voice;

public class LocalVoice : IDisposable
{
	public const int DATA_POOL_CAPACITY = 50;

	private bool transmitEnabled = true;

	private bool debugEchoMode;

	protected VoiceInfo info;

	protected IEncoder encoder;

	internal byte id;

	internal int channelId;

	internal byte evNumber;

	protected VoiceClient voiceClient;

	protected ArraySegment<byte> configFrame;

	protected volatile bool disposed;

	protected object disposeLock = new object();

	private const int NO_TRANSMIT_TIMEOUT_MS = 100;

	private int lastTransmitTime = Environment.TickCount - 100;

	internal Dictionary<byte, int> eventTimestamps = new Dictionary<byte, int>();

	private SpacingProfile sendSpacingProfile = new SpacingProfile(1000);

	[Obsolete("Use InterestGroup.")]
	public byte Group
	{
		get
		{
			return InterestGroup;
		}
		set
		{
			InterestGroup = value;
		}
	}

	public byte InterestGroup { get; set; }

	public VoiceInfo Info => info;

	public bool TransmitEnabled
	{
		get
		{
			return transmitEnabled;
		}
		set
		{
			if (transmitEnabled != value)
			{
				if (transmitEnabled && encoder != null && voiceClient.transport.IsChannelJoined(channelId))
				{
					encoder.EndOfStream();
				}
				transmitEnabled = value;
			}
		}
	}

	public bool IsCurrentlyTransmitting => Environment.TickCount - lastTransmitTime < 100;

	public int FramesSent { get; private set; }

	public int FramesSentBytes { get; private set; }

	public bool Reliable { get; set; }

	public bool Encrypt { get; set; }

	public IServiceable LocalUserServiceable { get; set; }

	public bool DebugEchoMode
	{
		get
		{
			return debugEchoMode;
		}
		set
		{
			if (debugEchoMode == value)
			{
				return;
			}
			debugEchoMode = value;
			if (voiceClient != null && voiceClient.transport != null && voiceClient.transport.IsChannelJoined(channelId))
			{
				if (debugEchoMode)
				{
					voiceClient.sendVoicesInfoAndConfigFrame(new List<LocalVoice> { this }, channelId, -1);
				}
				else
				{
					voiceClient.transport.SendVoiceRemove(this, channelId, -1);
				}
			}
		}
	}

	public string SendSpacingProfileDump => sendSpacingProfile.Dump;

	public int SendSpacingProfileMax => sendSpacingProfile.Max;

	public byte ID => id;

	public byte EvNumber => evNumber;

	protected string shortName => "v#" + id + "ch#" + voiceClient.channelStr(channelId);

	public string Name => "Local " + info.Codec.ToString() + " v#" + id + " ch#" + voiceClient.channelStr(channelId);

	public string LogPrefix => "[PV] " + Name;

	public void SendSpacingProfileStart()
	{
		sendSpacingProfile.Start();
	}

	internal LocalVoice()
	{
	}

	internal LocalVoice(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, int channelId)
	{
		info = voiceInfo;
		this.channelId = channelId;
		this.voiceClient = voiceClient;
		this.id = id;
		if (encoder == null)
		{
			string fmt = LogPrefix + ": encoder is null";
			voiceClient.logger.LogError(fmt);
			throw new ArgumentNullException("encoder");
		}
		this.encoder = encoder;
		this.encoder.Output = sendFrame;
	}

	internal virtual void service()
	{
		while (true)
		{
			FrameFlags flags;
			ArraySegment<byte> compressed = encoder.DequeueOutput(out flags);
			if (compressed.Count == 0)
			{
				break;
			}
			sendFrame(compressed, flags);
		}
		if (LocalUserServiceable != null)
		{
			LocalUserServiceable.Service(this);
		}
	}

	internal void sendConfigFrame(int targetPlayerId)
	{
		if (configFrame.Count != 0)
		{
			voiceClient.logger.LogInfo(LogPrefix + " Sending config frame to pl " + targetPlayerId);
			sendFrame0(configFrame, FrameFlags.Config, targetPlayerId, reliable: true);
		}
	}

	internal void sendFrame(ArraySegment<byte> compressed, FrameFlags flags)
	{
		if ((flags & FrameFlags.Config) != 0)
		{
			byte[] array = ((configFrame.Array != null && configFrame.Array.Length >= compressed.Count) ? configFrame.Array : new byte[compressed.Count]);
			Buffer.BlockCopy(compressed.Array, compressed.Offset, array, 0, compressed.Count);
			configFrame = new ArraySegment<byte>(array, 0, compressed.Count);
			voiceClient.logger.LogInfo(LogPrefix + " Got config frame " + configFrame.Count + " bytes");
		}
		if (voiceClient.transport.IsChannelJoined(channelId) && TransmitEnabled)
		{
			sendFrame0(compressed, flags, 0, Reliable);
		}
	}

	internal void sendFrame0(ArraySegment<byte> compressed, FrameFlags flags, int targetPlayerId, bool reliable)
	{
		if ((flags & FrameFlags.Config) != 0)
		{
			reliable = true;
		}
		if ((flags & FrameFlags.KeyFrame) != 0)
		{
			reliable = true;
		}
		_ = flags & FrameFlags.EndOfStream;
		FramesSent++;
		FramesSentBytes += compressed.Count;
		voiceClient.transport.SendFrame(compressed, flags, evNumber, id, channelId, targetPlayerId, reliable, this);
		sendSpacingProfile.Update(lost: false, flush: false);
		if (DebugEchoMode)
		{
			eventTimestamps[evNumber] = Environment.TickCount;
		}
		evNumber++;
		if (compressed.Count > 0 && (flags & FrameFlags.Config) == 0)
		{
			lastTransmitTime = Environment.TickCount;
		}
	}

	public void RemoveSelf()
	{
		if (voiceClient != null)
		{
			voiceClient.RemoveLocalVoice(this);
		}
	}

	public virtual void Dispose()
	{
		if (!disposed)
		{
			if (encoder != null)
			{
				encoder.Dispose();
			}
			disposed = true;
		}
	}
}
