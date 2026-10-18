using System.Text;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Liv.Lck.Core.Serialization;

[Preserve]
internal class LckJsonSerializer : ILckSerializer
{
	public SerializationType SerializationType => SerializationType.JsonUTF8;

	[Preserve]
	public LckJsonSerializer()
	{
	}

	public byte[] Serialize(object data)
	{
		string s = JsonConvert.SerializeObject(data);
		return Encoding.UTF8.GetBytes(s);
	}

	public T Deserialize<T>(byte[] data)
	{
		return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
	}
}
