namespace g3;

public class ShiftIndexMap : IIndexMap
{
	public int Shift;

	public int this[int index] => index + Shift;

	public ShiftIndexMap(int n)
	{
		Shift = n;
	}
}
