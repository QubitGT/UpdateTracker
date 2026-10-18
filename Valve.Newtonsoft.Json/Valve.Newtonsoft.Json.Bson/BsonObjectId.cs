using System;
using Valve.Newtonsoft.Json.Utilities;

namespace Valve.Newtonsoft.Json.Bson;

public class BsonObjectId
{
	public byte[] Value { get; private set; }

	public BsonObjectId(byte[] value)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		if (value.Length != 12)
		{
			throw new ArgumentException("An ObjectId must be 12 bytes", "value");
		}
		Value = value;
	}
}
