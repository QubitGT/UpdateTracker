namespace SouthPointe.Serialization.MessagePack;

public class DecimalHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private ITypeHandler intArrayHandler;

	public DecimalHandler(SerializationContext context)
	{
		this.context = context;
	}

	public object Read(Format format, FormatReader reader)
	{
		intArrayHandler = intArrayHandler ?? context.TypeHandlers.Get<int[]>();
		return new decimal((int[])intArrayHandler.Read(format, reader));
	}

	public void Write(object obj, FormatWriter writer)
	{
		intArrayHandler = intArrayHandler ?? context.TypeHandlers.Get<int[]>();
		int[] bits = decimal.GetBits((decimal)obj);
		intArrayHandler.Write(bits, writer);
	}
}
