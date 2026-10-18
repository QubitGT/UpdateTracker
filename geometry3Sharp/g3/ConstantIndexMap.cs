namespace g3;

public class ConstantIndexMap : IIndexMap
{
	public int Constant;

	public int this[int index] => Constant;

	public ConstantIndexMap(int c)
	{
		Constant = c;
	}
}
