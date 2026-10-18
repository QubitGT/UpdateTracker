namespace Photon.Realtime;

public class FindFriendsOptions
{
	public bool CreatedOnGs;

	public bool Visible = true;

	public bool Open = true;

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
