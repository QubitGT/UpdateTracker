namespace g3;

public class Radial3DArrowGenerator : VerticalGeneralizedCylinderGenerator
{
	public float StickRadius = 0.5f;

	public float StickLength = 1f;

	public float HeadBaseRadius = 1f;

	public float TipRadius;

	public float HeadLength = 0.5f;

	public override MeshGenerator Generate()
	{
		Sections = new CircularSection[4];
		Sections[0] = new CircularSection(StickRadius, 0f);
		Sections[1] = new CircularSection(StickRadius, StickLength);
		Sections[2] = new CircularSection(HeadBaseRadius, StickLength);
		Sections[3] = new CircularSection(TipRadius, StickLength + HeadLength);
		Capped = true;
		NoSharedVertices = true;
		base.Generate();
		return this;
	}
}
