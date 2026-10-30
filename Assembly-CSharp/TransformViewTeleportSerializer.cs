using Photon.Pun;

public class TransformViewTeleportSerializer : NetworkComponent
{
	private bool willTeleport;

	private GorillaNetworkTransform transformView;

	protected override void Start()
	{
		base.Start();
		transformView = GetComponent<GorillaNetworkTransform>();
	}

	public void SetWillTeleport()
	{
		willTeleport = true;
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!transformView.RespectOwnership || info.Sender == info.photonView.Owner)
		{
			stream.SendNext(willTeleport);
			willTeleport = false;
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if ((!transformView.RespectOwnership || info.Sender == info.photonView.Owner) && (bool)stream.ReceiveNext())
		{
			transformView.GTAddition_DoTeleport();
		}
	}
}
