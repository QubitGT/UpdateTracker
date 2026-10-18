using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Voice;

public class VoiceClient : IDisposable
{
	public delegate void RemoteVoiceInfoDelegate(int channelId, int playerId, byte voiceId, VoiceInfo voiceInfo, ref RemoteVoiceOptions options);

	public struct CreateOptions
	{
		public byte VoiceIDMin;

		public byte VoiceIDMax;

		public static CreateOptions Default = new CreateOptions
		{
			VoiceIDMin = 1,
			VoiceIDMax = 15
		};
	}

	internal IVoiceTransport transport;

	internal ILogger logger;

	private int prevRtt;

	private Dictionary<Codec, int> remoteVoiceDelayFrames = new Dictionary<Codec, int>();

	private byte voiceIDMin;

	private byte voiceIDMax;

	private byte voiceIdLast;

	private byte globalInterestGroup;

	private Dictionary<byte, LocalVoice> localVoices = new Dictionary<byte, LocalVoice>();

	private Dictionary<int, List<LocalVoice>> localVoicesPerChannel = new Dictionary<int, List<LocalVoice>>();

	private Dictionary<int, Dictionary<byte, RemoteVoice>> remoteVoices = new Dictionary<int, Dictionary<byte, RemoteVoice>>();

	private Random rnd = new Random();

	public int FramesLost { get; internal set; }

	public int FramesReceived { get; private set; }

