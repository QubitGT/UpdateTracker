namespace Oculus.Interaction.Input.Compatibility.OVR;

public static class HandFingerUtils
{
	public static HandFingerFlags ToFlags(HandFinger handFinger)
	{
		return (HandFingerFlags)(1 << (int)handFinger);
	}
}
