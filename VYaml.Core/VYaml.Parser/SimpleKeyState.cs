namespace VYaml.Parser;

internal struct SimpleKeyState
{
	public bool Possible;

	public bool Required;

	public int TokenNumber;

	public Marker Start;
}
