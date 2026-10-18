using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[JsonObject]
internal readonly struct Response204
{
}
