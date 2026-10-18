using System.Collections.Generic;

namespace ExitGames.Client.Photon;

internal class NCommandPool
{
	private readonly Stack<NCommand> pool = new Stack<NCommand>();

	public NCommand Acquire(EnetPeer peer, byte[] inBuff, ref int readingOffset)
	{
		NCommand nCommand;
		lock (pool)
		{
			if (pool.Count == 0)
			{
				nCommand = new NCommand(peer, inBuff, ref readingOffset);
				nCommand.returnPool = this;
			}
			else
			{
				nCommand = pool.Pop();
				nCommand.Initialize(peer, inBuff, ref readingOffset);
			}
		}
		return nCommand;
	}

	public NCommand Acquire(EnetPeer peer, byte commandType, StreamBuffer payload, byte channel)
	{
		NCommand nCommand;
		lock (pool)
		{
			if (pool.Count == 0)
			{
				nCommand = new NCommand(peer, commandType, payload, channel);
				nCommand.returnPool = this;
			}
			else
			{
				nCommand = pool.Pop();
				nCommand.Initialize(peer, commandType, payload, channel);
			}
		}
		return nCommand;
	}

	public void Release(NCommand nCommand)
	{
		nCommand.Reset();
		lock (pool)
		{
			pool.Push(nCommand);
		}
	}
}