	public int FramesSent
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
			{
				num += localVoice.Value.FramesSent;
			}
			return num;
		}
	}

	public int FramesSentBytes
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
			{
				num += localVoice.Value.FramesSentBytes;
			}
			return num;
		}
	}

	public int RoundTripTime { get; private set; }

	public int RoundTripTimeVariance { get; private set; }

	public bool SuppressInfoDuplicateWarning { get; set; }

	public RemoteVoiceInfoDelegate OnRemoteVoiceInfoAction { get; set; }

	public int DebugLostPercent { get; set; }

	public IEnumerable<LocalVoice> LocalVoices
	{
		get
		{
			LocalVoice[] array = new LocalVoice[localVoices.Count];
			localVoices.Values.CopyTo(array, 0);
			return array;
		}
	}

	public IEnumerable<RemoteVoiceInfo> RemoteVoiceInfos
	{
		get
		{
			foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> playerVoices in remoteVoices)
			{
				foreach (KeyValuePair<byte, RemoteVoice> item in playerVoices.Value)
				{
					yield return new RemoteVoiceInfo(item.Value.channelId, playerVoices.Key, item.Key, item.Value.Info);
				}
			}
		}
	}

	internal byte GlobalInterestGroup
	{
		get
		{
			return globalInterestGroup;
		}
		set
		{
			globalInterestGroup = value;
			foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
			{
				localVoice.Value.InterestGroup = globalInterestGroup;
			}
		}
	}

	public IEnumerable<LocalVoice> LocalVoicesInChannel(int channelId)
	{
		if (localVoicesPerChannel.TryGetValue(channelId, out var value))
		{
			LocalVoice[] array = new LocalVoice[value.Count];
			value.CopyTo(array, 0);
			return array;
		}
		return new LocalVoice[0];
	}

	public void LogSpacingProfiles()
	{
		foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
		{
			localVoice.Value.SendSpacingProfileStart();
			logger.LogInfo(localVoice.Value.LogPrefix + " ev. prof.: " + localVoice.Value.SendSpacingProfileDump);
		}
		foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> remoteVoice in remoteVoices)
		{
			foreach (KeyValuePair<byte, RemoteVoice> item in remoteVoice.Value)
			{
				item.Value.ReceiveSpacingProfileStart();
				logger.LogInfo(item.Value.LogPrefix + " ev. prof.: " + item.Value.ReceiveSpacingProfileDump);
			}
		}
	}

	public void LogStats()
	{
		int statDisposerCreated = FrameBuffer.statDisposerCreated;
		int statDisposerDisposed = FrameBuffer.statDisposerDisposed;
		int statPinned = FrameBuffer.statPinned;
		int statUnpinned = FrameBuffer.statUnpinned;
		logger.LogInfo("[PV] FrameBuffer stats Disposer: " + statDisposerCreated + " - " + statDisposerDisposed + " = " + (statDisposerCreated - statDisposerDisposed));
		logger.LogInfo("[PV] FrameBuffer stats Pinned: " + statPinned + " - " + statUnpinned + " = " + (statPinned - statUnpinned));
	}

	public void SetRemoteVoiceDelayFrames(Codec codec, int delayFrames)
	{
		remoteVoiceDelayFrames[codec] = delayFrames;
		foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> remoteVoice in remoteVoices)
		{
			foreach (KeyValuePair<byte, RemoteVoice> item in remoteVoice.Value)
			{
				if (codec == item.Value.Info.Codec)
				{
					item.Value.DelayFrames = delayFrames;
				}
			}
		}
	}

	public VoiceClient(IVoiceTransport transport, ILogger logger, CreateOptions opt = default(CreateOptions))
	{
		this.transport = transport;
		this.logger = logger;
		if (opt.Equals(default(CreateOptions)))
		{
			opt = CreateOptions.Default;
		}
		voiceIDMin = opt.VoiceIDMin;
		voiceIDMax = opt.VoiceIDMax;
		voiceIdLast = voiceIDMax;
	}

	public void Service()
	{
		foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
		{
			localVoice.Value.service();
		}
	}

	private LocalVoice createLocalVoice(int channelId, Func<byte, int, LocalVoice> voiceFactory)
	{
		byte newVoiceId = getNewVoiceId();
		if (newVoiceId != 0)
		{
			LocalVoice localVoice = voiceFactory(newVoiceId, channelId);
			if (localVoice != null)
			{
				addVoice(newVoiceId, channelId, localVoice);
				logger.LogInfo(localVoice.LogPrefix + " added enc: " + localVoice.Info.ToString());
				return localVoice;
			}
		}
		return null;
	}

	public LocalVoice CreateLocalVoice(VoiceInfo voiceInfo, int channelId = 0, IEncoder encoder = null)
	{
		return createLocalVoice(channelId, (byte vId, int chId) => new LocalVoice(this, encoder, vId, voiceInfo, chId));
	}

	public LocalVoiceFramed<T> CreateLocalVoiceFramed<T>(VoiceInfo voiceInfo, int frameSize, int channelId = 0, IEncoder encoder = null)
	{
		return (LocalVoiceFramed<T>)createLocalVoice(channelId, (byte vId, int chId) => new LocalVoiceFramed<T>(this, encoder, vId, voiceInfo, chId, frameSize));
	}

	public LocalVoiceAudio<T> CreateLocalVoiceAudio<T>(VoiceInfo voiceInfo, IAudioDesc audioSourceDesc, IEncoder encoder, int channelId)
	{
		return (LocalVoiceAudio<T>)createLocalVoice(channelId, (byte vId, int chId) => LocalVoiceAudio<T>.Create(this, vId, encoder, voiceInfo, audioSourceDesc, chId));
	}

	public LocalVoice CreateLocalVoiceAudioFromSource(VoiceInfo voiceInfo, IAudioDesc source, AudioSampleType sampleType, IEncoder encoder = null, int channelId = 0)
	{
		if (sampleType == AudioSampleType.Source)
		{
			if (source is IAudioPusher<float> || source is IAudioReader<float>)
			{
				sampleType = AudioSampleType.Float;
			}
			else if (source is IAudioPusher<short> || source is IAudioReader<short>)
			{
				sampleType = AudioSampleType.Short;
			}
		}
		if (encoder == null)
		{
			switch (sampleType)
			{
			case AudioSampleType.Float:
				encoder = Platform.CreateDefaultAudioEncoder<float>(logger, voiceInfo);
				break;
			case AudioSampleType.Short:
				encoder = Platform.CreateDefaultAudioEncoder<short>(logger, voiceInfo);
				break;
			}
		}
		if (source is IAudioPusher<float>)
		{
			if (sampleType == AudioSampleType.Short)
			{
				logger.LogInfo("[PV] Creating local voice with source samples type conversion from IAudioPusher float to short.");
				LocalVoiceAudio<short> localVoice4 = CreateLocalVoiceAudio<short>(voiceInfo, source, encoder, channelId);
				FactoryReusableArray<float> bufferFactory = new FactoryReusableArray<float>(0);
				((IAudioPusher<float>)source).SetCallback(delegate(float[] buf)
				{
					short[] array2 = localVoice4.BufferFactory.New(buf.Length);
					AudioUtil.Convert(buf, array2, buf.Length);
					localVoice4.PushDataAsync(array2);
				}, bufferFactory);
				return localVoice4;
			}
			LocalVoiceAudio<float> localVoice3 = CreateLocalVoiceAudio<float>(voiceInfo, source, encoder, channelId);
			((IAudioPusher<float>)source).SetCallback(delegate(float[] buf)
			{
				localVoice3.PushDataAsync(buf);
			}, localVoice3.BufferFactory);
			return localVoice3;
		}
		if (source is IAudioPusher<short>)
		{
			if (sampleType == AudioSampleType.Float)
			{
				logger.LogInfo("[PV] Creating local voice with source samples type conversion from IAudioPusher short to float.");
				LocalVoiceAudio<float> localVoice2 = CreateLocalVoiceAudio<float>(voiceInfo, source, encoder, channelId);
				FactoryReusableArray<short> bufferFactory2 = new FactoryReusableArray<short>(0);
				((IAudioPusher<short>)source).SetCallback(delegate(short[] buf)
				{
					float[] array = localVoice2.BufferFactory.New(buf.Length);
					AudioUtil.Convert(buf, array, buf.Length);
					localVoice2.PushDataAsync(array);
				}, bufferFactory2);
				return localVoice2;
			}
			LocalVoiceAudio<short> localVoice = CreateLocalVoiceAudio<short>(voiceInfo, source, encoder, channelId);
			((IAudioPusher<short>)source).SetCallback(delegate(short[] buf)
			{
				localVoice.PushDataAsync(buf);
			}, localVoice.BufferFactory);
			return localVoice;
		}
		if (source is IAudioReader<float>)
		{
			if (sampleType == AudioSampleType.Short)
			{
				logger.LogInfo("[PV] Creating local voice with source samples type conversion from IAudioReader float to short.");
				LocalVoiceAudio<short> localVoiceAudio = CreateLocalVoiceAudio<short>(voiceInfo, source, encoder, channelId);
				localVoiceAudio.LocalUserServiceable = new BufferReaderPushAdapterAsyncPoolFloatToShort(localVoiceAudio, source as IAudioReader<float>);
				return localVoiceAudio;
			}
			LocalVoiceAudio<float> localVoiceAudio2 = CreateLocalVoiceAudio<float>(voiceInfo, source, encoder, channelId);
			localVoiceAudio2.LocalUserServiceable = new BufferReaderPushAdapterAsyncPool<float>(localVoiceAudio2, source as IAudioReader<float>);
			return localVoiceAudio2;
		}
		if (source is IAudioReader<short>)
		{
			if (sampleType == AudioSampleType.Float)
			{
				logger.LogInfo("[PV] Creating local voice with source samples type conversion from IAudioReader short to float.");
				LocalVoiceAudio<float> localVoiceAudio3 = CreateLocalVoiceAudio<float>(voiceInfo, source, encoder, channelId);
				localVoiceAudio3.LocalUserServiceable = new BufferReaderPushAdapterAsyncPoolShortToFloat(localVoiceAudio3, source as IAudioReader<short>);
				return localVoiceAudio3;
			}
			LocalVoiceAudio<short> localVoiceAudio4 = CreateLocalVoiceAudio<short>(voiceInfo, source, encoder, channelId);
			localVoiceAudio4.LocalUserServiceable = new BufferReaderPushAdapterAsyncPool<short>(localVoiceAudio4, source as IAudioReader<short>);
			return localVoiceAudio4;
		}
		logger.LogError("[PV] CreateLocalVoiceAudioFromSource does not support Voice.IAudioDesc of type {0}", source.GetType());
		return LocalVoiceAudioDummy.Dummy;
	}

	private byte idInc(byte id)
	{
		if (id != voiceIDMax)
		{
			return (byte)(id + 1);
		}
		return voiceIDMin;
	}

	private byte getNewVoiceId()
	{
		bool[] array = new bool[256];
		foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
		{
			array[localVoice.Value.id] = true;
		}
		for (byte b = idInc(voiceIdLast); b != voiceIdLast; b = idInc(b))
		{
			if (!array[b])
			{
				voiceIdLast = b;
				return b;
			}
		}
		return 0;
	}

	private void addVoice(byte newId, int channelId, LocalVoice v)
	{
		localVoices[newId] = v;
		if (!localVoicesPerChannel.TryGetValue(channelId, out var value))
		{
			value = new List<LocalVoice>();
			localVoicesPerChannel[channelId] = value;
		}
		value.Add(v);
		if (transport.IsChannelJoined(channelId))
		{
			sendVoicesInfoAndConfigFrame(new List<LocalVoice> { v }, channelId, 0);
		}
		v.InterestGroup = GlobalInterestGroup;
	}

	public void RemoveLocalVoice(LocalVoice voice)
	{
		localVoices.Remove(voice.id);
		localVoicesPerChannel[voice.channelId].Remove(voice);
		if (transport.IsChannelJoined(voice.channelId))
		{
			transport.SendVoiceRemove(voice, voice.channelId, 0);
		}
		voice.Dispose();
		logger.LogInfo(voice.LogPrefix + " removed");
	}

	private void sendChannelVoicesInfo(int channelId, int targetPlayerId)
	{
		if (transport.IsChannelJoined(channelId) && localVoicesPerChannel.TryGetValue(channelId, out var value))
		{
			sendVoicesInfoAndConfigFrame(value, channelId, targetPlayerId);
		}
	}

	internal void sendVoicesInfoAndConfigFrame(IEnumerable<LocalVoice> voiceList, int channelId, int targetPlayerId)
	{
		transport.SendVoicesInfo(voiceList, channelId, targetPlayerId);
		foreach (LocalVoice voice in voiceList)
		{
			voice.sendConfigFrame(targetPlayerId);
		}
		if (targetPlayerId == 0)
		{
			IEnumerable<LocalVoice> enumerable = localVoices.Values.Where((LocalVoice x) => x.DebugEchoMode);
			if (enumerable.Count() > 0)
			{
				transport.SendVoicesInfo(enumerable, channelId, -1);
			}
		}
	}

	private void clearRemoteVoices()
	{
		foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> remoteVoice in remoteVoices)
		{
			foreach (KeyValuePair<byte, RemoteVoice> item in remoteVoice.Value)
			{
				item.Value.removeAndDispose();
			}
		}
		remoteVoices.Clear();
		logger.LogInfo("[PV] Remote voices cleared");
	}

	private void clearRemoteVoicesInChannel(int channelId)
	{
		foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> remoteVoice in remoteVoices)
		{
			List<byte> list = new List<byte>();
			foreach (KeyValuePair<byte, RemoteVoice> item in remoteVoice.Value)
			{
				if (item.Value.channelId == channelId)
				{
					item.Value.removeAndDispose();
					list.Add(item.Key);
				}
			}
			foreach (byte item2 in list)
			{
				remoteVoice.Value.Remove(item2);
			}
		}
		logger.LogInfo("[PV] Remote voices for channel " + channelStr(channelId) + " cleared");
	}

	private void clearRemoteVoicesInChannelForPlayer(int channelId, int playerId)
	{
		Dictionary<byte, RemoteVoice> value = null;
		if (!remoteVoices.TryGetValue(playerId, out value))
		{
			return;
		}
		List<byte> list = new List<byte>();
		foreach (KeyValuePair<byte, RemoteVoice> item in value)
		{
			if (item.Value.channelId == channelId)
			{
				item.Value.removeAndDispose();
				list.Add(item.Key);
			}
		}
		foreach (byte item2 in list)
		{
			value.Remove(item2);
		}
	}

	public void onJoinChannel(int channel)
	{
		sendChannelVoicesInfo(channel, 0);
	}

	public void onLeaveChannel(int channel)
	{
		clearRemoteVoicesInChannel(channel);
	}

	public void onLeaveAllChannels()
	{
		clearRemoteVoices();
	}

	public void onPlayerJoin(int channelId, int playerId)
	{
		sendChannelVoicesInfo(channelId, playerId);
	}

	public void onPlayerLeave(int channelId, int playerId)
	{
		clearRemoteVoicesInChannelForPlayer(channelId, playerId);
	}

	public void onVoiceInfo(int channelId, int playerId, byte voiceId, byte eventNumber, VoiceInfo info)
	{
		Dictionary<byte, RemoteVoice> value = null;
		if (!remoteVoices.TryGetValue(playerId, out value))
		{
			value = new Dictionary<byte, RemoteVoice>();
			remoteVoices[playerId] = value;
		}
		if (!value.ContainsKey(voiceId))
		{
			string text = " p#" + playerStr(playerId) + " v#" + voiceId + " ch#" + channelStr(channelId);
			logger.LogInfo("[PV] " + text + " Info received: " + info.ToString() + " ev=" + eventNumber);
			string logPrefix = "[PV] Remote " + info.Codec.ToString() + text;
			RemoteVoiceOptions options = new RemoteVoiceOptions(logger, logPrefix, info);
			if (OnRemoteVoiceInfoAction != null)
			{
				OnRemoteVoiceInfoAction(channelId, playerId, voiceId, info, ref options);
			}
			if (options.Decoder != null)
			{
				RemoteVoice remoteVoice2 = (value[voiceId] = new RemoteVoice(this, options, channelId, playerId, voiceId, info, eventNumber));
				if (remoteVoiceDelayFrames.TryGetValue(info.Codec, out var value2))
				{
					remoteVoice2.DelayFrames = value2;
				}
			}
		}
		else if (!SuppressInfoDuplicateWarning)
		{
			logger.LogWarning("[PV] Info duplicate for voice #" + voiceId + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId));
		}
	}

	public void onVoiceRemove(int channelId, int playerId, byte[] voiceIds)
	{
		Dictionary<byte, RemoteVoice> value = null;
		if (remoteVoices.TryGetValue(playerId, out value))
		{
			for (int i = 0; i < voiceIds.Length; i++)
			{
				byte key = voiceIds[i];
				if (value.TryGetValue(key, out var value2))
				{
					value.Remove(key);
					logger.LogInfo("[PV] Remote voice #" + key + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " removed");
					value2.removeAndDispose();
				}
				else
				{
					logger.LogWarning("[PV] Remote voice #" + key + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " not found when trying to remove");
				}
			}
		}
		else
		{
			logger.LogWarning("[PV] Remote voice list of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " not found when trying to remove voice(s)");
		}
	}

	public void onFrame(int channelId, int playerId, byte voiceId, byte evNumber, ref FrameBuffer receivedBytes, bool isLocalPlayer)
	{
		if (isLocalPlayer && localVoices.TryGetValue(voiceId, out var value) && value.eventTimestamps.TryGetValue(evNumber, out var value2))
		{
			int num = Environment.TickCount - value2;
			int num2 = num - prevRtt;
			prevRtt = num;
			if (num2 < 0)
			{
				num2 = -num2;
			}
			RoundTripTimeVariance = (num2 + RoundTripTimeVariance * 19) / 20;
			RoundTripTime = (num + RoundTripTime * 19) / 20;
		}
		if (DebugLostPercent > 0 && rnd.Next(100) < DebugLostPercent)
		{
			logger.LogWarning("[PV] Debug Lost Sim: 1 packet dropped");
			return;
		}
		FramesReceived++;
		Dictionary<byte, RemoteVoice> value3 = null;
		if (remoteVoices.TryGetValue(playerId, out value3))
		{
			RemoteVoice value4 = null;
			if (value3.TryGetValue(voiceId, out value4))
			{
				value4.receiveBytes(ref receivedBytes, evNumber);
				return;
			}
			logger.LogWarning("[PV] Frame event for not inited voice #" + voiceId + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId));
		}
		else
		{
			logger.LogWarning("[PV] Frame event for voice #" + voiceId + " of not inited player " + playerStr(playerId) + " at channel " + channelStr(channelId));
		}
	}

	internal string channelStr(int channelId)
	{
		string text = transport.ChannelIdStr(channelId);
		if (text != null)
		{
			return channelId + "(" + text + ")";
		}
		return channelId.ToString();
	}

	internal string playerStr(int playerId)
	{
		string text = transport.PlayerIdStr(playerId);
		if (text != null)
		{
			return playerId + "(" + text + ")";
		}
		return playerId.ToString();
	}

	public void Dispose()
	{
		foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
		{
			localVoice.Value.Dispose();
		}
		foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> remoteVoice in remoteVoices)
		{
			foreach (KeyValuePair<byte, RemoteVoice> item in remoteVoice.Value)
			{
				item.Value.Dispose();
			}
		}
	}
}
