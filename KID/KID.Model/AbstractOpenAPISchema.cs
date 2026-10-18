using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KID.Model;

public abstract class AbstractOpenAPISchema
{
	public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
	{
		ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		MissingMemberHandling = MissingMemberHandling.Error,
		ContractResolver = new DefaultContractResolver
		{
			NamingStrategy = new CamelCaseNamingStrategy
			{
				OverrideSpecifiedNames = false
			}
		}
	};

	public static readonly JsonSerializerSettings AdditionalPropertiesSerializerSettings = new JsonSerializerSettings
	{
		ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		MissingMemberHandling = MissingMemberHandling.Ignore,
		ContractResolver = new DefaultContractResolver
		{
			NamingStrategy = new CamelCaseNamingStrategy
			{
				OverrideSpecifiedNames = false
			}
		}
	};

	public abstract object ActualInstance { get; set; }

	public bool IsNullable { get; protected set; }

	public string SchemaType { get; protected set; }

	public abstract string ToJson();
}
