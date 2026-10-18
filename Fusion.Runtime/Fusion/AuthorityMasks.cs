namespace Fusion;

public static class AuthorityMasks
{
	public const int STATE = 1;

	public const int INPUT = 2;

	public const int PROXY = 4;

	public const int NONE = 0;

	public const int ALL = 7;

	internal static int Create(bool state, bool input)
	{
		if (state)
		{
			return 1 | (input ? 2 : 0);
		}
		return input ? 2 : 4;
	}
}
