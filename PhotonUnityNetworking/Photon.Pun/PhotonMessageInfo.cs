using System;
using Photon.Realtime;

namespace Photon.Pun;

public struct PhotonMessageInfo
{
	private readonly int timeInt;

	public readonly Player Sender;

	public readonly PhotonView photonView;

	[Obsolete("Use SentServerTime instead.")]
	public double timestamp => (double)(uint)timeInt / 1000.0;

	public double SentServerTime => (double)(uint)timeInt / 1000.0;

	public int SentServerTimestamp => timeInt;

	public PhotonMessageInfo(Player player, int timestamp, PhotonView view)
	{
		Sender = player;
		timeInt = timestamp;
		photonView = view;
	}

	public override string ToString()
	{
		return string.Format("[PhotonMessageInfo: Sender='{1}' Senttime={0}]", SentServerTime, Sender);
	}
}
