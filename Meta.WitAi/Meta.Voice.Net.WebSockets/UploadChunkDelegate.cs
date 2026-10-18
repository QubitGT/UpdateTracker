using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets;

public delegate void UploadChunkDelegate(string requestId, WitResponseNode jsonData, byte[] binaryData);
