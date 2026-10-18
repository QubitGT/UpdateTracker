namespace UnityEngine.ProBuilder;

internal sealed class Transform2D
{
	public Vector2 position;

	public float rotation;

	public Vector2 scale;

	public Transform2D(Vector2 position, float rotation, Vector2 scale)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
	}

	public Vector2 TransformPoint(Vector2 p)
	{
		p += position;
		p.RotateAroundPoint(p, rotation);
		p.ScaleAroundPoint(p, scale);
		return p;
	}

	public override string ToString()
	{
		string[] obj = new string[6] { "T: ", null, null, null, null, null };
		Vector2 vector = position;
		obj[1] = vector.ToString();
		obj[2] = "\nR: ";
		obj[3] = rotation.ToString();
		obj[4] = "°\nS: ";
		vector = scale;
		obj[5] = vector.ToString();
		return string.Concat(obj);
	}
}
