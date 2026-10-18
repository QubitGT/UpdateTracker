namespace Oculus.Interaction.Input;

public static class HandFingerUtils
{
	public static HandFingerFlags ToFlags(HandFinger handFinger)
	{
		return (HandFingerFlags)(1 << (int)handFinger);
	}
}
