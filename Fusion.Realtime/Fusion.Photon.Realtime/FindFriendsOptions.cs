namespace Fusion.Photon.Realtime;

internal class FindFriendsOptions
{
	public bool CreatedOnGs = false;

	public bool Visible = false;

	public bool Open = false;

	internal int ToIntFlags()
	{
		int num = 0;
		if (CreatedOnGs)
		{
			num |= 1;
		}
		if (Visible)
		{
			num |= 2;
		}
		if (Open)
		{
			num |= 4;
		}
		return num;
	}
}
