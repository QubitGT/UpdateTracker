using System.Runtime.Serialization;

namespace Valve.Newtonsoft.Json.Serialization;

public delegate void SerializationCallback(object o, StreamingContext context);
