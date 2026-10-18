namespace Technie.PhysicsCreator.QHull;

public class FaceList
{
	private Face head;

	private Face tail;

	public void clear()
	{
		head = (tail = null);
	}

	public void add(Face vtx)
	{
		if (head == null)
		{
			head = vtx;
		}
		else
		{
			tail.next = vtx;
		}
		vtx.next = null;
		tail = vtx;
	}

	public Face first()
	{
		return head;
	}

	public bool isEmpty()
	{
		return head == null;
	}
}
