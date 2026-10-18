namespace g3;

public abstract class GenericMaterial
{
	public enum KnownMaterialTypes
	{
		OBJ_MTL_Format
	}

	public static readonly float Invalidf = float.MinValue;

	public static readonly Vector3f Invalid = new Vector3f(-1f, -1f, -1f);

	public string name;

	public int id;

	public abstract Vector3f DiffuseColor { get; set; }

	public abstract float Alpha { get; set; }

	public KnownMaterialTypes Type { get; set; }
}
