using System;

namespace Valve.Newtonsoft.Json.Serialization;

public interface IContractResolver
{
	JsonContract ResolveContract(Type type);
}
