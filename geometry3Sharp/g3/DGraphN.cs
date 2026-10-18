namespace g3;

public class DGraphN : DGraph
{
	public int AppendVertex()
	{
		return append_vertex_internal();
	}

	protected override int append_new_split_vertex(int a, int b)
	{
		return AppendVertex();
	}
}
