namespace g3;

public abstract class DynamicPriorityQueueNode
{
	public float priority { get; protected internal set; }

	internal int index { get; set; }
}
