using Photon.Pun;

internal abstract class GorillaSerializerMasterOnly : GorillaWrappedSerializer
{
	protected override bool ValidOnSerialize(PhotonStream stream, in PhotonMessageInfo info)
	{
		if (info.Sender != PhotonNetwork.MasterClient)
		{
			return false;
		}
		return true;
	}
}
