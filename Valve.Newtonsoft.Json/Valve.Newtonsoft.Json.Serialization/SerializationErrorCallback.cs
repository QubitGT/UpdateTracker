using System.Runtime.Serialization;

namespace Valve.Newtonsoft.Json.Serialization;

public delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);
