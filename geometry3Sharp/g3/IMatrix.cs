namespace g3;

public interface IMatrix
{
	int Rows { get; }

	int Columns { get; }

	Index2i Size { get; }

	double this[int r, int c] { get; set; }

	void Set(int r, int c, double value);
}
