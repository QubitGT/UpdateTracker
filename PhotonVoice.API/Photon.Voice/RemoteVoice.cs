using System;
using System.Collections.Generic;
using System.Threading;

namespace Photon.Voice;

internal class RemoteVoice : IDisposable
{
	internal RemoteVoiceOptions options;

	internal int channelId;

	private int playerId;

	private byte voiceId;

	private volatile bool disposed;

	private object disposeLock = new object();

	private SpacingProfile receiveSpacingProfile = new SpacingProfile(1000);

	internal byte lastEvNumber;

	private VoiceClient voiceClient;

	private Queue<FrameBuffer> frameQueue = new Queue<FrameBuffer>();

	private AutoResetEvent frameQueueReady = new AutoResetEvent(initialState: false);

	private int flushingFramePosInQueue = -1;

	private FrameBuffer nullFrame;

	internal VoiceInfo Info { get; private set; }

	internal int DelayFrames { get; set; }

	private string shortName => "v#" + voiceId + "ch#" + voiceClient.channelStr(channelId) + "p#" + playerId;

	public string LogPrefix { get; private set; }

	public string ReceiveSpacingProfileDump => receiveSpacingProfile.Dump;

	public int ReceiveSpacingProfileMax => receiveSpacingProfile.Max;

	internal RemoteVoice(VoiceClient client, RemoteVoiceOptions options, int channelId, int playerId, byte voiceId, VoiceInfo info, byte lastEventNumber)
	{
		this.options = options;
		LogPrefix = options.logPrefix;
		voiceClient = client;
		this.channelId = channelId;
		this.playerId = playerId;
		this.voiceId = voiceId;
		Info = info;
		lastEvNumber = lastEventNumber;
		if (this.options.Decoder == null)
		{
			string fmt = LogPrefix + ": decoder is null (set it with options Decoder property or SetOutput method in OnRemoteVoiceInfoAction)";
			voiceClient.logger.LogError(fmt);
			disposed = true;
			return;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			decodeThread();
		});
		Util.SetThreadName(thread, "[PV] Dec" + shortName);
		thread.Start();
	}

	public void ReceiveSpacingProfileStart()
	{
		receiveSpacingProfile.Start();
	}

	private static byte byteDiff(byte latest, byte last)
	{
		return (byte)(latest - (last + 1));
	}

	internal void receiveBytes(ref FrameBuffer receivedBytes, byte evNumber)
	{
		if (evNumber != lastEvNumber)
		{
			int num = byteDiff(evNumber, lastEvNumber);
			if (num == 0)
			{
				lastEvNumber = evNumber;
			}
			else if (num < 127)
			{
				voiceClient.logger.LogWarning(LogPrefix + " evNumer: " + evNumber + " playerVoice.lastEvNumber: " + lastEvNumber + " missing: " + num + " r/b " + receivedBytes.Length);
				voiceClient.FramesLost += num;
				lastEvNumber = evNumber;
				receiveNullFrames(num);
			}
			else
			{
				voiceClient.logger.LogWarning(LogPrefix + " evNumer: " + evNumber + " playerVoice.lastEvNumber: " + lastEvNumber + " late: " + (255 - num) + " r/b " + receivedBytes.Length);
			}
		}
		receiveFrame(ref receivedBytes);
	}

	private void receiveFrame(ref FrameBuffer frame)
	{
		lock (disposeLock)
		{
			if (disposed)
			{
				return;
			}
			receiveSpacingProfile.Update(lost: false, (frame.Flags & FrameFlags.EndOfStream) != 0);
			lock (frameQueue)
			{
				frameQueue.Enqueue(frame);
				frame.Retain();
				if ((frame.Flags & FrameFlags.EndOfStream) != 0)
				{
					flushingFramePosInQueue = frameQueue.Count - 1;
				}
			}
			frameQueueReady.Set();
		}
	}

	private void receiveNullFrames(int count)
	{
		lock (disposeLock)
		{
			if (disposed)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				receiveSpacingProfile.Update(lost: true, flush: false);
				lock (frameQueue)
				{
					frameQueue.Enqueue(nullFrame);
				}
			}
			frameQueueReady.Set();
		}
	}

	private void decodeThread()
	{
		voiceClient.logger.LogInfo(LogPrefix + ": Starting decode thread");
		IDecoder decoder = options.Decoder;
		try
		{
			decoder.Open(Info);
			while (!disposed)
			{
				frameQueueReady.WaitOne();
				while (!disposed)
				{
					bool flag = false;
					FrameBuffer buf;
					lock (frameQueue)
					{
						int num = 0;
						if (flushingFramePosInQueue < 0 && DelayFrames > 0 && DelayFrames < 300)
						{
							num = DelayFrames;
						}
						if (frameQueue.Count > num)
						{
							buf = frameQueue.Dequeue();
							flushingFramePosInQueue--;
							if (flushingFramePosInQueue == int.MinValue)
							{
								flushingFramePosInQueue = -1;
							}
							flag = true;
							goto IL_00e9;
						}
					}
					break;
					IL_00e9:
					if (flag)
					{
						decoder.Input(ref buf);
						buf.Release();
					}
				}
			}
		}
		catch (Exception ex)
		{
			voiceClient.logger.LogError(LogPrefix + ": Exception in decode thread: " + ex);
			throw ex;
		}
		finally
		{
			lock (disposeLock)
			{
				disposed = true;
			}
			frameQueueReady.Close();
			lock (frameQueue)
			{
				while (frameQueue.Count > 0)
				{
					frameQueue.Dequeue().Release();
				}
			}
			decoder.Dispose();
			voiceClient.logger.LogInfo(LogPrefix + ": Exiting decode thread");
		}
	}

	internal void removeAndDispose()
	{
		if (options.OnRemoteVoiceRemoveAction != null)
		{
			options.OnRemoteVoiceRemoveAction();
		}
		Dispose();
	}

	public void Dispose()
	{
		lock (disposeLock)
		{
			if (!disposed)
			{
				disposed = true;
				frameQueueReady.Set();
			}
		}
	}
}
