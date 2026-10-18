namespace Oculus.Interaction;

public interface ICurvedPlane
{
	Cylinder Cylinder { get; }

	float ArcDegrees { get; }

	float Rotation { get; }

	float Bottom { get; }

	float Top { get; }
}
