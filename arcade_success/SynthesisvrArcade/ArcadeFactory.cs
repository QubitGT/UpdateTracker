namespace SynthesisvrArcade;

public static class ArcadeFactory
{
	public static ArcadeInterface Synthesis()
	{
		return new PublicArcade();
	}
}
