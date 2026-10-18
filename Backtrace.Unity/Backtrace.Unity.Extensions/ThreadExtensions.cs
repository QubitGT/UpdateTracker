using System.Globalization;
using System.Threading;

namespace Backtrace.Unity.Extensions;

internal static class ThreadExtensions
{
	public static string GenerateValidThreadName(this Thread thread)
	{
		string name = thread.Name;
		return string.IsNullOrEmpty(name) ? thread.ManagedThreadId.ToString(CultureInfo.InvariantCulture) : name;
	}
}
