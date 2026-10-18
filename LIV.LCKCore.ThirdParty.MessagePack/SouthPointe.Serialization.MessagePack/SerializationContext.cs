namespace SouthPointe.Serialization.MessagePack;

public class SerializationContext
{
	private static SerializationContext defaultContext;

	public readonly DateTimeOptions DateTimeOptions;

	public readonly EnumOptions EnumOptions;

	public readonly ArrayOptions ArrayOptions;

	public readonly MapOptions MapOptions;

	public readonly JsonOptions JsonOptions;

	public readonly TypeHandlers TypeHandlers;

	public static SerializationContext Default
	{
		get
		{
			SerializationContext result = defaultContext ?? new SerializationContext();
			defaultContext = result;
			return result;
		}
	}

	public SerializationContext()
	{
		DateTimeOptions = new DateTimeOptions();
		EnumOptions = new EnumOptions();
		ArrayOptions = new ArrayOptions();
		MapOptions = new MapOptions();
		JsonOptions = new JsonOptions();
		TypeHandlers = new TypeHandlers(this);
	}
}
