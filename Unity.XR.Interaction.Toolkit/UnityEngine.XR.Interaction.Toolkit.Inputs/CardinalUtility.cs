namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

public static class CardinalUtility
{
	public static Cardinal GetNearestCardinal(Vector2 value)
	{
		float num = Mathf.Atan2(value.y, value.x) * 57.29578f;
		float num2 = Mathf.Abs(num);
		if (num2 < 45f)
		{
			return Cardinal.East;
		}
		if (num2 > 135f)
		{
			return Cardinal.West;
		}
		if (!(num >= 0f))
		{
			return Cardinal.South;
		}
		return Cardinal.North;
	}
}
