using System.Threading;

namespace Photon.Voice;

internal static class Util
{
	public static void SetThreadName(Thread t, string name)
	{
		if (name.Length > 25)
		{
			name = name.Substring(0, 25);
		}
		t.Name = name;
	}
}
