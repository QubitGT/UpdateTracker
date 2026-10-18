using System;

namespace Photon.Pun.UtilityScripts;

[Serializable]
public class PhotonTeam
{
	public string Name;

	public byte Code;

	public override string ToString()
	{
		return $"{Name} [{Code}]";
	}
}
