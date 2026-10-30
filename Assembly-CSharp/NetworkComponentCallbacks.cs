using System;
using Photon.Pun;

public class NetworkComponentCallbacks : NetworkComponent
{
	public Action ReadData;

	public Action WriteData;

	public Action<PhotonStream, PhotonMessageInfo> ReadPunData;

	public Action<PhotonStream, PhotonMessageInfo> WritePunData;

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		ReadPunData(stream, info);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		WritePunData(stream, info);
	}
}
