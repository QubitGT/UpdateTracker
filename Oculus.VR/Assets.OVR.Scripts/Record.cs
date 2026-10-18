namespace Assets.OVR.Scripts;

internal class Record
{
	public int sortOrder;

	public string category;

	public string message;

	public Record(int order, string cat, string msg)
	{
		sortOrder = order;
		category = cat;
		message = msg;
	}
}
