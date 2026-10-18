using System.Text;

namespace VYaml.Internal;

internal static class StringEncoding
{
	public static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}
