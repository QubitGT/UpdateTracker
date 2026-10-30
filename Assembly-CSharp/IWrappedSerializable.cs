using Photon.Pun;

internal interface IWrappedSerializable
{
	void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info);

	void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info);
}
