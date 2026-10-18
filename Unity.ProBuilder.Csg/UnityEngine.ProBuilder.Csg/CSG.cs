namespace UnityEngine.ProBuilder.Csg;

internal static class CSG
{
	public enum BooleanOp
	{
		Intersection,
		Union,
		Subtraction
	}

	private const float k_DefaultEpsilon = 1E-05f;

	private static float s_Epsilon = 1E-05f;

	public static float epsilon
	{
		get
		{
			return s_Epsilon;
		}
		set
		{
			s_Epsilon = value;
		}
	}

	public static Model Perform(BooleanOp op, GameObject lhs, GameObject rhs)
	{
		return op switch
		{
			BooleanOp.Intersection => Intersect(lhs, rhs), 
			BooleanOp.Union => Union(lhs, rhs), 
			BooleanOp.Subtraction => Subtract(lhs, rhs), 
			_ => null, 
		};
	}

	public static Model Union(GameObject lhs, GameObject rhs)
	{
		Model model = new Model(lhs);
		Model model2 = new Model(rhs);
		Node a = new Node(model.ToPolygons());
		Node b = new Node(model2.ToPolygons());
		return new Model(Node.Union(a, b).AllPolygons());
	}

	public static Model Subtract(GameObject lhs, GameObject rhs)
	{
		Model model = new Model(lhs);
		Model model2 = new Model(rhs);
		Node a = new Node(model.ToPolygons());
		Node b = new Node(model2.ToPolygons());
		return new Model(Node.Subtract(a, b).AllPolygons());
	}

	public static Model Intersect(GameObject lhs, GameObject rhs)
	{
		Model model = new Model(lhs);
		Model model2 = new Model(rhs);
		Node a = new Node(model.ToPolygons());
		Node b = new Node(model2.ToPolygons());
		return new Model(Node.Intersect(a, b).AllPolygons());
	}
}
