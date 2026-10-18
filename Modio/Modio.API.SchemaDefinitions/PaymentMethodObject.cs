using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct PaymentMethodObject
{
	internal readonly string Name;

	internal readonly string Id;

	internal readonly long Amount;

	internal readonly string DisplayAmount;

	[JsonConstructor]
	public PaymentMethodObject(string name, string id, long amount, string display_amount)
	{
		Name = name;
		Id = id;
		Amount = amount;
		DisplayAmount = display_amount;
	}
}
