using System;
using Newtonsoft.Json.Linq;

namespace Modio.Customizations;

[Serializable]
internal struct WssMessage
{
	public string operation;

	public JToken context;

	public bool TryGetValue<TOutput>(out TOutput output) where TOutput : struct
	{
		JToken jToken = context;
		if (jToken != null)
		{
			output = jToken.ToObject<TOutput>();
			return true;
		}
		output = default(TOutput);
		return false;
	}
}
