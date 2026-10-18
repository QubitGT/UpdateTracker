namespace g3;

public enum ValidationStatus
{
	Ok,
	NotAVertex,
	NotBoundaryVertex,
	NotBoundaryEdge,
	NotATriangle,
	VerticesNotConnectedByEdge,
	IncorrectLoopOrientation,
	DuplicateTriangles,
	NearDegenerateMeshEdges,
	NearDenegerateInputGeometry
}
