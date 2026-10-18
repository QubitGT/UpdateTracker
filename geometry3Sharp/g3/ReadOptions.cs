namespace g3;

public class ReadOptions
{
	public bool ReadMaterials;

	public CommandArgumentSet CustomFlags = new CommandArgumentSet();

	public static readonly ReadOptions Defaults = new ReadOptions
	{
		ReadMaterials = false
	};

	public ReadOptions()
	{
		ReadMaterials = false;
	}
}
