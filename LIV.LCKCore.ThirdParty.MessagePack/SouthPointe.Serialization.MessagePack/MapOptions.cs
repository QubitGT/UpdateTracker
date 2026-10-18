using System.Reflection;

namespace SouthPointe.Serialization.MessagePack;

public class MapOptions
{
	public bool RequireSerializableAttribute = true;

	public bool IgnoreAutoPropertyValues = true;

	public bool IgnoreNullOnPack = true;

	public bool IgnoreUnknownFieldOnUnpack = true;

	public bool AllowEmptyArrayOnUnpack = true;

	public IMapNamingStrategy NamingStrategy = new DefaultNamingStrategy();

	public BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField;
}
