using System;
using Fusion.Photon.Realtime;
using Fusion.Protocol;

namespace Fusion;

internal class CloudCommunicator : CommunicatorBase, IDisposable
{
	private readonly byte[] _buffer = new byte[65536];

	public FusionRelayClient Client { get; private set; }

	public override int CommunicatorID => (Client != null) ? Client.LocalPlayer.ActorNumber : (-1);

	public bool WasExtracted { get; set; }

	public CloudCommunicator(FusionAppSettings clientConfig)
	{
		Client = new FusionRelayClient(clientConfig);
		Client.OnEventCallback += PushPackage;
	}

	public override void Service()
	{
		if (Client != null)
		{
			Client.Update();
			base.Service();
		}
	}

	public unsafe override bool SendPackage(byte code, int targetActor, bool reliable, byte* buffer, int bufferLength)
	{
		return Client != null && Client.SendEvent(targetActor, code, buffer, bufferLength, reliable);
	}

	protected override void ConvertData(object data, out byte[] dataBuffer, out int maxLength)
	{
		dataBuffer = null;
		maxLength = _buffer.Length;
		Client.ExtractData(data, _buffer, ref maxLength);
		if (maxLength > 0)
		{
			dataBuffer = _buffer;
		}
	}

	public void Reset()
	{
		Client.Reset();
		MessageSendQueue.Clear();
		RecvQueue.Clear();
		Callbacks.Clear();
	}

	public void Dispose()
	{
		if (!WasExtracted)
		{
			Client = null;
		}
	}
}
