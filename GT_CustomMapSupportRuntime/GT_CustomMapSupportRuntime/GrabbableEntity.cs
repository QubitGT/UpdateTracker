using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class GrabbableEntity : MapEntity
{
	public AudioSource? audioSource;

	public AudioClip? catchSound;

	public float catchSoundVolume;

	public AudioClip? throwSound;

	public float throwSoundVolume;

	public override long GetPackedCreateData()
	{
		return (long)entityTypeId + (long)(lua_EntityID << 8);
	}

	public static void UnpackCreateData(long data, out byte entityTypeID, out short luaAgentID)
	{
		entityTypeID = (byte)(data & 0xFF);
		luaAgentID = (short)((data >> 8) & 0xFFFF);
	}
}
