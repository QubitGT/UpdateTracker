namespace g3;

public class OBJMaterial : GenericMaterial
{
	public Vector3f Ka;

	public Vector3f Kd;

	public Vector3f Ks;

	public Vector3f Ke;

	public Vector3f Tf;

	public int illum;

	public float d;

	public float Ns;

	public float sharpness;

	public float Ni;

	public string map_Ka;

	public string map_Kd;

	public string map_Ks;

	public string map_Ke;

	public string map_d;

	public string map_Ns;

	public string bump;

	public string disp;

	public string decal;

	public string refl;

	public override Vector3f DiffuseColor
	{
		get
		{
			if (!(Kd == GenericMaterial.Invalid))
			{
				return Kd;
			}
			return new Vector3f(1f, 1f, 1f);
		}
		set
		{
			Kd = value;
		}
	}

	public override float Alpha
	{
		get
		{
			if (d != GenericMaterial.Invalidf)
			{
				return d;
			}
			return 1f;
		}
		set
		{
			d = value;
		}
	}

	public OBJMaterial()
	{
		base.Type = KnownMaterialTypes.OBJ_MTL_Format;
		id = -1;
		name = "///INVALID_NAME";
		Ka = (Kd = (Ks = (Ke = (Tf = GenericMaterial.Invalid))));
		illum = -1;
		d = (Ns = (sharpness = (Ni = GenericMaterial.Invalidf)));
	}
}
