using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct GameTokenPackObject
{
	public readonly long Id;

	public readonly long TokenPackId;

	public readonly long Price;

	public readonly long Amount;

	public readonly string Portal;

	public readonly string Sku;

	public readonly string Name;

	public readonly string Description;

	public readonly long DateAdded;

	public readonly long DateUpdated;

	[JsonConstructor]
	public GameTokenPackObject(long id, long token_pack_id, long price, long amount, string portal, string sku, string name, string description, long date_added, long date_updated)
	{
		Id = id;
		TokenPackId = token_pack_id;
		Price = price;
		Amount = amount;
		Portal = portal;
		Sku = sku;
		Name = name;
		Description = description;
		DateAdded = date_added;
		DateUpdated = date_updated;
	}
}
