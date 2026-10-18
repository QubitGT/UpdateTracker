namespace g3;

public interface Query2
{
	int ToLine(int i, int v0, int v1);

	int ToLine(ref Vector2d test, int v0, int v1);

	int ToTriangle(int i, int v0, int v1, int v2);

	int ToTriangle(ref Vector2d test, int v0, int v1, int v2);

	int ToCircumcircle(int i, int v0, int v1, int v2);

	int ToCircumcircle(ref Vector2d test, int v0, int v1, int v2);
}
